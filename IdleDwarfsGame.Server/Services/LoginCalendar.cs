using IdleDwarfsGame.Server.Enums;

// Награда дня календаря. Золото и опыт — в «уровнях кампании», как у ежедневных заданий (DailyQuests.GoldFor);
// сундуки — уровня текущей главы кампании
public record LoginDayReward(double GoldStages, int RainbowCoins, int ElderSeals, ChestType[] Chests)
{
    // Последний день недели — крупная награда, фронт выделяет его
    public bool Big { get; init; }
}

// Где игрок в календаре на сегодня
public record LoginCalendarPosition(
    // День цикла, который можно забрать сегодня (или уже забранный сегодня), 1..CycleLength
    int Day,
    bool ClaimedToday,
    // Сколько дней цикла уже забрано (с учётом сегодняшнего)
    int ClaimedDays,
    // Цикл, к которому относится Day
    int Cycle,
    // Серия с учётом сегодня: если сегодня ещё не забрано — какой она станет после получения
    int Streak,
    // Серию оборвал пропуск: календарь начался заново
    bool Broken);

// Календарь входов («месячный пропуск»): каждые сутки (00:00 UTC) можно забрать награду следующего дня цикла.
// Цикл — CycleLength дней, награды растут к концу каждой недели и к концу цикла. Пропустил сутки — цикл и серия
// начинаются с первого дня (MissedDaysAllowed — сколько суток пропуска прощается). Забрал последний день —
// на следующие сутки начинается новый цикл с первого дня.
//
// Доход за полный цикл: радужные монеты 10 + 15 + 25 + 50 = 100, печати 5+10+10+15+15+20+20+25 = 120 и 18 сундуков
public static class LoginCalendar
{
    public const int CycleLength = 28;

    // 0 — строго подряд; 1 — прощается один пропущенный день и т.д.
    public const int MissedDaysAllowed = 0;

    private static readonly ChestType[] None = [];

    public static readonly LoginDayReward[] Days =
    [
        // Неделя 1
        new(2, 0, 0, None),
        new(0, 0, 5, None),
        new(3, 0, 0, None),
        new(0, 0, 0, [ChestType.Gem]),
        new(0, 0, 10, None),
        new(4, 0, 0, None),
        new(0, 10, 0, [ChestType.Hero]) { Big = true },
        // Неделя 2
        new(4, 0, 0, None),
        new(0, 0, 10, None),
        new(0, 0, 0, [ChestType.Equipment]),
        new(5, 0, 0, None),
        new(0, 0, 15, None),
        new(0, 0, 0, [ChestType.Gem]),
        new(0, 15, 0, [ChestType.Hero, ChestType.Equipment]) { Big = true },
        // Неделя 3
        new(6, 0, 0, None),
        new(0, 0, 15, None),
        new(0, 0, 0, [ChestType.Gem, ChestType.Gem]),
        new(7, 0, 0, None),
        new(0, 0, 20, None),
        new(0, 0, 0, [ChestType.Equipment]),
        new(0, 25, 0, [ChestType.Hero, ChestType.Hero]) { Big = true },
        // Неделя 4
        new(8, 0, 0, None),
        new(0, 0, 20, None),
        new(0, 0, 0, [ChestType.Gem, ChestType.Equipment]),
        new(10, 0, 0, None),
        new(0, 0, 25, None),
        new(0, 0, 0, [ChestType.Hero]),
        new(0, 50, 0, [ChestType.Hero, ChestType.Hero, ChestType.Equipment, ChestType.Equipment]) { Big = true }
    ];

    public static LoginDayReward RewardOf(int day) => Days[day - 1];

    // Продолжает ли сегодняшний вход серию после последней награды
    public static bool Continues(DateTime? lastClaimDay, DateTime today) =>
        lastClaimDay is { } last && (today - last).TotalDays <= 1 + MissedDaysAllowed;

    public static LoginCalendarPosition PositionOf(PlayerLoginCalendar? state, DateTime today)
    {
        if (state?.LastClaimDay is not { } last)
            return new LoginCalendarPosition(1, false, 0, 1, 1, false);

        if (last >= today)
            return new LoginCalendarPosition(state.Day, true, state.Day, state.Cycle, state.Streak, false);

        if (!Continues(last, today))
            return new LoginCalendarPosition(1, false, 0, state.Cycle + 1, 1, true);

        // Цикл закончен — сегодня первый день нового
        return state.Day >= CycleLength
            ? new LoginCalendarPosition(1, false, 0, state.Cycle + 1, state.Streak + 1, false)
            : new LoginCalendarPosition(state.Day + 1, false, state.Day, state.Cycle, state.Streak + 1, false);
    }

    // Состояние после получения награды сегодня (position — ещё не забранный день)
    public static PlayerLoginCalendar AfterClaim(Guid playerId, PlayerLoginCalendar? state, LoginCalendarPosition position, DateTime today) => new()
    {
        PlayerId = playerId,
        Cycle = position.Cycle,
        Day = position.Day,
        LastClaimDay = today,
        Streak = position.Streak,
        BestStreak = Math.Max(state?.BestStreak ?? 0, position.Streak),
        TotalDays = (state?.TotalDays ?? 0) + 1,
        Version = (state?.Version ?? 0) + 1
    };
}
