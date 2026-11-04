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

        // 1. Начисляем энергию за удар (рунный резонанс добавляет свою); мороз урезает прирост на свой процент
        if (world.Energy.ContainsKey(attacker))
        {
            var energy = world.Energy[attacker];
            int resonance = world.GetPassive(attacker, HeroPassive.RuneResonance)?.Percent ?? 0;
            int gain = (energy.GainPerAttack + resonance) * (100 - FrostSlowPercent(world, attacker)) / 100;
            energy.Current = Math.Min(energy.Max, energy.Current + gain);
        }

        state.ActionCompleted = true;

        // 2. Бьём
        int attack = world.Attack[attacker].Damage;
        Strike(world, context, attacker, target.Id, attack);

        // 3. Раскол: следующий по линии враг получает долю удара отдельным действием
        if (world.GetPassive(attacker, HeroPassive.Cleave) is { } cleave && world.IsAlive(attacker))
        {
            var behind = NextInLine(world, attacker, target.Id).Take(Math.Max(1, cleave.Targets)).ToList();
            foreach (int targetId in behind)
            {
                context.NextAction();
                Strike(world, context, attacker, targetId, attack * cleave.Percent / 100, nameof(HeroPassive.Cleave));
            }
        }

        // 4. Двойной удар: с шансом второй замах в полную силу. Энергия за него не начисляется
        if (world.GetPassive(attacker, HeroPassive.DoubleStrike) is not { } doubleStrike) return;
        if (!world.IsAlive(attacker) || context.Random.Next(100) >= doubleStrike.Percent) return;

        foreach (int targetId in world.GetAliveEnemies(attacker).Take(Math.Max(1, doubleStrike.Targets)).Select(e => e.Id).ToList())
        {
            context.NextAction();
            Strike(world, context, attacker, targetId, world.Attack[attacker].Damage, nameof(HeroPassive.DoubleStrike));
        }
    }

    // Живые враги за ударенной целью по линии; если она погибла — первые живые
    private static IEnumerable<int> NextInLine(World world, int attacker, int struckId)
    {
        var enemies = world.GetAliveEnemies(attacker).Select(e => e.Id).ToList();
        int index = enemies.IndexOf(struckId);
        return index >= 0 ? enemies.Skip(index + 1) : enemies;
    }

    private static void Strike(World world, CombatContext context, int attacker, int targetId, int amount, string skillName = "")
    {
        // Критический удар: двойной урон с шансом
        bool critical = world.GetPassive(attacker, HeroPassive.CriticalStrike) is { } crit && context.Random.Next(100) < crit.Percent;
        if (critical) amount *= 2;

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
            IsCritical = critical,
            TargetRemainingHp = hpLeft,
            AttackerEnergy = world.GetEnergy(attacker)
        });

        PassiveReactions.Vampirism(world, context, attacker, damage);
        ApplySharpTongue(world, context, attacker, targetId);
        Thorns(world, context, attacker, targetId);
    }

    // Шипы: ударившего героя базовой атакой ранит в ответ — доля атаки героя
    private static void Thorns(World world, CombatContext context, int attacker, int struckId)
    {
        if (world.GetPassive(struckId, HeroPassive.Thorns) is not { } thorns || !world.IsAlive(attacker)) return;

        int damage = world.ScaleDamage(Math.Max(1, world.Attack[struckId].Damage * thorns.Percent / 100));
        int hpLeft = world.ApplyDamage(attacker, damage);
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Damage,
            Phase = "Passive",
            AttackerId = struckId,
            TargetId = attacker,
            AttackerTeam = world.Team[struckId].TeamId,
            TargetTeam = world.Team[attacker].TeamId,
            ActionType = nameof(HeroPassive.Thorns),
            SkillName = nameof(HeroPassive.Thorns),
            DamageType = DamageType.Physical,
            Damage = damage,
            TargetRemainingHp = hpLeft
        });
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

// Срабатывания пассивок, общие для ударов и спеллов
public static class PassiveReactions
{
    // Вампиризм: герой лечится на долю нанесённого урона
    public static void Vampirism(World world, CombatContext context, int attacker, int damage)
    {
        if (damage <= 0 || world.GetPassive(attacker, HeroPassive.Vampirism) is not { } vampirism || !world.IsAlive(attacker)) return;

        int healed = world.ApplyHeal(attacker, damage * vampirism.Percent / 100);
        if (healed <= 0) return;

        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Heal,
            Phase = "Passive",
            AttackerId = attacker,
            TargetId = attacker,
            AttackerTeam = world.Team[attacker].TeamId,
            TargetTeam = world.Team[attacker].TeamId,
            ActionType = nameof(HeroPassive.Vampirism),
            SkillName = nameof(HeroPassive.Vampirism),
            Heal = healed,
            TargetRemainingHp = world.Health[attacker].Current
        });
    }
}
