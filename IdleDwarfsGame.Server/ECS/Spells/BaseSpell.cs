using System;
using System.Linq;
using System.Collections.Generic;

public abstract class BaseSpell : ISpell
{
    public int Id { get; }
    public string Name { get; }
    public int Level { get; }
    public int EnergyCost { get; }
    public virtual DamageType DamageType => DamageType.Magical;

    protected BaseSpell(int id, string name, int level, int energyCost)
    {
        Id = id;
        Name = name;
        Level = level;
        EnergyCost = energyCost;
    }

    public virtual void Execute(World world, int casterId, CombatContext context)
    {
        var targets = GetTargets(world, casterId, context.Random)
            .Where(id => id != -1 && world.IsAlive(id))
            .ToList();
        if (targets.Count == 0) return;

        // 1. Лог каста — фронт проигрывает анимацию каста и снаряды ко всем целям
        LogCast(world, casterId, context, targets);

        foreach (var targetId in targets)
        {
            // 2. Считаем и наносим урон
            int damage = world.ScaleDamage(GetDamage(world, casterId, targetId));
            int hpLeft = world.ApplyDamage(targetId, damage);

            // 3. Пишем лог попадания
            context.AddLog(new CombatLogEntry
            {
                EventType = CombatEventType.Damage,
                AttackerId = casterId,
                TargetId = targetId,
                AttackerTeam = world.Team[casterId].TeamId,
                TargetTeam = world.Team[targetId].TeamId,
                ActionType = $"{Name} (Lvl {Level})",
                SkillId = Id,
                SkillName = Name,
                DamageType = DamageType,
                Damage = damage,
                TargetRemainingHp = hpLeft
            });

            // 4. Накладываем дополнительные эффекты (огонь, оглушение и т.д.)
            if (world.IsAlive(targetId))
                ApplyEffects(world, casterId, targetId, context);

            // 5. То, что зависит от нанесённого урона и срабатывает даже по убитой цели (вампиризм)
            AfterHit(world, casterId, targetId, damage, context);
        }
    }

    // Лечение с логом: фронт показывает его в той же группе, что и каст. Возвращает восстановленное HP
    protected int HealTarget(World world, int casterId, int targetId, int amount, CombatContext context)
    {
        int healed = world.ApplyHeal(targetId, amount);
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.Heal,
            AttackerId = casterId,
            TargetId = targetId,
            AttackerTeam = world.Team[casterId].TeamId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = $"{Name} (Lvl {Level})",
            SkillId = Id,
            SkillName = Name,
            Heal = healed,
            TargetRemainingHp = world.Health[targetId].Current
        });
        return healed;
    }

    protected void LogCast(World world, int casterId, CombatContext context, List<int> targets)
    {
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.SkillCast,
            AttackerId = casterId,
            AttackerTeam = world.Team[casterId].TeamId,
            TargetId = targets[0],
            TargetTeam = world.Team[targets[0]].TeamId,
            ActionType = $"{Name} (Lvl {Level})",
            SkillId = Id,
            SkillName = Name,
            SkillLevel = Level,
            DamageType = DamageType,
            AttackerEnergy = world.GetEnergy(casterId),
            Text = $"{Name} → {targets.Count} target(s)"
        });
    }

    // Каждый спелл решает сам, сколько целей бить. Случайность берётся из боя — так бой с одним сидом воспроизводим
    protected abstract List<int> GetTargets(World world, int casterId, Random random);

    // Свой урон для каждого спелла
    protected abstract int GetDamage(World world, int casterId, int targetId);

    // Виртуальный метод для эффектов. По умолчанию ничего не делает.
    protected virtual void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
    }

    protected virtual void AfterHit(World world, int casterId, int targetId, int damage, CombatContext context)
    {
    }

    public abstract SpellEstimate Estimate(int casterAttack, int enemyCount);

    // Вспомогательные методы для поиска целей
    protected List<Entity> GetAliveEnemies(World world, int casterId) => world.GetAliveEnemies(casterId);

    protected List<int> GetRandomTargets(List<Entity> availableTargets, int count, Random random)
    {
        return availableTargets.OrderBy(_ => random.Next()).Take(count).Select(e => e.Id).ToList();
    }

    // Первые count врагов от передней линии; null — все враги
    protected List<int> GetFrontTargets(World world, int casterId, int? count) =>
        GetAliveEnemies(world, casterId).Take(count ?? int.MaxValue).Select(e => e.Id).ToList();

    protected int ScaledDamage(World world, int casterId, double multiplier) =>
        (int)(world.Attack[casterId].Damage * multiplier);
}
