using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

// Прогресс обучения: статусы сценариев, отметка «попробовал» из учёта действий, сброс
public class TutorialTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly TutorialService _service;
    private readonly ActivityTracker _activity;
    private readonly Guid _playerId = Guid.NewGuid();

    public TutorialTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Newbie" });
        _context.SaveChanges();

        var tutorials = new TutorialRepository(_context);
        _service = new TutorialService(tutorials, new CampaignRepository(_context), TimeProvider.System);
        _activity = new ActivityTracker(new ActivityRepository(_context), TimeProvider.System, NullLogger<ActivityTracker>.Instance, tutorials);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task NewPlayer_HasNoProgress()
    {
        var progress = await _service.GetAsync(_playerId);

        Assert.Empty(progress.Scenarios);
        Assert.Empty(progress.Done);
        Assert.Equal(0, progress.CampaignCleared);
    }

    [Fact]
    public async Task Status_IsSavedAndOverwritten()
    {
        await _service.SetStatusAsync(_playerId, "warehouse", TutorialStatus.Skipped);
        await _service.SetStatusAsync(_playerId, "warehouse", TutorialStatus.Completed);
        await _service.SetStatusAsync(_playerId, "altar.abilities", TutorialStatus.Skipped);

        var progress = await _service.GetAsync(_playerId);

        Assert.Equal(TutorialStatus.Completed, progress.Scenarios["warehouse"]);
        Assert.Equal(TutorialStatus.Skipped, progress.Scenarios["altar.abilities"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Warehouse")]
    [InlineData("ware house")]
    [InlineData("1warehouse")]
    public async Task InvalidKey_IsRejected(string key)
    {
        await Assert.ThrowsAsync<TutorialException>(() => _service.SetStatusAsync(_playerId, key, TutorialStatus.Completed));
    }

    [Fact]
    public async Task TooManyKeys_AreRejected_ButKnownKeysStillUpdate()
    {
        for (int i = 0; i < TutorialService.MaxKeysPerPlayer; i++)
            _context.PlayerTutorials.Add(new PlayerTutorial { PlayerId = _playerId, Key = $"k{i}", Status = TutorialStatus.Completed });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<TutorialException>(() => _service.SetStatusAsync(_playerId, "one-more", TutorialStatus.Completed));
        await _service.SetStatusAsync(_playerId, "k0", TutorialStatus.Skipped);
    }

    [Fact]
    public async Task SeveralKeys_AreMarkedAtOnce_AndInvalidKeyWritesNothing()
    {
        await _service.SetStatusAsync(_playerId, "warehouse", TutorialStatus.Completed);

        await Assert.ThrowsAsync<TutorialException>(() =>
            _service.SetStatusesAsync(_playerId, ["forge", "Bad Key"], TutorialStatus.Skipped));
        Assert.DoesNotContain("forge", (await _service.GetAsync(_playerId)).Scenarios.Keys);

        await _service.SetStatusesAsync(_playerId, ["warehouse", "forge", "tower", "forge"], TutorialStatus.Skipped);
        var progress = await _service.GetAsync(_playerId);
        Assert.Equal(TutorialStatus.Skipped, progress.Scenarios["warehouse"]);
        Assert.Equal(TutorialStatus.Skipped, progress.Scenarios["forge"]);
        Assert.Equal(TutorialStatus.Skipped, progress.Scenarios["tower"]);
    }

    [Fact]
    public async Task ActivityAction_MarksActivityDone_Once()
    {
        await _activity.RecordAsync(_playerId, ActivityKind.ChestOpen);
        await _activity.RecordAsync(_playerId, ActivityKind.ChestOpen);
        await _activity.RecordAsync(_playerId, ActivityKind.ExpeditionStart);
        // У этого действия нет своей активности
        await _activity.RecordAsync(_playerId, ActivityKind.ExpeditionClaim);

        var progress = await _service.GetAsync(_playerId);

        Assert.Equal(["expeditions", "warehouse"], progress.Done);
        Assert.Empty(progress.Scenarios);
    }

    [Fact]
    public async Task Done_AndStatus_ShareTheRow()
    {
        await _service.SetStatusAsync(_playerId, "warehouse", TutorialStatus.Completed);
        await _activity.RecordAsync(_playerId, ActivityKind.ChestOpen);

        var progress = await _service.GetAsync(_playerId);

        Assert.Equal(TutorialStatus.Completed, progress.Scenarios["warehouse"]);
        Assert.Contains("warehouse", progress.Done);
        Assert.Equal(1, await _context.PlayerTutorials.CountAsync(t => t.PlayerId == _playerId));
    }

    [Fact]
    public async Task Reset_ClearsStatuses_ButKeepsDoneActivities()
    {
        await _service.SetStatusAsync(_playerId, "warehouse", TutorialStatus.Completed);
        await _service.SetStatusAsync(_playerId, "mine", TutorialStatus.Skipped);
        await _activity.RecordAsync(_playerId, ActivityKind.ChestOpen);

        await _service.ResetAsync(_playerId);
        var progress = await _service.GetAsync(_playerId);

        Assert.Empty(progress.Scenarios);
        Assert.Equal(["warehouse"], progress.Done);
    }

    [Fact]
    public async Task Progress_CountsClearedCampaignStages()
    {
        _context.PlayerCampaignProgress.Add(new PlayerCampaignProgress { PlayerId = _playerId, StageId = 1, ClearedAt = DateTime.UtcNow });
        _context.PlayerCampaignProgress.Add(new PlayerCampaignProgress { PlayerId = _playerId, StageId = 2, ClearedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        Assert.Equal(2, (await _service.GetAsync(_playerId)).CampaignCleared);
    }
}
