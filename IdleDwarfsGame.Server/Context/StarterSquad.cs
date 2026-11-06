// Отряд, который получает каждый новый аккаунт: 5 базовых дворфов 1-го уровня, сразу расставленных по слотам,
// и ещё ReserveSize таких же в запасе (Roster) — на шахты, экспедиции и первые слияния.
// Способностей у дворфов 1-го ранга нет (HeroGenerator.GrantAbilities)
public static class StarterSquad
{
    public const int Size = 5;
    public const int ReserveSize = 10;

    // Всё, с чем начинает новый игрок: отряд (Create) и запас дворфов 1-го ранга вне отряда — всего 15
    public static List<Hero> Roster(Guid playerId, Random? random = null)
    {
        random ??= Random.Shared;
        var heroes = Create(playerId, random);
        for (int i = 0; i < ReserveSize; i++)
            heroes.Add(HeroGenerator.Create(playerId, HeroLevels.Min, string.Empty, random));
        return heroes;
    }

    // Имена берёт и HeroGenerator для героев из сундуков; статы — по профессии (ProfessionBook)
    internal static readonly string[] Names =
    [
        "Borin", "Durgan", "Thrain", "Hilda", "Gorim", "Kili", "Balin", "Brynja",
        "Nori", "Dwalin", "Astrid", "Fundin", "Groa", "Torvin", "Sigrun", "Bofur"
    ];

    public static List<Hero> Create(Guid playerId, Random? random = null)
    {
        random ??= Random.Shared;
        var names = Names.OrderBy(_ => random.Next()).Take(Size).ToArray();

        return ProfessionBook.SquadOrder
            .Select((profession, index) => (profession, role: ProfessionBook.Of(profession), index))
            .Select(p => new Hero
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Name = names[p.index],
                Profession = p.profession,
                UnitType = "Dwarf",
                Element = HeroElement.Basic,
                Level = HeroLevels.Min,
                PlaceInSquad = p.index + 1,
                MaxHp = p.role.MaxHp,
                Attack = p.role.Attack,
                Speed = p.role.Speed,
                MaxEnergy = 100,
                StartEnergy = 0,
                EnergyPerAttack = 20
            })
            .ToList();
    }
}
