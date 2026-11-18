// Ауры дворфов: у каждой стихии своя, сила растёт с уровнем 1..MaxLevel.
// Дворф 4-го ранга получает ауру своей стихии 1-го уровня, выше её поднимают слиянием извлечённых аур (AbilityService).
// Ключ ауры — название эффекта (StatusEffectFactory): так он хранится и у героя (Hero.AuraEffect), и у извлечённой ауры.
//
// Ауры срабатывают раз в Interval раундов (первый раз — в первом раунде) и кладут одно наложение на каждую цель.
// Обычный интервал — 3 раунда, у каждой стихии свой характер (docs/effect-rules.md, раздел «Ауры»):
//   горение — сильное наложение на 2 хода;          мороз — замедление на 2 хода;
//   шок — один мощный разряд;                        яд — слабый, но долгий (3 / 4 / 5 ходов);
//   выжигание энергии — большой разовый ожог;        реген — лечение на 3 хода, без перерывов.
// Кровотечение — исключение: срабатывает каждый раунд, потому что одно наложение даёт ровно один стак
// и копится по своим правилам (BleedEffect). Артефакт «−1 к интервалу» (AuraIntervalEffect) превращает 3 раунда в 2
public static class AuraBook
{
    public const int MinLevel = 1;
    public const int MaxLevel = 3;
    // С этого ранга дворф получает ауру
    public const int MinRank = 4;
    // Обычный интервал ауры в раундах
    public const int DefaultInterval = 3;

    public record Definition(string Effect, AuraTarget Target, int[] PowerByLevel, int[] DurationByLevel, int Interval = DefaultInterval)
    {
        // Длительность наложения 1-го уровня — для каталога и подсказок
        public int Duration => DurationByLevel[0];
    }

    // Сила в процентах: урон или лечение за ход — доля атаки носителя (EntityFactory), мороз — процент замедления энергии,
    // у выжигания энергии — сколько единиц энергии сгорает за срабатывание.
    // Средняя сила за раунд = сила × ходы / интервал: у 1-го уровня она как у прежних аур «каждый раунд»,
    // к 3-му уровню растёт втрое (прежние ауры росли вдвое)
    private static readonly Dictionary<string, Definition> Definitions = new Definition[]
    {
        // 12 / 24 / 36% × 2 хода раз в 3 раунда = 8 / 16 / 24% за раунд
        new("Burn", AuraTarget.Enemies, [12, 24, 36], [2, 2, 2]),
        // Замедление держится 2 хода из 3
        new("Frost", AuraTarget.Enemies, [10, 20, 30], [2, 2, 2]),
        // Один разряд раз в 3 раунда: 24 / 48 / 72% = 8 / 16 / 24% за раунд
        new("Shock", AuraTarget.Enemies, [24, 48, 72], [1, 1, 1]),
        // Долгий яд: 8 × 3, 10 × 4, 12 × 5 раз в 3 раунда = 8 / 13 / 20% за раунд; с артефактом слои накладываются
        new("Poison", AuraTarget.Enemies, [8, 10, 12], [3, 4, 5]),
        // Тьма не проклинает, а сразу выжигает врагам энергию: сила — сколько единиц энергии
        new(EnergyBurnEffect.Name, AuraTarget.Enemies, [10, 16, 24], [1, 1, 1]),
        // Кровотечение копится с аурами других носителей до 5,5× (BleedEffect) — сила ниже, чем у огня (docs/effect-rules.md).
        // Срабатывает каждый раунд: за наложение — один стак
        new("Bleed", AuraTarget.Enemies, [6, 9, 12], [1, 1, 1], Interval: 1),
        // Реген на 3 хода раз в 3 раунда — лечит без перерывов: 10 / 20 / 30% атаки за ход
        new("Regen", AuraTarget.Allies, [10, 20, 30], [3, 3, 3])
    }.ToDictionary(d => d.Effect);

    // Все ауры (эффекты) — для каталога админки
    public static IReadOnlyCollection<string> Effects => Definitions.Keys;

    // Мороз и выжигание энергии не наносят урона: у мороза сила — сам процент замедления, у выжигания — единицы энергии
    public static bool PowerIsPercentOfAttack(string effect) => effect is not ("Frost" or EnergyBurnEffect.Name);

    // Сила статуса ауры за ход для носителя с такой атакой
    public static int Resolve(string effect, int percent, int attack) =>
        PowerIsPercentOfAttack(effect)
            ? Math.Max(1, (int)Math.Round(attack * percent / 100.0, MidpointRounding.AwayFromZero))
            : percent;

    private static readonly Dictionary<HeroElement, string> ByElement = new()
    {
        [HeroElement.Fire] = "Burn",
        [HeroElement.Frost] = "Frost",
        [HeroElement.Lightning] = "Shock",
        [HeroElement.Nature] = "Poison",
        [HeroElement.Dark] = EnergyBurnEffect.Name,
        [HeroElement.Blood] = "Bleed",
        [HeroElement.Radiance] = "Regen"
    };

    public static Definition? Find(string? effect) =>
        effect != null && Definitions.TryGetValue(effect, out var definition) ? definition : null;

    // null — у стихии ауры нет (Basic)
    public static string? EffectFor(HeroElement element) => ByElement.GetValueOrDefault(element);

    public static int Power(Definition definition, int level) =>
        definition.PowerByLevel[Math.Clamp(level, MinLevel, MaxLevel) - MinLevel];

    public static int Duration(Definition definition, int level) =>
        definition.DurationByLevel[Math.Clamp(level, MinLevel, MaxLevel) - MinLevel];

    // Ауры монстров (кампания, башня): способность монстра, а не аура дворфа — бьют каждый раунд слабым наложением на ход
    // (прежние ауры дворфов). Кампания и башня сбалансированы под них, правило «раз в 3 раунда» их не касается
    private static readonly Dictionary<string, int[]> MonsterPowerByLevel = new()
    {
        ["Burn"] = [8, 12, 16],
        ["Frost"] = [5, 8, 12],
        ["Shock"] = [8, 12, 16],
        ["Poison"] = [8, 12, 16],
        [EnergyBurnEffect.Name] = [3, 4, 5],
        ["Bleed"] = [6, 9, 12],
        ["Regen"] = [10, 15, 20]
    };

    // Записывает монстру ауру его стихии: каждый раунд, наложение на ход
    public static void ApplyMonster(Hero hero, Definition definition, int level)
    {
        level = Math.Clamp(level, MinLevel, MaxLevel);
        hero.AuraEffect = definition.Effect;
        hero.AuraTarget = definition.Target;
        hero.AuraTrigger = TriggerPhase.OnRoundStart;
        hero.AuraPower = MonsterPowerByLevel[definition.Effect][level - MinLevel];
        hero.AuraDuration = 1;
        hero.AuraInterval = 1;
        hero.AuraLevel = level;
    }

    // Записывает ауру в героя; прежняя аура пропадает
    public static void Apply(Hero hero, Definition definition, int level)
    {
        level = Math.Clamp(level, MinLevel, MaxLevel);
        hero.AuraEffect = definition.Effect;
        hero.AuraTarget = definition.Target;
        hero.AuraTrigger = TriggerPhase.OnRoundStart;
        hero.AuraPower = Power(definition, level);
        hero.AuraDuration = Duration(definition, level);
        hero.AuraInterval = definition.Interval;
        hero.AuraLevel = level;
    }
}
