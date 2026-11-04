// Генерируемая часть кампании: уровни после вводных десяти (CampaignSeedData) до TotalStages.
// Кампания идёт главами по ChapterSize уровней; каждая глава — регион одной стихии на её арене,
// регионы сменяют друг друга по кругу. В главе враги из набора региона, к концу главы их больше,
// последний уровень главы — босс региона.
// Силу врагов задаёт EnemyLevelFor: путь рассчитан на весь рост дворфов от 1-го ранга до 4-го ранга 100-го уровня.
// Всё детерминировано: сид одинаков между запусками, поэтому EF видит одни и те же строки
public static class CampaignGenerator
{
    public const int TotalStages = 500;
    public const int ChapterSize = 10;
    // Уровни 1..HandcraftedStages описаны вручную (CampaignSeedData)
    public const int HandcraftedStages = 10;

    // Во сколько раз HP босса больше, чем у шаблона на том же уровне врагов
    public const double BossHpMultiplier = 2.5;

    // Опорные точки «уровень кампании → уровень врагов», между ними — линейно.
    // Каждый уровень врагов — +4.5% к их статам (BalanceCalculator). Ориентиры по симуляции (BalanceSimulation):
    // босса 50-го уровня проходит отряд 2-го ранга 20-го уровня, 150-го — 3-го ранга 50-го,
    // 250 / 350 / 500-го — 4-го ранга 25 / 50 / 100-го. Между рангами — скачок: слияние даёт новые способности
    private static readonly (int Stage, int EnemyLevel)[] LevelCurve =
    [
        (10, 13), (15, 15), (30, 17), (50, 22), (60, 27), (100, 44), (150, 54), (165, 58), (250, 70), (350, 82), (425, 90), (500, 98)
    ];

    public static int EnemyLevelFor(int stageId)
    {
        if (stageId <= LevelCurve[0].Stage) return LevelCurve[0].EnemyLevel;
        for (int i = 1; i < LevelCurve.Length; i++)
        {
            var (toStage, toLevel) = LevelCurve[i];
            var (fromStage, fromLevel) = LevelCurve[i - 1];
            if (stageId <= toStage)
                return fromLevel + (int)Math.Round((double)(toLevel - fromLevel) * (stageId - fromStage) / (toStage - fromStage));
        }
        return LevelCurve[^1].EnemyLevel;
    }

    // Минимальный уровень заклинаний монстров: дальше по кампании они кастуют сильнее
    public static int MonsterSpellLevel(int stageId) => stageId switch
    {
        <= 50 => 1,
        <= 150 => 2,
        _ => 3
    };

    // Монстры рисуются только спрайтами, которые есть на фронте
    private const string Golem = "Golem", Goblin = "Goblin", Skeleton = "Skeleton", Draconid = "Draconid";

    // Враги региона: Tank впереди, Melee и Caster — основа, Elite — к концу главы, Boss — на последнем уровне
    public record Region(
        string Name, int ArenaId,
        Guid Tank, Guid Melee, Guid Caster, Guid Elite, Guid Boss,
        string[] Places, string MobsText, string BossText);

    private static Guid Id(int n) => Guid.Parse($"33333333-0000-4000-8000-{n:D12}");

    // Лава переиспользует шаблоны вводных уровней и башни
    private static readonly Guid MagmaGolem = Guid.Parse("22222222-0000-4000-8000-000000000001");
    private static readonly Guid GoblinScout = Guid.Parse("22222222-0000-4000-8000-000000000002");
    private static readonly Guid GoblinShaman = Guid.Parse("22222222-0000-4000-8000-000000000003");
    private static readonly Guid EmberDrake = Guid.Parse("22222222-0000-4000-8000-000000000004");

    // Регионы по главам начиная со второй (уровни 11–20), дальше по кругу
    public static readonly Region[] Regions =
    [
        new("Storm Peak", CampaignSeedData.StormArenaId, Id(201), Id(202), Id(203), Id(204), Id(205),
            ["Thundering Steps", "Cloudbreak Ridge", "Spark Hollow", "Rumbling Cliffs", "Static Fields",
             "Lightning Spire", "Howling Saddle", "Stormwatch", "Skyforge Ruins", "Eye of the Storm"],
            "Storm creatures crackle with lightning on the slopes.", "Voltaris rides the storm above the summit."),
        new("Venom Marsh", CampaignSeedData.MarshArenaId, Id(211), Id(212), Id(213), Id(214), Id(215),
            ["Rotting Shallows", "Fen of Thorns", "Mossy Causeway", "Drowned Grove", "Toad Hollow",
             "Blackwater Bend", "Sporefield", "Hanging Roots", "Witchlight Bog", "Heart of the Mire"],
            "Poison drips from every branch of the marsh.", "Mirefang waits where the bog runs deepest."),
        new("Shadow Crypt", CampaignSeedData.CryptArenaId, Id(221), Id(222), Id(223), Id(224), Id(225),
            ["Silent Ossuary", "Hall of Bones", "Whispering Vault", "Gravewind Stair", "Chapel of Ash",
             "Forgotten Catacombs", "Mourners' Gallery", "Sunless Depths", "Sealed Reliquary", "Throne of Night"],
            "The dead of the crypt rise against the intruders.", "Morvath the Hollow rules the deepest vault."),
        new("Crystal Sanctum", CampaignSeedData.SanctumArenaId, Id(231), Id(232), Id(233), Id(234), Id(235),
            ["Prism Gate", "Shimmering Hall", "Glass Gardens", "Hall of Mirrors", "Choir of Light",
             "Radiant Nave", "Sunstone Bridge", "Gleaming Spire", "Starlit Archive", "Crown of Dawn"],
            "Crystal guardians shine with blinding light.", "Aurelion the Shining blazes at the heart of the sanctum."),
        new("Blood Altar", CampaignSeedData.AltarArenaId, Id(241), Id(242), Id(243), Id(244), Id(245),
            ["Crimson Steps", "Bleeding Stones", "Hall of Offerings", "Red Pools", "Thorned Pillars",
             "Cutter's Alley", "Vein Tunnels", "Garnet Court", "Sacrificial Ring", "The Great Altar"],
            "Garnet cultists thirst for fresh blood.", "Vargul the Bloodbound stands over the great altar."),
        new("Lava Arena", CampaignSeedData.LavaArenaId, MagmaGolem, GoblinScout, GoblinShaman, EmberDrake, Id(255),
            ["Ashen Road", "Magma Falls", "Burning Bridge", "Slag Pits", "Obsidian Hall",
             "Ember Wastes", "Firebrand Camp", "Molten Vault", "Smelter's Hold", "Caldera Throne"],
            "Fire tribes guard the forges deeper under the mountain.", "Ignar the Ashborn rises from the caldera."),
        new("Ice Arena", CampaignSeedData.IceArenaId, Id(261), Id(262), Id(263), Id(264), Id(265),
            ["Snowblind Path", "Frostfang Gorge", "Frozen Lake", "Icicle Cave", "Hailstone Ridge",
             "Glacier Heart", "Whiteout Plains", "Rimehold Keep", "Avalanche Pass", "Peak of Winter"],
            "Frozen hunters stalk the white wastes.", "Kryssa the Winter Drake guards the peak.")
    ];

    // Набор шаблонов региона: id первого — first, дальше по порядку Tank, Melee, Caster, Elite, Boss
    private record Set(int First, HeroElement Element, string Immunity,
        string Tank, string Melee, string Caster, string Elite, string Boss, string BossType,
        int CasterSpell, int EliteSpell, int BossSpell, string BossAura);

    private static readonly Set[] Sets =
    [
        new(201, HeroElement.Lightning, "Shock", "Storm Golem", "Spark Goblin", "Thunder Skeleton", "Storm Drake",
            "Voltaris the Storm Drake", Draconid, CasterSpell: 4, EliteSpell: 25, BossSpell: 28, BossAura: "Shock"),
        new(211, HeroElement.Nature, "Poison", "Moss Golem", "Bog Goblin", "Thorn Skeleton", "Swamp Drake",
            "Mirefang the Rotting", Draconid, CasterSpell: 31, EliteSpell: 34, BossSpell: 32, BossAura: "Poison"),
        new(221, HeroElement.Dark, "Curse", "Grave Golem", "Shade Goblin", "Hex Skeleton", "Night Drake",
            "Morvath the Hollow", Skeleton, CasterSpell: 43, EliteSpell: 37, BossSpell: 55, BossAura: "Curse"),
        new(231, HeroElement.Radiance, "Stun", "Crystal Golem", "Prism Goblin", "Dawn Skeleton", "Radiant Drake",
            "Aurelion the Shining", Golem, CasterSpell: 52, EliteSpell: 49, BossSpell: 47, BossAura: "Regen"),
        new(241, HeroElement.Blood, "Bleed", "Flesh Golem", "Leech Goblin", "Bone Reaver", "Blood Drake",
            "Vargul the Bloodbound", Draconid, CasterSpell: 58, EliteSpell: 61, BossSpell: 59, BossAura: "Bleed"),
        new(261, HeroElement.Frost, "Frost", "Glacier Golem", "Snow Goblin", "Frost Skeleton", "Frost Drake",
            "Kryssa the Winter Drake", Draconid, CasterSpell: 16, EliteSpell: 19, BossSpell: 23, BossAura: "Frost")
    ];

    // Шаблоны монстров регионов. Статы — первого уровня врагов, дальше их растит EnemyLevelFor
    public static Hero[] Templates =>
    [
        .. Sets.SelectMany(SetHeroes),
        Monster(Id(255), "Ignar the Ashborn", Draconid, HeroElement.Fire, 1400, 120, 5, [11], ["Burn", "Stun"], aura: "Burn")
    ];

    private static IEnumerable<Hero> SetHeroes(Set set)
    {
        yield return Monster(Id(set.First), set.Tank, Golem, set.Element, 1000, 70, 3, [], [set.Immunity]);
        yield return Monster(Id(set.First + 1), set.Melee, Goblin, set.Element, 520, 85, 8, [], []);
        yield return Monster(Id(set.First + 2), set.Caster, Skeleton, set.Element, 500, 80, 6, [set.CasterSpell], [], startEnergy: 20);
        yield return Monster(Id(set.First + 3), set.Elite, Draconid, set.Element, 760, 95, 5, [set.EliteSpell], [set.Immunity]);
        yield return Monster(Id(set.First + 4), set.Boss, set.BossType, set.Element, 1400, 120, 5, [set.BossSpell], [set.Immunity, "Stun"],
            aura: set.BossAura);
    }

    private static Hero Monster(Guid id, string name, string type, HeroElement element, int hp, int attack, int speed,
        List<int> spells, List<string> immunities, int startEnergy = 0, string? aura = null)
    {
        var hero = new Hero
        {
            Id = id, PlayerId = CampaignSeedData.CampaignPlayerId,
            Name = name, UnitType = type, Element = element, Level = 4,
            MaxHp = hp, Attack = attack, Speed = speed,
            MaxEnergy = 100, StartEnergy = startEnergy, EnergyPerAttack = 20,
            SpellIds = spells, Immunities = immunities
        };
        if (aura != null && AuraBook.Find(aura) is { } definition)
        {
            hero.AuraEffect = definition.Effect;
            hero.AuraTarget = definition.Target;
            hero.AuraTrigger = TriggerPhase.OnRoundStart;
            hero.AuraPower = AuraBook.Power(definition, AuraBook.MinLevel);
            hero.AuraDuration = 1;
        }
        return hero;
    }

    public static Region RegionOf(int stageId) => Regions[((stageId - 1) / ChapterSize - 1) % Regions.Length];

    // Номер прохода по кругу регионов: «Thundering Steps II» — второй заход в Storm Peak
    private static int CycleOf(int stageId) => ((stageId - 1) / ChapterSize - 1) / Regions.Length + 1;

    private static readonly string[] Roman = ["I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"];

    public static IEnumerable<CampaignStage> Stages()
    {
        for (int id = HandcraftedStages + 1; id <= TotalStages; id++)
        {
            var region = RegionOf(id);
            int index = (id - 1) % ChapterSize;
            bool boss = index == ChapterSize - 1;
            int cycle = CycleOf(id);
            yield return new CampaignStage
            {
                Id = id,
                Name = $"{region.Places[index]} {Roman[Math.Min(cycle, Roman.Length) - 1]}",
                Description = boss ? region.BossText : region.MobsText,
                Type = boss ? StageType.Boss : StageType.Mobs,
                EnemyLevel = EnemyLevelFor(id),
                ArenaId = region.ArenaId
            };
        }
    }

    // Состав уровня: в начале главы трое, к середине четверо, к концу пятеро; на последнем — босс с охраной
    public static IEnumerable<CampaignStageEnemy> StageEnemies()
    {
        for (int id = HandcraftedStages + 1; id <= TotalStages; id++)
        {
            var region = RegionOf(id);
            int index = (id - 1) % ChapterSize;
            Guid[] line = index == ChapterSize - 1
                ? [region.Tank, region.Boss, region.Caster, region.Elite]
                : index switch
                {
                    < 3 => [region.Tank, region.Melee, region.Caster],
                    < 6 => [region.Tank, region.Melee, region.Caster, index % 2 == 0 ? region.Elite : region.Melee],
                    _ => [region.Tank, region.Melee, region.Caster, region.Elite, index % 2 == 0 ? region.Caster : region.Melee]
                };

            for (int position = 1; position <= line.Length; position++)
            {
                bool isBoss = line[position - 1] == region.Boss;
                yield return new CampaignStageEnemy
                {
                    Id = id * 100 + position,
                    StageId = id,
                    Position = position,
                    HeroId = line[position - 1],
                    IsBoss = isBoss,
                    HpMultiplier = isBoss ? BossHpMultiplier : 1
                };
            }
        }
    }
}
