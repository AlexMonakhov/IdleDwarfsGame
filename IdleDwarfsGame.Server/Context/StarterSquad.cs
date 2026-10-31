// Отряд, который получает каждый новый аккаунт: 5 базовых дворфов 1-го уровня, сразу расставленных по слотам.
public static class StarterSquad
{
    public const int Size = 5;

    // Имена и роли берёт и HeroGenerator для героев из сундуков
    internal static readonly string[] Names =
    [
        "Borin", "Durgan", "Thrain", "Hilda", "Gorim", "Kili", "Balin", "Brynja",
        "Nori", "Dwalin", "Astrid", "Fundin", "Groa", "Torvin", "Sigrun", "Bofur"
    ];

    // Роли по слотам: впереди крепкий защитник, сзади быстрый разведчик
    internal static readonly (int MaxHp, int Attack, int Speed)[] Roles =
    [
        (140, 8, 3),  // защитник
        (110, 11, 5), // воин
        (100, 13, 4), // секироносец
        (95, 10, 6),  // рудокоп
        (80, 10, 8)   // разведчик
    ];

    public static List<Hero> Create(Guid playerId, Random? random = null)
    {
        random ??= Random.Shared;
        var names = Names.OrderBy(_ => random.Next()).Take(Size).ToArray();

        return Roles
            .Select((role, index) => new Hero
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Name = names[index],
                UnitType = "Dwarf",
                Element = HeroElement.Basic,
                Level = HeroLevels.Min,
                PlaceInSquad = index + 1,
                MaxHp = role.MaxHp,
                Attack = role.Attack,
                Speed = role.Speed,
                MaxEnergy = 100,
                StartEnergy = 0,
                EnergyPerAttack = 20,
                Passives = [PassiveBook.Random(random)]
            })
            .ToList();
    }
}
