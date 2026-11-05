// Рейтинговая арена — та же арена нарастающей ярости, но:
// - драться можно только закрепив отряд: в бой выходит его снимок (RankedSquad) — и в нападении, и в защите,
//   так что прокачка и перестановки в отряде аккаунта на арену не влияют. Перезакрепить можно раз в сутки (00:00 UTC);
// - соперники — игроки с закреплённым отрядом, чей рейтинг месячного сезона отличается от нашего не больше чем на 5%;
// - свои дневной, недельный и месячный сезоны и таблицы лидеров, отдельно от обычной арены; сезон начинается
//   с RankedArena.StartRating. Как и в обычной арене, рейтинг меняется только у нападавшего
public class RankedPvpService(
    IPvpRepository pvp,
    IUserRepository users,
    IHeroesRepository heroes,
    IEntityFactory entityFactory,
    CombatService combatService,
    PvpService pvpService,
    SquadPowerService squadPower,
    TimeProvider time
)
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<RankedOverview> GetOverviewAsync(Guid userId)
    {
        var now = Now;
        var day = PvpSeasons.DayOf(now);
        var user = await RequireUserAsync(userId);
        var accountSquad = await heroes.GetSquadAsync(user.PlayerId);
        var locked = await pvp.GetRankedSquadAsync(userId);
        var mySquad = locked?.Heroes() ?? [];
        int myPower = locked?.Power ?? 0;
        int myRating = await RatingAsync(userId, now);
        var beaten = await pvp.GetBeatenAsync(userId, day, PvpMode.Ranked);
        var scouts = await pvpService.GetScoutsAsync(userId, PvpMode.Ranked, day);
        // Без закреплённого отряда соперников не ищем: драться всё равно нельзя
        var pick = locked == null
            ? new PvpMatchmaking.Pick<Matched>([], RankedArena.StartSpreadPercent)
            : await PickOpponentsAsync(userId, myRating, now, beaten);

        return new RankedOverview
        {
            ServerTime = now,
            DayResetAt = day.AddDays(1),
            Squad = locked == null ? null : new RankedSquadInfo
            {
                LockedAt = PvpService.AsUtc(locked.LockedAt),
                CanChangeAt = RankedArena.CanChangeAt(locked.LockedAt),
                Power = myPower,
                Units = Units(mySquad)
            },
            CanLockSquad = CanLock(locked, now),
            AccountPower = squadPower.Of(accountSquad),
            AccountSquad = Units(accountSquad),
            MyRating = myRating,
            StartRating = RankedArena.StartRating,
            MatchSeason = RankedArena.MatchSeason,
            RatingSpreadPercent = pick.SpreadPercent,
            OpponentCount = PvpMatchmaking.OpponentCount,
            Opponents = pick.Opponents
                .Select(o => ToOpponentInfo(o, myPower, myRating,
                    PvpService.ForecastOf(scouts.ByOpponent.GetValueOrDefault(o.Candidate.UserId))))
                .ToList(),
            BeatenToday = beaten.Count,
            ScoutsLeft = scouts.Left,
            ScoutLimit = PvpScouting.DailyLimit,
            Seasons = await pvpService.GetSeasonsAsync(userId, now, PvpMode.Ranked),
            History = await pvpService.GetHistoryAsync(userId, PvpMode.Ranked),
            Arena = PvpArenaInfo.Current
        };
    }

    // Снимок текущего отряда аккаунта становится отрядом арены
    public async Task<RankedOverview> LockSquadAsync(Guid userId)
    {
        var now = Now;
        var user = await RequireUserAsync(userId);
        var squad = await heroes.GetSquadAsync(user.PlayerId);
        if (squad.Count == 0)
            throw new PvpException(PvpError.NoSquad, "Your squad is empty");

        var locked = await pvp.GetRankedSquadAsync(userId);
        if (!CanLock(locked, now))
            throw new PvpException(PvpError.SquadChangeCooldown, "The ranked squad can be changed once a day");

        var snapshot = new RankedSquad
        {
            UserId = userId,
            LockedAt = now,
            HeroesJson = RankedSquad.Serialize(squad),
            Power = squadPower.Of(squad)
        };
        // Параллельный запрос успел закрепить отряд раньше — сегодня его уже не поменять
        if (!await pvp.SaveRankedSquadAsync(snapshot, locked?.LockedAt))
            throw new PvpException(PvpError.SquadChangeCooldown, "The ranked squad can be changed once a day");

        return await GetOverviewAsync(userId);
    }

    public async Task<PvpFightResult> FightAsync(Guid userId, Guid opponentUserId)
    {
        var now = Now;
        var day = PvpSeasons.DayOf(now);
        await RequireUserAsync(userId);
        var locked = await pvp.GetRankedSquadAsync(userId)
            ?? throw new PvpException(PvpError.NoRankedSquad, "Lock your squad to fight in the ranked arena");
        var mySquad = locked.Heroes();

        var beaten = await pvp.GetBeatenAsync(userId, day, PvpMode.Ranked);
        if (beaten.Contains(opponentUserId))
            throw new PvpException(PvpError.AlreadyBeaten, "You have already beaten this player today");

        int myRating = await RatingAsync(userId, now);
        var opponent = await RequireMatchedAsync(userId, opponentUserId, myRating, now, beaten);

        var world = new World();
        entityFactory.CreateSquad(world, mySquad, teamId: 1);
        var combat = await combatService.StartRankedFight(world, new PvpScenario(entityFactory, opponent.Squad));
        bool won = combat.WinningTeam == 1;
        int theirRating = RatingOf(opponent.Candidate);

        var battle = new PvpBattle
        {
            Id = Guid.NewGuid(),
            Mode = PvpMode.Ranked,
            AttackerUserId = userId,
            DefenderUserId = opponentUserId,
            FoughtAt = now,
            Day = day,
            AttackerWon = won,
            AttackerPower = locked.Power,
            DefenderPower = opponent.Candidate.Power,
            AttackerRating = myRating,
            DefenderRating = theirRating,
            Points = PvpRating.DeltaFor(won, myRating, theirRating)
        };
        return await pvpService.RecordFightAsync(battle, opponent.Candidate.Username, combat);
    }

    // Разведка: снимок против снимка, лимит общий с обычной ареной
    public async Task<PvpScoutResult> ScoutAsync(Guid userId, Guid opponentUserId)
    {
        var now = Now;
        var day = PvpSeasons.DayOf(now);
        await RequireUserAsync(userId);
        var locked = await pvp.GetRankedSquadAsync(userId)
            ?? throw new PvpException(PvpError.NoRankedSquad, "Lock your squad to fight in the ranked arena");

        var beaten = await pvp.GetBeatenAsync(userId, day, PvpMode.Ranked);
        var opponent = await RequireMatchedAsync(userId, opponentUserId, await RatingAsync(userId, now), now, beaten);
        return await pvpService.ScoutAsync(userId, PvpMode.Ranked, opponentUserId, locked.Heroes(), opponent.Squad, now);
    }

    /* ===== Подбор ===== */

    // Драться и разведывать можно только тех, кого предложил подбор, — иначе через API можно было бы выбрать самого слабого
    private async Task<Matched> RequireMatchedAsync(Guid userId, Guid opponentUserId, int myRating, DateTime now, HashSet<Guid> beaten)
    {
        var pick = await PickOpponentsAsync(userId, myRating, now, beaten);
        var opponent = pick.Opponents.FirstOrDefault(o => o.Candidate.UserId == opponentUserId);
        if (opponent != null)
            return opponent;

        if (await pvp.GetRankedCandidateAsync(opponentUserId, RankedArena.MatchSeason, MatchSeasonStart(now)) == null)
            throw new PvpException(PvpError.OpponentNotFound, "Opponent not found");
        throw new PvpException(PvpError.NotAnOpponent, "This player is not among your opponents");
    }

    // Выбранный соперник со снимком отряда
    private record Matched(RankedCandidate Candidate, List<Hero> Squad);

    // Подбор по рейтингу; снимки героев грузятся только для выбранных
    private async Task<PvpMatchmaking.Pick<Matched>> PickOpponentsAsync(Guid userId, int myRating, DateTime now, HashSet<Guid> beaten)
    {
        var candidates = (await pvp.GetRankedCandidatesAsync(userId, RankedArena.MatchSeason, MatchSeasonStart(now)))
            .Where(c => !beaten.Contains(c.UserId));
        var pick = PvpMatchmaking.Choose(candidates, c => c.UserId, RatingOf, myRating,
            PvpMatchmaking.SeedFor(userId, PvpSeasons.DayOf(now)), RankedArena.StartSpreadPercent);

        var squads = await pvp.GetRankedSquadsAsync(pick.Opponents.Select(c => c.UserId).ToList());
        var opponents = pick.Opponents
            .Where(c => squads.GetValueOrDefault(c.UserId)?.Count > 0)
            .Select(c => new Matched(c, squads[c.UserId]))
            .ToList();
        return new PvpMatchmaking.Pick<Matched>(opponents, pick.SpreadPercent);
    }

    // Соперник снимок против снимка: оценка — по мощи снимков, очки за бой — по рейтингу
    private PvpOpponentInfo ToOpponentInfo(Matched opponent, int myPower, int myRating, BattleForecast? forecast)
    {
        int rating = RatingOf(opponent.Candidate);
        var info = pvpService.ToOpponentInfo(opponent.Candidate.UserId, opponent.Candidate.Username, opponent.Squad,
            opponent.Candidate.Power, myPower, forecast);
        info.Rating = rating;
        info.RatingDiffPercent = Math.Round(PvpMatchmaking.DiffPercent(myRating, rating), 1);
        info.WinPoints = PvpRating.WinFor(myRating, rating);
        info.LossPoints = PvpRating.LossFor(myRating, rating);
        return info;
    }

    private static int RatingOf(RankedCandidate candidate) => candidate.Rating ?? RankedArena.StartRating;

    private static DateTime MatchSeasonStart(DateTime now) => PvpSeasons.StartOf(RankedArena.MatchSeason, now);

    private async Task<int> RatingAsync(Guid userId, DateTime now) =>
        (await pvp.GetScoreAsync(userId, PvpMode.Ranked, RankedArena.MatchSeason, MatchSeasonStart(now)))?.Points
        ?? RankedArena.StartRating;

    /* ===== Отряд ===== */

    private static bool CanLock(RankedSquad? locked, DateTime now) =>
        locked == null || now >= RankedArena.CanChangeAt(locked.LockedAt);

    private List<CombatUnitInfo> Units(IEnumerable<Hero> squad)
    {
        var world = new World();
        entityFactory.CreateSquad(world, squad, teamId: 1);
        return CombatSnapshot.Units(world);
    }

    private async Task<User> RequireUserAsync(Guid userId) =>
        await users.GetByIdAsync(userId) ?? throw new PvpException(PvpError.PlayerNotFound, "Player not found");
}
