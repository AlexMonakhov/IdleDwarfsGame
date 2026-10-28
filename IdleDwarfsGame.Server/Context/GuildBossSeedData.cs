// Шаблоны боссов гильдии: по боссу на каждую стихию, у каждого — своя арена (GuildBosses).
// Лежат в таблице Heroes у отдельного служебного игрока, чтобы не попадать во врагов башни.
// HP шаблона в бою не используется — его задаёт GuildBosses, атака масштабируется уровнем босса
public static class GuildBossSeedData
{
    public static readonly Guid PlayerId = Guid.Parse("b0b0b0b0-0000-4000-8000-000000000004");

    public static readonly Guid MossbackTroll = Guid.Parse("44444444-0000-4000-8000-000000000001");
    public static readonly Guid RimehornYeti = Guid.Parse("44444444-0000-4000-8000-000000000002");
    public static readonly Guid AshmawWarlord = Guid.Parse("44444444-0000-4000-8000-000000000003");
    public static readonly Guid GrukkStormcaller = Guid.Parse("44444444-0000-4000-8000-000000000004");
    public static readonly Guid NyxaraHollowQueen = Guid.Parse("44444444-0000-4000-8000-000000000005");
    public static readonly Guid SeraphelShatteredLight = Guid.Parse("44444444-0000-4000-8000-000000000006");

    public static Player Player => new() { Id = PlayerId, Name = "Guild Bosses", Rating = 0 };

    public static Hero[] Heroes =>
    [
        // ===== Лёгкие =====
        new Hero
        {
            Id = MossbackTroll, PlayerId = PlayerId,
            Name = "Mossback Troll", UnitType = "Troll", Element = HeroElement.Nature, Level = 4,
            MaxHp = 210, Attack = 13, Speed = 3,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            SpellIds = [31], // Venom Thorns 1
            Immunities = ["Stun", "Poison"]
        },
        new Hero
        {
            Id = RimehornYeti, PlayerId = PlayerId,
            Name = "Rimehorn Yeti", UnitType = "Yeti", Element = HeroElement.Frost, Level = 4,
            MaxHp = 210, Attack = 13, Speed = 3,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            SpellIds = [16], // Frost Bolt 1
            Immunities = ["Frost", "Freeze"]
        },

        // ===== Средние =====
        new Hero
        {
            Id = AshmawWarlord, PlayerId = PlayerId,
            Name = "Ashmaw the Warlord", UnitType = "Orc", Element = HeroElement.Fire, Level = 4,
            MaxHp = 180, Attack = 12, Speed = 4,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            // Без ауры жара: лава и так поджигает весь отряд каждый раунд
            SpellIds = [10], // Flame Wave 1
            Immunities = ["Burn"]
        },
        new Hero
        {
            Id = GrukkStormcaller, PlayerId = PlayerId,
            Name = "Grukk Stormcaller", UnitType = "Ogre", Element = HeroElement.Lightning, Level = 4,
            MaxHp = 180, Attack = 12, Speed = 4,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            SpellIds = [25], // Chain Lightning 1
            Immunities = ["Shock", "Stun"]
        },

        // ===== Сложные =====
        new Hero
        {
            Id = NyxaraHollowQueen, PlayerId = PlayerId,
            Name = "Nyxara the Hollow Queen", UnitType = "Skeleton", Element = HeroElement.Dark, Level = 4,
            MaxHp = 100, Attack = 13, Speed = 6,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 25,
            // Проклятие накладывает склеп, поэтому стрела без него
            SpellIds = [37], // Shadow Bolt 1
            Immunities = ["Curse", "Frost"]
        },
        new Hero
        {
            Id = SeraphelShatteredLight, PlayerId = PlayerId,
            Name = "Seraphel the Shattered Light", UnitType = "Golem", Element = HeroElement.Radiance, Level = 4,
            MaxHp = 100, Attack = 13, Speed = 6,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 25,
            SpellIds = [47], // Prism Ray 2
            Immunities = ["Stun", "Freeze"]
        }
    ];
}
