using IdleDwarfsGame.Server.Enums;

// Таблицы дропа сундуков (ChestDropTables + DropEntry). Как читать запись — см. DropEntry и ItemDropService:
// группы по ItemType бросаются Rolls раз, в группе выпадает одна запись с шансом Probability или ничего.
// Поменять дроп: правим здесь и добавляем миграцию (dotnet ef migrations add ...) — EF сам обновит строки в БД.
// Id записей выдаются по порядку, поэтому новые записи лучше дописывать в конец таблицы.
public static class LootSeedData
{
    // Сундуки каких уровней может выдать кампания (CampaignRewards.ChestLevelFor) — для них таблицы обязаны быть
    public const int MaxChestLevel = 3;

    // Камни стихий, из которых выпадают обычные гемы
    private static readonly GemType[] ElementGems = [GemType.Ruby, GemType.Sapphire, GemType.Topaz, GemType.Emerald];

    private static readonly TableDefinition[] Definitions =
    [
        // ===== Сундуки героев: ранг героя (Hero.Level), стихия случайная =====
        new(ChestType.Hero, 1, Rolls: 1,
        [
            Hero(rank: 1, 0.80),
            Hero(rank: 2, 0.20)
        ]),
        new(ChestType.Hero, 2, Rolls: 1,
        [
            Hero(rank: 1, 0.50),
            Hero(rank: 2, 0.40),
            Hero(rank: 3, 0.10)
        ]),
        new(ChestType.Hero, 3, Rolls: 1,
        [
            Hero(rank: 2, 0.50),
            Hero(rank: 3, 0.40),
            Hero(rank: 4, 0.10)
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
            Gem(GemType.Diamond, Rarity.Epic, 0.04, 1, 1)
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

    private record TableDefinition(ChestType Type, int Level, int Rolls, DropEntry[] Entries);
}
