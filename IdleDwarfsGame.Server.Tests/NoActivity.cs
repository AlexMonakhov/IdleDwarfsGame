using System;
using System.Threading.Tasks;

// Учёт действий для тестов, которым задания не нужны: ничего не записывает
public sealed class NoActivity : IActivityTracker
{
    public static readonly NoActivity Instance = new();

    public Task RecordAsync(Guid playerId, ActivityKind kind, int amount = 1) => Task.CompletedTask;
}
