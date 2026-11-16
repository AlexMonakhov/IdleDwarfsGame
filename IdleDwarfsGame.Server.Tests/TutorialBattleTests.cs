using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Xunit;

// Обучающий бой (TutorialBattle): всегда одинаковый, выигран, и в нём есть всё, на чём фронт останавливается
// с пояснением (паузы tutorial/battle.ts на фронте ждут эти события)
public class TutorialBattleTests
{
    private static readonly EntityFactory Factory = new();

    // Новичок: своих дворфов нет, наставник один. withSquad — аккаунт, у которого отряд уже был
    private static (CombatContext Context, List<CombatUnitInfo> Units) Run(bool withSquad = false, int squadSeed = 1)
    {
        var repository = new CampaignTests.SeedCampaignRepository();
        var stage = repository.Stage(TutorialBattle.StageId);
        var playerId = Guid.NewGuid();

        var world = new World();
        List<Hero> squad = withSquad ? StarterSquad.Create(playerId, new Random(squadSeed)) : [];
        Factory.CreateSquad(world, TutorialBattle.Squad(playerId, squad), teamId: 1);
        new CampaignScenario(repository, Factory).SpawnEnemies(world, stage);
        PreparedFight.AddArena(world, stage.Arena);
        var units = CombatSnapshot.Units(world);
        return (TutorialBattle.Engine().Run(world), units);
    }

    private static int MentorEntity(List<CombatUnitInfo> units) => units.Single(u => u.HeroId == TutorialBattle.MentorId).EntityId;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Mentor_Wins(bool withSquad)
    {
        var (context, units) = Run(withSquad);

        Assert.True(context.Win);
        CombatEngineTests.AssertLogsReplay(units, context.Logs);
    }

    [Fact]
    public void Newbie_FightsWithTheMentorAlone()
    {
        var (_, units) = Run();

        var mine = Assert.Single(units, u => u.Team == 1);
        Assert.Equal(TutorialBattle.MentorId, mine.HeroId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Mentor_CastsFireballOnHisSecondTurn_AtSeveralEnemies(bool withSquad)
    {
        var (context, units) = Run(withSquad);
        int mentor = MentorEntity(units);

        var turns = context.Logs
            .Where(l => l.AttackerId == mentor && l.EventType is CombatEventType.Attack or CombatEventType.SkillCast)
            .ToList();
        Assert.Equal(CombatEventType.Attack, turns[0].EventType);
        Assert.Equal(CombatEventType.SkillCast, turns[1].EventType);
        Assert.Equal("Fireball", turns[1].SkillName);
        Assert.Equal(3, turns[1].SkillLevel);

        var hits = context.Logs.Where(l => l.ActionId == turns[1].ActionId && l.EventType == CombatEventType.Damage).ToList();
        Assert.True(hits.Count >= 2, $"Fireball hit {hits.Count} enemies");
        Assert.Contains(context.Logs, l => l.AttackerId == mentor && l.EventType == CombatEventType.StatusApplied);
    }

    // Паузы фронта: каст врага, смерть врага и победа — все есть в бою
    [Fact]
    public void Battle_HasEveryPausedEvent()
    {
        var (context, _) = Run();

        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.SkillCast && l.AttackerTeam == 2);
        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.Death && l.TargetTeam == 2);
        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.CombatEnd);
    }

    [Fact]
    public void Battle_IsAlwaysTheSame()
    {
        static string Shape((CombatContext Context, List<CombatUnitInfo> Units) run) =>
            JsonSerializer.Serialize(run.Context.Logs.Select(l => new { l.EventType, l.AttackerId, l.TargetId, l.Damage, l.SkillName }));

        Assert.Equal(Shape(Run()), Shape(Run()));
        // Имена стартового отряда случайны, статы — нет
        Assert.Equal(Shape(Run(true, 1)), Shape(Run(true, 2)));
    }

    [Fact]
    public async Task FirstTryOfStageOne_IsTheTutorialBattle()
    {
        var repository = new CampaignTests.SeedCampaignRepository();
        var playerId = Guid.NewGuid();
        var heroes = new Mock<IHeroesRepository>();
        heroes.Setup(h => h.GetSquadAsync(playerId)).ReturnsAsync(new List<Hero>());
        var scenario = new CampaignScenario(repository, Factory);
        // Обычный бой здесь собрать не из чего (WorldFactory — пустой мок): пройти может только обучающий
        var combat = new CombatService(Mock.Of<IWorldFactory>(), new CombatEngine(new Random(0)), [scenario]);
        var service = new CampaignService(repository, heroes.Object, Factory, [scenario], combat,
            Mock.Of<IRewardRepository>(), new Random(0), TimeProvider.System, NoActivity.Instance);

        var first = await service.FightAsync(playerId, TutorialBattle.StageId);

        Assert.True(first.Tutorial);
        Assert.Equal(1, first.Combat.WinningTeam);
        Assert.Contains(first.Combat.Units, u => u.HeroId == TutorialBattle.MentorId);
        Assert.Equal(TutorialBattle.StageId + 1, first.CurrentStageId);
        Assert.False(TutorialBattle.AppliesTo(TutorialBattle.StageId, alreadyCleared: true));
    }
}
