using System.Text.Json;
using System.Text.Json.Serialization;

public enum DelveRunStatus { Active, Dead, Completed, Retreated }

// Что ждёт решения игрока, прежде чем идти дальше: выбрать реликвию из трёх или решить, что делать на привале
public enum DelvePending { None, Relic, Camp }

// Забег «Глубинного спуска» (DelveRules). Отряд — снимок на старте (как у рейтинговой арены): прокачка и перестановки
// во время забега на него не влияют, а герои не заняты — ими можно пользоваться где угодно.
// Своё состояние отряда: доля HP каждого героя (0 — пал) сохраняется между боями, реликвии усиливают весь отряд.
// Ключ — Id; один забег на аккаунт в сутки — уникальный индекс (UserId, Day)
public class DelveRun
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime WeekStart { get; set; }
    public DateTime Day { get; set; }

    // Последний пройденный узел карты: этаж 0 — вход (ещё никуда не ходили)
    public int Floor { get; set; }
    public int Column { get; set; }
    public DelveRunStatus Status { get; set; }
    public DelvePending Pending { get; set; }

    public string SquadJson { get; set; } = "[]";
    // HeroId → доля HP 0..1
    public string HpJson { get; set; } = "{}";
    public string RelicsJson { get; set; } = "[]";
    // Реликвии на выбор (Pending == Relic)
    public string OfferJson { get; set; } = "[]";

    public long GoldEarned { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public int Version { get; set; }

    private static readonly JsonSerializerOptions Json = new() { Converters = { new JsonStringEnumConverter() } };

    public List<Hero> Squad() => JsonSerializer.Deserialize<List<Hero>>(SquadJson, Json) ?? [];
    public Dictionary<Guid, double> Hp() => JsonSerializer.Deserialize<Dictionary<Guid, double>>(HpJson) ?? new();
    public void SetHp(Dictionary<Guid, double> hp) => HpJson = JsonSerializer.Serialize(hp);
    public List<string> Relics() => JsonSerializer.Deserialize<List<string>>(RelicsJson) ?? [];
    public void SetRelics(List<string> relics) => RelicsJson = JsonSerializer.Serialize(relics);
    public List<string> Offer() => JsonSerializer.Deserialize<List<string>>(OfferJson) ?? [];
    public void SetOffer(List<string> offer) => OfferJson = JsonSerializer.Serialize(offer);
}
