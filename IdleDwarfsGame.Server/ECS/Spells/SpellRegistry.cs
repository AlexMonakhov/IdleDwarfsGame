// Единый реестр спеллов: Id спелла хранится в БД у героя (Hero.SpellIds)
public static class SpellRegistry
{
    public static readonly IReadOnlyDictionary<int, ISpell> All = new ISpell[]
    {
        new Fireball(1), new Fireball(2), new Fireball(3),          // 1, 2, 3
        new Lightning(1), new Lightning(2), new Lightning(3),       // 4, 5, 6
        new HealingRune(1), new HealingRune(2), new HealingRune(3)  // 7, 8, 9
    }.ToDictionary(s => s.Id);

    public static string Describe(int spellId) =>
        All.TryGetValue(spellId, out var spell) ? $"{spell.Name} {spell.Level}" : $"Unknown #{spellId}";
}
