public class RequestActivityTests
{
    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void NoRequestsSinceStart_IsIdle()
    {
        var activity = new RequestActivity(new TestClock(DateTimeOffset.UtcNow));

        Assert.False(activity.WasActiveWithin(WorldEventsTicker.ActiveWindow));
    }

    [Fact]
    public void RecentRequest_IsActive_UntilWindowPasses()
    {
        var clock = new TestClock(new DateTimeOffset(2026, 11, 13, 12, 0, 0, TimeSpan.Zero));
        var activity = new RequestActivity(clock);

        activity.Touch();
        clock.Now += WorldEventsTicker.ActiveWindow;
        Assert.True(activity.WasActiveWithin(WorldEventsTicker.ActiveWindow));

        clock.Now += TimeSpan.FromSeconds(1);
        Assert.False(activity.WasActiveWithin(WorldEventsTicker.ActiveWindow));
    }
}
