using System.Linq;

public class DamageSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        int attacker = context.CurrentActor;
        if (attacker == -1 || !world.TurnState.ContainsKey(attacker)) return;

        // Если спелл уже был скастован в SpellSystem (или персонаж в стане) - пропускаем
        var state = world.TurnState[attacker];
        if (!state.CanAct || state.ActionCompleted) return;

        int target = FindTarget(world, attacker); // Используем твой метод поиска цели
        if (target == -1)
        {
            context.CombatEnded = true;
            return;
        }

        // 1. Наносим урон
        int damage = world.Attack[attacker].Damage;
        world.Health[target].Current -= damage;

        // 2. НАЧИСЛЯЕМ ЭНЕРГИЮ ЗА УДАР
        if (world.Energy.ContainsKey(attacker))
        {
            var energy = world.Energy[attacker];
            energy.Current += energy.GainPerAttack;

            // Не даем энергии превысить максимальное значение
            if (energy.Current > energy.Max)
                energy.Current = energy.Max;
        }

        // 3. Пишем лог
        context.Logs.Add(new CombatLogEntry
        {
            AttackerId = attacker,
            TargetId = target,
            AttackerTeam = world.Team[attacker].TeamId,
            TargetTeam = world.Team[target].TeamId,
            ActionType = "BasicAttack",
            Damage = damage,
            TargetRemainingHp = Math.Max(0, world.Health[target].Current)
        });
    }

    private int FindTarget(World world, int attacker)
    {
        int enemyTeam = world.Team[attacker].TeamId == 1 ? 2 : 1;

        return world.Entities
            .FirstOrDefault(e =>
                world.Team[e.Id].TeamId == enemyTeam &&
                world.Health[e.Id].Current > 0)?.Id ?? -1;
    }
}