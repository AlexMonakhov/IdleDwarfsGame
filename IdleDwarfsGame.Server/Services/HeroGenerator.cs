// Новый герой из сундука: статы случайной роли стартового отряда, усиленные рангом, случайное имя.
// На первом ранге стихии нет (Basic), на остальных — указанная в таблице дропа или случайная.
// Заклинания — из книги стихии под ранг (ElementSpellBook)
public static class HeroGenerator
{
    // +25% к базовым HP и атаке за каждый ранг выше первого: герой 4-го ранга примерно как герои демо-отряда
    public const double StatBonusPerRank = 0.25;

    // Аметист и бриллиант редкие — как и их камни в сундуках гемов: выпадают вдвое реже остальных
    private static readonly (HeroElement Element, int Weight)[] Elements =
    [
        (HeroElement.Fire, 2), (HeroElement.Frost, 2), (HeroElement.Lightning, 2), (HeroElement.Nature, 2),
        (HeroElement.Dark, 1), (HeroElement.Radiance, 1)
    ];

    private static HeroElement RandomElement(Random random)
    {
        int roll = random.Next(Elements.Sum(e => e.Weight));
        foreach (var (element, weight) in Elements)
        {
            if (roll < weight) return element;
            roll -= weight;
        }
        return Elements[^1].Element;
    }

    public static double StatFactor(int rank) =>
        1 + StatBonusPerRank * (Math.Clamp(rank, HeroLevels.Min, HeroLevels.Max) - HeroLevels.Min);

    public static Hero Create(Guid playerId, int rank, string element, Random random)
    {
        rank = Math.Clamp(rank, HeroLevels.Min, HeroLevels.Max);
        var role = StarterSquad.Roles[random.Next(StarterSquad.Roles.Length)];
        var heroElement = Enum.TryParse<HeroElement>(element, out var parsed) && parsed != HeroElement.Basic
            ? parsed
            : RandomElement(random);
        heroElement = HeroLevels.NormalizeElement(rank, heroElement);
        double factor = StatFactor(rank);

        return new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Name = StarterSquad.Names[random.Next(StarterSquad.Names.Length)],
            UnitType = "Dwarf",
            Element = heroElement,
            Level = rank,
            MaxHp = (int)Math.Round(role.MaxHp * factor, MidpointRounding.AwayFromZero),
            Attack = (int)Math.Round(role.Attack * factor, MidpointRounding.AwayFromZero),
            Speed = role.Speed,
            MaxEnergy = 100,
            StartEnergy = 0,
            EnergyPerAttack = 20,
            SpellIds = ElementSpellBook.SpellsFor(heroElement, rank),
            Passives = [PassiveBook.Random(random)]
        };
    }
}
