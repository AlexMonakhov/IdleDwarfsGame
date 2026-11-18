using System;
using System.Linq;
using System.Threading.Tasks;
using IdleDwarfsGame.Server.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class AchievementTests : IDisposable
{
    private static readonly DateTime Start = new(2026, 11, 6, 10, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TestClock _clock = new(Start);
    private readonly ActivityTracker _activity;
    private readonly AchievementService _achievements;
    private readonly LoginCalendarService _calendar;
    private readonly Guid _playerId = Guid.NewGuid();

    public AchievementTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Achiever" });
        _context.SaveChanges();

        var players = new PlayerRepository(_context);
        _activity = new ActivityTracker(new ActivityRepository(_context), _clock, NullLogger<ActivityTracker>.Instance);
        _achievements = new AchievementService(new AchievementRepository(_context), players, _clock);
        _calendar = new LoginCalendarService(new LoginCalendarRepository(_context), players, _clock);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<Player> PlayerAsync() => await _context.Players.AsNoTracking().FirstAsync(p => p.Id == _playerId);

    private Task<int> ChestCountAsync() => _context.PlayerChests.CountAsync(c => c.PlayerId == _playerId);

    /* ===== Календарь входов ===== */

    [Fact]
    public async Task Calendar_FirstVisit_OffersDayOne_ClaimGrantsIt_SecondClaimSameDayIsRejected()
    {
        var calendar = await _calendar.GetAsync(_playerId);
        Assert.Equal(1, calendar.Day);
        Assert.False(calendar.ClaimedToday);
        Assert.Equal(LoginCalendar.CycleLength, calendar.Days.Count);
        Assert.True(calendar.Days[0].Today);

        var result = await _calendar.ClaimAsync(_playerId);
        Assert.True(result.Calendar.ClaimedToday);
        Assert.Equal(1, result.Calendar.ClaimedDays);
        Assert.Equal(1, result.Calendar.Streak);
        Assert.True(result.Calendar.Days[0].Claimed);

        var error = await Assert.ThrowsAsync<AchievementException>(() => _calendar.ClaimAsync(_playerId));
        Assert.Equal(AchievementError.AlreadyClaimed, error.Code);
    }

    [Fact]
    public async Task Calendar_ConsecutiveDays_Advance_AndWeeklyRewardGrantsRainbowCoinsAndChests()
    {
        for (int day = 1; day <= 7; day++)
        {
            var result = await _calendar.ClaimAsync(_playerId);
            Assert.Equal(day, result.Calendar.Day);
            _clock.Advance(TimeSpan.FromDays(1));
        }

        var player = await PlayerAsync();
        int expectedRainbow = LoginCalendar.Days.Take(7).Sum(d => d.RainbowCoins);
        int expectedSeals = LoginCalendar.Days.Take(7).Sum(d => d.ElderSeals);
        Assert.Equal(expectedRainbow, player.RainbowCoins);
        Assert.Equal(expectedSeals, player.ElderSeals);
        Assert.Equal(LoginCalendar.Days.Take(7).Sum(d => d.Chests.Length), await ChestCountAsync());

        var calendar = await _calendar.GetAsync(_playerId);
        Assert.Equal(8, calendar.Day);
        Assert.Equal(7, calendar.Streak);
        Assert.Equal(7, calendar.BestStreak);
    }

    [Fact]
    public async Task Calendar_MissedDay_StartsOverFromDayOne_ButKeepsBestStreakAndTotal()
    {
        await _calendar.ClaimAsync(_playerId);
        _clock.Advance(TimeSpan.FromDays(1));
        await _calendar.ClaimAsync(_playerId);

        _clock.Advance(TimeSpan.FromDays(2 + LoginCalendar.MissedDaysAllowed));
        var calendar = await _calendar.GetAsync(_playerId);
        Assert.True(calendar.Broken);
        Assert.Equal(1, calendar.Day);
        Assert.Equal(0, calendar.ClaimedDays);
        Assert.Equal(0, calendar.Streak);

        var result = await _calendar.ClaimAsync(_playerId);
        Assert.Equal(1, result.Calendar.Day);
        Assert.Equal(1, result.Calendar.Streak);
        Assert.Equal(2, result.Calendar.BestStreak);
        Assert.Equal(3, result.Calendar.TotalDays);
    }

    [Fact]
    public async Task Calendar_AfterTheLastDay_NextCycleStartsFromDayOne_StreakGoesOn()
    {
        for (int day = 1; day <= LoginCalendar.CycleLength; day++)
        {
            await _calendar.ClaimAsync(_playerId);
            _clock.Advance(TimeSpan.FromDays(1));
        }

        var calendar = await _calendar.GetAsync(_playerId);
        Assert.Equal(2, calendar.Cycle);
        Assert.Equal(1, calendar.Day);
        Assert.Equal(0, calendar.ClaimedDays);
        Assert.Equal(LoginCalendar.CycleLength, calendar.Streak);
        Assert.Equal(LoginCalendar.Days.Sum(d => d.RainbowCoins), (await PlayerAsync()).RainbowCoins);
    }

    /* ===== Достижения ===== */

    [Fact]
    public async Task Achievement_ProgressComesFromLifetimeCounters_AcrossDays()
    {
        await _activity.RecordAsync(_playerId, ActivityKind.ChestOpen, 6);
        _clock.Advance(TimeSpan.FromDays(3));
        await _activity.RecordAsync(_playerId, ActivityKind.ChestOpen, 5);

        var board = await _achievements.GetAsync(_playerId);
        var chests = board.Achievements.Single(a => a.Key == "chests_opened");
        Assert.Equal(11, chests.Progress);
        Assert.Equal(1, chests.ReachedTiers);
        Assert.Equal(0, chests.ClaimedTiers);
        Assert.Equal(1, board.Claimable);
    }

    [Fact]
    public async Task Achievement_ClaimTakesEveryReachedTier_GrantsTheirRewards_ThenNothingLeft()
    {
        var achievement = Achievements.Find("chests_opened")!;
        await _activity.RecordAsync(_playerId, ActivityKind.ChestOpen, (int)achievement.Targets[1]);

        var result = await _achievements.ClaimAsync(_playerId, achievement.Key);
        var info = result.Board.Achievements.Single(a => a.Key == achievement.Key);
        Assert.Equal(2, info.ClaimedTiers);
        Assert.Equal(Achievements.Tiers[0].RainbowCoins + Achievements.Tiers[1].RainbowCoins, result.Granted.RainbowCoins);

        var player = await PlayerAsync();
        Assert.Equal(result.Granted.RainbowCoins, player.RainbowCoins);
        Assert.Equal(result.Granted.ElderSeals, player.ElderSeals);
        Assert.Equal(Achievements.Tiers[1].Chests.Length, await ChestCountAsync());

        var error = await Assert.ThrowsAsync<AchievementException>(() => _achievements.ClaimAsync(_playerId, achievement.Key));
        Assert.Equal(AchievementError.NothingToClaim, error.Code);
    }

    [Fact]
    public async Task Achievement_LoginStreak_UnlocksTheSnowFrame()
    {
        var streak = Achievements.Find("login_streak")!;
        for (int day = 0; day < streak.Targets[streak.FrameTier]; day++)
        {
            await _calendar.ClaimAsync(_playerId);
            _clock.Advance(TimeSpan.FromDays(1));
        }

        var before = await _achievements.GetAsync(_playerId);
        Assert.DoesNotContain("snow", before.UnlockedFrames);

        var result = await _achievements.ClaimAllAsync(_playerId);
        Assert.Contains("snow", result.Granted.Frames);
        Assert.Contains("snow", result.Board.UnlockedFrames);
        Assert.Equal(0, result.Board.Claimable);
        // Дни входа тоже засчитаны
        Assert.True(result.Board.Achievements.Single(a => a.Key == "login_days").ClaimedTiers >= 1);
    }

    [Fact]
    public async Task Achievement_UnknownKey_AndNothingToClaimAll_AreRejected()
    {
        var unknown = await Assert.ThrowsAsync<AchievementException>(() => _achievements.ClaimAsync(_playerId, "nope"));
        Assert.Equal(AchievementError.UnknownAchievement, unknown.Code);

        var nothing = await Assert.ThrowsAsync<AchievementException>(() => _achievements.ClaimAllAsync(_playerId));
        Assert.Equal(AchievementError.NothingToClaim, nothing.Code);
    }

    [Fact]
    public void Achievements_HaveUniqueKeys_AscendingTargets_AndFramesOnExistingTiers()
    {
        Assert.Equal(Achievements.All.Length, Achievements.All.Select(a => a.Key).Distinct().Count());
        Assert.All(Achievements.All, a =>
        {
            Assert.True(a.Key.Length <= 32);
            Assert.True(a.Targets.Length <= Achievements.Tiers.Length);
            Assert.True(a.Targets.Zip(a.Targets.Skip(1)).All(p => p.First < p.Second));
            Assert.Equal(a.Metric == AchievementMetric.Activity, a.Kind != null);
            if (a.Frame != null) Assert.InRange(a.FrameTier, 0, a.Targets.Length - 1);
        });
        Assert.Equal(["embers", "runes", "snow"], Achievements.All.Where(a => a.Frame != null).Select(a => a.Frame!).Order());
    }

    private sealed class TestClock(DateTime start) : TimeProvider
    {
        public DateTime UtcNow { get; private set; } = start;
        public void Advance(TimeSpan by) => UtcNow += by;
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
