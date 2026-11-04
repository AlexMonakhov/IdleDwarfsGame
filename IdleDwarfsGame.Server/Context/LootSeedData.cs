using IdleDwarfsGame.Server.Enums;

// Таблицы дропа сундуков (ChestDropTables + DropEntry). Как читать запись — см. DropEntry и ItemDropService:
// группы по ItemType бросаются Rolls раз, в группе выпадает одна запись с шансом Probability или ничего.
// Поменять дроп: правим здесь и добавляем миграцию (dotnet ef migrations add ...) — EF сам обновит строки в БД.
// Id записей выдаются по порядку, поэтому новые записи лучше дописывать в конец таблицы.
public static class LootSeedData
{
    // Сундуки каких уровней может выдать кампания (CampaignRewards.ChestLevelFor) — для них таблицы обязаны быть
    public const int MaxChestLevel = 5;

    // Камни стихий, из которых выпадают обычные гемы
    private static readonly GemType[] ElementGems = [GemType.Ruby, GemType.Sapphire, GemType.Topaz, GemType.Emerald];

    private static readonly TableDefinition[] Definitions =
    [
        // ===== Сундуки героев: два дворфа, ранг каждого — по таблице, стихия случайная.
        // 1-й ранг выпадает всегда: это материал для слияния и работники шахт =====
        new(ChestType.Hero, 1, Rolls: 2,
        [
            Hero(rank: 1, 0.75),
            Hero(rank: 2, 0.23),
            Hero(rank: 3, 0.02)
        ]),
        new(ChestType.Hero, 2, Rolls: 2,
        [
            Hero(rank: 1, 0.50),
            Hero(rank: 2, 0.40),
            Hero(rank: 3, 0.09),
            Hero(rank: 4, 0.01)
        ]),
        new(ChestType.Hero, 3, Rolls: 2,
        [
            Hero(rank: 1, 0.30),
            Hero(rank: 2, 0.45),
            Hero(rank: 3, 0.22),
            Hero(rank: 4, 0.03)
        ]),
        new(ChestType.Hero, 4, Rolls: 2,
        [
            Hero(rank: 1, 0.20),
            Hero(rank: 2, 0.40),
            Hero(rank: 3, 0.33),
            Hero(rank: 4, 0.07)
        ]),
        new(ChestType.Hero, 5, Rolls: 2,
        [
            Hero(rank: 1, 0.10),
            Hero(rank: 2, 0.35),
            Hero(rank: 3, 0.43),
            Hero(rank: 4, 0.12)
        ]),

        // ===== Сундуки гемов =====
        new(ChestType.Gem, 1, Rolls: 2,
        [
            .. ElementGems.Select(gem => Gem(gem, Rarity.Common, 0.22, 2, 4)),
            Gem(GemType.Diamond, Rarity.Rare, 0.05, 1, 1)
            // остаток 7% — пустой бросок
        ]),
        new(ChestType.Gem, 2, Rolls: 3,
        [
            .. ElementGems.Select(gem => Gem(gem, Rarity.Common, 0.18, 3, 6)),
            .. ElementGems.Select(gem => Gem(gem, Rarity.Uncommon, 0.05, 1, 2)),
            Gem(GemType.Diamond, Rarity.Rare, 0.06, 1, 1)
        ]),
        new(ChestType.Gem, 3, Rolls: 3,
        [
            .. ElementGems.Select(gem => Gem(gem, Rarity.Common, 0.12, 4, 8)),
            .. ElementGems.Select(gem => Gem(gem, Rarity.Uncommon, 0.09, 2, 3)),
            Gem(GemType.Diamond, Rarity.Rare, 0.12, 1, 2),
            Gem(GemType.Diamond, Rarity.Mythic, 0.04, 1, 1)
        ]),
        new(ChestType.Gem, 4, Rolls: 4,
        [
            .. ElementGems.Select(gem => Gem(gem, Rarity.Common, 0.10, 6, 12)),
            .. ElementGems.Select(gem => Gem(gem, Rarity.Uncommon, 0.10, 3, 5)),
            Gem(GemType.Diamond, Rarity.Rare, 0.14, 1, 3),
            Gem(GemType.Diamond, Rarity.Mythic, 0.06, 1, 1)
        ]),
        new(ChestType.Gem, 5, Rolls: 5,
        [
            .. ElementGems.Select(gem => Gem(gem, Rarity.Common, 0.08, 10, 20)),
            .. ElementGems.Select(gem => Gem(gem, Rarity.Uncommon, 0.11, 4, 8)),
            Gem(GemType.Diamond, Rarity.Rare, 0.16, 2, 4),
            Gem(GemType.Diamond, Rarity.Mythic, 0.08, 1, 2)
        ]),

        // ===== Сундуки снаряжения: шанс редкости делится поровну между предметами этой редкости =====
        new(ChestType.Equipment, 1, Rolls: 1,
        [
            .. EquipmentOf(Rarity.Common, 0.60),
            .. EquipmentOf(Rarity.Uncommon, 0.30),
            .. EquipmentOf(Rarity.Rare, 0.09),
            .. EquipmentOf(Rarity.Mythic, 0.01)
        ]),
        new(ChestType.Equipment, 2, Rolls: 1,
        [
            .. EquipmentOf(Rarity.Common, 0.30),
            .. EquipmentOf(Rarity.Uncommon, 0.40),
            .. EquipmentOf(Rarity.Rare, 0.22),
            .. EquipmentOf(Rarity.Mythic, 0.07),
            .. EquipmentOf(Rarity.Legendary, 0.01)
        ]),
        new(ChestType.Equipment, 3, Rolls: 1,
        [
            .. EquipmentOf(Rarity.Uncommon, 0.25),
            .. EquipmentOf(Rarity.Rare, 0.40),
            .. EquipmentOf(Rarity.Mythic, 0.25),
            .. EquipmentOf(Rarity.Legendary, 0.09),
            .. EquipmentOf(Rarity.Artifact, 0.01)
        ]),
        new(ChestType.Equipment, 4, Rolls: 1,
        [
            .. EquipmentOf(Rarity.Rare, 0.35),
            .. EquipmentOf(Rarity.Mythic, 0.40),
            .. EquipmentOf(Rarity.Legendary, 0.20),
            .. EquipmentOf(Rarity.Artifact, 0.05)
        ]),
        new(ChestType.Equipment, 5, Rolls: 1,
        [
            .. EquipmentOf(Rarity.Mythic, 0.45),
            .. EquipmentOf(Rarity.Legendary, 0.40),
            .. EquipmentOf(Rarity.Artifact, 0.15)
        ])
    ];

    public static Guid TableId(ChestType type, int level) =>
        Guid.Parse($"c4e50000-0000-4000-8000-{(int)type:D6}{level:D6}");

    public static ChestDropTable[] Tables => Definitions
        .Select(d => new ChestDropTable { Id = TableId(d.Type, d.Level), ChestType = d.Type, ChestLevel = d.Level, Rolls = d.Rolls })
        .ToArray();

    // Записи с внешним ключом на таблицу: для HasData у owned-сущностей ключ владельца задаётся явно
    public static object[] Entries => Definitions
        .SelectMany(d => d.Entries.Select((e, index) => (object)new
        {
            Id = Guid.Parse($"c4e50001-0000-4000-8000-{(int)d.Type:D4}{d.Level:D4}{index:D4}"),
            ChestDropTableId = TableId(d.Type, d.Level),
            e.Probability,
            e.MinAmount,
            e.MaxAmount,
            e.ItemType,
            e.SubType,
            e.Rarity,
            e.ItemLevel
        }))
        .ToArray();

    // Те же таблицы целиком, без БД — для тестов и проверки баланса
    public static List<ChestDropTable> TablesWithEntries() => Definitions
        .Select(d => new ChestDropTable
        {
            Id = TableId(d.Type, d.Level),
            ChestType = d.Type,
            ChestLevel = d.Level,
            Rolls = d.Rolls,
            Entries = d.Entries
        })
        .ToList();

    // Ранг героя задаёт и редкость записи — фронт красит выпавшего героя по ней
    private static DropEntry Hero(int rank, double probability) => new()
    {
        ItemType = ItemType.Hero,
        ItemLevel = rank,
        Rarity = (Rarity)Math.Clamp(rank - 1, (int)Rarity.Common, (int)Rarity.Legendary),
        Probability = probability,
        MinAmount = 1,
        MaxAmount = 1
    };

    private static DropEntry Gem(GemType gem, Rarity rarity, double probability, int min, int max) => new()
    {
        ItemType = ItemType.Gem,
        SubType = gem.ToString(),
        Rarity = rarity,
        ItemLevel = 1,
        Probability = probability,
        MinAmount = min,
        MaxAmount = max
    };

    // Предмет из каталога: SubType — его ключ (EquipmentCatalog)
    private static IEnumerable<DropEntry> EquipmentOf(Rarity rarity, double totalProbability)
    {
        var items = EquipmentCatalog.All.Where(i => i.Rarity == rarity).ToList();
        return items.Select(item => new DropEntry
        {
            ItemType = ItemType.Equipment,
            SubType = item.Key,
            Rarity = rarity,
            ItemLevel = 1,
            Probability = totalProbability / items.Count,
            MinAmount = 1,
            MaxAmount = 1
        });
    }

    private record TableDefinition(ChestType Type, int Level, int Rolls, DropEntry[] Entries);
}
