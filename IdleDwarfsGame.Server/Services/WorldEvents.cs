// Ивенты для всех аккаунтов сервера. Каждую неделю (с понедельника 00:00 UTC) идёт один ивент, виды чередуются:
// на неделе RotationStart — общие шахты, на следующей — мировые боссы и т.д.
// Ивент активен ActiveDuration (шесть суток): в это время копится его валюта. Последние сутки недели — только лавка ивента,
// чтобы потратить накопленное. С началом следующей недели остаток валюты сгорает.
// Расписание считается формулой, строк ивента в БД нет: ключ ивента — момент его начала (StartsAt)
public enum WorldEventKind { CommunityMines, WorldBosses }

public record WorldEventWindow(WorldEventKind Kind, DateTime StartsAt, DateTime EndsAt, DateTime ShopClosesAt)
{
    public bool IsActive(DateTime now) => now >= StartsAt && now < EndsAt;
    public bool IsShopOpen(DateTime now) => now >= StartsAt && now < ShopClosesAt;
}

public static class WorldEvents
{
    public static readonly DateTime RotationStart = new(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc);

    public static readonly TimeSpan Week = TimeSpan.FromDays(7);
    public static readonly TimeSpan ActiveDuration = TimeSpan.FromDays(6);

    public static readonly WorldEventKind[] Rotation = [WorldEventKind.CommunityMines, WorldEventKind.WorldBosses];

    public static DateTime WeekOf(DateTime utc) => PvpSeasons.StartOf(PvpSeasonType.Weekly, utc);

    // Сколько недель прошло с RotationStart (до неё — отрицательное число)
    public static int WeekIndex(DateTime week) => (int)Math.Floor((WeekOf(week) - RotationStart).TotalDays / 7);

    // Ивент недели, в которую попадает момент: активный, с открытой лавкой или (для моментов до начала) будущий
    public static WorldEventWindow Of(DateTime utc)
    {
        var start = WeekOf(utc);
        int index = WeekIndex(start);
        var kind = Rotation[((index % Rotation.Length) + Rotation.Length) % Rotation.Length];
        return new WorldEventWindow(kind, start, start + ActiveDuration, start + Week);
    }

    // Ивент, который идёт сейчас; null — сейчас день лавки
    public static WorldEventWindow? ActiveAt(DateTime utc)
    {
        var window = Of(utc);
        return window.IsActive(utc) ? window : null;
    }

    public static WorldEventWindow Next(DateTime utc) => Of(WeekOf(utc) + Week);
}
