using System;
using System.Linq;
using System.Collections.Generic;

public abstract class BaseSpell : ISpell
{
    public int Id { get; }
    public string Name { get; }
    public int Level { get; }
    public int EnergyCost { get; }

    protected static Random _random = new();

    protected BaseSpell(int id, string name, int level, int energyCost)
    {
        Id = id;
        Name = name;
        Level = level;
        EnergyCost = energyCost;
    }

    public virtual void Execute(World world, int casterId, CombatContext context)
    {
        var targets = GetTargets(world, casterId);
        if (targets.Count == 0) return;

        foreach (var targetId in targets)
        {
            if (targetId == -1 || world.Health[targetId].Current <= 0) continue;

            // 1. Считаем и наносим урон
            int damage = GetDamage(world, casterId, targetId);
            world.Health[targetId].Current -= damage;

            // 2. Накладываем дополнительные эффекты (стан, горение и т.д.)
            ApplyEffects(world, casterId, targetId, context);

            // 3. Пишем лог
            context.Logs.Add(new CombatLogEntry
            {
                AttackerId = casterId,
                TargetId = targetId,
                AttackerTeam = world.Team[casterId].TeamId,
                TargetTeam = world.Team[targetId].TeamId,
                ActionType = $"{Name} (Lvl {Level})",
                Damage = damage,
                TargetRemainingHp = Math.Max(0, world.Health[targetId].Current)
            });
        }
    }

    // Теперь каждый спелл САМ решает, сколько целей он бьет
    protected abstract List<int> GetTargets(World world, int casterId);

    // Урон тоже считает сам спелл
    protected abstract int GetDamage(World world, int casterId, int targetId);

    // Виртуальный метод для эффектов. По умолчанию ничего не делает.
    protected virtual void ApplyEffects(World world, int casterId, int targetId, CombatContext context)
    {
    }

    // Вспомогательные методы оставляем в базе для удобства
    protected List<Entity> GetAliveEnemies(World world, int casterId)
    {
        int enemyTeam = world.Team[casterId].TeamId == 1 ? 2 : 1;
        return world.Entities
            .Where(e => world.Team[e.Id].TeamId == enemyTeam && world.Health[e.Id].Current > 0)
            .ToList();
    }

    protected List<int> GetRandomTargets(List<Entity> availableTargets, int count)
    {
        return availableTargets.OrderBy(_ => _random.Next()).Take(count).Select(e => e.Id).ToList();
    }
}
