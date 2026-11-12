// Правила «Гонки глубины» — недельного соревнования гильдий (вкладка гильдии под боссами).
//
// Неделя — с понедельника 00:00 UTC (как у ивентов, WorldEvents.WeekOf). Гонка идёт ActiveDuration (6 суток),
// последние сутки — итоги: места заморожены, награду уже можно забрать. Забрать награду прошлой недели можно
// и всю следующую неделю.
//
// Каждый участник ставит в шахту гильдии одного дворфа (не из отряда, не занятого). Дворф копает руду:
//   скорость — CommunityMineRules.DwarfRate (ранг, прокачка, рудокоп ×1.5): ранг 1 — 10 в час, рудокоп 4-го ранга — ~74;
//   кирка дворфа (личное улучшение, платит владелец) — +PickPerLevel за уровень, до ×2.08;
//   подъёмник (общее улучшение гильдии, платит любой участник) — +ShaftPerLevel за уровень, до ×2.12.
// Руда идёт сразу в два места:
//   очки гильдии — вся добытая руда (Mined), по ним рейтинг гильдий сервера;
//   склад владельца — руда меняется на золото при сборе. Склад вмещает StorageHours часов добычи: заходить надо,
//   но гильдия очки не теряет, даже если склад полон.
// Золото за руду и цены улучшений — в «уровнях кампании» игрока (DailyQuests.GoldFor): новичку и ветерану одинаково
// ощутимо. Базовый дворф приносит ~0.1 уровня кампании золота в час, рудокоп 4-го ранга со всеми улучшениями — ~3.
//
// Награда — значки почёта (валюта лавки гильдии) каждому, чей дворф накопал хоть что-то, по месту гильдии.
// Для сравнения: удары по боссам гильдии дают активному игроку ~1 000 значков в неделю
public static class GuildDepthRules
{
    public static readonly TimeSpan Week = WorldEvents.Week;
    public static readonly TimeSpan ActiveDuration = TimeSpan.FromDays(6);

    public static DateTime WeekOf(DateTime utc) => WorldEvents.WeekOf(utc);
    public static DateTime EndOf(DateTime weekStart) => weekStart + ActiveDuration;
    public static bool IsActive(DateTime weekStart, DateTime now) => now >= weekStart && now < EndOf(weekStart);

    /* ===== Добыча ===== */

    public const int MaxPickLevel = 10;
    public const double PickPerLevel = 0.12;
    public const int MaxShaftLevel = 10;
    public const double ShaftPerLevel = 0.125;

    public static double PickFactor(int level) => 1 + PickPerLevel * (Math.Clamp(level, 1, MaxPickLevel) - 1);
    public static double ShaftFactor(int level) => 1 + ShaftPerLevel * (Math.Clamp(level, 1, MaxShaftLevel) - 1);

    public static double RateOf(GuildDepthDwarf dwarf, int shaftLevel) =>
        dwarf.HeroId == null ? 0 : dwarf.BaseRate * PickFactor(dwarf.PickLevel) * ShaftFactor(shaftLevel);

    public const double StorageHours = 12;

    public static double StorageOf(GuildDepthDwarf dwarf, int shaftLevel) => RateOf(dwarf, shaftLevel) * StorageHours;

    // Досчитать добычу до min(now, конец гонки) по нынешней скорости. Склад не растёт выше вместимости, очки — растут
    public static void Settle(GuildDepthDwarf dwarf, int shaftLevel, DateTime now)
    {
        var until = now < EndOf(dwarf.WeekStart) ? now : EndOf(dwarf.WeekStart);
        if (until <= dwarf.UpdatedAt)
            return;
        double rate = RateOf(dwarf, shaftLevel);
        double mined = rate * (until - dwarf.UpdatedAt).TotalHours;
        dwarf.Mined += mined;
        dwarf.Stored = Math.Max(dwarf.Stored, Math.Min(StorageOf(dwarf, shaftLevel), dwarf.Stored + mined));
        dwarf.UpdatedAt = until;
    }

    // Золото за руду — в уровнях кампании: 100 руды — 1 уровень
    public const double GoldStagesPerOre = 0.01;

    /* ===== Улучшения (в уровнях кампании) ===== */

    public const double PickBaseStages = 1;
    public const double PickGrowth = 1.6;
    public const double ShaftBaseStages = 4;
    public const double ShaftGrowth = 1.55;

    // null — уровень максимальный. Кирка до потолка — ~110 уровней кампании, подъёмник — ~370 (на всю гильдию)
    public static double? PickCostStages(int level) =>
        level >= MaxPickLevel ? null : PickBaseStages * Math.Pow(PickGrowth, Math.Max(1, level) - 1);

    public static double? ShaftCostStages(int level) =>
        level >= MaxShaftLevel ? null : ShaftBaseStages * Math.Pow(ShaftGrowth, Math.Max(1, level) - 1);

    /* ===== Награда ===== */

    public const int LeaderboardSize = 20;

    // Значки почёта по месту гильдии (с 1-го)
    public static int HonorBadgesFor(int place) => place switch
    {
        1 => 600,
        2 => 480,
        3 => 400,
        <= 5 => 320,
        <= 10 => 250,
        _ => 160
    };
}
