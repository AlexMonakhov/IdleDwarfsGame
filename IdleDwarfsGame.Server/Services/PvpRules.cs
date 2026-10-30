// Арена, на которой идут все бои PvP. В БД её нет: эффект у неё свой (RisingFuryArenaEffect), а не статус из StatusEffectFactory
public static class PvpArena
{
    public const string Name = "Arena of Rising Fury";
    public const string Description = "Every 5 rounds the crowd roars louder: all damage dealt by every fighter grows by 10%, and the bonus stacks.";
    // Ключ фона на фронте (arenaBackgrounds.ts)
    public const string Background = "colosseum";

    public const int IntervalRounds = 5;
    public const int BonusPercent = 10;

    // Лимит раундов: к нему урон вырастает почти вдвое, так что до него бои почти не доживают.
    // Дожили — поражение нападавшего, как и в других режимах
    public const int FightRounds = 50;
}

public enum PvpSeasonType { Daily, Weekly, Monthly }

// Сезоны идут параллельно: каждый бой меняет очки сразу во всех трёх. Границы — по UTC:
// день с 00:00, неделя с понедельника, месяц с 1-го числа
public static class PvpSeasons
{
    public static readonly PvpSeasonType[] All = [PvpSeasonType.Daily, PvpSeasonType.Weekly, PvpSeasonType.Monthly];

    public static DateTime DayOf(DateTime utc) => DateTime.SpecifyKind(utc.Date, DateTimeKind.Utc);

    public static DateTime StartOf(PvpSeasonType type, DateTime utc)
    {
        var day = DayOf(utc);
        return type switch
        {
            PvpSeasonType.Daily => day,
            // DayOfWeek: воскресенье — 0, а неделя начинается с понедельника
            PvpSeasonType.Weekly => day.AddDays(-(((int)day.DayOfWeek + 6) % 7)),
            PvpSeasonType.Monthly => new DateTime(day.Year, day.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static DateTime EndOf(PvpSeasonType type, DateTime start) => type switch
    {
        PvpSeasonType.Daily => start.AddDays(1),
        PvpSeasonType.Weekly => start.AddDays(7),
        PvpSeasonType.Monthly => start.AddMonths(1),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}

// Очки сезона за бой. Меняются только у нападавшего: защита идёт без участия игрока.
// Цена зависит от соотношения мощи r = мощь соперника / мощь нападавшего:
// победа над сильным даёт больше (WinPoints × r²), поражение от слабого снимает больше (LossPoints / r²).
// При равной мощи +20 / −15, на краях обычного окна подбора (±10%) — +24…+16 и −12…−19.
// Рейтинговая арена считает по той же формуле, только вместо мощи подставляет рейтинг (±5% — +22…+18 и −14…−17)
public static class PvpRating
{
    public const int WinPoints = 20;
    public const int LossPoints = 15;
    public const int MinPoints = 5;
    public const int MaxPoints = 60;

    public static int WinFor(int attackerPower, int defenderPower) =>
        Clamp(WinPoints * Ratio(attackerPower, defenderPower) * Ratio(attackerPower, defenderPower));

    public static int LossFor(int attackerPower, int defenderPower) =>
        Clamp(LossPoints / (Ratio(attackerPower, defenderPower) * Ratio(attackerPower, defenderPower)));

    // Изменение очков нападавшего: положительное за победу, отрицательное за поражение
    public static int DeltaFor(bool won, int attackerPower, int defenderPower) =>
        won ? WinFor(attackerPower, defenderPower) : -LossFor(attackerPower, defenderPower);

    private static double Ratio(int attackerPower, int defenderPower) =>
        Math.Max(1, defenderPower) / (double)Math.Max(1, attackerPower);

    private static int Clamp(double points) =>
        Math.Clamp((int)Math.Round(points, MidpointRounding.AwayFromZero), MinPoints, MaxPoints);
}

// Рейтинговая арена: та же арена нарастающей ярости, но отряды — закреплённые снимки (RankedSquad),
// а соперники подбираются по рейтингу, а не по мощи. Очки за бой — по формуле PvpRating от соотношения рейтингов
public static class RankedArena
{
    // С него начинается каждый сезон: от нуля окно в ±5% не работало бы
    public const int StartRating = 1000;
    public const int StartSpreadPercent = 5;

    // Подбор — по рейтингу месячного сезона: он копится дольше всех и честнее показывает силу игрока
    public const PvpSeasonType MatchSeason = PvpSeasonType.Monthly;

    // Отряд закрепляется в любой момент, а перезакрепить его можно только в следующие сутки (00:00 UTC)
    public static DateTime CanChangeAt(DateTime lockedAt) => PvpSeasons.DayOf(lockedAt).AddDays(1);
}

// Подбор соперников: OpponentCount игроков, чья мощь (или рейтинг) отличается от нашей не больше чем на стартовое окно.
// Не набралось — окно расширяется по SpreadStepPercent, пока не наберётся (или пока не кончатся игроки).
// Если в окно попало больше игроков, чем нужно, выбираются случайные — но с сидом от игрока и суток,
// чтобы список не менялся от перезахода в окно
public static class PvpMatchmaking
{
    public const int OpponentCount = 5;
    public const int StartSpreadPercent = 10;
    public const int SpreadStepPercent = 1;

    // Насколько мощь соперника отличается от нашей, в процентах от нашей (со знаком: + — соперник сильнее)
    public static double DiffPercent(int myPower, int theirPower) =>
        (theirPower - myPower) * 100.0 / Math.Max(1, myPower);

    // Итог подбора: соперники и окно, на котором их набрали
    public record Pick<T>(List<T> Opponents, int SpreadPercent);

    public static Pick<T> Choose<T>(IEnumerable<T> candidates, Func<T, Guid> id, Func<T, int> power, int myPower, int seed,
        int startSpreadPercent = StartSpreadPercent)
    {
        var withDiff = candidates
            .OrderBy(id)
            .Select(c => (Candidate: c, Diff: Math.Abs(DiffPercent(myPower, power(c)))))
            .ToList();
        if (withDiff.Count == 0)
            return new Pick<T>([], startSpreadPercent);

        // Пошаговое расширение окна по 1% до первого, в котором OpponentCount игроков, —
        // это шаг, накрывающий OpponentCount-го по близости соперника
        var nearest = withDiff.Select(c => c.Diff).Order().ElementAt(Math.Min(OpponentCount, withDiff.Count) - 1);
        int spread = startSpreadPercent;
        if (nearest > spread)
            spread += (int)Math.Ceiling((nearest - spread) / SpreadStepPercent) * SpreadStepPercent;

        var random = new Random(seed);
        var opponents = withDiff
            .Where(c => c.Diff <= spread)
            .OrderBy(_ => random.Next())
            .Take(OpponentCount)
            .OrderBy(c => c.Diff)
            .Select(c => c.Candidate)
            .ToList();
        return new Pick<T>(opponents, spread);
    }

    // Сид не зависит от процесса (string.GetHashCode в .NET случайный на каждый запуск)
    public static int SeedFor(Guid userId, DateTime day) =>
        BitConverter.ToInt32(userId.ToByteArray(), 0) ^ (int)(day.Ticks / TimeSpan.TicksPerDay);
}
