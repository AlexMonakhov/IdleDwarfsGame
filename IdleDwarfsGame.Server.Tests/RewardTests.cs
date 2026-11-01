using System;
using System.Linq;
using System.Threading.Tasks;
using IdleDwarfsGame.Server.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class RewardTests : IDisposable
{
    private static readonly EntityFactory Factory = new();
    private static readonly DateTime Now = new(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly Guid _playerId = Guid.NewGuid();

    public RewardTests()
    {
        // In-memory SQLite живёт, пока открыто соединение. Таблицы дропа приходят из сидов (LootSeedData)
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Tester", Gold = 10, Experience = 5 });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void FixedPart_GrowsWithStage_AndBossPaysDouble()
    {
        var first = CampaignRewards.FixedFor(Stage(1));
        Assert.Equal(50, first.Gold);
        Assert.Equal(25, first.Experience);

        Assert.True(CampaignRewards.FixedFor(Stage(4)).Gold > CampaignRewards.FixedFor(Stage(3)).Gold);

        var mobs = CampaignRewards.FixedFor(Stage(5));
        var boss = CampaignRewards.FixedFor(Stage(5, StageType.Boss));
        Assert.Equal(mobs.Gold * 2, boss.Gold, tolerance: 1);
    }

    [Fact]
    public void Roll_AddsRandomBonus_AndBossAlwaysDropsChest()
    {
        var random = new Random(3);
        var mobsStage = Stage(3);
        var fixedPart = CampaignRewards.FixedFor(mobsStage);
        int mobsWithChest = 0;

        for (int i = 0; i < 1000; i++)
        {
            var reward = CampaignRewards.Roll(_playerId, mobsStage, random, Now);
            Assert.InRange(reward.Gold, fixedPart.Gold, fixedPart.Gold * (1 + CampaignRewards.RandomBonus) + 1);
            Assert.InRange(reward.Experience, fixedPart.Experience, fixedPart.Experience * (1 + CampaignRewards.RandomBonus) + 1);
            if (reward.Chests.Count > 0) mobsWithChest++;

            var bossReward = CampaignRewards.Roll(_playerId, Stage(10, StageType.Boss), random, Now);
            Assert.NotEmpty(bossReward.Chests);
            Assert.All(bossReward.Chests, c => Assert.Equal(2, c.ChestLevel));
        }

        // Сундук с обычного уровня — редкость (15%, 5% и 10% независимо ≈ 27%)
        Assert.InRange(mobsWithChest, 220, 330);
    }

    [Fact]
    public async Task ClearedStage_RewardIsPending_UntilClaimed_AndClaimedOnce()
    {
        var reward = BossReward(stageId: 5);
        Assert.True(await new CampaignRepository(_context).MarkClearedAsync(_playerId, 5, reward));

        // Пока награду не забрали, на аккаунте ничего не изменилось
        var rewards = new RewardRepository(_context);
        Assert.Equal(reward.Id, (await rewards.GetUnclaimedAsync(_playerId, RewardSource.Campaign))?.Id);
        Assert.Equal(10, (await Resources()).Gold);
        Assert.Empty(await new InventoryRepository(_context).GetClosedChestsAsync(_playerId));

        var service = new RewardService(rewards, new PlayerRepository(_context), new TestClock(Now));
        var result = await service.ClaimAsync(_playerId, reward.Id);

        Assert.Equal(reward.Gold, result.Reward.Gold);
        Assert.Equal(10 + reward.Gold, result.Resources.Gold);
        Assert.Equal(5 + reward.Experience, result.Resources.Experience);
        Assert.Null(await rewards.GetUnclaimedAsync(_playerId, RewardSource.Campaign));

        var chests = await new InventoryRepository(_context).GetClosedChestsAsync(_playerId);
        Assert.Equal(2, chests.Count);
        Assert.Contains(chests, c => c.ChestDropTable.ChestType == ChestType.Gem && c.ChestDropTable.ChestLevel == 1);
        Assert.Contains(chests, c => c.ChestDropTable.ChestType == ChestType.Hero && c.ChestDropTable.ChestLevel == 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClaimAsync(_playerId, reward.Id));
        Assert.Equal(10 + reward.Gold, (await Resources()).Gold);
    }

    [Fact]
    public async Task ClearedStageTwice_SecondRewardIsNotSaved()
    {
        var campaign = new CampaignRepository(_context);
        Assert.True(await campaign.MarkClearedAsync(_playerId, 1, BossReward(1)));
        Assert.False(await campaign.MarkClearedAsync(_playerId, 1, BossReward(1)));

        Assert.Equal(1, await _context.PlayerRewards.CountAsync(r => r.PlayerId == _playerId));
    }

    [Fact]
    public async Task ClaimForeignReward_IsRejected()
    {
        var reward = BossReward(5);
        await new CampaignRepository(_context).MarkClearedAsync(_playerId, 5, reward);

        var service = new RewardService(new RewardRepository(_context), new PlayerRepository(_context), new TestClock(Now));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClaimAsync(Guid.NewGuid(), reward.Id));
    }

    [Fact]
    public async Task OpenChests_GemsStackInInventory_HeroesJoinCollection_AndChestOpensOnce()
    {
        var reward = BossReward(5);
        await new CampaignRepository(_context).MarkClearedAsync(_playerId, 5, reward);
        await new RewardService(new RewardRepository(_context), new PlayerRepository(_context), new TestClock(Now))
            .ClaimAsync(_playerId, reward.Id);

        var inventory = new InventoryRepository(_context);
        var chests = new ChestService(inventory, new ItemDropService(new Random(5)), Factory, new Random(5));
        var gemChest = (await chests.GetInventoryAsync(_playerId)).Chests.Single(c => c.Type == ChestType.Gem);
        var heroChest = (await chests.GetInventoryAsync(_playerId)).Chests.Single(c => c.Type == ChestType.Hero);

        var gems = await chests.OpenAsync(_playerId, gemChest.Id);
        Assert.All(gems.Items, i => Assert.Equal(ItemType.Gem, i.ItemType));
        Assert.Empty(gems.Heroes);

        var heroes = await chests.OpenAsync(_playerId, heroChest.Id);
        var hero = Assert.Single(heroes.Heroes);
        Assert.InRange(hero.Level, 1, 2);
        Assert.Null(hero.PlaceInSquad);
        Assert.Equal(_playerId, (await _context.Heroes.AsNoTracking().SingleAsync(h => h.Id == hero.Id)).PlayerId);

        // Всё выпавшее лежит в инвентаре, а сундуки больше не открыть
        var after = await chests.GetInventoryAsync(_playerId);
        Assert.Empty(after.Chests);
        Assert.Equal(gems.Items.Sum(i => i.Quantity), after.Items.Sum(i => i.Quantity));
        await Assert.ThrowsAsync<InvalidOperationException>(() => chests.OpenAsync(_playerId, gemChest.Id));
    }

    [Fact]
    public async Task OpenChest_SameGemsAddToExistingStack()
    {
        var inventory = new InventoryRepository(_context);
        var table = LootSeedData.TableId(ChestType.Gem, 1);
        var chestIds = Enumerable.Range(0, 2).Select(_ => Guid.NewGuid()).ToList();
        foreach (var id in chestIds)
            _context.PlayerChests.Add(new() { Id = id, PlayerId = _playerId, ChestDropTableId = table });
        await _context.SaveChangesAsync();

        var ruby = new LootDrop(ItemType.Gem, nameof(GemType.Ruby), Rarity.Common, 1, 3);
        foreach (var id in chestIds)
        {
            var chest = await inventory.GetClosedChestAsync(_playerId, id);
            Assert.True(await inventory.OpenChestAsync(chest!, [ruby], []));
        }

        var item = Assert.Single(await inventory.GetItemsAsync(_playerId));
        Assert.Equal(6, item.Quantity);
    }

    [Fact]
    public void GeneratedHero_HasRankAndElement()
    {
        var random = new Random(11);

        var basic = HeroGenerator.Create(_playerId, rank: 1, element: "Fire", random);
        Assert.Equal(HeroElement.Basic, basic.Element);

        var frost = HeroGenerator.Create(_playerId, rank: 3, element: "Frost", random);
        Assert.Equal(HeroElement.Frost, frost.Element);
        Assert.Equal(3, frost.Level);

        var randomElement = HeroGenerator.Create(_playerId, rank: 2, element: "", random);
        Assert.NotEqual(HeroElement.Basic, randomElement.Element);
    }

    private async Task<PlayerResources> Resources() =>
        await new PlayerRepository(_context).GetResourcesAsync(_playerId) ?? new PlayerResources();

    // Награда босса с сундуком гемов и героев (уровень сундука по номеру уровня)
    private PlayerReward BossReward(int stageId)
    {
        var id = Guid.NewGuid();
        int level = CampaignRewards.ChestLevelFor(stageId);
        return new PlayerReward
        {
            Id = id,
            PlayerId = _playerId,
            Source = RewardSource.Campaign,
            SourceId = stageId,
            Gold = 300,
            Experience = 150,
            CreatedAt = Now,
            Chests =
            [
                new() { Id = Guid.NewGuid(), RewardId = id, ChestType = ChestType.Gem, ChestLevel = level },
                new() { Id = Guid.NewGuid(), RewardId = id, ChestType = ChestType.Hero, ChestLevel = level }
            ]
        };
    }

    private static CampaignStage Stage(int id, StageType type = StageType.Mobs) => new() { Id = id, Type = type };

    private sealed class TestClock(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(now);
    }
}
