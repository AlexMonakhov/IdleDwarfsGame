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

    // Каст: сам спелл (Perform), затем эффекты предметов на этот спелл (SpellBoundEffect.AfterCast).
    // Что произошло за каст, собирается в CastScope — по нему предметы решают, срабатывать ли
    public void Execute(World world, int casterId, CombatContext context)
    {
        // Предметы могут добавить целей, урона и срабатываний (SpellModifier); без них modifier == null
        SpellModifier? modifier = null;
        world.Gear.GetValueOrDefault(casterId)?.Spells.TryGetValue(Name, out modifier);

        var scope = new CastScope(world, casterId, context, this, modifier);
        var outer = context.CurrentCast;
        context.CurrentCast = scope;
        int logsBefore = context.Logs.Count;
        try
        {
            Perform(scope);
            scope.Open = false;

            bool cast = context.Logs.Skip(logsBefore).Any(l => l.EventType == CombatEventType.SkillCast && l.AttackerId == casterId);
            if (!cast || modifier == null) return;

            modifier.CastCount++;
            foreach (var effect in modifier.Effects.ToList())
                effect.AfterCast(scope);
        }
        finally
        {
            context.CurrentCast = outer;
        }
    }

    // Само действие спелла. Спеллы со своей логикой (лечение) переопределяют его
    protected virtual void Perform(CastScope scope)
    {
        var (world, casterId, context) = (scope.World, scope.CasterId, scope.Context);
        var targets = GetTargets(world, casterId, context.Random)
            .Where(id => id != -1 && world.IsAlive(id))
            .ToList();
        if (targets.Count == 0) return;

        if (scope.Modifier is { ExtraTargets: > 0 } modifier)
            targets.AddRange(GetAliveEnemies(world, casterId).Select(e => e.Id).Except(targets).Take(modifier.ExtraTargets).ToList());

        // 1. Лог каста — фронт проигрывает анимацию каста и снаряды ко всем целям
        LogCast(world, casterId, context, targets);
        OnCast(world, casterId, context);

        foreach (var targetId in targets)
            Hit(world, casterId, targetId, CastDamage(world, casterId, targetId, scope.Modifier), context);
    }

    // Урон каста по цели: свой урон спелла, бонус предметов и пассивка «Чародей»
    private int CastDamage(World world, int casterId, int targetId, SpellModifier? modifier)
    {
        int damage = GetDamage(world, casterId, targetId);
        if (modifier != null)
            damage = damage * (100 + modifier.DamagePercent) / 100;
        int sorcerer = world.GetPassive(casterId, HeroPassive.Sorcerer)?.Percent ?? 0;
        if (sorcerer > 0)
            damage = damage * (100 + sorcerer) / 100;
        return damage;
    }

    // Спелл, вызванный предметом: своё действие с логом каста, урон задан процентом от атаки кастера.
    // Цели уже выбраны, энергия не тратится, новые срабатывания он не вызывает
    public void CastTriggered(World world, int casterId, CombatContext context, List<int> targets, int damagePercent)
    {
        int damage = world.Attack[casterId].Damage * damagePercent / 100;
        TriggerHits(world, casterId, context, targets.Select(t => (t, damage)).ToList(), applyEffects: true);
    }

    // Отдельное действие от предмета: лог каста этого спелла и удары с заданным уроном по каждой цели.
    // applyEffects — накладывать ли статусы спелла; afterHit — что сделать с целью сразу после удара (снять статус и т.п.)
    public CastScope TriggerHits(World world, int casterId, CombatContext context, IReadOnlyList<(int Target, int Damage)> hits,
        bool applyEffects, Action<int>? afterHit = null)
    {
        var scope = new CastScope(world, casterId, context, this, null);
        var alive = hits.Where(h => world.IsAlive(h.Target)).ToList();
        if (alive.Count == 0) return scope;

        RunTriggered(scope, () =>
        {
            LogCast(world, casterId, context, alive.Select(h => h.Target).Distinct().ToList());
            foreach (var (target, damage) in alive)
            {
                Hit(world, casterId, target, damage, context, applyEffects);
                afterHit?.Invoke(target);
            }
        });
        return scope;
    }

    // Отдельное действие от предмета: лечение союзников с заданной силой (визуал — этот спелл)
    public void TriggerHeals(World world, int casterId, CombatContext context, IReadOnlyList<(int Target, int Amount)> heals)
    {
        var alive = heals.Where(h => world.IsAlive(h.Target) && h.Amount > 0).ToList();
        if (alive.Count == 0) return;

        RunTriggered(new CastScope(world, casterId, context, this, null), () =>
        {
            LogCast(world, casterId, context, alive.Select(h => h.Target).Distinct().ToList());
            foreach (var (target, amount) in alive)
                HealTarget(world, casterId, target, amount, context);
        });
    }

    // Отдельное действие от предмета без урона: снаряд спелла летит в цели и накладывает на каждую свой статус
    public void TriggerStatuses(World world, int casterId, CombatContext context, IReadOnlyList<(int Target, IStatusEffect Effect)> statuses)
    {
        var alive = statuses.Where(s => world.IsAlive(s.Target)).ToList();
        if (alive.Count == 0) return;

        RunTriggered(new CastScope(world, casterId, context, this, null), () =>
        {
            LogCast(world, casterId, context, alive.Select(s => s.Target).Distinct().ToList());
            foreach (var (target, effect) in alive)
                effect.ApplyEffect(world, target, context, casterId);
        });
    }

    // Повтор этого же спелла от предмета: свой урон × percent и свои статусы, без энергии и без новых срабатываний.
    // targets == null — цели выбираются заново, как у обычного каста
    public CastScope Echo(World world, int casterId, CombatContext context, List<int>? targets, int percent)
    {
        var scope = new CastScope(world, casterId, context, this, null);
        var chosen = (targets ?? GetTargets(world, casterId, context.Random)).Where(id => id != -1 && world.IsAlive(id)).ToList();
        if (chosen.Count == 0) return scope;

        var modifier = world.Gear.GetValueOrDefault(casterId)?.Spells.GetValueOrDefault(Name);
        RunTriggered(scope, () =>
        {
            LogCast(world, casterId, context, chosen);
            foreach (var target in chosen)
                Hit(world, casterId, target, CastDamage(world, casterId, target, modifier) * percent / 100, context);
        });
        return scope;
    }

    // Удар этим спеллом вне его обычного каста (взрыв стаков и т.п.): та же запись урона, статусы не накладываются
    public void ExtraHit(World world, int casterId, int targetId, int damage, CombatContext context) =>
        Hit(world, casterId, targetId, damage, context, applyEffects: false);

    // Действие от предмета — отдельное действие в логе со своим кастом; в его CastScope пишутся попадания
    private static void RunTriggered(CastScope scope, Action body)
    {
        var context = scope.Context;
        var outer = context.CurrentCast;
        context.NextAction();
        context.CurrentCast = scope;
        try
        {
            body();
        }
        finally
        {
            scope.Open = false;
            context.CurrentCast = outer;
        }
    }

    // Каст, в который сейчас пишет этот спелл: открытый CastScope этого же спелла
    private CastScope? Recording(CombatContext context) =>
        context.CurrentCast is { Open: true } scope && scope.Spell == this ? scope : null;

    private void Hit(World world, int casterId, int targetId, int amount, CombatContext context, bool applyEffects = true)
    {
        // Метка на цели (MarkEffect) усиливает урон любых заклинаний
        if (MarkEffect.Find(world, targetId) is { } mark)
            amount = amount * (100 + mark.Power) / 100;

        // 2. Считаем и наносим урон
        int damage = world.ResolveHit(casterId, targetId, amount);
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

        var scope = Recording(context);
        if (scope != null)
        {
            if (!scope.Targets.Contains(targetId)) scope.Targets.Add(targetId);
            if (hpLeft == 0) scope.Killed.Add(targetId);
        }

        // 4. Накладываем дополнительные эффекты (огонь, оглушение и т.д.)
        if (applyEffects && world.IsAlive(targetId))
        {
            int logsBefore = context.Logs.Count;
            ApplyEffects(world, casterId, targetId, context);
            if (scope != null)
            {
                foreach (var log in context.Logs.Skip(logsBefore).Where(l => l.EventType == CombatEventType.StatusApplied && l.TargetId == targetId))
                    scope.RecordApplied(targetId, log.ActionType);
            }
        }

        // 5. То, что зависит от нанесённого урона и срабатывает даже по убитой цели (вампиризм)
        AfterHit(world, casterId, targetId, damage, context);
        PassiveReactions.Vampirism(world, context, casterId, damage);
    }

    // Лечение с логом: фронт показывает его в той же группе, что и каст. Возвращает восстановленное HP
    public int HealTarget(World world, int casterId, int targetId, int amount, CombatContext context)
    {
        int healed = world.ApplyHeal(targetId, amount);
        if (Recording(context) is { } scope)
            scope.Overheal += Math.Max(0, amount - healed);

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

    // Пишет в открытый каст этого спелла (если он идёт): для спеллов с платой здоровьем и снятием проклятий
    protected void RecordInCast(CombatContext context, Action<CastScope> record)
    {
        if (Recording(context) is { } scope) record(scope);
    }

    // Каждый спелл решает сам, сколько целей бить. Случайность берётся из боя — так бой с одним сидом воспроизводим
    protected abstract List<int> GetTargets(World world, int casterId, Random random);

    // Свой урон для каждого спелла
    protected abstract int GetDamage(World world, int casterId, int targetId);

    // Сразу после лога каста, до попаданий (плата кровью у Blood Price). Спелл от предмета его не вызывает
    protected virtual void OnCast(World world, int casterId, CombatContext context)
    {
    }

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
