// Игрок, которого можно встретить в PvP: его отряд — герои с местом в отряде, по порядку мест
public record PvpCandidate(Guid UserId, string Username, List<Hero> Squad);
// Игрок рейтинговой арены: закреплённый снимок отряда и очки в сезоне подбора (null — в сезоне ещё не дрался)
public record RankedCandidate(Guid UserId, string Username, List<Hero> Squad, int? Rating);
public record PvpLeaderboardEntry(Guid UserId, string Username, int Points, int Wins, int Losses);
public record PvpHistoryEntry(PvpBattle Battle, string AttackerName, string DefenderName);

public interface IPvpRepository
{
    /// <summary>
    /// All accounts except <paramref name="excludeUserId"/> that have at least one hero placed in the squad.
    /// </summary>
    Task<List<PvpCandidate>> GetCandidatesAsync(Guid excludeUserId);
    Task<PvpCandidate?> GetCandidateAsync(Guid userId);

    /// <summary>
    /// All accounts except <paramref name="excludeUserId"/> with a locked ranked squad, with their ranked points in the given season.
    /// </summary>
    Task<List<RankedCandidate>> GetRankedCandidatesAsync(Guid excludeUserId, PvpSeasonType type, DateTime start);
    Task<RankedCandidate?> GetRankedCandidateAsync(Guid userId, PvpSeasonType type, DateTime start);

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
