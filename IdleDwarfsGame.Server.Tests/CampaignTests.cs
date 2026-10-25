using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Xunit.Abstractions;

public class CampaignTests
{
    private readonly ITestOutputHelper _output;

    public CampaignTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly EntityFactory Factory = new();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void SeededStage_EndsAndLogsReplayConsistently(int stageId)
    {
        var repository = new SeedCampaignRepository();
        var stage = repository.Stage(stageId);
        var scenario = new CampaignScenario(repository, Factory);
        int wins = 0;

        for (int seed = 0; seed < 50; seed++)
        {
            var world = new World();
            Factory.CreateSquad(world, DemoSquad(), teamId: 1);
            scenario.SpawnEnemies(world, stage);
            world.AddGlobalEffect(ArenaStatusEffect.FromArena(stage.Arena)!);

            var units = CombatSnapshot.Units(world);
            var context = new CombatEngine(new Random(seed)).Run(world);

            Assert.True(context.CombatEnded);
            CombatEngineTests.AssertLogsReplay(units, context.Logs);
            if (context.Win) wins++;
        }

        _output.WriteLine($"Stage {stageId} ({stage.Name}): player wins {wins}/50");
    }

    [Fact]
    public void Boss_GetsHpMultiplierAndBossFlag()
    {
        var repository = new SeedCampaignRepository();
        var stage = repository.Stage(5);
        var bossEntry = stage.Enemies.Single(e => e.IsBoss);

        var world = new World();
        new CampaignScenario(repository, Factory).SpawnEnemies(world, stage);
        var boss = CombatSnapshot.Units(world).Single(u => u.IsBoss);

        int expectedHp = (int)Math.Round(BalanceCalculator.ScaleStat(bossEntry.Hero.MaxHp, stage.EnemyLevel) * bossEntry.HpMultiplier);
        Assert.Equal(bossEntry.Hero.Name, boss.Name);
        Assert.Equal(expectedHp, boss.MaxHp);
        Assert.Equal(expectedHp, boss.Hp);
        Assert.Contains("Burn", boss.Immunities);
    }

    [Fact]
    public async Task Overview_ShowsCurrentStageAndNextLockedOnes()
    {
        var repository = new SeedCampaignRepository();
        await repository.MarkClearedAsync(HeroSeedData.DemoPlayerId, 1);
        await repository.MarkClearedAsync(HeroSeedData.DemoPlayerId, 2);

        var overview = await CreateService(repository).GetOverviewAsync(HeroSeedData.DemoPlayerId);

        Assert.Equal(3, overview.CurrentStageId);
        Assert.Equal(2, overview.ClearedCount);
        Assert.Equal(new[] { 3, 4, 5, 6, 7 }, overview.Stages.Select(s => s.Id));
        Assert.Equal(StageStatus.Available, overview.Stages[0].Status);
        Assert.All(overview.Stages.Skip(1), s => Assert.Equal(StageStatus.Locked, s.Status));
        Assert.Equal(5, overview.Squad.Count);
        Assert.Equal("Burn", overview.Stages[0].Arena.EffectName);
    }

    [Fact]
    public async Task Overview_EmptyWhenCampaignCompleted()
    {
        var repository = new SeedCampaignRepository();
        foreach (var stage in CampaignSeedData.Stages)
            await repository.MarkClearedAsync(HeroSeedData.DemoPlayerId, stage.Id);

        var overview = await CreateService(repository).GetOverviewAsync(HeroSeedData.DemoPlayerId);

        Assert.Null(overview.CurrentStageId);
        Assert.Empty(overview.Stages);
    }

    [Fact]
    public async Task Fight_LockedStageIsRejected()
    {
        var service = CreateService(new SeedCampaignRepository());

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => service.FightAsync(HeroSeedData.DemoPlayerId, 2));
        Assert.Contains("locked", error.Message);
    }

    private static List<Hero> DemoSquad() => HeroSeedData.Heroes
        .Where(h => h.PlayerId == HeroSeedData.DemoPlayerId && h.PlaceInSquad != null)
        .OrderBy(h => h.PlaceInSquad)
        .ToList();

    private static CampaignService CreateService(ICampaignRepository repository)
    {
        var heroes = new Mock<IHeroesRepository>();
        heroes.Setup(h => h.GetSquadAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(DemoSquad());

        var scenarios = new ICombatScenario[] { new CampaignScenario(repository, Factory) };
        var combatService = new CombatService(Mock.Of<IWorldFactory>(), Mock.Of<ICombatEngine>(), scenarios, Mock.Of<IRewardService>());
        return new CampaignService(repository, heroes.Object, Factory, scenarios, combatService);
    }

    // Кампания из сидов в памяти, без БД
    internal class SeedCampaignRepository : ICampaignRepository
    {
        private readonly Dictionary<int, CampaignStage> _stages;
        private readonly HashSet<(Guid, int)> _cleared = new();

        public SeedCampaignRepository()
        {
            var heroes = HeroSeedData.Heroes.Concat(CampaignSeedData.Heroes).ToDictionary(h => h.Id);
            var arenas = CampaignSeedData.Arenas.ToDictionary(a => a.Id);
            var enemies = CampaignSeedData.StageEnemies;

            _stages = CampaignSeedData.Stages.ToDictionary(s => s.Id, s =>
            {
                s.Arena = arenas[s.ArenaId];
                s.Enemies = enemies.Where(e => e.StageId == s.Id).OrderBy(e => e.Position).ToList();
                s.Enemies.ForEach(e => e.Hero = heroes[e.HeroId]);
                return s;
            });
        }

        public CampaignStage Stage(int id) => _stages[id];

        public Task<List<int>> GetStageIdsAsync() => Task.FromResult(_stages.Keys.Order().ToList());

        public Task<List<CampaignStage>> GetStagesAsync(IReadOnlyCollection<int> stageIds) =>
            Task.FromResult(stageIds.Order().Select(id => _stages[id]).ToList());

        public Task<CampaignStage?> GetStageAsync(int stageId) => Task.FromResult(_stages.GetValueOrDefault(stageId));

        public Task<HashSet<int>> GetClearedStageIdsAsync(Guid playerId) =>
            Task.FromResult(_cleared.Where(c => c.Item1 == playerId).Select(c => c.Item2).ToHashSet());

        public Task<bool> MarkClearedAsync(Guid playerId, int stageId) => Task.FromResult(_cleared.Add((playerId, stageId)));
    }
}
