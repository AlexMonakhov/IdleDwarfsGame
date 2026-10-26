// Новый герой из сундука: статы случайной роли стартового отряда, усиленные рангом, случайное имя.
// На первом ранге стихии нет (Basic), на остальных — указанная в таблице дропа или случайная
public static class HeroGenerator
{
    // +25% к базовым HP и атаке за каждый ранг выше первого: герой 4-го ранга примерно как герои демо-отряда
    public const double StatBonusPerRank = 0.25;

    private static readonly HeroElement[] Elements =
        [HeroElement.Fire, HeroElement.Frost, HeroElement.Lightning, HeroElement.Nature];

    public static double StatFactor(int rank) =>
        1 + StatBonusPerRank * (Math.Clamp(rank, HeroLevels.Min, HeroLevels.Max) - HeroLevels.Min);

    public static Hero Create(Guid playerId, int rank, string element, Random random)
    {
        rank = Math.Clamp(rank, HeroLevels.Min, HeroLevels.Max);
        var role = StarterSquad.Roles[random.Next(StarterSquad.Roles.Length)];
        var heroElement = Enum.TryParse<HeroElement>(element, out var parsed) && parsed != HeroElement.Basic
            ? parsed
            : Elements[random.Next(Elements.Length)];
        double factor = StatFactor(rank);

        return new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Name = StarterSquad.Names[random.Next(StarterSquad.Names.Length)],
            UnitType = "Dwarf",
            Element = HeroLevels.NormalizeElement(rank, heroElement),
            Level = rank,
            MaxHp = (int)Math.Round(role.MaxHp * factor, MidpointRounding.AwayFromZero),
            Attack = (int)Math.Round(role.Attack * factor, MidpointRounding.AwayFromZero),
            Speed = role.Speed,
            MaxEnergy = 100,
            StartEnergy = 0,
            EnergyPerAttack = 20
        };
    }
}
