// Единый реестр спеллов: Id спелла хранится в БД у героя (Hero.SpellIds)
public static class SpellRegistry
{
    public static readonly IReadOnlyDictionary<int, ISpell> All = new ISpell[]
    {
        new Fireball(1), new Fireball(2), new Fireball(3),                      // 1, 2, 3
        new Lightning(1), new Lightning(2), new Lightning(3),                   // 4, 5, 6
        new HealingRune(1), new HealingRune(2), new HealingRune(3),             // 7, 8, 9
        new FlameWave(1), new FlameWave(2), new FlameWave(3),                   // 10, 11, 12
        new Meteor(1), new Meteor(2), new Meteor(3),                            // 13, 14, 15
        new FrostBolt(1), new FrostBolt(2), new FrostBolt(3),                   // 16, 17, 18
        new Blizzard(1), new Blizzard(2), new Blizzard(3),                      // 19, 20, 21
        new GlacialPrison(1), new GlacialPrison(2), new GlacialPrison(3),       // 22, 23, 24
        new ChainLightning(1), new ChainLightning(2), new ChainLightning(3),    // 25, 26, 27
        new Thunderclap(1), new Thunderclap(2), new Thunderclap(3),             // 28, 29, 30
        new VenomThorns(1), new VenomThorns(2), new VenomThorns(3),             // 31, 32, 33
        new EntanglingRoots(1), new EntanglingRoots(2), new EntanglingRoots(3), // 34, 35, 36
        new ShadowBolt(1), new ShadowBolt(2), new ShadowBolt(3),                // 37, 38, 39
        new LifeDrain(1), new LifeDrain(2), new LifeDrain(3),                   // 40, 41, 42
        new Hex(1), new Hex(2), new Hex(3),                                     // 43, 44, 45
        new PrismRay(1), new PrismRay(2), new PrismRay(3),                      // 46, 47, 48
        new RadiantNova(1), new RadiantNova(2), new RadiantNova(3),             // 49, 50, 51
        new Dawnlight(1), new Dawnlight(2), new Dawnlight(3)                    // 52, 53, 54
    }.ToDictionary(s => s.Id);

    public static string Describe(int spellId) =>
        All.TryGetValue(spellId, out var spell) ? $"{spell.Name} {spell.Level}" : $"Unknown #{spellId}";

    // Id спелла по семейству и уровню 1..3: у семейства три подряд идущих Id, первый — у уровня 1
    public static int IdOf(int firstLevelId, int level) => firstLevelId + Math.Clamp(level, 1, 3) - 1;
}
