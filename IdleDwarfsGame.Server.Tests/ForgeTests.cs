using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ForgeTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly ForgeService _service;
    private readonly Guid _playerId = Guid.NewGuid();

    public ForgeTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Smith" });
        _context.SaveChanges();

        _service = new ForgeService(new ForgeRepository(_context), new Random(5));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static EquipmentDefinition Item(string key) => EquipmentCatalog.Find(key)!;

    private List<Guid> Give(params string[] keys)
    {
        var items = keys.Select(k => new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = _playerId, ItemKey = k }).ToList();
        _context.PlayerEquipment.AddRange(items);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        return items.Select(i => i.Id).ToList();
    }

    private void GiveGems(GemType gem, Rarity rarity, int quantity)
    {
        _context.PlayerItems.Add(new PlayerItem
        {
            Id = Guid.NewGuid(), PlayerId = _playerId, ItemType = ItemType.Gem, SubType = gem.ToString(), Rarity = rarity, Level = 1, Quantity = quantity
        });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    private int GemsOf(GemType gem, Rarity rarity) => _context.PlayerItems.AsNoTracking()
        .Where(i => i.PlayerId == _playerId && i.SubType == gem.ToString() && i.Rarity == rarity)
        .Sum(i => i.Quantity);

    /* ===== Вид нового предмета ===== */

    [Fact]
    public void RollType_FiveWeapons_AlwaysWeapon()
    {
        var weapons = Enumerable.Repeat(Item("RustyAxe"), 5).ToList();
        var random = new Random(1);
        Assert.All(Enumerable.Range(0, 200), _ => Assert.Equal(EquipmentType.Weapon, ForgeService.RollType(weapons, random)));
    }

    [Fact]
    public void RollType_OneWeaponFourRings_RingAboutEightyPercent()
    {
        var mixed = new[] { Item("RustyAxe"), Item("CopperRing"), Item("CopperRing"), Item("CopperRing"), Item("CopperRing") };
        var random = new Random(2);
        double rings = Enumerable.Range(0, 5000).Count(_ => ForgeService.RollType(mixed, random) == EquipmentType.Ring) / 5000.0;
        Assert.InRange(rings, 0.77, 0.83);
    }

    /* ===== Переплавка предметов ===== */

    [Fact]
    public async Task FuseItems_ConsumesFive_AndForgesOneOfTheNextRarity()
    {
        var ids = Give("RustyAxe", "RustyAxe", "RustyAxe", "RustyAxe", "RustyAxe");

        var result = await _service.FuseItemsAsync(_playerId, ids);

        Assert.Equal(Rarity.Uncommon, result.Item.Rarity);
        Assert.Equal(EquipmentType.Weapon, result.Item.Type);
        Assert.Single(result.Forge.Items);
        Assert.Equal(1, await _context.PlayerEquipment.CountAsync(e => e.PlayerId == _playerId));
    }

    [Fact]
    public async Task FuseItems_MythicGoesToLegendary_ButLegendaryCannotBecomeArtifact()
    {
        var mythic = Give("RingOfCleaving", "RingOfCleaving", "RingOfCleaving", "RingOfCleaving", "RingOfCleaving");
        Assert.Equal(Rarity.Legendary, (await _service.FuseItemsAsync(_playerId, mythic)).Item.Rarity);

        var legendary = Give("RingOfWildfire", "RingOfWildfire", "RingOfWildfire", "RingOfWildfire", "RingOfWildfire");
        var error = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseItemsAsync(_playerId, legendary));
        Assert.Equal(ForgeError.MaxRarity, error.Code);
        Assert.Equal(6, await _context.PlayerEquipment.CountAsync(e => e.PlayerId == _playerId));
    }

    [Fact]
    public async Task FuseItems_DifferentRarities_Rejected()
    {
        var ids = Give("RustyAxe", "RustyAxe", "RustyAxe", "RustyAxe", "DwarvenHammer");
        var error = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseItemsAsync(_playerId, ids));
        Assert.Equal(ForgeError.DifferentRarities, error.Code);
    }

    [Fact]
    public async Task FuseItems_WornItem_Rejected_AndNothingIsLost()
    {
        var ids = Give("IronHelm", "IronHelm", "IronHelm", "IronHelm", "IronHelm");
        var hero = StarterSquad.Create(_playerId)[0];
        _context.Heroes.Add(hero);
        _context.SaveChanges();
        await _context.PlayerEquipment.Where(e => e.Id == ids[0])
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.HeroId, hero.Id).SetProperty(e => e.Slot, EquipmentSlot.Helmet));

        var error = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseItemsAsync(_playerId, ids));

        Assert.Equal(ForgeError.ItemNotFound, error.Code);
        Assert.Equal(5, await _context.PlayerEquipment.CountAsync(e => e.PlayerId == _playerId));
    }

    [Fact]
    public async Task FuseItems_NeedsFiveDifferentItems()
    {
        var ids = Give("RustyAxe", "RustyAxe", "RustyAxe", "RustyAxe");
        var four = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseItemsAsync(_playerId, ids));
        Assert.Equal(ForgeError.WrongItemCount, four.Code);

        var repeated = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseItemsAsync(_playerId, [.. ids, ids[0]]));
        Assert.Equal(ForgeError.WrongItemCount, repeated.Code);
    }

    /* ===== Сплав камней ===== */

    [Fact]
    public async Task FuseGems_FiveIntoOneOfTheNextStep_SeveralTimesAtOnce()
    {
        GiveGems(GemType.Ruby, Rarity.Common, 12);

        var result = await _service.FuseGemsAsync(_playerId, GemType.Ruby, Rarity.Common, 2);

        Assert.Equal((Rarity.Uncommon, 2), (result.Rarity, result.Created));
        Assert.Equal(2, GemsOf(GemType.Ruby, Rarity.Common));
        Assert.Equal(2, GemsOf(GemType.Ruby, Rarity.Uncommon));

        // Новые камни той же ступени прибавляются к уже заведённой стопке, а не заводят вторую
        await _context.PlayerItems.Where(i => i.SubType == "Ruby" && i.Rarity == Rarity.Common)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Quantity, i => i.Quantity + 3));
        await _service.FuseGemsAsync(_playerId, GemType.Ruby, Rarity.Common, 1);
        Assert.Equal(3, GemsOf(GemType.Ruby, Rarity.Uncommon));
        Assert.Equal(1, await _context.PlayerItems.CountAsync(i => i.SubType == "Ruby" && i.Rarity == Rarity.Uncommon));
    }

    [Fact]
    public async Task FuseGems_NotEnough_Rejected_AndNothingIsTaken()
    {
        GiveGems(GemType.Topaz, Rarity.Rare, 9);

        var error = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseGemsAsync(_playerId, GemType.Topaz, Rarity.Rare, 2));

        Assert.Equal(ForgeError.NotEnoughGems, error.Code);
        Assert.Equal(9, GemsOf(GemType.Topaz, Rarity.Rare));
    }

    [Fact]
    public async Task FuseGems_UnknownGemOrTier_Rejected()
    {
        GiveGems(GemType.Ruby, Rarity.Common, 10);

        var gem = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseGemsAsync(_playerId, (GemType)99, Rarity.Common, 1));
        var tier = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseGemsAsync(_playerId, GemType.Ruby, (Rarity)(-1), 1));

        Assert.Equal(ForgeError.UnknownGem, gem.Code);
        Assert.Equal(ForgeError.UnknownGem, tier.Code);
        Assert.Equal(10, GemsOf(GemType.Ruby, Rarity.Common));
    }

    [Fact]
    public async Task FuseItems_NullList_Rejected()
    {
        var error = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseItemsAsync(_playerId, null));
        Assert.Equal(ForgeError.WrongItemCount, error.Code);
    }

    [Fact]
    public async Task FuseGems_LegendaryCannotGoHigher()
    {
        GiveGems(GemType.Diamond, Rarity.Legendary, 10);
        var error = await Assert.ThrowsAsync<ForgeException>(() => _service.FuseGemsAsync(_playerId, GemType.Diamond, Rarity.Legendary, 1));
        Assert.Equal(ForgeError.MaxRarity, error.Code);
    }
}
