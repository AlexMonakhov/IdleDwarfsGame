using System.Text.Json;
using System.Text.Json.Serialization;

// Обычная арена — отряды аккаунтов как есть; рейтинговая — закреплённые снимки отрядов (RankedSquad).
// Бои и очки двух арен хранятся в одних таблицах, но друг с другом не смешиваются
public enum PvpMode { Casual, Ranked }

// Бой PvP: нападавший против сохранённого отряда защитника (защитник в бою не участвует).
// Победы за сутки (Day — 00:00 UTC) убирают защитника из подбора нападавшего до конца суток:
// уникальный индекс (AttackerUserId, DefenderUserId, Day, Mode) по победам не даст победить его дважды
public class PvpBattle
{
    public Guid Id { get; set; }
    public PvpMode Mode { get; set; }
    public Guid AttackerUserId { get; set; }
    public Guid DefenderUserId { get; set; }
    public DateTime FoughtAt { get; set; }
    public DateTime Day { get; set; }
    public bool AttackerWon { get; set; }

    // Мощь отрядов на момент боя — по ним считались очки обычной арены
    public int AttackerPower { get; set; }
    public int DefenderPower { get; set; }

    // Рейтинг сторон до боя — по нему считались очки рейтинговой арены (в обычной — 0)
    public int AttackerRating { get; set; }
    public int DefenderRating { get; set; }

    // Изменение очков нападавшего во всех сезонах (поражение — отрицательное)
    public int Points { get; set; }
}

// Очки игрока в одном сезоне одной арены. Ключ (UserId, Mode, Type, Start): новый сезон — новая строка.
// Обычная арена начинает сезон с нуля, рейтинговая — с RankedArena.StartRating. Очки не опускаются ниже нуля
public class PvpSeasonScore
{
    public Guid UserId { get; set; }
    public PvpMode Mode { get; set; }
    public PvpSeasonType Type { get; set; }
    public DateTime Start { get; set; }

    public int Points { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    // При равных очках выше тот, кто набрал их раньше
    public DateTime UpdatedAt { get; set; }
}

// Отряд игрока на рейтинговой арене — снимок героев на момент закрепления: прокачка и перестановки
// в отряде аккаунта на него не влияют. Перезакрепить можно раз в сутки (RankedArena.CanChangeAt)
public class RankedSquad
{
    public Guid UserId { get; set; }
    public DateTime LockedAt { get; set; }

    // Герои по порядку мест, JSON. Енамы строками — как и в таблице героев, чтобы порядок значений можно было менять
    public string HeroesJson { get; set; } = "[]";

    private static readonly JsonSerializerOptions Json = new() { Converters = { new JsonStringEnumConverter() } };

    public List<Hero> Heroes() => JsonSerializer.Deserialize<List<Hero>>(HeroesJson, Json) ?? [];

    public static string Serialize(IEnumerable<Hero> heroes) => JsonSerializer.Serialize(heroes, Json);
}
