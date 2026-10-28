// Бой PvP: против нападавшего выходит сохранённый отряд другого игрока — те же герои и позиции, что у него в отряде.
// Бой идёт на арене PvP (RisingFuryArenaEffect), наград за сам бой нет — меняются очки сезона (PvpService)
public class PvpScenario(IEntityFactory factory, IReadOnlyList<Hero> defenderSquad) : ICombatScenario
{
    public CombatMode Mode => CombatMode.Pvp;

    public int MaxRounds => PvpArena.FightRounds;

    public Task PopulateEnemiesAsync(World world, int level)
    {
        SpawnDefenders(world);
        return Task.CompletedTask;
    }

    // То же без боя — для превью соперника и прогноза
    public List<int> SpawnDefenders(World world) => factory.CreateSquad(world, defenderSquad, teamId: 2);

    public Task<RewardEntity?> CalculateRewardsAsync(int level, List<CombatLogEntry> logs) =>
        Task.FromResult<RewardEntity?>(null);
}
