using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ShopTests : IDisposable
{
    private static readonly EntityFactory Factory = new();

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(new DateTime(2026, 11, 6, 10, 0, 0, DateTimeKind.Utc));
    private readonly ShopService _service;
    private readonly Guid _playerId = Guid.NewGuid();

    public ShopTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Shopper", RainbowCoins = ShopRules.StarterRainbowCoins });
        _context.SaveChanges();

        _service = new ShopService(new ShopRepository(_context), new PlayerRepository(_context), Factory, new Random(7), _clock, NoActivity.Instance);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task SetWalletAsync(long gold = 0, long rainbow = 0, long blood = 0, long badges = 0)
    {
        await _context.Players
            .Where(p => p.Id == _playerId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Gold, gold)
                .SetProperty(p => p.RainbowCoins, rainbow)
                .SetProperty(p => p.BloodCoins, blood)
                .SetProperty(p => p.HonorBadges, badges));
    }

    private async Task AddGemsAsync(GemType gem, Rarity rarity, int quantity)
    {
        _context.PlayerItems.Add(new PlayerItem
        {
            Id = Guid.NewGuid(), PlayerId = _playerId, ItemType = ItemType.Gem, SubType = gem.ToString(), Rarity = rarity, Level = 1, Quantity = quantity
        });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    /* ===== Правила ===== */

    [Fact]
    public void Chances_LegendaryAndTopRankAreOnePercent_ArtifactIsATenthOfAPercent()
    {
        Assert.Equal(0.01, ShopRules.ItemChance(Rarity.Legendary), 6);
        Assert.Equal(0.001, ShopRules.ItemChance(Rarity.Artifact), 6);
        Assert.Equal(0.01, ShopRules.HeroChance(4), 6);
        Assert.Equal(1.0, Enum.GetValues<Rarity>().Sum(ShopRules.ItemChance), 6);
    }

    [Fact]
    public void HeroPrice_GrowsWithRank_AndTakesGemsOfItsElementByRankStep()
    {
        var random = new Random(1);
        var rank1 = HeroGenerator.Create(_playerId, 1, "", random);
        var rank2 = HeroGenerator.Create(_playerId, 2, "Fire", random);
        var rank3 = HeroGenerator.Create(_playerId, 3, "Frost", random);
        var rank4 = HeroGenerator.Create(_playerId, 4, "Nature", random);

        var p1 = ShopRules.PriceOf(ShopKind.Market, rank1);
        var p2 = ShopRules.PriceOf(ShopKind.Market, rank2);
        var p3 = ShopRules.PriceOf(ShopKind.Market, rank3);
        var p4 = ShopRules.PriceOf(ShopKind.Market, rank4);

        // У 1-го ранга стихии нет — только золото
        Assert.Null(p1.Gem);
        Assert.Equal((GemType.Ruby, Rarity.Common), (p2.Gem!.Value, p2.GemRarity));
        Assert.Equal((GemType.Sapphire, Rarity.Uncommon), (p3.Gem!.Value, p3.GemRarity));
        Assert.Equal((GemType.Emerald, Rarity.Rare), (p4.Gem!.Value, p4.GemRarity));
        Assert.True(p1.Gold < p2.Gold && p2.Gold < p3.Gold && p3.Gold < p4.Gold);
        Assert.Equal(0, p4.Currency);

        // Подпольный рынок — только своя валюта
        var pvp = ShopRules.PriceOf(ShopKind.Pvp, rank4);
        Assert.Equal(0, pvp.Gold);
        Assert.Null(pvp.Gem);
        Assert.True(pvp.Currency > ShopRules.PriceOf(ShopKind.Pvp, rank3).Currency);
    }

    [Fact]
    public void HeroPrice_GrowsWithPassiveLevelAndImmunity()
    {
        var hero = HeroGenerator.Create(_playerId, 4, "Dark", new Random(3));
        hero.Passives = [HeroPassive.CriticalStrike];
        hero.Immunities = [];
        hero.PassiveLevel = 1;
        long plain = ShopRules.PriceOf(ShopKind.Market, hero).Gold;

        hero.PassiveLevel = 3;
        long strongPassive = ShopRules.PriceOf(ShopKind.Market, hero).Gold;
        hero.Immunities = ["Stun"];
        long withImmunity = ShopRules.PriceOf(ShopKind.Market, hero).Gold;

        Assert.True(strongPassive > plain);
        Assert.True(withImmunity > strongPassive);
    }

    [Fact]
    public void SpecialShops_GiveTopPassiveLevelForTheRank()
    {
        var random = new Random(11);
        for (int i = 0; i < 300; i++)
        {
            var hero = ShopRules.RollHero(ShopKind.Pvp, _playerId, random);
            if (hero.Passives.Count > 0)
                Assert.Equal(PassiveBook.MaxLevelFor(hero.Level), hero.PassiveLevel);
        }
    }

    [Fact]
    public void Nice_KeepsTwoSignificantDigits()
    {
        Assert.Equal(3_500_000, ShopRules.Nice(3_456_000));
        Assert.Equal(120, ShopRules.Nice(117));
        Assert.Equal(46, ShopRules.Nice(46));
    }

    /* ===== Полка ===== */

    [Fact]
    public async Task FirstVisit_FillsEightItemsAndEightHeroes_AndKeepsThemWithinTheDay()
    {
        var first = await _service.GetAsync(_playerId, ShopKind.Market);
        Assert.Equal(ShopRules.SlotCount, first.Items.Count);
        Assert.Equal(ShopRules.SlotCount, first.Heroes.Count);
        Assert.All(first.Items, o => Assert.NotNull(o.Item));
        Assert.All(first.Heroes, o => Assert.NotNull(o.Hero));
        Assert.True(first.FreeReroll);

        _clock.Advance(TimeSpan.FromHours(5));
        var again = await _service.GetAsync(_playerId, ShopKind.Market);
        Assert.Equal(first.Items.Select(o => o.Id), again.Items.Select(o => o.Id));
    }

    [Fact]
    public async Task NewDay_RefreshesEverythingExceptLockedOffers()
    {
        var shop = await _service.GetAsync(_playerId, ShopKind.Market);
        var keptItem = shop.Items[3];
        var keptHero = shop.Heroes[5];
        await _service.SetLockedAsync(_playerId, keptItem.Id, true);
        await _service.SetLockedAsync(_playerId, keptHero.Id, true);

        _clock.Advance(TimeSpan.FromDays(1));
        var next = await _service.GetAsync(_playerId, ShopKind.Market);

        Assert.Contains(next.Items, o => o.Id == keptItem.Id && o.Locked && o.Slot == keptItem.Slot);
        Assert.Contains(next.Heroes, o => o.Id == keptHero.Id && o.Locked);
        Assert.Equal(1, next.Items.Count(o => shop.Items.Any(old => old.Id == o.Id)));
        Assert.Equal(ShopRules.SlotCount, next.Items.Count);
    }

    [Fact]
    public async Task Lock_OnlyOneOfEachKind()
    {
        var shop = await _service.GetAsync(_playerId, ShopKind.Market);
        await _service.SetLockedAsync(_playerId, shop.Items[0].Id, true);
        var after = await _service.SetLockedAsync(_playerId, shop.Items[1].Id, true);

        Assert.Equal([shop.Items[1].Id], after.Items.Where(o => o.Locked).Select(o => o.Id));
    }

    [Fact]
    public async Task Reroll_FirstIsFree_ThenCostsRainbowCoins_AndKeepsLocked()
    {
        var shop = await _service.GetAsync(_playerId, ShopKind.Market);
        await _service.SetLockedAsync(_playerId, shop.Items[2].Id, true);
        await SetWalletAsync(rainbow: ShopRules.RerollCost);

        var free = await _service.RerollAsync(_playerId, ShopKind.Market);
        Assert.False(free.FreeReroll);
        Assert.Equal(ShopRules.RerollCost, free.Wallet.RainbowCoins);
        Assert.Contains(free.Items, o => o.Id == shop.Items[2].Id);
        Assert.DoesNotContain(free.Heroes, o => shop.Heroes.Any(old => old.Id == o.Id));

        var paid = await _service.RerollAsync(_playerId, ShopKind.Market);
        Assert.Equal(0, paid.Wallet.RainbowCoins);

        var error = await Assert.ThrowsAsync<ShopException>(() => _service.RerollAsync(_playerId, ShopKind.Market));
        Assert.Equal(ShopError.NotEnoughRainbowCoins, error.Code);

        // Наутро бесплатное обновление снова доступно
        _clock.Advance(TimeSpan.FromDays(1));
        Assert.True((await _service.GetAsync(_playerId, ShopKind.Market)).FreeReroll);
    }

    /* ===== Покупка ===== */

    [Fact]
    public async Task BuyItem_ChargesGold_GivesTheItem_AndCannotBeBoughtTwice()
    {
        var shop = await _service.GetAsync(_playerId, ShopKind.Market);
        var offer = shop.Items[0];
        await SetWalletAsync(gold: offer.Price.Gold + 5);

        var result = await _service.BuyAsync(_playerId, offer.Id);

        Assert.Equal(offer.Item!.Key, result.Item!.Key);
        Assert.Equal(5, result.Shop.Wallet.Gold);
        Assert.True(result.Shop.Items.Single(o => o.Id == offer.Id).Sold);
        Assert.Equal(1, await _context.PlayerEquipment.CountAsync(e => e.PlayerId == _playerId && e.ItemKey == offer.Item.Key));

        var again = await Assert.ThrowsAsync<ShopException>(() => _service.BuyAsync(_playerId, offer.Id));
        Assert.Equal(ShopError.AlreadySold, again.Code);
    }

    [Fact]
    public async Task Buy_WithoutEnoughGold_Fails_AndChargesNothing()
    {
        var shop = await _service.GetAsync(_playerId, ShopKind.Market);
        var offer = shop.Items[0];
        await SetWalletAsync(gold: offer.Price.Gold - 1);

        var error = await Assert.ThrowsAsync<ShopException>(() => _service.BuyAsync(_playerId, offer.Id));

        Assert.Equal(ShopError.NotEnoughGold, error.Code);
        Assert.Equal(offer.Price.Gold - 1, (await _context.Players.AsNoTracking().SingleAsync(p => p.Id == _playerId)).Gold);
        Assert.Equal(0, await _context.PlayerEquipment.CountAsync(e => e.PlayerId == _playerId));
    }

    [Fact]
    public async Task BuyHero_ChargesGoldAndGems_AndAddsTheDwarfOutsideTheSquad()
    {
        // Ищем дворфа со стихией: за него берут и камни
        ShopOfferInfo? offer = null;
        for (int day = 0; day < 30 && offer == null; day++)
        {
            var shop = await _service.GetAsync(_playerId, ShopKind.Market);
            offer = shop.Heroes.FirstOrDefault(o => o.Price.Gem != null);
            _clock.Advance(TimeSpan.FromDays(1));
        }
        Assert.NotNull(offer);
        _clock.Advance(TimeSpan.FromDays(-1));

        await SetWalletAsync(gold: offer!.Price.Gold);
        await AddGemsAsync(offer.Price.Gem!.Value, offer.Price.GemRarity, offer.Price.Gems - 1);
        var short1 = await Assert.ThrowsAsync<ShopException>(() => _service.BuyAsync(_playerId, offer.Id));
        Assert.Equal(ShopError.NotEnoughGems, short1.Code);

        // Недостающий камень — в ту же стопку
        await _context.PlayerItems
            .Where(i => i.PlayerId == _playerId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Quantity, i => i.Quantity + 1));

        var result = await _service.BuyAsync(_playerId, offer.Id);

        Assert.Equal(offer.Hero!.Level, result.Hero!.Level);
        Assert.Null(result.Hero.PlaceInSquad);
        Assert.Equal(0, result.Shop.Wallet.Gold);
        Assert.DoesNotContain(result.Shop.Wallet.Gems, g => g.Gem == offer.Price.Gem && g.Rarity == offer.Price.GemRarity);
        Assert.True(await _context.Heroes.AnyAsync(h => h.Id == result.Hero.Id && h.PlayerId == _playerId));
    }

    [Fact]
    public async Task PvpShop_ChargesBloodCoinsOnly()
    {
        var shop = await _service.GetAsync(_playerId, ShopKind.Pvp);
        var offer = shop.Items[0];
        Assert.Equal(0, offer.Price.Gold);
        await SetWalletAsync(blood: offer.Price.Currency);

        var result = await _service.BuyAsync(_playerId, offer.Id);

        Assert.Equal(0, result.Shop.Wallet.BloodCoins);
        Assert.True(result.Shop.Items.Single(o => o.Id == offer.Id).Sold);
    }

    [Fact]
    public async Task UnknownShop_Rejected_SoNothingIsFree()
    {
        var error = await Assert.ThrowsAsync<ShopException>(() => _service.GetAsync(_playerId, (ShopKind)5));
        Assert.Equal(ShopError.UnknownShop, error.Code);
        Assert.Equal(ShopError.UnknownShop, (await Assert.ThrowsAsync<ShopException>(() => _service.RerollAsync(_playerId, (ShopKind)5))).Code);
        Assert.Equal(0, await _context.ShopOffers.CountAsync());
    }

    [Fact]
    public async Task Shops_HaveSeparateShelves()
    {
        var market = await _service.GetAsync(_playerId, ShopKind.Market);
        var guild = await _service.GetAsync(_playerId, ShopKind.Guild);

        Assert.Empty(market.Items.Select(o => o.Id).Intersect(guild.Items.Select(o => o.Id)));
        Assert.All(guild.Items, o => Assert.True(o.Price.Currency > 0 && o.Price.Gold == 0));
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
