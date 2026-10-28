// Книга заклинаний стихии: три семейства спеллов, которые герой открывает с рангом (Hero.Level).
// Ранг 1 — без стихии и без спеллов, дальше:
//   ранг 2 — первый спелл 1 уровня
//   ранг 3 — первый 2 уровня, второй 1 уровня
//   ранг 4 — первый 3 уровня, второй 2 уровня, третий 1 уровня
//   ранг 5 — все три на 3 уровне
public static class ElementSpellBook
{
    // Id первого уровня каждого семейства (см. SpellRegistry)
    private static readonly Dictionary<HeroElement, int[]> Families = new()
    {
        [HeroElement.Fire] = [1, 10, 13],      // Fireball, Flame Wave, Meteor
        [HeroElement.Frost] = [16, 19, 22],    // Frost Bolt, Blizzard, Glacial Prison
        [HeroElement.Lightning] = [4, 25, 28], // Lightning, Chain Lightning, Thunderclap
        [HeroElement.Nature] = [7, 31, 34],    // Healing Rune, Venom Thorns, Entangling Roots
        [HeroElement.Dark] = [37, 40, 43],     // Shadow Bolt, Life Drain, Hex
        [HeroElement.Radiance] = [46, 49, 52]  // Prism Ray, Radiant Nova, Dawnlight
    };

    // Уровни семейств на каждом ранге; 0 — семейство ещё закрыто
    private static readonly int[][] LevelsByRank =
    [
        [0, 0, 0], // ранг 1
        [1, 0, 0], // ранг 2
        [2, 1, 0], // ранг 3
        [3, 2, 1], // ранг 4
        [3, 3, 3]  // ранг 5
    ];

    public static IReadOnlyCollection<HeroElement> Elements => Families.Keys;

    public static List<int> SpellsFor(HeroElement element, int rank)
    {
        if (!Families.TryGetValue(element, out var families))
            return [];

        var levels = LevelsByRank[Math.Clamp(rank, HeroLevels.Min, HeroLevels.Max) - HeroLevels.Min];
        return families
            .Zip(levels)
            .Where(f => f.Second > 0)
            .Select(f => SpellRegistry.IdOf(f.First, f.Second))
            .ToList();
    }
}
