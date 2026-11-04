// Пассивные способности героев. Хранятся строкой — значения можно переставлять, но не переименовывать без миграции
public enum HeroPassive
{
    // Атакующий сильнее героя (атака выше) — урон от его ударов и спеллов меньше на Percent
    SurvivalOfTheWeakest,
    // Каждый удар базовой атакой поджигает: Percent от атаки за ход, Turns ходов, Targets целей от ударенной и дальше по линии
    SharpTongue,
    // Базовая атака бьёт второй раз силой Percent: ударенную цель и ещё Targets - 1 следующих
    DoubleStrike
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

    public static PassiveValues Values(HeroPassive passive, int level)
    {
        level = Math.Clamp(level, MinLevel, MaxLevel);
        return passive switch
        {
            HeroPassive.SurvivalOfTheWeakest => new PassiveValues(Percent: 5 * level),
            HeroPassive.SharpTongue => new PassiveValues(Percent: 10 * level, Turns: 1, Targets: 1),
            HeroPassive.DoubleStrike => new PassiveValues(Percent: 25 + 25 * level, Targets: 1),
            _ => default
        };
    }
}

// Пассивки юнита в бою. Base — без колец: от него считается вклад предметов в боевую мощь
public class PassiveComponent
{
    public Dictionary<HeroPassive, PassiveValues> Base { get; set; } = new();
    public Dictionary<HeroPassive, PassiveValues> Active { get; set; } = new();
}
