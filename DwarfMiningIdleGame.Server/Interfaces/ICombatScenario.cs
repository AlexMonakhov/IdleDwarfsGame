public enum CombatMode { Boss, Tower }

public interface ICombatScenario
{
    CombatMode Mode { get; }

    // Спавн врагов в мир (твоя текущая логика)
    Task PopulateEnemiesAsync(World world, int difficultyOrFloor);

    // Логика выдачи наград в зависимости от режима
    Task<RewardEntity> CalculateRewardsAsync(int difficultyOrFloor, List<CombatLogEntry> combatResult);
}



