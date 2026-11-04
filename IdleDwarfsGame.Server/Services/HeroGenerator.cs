// Новый герой из сундука или слияния дворфов: случайная профессия, её статы с множителем ранга (HeroLevels.RankFactor), случайное имя.
// На первом ранге стихии нет (Basic), на остальных — указанная в таблице дропа или случайная.
// Способности по рангу (GrantAbilities):
//   ранг 1 — ничего
//   ранг 2 — заклинание стихии 1 уровня
//   ранг 3 — заклинание 2 уровня и пассивка 1–2 уровня
//   ранг 4 — заклинание 3 уровня, пассивка 1–3 уровня, аура стихии и с шансом ImmunityChance один иммунитет
public static class HeroGenerator
{
    // Иммунитет — редкость: несколько у одного дворфа не бывает
    public const int ImmunityMinRank = 4;
    public const double ImmunityChance = 0.1;

    public static readonly string[] Immunities = ["Stun", "Freeze", "Burn", "Frost", "Poison", "Curse", "Shock", "Bleed"];

    // Аметист, гранат и бриллиант редкие: выпадают вдвое реже остальных
    private static readonly (HeroElement Element, int Weight)[] Elements =
    [
        (HeroElement.Fire, 2), (HeroElement.Frost, 2), (HeroElement.Lightning, 2), (HeroElement.Nature, 2),
        (HeroElement.Dark, 1), (HeroElement.Blood, 1), (HeroElement.Radiance, 1)
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

    public static double StatFactor(int rank) => HeroLevels.RankFactor(rank);

    public static int ScaleToRank(int baseValue, int rank) =>
        (int)Math.Round(baseValue * StatFactor(rank), MidpointRounding.AwayFromZero);

    public static Hero Create(Guid playerId, int rank, string element, Random random)
    {
        rank = Math.Clamp(rank, HeroLevels.Min, HeroLevels.Available);
        var profession = ProfessionBook.Random(random);
        var role = ProfessionBook.Of(profession);
        var heroElement = Enum.TryParse<HeroElement>(element, out var parsed) && parsed != HeroElement.Basic
            ? parsed
            : RandomElement(random);
        heroElement = HeroLevels.NormalizeElement(rank, heroElement);

        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Name = StarterSquad.Names[random.Next(StarterSquad.Names.Length)],
            UnitType = "Dwarf",
            Profession = profession,
            Element = heroElement,
            Level = rank,
            MaxHp = ScaleToRank(role.MaxHp, rank),
            Attack = ScaleToRank(role.Attack, rank),
            Speed = role.Speed,
            MaxEnergy = 100,
            StartEnergy = 0,
            EnergyPerAttack = 20
        };
        GrantAbilities(hero, random);
        return hero;
    }

    // Способности под ранг и стихию героя; прежние стираются
    public static void GrantAbilities(Hero hero, Random random)
    {
        hero.SpellIds = ElementSpellBook.RandomSpell(hero.Element, hero.Level, random) is { } spell ? [spell] : [];

        int passiveLevel = PassiveBook.RandomLevel(hero.Level, random);
        hero.Passives = passiveLevel > 0 ? [PassiveBook.Random(random)] : [];
        hero.PassiveLevel = Math.Max(PassiveBook.MinLevel, passiveLevel);

        hero.AuraEffect = null;
        if (hero.Level >= AuraBook.MinRank && AuraBook.Find(AuraBook.EffectFor(hero.Element)) is { } aura)
            AuraBook.Apply(hero, aura, AuraBook.MinLevel);

        hero.Immunities = hero.Level >= ImmunityMinRank && random.NextDouble() < ImmunityChance
            ? [Immunities[random.Next(Immunities.Length)]]
            : [];
    }
}
