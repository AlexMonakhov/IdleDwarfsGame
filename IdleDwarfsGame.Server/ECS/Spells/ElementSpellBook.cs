// Книга заклинаний стихии: до трёх семейств спеллов. Дворф знает одно заклинание своей стихии,
// его уровень зависит от ранга (Hero.Level). У стихии с меньшим числом семейств выбор просто меньше:
//   ранг 1 — без стихии и без спелла
//   ранг 2 — спелл 1 уровня
//   ранг 3 — спелл 2 уровня
//   ранг 4 и выше — спелл 3 уровня
// Другие спеллы дворф получает только вставкой извлечённых способностей (AbilityService)
public static class ElementSpellBook
{
    public const int MaxLevel = 3;

    // Id первого уровня каждого семейства (см. SpellRegistry)
    private static readonly Dictionary<HeroElement, int[]> Families = new()
    {
        [HeroElement.Fire] = [1, 10, 13],      // Fireball, Flame Wave, Meteor
        [HeroElement.Frost] = [16, 19, 22],    // Frost Bolt, Blizzard, Glacial Prison
        [HeroElement.Lightning] = [4, 25, 28], // Lightning, Chain Lightning, Thunderclap
        [HeroElement.Nature] = [7, 31, 34],    // Healing Rune, Venom Thorns, Entangling Roots
        [HeroElement.Dark] = [37, 43],         // Shadow Bolt, Hex
        [HeroElement.Radiance] = [46, 49, 52], // Prism Ray, Radiant Nova, Dawnlight
        [HeroElement.Blood] = [40]             // Life Drain
    };

    public static IReadOnlyCollection<HeroElement> Elements => Families.Keys;

    // Id первого уровня семейств стихии; у Basic семейств нет
    public static IReadOnlyList<int> FamiliesOf(HeroElement element) =>
        Families.TryGetValue(element, out var families) ? families : [];

    // Уровень спелла дворфа этого ранга; 0 — спелла нет
    public static int SpellLevelFor(int rank) => Math.Clamp(rank - HeroLevels.Min, 0, MaxLevel);

    // Случайное заклинание стихии под ранг; null — дворфу этого ранга или стихии спелл не положен
    public static int? RandomSpell(HeroElement element, int rank, Random random)
    {
        var families = FamiliesOf(element);
        int level = SpellLevelFor(rank);
        if (families.Count == 0 || level == 0)
            return null;

        return SpellRegistry.IdOf(families[random.Next(families.Count)], level);
    }

    // Id первого уровня семейства, к которому относится спелл; null — такого спелла нет
    public static int? FamilyOf(int spellId) =>
        SpellRegistry.All.TryGetValue(spellId, out var spell) ? spellId - (spell.Level - 1) : null;

    // Стихия, в книге которой лежит семейство; Basic — ни в одной
    public static HeroElement ElementOf(int familyId) =>
        Families.FirstOrDefault(f => f.Value.Contains(familyId)).Key;
}
