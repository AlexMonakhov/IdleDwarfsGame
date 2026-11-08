using System;
using System.Linq;
using System.Threading.Tasks;
using IdleDwarfsGame.Server.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class QuestTests : IDisposable
{
    // Пятница: идёт ивент недели (с понедельника 2 ноября)
    private static readonly DateTime Friday = new(2026, 11, 6, 10, 0, 0, DateTimeKind.Utc);
    // Воскресенье той же недели — день лавки, ивент уже не идёт
    private static readonly DateTime Sunday = new(2026, 11, 8, 10, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(Friday);
    private readonly ActivityTracker _activity;
    private readonly QuestService _service;
    private readonly Guid _playerId = Guid.NewGuid();

    public QuestTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Quester", RainbowCoins = 20 });
        _context.SaveChanges();

        var activityRepository = new ActivityRepository(_context);
        _activity = new ActivityTracker(activityRepository, _clock, NullLogger<ActivityTracker>.Instance);
        _service = new QuestService(new QuestRepository(_context), activityRepository, new PlayerRepository(_context), new Random(3), _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task ClearStagesAsync(int count)
    {
        for (int stage = 1; stage <= count; stage++)
            _context.PlayerCampaignProgress.Add(new PlayerCampaignProgress { PlayerId = _playerId, StageId = stage, ClearedAt = Friday });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    private async Task CompleteAsync(QuestInfo quest) => await _activity.RecordAsync(_playerId, quest.Kind, quest.Target);

    private async Task<Player> PlayerAsync() => await _context.Players.AsNoTracking().FirstAsync(p => p.Id == _playerId);

    /* ===== Доска ===== */

    [Fact]
    public async Task FirstVisit_GivesTwoEasyTwoMediumOneHard_WorthExactlyTheFullBar()
    {
        await ClearStagesAsync(30);
        var board = await _service.GetAsync(_playerId);

        Assert.Equal(DailyQuests.SlotCount, board.Quests.Count);
        Assert.Equal(
            [QuestDifficulty.Easy, QuestDifficulty.Easy, QuestDifficulty.Medium, QuestDifficulty.Medium, QuestDifficulty.Hard],
            board.Quests.Select(q => q.Difficulty));
        Assert.Equal(DailyQuests.MaxPoints, board.Quests.Sum(q => q.Points));
        // Разделов открыто много — действия на доске не повторяются
        Assert.Equal(board.Quests.Count, board.Quests.Select(q => q.Kind).Distinct().Count());
        Assert.All(board.Quests, q => Assert.True(q.Gold > 0 && q.ElderSeals > 0));
    }

    [Fact]
    public async Task SameDay_KeepsTheBoard_NewDay_BuildsAFreshOne()
    {
        var first = await _service.GetAsync(_playerId);
        var again = await _service.GetAsync(_playerId);
        Assert.Equal(first.Quests.Select(q => q.Id), again.Quests.Select(q => q.Id));

        _clock.Advance(TimeSpan.FromDays(1));
        var tomorrow = await _service.GetAsync(_playerId);
        Assert.Empty(first.Quests.Select(q => q.Id).Intersect(tomorrow.Quests.Select(q => q.Id)));
        Assert.Equal(DailyQuests.SlotCount, await _context.PlayerQuests.CountAsync());
    }

    [Fact]
    public async Task NewPlayer_GetsOnlyQuestsOfOpenFeatures_WithoutRepeatedTemplates()
    {
        var board = await _service.GetAsync(_playerId);

        Assert.All(board.Quests, q =>
            Assert.True(DailyQuests.IsOpen(DailyQuests.Find(q.Key)!.Feature, new PlayerProgress(0, null, false))));
        Assert.Equal(board.Quests.Count, board.Quests.Select(q => q.Key).Distinct().Count());
    }

    [Fact]
    public async Task GuildQuests_OnlyForGuildMembers()
    {
        await ClearStagesAsync(30);
        var context = await new PlayerRepository(_context).GetProgressAsync(_playerId);
        Assert.False(context.InGuild);

        var random = new Random(1);
        for (int i = 0; i < 200; i++)
            Assert.NotEqual(GameFeature.Guild, DailyQuests.Pick(QuestDifficulty.Hard, context, [], random).Feature);
    }

    /* ===== Прогресс и награды ===== */

    [Fact]
    public async Task Progress_CountsActionsDoneBeforeTheFirstVisit()
    {
        await _activity.RecordAsync(_playerId, ActivityKind.CampaignFight, 2);
        await _activity.RecordAsync(_playerId, ActivityKind.CampaignFight);
        await _activity.RecordAsync(_playerId, ActivityKind.CampaignWin, 40);

        var board = await _service.GetAsync(_playerId);
        var fight = board.Quests.Single(q => q.Kind == ActivityKind.CampaignFight);
        Assert.Equal(3, fight.Progress);
        Assert.True(fight.Completed);
        // Прогресс не больше цели
        Assert.All(board.Quests.Where(q => q.Kind == ActivityKind.CampaignWin), q => Assert.Equal(q.Target, q.Progress));
    }

    [Fact]
    public async Task Claim_NotCompleted_IsRefused()
    {
        var board = await _service.GetAsync(_playerId);

        var error = await Assert.ThrowsAsync<QuestException>(() => _service.ClaimAsync(_playerId, board.Quests[0].Id));
        Assert.Equal(QuestError.NotCompleted, error.Code);
    }

    [Fact]
    public async Task Claim_GrantsSealsGoldAndExperienceOnce()
    {
        var board = await _service.GetAsync(_playerId);
        var quest = board.Quests[0];
        await CompleteAsync(quest);

        var result = await _service.ClaimAsync(_playerId, quest.Id);
        var player = await PlayerAsync();
        Assert.Equal(quest.ElderSeals, player.ElderSeals);
        Assert.Equal(quest.Gold, player.Gold);
        Assert.Equal(quest.Experience, player.Experience);
        Assert.Equal(quest.Points, result.Board.Points);
        Assert.True(result.Board.Quests[0].Claimed);

        var error = await Assert.ThrowsAsync<QuestException>(() => _service.ClaimAsync(_playerId, quest.Id));
        Assert.Equal(QuestError.AlreadyClaimed, error.Code);
        Assert.Equal(quest.ElderSeals, (await PlayerAsync()).ElderSeals);
    }

    [Fact]
    public async Task YesterdaysCounters_DoNotCountToday()
    {
        await _activity.RecordAsync(_playerId, ActivityKind.CampaignFight, 10);
        _clock.Advance(TimeSpan.FromDays(1));

        var board = await _service.GetAsync(_playerId);
        Assert.All(board.Quests, q => Assert.Equal(0, q.Progress));
    }

    [Fact]
    public async Task Claimable_CountsCompletedQuestsAndReachedMilestones()
    {
        var board = await _service.GetAsync(_playerId);
        Assert.Equal(0, board.Claimable);

        await CompleteAsync(board.Quests[0]);
        await CompleteAsync(board.Quests[1]);
        Assert.Equal(2, (await _service.GetAsync(_playerId)).Claimable);

        // Два лёгких задания — 30 очков: забраны оба, открыт первый порог (20)
        await _service.ClaimAsync(_playerId, board.Quests[0].Id);
        var result = await _service.ClaimAsync(_playerId, board.Quests[1].Id);
        Assert.Equal(1, result.Board.Claimable);
    }

    /* ===== Шкала очков ===== */

    [Fact]
    public async Task Milestone_NeedsClaimedPoints_AndPaysOnlyOnce()
    {
        var board = await _service.GetAsync(_playerId);
        var error = await Assert.ThrowsAsync<QuestException>(() => _service.ClaimMilestoneAsync(_playerId, 0));
        Assert.Equal(QuestError.MilestoneNotReached, error.Code);

        // Выполнено, но не забрано — очков ещё нет
        await CompleteAsync(board.Quests[4]);
        error = await Assert.ThrowsAsync<QuestException>(() => _service.ClaimMilestoneAsync(_playerId, 0));
        Assert.Equal(QuestError.MilestoneNotReached, error.Code);

        await _service.ClaimAsync(_playerId, board.Quests[4].Id);
        long goldBefore = (await PlayerAsync()).Gold;
        var result = await _service.ClaimMilestoneAsync(_playerId, 0);
        Assert.True(result.Reward.Gold > 0);
        Assert.Equal(goldBefore + result.Reward.Gold, (await PlayerAsync()).Gold);
        Assert.True(result.Board.Milestones[0].Claimed);

        error = await Assert.ThrowsAsync<QuestException>(() => _service.ClaimMilestoneAsync(_playerId, 0));
        Assert.Equal(QuestError.AlreadyClaimed, error.Code);
    }

    [Fact]
    public async Task FullBar_GivesTheHeroChestAndAllSeals()
    {
        var board = await _service.GetAsync(_playerId);
        foreach (var quest in board.Quests)
        {
            await CompleteAsync(quest);
            await _service.ClaimAsync(_playerId, quest.Id);
        }
        for (int i = 0; i < DailyQuests.Milestones.Length; i++)
            await _service.ClaimMilestoneAsync(_playerId, i);

        var player = await PlayerAsync();
        int seals = board.Quests.Sum(q => q.ElderSeals) + DailyQuests.Milestones.Sum(m => m.ElderSeals);
        Assert.Equal(seals, player.ElderSeals);
        Assert.Equal(50, seals);

        var chests = await _context.PlayerChests.Include(c => c.ChestDropTable).Where(c => c.PlayerId == _playerId).ToListAsync();
        Assert.Contains(chests, c => c.ChestDropTable.ChestType == ChestType.Hero);
        Assert.Contains(chests, c => c.ChestDropTable.ChestType == ChestType.Gem);
    }

    [Fact]
    public async Task NewDay_ResetsMilestones()
    {
        var board = await _service.GetAsync(_playerId);
        await CompleteAsync(board.Quests[4]);
        await _service.ClaimAsync(_playerId, board.Quests[4].Id);
        await _service.ClaimMilestoneAsync(_playerId, 0);

        _clock.Advance(TimeSpan.FromDays(1));
        var tomorrow = await _service.GetAsync(_playerId);
        Assert.Equal(0, tomorrow.Points);
        Assert.All(tomorrow.Milestones, m => Assert.False(m.Claimed));
    }

    /* ===== Замена ===== */

    [Fact]
    public async Task Reroll_FirstIsFree_ThenCostsRainbowCoins()
    {
        await ClearStagesAsync(30);
        var board = await _service.GetAsync(_playerId);
        var quest = board.Quests[2];

        var rerolled = await _service.RerollAsync(_playerId, quest.Id);
        var replacement = rerolled.Quests.Single(q => q.Slot == quest.Slot);
        Assert.NotEqual(quest.Id, replacement.Id);
        Assert.NotEqual(quest.Key, replacement.Key);
        Assert.Equal(QuestDifficulty.Medium, replacement.Difficulty);
        Assert.False(rerolled.FreeReroll);
        Assert.Equal(20, (await PlayerAsync()).RainbowCoins);

        await _service.RerollAsync(_playerId, replacement.Id);
        Assert.Equal(20 - DailyQuests.RerollCost, (await PlayerAsync()).RainbowCoins);
    }

    [Fact]
    public async Task Reroll_WithoutCoins_IsRefusedAndKeepsTheQuest()
    {
        await _context.Players.Where(p => p.Id == _playerId).ExecuteUpdateAsync(s => s.SetProperty(p => p.RainbowCoins, 0));
        var board = await _service.GetAsync(_playerId);
        var free = await _service.RerollAsync(_playerId, board.Quests[0].Id);
        var quest = free.Quests[0];

        var error = await Assert.ThrowsAsync<QuestException>(() => _service.RerollAsync(_playerId, quest.Id));
        Assert.Equal(QuestError.NotEnoughRainbowCoins, error.Code);
        Assert.Equal(quest.Id, (await _service.GetAsync(_playerId)).Quests[0].Id);
    }

    [Fact]
    public async Task Reroll_ClaimedQuest_IsRefused()
    {
        var board = await _service.GetAsync(_playerId);
        await CompleteAsync(board.Quests[0]);
        await _service.ClaimAsync(_playerId, board.Quests[0].Id);

        var error = await Assert.ThrowsAsync<QuestException>(() => _service.RerollAsync(_playerId, board.Quests[0].Id));
        Assert.Equal(QuestError.AlreadyClaimed, error.Code);
    }

    /* ===== Ивент ===== */

    [Fact]
    public async Task DuringAnEvent_QuestsAlsoPayItsCurrency()
    {
        var board = await _service.GetAsync(_playerId);
        Assert.NotNull(board.Event);
        Assert.All(board.Quests, q => Assert.True(q.EventCurrency > 0));

        var quest = board.Quests[0];
        await CompleteAsync(quest);
        await _service.ClaimAsync(_playerId, quest.Id);

        var wallet = await _context.PlayerEventWallets.SingleAsync(w => w.PlayerId == _playerId);
        Assert.Equal(WorldEvents.Of(Friday).StartsAt, wallet.EventStart);
        Assert.Equal(quest.EventCurrency, wallet.Currency);
        Assert.Equal(quest.EventCurrency, wallet.Earned);
    }

    [Fact]
    public async Task OnTheShopDay_QuestsPayNoEventCurrency()
    {
        _clock.Advance(Sunday - Friday);
        var board = await _service.GetAsync(_playerId);

        Assert.Null(board.Event);
        Assert.All(board.Quests, q => Assert.Equal(0, q.EventCurrency));
        Assert.All(board.Milestones, m => Assert.Equal(0, m.EventCurrency));
    }

    /* ===== Счётчики ===== */

    [Fact]
    public async Task Activity_AddsUpWithinTheDay()
    {
        await _activity.RecordAsync(_playerId, ActivityKind.MineDig, 30);
        await _activity.RecordAsync(_playerId, ActivityKind.MineDig, 12);
        await _activity.RecordAsync(_playerId, ActivityKind.MineDig, 0);

        var counters = await new ActivityRepository(_context).GetDayAsync(_playerId, Friday.Date);
        Assert.Equal(42, counters[ActivityKind.MineDig]);
        Assert.Single(counters);
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
