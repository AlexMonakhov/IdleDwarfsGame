// Пассивные способности героев. Хранятся строкой — значения можно переставлять, но не переименовывать без миграции.
// Все числа — проценты от атаки героя, если не сказано иное. Уровни 1 / 2 / 3 — PassiveBook.Values
public enum HeroPassive
{
    // Атакующий сильнее героя (атака выше) — урон от его ударов и спеллов меньше на Percent
    SurvivalOfTheWeakest,
    // Каждый удар базовой атакой поджигает: Percent от атаки за ход, Turns ходов, Targets целей от ударенной и дальше по линии
    SharpTongue,
    // С шансом Percent базовая атака бьёт второй раз в полную силу: ударенную цель и ещё Targets - 1 следующих
    DoubleStrike,
    // С шансом Percent базовая атака наносит двойной урон
    CriticalStrike,
    // Удары и спеллы по врагам ниже ExecuteThreshold% здоровья сильнее на Percent
    Executioner,
    // Базовая атака задевает следующего по линии врага (Targets) с силой Percent
    Cleave,
    // Спеллы героя наносят на Percent больше урона
    Sorcerer,
    // Каждый удар даёт на Percent единиц энергии больше
    RuneResonance,
    // Удары и спеллы лечат героя на Percent нанесённого урона
    Vampirism,
    // Урон ударов и спеллов по герою меньше на Percent
    StoneSkin,
    // Ударивший героя базовой атакой получает Percent атаки героя в ответ
    Thorns,
    // Раз за бой смертельный удар оставляет героя с 1 HP, и он лечится на Percent своего максимального HP
    LastStand,
    // Каждый погибший союзник даёт +Percent к атаке героя до конца боя
    Avenger,
    // Горение, яд, проклятие, шок и кровотечение от героя сильнее на Percent и держатся на Turns ходов дольше
    ElementMastery
}

// Значения пассивки на конкретном юните. Кольца прибавляют к ним свои (PassivePercent, PassiveTurns, PassiveTargets)
public readonly record struct PassiveValues(int Percent = 0, int Turns = 0, int Targets = 0)
{
    public static PassiveValues operator +(PassiveValues a, PassiveValues b) =>
        new(a.Percent + b.Percent, a.Turns + b.Turns, a.Targets + b.Targets);
}

public static class PassiveBook
{
    public const int MinLevel = 1;
    public const int MaxLevel = 3;

    private static readonly HeroPassive[] All = Enum.GetValues<HeroPassive>();

    // Пассивка открывается с 3-го ранга: на нём её уровень 1–2, с 4-го — 1–3; 0 — пассивки нет.
    // Выше этого уровень поднимают только слиянием извлечённых пассивок (AbilityService)
    public static int MaxLevelFor(int rank) => rank switch
    {
        <= 2 => 0,
        3 => 2,
        _ => MaxLevel
    };

    public static HeroPassive Random(Random random) => All[random.Next(All.Length)];

    // Случайный уровень пассивки дворфа этого ранга; 0 — пассивка ему не положена
    public static int RandomLevel(int rank, Random random)
    {
        int max = MaxLevelFor(rank);
        return max == 0 ? 0 : random.Next(MinLevel, max + 1);
    }

    // Порог «добивания» для Executioner, в процентах здоровья цели
    public const int ExecuteThreshold = 30;

    public static PassiveValues Values(HeroPassive passive, int level)
    {
        level = Math.Clamp(level, MinLevel, MaxLevel);
        int ByLevel(int first, int second, int third) => level switch { 1 => first, 2 => second, _ => third };
        return passive switch
        {
            HeroPassive.SurvivalOfTheWeakest => new PassiveValues(Percent: 5 * level),
            HeroPassive.SharpTongue => new PassiveValues(Percent: ByLevel(10, 15, 20), Turns: 2, Targets: 1),
            HeroPassive.DoubleStrike => new PassiveValues(Percent: ByLevel(20, 30, 40), Targets: 1),
            HeroPassive.CriticalStrike => new PassiveValues(Percent: ByLevel(15, 20, 25)),
            HeroPassive.Executioner => new PassiveValues(Percent: ByLevel(30, 50, 80)),
            HeroPassive.Cleave => new PassiveValues(Percent: ByLevel(30, 45, 60), Targets: 1),
            HeroPassive.Sorcerer => new PassiveValues(Percent: ByLevel(15, 30, 50)),
            HeroPassive.RuneResonance => new PassiveValues(Percent: ByLevel(5, 8, 12)),
            HeroPassive.Vampirism => new PassiveValues(Percent: ByLevel(10, 15, 20)),
            HeroPassive.StoneSkin => new PassiveValues(Percent: ByLevel(8, 12, 16)),
            HeroPassive.Thorns => new PassiveValues(Percent: ByLevel(20, 35, 50)),
            HeroPassive.LastStand => new PassiveValues(Percent: ByLevel(20, 30, 40)),
            HeroPassive.Avenger => new PassiveValues(Percent: ByLevel(20, 30, 40)),
            HeroPassive.ElementMastery => new PassiveValues(Percent: ByLevel(20, 30, 40), Turns: 1),
            _ => default
        };
    }
}

// Пассивки юнита в бою. Base — без колец: от него считается вклад предметов в боевую мощь
public class PassiveComponent
{
    public Dictionary<HeroPassive, PassiveValues> Base { get; set; } = new();
    public Dictionary<HeroPassive, PassiveValues> Active { get; set; } = new();

    // LastStand уже спас героя в этом бою
    public bool LastStandUsed { get; set; }
}
