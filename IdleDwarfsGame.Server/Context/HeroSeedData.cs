// Стартовые данные: отряд демо-игрока и шаблоны врагов башни.
// Статы демо-дворфов — профессия × множитель ранга (ProfessionBook, HeroLevels.RankFactor).
// Враги лежат в той же таблице Heroes, но принадлежат служебному игроку EnemyPlayerId.
public static class HeroSeedData
{
    public static readonly Guid DemoPlayerId = Guid.Parse("a1d2f3e4-0000-4000-8000-000000000001");
    public static readonly Guid EnemyPlayerId = Guid.Parse("e0e0e0e0-0000-4000-8000-000000000002");

    public static Player[] Players =>
    [
        new Player { Id = DemoPlayerId, Name = "Demo Player", Rating = 0 },
        new Player { Id = EnemyPlayerId, Name = "Tower Monsters", Rating = 0 }
    ];

    public static Hero[] Heroes =>
    [
        // ===== Отряд игрока =====
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000001"), PlayerId = DemoPlayerId,
            Name = "Brakk Ironbeard", UnitType = "Dwarf", Profession = HeroProfession.Defender, Element = HeroElement.Fire, Level = 3, PlaceInSquad = 1,
            MaxHp = 2828, Attack = 212, Speed = 3,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            Immunities = ["Stun"],
            // Защитник: регенерация всему отряду в начале раунда
            AuraEffect = "Regen", AuraTarget = AuraTarget.Allies, AuraTrigger = TriggerPhase.OnRoundStart, AuraPower = 10, AuraDuration = 1
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000002"), PlayerId = DemoPlayerId,
            Name = "Dain Emberforge", UnitType = "Dwarf", Profession = HeroProfession.Axeman, Element = HeroElement.Fire, Level = 3, PlaceInSquad = 2,
            MaxHp = 2036, Attack = 353, Speed = 4,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 20,
            SpellIds = [2], // Fireball 2
            Immunities = ["Burn"]
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000003"), PlayerId = DemoPlayerId,
            Name = "Thora Stormhammer", UnitType = "Dwarf", Profession = HeroProfession.Warrior, Element = HeroElement.Frost, Level = 3, PlaceInSquad = 3,
            MaxHp = 2488, Attack = 283, Speed = 5,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 25,
            SpellIds = [4, 6] // Lightning 1, Lightning 3
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000004"), PlayerId = DemoPlayerId,
            Name = "Grim Ashaxe", UnitType = "Dwarf", Profession = HeroProfession.Miner, Element = HeroElement.Fire, Level = 3, PlaceInSquad = 4,
            MaxHp = 2262, Attack = 269, Speed = 6,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000005"), PlayerId = DemoPlayerId,
            Name = "Runa Frostpick", UnitType = "Dwarf", Profession = HeroProfession.Scout, Element = HeroElement.Frost, Level = 3, PlaceInSquad = 5,
            MaxHp = 1810, Attack = 311, Speed = 8,
            MaxEnergy = 100, StartEnergy = 40, EnergyPerAttack = 20,
            SpellIds = [7] // Healing Rune 1
        },
        // Запасной герой, не в отряде
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000006"), PlayerId = DemoPlayerId,
            Name = "Oin Deepdelver", UnitType = "Dwarf", Profession = HeroProfession.Miner, Element = HeroElement.Frost, Level = 3, PlaceInSquad = null,
            MaxHp = 2262, Attack = 269, Speed = 6,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            SpellIds = [1]
        },
        // Запасные герои 4-го ранга новых стихий: полная книга заклинаний (ElementSpellBook)
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000007"), PlayerId = DemoPlayerId,
            Name = "Morgra Duskvein", UnitType = "Dwarf", Profession = HeroProfession.Axeman, Element = HeroElement.Dark, Level = 4, PlaceInSquad = null,
            MaxHp = 7024, Attack = 1219, Speed = 4,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 20,
            SpellIds = [39, 45], // Shadow Bolt 3, Hex 3
            Immunities = ["Curse"]
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000008"), PlayerId = DemoPlayerId,
            Name = "Elda Brightfacet", UnitType = "Dwarf", Profession = HeroProfession.Warrior, Element = HeroElement.Radiance, Level = 4, PlaceInSquad = null,
            MaxHp = 8584, Attack = 975, Speed = 5,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 20,
            SpellIds = [48, 51, 54], // Prism Ray 3, Radiant Nova 3, Dawnlight 3
            Immunities = ["Stun"]
        },

        // ===== Враги башни (статы базовые, масштабируются этажом: BalanceCalculator) =====
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000001"), PlayerId = EnemyPlayerId,
            Name = "Orc Brute", UnitType = "Orc", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 1,
            MaxHp = 1000, Attack = 70, Speed = 3,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            Immunities = ["Stun"]
        },
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000002"), PlayerId = EnemyPlayerId,
            Name = "Goblin Scout", UnitType = "Goblin", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 2,
            MaxHp = 480, Attack = 70, Speed = 9,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20
        },
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000003"), PlayerId = EnemyPlayerId,
            Name = "Goblin Shaman", UnitType = "Goblin", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 3,
            MaxHp = 440, Attack = 65, Speed = 7,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 20,
            SpellIds = [1] // Fireball 1
        },
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000004"), PlayerId = EnemyPlayerId,
            Name = "Orc Warlord", UnitType = "Orc", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 4,
            MaxHp = 800, Attack = 75, Speed = 4,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            Immunities = ["Burn"],
            // Аура жара: поджигает врагов в начале раунда
            AuraEffect = "Burn", AuraTarget = AuraTarget.Enemies, AuraTrigger = TriggerPhase.OnRoundStart, AuraPower = 8, AuraDuration = 1
        },
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000005"), PlayerId = EnemyPlayerId,
            Name = "Skeleton Mage", UnitType = "Skeleton", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 5,
            MaxHp = 480, Attack = 75, Speed = 6,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 25,
            SpellIds = [5], // Lightning 2
            Immunities = ["Frost"]
        }
    ];
}
