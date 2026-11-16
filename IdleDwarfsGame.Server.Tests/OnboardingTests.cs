using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Стартовое обучение (Onboarding): путь новичка от обучающего боя до шахты, выдачи «один раз», пропуск
public class OnboardingTests : IDisposable
{
    private static readonly EntityFactory Factory = new();
    private static readonly DateTime Start = new(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(Start);
    private readonly Guid _playerId = Guid.NewGuid();

    private readonly OnboardingService _onboarding;
    private readonly CampaignService _campaign;
    private readonly RewardService _rewards;
    private readonly ChestService _chests;
    private readonly ExpeditionService _expeditions;
    private readonly MiningService _mining;
    private readonly TutorialService _tutorial;

    public OnboardingTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        var tutorials = new TutorialRepository(_context);
        var heroes = new HeroesRepository(_context, _clock);
        var players = new PlayerRepository(_context);
        var campaignRepository = new CampaignRepository(_context);
        ICombatScenario[] scenarios = [new CampaignScenario(campaignRepository, Factory)];
        var combat = new CombatService(new WorldFactory(heroes, Factory), new CombatEngine(new Random(1)), scenarios);
        var squadPower = new SquadPowerService(new PvpRepository(_context), Factory);

        _onboarding = new OnboardingService(_context, tutorials, squadPower, new Random(1), _clock);
        _campaign = new CampaignService(campaignRepository, heroes, Factory, scenarios, combat, new RewardRepository(_context),
            new Random(1), _clock, NoActivity.Instance, _onboarding);
        _rewards = new RewardService(new RewardRepository(_context), players, _clock);
        _chests = new ChestService(new InventoryRepository(_context), new ItemDropService(new Random(1)), Factory, new Random(1),
            NoActivity.Instance, players, _clock, _onboarding);
        var expeditionRepository = new ExpeditionRepository(_context);
        _expeditions = new ExpeditionService(expeditionRepository, heroes, players, Factory, _clock, NoActivity.Instance, _onboarding);
        _mining = new MiningService(new MiningRepository(_context, _clock), heroes, expeditionRepository, _clock, NoActivity.Instance, _onboarding);
        _tutorial = new TutorialService(tutorials, campaignRepository, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    // Как регистрация: игрок без дворфов и строка стартового обучения
    private async Task RegisterAsync(bool onboarding = true)
    {
        _context.Players.Add(new Player { Id = _playerId, Name = "Newbie" });
        if (onboarding)
            _context.PlayerTutorials.Add(Onboarding.StartRow(_playerId, Start));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    private async Task<Player> PlayerAsync() => await _context.Players.AsNoTracking().FirstAsync(p => p.Id == _playerId);

    private Task<System.Collections.Generic.List<Hero>> HeroesAsync() =>
        _context.Heroes.AsNoTracking().Where(h => h.PlayerId == _playerId).ToListAsync();

    // Бой → «Забрать» в кампании → сундук открыт
    private async Task FightClaimAndOpenAsync()
    {
        var fight = await _campaign.FightAsync(_playerId, TutorialBattle.StageId);
        Assert.True(fight.Tutorial);
        var overview = await _campaign.GetOverviewAsync(_playerId);
        var rewardId = overview.ClearedStages.Single(s => s.RewardId != null).RewardId!.Value;
        await _rewards.ClaimAsync(_playerId, rewardId);
        _context.ChangeTracker.Clear();

        var chest = (await _chests.GetInventoryAsync(_playerId)).Chests.Single();
        await _chests.OpenAsync(_playerId, chest.Id);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task NewPlayer_OnboardingIsActive_WithoutHeroes()
    {
        await RegisterAsync();

        Assert.True(await _onboarding.IsActiveAsync(_playerId));
        Assert.Empty(await HeroesAsync());
        Assert.True((await _tutorial.GetAsync(_playerId)).Onboarding.Active);
    }

    [Fact]
    public async Task TutorialBattle_GivesStarterChestAndGoldForTheFirstUpgrade()
    {
        await RegisterAsync();

        await FightClaimAndOpenAsync();

        var player = await PlayerAsync();
        Assert.Equal(Onboarding.StageGold, player.Gold);
        Assert.Equal(Onboarding.StageExperience, player.Experience);
        // Ровно на улучшение героя 1 → 2
        Assert.Equal(HeroUpgradeService.CostFor(1, 1)!.Gold, player.Gold);

        var heroes = await HeroesAsync();
        Assert.Equal(Onboarding.StarterHeroCount, heroes.Count);
        Assert.Equal(StarterSquad.Size, heroes.Count(h => h.PlaceInSquad != null));
        Assert.All(heroes, h => Assert.Equal(HeroLevels.Min, h.Level));
        Assert.True((await PlayerAsync()).SquadPower > 0);
    }

    [Fact]
    public async Task StarterHeroes_AreGivenOnce()
    {
        await RegisterAsync();
        await FightClaimAndOpenAsync();

        // Второй стартовый сундук (например, из письма) дворфов уже не даёт
        _context.PlayerChests.Add(new IdleDwarfsGame.Server.Entities.GameEntities.PlayerChest
        {
            Id = Guid.NewGuid(), PlayerId = _playerId, ChestDropTableId = LootSeedData.TableId(IdleDwarfsGame.Server.Enums.ChestType.Starter, 1)
        });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        var chest = (await _chests.GetInventoryAsync(_playerId)).Chests.Single();
        var opened = await _chests.OpenAsync(_playerId, chest.Id);

        Assert.Empty(opened.Heroes);
        Assert.Equal(Onboarding.StarterHeroCount, (await HeroesAsync()).Count);
    }

    [Fact]
    public async Task TutorialExpedition_TakesSeconds_AndPaysForTheMineUpgrade()
    {
        await RegisterAsync();
        await FightClaimAndOpenAsync();

        var board = await _expeditions.GetBoardAsync(_playerId);
        var tutorial = board.Expeditions.Single(e => e.TemplateKey == Onboarding.ExpeditionTemplate);
        Assert.Equal(Onboarding.ExpeditionSeconds, tutorial.DurationSeconds);
        // Обычные задания доски на месте
        Assert.Equal(ExpeditionService.Durations.Length, board.Expeditions.Count(e => e.TemplateKey != Onboarding.ExpeditionTemplate));

        var reserve = (await HeroesAsync()).Where(h => h.PlaceInSquad == null).Take(2).Select(h => h.Id).ToList();
        await _expeditions.StartAsync(_playerId, tutorial.Id, reserve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _expeditions.ClaimAsync(_playerId, tutorial.Id));

        _clock.Advance(TimeSpan.FromSeconds(Onboarding.ExpeditionSeconds));
        var claimed = await _expeditions.ClaimAsync(_playerId, tutorial.Id);

        Assert.Equal(Onboarding.ExpeditionGold, claimed.Reward.Gold);
        Assert.Equal(MiningService.UpgradeCost(MineType.Gold, MineUpgrade.Level, 1).Gold, claimed.Reward.Gold);
        // Больше не появляется
        Assert.DoesNotContain((await _expeditions.GetBoardAsync(_playerId)).Expeditions, e => e.TemplateKey == Onboarding.ExpeditionTemplate);
    }

    [Fact]
    public async Task Mine_FirstCollectWithADwarf_PaysForTheNextHeroUpgrade_Once()
    {
        await RegisterAsync();
        await FightClaimAndOpenAsync();
        await _context.Players.Where(p => p.Id == _playerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Gold, Onboarding.ExpeditionGold));
        long goldBefore = Onboarding.ExpeditionGold;

        var built = await _mining.BuildAsync(_playerId, Onboarding.MinePlot, Onboarding.MineKind);
        await _mining.UpgradeAsync(_playerId, built.Mine.Id, MineUpgrade.Level);
        var dwarf = (await HeroesAsync()).First(h => h.PlaceInSquad == null).Id;
        await _mining.AssignDwarfAsync(_playerId, built.Mine.Id, dwarf);
        _clock.Advance(TimeSpan.FromSeconds(5));

        var first = await _mining.CollectAsync(_playerId, built.Mine.Id);

        Assert.Equal(Onboarding.MineGold, first.Collected);
        Assert.Equal(goldBefore - Onboarding.ExpeditionGold + Onboarding.MineGold, (await PlayerAsync()).Gold);
        // Второй сбор — обычный: за секунды дворф накопал меньше монеты
        _clock.Advance(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAsync<MiningException>(() => _mining.CollectAsync(_playerId, built.Mine.Id));
    }

    [Fact]
    public async Task Skip_GivesEverything_AndEndsOnboarding()
    {
        await RegisterAsync();

        await _onboarding.SkipAsync(_playerId);

        Assert.False(await _onboarding.IsActiveAsync(_playerId));
        var heroes = await HeroesAsync();
        Assert.Equal(Onboarding.StarterHeroCount, heroes.Count);
        Assert.Equal(StarterSquad.Size, heroes.Count(h => h.PlaceInSquad != null));

        var mine = await _context.Mines.Include(m => m.Dwarves).AsNoTracking().SingleAsync(m => m.PlayerId == _playerId);
        Assert.Equal(Onboarding.MineKind, mine.Type);
        Assert.True(mine.Level >= MiningService.DwarfUnlockLevel);
        Assert.Single(mine.Dwarves);
        Assert.Null(heroes.Single(h => h.Id == mine.Dwarves[0].HeroId).PlaceInSquad);

        var player = await PlayerAsync();
        Assert.Equal(Onboarding.StageGold + Onboarding.ExpeditionGold + Onboarding.MineGold, player.Gold);
        Assert.Equal(Onboarding.StageExperience + Onboarding.ExpeditionExperience, player.Experience);

        // 1-й уровень кампании пройден, а здания обучения знакомы — их знакомства не запускаются
        var progress = await _tutorial.GetAsync(_playerId);
        Assert.Equal(1, progress.CampaignCleared);
        Assert.False((await _campaign.FightAsync(_playerId, TutorialBattle.StageId)).Tutorial);
        Assert.Empty(await _context.PlayerRewards.AsNoTracking().Where(r => r.PlayerId == _playerId).ToListAsync());
        foreach (var activity in Onboarding.CoveredActivities)
        {
            Assert.Equal(TutorialStatus.Skipped, progress.Scenarios[activity]);
            Assert.Contains(activity, progress.Done);
        }

        // Пропуск второй раз и обычный бой 1-го уровня ничего лишнего не дают
        await _onboarding.SkipAsync(_playerId);
        Assert.Equal(player.Gold, (await PlayerAsync()).Gold);
        Assert.Equal(TutorialStatus.Skipped, (await _tutorial.GetAsync(_playerId)).Scenarios[Onboarding.Key]);
    }

    [Fact]
    public async Task SkipAfterSomeSteps_GivesOnlyWhatIsLeft()
    {
        await RegisterAsync();
        await FightClaimAndOpenAsync();
        long gold = (await PlayerAsync()).Gold;

        await _onboarding.SkipAsync(_playerId);

        Assert.Equal(Onboarding.StarterHeroCount, (await HeroesAsync()).Count);
        Assert.Equal(gold + Onboarding.ExpeditionGold + Onboarding.MineGold, (await PlayerAsync()).Gold);
    }

    [Fact]
    public async Task OldAccount_HasNoOnboardingExceptions()
    {
        await RegisterAsync(onboarding: false);
        _context.Heroes.AddRange(StarterSquad.Create(_playerId, new Random(1)));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var fight = await _campaign.FightAsync(_playerId, TutorialBattle.StageId);
        var reward = await _context.PlayerRewards.Include(r => r.Chests).AsNoTracking().SingleAsync(r => r.PlayerId == _playerId);

        Assert.True(fight.Tutorial);
        Assert.DoesNotContain(reward.Chests, c => c.ChestType == IdleDwarfsGame.Server.Enums.ChestType.Starter);
        Assert.DoesNotContain((await _expeditions.GetBoardAsync(_playerId)).Expeditions, e => e.TemplateKey == Onboarding.ExpeditionTemplate);
        Assert.False((await _tutorial.GetAsync(_playerId)).Onboarding.Active);
    }

    [Fact]
    public async Task Complete_MarksTheBuildingsItWentThrough_AsSeen()
    {
        await RegisterAsync();

        await _onboarding.CompleteAsync(_playerId);
        var progress = await _tutorial.GetAsync(_playerId);

        Assert.False(progress.Onboarding.Active);
        Assert.All(Onboarding.CoveredActivities, key => Assert.Equal(TutorialStatus.Completed, progress.Scenarios[key]));
    }

    [Fact]
    public async Task Step_IsSaved_AndResetKeepsOnboarding()
    {
        await RegisterAsync();

        await _onboarding.SetStepAsync(_playerId, 7);
        await _tutorial.ResetAsync(_playerId);
        var progress = await _tutorial.GetAsync(_playerId);

        Assert.True(progress.Onboarding.Active);
        Assert.Equal(7, progress.Onboarding.Step);
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
