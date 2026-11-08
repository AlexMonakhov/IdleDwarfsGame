public interface IWorldBossRepository
{
    // Последний этап ивента (живой или уже убитый); null — боссов ещё не было
    Task<WorldBoss?> GetLatestAsync(DateTime eventStart);

    /// <summary>
    /// Creates the boss of the next stage. False — a parallel request has already created this stage.
    /// </summary>
    Task<bool> TryCreateAsync(WorldBoss boss);

    Task<DateTime?> GetCooldownAsync(Guid playerId);

    /// <summary>
    /// Starts the attack cooldown. Null — started; otherwise the moment the current cooldown ends.
    /// </summary>
    Task<DateTime?> TryStartCooldownAsync(Guid playerId, DateTime now, DateTime readyAt);

    /// <summary>
    /// Takes up to <paramref name="damage"/> HP from the boss (never below zero), adds it to the player's contribution
    /// and credits <paramref name="attackCurrency"/> to the player's wallet of the event, all in one transaction.
    /// </summary>
    Task<WorldBossDamageResult> ApplyDamageAsync(WorldBoss boss, Guid playerId, int damage, int attackCurrency, DateTime now);

    // Лучшие по урону в боссе — с именами
    Task<List<WorldBossDamageEntry>> GetTopAsync(Guid bossId, int count);

    Task<WorldBossContribution?> GetContributionAsync(Guid bossId, Guid playerId);

    // Убитые боссы ивента, которых игрок бил и за которых ещё не забрал награду
    Task<List<(WorldBoss Boss, WorldBossContribution Contribution)>> GetUnclaimedKillsAsync(Guid playerId, DateTime eventStart);

    /// <summary>
    /// Records the claims and credits their currency to the player's wallet of the event. False — some of them
    /// have just been claimed by a parallel request; nothing is credited then.
    /// </summary>
    Task<bool> ClaimAsync(Guid playerId, DateTime eventStart, IReadOnlyList<WorldBossRewardClaim> claims);

    // Урон за весь ивент: лучшие игроки, а также урон и место игрока
    Task<List<WorldBossDamageEntry>> GetEventTopAsync(DateTime eventStart, int count);
    Task<(long Damage, int Rank)> GetEventRankAsync(DateTime eventStart, Guid playerId);
}

public record WorldBossDamageResult(int AppliedDamage, int CurrentHp, bool Killed);

public record WorldBossDamageEntry(Guid PlayerId, string Name, long Damage);
