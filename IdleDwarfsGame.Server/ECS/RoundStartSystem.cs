public class RoundStartSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        // Если это не начало нового раунда — просто выходим!
        if (!context.IsNewRound) return;

        // 1. Проверка лимита раундов (Таймаут боя)
        if (context.CurrentRound > context.MaxRounds)
        {
            context.CombatEnded = true;
            context.Win = false; // Превышен лимит раундов — поражение нападающего
            context.Logs.Add(new CombatLogEntry
            {
                ActionType = "RoundLimitReached",
                Damage = 0
            });
            return;
        }

        // 2. Лог начала раунда для UI
        context.Logs.Add(new CombatLogEntry
        {
            ActionType = $"RoundStart_{context.CurrentRound}",
            Damage = 0
        });

        // 3. Здесь же можно вызывать глобальные ауры / погоду / восстанавливать энергию раз в раунд
    }
}