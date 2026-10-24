public class RoundStartSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        // Если это НЕ начало нового раунда — ничего не делаем!
        if (!context.IsNewRound) return;

        // 1. Проверка лимита раундов (защита от бесконечного боя)
        if (context.CurrentRound > context.MaxRounds)
        {
            context.CurrentRound = context.MaxRounds;
            context.CombatEnded = true;
            context.Win = false; // Ничья по лимиту засчитывается игроку как поражение
            context.WinningTeam = 2;
            context.AddLog(new CombatLogEntry
            {
                EventType = CombatEventType.CombatEnd,
                Phase = "End",
                ActionType = "RoundLimitReached",
                WinningTeam = 2,
                Text = "Round limit reached"
            });
            return;
        }

        // 2. Лог начала раунда для UI
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.RoundStart,
            Phase = "RoundStart",
            ActionType = $"RoundStart_{context.CurrentRound}"
        });
    }
}
