using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

// Симулятор баланса: прогоняет настоящий CombatEngine на эталонных отрядах и печатает шанс победы и длину боя.
// Запуск отчёта: dotnet test --filter BalanceSimulation --logger "console;verbosity=detailed".
// Границы баланса, которые должны держаться, проверяет CampaignBalanceTests (ниже)
public class BalanceSimulation(ITestOutputHelper output)
{
    private static readonly EntityFactory Factory = new();
    public const int Fights = 100;

    // Стартовый отряд (5 дворфов 1-го ранга), прокачанный до level
    public static List<Hero> Starters(int level) => StarterSquad.Create(Guid.NewGuid(), new Random(7))
        .Select(h => { h.UpgradeLevel = level; return h; })
        .ToList();

    // Отряд дворфов ранга rank на уровне level: профессии как у стартового отряда, способности — как из сундука
    public static List<Hero> Squad(int rank, int level, int seed = 7)
    {
        var random = new Random(seed);
        var playerId = Guid.NewGuid();
        return ProfessionBook.SquadOrder.Select((profession, index) =>
        {
            var hero = HeroGenerator.Create(playerId, rank, string.Empty, random);
            var stats = ProfessionBook.Of(profession);
            hero.Profession = profession;
            hero.MaxHp = HeroGenerator.ScaleToRank(stats.MaxHp, rank);
            hero.Attack = HeroGenerator.ScaleToRank(stats.Attack, rank);
            hero.Speed = stats.Speed;
            hero.UpgradeLevel = level;
            hero.PlaceInSquad = index + 1;
            return hero;
        }).ToList();
    }

    public record Outcome(double WinRate, double Rounds);

    public static Outcome Campaign(List<Hero> squad, int stageId, int fights = Fights)
    {
        var repository = new CampaignTests.SeedCampaignRepository();
        var stage = repository.Stage(stageId);
        var scenario = new CampaignScenario(repository, Factory);
        int wins = 0, rounds = 0;
        for (int seed = 0; seed < fights; seed++)
        {
            var world = new World();
            Factory.CreateSquad(world, squad, teamId: 1);
            scenario.SpawnEnemies(world, stage);
            world.AddGlobalEffect(ArenaStatusEffect.FromArena(stage.Arena)!);
            var context = new CombatEngine(new Random(seed)).Run(world);
            if (context.Win) wins++;
            rounds += context.CurrentRound;
        }
        return new Outcome((double)wins / fights, (double)rounds / fights);
    }

    // Средний урон отряда по боссу гильдии за одну попытку
    public static double GuildBossDamage(List<Hero> squad, GuildBossDefinition definition, int fights = 20)
    {
        var templates = GuildBossSeedData.Heroes.ToDictionary(h => h.Id);
        var damages = new List<int>();
        foreach (var variant in definition.Variants)
        {
            for (int seed = 0; seed < fights; seed++)
            {
                var scenario = new GuildBossScenario(Factory, templates[variant.TemplateHeroId], definition, definition.MaxHp);
                var world = new World();
                Factory.CreateSquad(world, squad, teamId: 1);
                scenario.PopulateEnemiesAsync(world, definition.Level).Wait();
                world.AddGlobalEffect(ArenaStatusEffect.FromArena(GuildBosses.ArenaOf(variant))!);
                new CombatEngine(new Random(seed)).Run(world, GuildBosses.FightRounds);
                damages.Add(definition.MaxHp - scenario.BossHpLeft);
            }
        }
        return damages.Average();
    }

    [Fact]
    public void Report()
    {
        int[] levels = [1, 3, 5, 7, 10];
        output.WriteLine("Campaign, starter squad (rank 1) by upgrade level: win% / rounds");
        output.WriteLine("stage | " + string.Join(" | ", levels.Select(l => $"lvl {l,2}")));
        foreach (int stageId in Enumerable.Range(1, 10))
        {
            var cells = levels.Select(l => Campaign(Starters(l), stageId, 50)).Select(o => $"{o.WinRate * 100,3:F0}% {o.Rounds,4:F1}");
            output.WriteLine($"{stageId,5} | " + string.Join(" | ", cells));
        }

        output.WriteLine("");
        output.WriteLine("Guild bosses: average damage per attempt and attempts to kill");
        var squads = new (string Name, List<Hero> Squad)[]
        {
            ("rank 1 lvl 10", Starters(10)),
            ("rank 2 lvl 20", Squad(2, 20)),
            ("rank 3 lvl 50", Squad(3, 50)),
            ("rank 3 lvl 25", Squad(3, 25)),
            ("rank 4 lvl 50", Squad(4, 50))
        };
        foreach (var definition in GuildBosses.All)
        {
            foreach (var (name, squad) in squads)
            {
                double damage = GuildBossDamage(squad, definition, 5);
                output.WriteLine($"{definition.Tier,-6} (lvl {definition.Level,2}, {definition.MaxHp,9:N0} HP) / {name}: {damage,9:N0} per attempt, ~{definition.MaxHp / Math.Max(1, damage):F0} attempts");
            }
        }
    }
}

// Границы баланса кампании: если правка статов, спеллов или врагов их ломает — кривая сложности уехала
public class CampaignBalanceTests
{
    private const int Fights = 20;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void FirstStages_AreWonByTheStarterSquad(int stageId) =>
        Assert.True(BalanceSimulation.Campaign(BalanceSimulation.Starters(1), stageId, Fights).WinRate >= 0.9);

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    public void Bosses_NeedAnUpgradedSquad(int stageId) =>
        Assert.True(BalanceSimulation.Campaign(BalanceSimulation.Starters(1), stageId, Fights).WinRate <= 0.1);

    [Fact]
    public void LastBoss_IsWonByAMaxedRankOneSquad() =>
        Assert.True(BalanceSimulation.Campaign(BalanceSimulation.Starters(10), 10, Fights).WinRate >= 0.7);

    [Fact]
    public void RankTwoSquad_WinsEveryStageQuickly()
    {
        var squad = BalanceSimulation.Squad(2, 5);
        foreach (int stageId in Enumerable.Range(1, 10))
        {
            var outcome = BalanceSimulation.Campaign(squad, stageId, Fights);
            Assert.True(outcome.WinRate >= 0.95, $"stage {stageId}: {outcome.WinRate:P0}");
            Assert.True(outcome.Rounds <= 9, $"stage {stageId}: {outcome.Rounds:F1} rounds");
        }
    }
}

// Контрольные точки длинной кампании: какой отряд проходит босса главы, а какой ещё нет
public class LongCampaignBalanceTests
{
    private const int Fights = 20;

    [Theory]
    [InlineData(50, 2, 20, 2, 1)]
    [InlineData(150, 3, 50, 3, 1)]
    [InlineData(250, 4, 25, 4, 1)]
    [InlineData(350, 4, 50, 4, 25)]
    [InlineData(500, 4, 100, 4, 50)]
    public void ChapterBoss_IsWonByTheReferenceSquadOnly(int stageId, int rank, int level, int weakRank, int weakLevel)
    {
        var strong = BalanceSimulation.Campaign(BalanceSimulation.Squad(rank, level), stageId, Fights);
        var weak = BalanceSimulation.Campaign(BalanceSimulation.Squad(weakRank, weakLevel), stageId, Fights);

        Assert.True(strong.WinRate >= 0.5, $"rank {rank} lvl {level}: {strong.WinRate:P0}");
        Assert.True(weak.WinRate <= 0.1, $"rank {weakRank} lvl {weakLevel}: {weak.WinRate:P0}");
    }

    [Fact]
    public void Campaign_HasAllStagesWithEnemies()
    {
        var stages = CampaignSeedData.Stages;
        Assert.Equal(CampaignGenerator.TotalStages, stages.Length);
        Assert.Equal(Enumerable.Range(1, CampaignGenerator.TotalStages), stages.Select(s => s.Id));
        var enemies = CampaignSeedData.StageEnemies.GroupBy(e => e.StageId).ToDictionary(g => g.Key, g => g.ToList());
        var heroes = CampaignSeedData.Heroes.Concat(HeroSeedData.Heroes).Select(h => h.Id).ToHashSet();
        foreach (var stage in stages)
        {
            Assert.InRange(enemies[stage.Id].Count, 2, 5);
            Assert.All(enemies[stage.Id], e => Assert.Contains(e.HeroId, heroes));
            Assert.Equal(stage.Type == StageType.Boss, enemies[stage.Id].Any(e => e.IsBoss));
        }
        Assert.Equal(CampaignSeedData.StageEnemies.Length, CampaignSeedData.StageEnemies.Select(e => e.Id).Distinct().Count());
    }

    [Fact]
    public void EnemyLevel_NeverDrops()
    {
        var levels = CampaignSeedData.Stages.Skip(CampaignGenerator.HandcraftedStages).Select(s => s.EnemyLevel).ToList();
        Assert.True(levels.Zip(levels.Skip(1)).All(p => p.Second >= p.First));
    }
}
