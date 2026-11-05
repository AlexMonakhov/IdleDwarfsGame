using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdleDwarfsGame.Server.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Xunit.Abstractions;

public class TowerTests : IDisposable
{
    private static readonly EntityFactory Factory = new();
    // Среда недели, с которой круг стихий начинается с огня
    private static readonly DateTime Now = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Week = InfinityTower.WeekOf(Now);

    private readonly ITestOutputHelper _output;
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly Guid _playerId = Guid.NewGuid();

    public TowerTests(ITestOutputHelper output)
    {
        _output = output;
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
    public void Week_StartsOnMondayUtc()
    {
        Assert.Equal(new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc), Week);
        Assert.Equal(new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc), InfinityTower.EndOf(Week));
        Assert.Equal(Week, InfinityTower.WeekOf(new DateTime(2026, 11, 8, 23, 59, 59, DateTimeKind.Utc)));
    }

    [Fact]
    public void Elements_CycleByBlocks_AndShiftEveryWeek()
    {
        // Эта неделя: 1–5 огонь, 6–10 лёд, 11–15 природа...
        Assert.All(Enumerable.Range(1, 5), floor => Assert.Equal(HeroElement.Fire, InfinityTower.ElementOf(Week, floor)));
        Assert.Equal(HeroElement.Frost, InfinityTower.ElementOf(Week, 6));
        Assert.Equal(HeroElement.Nature, InfinityTower.ElementOf(Week, 11));
        // После круга из семи стихий — снова огонь
        Assert.Equal(HeroElement.Fire, InfinityTower.ElementOf(Week, 36));

        // Следующая неделя начинается со льда, прошлая — с крови
        Assert.Equal(HeroElement.Frost, InfinityTower.ElementOf(Week.AddDays(7), 1));
        Assert.Equal(HeroElement.Fire, InfinityTower.ElementOf(Week.AddDays(7), 31));
        Assert.Equal(HeroElement.Blood, InfinityTower.ElementOf(Week.AddDays(-7), 1));

        // За семь недель каждая стихия хоть раз открывает башню
        var starts = Enumerable.Range(0, 7).Select(i => InfinityTower.ElementOf(Week.AddDays(7 * i), 1)).ToHashSet();
        Assert.Equal(InfinityTower.ElementOrder.Length, starts.Count);
    }

    [Fact]
    public void Floors_FourMobFloorsThenBossOfTheSameElement()
    {
        for (int floor = 1; floor <= 10; floor++)
        {
            var towerFloor = InfinityTower.Floor(Week, floor);
            bool boss = floor % 5 == 0;

            Assert.Equal(boss ? StageType.Boss : StageType.Mobs, towerFloor.Type);
            Assert.Equal(boss ? 1 : 0, towerFloor.Enemies.Count(e => e.IsBoss));
            // Босс — на 3-м слоте (правила расстановки — в шапке CampaignSeedData)
            Assert.All(towerFloor.Enemies.Where(e => e.IsBoss), e => Assert.Equal(3, e.Position));
            Assert.All(towerFloor.Enemies, e => Assert.Equal(towerFloor.Element, e.Hero.Element));
            Assert.Equal(towerFloor.Element, towerFloor.Arena.Element);
        }

        // На каждом этаже блока свой состав
        var lineups = Enumerable.Range(1, 5)
            .Select(floor => string.Join(",", InfinityTower.Floor(Week, floor).Enemies.Select(e => e.Hero.Name)))
            .ToList();
        Assert.Equal(5, lineups.Distinct().Count());
    }

    [Fact]
    public void EveryElement_HasValidMonsters()
    {
        foreach (var element in InfinityTower.ElementOrder)
        {
            foreach (var role in Enum.GetValues<TowerSeedData.Role>())
            {
                var hero = TowerSeedData.Create(element, role, spellLevel: 3);
                Assert.Equal(element, hero.Element);
                Assert.All(hero.SpellIds, id => Assert.True(SpellRegistry.All.ContainsKey(id), $"{hero.Name}: unknown spell {id}"));
                // У героя одно заклинание: кд нет, второе только мешало бы первому
                Assert.True(hero.SpellIds.Count <= 1, $"{hero.Name}: {hero.SpellIds.Count} spells");
                if (hero.AuraEffect != null)
                    Assert.NotNull(StatusEffectFactory.Create(hero.AuraEffect, 1, 1));
            }
        }
    }

    [Fact]
    public void EnemyStats_FollowTheEnemyLevelOfTheFloor_AndBossIsTougher()
    {
        CombatUnitInfo Guard(int floor)
        {
            var world = new World();
            new TowerFloorScenario(Factory, InfinityTower.Floor(Week, floor)).SpawnEnemies(world);
            return CombatSnapshot.Units(world).First(u => u.Position == 1);
        }

        // Этаж — уровень врагов: статы растут так же, как у врагов кампании этого уровня
        var template = TowerSeedData.Create(HeroElement.Fire, TowerSeedData.Role.Guard, 1);
        Assert.Equal(InfinityTower.FirstFloorEnemyLevel, InfinityTower.EnemyLevelFor(1));
        foreach (int floor in new[] { 1, 10, 30 })
        {
            int level = InfinityTower.EnemyLevelFor(floor);
            Assert.Equal(BalanceCalculator.ScaleStat(template.MaxHp, level), Guard(floor).MaxHp);
            Assert.Equal(BalanceCalculator.ScaleStat(template.Attack, level), Guard(floor).Attack);
        }

        var bossWorld = new World();
        new TowerFloorScenario(Factory, InfinityTower.Floor(Week, 5)).SpawnEnemies(bossWorld);
        var boss = CombatSnapshot.Units(bossWorld).Single(u => u.IsBoss);
        int bossHp = BalanceCalculator.ScaleStat(TowerSeedData.Create(HeroElement.Fire, TowerSeedData.Role.Boss, 1).MaxHp, InfinityTower.EnemyLevelFor(5));
        Assert.Equal((int)Math.Round(bossHp * InfinityTower.BossHpMultiplier), boss.MaxHp);
    }

    [Fact]
    public void FrostAura_DoesNotGrowWithFloor()
    {
        // Сила мороза — процент замедления, а не доля атаки: на высоких этажах она не должна расти
        int FrostPower(int floor)
        {
            var week = Enumerable.Range(0, 7).Select(i => Week.AddDays(7 * i)).First(w => InfinityTower.ElementOf(w, floor) == HeroElement.Frost);
            var world = new World();
            new TowerFloorScenario(Factory, InfinityTower.Floor(week, floor)).SpawnEnemies(world);
            return world.Auras.Values.Max(a => a.Power);
        }

        Assert.Equal(FrostPower(5), FrostPower(35));
    }

    [Fact]
    public void Rewards_GemsOfFloorElement_BossGivesElementInfinityChest()
    {
        var mobs = TowerRewards.For(_playerId, Week, 3, Now);
        Assert.Empty(mobs.Chests);
        var ruby = Assert.Single(mobs.Items);
        Assert.Equal(nameof(GemType.Ruby), ruby.SubType);
        Assert.Equal(TowerRewards.CommonGemsFor(3), ruby.Quantity);

        // 10-й этаж этой недели — ледяной босс: сапфиры, сундук бесконечности льда и (каждый второй босс) снаряжение
        var boss = TowerRewards.For(_playerId, Week, 10, Now);
        Assert.All(boss.Items, i => Assert.Equal(nameof(GemType.Sapphire), i.SubType));
        Assert.Contains(boss.Items, i => i.Rarity == Rarity.Uncommon);
        Assert.Contains(boss.Chests, c => c.ChestType == ChestType.Infinity && c.Element == HeroElement.Frost && c.ChestLevel == 2);
        Assert.Contains(boss.Chests, c => c.ChestType == ChestType.Equipment);

        // Золото и опыт этажа — как у уровня кампании с врагами того же уровня
        var sameEnemies = CampaignRewards.FixedFor(new CampaignStage { Type = StageType.Mobs, EnemyLevel = InfinityTower.EnemyLevelFor(3) });
        Assert.Equal(sameEnemies.Gold, mobs.Gold);
        Assert.Equal(sameEnemies.Experience, mobs.Experience);
    }

    [Fact]
    public void SkipReward_IsSumOfFloorRewards()
    {
        var floors = Enumerable.Range(1, 12).Select(f => TowerRewards.For(_playerId, Week, f, Now)).ToList();
        var skip = TowerRewards.ForFloors(_playerId, Week, 1, 12, Now);

        Assert.Equal(floors.Sum(r => r.Gold), skip.Gold);
        Assert.Equal(floors.Sum(r => r.Experience), skip.Experience);
        Assert.Equal(floors.Sum(r => r.Chests.Count), skip.Chests.Count);
        Assert.Equal(floors.SelectMany(r => r.Items).Sum(i => i.Quantity), skip.Items.Sum(i => i.Quantity));
        // Одинаковые камни сложены в одну стопку
        Assert.Equal(skip.Items.Count, skip.Items.Select(i => (i.SubType, i.Rarity)).Distinct().Count());
        Assert.All(skip.Items, i => Assert.Equal(skip.Id, i.RewardId));
    }

    [Fact]
    public void LootTables_ExistForEveryChestTheTowerGives()
    {
        var tables = LootSeedData.TablesWithEntries();
        for (int floor = 5; floor <= 100; floor += 5)
        {
            foreach (var chest in TowerRewards.For(_playerId, Week, floor, Now).Chests)
                Assert.Single(tables, t => t.ChestType == chest.ChestType && t.ChestLevel == chest.ChestLevel && t.Element == chest.Element);
        }

        // Герои и камни сундука бесконечности — только его стихии
        foreach (var table in tables.Where(t => t.ChestType == ChestType.Infinity))
        {
            Assert.All(table.Entries.Where(e => e.ItemType == ItemType.Hero), e => Assert.Equal(table.Element.ToString(), e.SubType));
            Assert.All(table.Entries.Where(e => e.ItemType == ItemType.Gem), e => Assert.Equal(InfinityTower.GemOf(table.Element).ToString(), e.SubType));
        }
    }

    [Fact]
    public async Task Overview_StartsFromFirstFloor_AndOffersSkipToLastWeeksBest()
    {
        var tower = new TowerRepository(_context);
        await tower.AdvanceAsync(_playerId, Week.AddDays(-14), 0, 4, Reward(), Now.AddDays(-14));
        await tower.AdvanceAsync(_playerId, Week.AddDays(-7), 0, 12, Reward(), Now.AddDays(-7));

        var overview = await CreateService().GetOverviewAsync(_playerId);

        Assert.Equal(0, overview.ClearedFloor);
        Assert.Equal(1, overview.CurrentFloor);
        Assert.Equal(12, overview.BestFloor);
        Assert.Equal(12, overview.SkipToFloor);
        Assert.Equal(TowerRewards.ForFloors(_playerId, Week, 1, 12, Now).Gold, overview.SkipReward!.Gold);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, overview.Floors.Select(f => f.Floor));
        Assert.Equal(StageStatus.Available, overview.Floors[0].Status);
        Assert.All(overview.Floors.Skip(1), f => Assert.Equal(StageStatus.Locked, f.Status));
        Assert.Equal(InfinityTower.RotationOf(Week), overview.Rotation);
    }

    [Fact]
    public async Task Skip_ClearsFloorsAndGrantsAllRewardsAtOnce()
    {
        var tower = new TowerRepository(_context);
        await tower.AdvanceAsync(_playerId, Week.AddDays(-7), 0, 10, Reward(), Now.AddDays(-7));
        await tower.AdvanceAsync(_playerId, Week, 0, 3, Reward(), Now);
        var service = CreateService();

        var result = await service.SkipAsync(_playerId);

        var expected = TowerRewards.ForFloors(_playerId, Week, 4, 10, Now);
        Assert.Equal(10, result.SkippedTo);
        Assert.Equal(10, await tower.GetFloorAsync(_playerId, Week));
        Assert.Equal(10 + expected.Gold, result.Resources.Gold);

        // Камни — сразу в инвентаре, сундуки боссов 5 и 10 — закрытыми: огненный и ледяной сундуки бесконечности
        var inventory = new InventoryRepository(_context);
        var items = await inventory.GetItemsAsync(_playerId);
        Assert.Equal(expected.Items.Sum(i => i.Quantity), items.Sum(i => i.Quantity));
        var chests = await inventory.GetClosedChestsAsync(_playerId);
        Assert.Contains(chests, c => c.ChestDropTable.ChestType == ChestType.Infinity && c.ChestDropTable.Element == HeroElement.Fire);
        Assert.Contains(chests, c => c.ChestDropTable.ChestType == ChestType.Infinity && c.ChestDropTable.Element == HeroElement.Frost);

        // Второй раз пропускать нечего
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SkipAsync(_playerId));
    }

    [Fact]
    public async Task OpenInfinityChest_DropsOnlyItsElement()
    {
        var tower = new TowerRepository(_context);
        await tower.AdvanceAsync(_playerId, Week.AddDays(-7), 0, 5, Reward(), Now.AddDays(-7));
        await CreateService().SkipAsync(_playerId);

        var chests = new ChestService(new InventoryRepository(_context), new ItemDropService(new Random(3)), Factory, new Random(3));
        var chest = (await chests.GetInventoryAsync(_playerId)).Chests.Single(c => c.Type == ChestType.Infinity);
        Assert.Equal(HeroElement.Fire, chest.Element);

        var opened = await chests.OpenAsync(_playerId, chest.Id);
        Assert.All(opened.Items, i => Assert.Equal(nameof(GemType.Ruby), i.SubType));
        Assert.All(opened.Heroes, h => Assert.Equal(HeroElement.Fire, h.Element));
    }

    [Fact]
    public async Task Fight_OnlyCurrentFloor()
    {
        var locked = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().FightAsync(_playerId, 2));
        Assert.Contains("locked", locked.Message);
    }

    [Fact]
    public async Task Overview_ListsFloorsWithUnclaimedRewards_AndClaimAllTakesThemAtOnce()
    {
        // Прошлая неделя: этаж 3 с незабранной наградой. Эта: 1 и 2 — награды ждут, 3 — уже забрана
        var tower = new TowerRepository(_context);
        var lastWeek = Week.AddDays(-7);
        var old = TowerRewards.For(_playerId, lastWeek, 3, Now.AddDays(-7));
        await tower.AdvanceAsync(_playerId, lastWeek, 0, 3, old, Now.AddDays(-7));
        var first = TowerRewards.For(_playerId, Week, 1, Now);
        var second = TowerRewards.For(_playerId, Week, 2, Now.AddMinutes(1));
        await tower.AdvanceAsync(_playerId, Week, 0, 1, first, Now);
        await tower.AdvanceAsync(_playerId, Week, 1, 2, second, Now.AddMinutes(1));
        await tower.AdvanceAsync(_playerId, Week, 2, 3, Reward(), Now.AddMinutes(2));
        var service = CreateService();

        var overview = await service.GetOverviewAsync(_playerId);

        Assert.Equal(4, overview.CurrentFloor);
        Assert.Equal(3, overview.UnclaimedRewards);
        Assert.Equal(new[] { 3, 1, 2, 3 }, overview.ClearedFloors.Select(f => f.Floor));
        Assert.Equal(new[] { true, false, false, false }, overview.ClearedFloors.Select(f => f.PreviousWeek));
        Assert.Equal(new Guid?[] { old.Id, first.Id, second.Id, null }, overview.ClearedFloors.Select(f => f.RewardId));
        Assert.Equal(first.Gold, overview.ClearedFloors[1].Reward!.Gold);
        // Стихия прошлонедельного этажа — по его неделе
        Assert.Equal(InfinityTower.ElementOf(lastWeek, 3), overview.ClearedFloors[0].Element);
        Assert.Equal(StageStatus.Available, overview.Floors[0].Status);

        var rewards = new RewardRepository(_context);
        var claim = await new RewardService(rewards, new PlayerRepository(_context), new TestClock(Now)).ClaimAllAsync(_playerId, RewardSource.Tower);

        Assert.Equal(new[] { old.Id, first.Id, second.Id }.Order(), claim.RewardIds.Order());
        Assert.Equal(old.Gold + first.Gold + second.Gold, claim.Reward.Gold);
        Assert.Equal(10 + claim.Reward.Gold, claim.Resources.Gold);
        // Одинаковые камни сложены в одну стопку
        Assert.Equal(claim.Reward.Items.Count, claim.Reward.Items.Select(i => (i.SubType, i.Rarity)).Distinct().Count());

        var after = await service.GetOverviewAsync(_playerId);
        Assert.Equal(0, after.UnclaimedRewards);
        Assert.Equal(3, Assert.Single(after.ClearedFloors).Floor);
    }

    [Fact]
    public async Task Advance_SameFloorTwice_SecondIsRejected()
    {
        var tower = new TowerRepository(_context);
        Assert.True(await tower.AdvanceAsync(_playerId, Week, 0, 1, Reward(), Now));
        Assert.False(await tower.AdvanceAsync(_playerId, Week, 0, 1, Reward(), Now));
        Assert.True(await tower.AdvanceAsync(_playerId, Week, 1, 2, Reward(), Now));
        Assert.False(await tower.AdvanceAsync(_playerId, Week, 1, 2, Reward(), Now));

        Assert.Equal(2, await tower.GetFloorAsync(_playerId, Week));
        Assert.Equal(2, await _context.PlayerRewards.CountAsync(r => r.PlayerId == _playerId && r.CreatedAt == Now));
    }

    [Fact]
    public void Balance_DemoSquadWinRateByFloor()
    {
        // Не проверка, а справка для балансировки: как далеко проходят отряды разной силы
        var squads = new (string Name, List<Hero> Heroes)[]
        {
            ("demo", DemoSquad()),
            ("demo, upgrade 10", DemoSquad().Select(h => WithUpgrade(h, 10)).ToList()),
            ("demo, upgrade 20", DemoSquad().Select(h => WithUpgrade(h, 20)).ToList())
        };

        foreach (var (name, squad) in squads)
        {
            var line = new List<string>();
            for (int floor = 1; floor <= 25; floor++)
            {
                var towerFloor = InfinityTower.Floor(Week, floor);
                var forecast = CombatPower.Forecast(() =>
                {
                    var world = new World();
                    Factory.CreateSquad(world, squad, teamId: 1);
                    new TowerFloorScenario(Factory, towerFloor).SpawnEnemies(world);
                    if (ArenaStatusEffect.FromArena(towerFloor.Arena) is { } effect)
                        world.AddGlobalEffect(effect);
                    return world;
                });
                line.Add($"{floor}:{forecast.WinChance * 100:0}%({(double)forecast.PlayerPower / forecast.EnemyPower:0.00})");
            }
            _output.WriteLine($"{name,-18} {string.Join(" ", line)}");
        }
    }

    private static Hero WithUpgrade(Hero hero, int level)
    {
        var copy = HeroSeedData.Heroes.Single(h => h.Id == hero.Id);
        copy.UpgradeLevel = level;
        return copy;
    }

    private static List<Hero> DemoSquad() => HeroSeedData.Heroes
        .Where(h => h.PlayerId == HeroSeedData.DemoPlayerId && h.PlaceInSquad != null)
        .OrderBy(h => h.PlaceInSquad)
        .ToList();

    // Уже забранная награда: для прогресса прошлых недель она не важна
    private PlayerReward Reward() => new()
    {
        Id = Guid.NewGuid(), PlayerId = _playerId, Source = RewardSource.Tower, CreatedAt = Now, ClaimedAt = Now
    };

    private TowerService CreateService()
    {
        var heroes = new Mock<IHeroesRepository>();
        heroes.Setup(h => h.GetSquadAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(DemoSquad());

        var rewards = new RewardRepository(_context);
        var clock = new TestClock(Now);
        var combat = new CombatService(Mock.Of<IWorldFactory>(), Mock.Of<ICombatEngine>(), []);
        return new TowerService(new TowerRepository(_context), heroes.Object, Factory, combat, rewards,
            new RewardService(rewards, new PlayerRepository(_context), clock), clock);
    }

    private sealed class TestClock(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(now);
    }
}
