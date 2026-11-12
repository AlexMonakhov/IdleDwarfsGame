public enum DelveNodeType { Fight, Elite, Camp, Treasure, Boss }

public record DelveNode(int Floor, int Column, DelveNodeType Type);

// Реликвия забега: усиливает весь отряд до конца забега (DelveService.ApplyRelics)
public record DelveRelic(string Key, bool Rare = false);

// «Глубинный спуск» — забег-рогалик по карте недели (концепт).
//
// Карта одна на всех на неделю (сид — неделя): Floors этажей по Columns узла. С узла в столбце c можно спуститься
// в столбцы c-1, c, c+1 следующего этажа. Первый этаж — бои, последний — один босс (в среднем столбце).
// Узлы: бой, элитный бой (сильнее, после победы — реликвия), привал (лечение или воскрешение), сокровищница (реликвия).
//
// Отряд — снимок на старте забега. HP героев сохраняется между боями, павшие не выходят до воскрешения.
// Пал весь отряд — забег окончен. Забег — один в сутки; в рейтинге недели — самый глубокий этаж.
//
// Враги — монстры уровня кампании возле текущего уровня игрока: на первом этаже чуть слабее (StageOffsetStart),
// с каждым этажом сильнее на StagePerFloor уровней. Награда — золото за каждую победу, в уровнях кампании
public static class DelveRules
{
    public const int Floors = 10;
    public const int Columns = 3;
    public const int BossColumn = 1;

    public const int StageOffsetStart = -8;
    public const int StagePerFloor = 2;
    public const int EliteStageBonus = 3;
    public const int BossStageBonus = 6;
    public const double EliteHp = 1.4;
    public const double BossHp = 2.2;

    // Привал: лечение живых или воскрешение одного павшего
    public const double CampHeal = 0.35;
    public const double CampRevive = 0.4;

    // Золото за победу: (база + за этаж) уровней кампании; элита и босс — больше
    public const double GoldStagesBase = 0.5;
    public const double GoldStagesPerFloor = 0.1;
    public const double EliteGold = 2;
    public const double BossGold = 5;

    public const int LeaderboardSize = 10;

    public static readonly DelveRelic[] Relics =
    [
        new("rage"),          // +15% атаки
        new("stoneskin"),     // +20% HP
        new("swiftboots"),    // +1 скорость
        new("bloodthirst"),   // после победы живые лечатся на 10%
        new("ale"),           // привал лечит ещё на 25%
        new("spark"),         // +40% энергии на старте боя
        new("giantslayer"),   // +30% атаки в боях с элитой и боссом
        new("phoenix", true)  // сразу воскрешает всех павших с 30% HP
    ];

    public const double RageAttack = 0.15;
    public const double StoneskinHp = 0.2;
    public const int SwiftSpeed = 1;
    public const double BloodthirstHeal = 0.1;
    public const double AleHeal = 0.25;
    public const double SparkEnergy = 0.4;
    public const double GiantslayerAttack = 0.3;
    public const double PhoenixHp = 0.3;
    public const int OfferSize = 3;

    public static DateTime WeekOf(DateTime utc) => WorldEvents.WeekOf(utc);
    public static DateTime DayOf(DateTime utc) => utc.Date;

    public static int SeedOf(DateTime week) => (int)(week.Ticks / TimeSpan.TicksPerDay);

    // Карта недели. Тип узла — по сиду; на 5-м этаже в среднем столбце всегда привал, чтобы было где перевести дух
    public static List<DelveNode> MapOf(DateTime week)
    {
        var random = new Random(SeedOf(week));
        var nodes = new List<DelveNode>();
        for (int floor = 1; floor <= Floors; floor++)
        {
            for (int column = 0; column < Columns; column++)
            {
                if (floor == Floors)
                {
                    if (column == BossColumn)
                        nodes.Add(new DelveNode(floor, column, DelveNodeType.Boss));
                    continue;
                }
                var type = floor == 1 ? DelveNodeType.Fight
                    : floor == 5 && column == 1 ? DelveNodeType.Camp
                    : Roll(random);
                nodes.Add(new DelveNode(floor, column, type));
            }
        }
        return nodes;
    }

    private static DelveNodeType Roll(Random random) => random.Next(100) switch
    {
        < 50 => DelveNodeType.Fight,
        < 65 => DelveNodeType.Elite,
        < 80 => DelveNodeType.Camp,
        _ => DelveNodeType.Treasure
    };

    // Можно ли с узла (floor, column) спуститься в узел next
    public static bool CanMove(int floor, int column, DelveNode next) =>
        next.Floor == floor + 1 && (floor == 0 || Math.Abs(next.Column - column) <= 1);

    public static int StageFor(int baseStage, DelveNode node, int maxStage)
    {
        int stage = baseStage + StageOffsetStart + StagePerFloor * (node.Floor - 1) + node.Type switch
        {
            DelveNodeType.Elite => EliteStageBonus,
            DelveNodeType.Boss => BossStageBonus,
            _ => 0
        };
        return Math.Clamp(stage, 1, Math.Max(1, maxStage));
    }

    public static double GoldStagesFor(DelveNode node) =>
        (GoldStagesBase + GoldStagesPerFloor * node.Floor) * node.Type switch
        {
            DelveNodeType.Elite => EliteGold,
            DelveNodeType.Boss => BossGold,
            _ => 1
        };

    // Три реликвии на выбор: без уже взятых, по сиду забега и этажа
    public static List<string> OfferFor(Guid runId, int floor, IReadOnlyCollection<string> owned)
    {
        // HashCode.Combine случаен в каждом процессе — сид собираем сами, чтобы выбор не менялся после перезапуска
        var random = new Random(BitConverter.ToInt32(runId.ToByteArray(), 0) ^ (floor * 7919));
        return Relics.Where(r => !owned.Contains(r.Key))
            .OrderBy(_ => random.Next())
            .Take(OfferSize)
            .Select(r => r.Key)
            .ToList();
    }
}
