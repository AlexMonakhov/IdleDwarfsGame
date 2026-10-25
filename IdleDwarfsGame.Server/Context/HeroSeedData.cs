// Стартовые данные: отряд демо-игрока и шаблоны врагов башни.
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
            Name = "Brakk Ironbeard", UnitType = "Dwarf", Element = HeroElement.Fire, Level = 4, PlaceInSquad = 1,
            MaxHp = 260, Attack = 14, Speed = 4,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            Immunities = ["Stun"],
            // Защитник: регенерация всему отряду в начале раунда
            AuraEffect = "Regen", AuraTarget = AuraTarget.Allies, AuraTrigger = TriggerPhase.OnRoundStart, AuraPower = 4, AuraDuration = 1
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000002"), PlayerId = DemoPlayerId,
            Name = "Dain Emberforge", UnitType = "Dwarf", Element = HeroElement.Fire, Level = 4, PlaceInSquad = 2,
            MaxHp = 130, Attack = 18, Speed = 7,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 20,
            SpellIds = [2], // Fireball 2
            Immunities = ["Burn"]
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000003"), PlayerId = DemoPlayerId,
            Name = "Thora Stormhammer", UnitType = "Dwarf", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 3,
            MaxHp = 150, Attack = 16, Speed = 6,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 25,
            SpellIds = [4, 6] // Lightning 1, Lightning 3
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000004"), PlayerId = DemoPlayerId,
            Name = "Grim Ashaxe", UnitType = "Dwarf", Element = HeroElement.Fire, Level = 4, PlaceInSquad = 4,
            MaxHp = 180, Attack = 20, Speed = 5,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20
        },
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000005"), PlayerId = DemoPlayerId,
            Name = "Runa Frostpick", UnitType = "Dwarf", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 5,
            MaxHp = 120, Attack = 12, Speed = 8,
            MaxEnergy = 100, StartEnergy = 40, EnergyPerAttack = 20,
            SpellIds = [7] // Healing Rune 1
        },
        // Запасной герой, не в отряде
        new Hero
        {
            Id = Guid.Parse("11111111-0000-4000-8000-000000000006"), PlayerId = DemoPlayerId,
            Name = "Oin Deepdelver", UnitType = "Dwarf", Element = HeroElement.Frost, Level = 4, PlaceInSquad = null,
            MaxHp = 140, Attack = 15, Speed = 5,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            SpellIds = [1]
        },

        // ===== Враги башни (статы базовые, масштабируются этажом) =====
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000001"), PlayerId = EnemyPlayerId,
            Name = "Orc Brute", UnitType = "Orc", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 1,
            MaxHp = 210, Attack = 13, Speed = 3,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            Immunities = ["Stun"]
        },
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000002"), PlayerId = EnemyPlayerId,
            Name = "Goblin Scout", UnitType = "Goblin", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 2,
            MaxHp = 90, Attack = 10, Speed = 9,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20
        },
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000003"), PlayerId = EnemyPlayerId,
            Name = "Goblin Shaman", UnitType = "Goblin", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 3,
            MaxHp = 85, Attack = 10, Speed = 7,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 20,
            SpellIds = [1] // Fireball 1
        },
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000004"), PlayerId = EnemyPlayerId,
            Name = "Orc Warlord", UnitType = "Orc", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 4,
            MaxHp = 180, Attack = 12, Speed = 4,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            Immunities = ["Burn"],
            // Аура жара: поджигает врагов в начале раунда
            AuraEffect = "Burn", AuraTarget = AuraTarget.Enemies, AuraTrigger = TriggerPhase.OnRoundStart, AuraPower = 3, AuraDuration = 1
        },
        new Hero
        {
            Id = Guid.Parse("22222222-0000-4000-8000-000000000005"), PlayerId = EnemyPlayerId,
            Name = "Skeleton Mage", UnitType = "Skeleton", Element = HeroElement.Frost, Level = 4, PlaceInSquad = 5,
            MaxHp = 100, Attack = 13, Speed = 6,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 25,
            SpellIds = [5], // Lightning 2
            Immunities = ["Frost"]
        }
    ];
}
