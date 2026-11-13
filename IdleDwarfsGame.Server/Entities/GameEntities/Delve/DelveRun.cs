using System.Text.Json;
using System.Text.Json.Serialization;

// Dead — пал весь отряд: забег окончен, но его можно один раз воскресить (DelveRules.ReviveCost)
public enum DelveRunStatus { Active, Dead, Retreated }

// Что ждёт решения игрока, прежде чем идти дальше: реликвия из трёх, привал, награда узла
public enum DelvePending { None, Relic, Camp, Reward }

// Забег «Глубинного спуска» (DelveRules). Отряд свой у забега (начинается с секироносцев 1-го ранга) и хранится
// здесь же JSON-ом вместе с полученными наградами: стихиями, спеллами, предметами, пассивками, аурами.
// Доля HP каждого героя (0 — пал) сохраняется между боями, реликвии усиливают весь отряд.
// Карта не хранится — её считают по Seed. Ключ — Id; один забег на аккаунт за период — уникальный индекс (UserId, PeriodStart)
public class DelveRun
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime PeriodStart { get; set; }
    public int Seed { get; set; }

    // Последний пройденный узел карты: этаж 0 — вход (ещё никуда не ходили)
    public int Floor { get; set; }
    public int Column { get; set; }
    public DelveRunStatus Status { get; set; }
    public DelvePending Pending { get; set; }
    // Воскрешение всего отряда уже использовано
    public bool Revived { get; set; }

    public string SquadJson { get; set; } = "[]";
    // HeroId → доля HP 0..1
    public string HpJson { get; set; } = "{}";
    public string RelicsJson { get; set; } = "[]";
    // Реликвии на выбор (Pending == Relic)
    public string OfferJson { get; set; } = "[]";
    // Награды на выбор (Pending == Reward)
    public string RewardsJson { get; set; } = "[]";

    public long GoldEarned { get; set; }
    public long ExperienceEarned { get; set; }
    public int RainbowCoinsEarned { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public int Version { get; set; }

    private static readonly JsonSerializerOptions Json = new() { Converters = { new JsonStringEnumConverter() } };

    public List<Hero> Squad() => (JsonSerializer.Deserialize<List<Hero>>(SquadJson, Json) ?? []).OrderBy(h => h.PlaceInSquad).ToList();
    public void SetSquad(IEnumerable<Hero> squad) => SquadJson = JsonSerializer.Serialize(squad, Json);
    public Dictionary<Guid, double> Hp() => JsonSerializer.Deserialize<Dictionary<Guid, double>>(HpJson) ?? new();
    public void SetHp(Dictionary<Guid, double> hp) => HpJson = JsonSerializer.Serialize(hp);
    public List<string> Relics() => JsonSerializer.Deserialize<List<string>>(RelicsJson) ?? [];
    public void SetRelics(List<string> relics) => RelicsJson = JsonSerializer.Serialize(relics);
    public List<string> Offer() => JsonSerializer.Deserialize<List<string>>(OfferJson) ?? [];
    public void SetOffer(List<string> offer) => OfferJson = JsonSerializer.Serialize(offer);
    public List<DelveReward> Rewards() => JsonSerializer.Deserialize<List<DelveReward>>(RewardsJson, Json) ?? [];
    public void SetRewards(List<DelveReward> rewards) => RewardsJson = JsonSerializer.Serialize(rewards, Json);
}
