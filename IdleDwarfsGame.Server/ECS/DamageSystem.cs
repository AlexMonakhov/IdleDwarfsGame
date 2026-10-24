using System.Linq;

public class DamageSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        int attacker = context.CurrentActor;
        if (attacker == -1 || !world.TurnState.ContainsKey(attacker) || !world.IsAlive(attacker)) return;

        // Если актёр уже сделал ход в SpellSystem (или оглушён) — пропускаем
        var state = world.TurnState[attacker];
        if (!state.CanAct || state.ActionCompleted) return;

        // Бьём ближайшего живого врага (передняя позиция). Если врагов нет — бой завершит HealthSystem
        var target = world.GetAliveEnemies(attacker).FirstOrDefault();
        if (target == null) return;

        // 1. Наносим урон
        int damage = world.Attack[attacker].Damage;
        int hpLeft = world.ApplyDamage(target.Id, damage);

        // 2. Начисляем энергию за удар
        if (world.Energy.ContainsKey(attacker))
        {
            var energy = world.Energy[attacker];
            energy.Current = Math.Min(energy.Max, energy.Current + energy.GainPerAttack);
        }

        state.ActionCompleted = true;

        // 3. Пишем лог
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Attack,
            AttackerId = attacker,
            TargetId = target.Id,
            AttackerTeam = world.Team[attacker].TeamId,
            TargetTeam = world.Team[target.Id].TeamId,
            ActionType = "BasicAttack",
            DamageType = DamageType.Physical,
            Damage = damage,
            TargetRemainingHp = hpLeft,
            AttackerEnergy = world.GetEnergy(attacker)
        });
    }
}
