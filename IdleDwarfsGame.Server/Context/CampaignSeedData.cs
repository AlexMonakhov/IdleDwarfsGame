// Стартовая кампания: 1-4 мобы на лавовой арене, 5 босс, 6-9 мобы на ледяной арене, 10 босс.
// Боссы и ледяные мобы — шаблоны в таблице Heroes у служебного игрока CampaignPlayerId,
// обычные мобы переиспользуют шаблоны врагов башни (HeroSeedData.EnemyPlayerId).
public static class CampaignSeedData
{
    public static readonly Guid CampaignPlayerId = Guid.Parse("c0c0c0c0-0000-4000-8000-000000000003");

    public const int LavaArenaId = 1;
    public const int IceArenaId = 2;

    // Шаблоны врагов башни
    private static readonly Guid OrcBrute = Guid.Parse("22222222-0000-4000-8000-000000000001");
    private static readonly Guid GoblinScout = Guid.Parse("22222222-0000-4000-8000-000000000002");
    private static readonly Guid GoblinShaman = Guid.Parse("22222222-0000-4000-8000-000000000003");
    private static readonly Guid OrcWarlord = Guid.Parse("22222222-0000-4000-8000-000000000004");
    private static readonly Guid SkeletonMage = Guid.Parse("22222222-0000-4000-8000-000000000005");

    // Шаблоны кампании
    private static readonly Guid FrostGoblin = Guid.Parse("33333333-0000-4000-8000-000000000001");
    private static readonly Guid RimeSkeleton = Guid.Parse("33333333-0000-4000-8000-000000000002");
    private static readonly Guid GorrakBoss = Guid.Parse("33333333-0000-4000-8000-000000000101");
    private static readonly Guid HrimgarBoss = Guid.Parse("33333333-0000-4000-8000-000000000102");

    public static Player Player => new() { Id = CampaignPlayerId, Name = "Campaign Monsters", Rating = 0 };

    public static Hero[] Heroes =>
    [
        new Hero
        {
            Id = FrostGoblin, PlayerId = CampaignPlayerId,
            Name = "Frost Goblin", UnitType = "Goblin", Element = HeroElement.Frost, Level = 4,
            MaxHp = 95, Attack = 11, Speed = 8,
            MaxEnergy = 100, StartEnergy = 0, EnergyPerAttack = 20,
            Immunities = ["Frost"]
        },
        new Hero
        {
            Id = RimeSkeleton, PlayerId = CampaignPlayerId,
            Name = "Rime Skeleton", UnitType = "Skeleton", Element = HeroElement.Frost, Level = 4,
            MaxHp = 120, Attack = 12, Speed = 5,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 20,
            SpellIds = [4], // Lightning 1
            Immunities = ["Frost"]
        },

        // ===== Боссы =====
        new Hero
        {
            Id = GorrakBoss, PlayerId = CampaignPlayerId,
            Name = "Gorrak the Molten", UnitType = "Orc", Element = HeroElement.Fire, Level = 4,
            MaxHp = 260, Attack = 16, Speed = 5,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 20,
            SpellIds = [2], // Fireball 2
            Immunities = ["Burn", "Stun"],
            // Раскалённая аура: поджигает весь отряд игрока в начале раунда
            AuraEffect = "Burn", AuraTarget = AuraTarget.Enemies, AuraTrigger = TriggerPhase.OnRoundStart, AuraPower = 4, AuraDuration = 1
        },
        new Hero
        {
            Id = HrimgarBoss, PlayerId = CampaignPlayerId,
            Name = "Hrimgar the Frostbound", UnitType = "Skeleton", Element = HeroElement.Frost, Level = 4,
            MaxHp = 240, Attack = 15, Speed = 6,
            MaxEnergy = 100, StartEnergy = 20, EnergyPerAttack = 25,
            SpellIds = [6], // Lightning 3
            Immunities = ["Frost", "Stun"],
            // Ледяное дыхание: морозит весь отряд игрока в начале раунда
            AuraEffect = "Frost", AuraTarget = AuraTarget.Enemies, AuraTrigger = TriggerPhase.OnRoundStart, AuraPower = 3, AuraDuration = 1
        }
    ];

    public static Arena[] Arenas =>
    [
        new Arena
        {
            Id = LavaArenaId, Name = "Lava Arena", Background = "lava",
            Description = "Molten stone burns every fighter at the start of each round.",
            EffectName = "Burn", EffectPower = 5, EffectDuration = 1
        },
        new Arena
        {
            Id = IceArenaId, Name = "Ice Arena", Background = "ice",
            Description = "Icy wind wounds every fighter and drains their energy at the start of each round.",
            EffectName = "Frost", EffectPower = 3, EffectDuration = 1
        }
    ];

    public static CampaignStage[] Stages =>
    [
        Stage(1, "Ember Gate", "Goblins guard the entrance to the fire caves.", StageType.Mobs, 1, LavaArenaId),
        Stage(2, "Smouldering Tunnels", "An orc leads scouts through the smouldering tunnels.", StageType.Mobs, 1, LavaArenaId),
        Stage(3, "Cinder Hall", "A skeleton mage has joined the orcs in the hall of ash.", StageType.Mobs, 2, LavaArenaId),
        Stage(4, "Warlord's Camp", "The orc warlord's camp. His aura sets everything ablaze.", StageType.Mobs, 2, LavaArenaId),
        Stage(5, "Heart of the Forge", "Gorrak the Molten guards the heart of the forge.", StageType.Boss, 2, LavaArenaId),
        Stage(6, "Frozen Pass", "Frost goblins lie in ambush at the pass.", StageType.Mobs, 3, IceArenaId),
        Stage(7, "Rime Caverns", "The rime caverns are crawling with undead.", StageType.Mobs, 3, IceArenaId),
        Stage(8, "Glacier Bridge", "A mixed warband holds the bridge across the glacier.", StageType.Mobs, 3, IceArenaId),
        Stage(9, "Hall of Whispers", "The last line of defence before the frost lord's lair.", StageType.Mobs, 3, IceArenaId),
        Stage(10, "Throne of Frost", "Hrimgar the Frostbound awaits on his throne of ice.", StageType.Boss, 3, IceArenaId)
    ];

    public static CampaignStageEnemy[] StageEnemies =>
    [
        Enemy(1, 1, GoblinScout),
        Enemy(1, 2, GoblinShaman),

        Enemy(2, 1, OrcBrute),
        Enemy(2, 2, GoblinScout),
        Enemy(2, 3, GoblinShaman),

        Enemy(3, 1, OrcBrute),
        Enemy(3, 2, GoblinScout),
        Enemy(3, 3, GoblinShaman),
        Enemy(3, 4, SkeletonMage),

        Enemy(4, 1, OrcBrute),
        Enemy(4, 2, GoblinScout),
        Enemy(4, 3, GoblinShaman),
        Enemy(4, 4, OrcWarlord),
        Enemy(4, 5, SkeletonMage),

        Enemy(5, 1, OrcBrute),
        Enemy(5, 2, GorrakBoss, isBoss: true, hpMultiplier: 2),
        Enemy(5, 3, GoblinShaman),

        Enemy(6, 1, FrostGoblin),
        Enemy(6, 2, RimeSkeleton),
        Enemy(6, 3, GoblinScout),

        Enemy(7, 1, OrcBrute),
        Enemy(7, 2, FrostGoblin),
        Enemy(7, 3, RimeSkeleton),
        Enemy(7, 4, SkeletonMage),

        Enemy(8, 1, OrcBrute),
        Enemy(8, 2, FrostGoblin),
        Enemy(8, 3, RimeSkeleton),
        Enemy(8, 4, SkeletonMage),
        Enemy(8, 5, GoblinShaman),

        Enemy(9, 1, OrcBrute),
        Enemy(9, 2, FrostGoblin),
        Enemy(9, 3, RimeSkeleton),
        Enemy(9, 4, GoblinScout),
        Enemy(9, 5, SkeletonMage),

        Enemy(10, 1, OrcBrute),
        Enemy(10, 2, HrimgarBoss, isBoss: true, hpMultiplier: 1.75)
    ];

    private static CampaignStage Stage(int id, string name, string description, StageType type, int enemyLevel, int arenaId) => new()
    {
        Id = id, Name = name, Description = description, Type = type, EnemyLevel = enemyLevel, ArenaId = arenaId
    };

    // Id = уровень * 100 + позиция: стабильный, не меняется при перестановке строк в сиде
    private static CampaignStageEnemy Enemy(int stageId, int position, Guid heroId, bool isBoss = false, double hpMultiplier = 1) => new()
    {
        Id = stageId * 100 + position, StageId = stageId, Position = position, HeroId = heroId, IsBoss = isBoss, HpMultiplier = hpMultiplier
    };
}
