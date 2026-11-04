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

    // Сила как у аур шлемов (EquipmentCatalog) на 1-м уровне. Мороз — процент замедления, остальное — урон или лечение за ход
    private static readonly Dictionary<string, Definition> Definitions = new Definition[]
    {
        new("Burn", AuraTarget.Enemies, [3, 5, 7]),
        new("Frost", AuraTarget.Enemies, [5, 8, 12]),
        new("Shock", AuraTarget.Enemies, [3, 5, 7]),
        new("Poison", AuraTarget.Enemies, [3, 5, 7]),
        new("Curse", AuraTarget.Enemies, [3, 5, 7]),
        new("Bleed", AuraTarget.Enemies, [3, 5, 7]),
        new("Regen", AuraTarget.Allies, [3, 5, 8])
    }.ToDictionary(d => d.Effect);

    private static readonly Dictionary<HeroElement, string> ByElement = new()
    {
        [HeroElement.Fire] = "Burn",
        [HeroElement.Frost] = "Frost",
        [HeroElement.Lightning] = "Shock",
        [HeroElement.Nature] = "Poison",
        [HeroElement.Dark] = "Curse",
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
