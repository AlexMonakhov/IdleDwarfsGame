// Игрок, которого можно встретить в PvP: его отряд — герои с местом в отряде, по порядку мест
public record PvpCandidate(Guid UserId, string Username, List<Hero> Squad);
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
    /// Defenders the attacker has beaten on this day.
    /// </summary>
    Task<HashSet<Guid>> GetBeatenAsync(Guid attackerUserId, DateTime day);

    /// <summary>
    /// Saves the battle and applies its points to the attacker's score in each season (not below zero) in one transaction.
    /// Returns false if it is a win over a defender already beaten on that day.
    /// </summary>
    Task<bool> RecordBattleAsync(PvpBattle battle, IReadOnlyList<(PvpSeasonType Type, DateTime Start)> seasons);

    Task<PvpSeasonScore?> GetScoreAsync(Guid userId, PvpSeasonType type, DateTime start);
    /// <summary>
    /// How many players are above this score in its season: more points, then more wins, then reached earlier.
    /// </summary>
    Task<int> CountAheadAsync(PvpSeasonScore score);
    Task<List<PvpLeaderboardEntry>> GetLeaderboardAsync(PvpSeasonType type, DateTime start, int limit);

    /// <summary>
    /// The user's latest battles, both attacks and defences, newest first.
    /// </summary>
    Task<List<PvpHistoryEntry>> GetHistoryAsync(Guid userId, int limit);
}
