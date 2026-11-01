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
    public List<SpellProc> Procs { get; } = new();
}

// После каста по всем его живым целям срабатывает другой спелл: урон — DamagePercent от атаки кастера
public record SpellProc(int SpellId, int DamagePercent);

// Аура, которую даёт предмет герою без своей ауры
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

// +N целей спелла: добираются следующие живые враги по линии
public record SpellTargetsEffect(string Spell, int Extra) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.Spell(Spell).ExtraTargets += Extra;
    public override ItemEffectInfo Info => new() { Kind = "SpellTargets", Spell = Spell, Value = Extra };
}

// +% к урону спелла
public record SpellDamageEffect(string Spell, int Percent) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.Spell(Spell).DamagePercent += Percent;
    public override ItemEffectInfo Info => new() { Kind = "SpellDamage", Spell = Spell, Value = Percent };
}

// После каста спелла по каждой его живой цели срабатывает другой спелл
public record SpellProcEffect(string Spell, int ProcSpellId, int DamagePercent) : ItemEffect
{
    public override void Apply(GearComponent gear) => gear.Spell(Spell).Procs.Add(new SpellProc(ProcSpellId, DamagePercent));

    public override ItemEffectInfo Info
    {
        get
        {
            var proc = SpellRegistry.All[ProcSpellId];
            return new ItemEffectInfo { Kind = "SpellProc", Spell = Spell, ProcSpell = proc.Name, ProcLevel = proc.Level, Value = DamagePercent };
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
