// Экспедиции: герои из инвентаря (не из отряда) уходят на задание на несколько часов и приносят золото и опыт.
//
// Доска заданий общая для окна в BoardLifetime часов (00:00, 06:00, 12:00, 18:00 UTC): на ней по заданию
// каждой длительности из Durations. Награда растёт с длительностью медленнее, чем линейно, и линейно — с мощью героев.
public class ExpeditionService(
    IExpeditionRepository expeditions,
    IHeroesRepository heroes,
    IPlayerRepository players,
    IEntityFactory entityFactory,
    TimeProvider time
)
{
    public const int MaxHeroes = 4;
    public static readonly TimeSpan BoardLifetime = TimeSpan.FromHours(6);

    // Длительности заданий на доске по местам
    public static readonly int[] Durations = [1, 4, 16];

    // Шаблоны заданий: название и описание — на фронте (expeditions.templates.<key>)
    public static readonly string[] Templates =
    [
        "scoutTunnels", "goblinCamp", "lostMine", "caravanEscort",
        "trollBridge", "spiderNest", "deepVein", "ruinedForge"
    ];

    // Часы идут в награду в степени HourExponent, т.е. чем длиннее задание, тем меньше награда за час:
    // 1ч → ×1, 4ч → ×3.0, 16ч → ×9.2
    public const double HourExponent = 0.8;

    // За час «эталонного» часа: база + доля от мощи (отряд из 4 стартовых дворфов ≈ 800 мощи ≈ 100 золота и 50 опыта)
    public const double BaseGoldPerHour = 20;
    public const double GoldPerPowerPerHour = 0.1;
    public const double BaseExperiencePerHour = 10;
    public const double ExperiencePerPowerPerHour = 0.05;

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<ExpeditionBoard> GetBoardAsync(Guid playerId)
    {
        var now = Now;
        var boardStartsAt = BoardStart(now);

        var list = await expeditions.GetBoardAsync(playerId, boardStartsAt);
        if (!list.Any(e => e.BoardStartsAt == boardStartsAt))
        {
            // Если доску параллельно создал другой запрос — просто перечитываем её
            await expeditions.ReplaceBoardAsync(playerId, boardStartsAt, CreateOffers(playerId, boardStartsAt));
            list = await expeditions.GetBoardAsync(playerId, boardStartsAt);
        }

        return new ExpeditionBoard
        {
            ServerTime = now,
            RefreshAt = boardStartsAt + BoardLifetime,
            MaxHeroes = MaxHeroes,
            Expeditions = list.Select(e => ToInfo(e, now)).ToList()
        };
    }

    public async Task<ExpeditionInfo> StartAsync(Guid playerId, Guid expeditionId, IReadOnlyList<Guid> heroIds)
    {
        var now = Now;
        var expedition = await expeditions.GetAsync(playerId, expeditionId)
            ?? throw new InvalidOperationException("Expedition not found");

        if (expedition.Status != ExpeditionStatus.Available)
            throw new InvalidOperationException("Expedition is already started");
        if (expedition.BoardStartsAt != BoardStart(now))
            throw new InvalidOperationException("The board has been refreshed, this expedition is no longer available");

        if (heroIds.Count is 0 or > MaxHeroes)
            throw new InvalidOperationException($"Send from 1 to {MaxHeroes} heroes");
        if (heroIds.Distinct().Count() != heroIds.Count)
            throw new InvalidOperationException("The same hero can't take several places");

        var owned = (await heroes.GetByPlayerAsync(playerId)).ToDictionary(h => h.Id);
        var foreign = heroIds.Where(id => !owned.ContainsKey(id)).ToList();
        if (foreign.Count > 0)
            throw new InvalidOperationException($"Heroes not found: {string.Join(", ", foreign)}");

        var selected = heroIds.Select(id => owned[id]).ToList();
        if (selected.Any(h => h.PlaceInSquad != null))
            throw new InvalidOperationException("Heroes from the squad can't go on expeditions");

        var busy = await expeditions.GetBusyHeroIdsAsync(playerId);
        if (heroIds.Any(busy.Contains))
            throw new InvalidOperationException("One of the heroes is already on an expedition");

        int power = selected.Sum(h => CombatPower.OfHero(entityFactory, h).Total);
        var reward = RewardFor(expedition.DurationHours, power);

        expedition.Status = ExpeditionStatus.InProgress;
        expedition.StartedAt = now;
        expedition.EndsAt = now.AddHours(expedition.DurationHours);
        expedition.Power = power;
        expedition.Gold = reward.Gold;
        expedition.Experience = reward.Experience;
        expedition.Heroes = heroIds
            .Select((heroId, index) => new ExpeditionHero { ExpeditionId = expedition.Id, HeroId = heroId, Slot = index + 1 })
            .ToList();

        await expeditions.StartAsync(expedition);
        return ToInfo(expedition, now);
    }

    public async Task<ExpeditionClaimResult> ClaimAsync(Guid playerId, Guid expeditionId)
    {
        var now = Now;
        var expedition = await expeditions.GetAsync(playerId, expeditionId)
            ?? throw new InvalidOperationException("Expedition not found");

        if (expedition.Status == ExpeditionStatus.Available)
            throw new InvalidOperationException("Expedition is not started");
        if (expedition.Status == ExpeditionStatus.InProgress && expedition.EndsAt > now)
            throw new InvalidOperationException("Expedition is not finished yet");

        if (!await expeditions.ClaimAsync(expedition, now))
            throw new InvalidOperationException("Reward is already claimed");

        return new ExpeditionClaimResult
        {
            Reward = new ExpeditionReward { Gold = expedition.Gold, Experience = expedition.Experience },
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }

    // Начало окна доски, в которое попадает момент времени
    public static DateTime BoardStart(DateTime utc) =>
        new(utc.Ticks - utc.Ticks % BoardLifetime.Ticks, DateTimeKind.Utc);

    public static ExpeditionRates RatesFor(int hours)
    {
        double effectiveHours = Math.Pow(hours, HourExponent);
        return new ExpeditionRates
        {
            BaseGold = BaseGoldPerHour * effectiveHours,
            GoldPerPower = GoldPerPowerPerHour * effectiveHours,
            BaseExperience = BaseExperiencePerHour * effectiveHours,
            ExperiencePerPower = ExperiencePerPowerPerHour * effectiveHours
        };
    }

    // Округление как у Math.round на фронте, чтобы превью совпадало с начисленным
    public static ExpeditionReward RewardFor(int hours, int power)
    {
        var rates = RatesFor(hours);
        return new ExpeditionReward
        {
            Gold = (int)Math.Round(rates.BaseGold + rates.GoldPerPower * power, MidpointRounding.AwayFromZero),
            Experience = (int)Math.Round(rates.BaseExperience + rates.ExperiencePerPower * power, MidpointRounding.AwayFromZero)
        };
    }

    private static List<Expedition> CreateOffers(Guid playerId, DateTime boardStartsAt)
    {
        var templates = Templates.OrderBy(_ => Random.Shared.Next()).Take(Durations.Length).ToArray();
        return Durations
            .Select((hours, index) => new Expedition
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                BoardStartsAt = boardStartsAt,
                Slot = index + 1,
                TemplateKey = templates[index],
                DurationHours = hours
            })
            .ToList();
    }

    private static ExpeditionInfo ToInfo(Expedition expedition, DateTime now) => new()
    {
        Id = expedition.Id,
        Slot = expedition.Slot,
        TemplateKey = expedition.TemplateKey,
        DurationHours = expedition.DurationHours,
        Status = expedition.Status == ExpeditionStatus.InProgress && expedition.EndsAt <= now
            ? ExpeditionStatus.Ready
            : expedition.Status,
        StartedAt = AsUtc(expedition.StartedAt),
        EndsAt = AsUtc(expedition.EndsAt),
        Rates = RatesFor(expedition.DurationHours),
        Power = expedition.Power,
        Reward = new ExpeditionReward { Gold = expedition.Gold, Experience = expedition.Experience },
        HeroIds = expedition.Heroes.OrderBy(h => h.Slot).Select(h => h.HeroId).ToList()
    };

    // SQLite возвращает даты без Kind — без Utc JSON уйдёт без "Z", и браузер прочитает его как местное время
    private static DateTime? AsUtc(DateTime? value) =>
        value is { } date ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : null;
}
