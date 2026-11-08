// Причина отказа — фронт показывает по коду переведённый текст (communityMines.errors.<code>)
public enum CommunityMineError
{
    // Сейчас не неделя общих шахт или ивент уже кончился (день лавки)
    EventNotActive,
    DepositNotFound,
    // Месторождение выработано или закрыто концом ивента
    DepositClosed,
    NoHeroes,
    HeroNotFound,
    HeroInSquad,
    // Дворф в экспедиции, в личной шахте или на другом месторождении
    HeroBusy,
    // На медь — только дворфы до CommunityMineRules.CopperMaxRank ранга
    RankTooHigh,
    TooManyDwarves,
    NotYourDwarf,
    MaxLevel,
    // Игрок уже купил CommunityMineRules.MaxUpgradesPerPlayer уровней этого месторождения
    UpgradeLimit,
    NotEnoughGold,
    NothingToClaim,
    Changed
}

public class CommunityMineException(CommunityMineError code, string message) : InvalidOperationException(message)
{
    public CommunityMineError Code { get; } = code;
}

// Ивент «Общие шахты» (правила — CommunityMineRules). Месторождения создаются по расписанию первым запросом
// (и фоновой задачей WorldEventsTicker), закрываются, когда выработаны или кончился ивент. Каждое изменение месторождения
// сначала засчитывает добытое по старому составу, потом меняет состав — так доли игроков копятся честно
public class CommunityMineService(
    ICommunityMineRepository mines,
    IHeroesRepository heroes,
    IExpeditionRepository expeditions,
    IPlayerRepository players,
    IActivityRepository activity,
    IFixedShopRepository wallets,
    TimeProvider clock)
{
    private const int Attempts = 3;

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    private static bool IsMinesWeek(WorldEventWindow window) => window.Kind == WorldEventKind.CommunityMines;

    public async Task<CommunityMinesOverview> GetAsync(Guid playerId)
    {
        var now = Now;
        var window = WorldEvents.Of(now);
        if (IsMinesWeek(window))
            await TickAsync(window, now);
        return await DescribeAsync(playerId, window, now);
    }

    // Фоновая задача: новые месторождения по расписанию и закрытие выработанных — без захода игроков
    public async Task TickAsync()
    {
        var now = Now;
        var window = WorldEvents.Of(now);
        if (IsMinesWeek(window))
            await TickAsync(window, now);
    }

    public async Task<CommunityMinesOverview> SendAsync(Guid playerId, Guid depositId, IReadOnlyList<Guid>? heroIds)
    {
        var now = Now;
        var window = RequireOpenWindow(now);
        if (heroIds == null || heroIds.Count == 0)
            throw new CommunityMineException(CommunityMineError.NoHeroes, "Choose dwarves to send");
        if (heroIds.Distinct().Count() != heroIds.Count)
            throw new CommunityMineException(CommunityMineError.NoHeroes, "The same dwarf can't be sent twice");

        var owned = (await heroes.GetByPlayerAsync(playerId)).ToDictionary(h => h.Id);
        var selected = heroIds.Select(id => owned.GetValueOrDefault(id)).ToList();
        if (selected.Any(h => h == null))
            throw new CommunityMineException(CommunityMineError.HeroNotFound, "Hero not found");
        if (selected.Any(h => h!.PlaceInSquad != null))
            throw new CommunityMineException(CommunityMineError.HeroInSquad, "Heroes from the squad can't work in a mine");

        var busy = await expeditions.GetBusyHeroIdsAsync(playerId);
        busy.UnionWith(await heroes.GetMiningHeroIdsAsync(playerId));
        if (heroIds.Any(busy.Contains))
            throw new CommunityMineException(CommunityMineError.HeroBusy, "One of the dwarves is busy");

        await MutateAsync(depositId, window, now, (state, save) =>
        {
            var deposit = state.Deposit;
            if (selected.Any(h => !CommunityMineRules.Allows(deposit.Kind, h!)))
                throw new CommunityMineException(CommunityMineError.RankTooHigh,
                    $"Only dwarves up to rank {CommunityMineRules.CopperMaxRank} can mine copper");
            int mine = state.Dwarves.Count(d => d.PlayerId == playerId);
            if (mine + selected.Count > CommunityMineRules.MaxDwarvesPerPlayer)
                throw new CommunityMineException(CommunityMineError.TooManyDwarves,
                    $"Up to {CommunityMineRules.MaxDwarvesPerPlayer} of your dwarves per deposit");

            save.AddDwarves = selected
                .Select(h => new DepositDwarf { HeroId = h!.Id, DepositId = deposit.Id, PlayerId = playerId, Rate = CommunityMineRules.DwarfRate(h) })
                .ToList();
            if (save.Workers.All(w => w.PlayerId != playerId))
                save.Workers.Add(new DepositWorker { DepositId = deposit.Id, PlayerId = playerId });
        });
        return await DescribeAsync(playerId, window, now);
    }

    public async Task<CommunityMinesOverview> RecallAsync(Guid playerId, Guid depositId, Guid heroId)
    {
        var now = Now;
        var window = WorldEvents.Of(now);
        if (!IsMinesWeek(window))
            throw new CommunityMineException(CommunityMineError.EventNotActive, "Community mines are not here this week");

        await MutateAsync(depositId, window, now, (state, save) =>
        {
            if (!state.Dwarves.Any(d => d.HeroId == heroId && d.PlayerId == playerId))
                throw new CommunityMineException(CommunityMineError.NotYourDwarf, "This dwarf doesn't work here");
            save.RemoveHeroIds = [heroId];
        });
        return await DescribeAsync(playerId, window, now);
    }

    public async Task<CommunityMinesOverview> UpgradeAsync(Guid playerId, Guid depositId)
    {
        var now = Now;
        var window = RequireOpenWindow(now);

        await MutateAsync(depositId, window, now, (state, save) =>
        {
            var deposit = state.Deposit;
            int gold = CommunityMineRules.UpgradeGold(deposit.Kind, deposit.SpeedLevel)
                ?? throw new CommunityMineException(CommunityMineError.MaxLevel, "The deposit is fully upgraded");

            var worker = save.Workers.FirstOrDefault(w => w.PlayerId == playerId);
            if (worker?.Upgrades >= CommunityMineRules.MaxUpgradesPerPlayer)
                throw new CommunityMineException(CommunityMineError.UpgradeLimit,
                    $"Up to {CommunityMineRules.MaxUpgradesPerPlayer} upgrades of a deposit per player");
            if (worker == null)
            {
                worker = new DepositWorker { DepositId = deposit.Id, PlayerId = playerId };
                save.Workers.Add(worker);
            }

            deposit.SpeedLevel++;
            worker.Upgrades++;
            worker.UpgradePoints += deposit.OreTotal * CommunityMineRules.UpgradeShare;
            save.PayerId = playerId;
            save.Gold = gold;
        });
        return await DescribeAsync(playerId, window, now);
    }

    // Забрать награды за все закрытые месторождения ивента разом
    public async Task<CommunityMinesClaimResult> ClaimAsync(Guid playerId)
    {
        var now = Now;
        var window = WorldEvents.Of(now);
        if (!IsMinesWeek(window))
            throw new CommunityMineException(CommunityMineError.EventNotActive, "Community mines are not here this week");
        await TickAsync(window, now);

        var claims = (await PendingAsync(playerId, window.StartsAt))
            .Select(p => new DepositRewardClaim { DepositId = p.DepositId, PlayerId = playerId, Currency = p.Currency, ClaimedAt = now })
            .ToList();
        if (claims.Count == 0)
            throw new CommunityMineException(CommunityMineError.NothingToClaim, "Nothing to claim");
        if (!await mines.ClaimAsync(playerId, window.StartsAt, claims))
            throw new CommunityMineException(CommunityMineError.Changed, "The rewards have just been claimed");

        return new CommunityMinesClaimResult
        {
            Currency = claims.Sum(c => c.Currency),
            Overview = await DescribeAsync(playerId, window, now)
        };
    }

    /* ===== Месторождения ===== */

    private static WorldEventWindow RequireOpenWindow(DateTime now)
    {
        var window = WorldEvents.Of(now);
        if (!IsMinesWeek(window) || !window.IsActive(now))
            throw new CommunityMineException(CommunityMineError.EventNotActive, "Community mines are not here this week");
        return window;
    }

    private async Task TickAsync(WorldEventWindow window, DateTime now)
    {
        // Сначала закрываем выработанные: их места на карте освобождаются под новые
        foreach (var state in await mines.GetStatesAsync(window.StartsAt))
        {
            if (state.Deposit.ClosedAt != null)
                continue;
            var probe = Clone(state);
            bool exhausted = CommunityMineRules.Settle(probe.Deposit, probe.Workers, RatesOf(probe.Dwarves), now, window.EndsAt);
            if (exhausted || now >= window.EndsAt)
            {
                try
                {
                    await MutateAsync(state.Deposit.Id, window, now, (_, _) => { });
                }
                catch (CommunityMineException ex) when (ex.Code is CommunityMineError.DepositClosed or CommunityMineError.Changed)
                {
                    // Закрыто — этого и добивались
                }
            }
        }
        await EnsureDepositsAsync(window, now);
    }

    // Новые месторождения по расписанию, пока открытых меньше MaxOpen
    private async Task EnsureDepositsAsync(WorldEventWindow window, DateTime now)
    {
        if (!window.IsActive(now))
            return;

        var deposits = await mines.GetDepositsAsync(window.StartsAt);
        int open = deposits.Count(d => d.ClosedAt == null);
        int next = deposits.Count == 0 ? 0 : deposits.Max(d => d.Index) + 1;
        int? active = null;

        while (open < CommunityMineRules.MaxOpen && now >= CommunityMineRules.ScheduledAt(window.StartsAt, next))
        {
            active ??= await activity.CountActivePlayersAsync(DailyQuests.DayOf(now - WorldBossRules.ActiveWindow));
            var kind = CommunityMineRules.KindOf(next);
            var deposit = new CommunityDeposit
            {
                Id = Guid.NewGuid(),
                EventStart = window.StartsAt,
                Index = next,
                Kind = kind,
                OreTotal = CommunityMineRules.OreOf(kind, active.Value),
                UpdatedAt = now,
                SpawnedAt = now
            };
            // Этот номер только что создал параллельный запрос — значит, он и продолжит; нам хватит перечитать
            if (!await mines.TryCreateAsync(deposit))
                return;
            open++;
            next++;
        }
    }

    // Изменить месторождение: засчитать добытое, при выработке или конце ивента закрыть, иначе применить change.
    // Параллельное изменение — пересчитываем заново
    private async Task MutateAsync(Guid depositId, WorldEventWindow window, DateTime now, Action<DepositState, DepositSave> change)
    {
        for (int attempt = 0; attempt < Attempts; attempt++)
        {
            var state = await mines.GetStateAsync(depositId);
            if (state == null || state.Deposit.EventStart != window.StartsAt)
                throw new CommunityMineException(CommunityMineError.DepositNotFound, "Deposit not found");
            if (state.Deposit.ClosedAt != null)
                throw new CommunityMineException(CommunityMineError.DepositClosed, "The deposit is closed");

            var deposit = state.Deposit;
            var save = new DepositSave { Deposit = deposit, ExpectedVersion = deposit.Version, Workers = state.Workers };
            bool exhausted = CommunityMineRules.Settle(deposit, save.Workers, RatesOf(state.Dwarves), now, window.EndsAt);
            bool closing = exhausted || now >= window.EndsAt;
            if (closing)
            {
                deposit.ClosedAt = deposit.ExhaustedAt ?? window.EndsAt;
                save.ReleaseAll = true;
            }
            else
            {
                change(state, save);
            }

            switch (await mines.SaveAsync(save))
            {
                case DepositSaveOutcome.Saved:
                    if (closing)
                        throw new CommunityMineException(CommunityMineError.DepositClosed, "The deposit is closed");
                    return;
                case DepositSaveOutcome.NotEnoughGold:
                    throw new CommunityMineException(CommunityMineError.NotEnoughGold, $"An upgrade costs {save.Gold} gold");
                case DepositSaveOutcome.HeroBusy:
                    throw new CommunityMineException(CommunityMineError.HeroBusy, "One of the dwarves is busy");
            }
        }
        throw new CommunityMineException(CommunityMineError.Changed, "The deposit has just changed, try again");
    }

    private static Dictionary<Guid, double> RatesOf(IEnumerable<DepositDwarf> dwarves) =>
        dwarves.GroupBy(d => d.PlayerId).ToDictionary(g => g.Key, g => g.Sum(d => d.Rate));

    private static DepositState Clone(DepositState state) => new(
        new CommunityDeposit
        {
            Id = state.Deposit.Id,
            EventStart = state.Deposit.EventStart,
            Index = state.Deposit.Index,
            Kind = state.Deposit.Kind,
            OreTotal = state.Deposit.OreTotal,
            OreMined = state.Deposit.OreMined,
            UpdatedAt = state.Deposit.UpdatedAt,
            SpeedLevel = state.Deposit.SpeedLevel,
            SpawnedAt = state.Deposit.SpawnedAt,
            ExhaustedAt = state.Deposit.ExhaustedAt,
            ClosedAt = state.Deposit.ClosedAt,
            Version = state.Deposit.Version
        },
        state.Workers.Select(w => new DepositWorker
        {
            DepositId = w.DepositId, PlayerId = w.PlayerId, Mined = w.Mined, UpgradePoints = w.UpgradePoints, Upgrades = w.Upgrades
        }).ToList(),
        state.Dwarves.ToList());

    private async Task<List<DepositPendingReward>> PendingAsync(Guid playerId, DateTime eventStart) =>
        (await mines.GetUnclaimedAsync(playerId, eventStart))
            .Select(x => new DepositPendingReward
            {
                DepositId = x.Deposit.Id,
                Index = x.Deposit.Index,
                Kind = x.Deposit.Kind,
                Contribution = CommunityMineRules.ContributionOf(x.Worker),
                Share = CommunityMineRules.ShareOf(x.Worker, x.Deposit),
                Currency = CommunityMineRules.RewardFor(x.Deposit, x.Worker)
            })
            .Where(p => p.Currency > 0)
            .ToList();

    /* ===== Ответ ===== */

    private async Task<CommunityMinesOverview> DescribeAsync(Guid playerId, WorldEventWindow window, DateTime now)
    {
        var wallet = await wallets.GetEventWalletAsync(playerId, window.StartsAt);
        bool minesWeek = IsMinesWeek(window);
        var states = minesWeek ? await mines.GetStatesAsync(window.StartsAt) : [];

        // Состояние на сейчас — без записи: добыча досчитывается по нынешнему составу
        var live = states.Select(Clone).ToList();
        foreach (var state in live)
            CommunityMineRules.Settle(state.Deposit, state.Workers, RatesOf(state.Dwarves), now, window.EndsAt);

        var contributions = live
            .SelectMany(s => s.Workers)
            .GroupBy(w => w.PlayerId)
            .Select(g => (PlayerId: g.Key, Contribution: g.Sum(CommunityMineRules.ContributionOf)))
            .Where(x => x.Contribution > 0)
            .OrderByDescending(x => x.Contribution)
            .ToList();

        var topIds = live
            .SelectMany(s => s.Workers.OrderByDescending(CommunityMineRules.ContributionOf).Take(5).Select(w => w.PlayerId))
            .Concat(contributions.Take(CommunityMineRules.LeaderboardSize).Select(c => c.PlayerId))
            .Distinct()
            .ToList();
        var names = await mines.GetNamesAsync(topIds);
        var myHeroes = (await heroes.GetByPlayerAsync(playerId)).ToDictionary(h => h.Id);

        int myIndex = contributions.FindIndex(c => c.PlayerId == playerId);
        var open = live.Count(s => s.Deposit.ClosedAt == null);
        int nextIndex = live.Count == 0 ? 0 : live.Max(s => s.Deposit.Index) + 1;
        var nextSpawn = CommunityMineRules.ScheduledAt(window.StartsAt, nextIndex);

        return new CommunityMinesOverview
        {
            Event = WorldEventInfo.From(window, now, wallet),
            Open = minesWeek && window.IsActive(now),
            Deposits = live.OrderBy(s => s.Deposit.Index).Select(s => DescribeDeposit(s, playerId, names, myHeroes)).ToList(),
            NextSpawnAt = minesWeek && window.IsActive(now) && nextSpawn < window.EndsAt
                ? (open < CommunityMineRules.MaxOpen && nextSpawn < now ? now : nextSpawn)
                : null,
            MaxOpen = CommunityMineRules.MaxOpen,
            MaxDwarvesPerPlayer = CommunityMineRules.MaxDwarvesPerPlayer,
            MinShare = CommunityMineRules.MinShare,
            Pending = minesWeek ? await PendingAsync(playerId, window.StartsAt) : [],
            Leaderboard = contributions
                .Take(CommunityMineRules.LeaderboardSize)
                .Select(c => new DepositContributorInfo { PlayerId = c.PlayerId, Name = names.GetValueOrDefault(c.PlayerId, string.Empty), Contribution = c.Contribution })
                .ToList(),
            MyContribution = myIndex >= 0 ? contributions[myIndex].Contribution : 0,
            MyRank = myIndex + 1,
            ServerTime = now,
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }

    private static DepositInfo DescribeDeposit(DepositState state, Guid playerId, Dictionary<Guid, string> names,
        Dictionary<Guid, Hero> myHeroes)
    {
        var deposit = state.Deposit;
        double baseRate = deposit.ExhaustedAt == null && deposit.ClosedAt == null ? state.Dwarves.Sum(d => d.Rate) : 0;
        var me = state.Workers.FirstOrDefault(w => w.PlayerId == playerId);

        return new DepositInfo
        {
            Id = deposit.Id,
            Index = deposit.Index,
            Kind = deposit.Kind,
            Size = CommunityMineRules.SizeOf(deposit.Kind),
            MaxRank = deposit.Kind == DepositKind.Copper ? CommunityMineRules.CopperMaxRank : null,
            OreTotal = deposit.OreTotal,
            OreMined = deposit.OreMined,
            Rate = baseRate * CommunityMineRules.SpeedFactor(deposit.SpeedLevel),
            SpeedLevel = deposit.SpeedLevel,
            MaxSpeedLevel = CommunityMineRules.MaxSpeedLevel,
            SpeedFactor = CommunityMineRules.SpeedFactor(deposit.SpeedLevel),
            NextSpeedFactor = CommunityMineRules.SpeedFactor(deposit.SpeedLevel + 1),
            Dwarves = state.Dwarves.Count,
            Participants = state.Workers.Count(w => CommunityMineRules.ContributionOf(w) > 0 || state.Dwarves.Any(d => d.PlayerId == w.PlayerId)),
            EtaAt = AsUtc(CommunityMineRules.EtaOf(deposit, baseRate)),
            SpawnedAt = AsUtc(deposit.SpawnedAt),
            ExhaustedAt = AsUtc(deposit.ExhaustedAt),
            ClosedAt = AsUtc(deposit.ClosedAt),
            MyDwarves = state.Dwarves
                .Where(d => d.PlayerId == playerId)
                .Select(d =>
                {
                    var hero = myHeroes.GetValueOrDefault(d.HeroId);
                    return new DepositDwarfInfo
                    {
                        HeroId = d.HeroId,
                        Name = hero?.Name ?? string.Empty,
                        Rank = hero?.Level ?? 1,
                        Element = hero?.Element ?? HeroElement.Basic,
                        Profession = hero?.Profession,
                        Rate = d.Rate
                    };
                })
                .ToList(),
            MyContribution = me == null ? 0 : CommunityMineRules.ContributionOf(me),
            MyShare = me == null ? 0 : CommunityMineRules.ShareOf(me, deposit),
            MyReward = me == null ? 0 : CommunityMineRules.RewardFor(deposit, me),
            UpgradeGold = CommunityMineRules.UpgradeGold(deposit.Kind, deposit.SpeedLevel),
            UpgradePoints = deposit.OreTotal * CommunityMineRules.UpgradeShare,
            MyUpgrades = me?.Upgrades ?? 0,
            MaxUpgradesPerPlayer = CommunityMineRules.MaxUpgradesPerPlayer,
            Top = state.Workers
                .OrderByDescending(CommunityMineRules.ContributionOf)
                .Where(w => CommunityMineRules.ContributionOf(w) > 0)
                .Take(5)
                .Select(w => new DepositContributorInfo
                {
                    PlayerId = w.PlayerId,
                    Name = names.GetValueOrDefault(w.PlayerId, string.Empty),
                    Contribution = CommunityMineRules.ContributionOf(w),
                    Share = CommunityMineRules.ShareOf(w, deposit)
                })
                .ToList()
        };
    }

    // БД возвращает даты без Kind — без Utc JSON уйдёт без "Z", и браузер прочитает его как местное время
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
    private static DateTime? AsUtc(DateTime? value) => value is { } v ? AsUtc(v) : null;
}
