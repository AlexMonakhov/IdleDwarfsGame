public enum CombatMode { Boss, Tower, Campaign, GuildBoss }

public interface ICombatScenario
{
    CombatMode Mode { get; }

    // Спавн врагов в мир (твоя текущая логика)
    Task PopulateEnemiesAsync(World world, int difficultyOrFloor);

    // Логика выдачи наград в зависимости от режима. null — режим наград не даёт
    Task<RewardEntity?> CalculateRewardsAsync(int difficultyOrFloor, List<CombatLogEntry> combatResult);
}



