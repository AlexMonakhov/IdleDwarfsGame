using System;
using System.Linq;
using System.Threading.Tasks;
using IdleDwarfsGame.Server.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class FixedShopTests : IDisposable
{
    private static readonly EntityFactory Factory = new();

    // Пятница недели общих шахт (ротация начинается 2 ноября)
    private static readonly DateTime Friday = new(2026, 11, 6, 10, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(Friday);
    private readonly FixedShopService _service;
    private readonly Guid _playerId = Guid.NewGuid();

    public FixedShopTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Collector" });
        _context.SaveChanges();

        _service = new FixedShopService(new FixedShopRepository(_context), new PlayerRepository(_context), Factory, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task SetSealsAsync(long seals) =>
        await _context.Players.Where(p => p.Id == _playerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.ElderSeals, seals));

    private async Task GiveEventCurrencyAsync(long amount)
    {
        await EventWallets.AddAsync(_context, _playerId, WorldEvents.Of(_clock.UtcNow).StartsAt, amount);
        _context.ChangeTracker.Clear();
    }

    private async Task<Player> PlayerAsync() => await _context.Players.AsNoTracking().FirstAsync(p => p.Id == _playerId);

    /* ===== Расписание ===== */

    [Fact]
    public void Events_AlternateWeekly_AndLeaveTheLastDayForTheShop()
    {
        var mines = WorldEvents.Of(Friday);
        Assert.Equal(WorldEventKind.CommunityMines, mines.Kind);
        Assert.Equal(new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc), mines.StartsAt);
        Assert.Equal(new DateTime(2026, 11, 8, 0, 0, 0, DateTimeKind.Utc), mines.EndsAt);
        Assert.Equal(new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc), mines.ShopClosesAt);

        Assert.NotNull(WorldEvents.ActiveAt(Friday));
        // Воскресенье — только лавка
        var sunday = new DateTime(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);
        Assert.Null(WorldEvents.ActiveAt(sunday));
        Assert.True(WorldEvents.Of(sunday).IsShopOpen(sunday));

        Assert.Equal(WorldEventKind.WorldBosses, WorldEvents.Of(Friday.AddDays(7)).Kind);
        Assert.Equal(WorldEventKind.CommunityMines, WorldEvents.Of(Friday.AddDays(14)).Kind);
        // До начала ротации чередование то же
        Assert.Equal(WorldEventKind.WorldBosses, WorldEvents.Of(Friday.AddDays(-7)).Kind);
    }

    /* ===== Лавка старейшин ===== */

    [Fact]
    public async Task ElderShop_ShowsFixedOffersWithLimits()
    {
        var shop = await _service.GetAsync(_playerId, FixedShopKind.Elder);

        Assert.Equal(FixedShops.Elder.Select(o => o.Key), shop.Offers.Select(o => o.Key));
        Assert.All(shop.Offers, o => Assert.Equal(0, o.Bought));
        Assert.Null(shop.Event);
        Assert.Equal(new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc), shop.ResetAt);
    }

    [Fact]
    public async Task ElderShop_Buy_ChargesSealsAndGivesTheChest()
    {
        await SetSealsAsync(100);
        var offer = FixedShops.Elder.First(o => o.Key == "gem_chest");

        var result = await _service.BuyAsync(_playerId, FixedShopKind.Elder, offer.Key);

        Assert.Equal(100 - offer.Price, (await PlayerAsync()).ElderSeals);
        Assert.Equal(100 - offer.Price, result.Shop.Currency);
        Assert.Equal(1, result.Shop.Offers.First(o => o.Key == offer.Key).Bought);
        var chest = await _context.PlayerChests.Include(c => c.ChestDropTable).SingleAsync(c => c.PlayerId == _playerId);
        Assert.Equal(ChestType.Gem, chest.ChestDropTable.ChestType);
    }

    [Fact]
    public async Task ElderShop_Limit_StopsPurchases_UntilNextWeek()
    {
        await SetSealsAsync(1_000);
        var offer = FixedShops.Elder.First(o => o.Key == "rainbow_coins");

        await _service.BuyAsync(_playerId, FixedShopKind.Elder, offer.Key);
        var error = await Assert.ThrowsAsync<FixedShopException>(() => _service.BuyAsync(_playerId, FixedShopKind.Elder, offer.Key));
        Assert.Equal(FixedShopError.LimitReached, error.Code);
        Assert.Equal(offer.Amount, (await PlayerAsync()).RainbowCoins);
        Assert.Equal(1_000 - offer.Price, (await PlayerAsync()).ElderSeals);

        _clock.Advance(TimeSpan.FromDays(7));
        await _service.BuyAsync(_playerId, FixedShopKind.Elder, offer.Key);
        Assert.Equal(offer.Amount * 2, (await PlayerAsync()).RainbowCoins);
    }

    [Fact]
    public async Task ElderShop_NotEnoughSeals_ChargesNothingAndKeepsTheLimit()
    {
        await SetSealsAsync(10);

        var error = await Assert.ThrowsAsync<FixedShopException>(() => _service.BuyAsync(_playerId, FixedShopKind.Elder, "hero_chest"));
        Assert.Equal(FixedShopError.NotEnoughCurrency, error.Code);
        Assert.Equal(10, (await PlayerAsync()).ElderSeals);
        Assert.Empty(await _context.FixedShopPurchases.ToListAsync());
        Assert.Empty(await _context.PlayerChests.ToListAsync());
    }

    [Fact]
    public async Task UnknownOffer_IsRefused()
    {
        var error = await Assert.ThrowsAsync<FixedShopException>(() => _service.BuyAsync(_playerId, FixedShopKind.Elder, "event_hero"));
        Assert.Equal(FixedShopError.OfferNotFound, error.Code);

        error = await Assert.ThrowsAsync<FixedShopException>(() => _service.GetAsync(_playerId, (FixedShopKind)9));
        Assert.Equal(FixedShopError.UnknownShop, error.Code);
    }

    /* ===== Лавка ивента ===== */

    [Fact]
    public async Task EventShop_ShowsTheEventAndItsCurrency()
    {
        await GiveEventCurrencyAsync(500);
        var shop = await _service.GetAsync(_playerId, FixedShopKind.Event);

        Assert.NotNull(shop.Event);
        Assert.Equal(WorldEventKind.CommunityMines, shop.Event!.Kind);
        Assert.True(shop.Event.Active);
        Assert.Equal(WorldEventKind.WorldBosses, shop.Event.NextKind);
        Assert.Equal(500, shop.Currency);
        Assert.Equal(500, shop.Event.Earned);
        Assert.Equal(FixedShops.CommunityMines.Select(o => o.Key), shop.Offers.Select(o => o.Key));
    }

    [Fact]
    public async Task EventHero_IsTheSameForEveryone_AndMatchesThePreview()
    {
        await GiveEventCurrencyAsync(2_500);
        var preview = (await _service.GetAsync(_playerId, FixedShopKind.Event)).Offers.Single(o => o.Key == "event_hero").Hero!;

        var result = await _service.BuyAsync(_playerId, FixedShopKind.Event, "event_hero");
        var hero = await _context.Heroes.AsNoTracking().SingleAsync(h => h.PlayerId == _playerId);

        Assert.Equal(FixedShops.MinesHero.Name, hero.Name);
        Assert.Equal(4, hero.Level);
        Assert.Equal(HeroElement.Lightning, hero.Element);
        Assert.Equal(["Stun"], hero.Immunities);
        Assert.Equal(preview.Name, result.Hero!.Name);
        Assert.Equal(preview.Profession, result.Hero.Profession);
        Assert.Null(hero.PlaceInSquad);

        var other = FixedShops.CreateHero(FixedShops.MinesHero, Guid.NewGuid());
        Assert.Equal(hero.Profession, other.Profession);
        Assert.Equal(hero.SpellIds, other.SpellIds);
        Assert.Equal(hero.Passives, other.Passives);

        Assert.Equal(500, (await _context.PlayerEventWallets.AsNoTracking().SingleAsync()).Currency);
    }

    [Fact]
    public async Task EventShop_Item_GoesToTheInventory()
    {
        await GiveEventCurrencyAsync(3_000);

        var result = await _service.BuyAsync(_playerId, FixedShopKind.Event, "artifact");

        Assert.Equal("ArmageddonBlade", (await _context.PlayerEquipment.SingleAsync(e => e.PlayerId == _playerId)).ItemKey);
        Assert.Equal(0, result.Shop.Currency);
    }

    [Fact]
    public async Task EventCurrency_OfLastWeek_IsGone()
    {
        await GiveEventCurrencyAsync(5_000);
        _clock.Advance(TimeSpan.FromDays(7));

        var shop = await _service.GetAsync(_playerId, FixedShopKind.Event);
        Assert.Equal(WorldEventKind.WorldBosses, shop.Event!.Kind);
        Assert.Equal(0, shop.Currency);

        var error = await Assert.ThrowsAsync<FixedShopException>(() => _service.BuyAsync(_playerId, FixedShopKind.Event, "gem_chest"));
        Assert.Equal(FixedShopError.NotEnoughCurrency, error.Code);
    }

    [Fact]
    public async Task EventShop_StillSellsOnTheShopDay()
    {
        await GiveEventCurrencyAsync(200);
        _clock.Advance(TimeSpan.FromDays(2)); // воскресенье

        var shop = await _service.GetAsync(_playerId, FixedShopKind.Event);
        Assert.False(shop.Event!.Active);

        await _service.BuyAsync(_playerId, FixedShopKind.Event, "gem_chest");
        Assert.Equal(100, (await _context.PlayerEventWallets.AsNoTracking().SingleAsync()).Currency);
    }

    [Fact]
    public async Task GoldPouch_ScalesWithCampaignProgress()
    {
        await SetSealsAsync(100);
        long early = (await _service.GetAsync(_playerId, FixedShopKind.Elder)).Offers.Single(o => o.Key == "gold_pouch").Gold;

        for (int stage = 1; stage <= 100; stage++)
            _context.PlayerCampaignProgress.Add(new PlayerCampaignProgress { PlayerId = _playerId, StageId = stage, ClearedAt = Friday });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        long later = (await _service.GetAsync(_playerId, FixedShopKind.Elder)).Offers.Single(o => o.Key == "gold_pouch").Gold;
        Assert.True(early > 0);
        Assert.True(later > early * 5);

        await _service.BuyAsync(_playerId, FixedShopKind.Elder, "gold_pouch");
        Assert.Equal(later, (await PlayerAsync()).Gold);
    }

    [Fact]
    public void Catalogs_ReferToRealItems_AndHaveUniqueKeys()
    {
        foreach (var catalog in new[] { FixedShops.Elder, FixedShops.CommunityMines, FixedShops.WorldBosses })
        {
            Assert.Equal(catalog.Length, catalog.Select(o => o.Key).Distinct().Count());
            Assert.All(catalog.Where(o => o.Kind == FixedRewardKind.Item), o => Assert.NotNull(EquipmentCatalog.Find(o.ItemKey!)));
            Assert.All(catalog, o => Assert.True(o.Price > 0 && o.Limit > 0));
        }
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
