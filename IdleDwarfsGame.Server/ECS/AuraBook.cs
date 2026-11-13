// Ауры дворфов: у каждой стихии своя, сила растёт с уровнем 1..MaxLevel.
// Дворф 4-го ранга получает ауру своей стихии 1-го уровня, выше её поднимают слиянием извлечённых аур (AbilityService).
// Ключ ауры — название эффекта (StatusEffectFactory): так он хранится и у героя (Hero.AuraEffect), и у извлечённой ауры
public static class AuraBook
{
    public const int MinLevel = 1;
    public const int MaxLevel = 3;
    // С этого ранга дворф получает ауру
    public const int MinRank = 4;

    public record Definition(string Effect, AuraTarget Target, int[] PowerByLevel, int Duration = 1);

    // Сила в процентах: урон или лечение за ход — доля атаки носителя (EntityFactory), мороз — процент замедления энергии,
    // у выжигания энергии — сколько единиц энергии сгорает за срабатывание
    private static readonly Dictionary<string, Definition> Definitions = new Definition[]
    {
        new("Burn", AuraTarget.Enemies, [8, 12, 16]),
        new("Frost", AuraTarget.Enemies, [5, 8, 12]),
        new("Shock", AuraTarget.Enemies, [8, 12, 16]),
        new("Poison", AuraTarget.Enemies, [8, 12, 16]),
        // Тьма не проклинает, а сразу выжигает врагам энергию: сила — сколько единиц энергии
        new(EnergyBurnEffect.Name, AuraTarget.Enemies, [3, 4, 5]),
        // Кровотечение копится с аурами других носителей до 5,5× (BleedEffect) — сила ниже, чем у огня (docs/effect-rules.md)
        new("Bleed", AuraTarget.Enemies, [6, 9, 12]),
        new("Regen", AuraTarget.Allies, [10, 15, 20])
    }.ToDictionary(d => d.Effect);

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

    // Записывает ауру в героя; прежняя аура пропадает
    public static void Apply(Hero hero, Definition definition, int level)
    {
        level = Math.Clamp(level, MinLevel, MaxLevel);
        hero.AuraEffect = definition.Effect;
        hero.AuraTarget = definition.Target;
        hero.AuraTrigger = TriggerPhase.OnRoundStart;
        hero.AuraPower = Power(definition, level);
        hero.AuraDuration = definition.Duration;
        hero.AuraInterval = 1;
        hero.AuraLevel = level;
    }
}
