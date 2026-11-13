// Прибавка предмета к характеристикам. Складывается поверх прокачки и уровня боя (EntityFactory)
public record ItemStats(int MaxHp = 0, int Attack = 0, int Speed = 0, int MaxEnergy = 0, int StartEnergy = 0, int EnergyPerAttack = 0)
{
    public static ItemStats operator +(ItemStats a, ItemStats b) => new(
        a.MaxHp + b.MaxHp,
        a.Attack + b.Attack,
        a.Speed + b.Speed,
        a.MaxEnergy + b.MaxEnergy,
        a.StartEnergy + b.StartEnergy,
        a.EnergyPerAttack + b.EnergyPerAttack);
}

// Что предмет меняет в спелле. Ключ — имя спелла: действует на все его уровни
public class SpellModifier
{
    public int ExtraTargets { get; set; }
    public int DamagePercent { get; set; }
    // Все эффекты предметов на этот спелл в порядке надевания: после каста они срабатывают в этом же порядке
    public List<SpellBoundEffect> Effects { get; } = new();
    // Сколько раз юнит скастовал этот спелл в бою (для «каждый N-й каст»). GearComponent свой у каждого юнита в бою
    public int CastCount { get; set; }
}

// Аура, которую даёт предмет герою без своей ауры. Power — процент, как у AuraBook
public record AuraGrant(string Effect, AuraTarget Target, int Power, int Duration = 1, int Interval = 1);

// Всё, что надетые предметы дают юниту. Собирается один раз при создании юнита
public class GearComponent
{
    public ItemStats Stats { get; set; } = new();
    public Dictionary<string, SpellModifier> Spells { get; } = new();
    public Dictionary<HeroPassive, PassiveValues> PassiveBonuses { get; } = new();

    public int AuraPowerPercent { get; set; }
    public int AuraDurationBonus { get; set; }
    public int AuraIntervalReduction { get; set; }
    public AuraGrant? AuraGrant { get; set; }

    // Аура без предметов: от неё считается вклад предметов в боевую мощь
    public AuraComponent? BaseAura { get; set; }

    public SpellModifier Spell(string name)
    {
        if (!Spells.TryGetValue(name, out var modifier))
            Spells[name] = modifier = new SpellModifier();
        return modifier;
    }

    public void AddPassive(HeroPassive passive, PassiveValues bonus) =>
        PassiveBonuses[passive] = PassiveBonuses.GetValueOrDefault(passive) + bonus;
}

// Эффект предмета для фронта: текст строится по Kind (equipment.effects.<Kind>), поля зависят от вида
public class ItemEffectInfo
{
    public string Kind { get; set; } = string.Empty;
    public int Value { get; set; }
    public string? Spell { get; set; }
    public string? ProcSpell { get; set; }
    public int? ProcLevel { get; set; }
    // Шанс срабатывания на цель, у SpellProc; 100 — всегда
    public int? Chance { get; set; }
    // Статус, от которого зависит эффект (детонация, распространение, условие срабатывания)
    public string? Status { get; set; }
    // Во что статус превращается (UpgradeStatus)
    public string? ToStatus { get; set; }
    // Второе число эффекта: сколько повторов, целей, стаков, каждый какой каст
    public int? Count { get; set; }
    // Порог HP в процентах
    public int? Threshold { get; set; }
    // Спеллы, из которых срабатывание выбирает случайный
    public List<string>? ProcSpells { get; set; }
    public HeroPassive? Passive { get; set; }
    public string? Aura { get; set; }
    public AuraTarget? AuraTarget { get; set; }
}

// Один эффект предмета. Чтобы добавить новый вид: запись здесь, чтение значения в бою и текст equipment.effects.<Kind> на фронте.
// Эффект, которому нечего усиливать (нет такого спелла, ауры или пассивки), просто ничего не делает
public abstract record ItemEffect
{
    public abstract void Apply(GearComponent gear);
    public abstract ItemEffectInfo Info { get; }
}

// Эффект предмета на конкретный спелл (по имени, на все уровни). Кладётся в SpellModifier.Effects и может:
// поменять числа спелла (Modify), сработать после каста (AfterCast) или отреагировать на «свои» статусы
// в бою — тик, окончание, смерть носителя (OnStatusTick, OnStatusExpired, OnDeath; см. ItemReactions).
// Estimate — сколько эффект в среднем добавляет к одному касту, для боевой мощи (CombatPower)
public abstract record SpellBoundEffect(string Spell) : ItemEffect
{
    public override void Apply(GearComponent gear)
    {
        var modifier = gear.Spell(Spell);
        modifier.Effects.Add(this);
        Modify(modifier);
    }

    protected virtual void Modify(SpellModifier modifier)
    {
    }

    public virtual void AfterCast(CastScope cast)
    {
    }

    // ownerId — носитель предмета; статус на targetId наложен им хотя бы частично
    public virtual void OnStatusTick(World world, CombatContext context, int ownerId, int targetId, StatusEffectBase status)
    {
    }

    public virtual void OnStatusExpired(World world, CombatContext context, int ownerId, int targetId, StatusEffectBase status)
    {
    }

    // Умер юнит deadId; statuses — что висело на нём в момент смерти
    public virtual void OnDeath(World world, CombatContext context, int ownerId, int deadId, IReadOnlyList<StatusEffectBase> statuses)
    {
    }

    public virtual SpellEstimate Estimate(CastEstimate cast) => default;
}

// Средний каст для оценки эффектов предмета: спелл, атака кастера, его оценка без предмета, урон по одной цели и число целей
public readonly record struct CastEstimate(ISpell Spell, double Attack, SpellEstimate Base, double PerTarget, double Targets);

// +N целей спелла: добираются следующие живые враги по линии
public record SpellTargetsEffect(string Spell, int Extra) : SpellBoundEffect(Spell)
{
    protected override void Modify(SpellModifier modifier) => modifier.ExtraTargets += Extra;
    public override ItemEffectInfo Info => new() { Kind = "SpellTargets", Spell = Spell, Value = Extra };
}

// +% к урону спелла
public record SpellDamageEffect(string Spell, int Percent) : SpellBoundEffect(Spell)
{
    protected override void Modify(SpellModifier modifier) => modifier.DamagePercent += Percent;
    public override ItemEffectInfo Info => new() { Kind = "SpellDamage", Spell = Spell, Value = Percent };
}

// Когда срабатывает SpellProcEffect
public enum ProcCondition
{
    // По каждой живой цели каста
    Always,
    // По целям, на которые каст наложил статус Status (оглушил, опутал)
    StatusApplied,
    // По всем врагам, если до каста у кого-то из союзников было меньше Threshold % HP
    AllyBelowHp
}

// После каста срабатывает другой спелл с уроном DamagePercent от атаки кастера (со своими статусами).
// Цели — по условию (ProcCondition), шанс Chance бросается отдельно на каждую; все, кому повезло, — один каст
public record SpellProcEffect(string Spell, int ProcSpellId, int DamagePercent, int Chance = 100,
    ProcCondition Condition = ProcCondition.Always, string? Status = null, int Threshold = 0) : SpellBoundEffect(Spell)
{
    public override void AfterCast(CastScope cast)
    {
        if (SpellRegistry.All.GetValueOrDefault(ProcSpellId) is not BaseSpell proc) return;
        var world = cast.World;

        IEnumerable<int> candidates = Condition switch
        {
            ProcCondition.StatusApplied => cast.Targets.Where(t => Status != null && cast.Applied(t, Status)),
            ProcCondition.AllyBelowHp => cast.LowestAllyHpPercent < Threshold
                ? world.GetAliveEnemies(cast.CasterId).Select(e => e.Id)
                : [],
            _ => cast.Targets
        };
        var targets = candidates
            .Where(world.IsAlive)
            .Where(_ => Chance >= 100 || cast.Context.Random.Next(100) < Chance)
            .ToList();
        if (targets.Count > 0)
            proc.CastTriggered(world, cast.CasterId, cast.Context, targets, DamagePercent);
    }

    // Сколько целей в среднем ловит срабатывание за каст
    private double ExpectedTargets(CastEstimate cast) => Condition switch
    {
        // Оглушение и опутывание — StunnedTurns оценки спелла: столько врагов в среднем пропускает ход
        ProcCondition.StatusApplied => cast.Base.StunnedTurns,
        // Союзник ниже порога бывает примерно в трети кастов, бьём всех врагов
        ProcCondition.AllyBelowHp => CombatPower.ReferenceTargets / 3.0,
        _ => cast.Targets
    };

    public override SpellEstimate Estimate(CastEstimate cast) =>
        new(Damage: ExpectedTargets(cast) * cast.Attack * DamagePercent / 100.0 * Math.Min(100, Chance) / 100.0);

    public override ItemEffectInfo Info
    {
        get
        {
            var proc = SpellRegistry.All[ProcSpellId];
            return new ItemEffectInfo
            {
                Kind = Condition switch
                {
                    ProcCondition.StatusApplied => "SpellProcOnStatus",
                    ProcCondition.AllyBelowHp => "SpellProcAllyLow",
                    _ => "SpellProc"
                },
                Spell = Spell,
                ProcSpell = proc.Name,
                ProcLevel = proc.Level,
                Value = DamagePercent,
                Chance = Chance,
                Status = Status,
                Threshold = Condition == ProcCondition.AllyBelowHp ? Threshold : null
            };
        }
    }
}

public record AuraPowerEffect(int Percent) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.AuraPowerPercent += Percent;
    public override ItemEffectInfo Info => new() { Kind = "AuraPower", Value = Percent };
}

public record AuraDurationEffect(int Turns) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.AuraDurationBonus += Turns;
    public override ItemEffectInfo Info => new() { Kind = "AuraDuration", Value = Turns };
}

// Аура срабатывает чаще: интервал меньше на Reduction, но не меньше 1
public record AuraIntervalEffect(int Reduction) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.AuraIntervalReduction += Reduction;
    public override ItemEffectInfo Info => new() { Kind = "AuraInterval", Value = Reduction };
}

// Даёт ауру герою без своей ауры. Своя аура героя важнее
public record AuraGrantEffect(string Effect, AuraTarget Target, int Power, int Duration = 1) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.AuraGrant ??= new AuraGrant(Effect, Target, Power, Duration);
    public override ItemEffectInfo Info => new() { Kind = "AuraGrant", Aura = Effect, AuraTarget = Target, Value = Power };
}

public record PassivePercentEffect(HeroPassive Passive, int Percent) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.AddPassive(Passive, new PassiveValues(Percent: Percent));
    public override ItemEffectInfo Info => new() { Kind = "PassivePercent", Passive = Passive, Value = Percent };
}

public record PassiveTurnsEffect(HeroPassive Passive, int Turns) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.AddPassive(Passive, new PassiveValues(Turns: Turns));
    public override ItemEffectInfo Info => new() { Kind = "PassiveTurns", Passive = Passive, Value = Turns };
}

public record PassiveTargetsEffect(HeroPassive Passive, int Targets) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.AddPassive(Passive, new PassiveValues(Targets: Targets));
    public override ItemEffectInfo Info => new() { Kind = "PassiveTargets", Passive = Passive, Value = Targets };
}
