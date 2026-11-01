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

        // 1. Начисляем энергию за удар; мороз урезает прирост на свой процент
        if (world.Energy.ContainsKey(attacker))
        {
            var energy = world.Energy[attacker];
            int gain = energy.GainPerAttack * (100 - FrostSlowPercent(world, attacker)) / 100;
            energy.Current = Math.Min(energy.Max, energy.Current + gain);
        }

        state.ActionCompleted = true;

        // 2. Бьём
        Strike(world, context, attacker, target.Id, world.Attack[attacker].Damage);

        // 3. Двойной удар: отдельные действия, чтобы фронт показал второй замах. Энергия за него не начисляется
        if (world.GetPassive(attacker, HeroPassive.DoubleStrike) is not { } doubleStrike) return;

        int secondHit = world.Attack[attacker].Damage * doubleStrike.Percent / 100;
        foreach (int targetId in world.GetAliveEnemies(attacker).Take(doubleStrike.Targets).Select(e => e.Id).ToList())
        {
            context.NextAction();
            Strike(world, context, attacker, targetId, secondHit, nameof(HeroPassive.DoubleStrike));
        }
    }

    private static void Strike(World world, CombatContext context, int attacker, int targetId, int amount, string skillName = "")
    {
        int damage = world.ResolveHit(attacker, targetId, amount);
        int hpLeft = world.ApplyDamage(targetId, damage);

        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Attack,
            AttackerId = attacker,
            TargetId = targetId,
            AttackerTeam = world.Team[attacker].TeamId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = "BasicAttack",
            SkillName = skillName,
            DamageType = DamageType.Physical,
            Damage = damage,
            TargetRemainingHp = hpLeft,
            AttackerEnergy = world.GetEnergy(attacker)
        });

        ApplySharpTongue(world, context, attacker, targetId);
    }

    // Острый язык: горение на ударенную цель и, если кольца добавили целей, на следующих за ней по линии
    private static void ApplySharpTongue(World world, CombatContext context, int attacker, int struckId)
    {
        if (world.GetPassive(attacker, HeroPassive.SharpTongue) is not { } tongue) return;

        int burn = Math.Max(1, world.Attack[attacker].Damage * tongue.Percent / 100);
        var enemies = world.GetAliveEnemies(attacker).Select(e => e.Id).ToList();
        var targets = enemies.SkipWhile(id => id != struckId).Take(tongue.Targets).ToList();

        // Ударом цель могла погибнуть — тогда соседи всё равно загораются
        if (targets.Count == 0) targets = enemies.Take(tongue.Targets - 1).ToList();

        foreach (int targetId in targets)
        {
            new BurnEffect(turns: tongue.Turns, damagePerTurn: burn).ApplyEffect(world, targetId, context, attacker);
            // ApplyEffect всегда пишет лог по живой цели. Фаза отличает этот статус от срабатывания ауры того же эффекта
            context.Logs[^1].Phase = "Passive";
        }
    }

    private static int FrostSlowPercent(World world, int entityId) =>
        world.StatusEffects.TryGetValue(entityId, out var statuses)
            ? statuses.ActiveEffects.OfType<FrostEffect>().FirstOrDefault()?.EnergySlowPercent ?? 0
            : 0;
}
