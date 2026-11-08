public interface ICommunityMineRepository
{
    Task<List<CommunityDeposit>> GetDepositsAsync(DateTime eventStart);

    // Месторождение вместе с вкладами и дворфами
    Task<DepositState?> GetStateAsync(Guid depositId);
    Task<List<DepositState>> GetStatesAsync(DateTime eventStart);

    /// <summary>
    /// Creates the deposit. False — a parallel request has already created one with this index.
    /// </summary>
    Task<bool> TryCreateAsync(CommunityDeposit deposit);

    /// <summary>
    /// Saves a settled deposit (it must still have <paramref name="save"/>.ExpectedVersion), every worker row,
    /// sends and recalls dwarves and charges gold for an upgrade — all in one transaction.
    /// </summary>
    Task<DepositSaveOutcome> SaveAsync(DepositSave save);

    // Закрытые месторождения ивента, где у игрока есть вклад и награда ещё не забрана
    Task<List<(CommunityDeposit Deposit, DepositWorker Worker)>> GetUnclaimedAsync(Guid playerId, DateTime eventStart);

    /// <summary>
    /// Records the claims and credits their currency to the event wallet. False — some were claimed in parallel.
    /// </summary>
    Task<bool> ClaimAsync(Guid playerId, DateTime eventStart, IReadOnlyList<DepositRewardClaim> claims);

    Task<Dictionary<Guid, string>> GetNamesAsync(IReadOnlyCollection<Guid> playerIds);
}

public record DepositState(CommunityDeposit Deposit, List<DepositWorker> Workers, List<DepositDwarf> Dwarves);

public class DepositSave
{
    public CommunityDeposit Deposit { get; set; } = null!;
    public int ExpectedVersion { get; set; }
    // Все вклады после пересчёта (новые строки добавятся)
    public List<DepositWorker> Workers { get; set; } = new();
    public List<DepositDwarf> AddDwarves { get; set; } = new();
    public List<Guid> RemoveHeroIds { get; set; } = new();
    // Месторождение закрылось — отпустить всех дворфов
    public bool ReleaseAll { get; set; }
    // Улучшение: кто платит и сколько золота
    public Guid? PayerId { get; set; }
    public long Gold { get; set; }
}

public enum DepositSaveOutcome
{
    Saved,
    // Месторождение изменили параллельным запросом — пересчитать и повторить
    Changed,
    NotEnoughGold,
    // Дворфа только что заняли в другом месте
    HeroBusy
}
