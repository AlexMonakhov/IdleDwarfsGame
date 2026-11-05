// Игрок, которого можно встретить в PvP, с сохранённой мощью отряда (Player.SquadPower).
// Сам отряд для подбора не нужен — его грузят только для выбранных соперников (GetDefenseSquadsAsync)
public record PvpCandidate(Guid UserId, string Username, Guid PlayerId, int Power);
// Игрок рейтинговой арены: мощь закреплённого снимка и очки в сезоне подбора (null — в сезоне ещё не дрался).
// Снимок героев — GetRankedSquadsAsync, тоже только для выбранных
public record RankedCandidate(Guid UserId, string Username, int Power, int? Rating);
public record PvpLeaderboardEntry(Guid UserId, string Username, int Points, int Wins, int Losses);
public record PvpHistoryEntry(PvpBattle Battle, string AttackerName, string DefenderName);

public interface IPvpRepository
{
    /// <summary>
    /// All accounts except <paramref name="excludeUserId"/> with a non-empty defence squad (stored SquadPower above zero).
    /// Reads only ids, names and powers — no heroes.
    /// </summary>
    Task<List<PvpCandidate>> GetCandidatesAsync(Guid excludeUserId);
    Task<PvpCandidate?> GetCandidateAsync(Guid userId);

    /// <summary>
    /// Defence squads of the given players: heroes placed in the squad, ordered by place. Players without one are absent.
    /// </summary>
    Task<Dictionary<Guid, List<Hero>>> GetDefenseSquadsAsync(IReadOnlyCollection<Guid> playerIds);
    /// <summary>
    /// Stored squad power of every player, for the recalculation on startup.
    /// </summary>
    Task<Dictionary<Guid, int>> GetSquadPowersAsync();
    Task SetSquadPowerAsync(Guid playerId, int power);

    /// <summary>
    /// All accounts except <paramref name="excludeUserId"/> with a locked ranked squad, with their ranked points in the given season.
    /// </summary>
    Task<List<RankedCandidate>> GetRankedCandidatesAsync(Guid excludeUserId, PvpSeasonType type, DateTime start);
    Task<RankedCandidate?> GetRankedCandidateAsync(Guid userId, PvpSeasonType type, DateTime start);
    /// <summary>
    /// Heroes of the locked ranked squads of the given users.
    /// </summary>
    Task<Dictionary<Guid, List<Hero>>> GetRankedSquadsAsync(IReadOnlyCollection<Guid> userIds);
    /// <summary>
    /// Every locked ranked squad, for the power recalculation on startup.
    /// </summary>
    Task<List<RankedSquad>> GetAllRankedSquadsAsync();
    Task SetRankedSquadPowerAsync(Guid userId, int power);

    Task<RankedSquad?> GetRankedSquadAsync(Guid userId);
    /// <summary>
    /// Saves the user's ranked squad. Replaces an existing one only if it is still locked at <paramref name="previousLockedAt"/>
    /// (null — the user has none yet). Returns false if a parallel request has locked a squad first.
    /// </summary>
    Task<bool> SaveRankedSquadAsync(RankedSquad squad, DateTime? previousLockedAt);

    /// <summary>
    /// Defenders the attacker has beaten on this day in this mode.
    /// </summary>
    Task<HashSet<Guid>> GetBeatenAsync(Guid attackerUserId, DateTime day, PvpMode mode);

    /// <summary>
    /// The user's scouts on this day in both modes.
    /// </summary>
    Task<List<PvpScout>> GetScoutsAsync(Guid userId, DateTime day);
    /// <summary>
    /// Saves a scout. Re-scouting the same opponent on the same day in the same mode overwrites it for free;
    /// a new opponent is saved only while the user has fewer than <paramref name="dailyLimit"/> scouts that day.
    /// Returns false if the limit is reached.
    /// </summary>
    Task<bool> SaveScoutAsync(PvpScout scout, int dailyLimit);

    /// <summary>
    /// Saves the battle and applies its points to the attacker's score in each season of its mode (not below zero) in one transaction.
    /// Returns false if it is a win over a defender already beaten on that day.
    /// </summary>
    Task<bool> RecordBattleAsync(PvpBattle battle, IReadOnlyList<(PvpSeasonType Type, DateTime Start)> seasons);

    Task<PvpSeasonScore?> GetScoreAsync(Guid userId, PvpMode mode, PvpSeasonType type, DateTime start);
    /// <summary>
    /// How many players are above this score in its season: more points, then more wins, then reached earlier.
    /// </summary>
    Task<int> CountAheadAsync(PvpSeasonScore score);
    Task<List<PvpLeaderboardEntry>> GetLeaderboardAsync(PvpMode mode, PvpSeasonType type, DateTime start, int limit);

    /// <summary>
    /// The user's latest battles in this mode, both attacks and defences, newest first.
    /// </summary>
    Task<List<PvpHistoryEntry>> GetHistoryAsync(Guid userId, PvpMode mode, int limit);
}
