// Бой по заранее собранному миру — для режимов, где отряд не берётся из БД как есть:
// наёмник друга в отряде, ставки на бой двух игроков, глубинный спуск (HP и реликвии сохраняются между боями).
// То же, что CombatService.RunCombat, но без наград сценария. Движок можно передать с сидом — тогда бой повторяем
public static class PreparedFight
{
    public static CombatResult Run(ICombatEngine engine, World world, int maxRounds)
    {
        var units = CombatSnapshot.Units(world);
        var context = engine.Run(world, maxRounds);
        return new CombatResult
        {
            Units = units,
            Logs = context.Logs,
            WinningTeam = context.Win ? 1 : 2,
            Rounds = context.CurrentRound
        };
    }

    // Эффект арены (из БД кампании) — глобальным эффектом мира
    public static void AddArena(World world, Arena? arena)
    {
        if (arena != null && ArenaStatusEffect.FromArena(arena) is { } effect)
            world.AddGlobalEffect(effect);
    }
}
