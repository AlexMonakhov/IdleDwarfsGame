// Коды ошибок уходят на фронт строкой — по ним он показывает переведённый текст (pvp.errors.<code>)
public enum PvpError
{
    PlayerNotFound,
    NoSquad,
    OpponentNotFound,
    NotAnOpponent,
    AlreadyBeaten,
    // Рейтинговая арена: отряд не закреплён / сегодня его уже закрепляли
    NoRankedSquad,
    SquadChangeCooldown,
    // Разведка: на сегодня потрачены все PvpScouting.DailyLimit
    ScoutLimit
}

public class PvpException(PvpError code, string message) : InvalidOperationException(message)
{
    public PvpError Code { get; } = code;
}

// PvP между аккаунтами: игрок нападает на сохранённый отряд другого игрока (защитник в бою не участвует).
// Соперники подбираются по мощи отряда (PvpMatchmaking). Побеждённый соперник пропадает из подбора до 00:00 UTC.
// Каждый бой меняет очки нападавшего сразу в трёх сезонах — дневном, недельном и месячном (PvpRating, PvpSeasons).
// Сезоны, история и прогноз боя здесь общие с рейтинговой ареной (RankedPvpService)
public class PvpService(
    IPvpRepository pvp,
    IUserRepository users,
    IHeroesRepository heroes,
    IEntityFactory entityFactory,
    CombatService combatService,
    SquadPowerService squadPower,
    TimeProvider time
)
{
    // Сколько лучших показывать в таблице каждого сезона и сколько боёв в истории
    public const int LeaderboardSize = 10;
    public const int HistorySize = 15;

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<PvpOverview> GetOverviewAsync(Guid userId)
    {
        var now = Now;
        var day = PvpSeasons.DayOf(now);
        var (user, squad) = await RequirePlayerAsync(userId);
        int myPower = squadPower.Of(squad);
        // Подстраховка: если какое-то действие над героями забыло пересчитать мощь, другие увидят верную после нашего захода
        await squadPower.RefreshAsync(user.PlayerId);
        var beaten = await pvp.GetBeatenAsync(userId, day, PvpMode.Casual);
        var pick = await PickOpponentsAsync(userId, myPower, day, beaten);
        var scouts = await GetScoutsAsync(userId, PvpMode.Casual, day);

        var squadWorld = new World();
        entityFactory.CreateSquad(squadWorld, squad, teamId: 1);

        return new PvpOverview
        {
            ServerTime = now,
            DayResetAt = day.AddDays(1),
            MyPower = myPower,
            Squad = CombatSnapshot.Units(squadWorld),
            PowerSpreadPercent = pick.SpreadPercent,
            OpponentCount = PvpMatchmaking.OpponentCount,
            Opponents = pick.Opponents
                .Select(o => ToOpponentInfo(o.Candidate.UserId, o.Candidate.Username, o.Squad, o.Candidate.Power, myPower,
                    ForecastOf(scouts.ByOpponent.GetValueOrDefault(o.Candidate.UserId))))
                .ToList(),
            BeatenToday = beaten.Count,
            ScoutsLeft = scouts.Left,
            ScoutLimit = PvpScouting.DailyLimit,
            Seasons = await GetSeasonsAsync(userId, now, PvpMode.Casual),
            History = await GetHistoryAsync(userId, PvpMode.Casual),
            Arena = PvpArenaInfo.Current
        };
    }

    public async Task<PvpFightResult> FightAsync(Guid userId, Guid opponentUserId)
    {
        var now = Now;
        var day = PvpSeasons.DayOf(now);
        var (user, squad) = await RequirePlayerAsync(userId);
        if (squad.Count == 0)
            throw new PvpException(PvpError.NoSquad, "Your squad is empty");

        var beaten = await pvp.GetBeatenAsync(userId, day, PvpMode.Casual);
        if (beaten.Contains(opponentUserId))
            throw new PvpException(PvpError.AlreadyBeaten, "You have already beaten this player today");

        int myPower = squadPower.Of(squad);
        var opponent = await RequireMatchedAsync(userId, opponentUserId, myPower, day, beaten);

        var scenario = new PvpScenario(entityFactory, opponent.Squad);
        var combat = await combatService.StartPvpFight(user.PlayerId, scenario);
        bool won = combat.WinningTeam == 1;
        int points = PvpRating.DeltaFor(won, myPower, opponent.Candidate.Power);

        var battle = new PvpBattle
        {
            Id = Guid.NewGuid(),
            Mode = PvpMode.Casual,
            AttackerUserId = userId,
            DefenderUserId = opponentUserId,
            FoughtAt = now,
            Day = day,
            AttackerWon = won,
            AttackerPower = myPower,
            DefenderPower = opponent.Candidate.Power,
            Points = points
        };
        return await RecordFightAsync(battle, opponent.Candidate.Username, combat);
    }

    // Разведка соперника из сегодняшнего подбора: точный прогноз боя за счёт дневного лимита
    public async Task<PvpScoutResult> ScoutAsync(Guid userId, Guid opponentUserId)
    {
        var now = Now;
        var day = PvpSeasons.DayOf(now);
        var (_, squad) = await RequirePlayerAsync(userId);
        if (squad.Count == 0)
            throw new PvpException(PvpError.NoSquad, "Your squad is empty");

        var beaten = await pvp.GetBeatenAsync(userId, day, PvpMode.Casual);
        var opponent = await RequireMatchedAsync(userId, opponentUserId, squadPower.Of(squad), day, beaten);
        return await ScoutAsync(userId, PvpMode.Casual, opponentUserId, squad, opponent.Squad, now);
    }

    /* ===== Подбор ===== */

    // Выбранный соперник с отрядом защиты
    private record Matched(PvpCandidate Candidate, List<Hero> Squad);

    // Подбор идёт по сохранённой мощи (Player.SquadPower) — отряды грузятся только для выбранных
    private async Task<PvpMatchmaking.Pick<Matched>> PickOpponentsAsync(Guid userId, int myPower, DateTime day, HashSet<Guid> beaten)
    {
        if (myPower <= 0)
            return new PvpMatchmaking.Pick<Matched>([], PvpMatchmaking.StartSpreadPercent);

        var candidates = (await pvp.GetCandidatesAsync(userId))
            .Where(c => !beaten.Contains(c.UserId));
        var pick = PvpMatchmaking.Choose(candidates, c => c.UserId, c => c.Power, myPower, PvpMatchmaking.SeedFor(userId, day));

        var squads = await pvp.GetDefenseSquadsAsync(pick.Opponents.Select(c => c.PlayerId).ToList());
        // Отряд могли разобрать между чтением мощи и отрядов — такого соперника просто не показываем
        var opponents = pick.Opponents
            .Where(c => squads.ContainsKey(c.PlayerId))
            .Select(c => new Matched(c, squads[c.PlayerId]))
            .ToList();
        return new PvpMatchmaking.Pick<Matched>(opponents, pick.SpreadPercent);
    }

    // Драться и разведывать можно только тех, кого предложил подбор, — иначе через API можно было бы выбрать самого слабого
    private async Task<Matched> RequireMatchedAsync(Guid userId, Guid opponentUserId, int myPower, DateTime day, HashSet<Guid> beaten)
    {
        var pick = await PickOpponentsAsync(userId, myPower, day, beaten);
        var opponent = pick.Opponents.FirstOrDefault(o => o.Candidate.UserId == opponentUserId);
        if (opponent != null)
            return opponent;

        if (await pvp.GetCandidateAsync(opponentUserId) == null)
            throw new PvpException(PvpError.OpponentNotFound, "Opponent not found");
        throw new PvpException(PvpError.NotAnOpponent, "This player is not among your opponents");
    }

    // Соперник в списке: грубая оценка по мощи всегда, точный прогноз — только разведанный (forecast).
    // Очки за бой — по мощи (рейтинговая арена потом пересчитывает их по рейтингу)
    public PvpOpponentInfo ToOpponentInfo(Guid opponentUserId, string username, IReadOnlyList<Hero> opponentSquad, int opponentPower,
        int myPower, BattleForecast? forecast)
    {
        var defenders = new World();
        new PvpScenario(entityFactory, opponentSquad).SpawnDefenders(defenders);

        return new PvpOpponentInfo
        {
            UserId = opponentUserId,
            Username = username,
            Power = opponentPower,
            PowerDiffPercent = Math.Round(PvpMatchmaking.DiffPercent(myPower, opponentPower), 1),
            Squad = CombatSnapshot.Units(defenders),
            Threat = PvpScouting.ThreatOf(myPower, opponentPower),
            Forecast = forecast,
            WinPoints = PvpRating.WinFor(myPower, opponentPower),
            LossPoints = PvpRating.LossFor(myPower, opponentPower)
        };
    }

    /* ===== Разведка ===== */

    // Сегодняшние разведки на этой арене (по сопернику) и сколько осталось — лимит общий для обеих арен
    public async Task<(Dictionary<Guid, PvpScout> ByOpponent, int Left)> GetScoutsAsync(Guid userId, PvpMode mode, DateTime day)
    {
        var scouts = await pvp.GetScoutsAsync(userId, day);
        return (scouts.Where(s => s.Mode == mode).ToDictionary(s => s.OpponentUserId),
            Math.Max(0, PvpScouting.DailyLimit - scouts.Count));
    }

    // Общая часть разведки обеих арен: прогноз прогоняется только здесь, по запросу игрока.
    // Тот же соперник повторно — бесплатно (например, после смены отряда); новый — пока не кончился лимит
    public async Task<PvpScoutResult> ScoutAsync(Guid userId, PvpMode mode, Guid opponentUserId,
        IReadOnlyList<Hero> mySquad, IReadOnlyList<Hero> opponentSquad, DateTime now)
    {
        var day = PvpSeasons.DayOf(now);
        var scouts = await pvp.GetScoutsAsync(userId, day);
        bool again = scouts.Any(s => s.Mode == mode && s.OpponentUserId == opponentUserId);
        // Лимит проверяем до прогноза, чтобы не гонять 50 боёв впустую; SaveScoutAsync проверит его ещё раз атомарно
        if (!again && scouts.Count >= PvpScouting.DailyLimit)
            throw new PvpException(PvpError.ScoutLimit, "No scouts left today");

        var forecast = Forecast(mySquad, opponentSquad);
        var scout = new PvpScout
        {
            UserId = userId,
            Day = day,
            Mode = mode,
            OpponentUserId = opponentUserId,
            WinChance = forecast.WinChance,
            PlayerPower = forecast.PlayerPower,
            EnemyPower = forecast.EnemyPower,
            ScoutedAt = now
        };
        if (!await pvp.SaveScoutAsync(scout, PvpScouting.DailyLimit))
            throw new PvpException(PvpError.ScoutLimit, "No scouts left today");

        return new PvpScoutResult
        {
            OpponentUserId = opponentUserId,
            Forecast = forecast,
            ScoutsLeft = Math.Max(0, PvpScouting.DailyLimit - scouts.Count - (again ? 0 : 1))
        };
    }

    // Мир прогноза собирается как настоящий бой: наш отряд, отряд соперника, арена PvP
    private BattleForecast Forecast(IReadOnlyList<Hero> mySquad, IReadOnlyList<Hero> opponentSquad)
    {
        var scenario = new PvpScenario(entityFactory, opponentSquad);
        return CombatPower.Forecast(() =>
        {
            var world = new World();
            entityFactory.CreateSquad(world, mySquad, teamId: 1);
            scenario.SpawnDefenders(world);
            world.AddGlobalEffect(new RisingFuryArenaEffect());
            return world;
        });
    }

    public static BattleForecast? ForecastOf(PvpScout? scout) => scout == null ? null : new BattleForecast
    {
        PlayerPower = scout.PlayerPower,
        EnemyPower = scout.EnemyPower,
        WinChance = scout.WinChance
    };

    // Записывает бой и очки во всех сезонах его арены
    public async Task<PvpFightResult> RecordFightAsync(PvpBattle battle, string opponentName, CombatResult combat)
    {
        // Параллельный бой успел победить этого соперника раньше — второй победы за сутки не будет
        if (!await pvp.RecordBattleAsync(battle, SeasonsOf(battle.FoughtAt)))
            throw new PvpException(PvpError.AlreadyBeaten, "You have already beaten this player today");

        var seasons = new List<PvpSeasonPointsInfo>();
        foreach (var (type, start) in SeasonsOf(battle.FoughtAt))
        {
            var score = await pvp.GetScoreAsync(battle.AttackerUserId, battle.Mode, type, start);
            seasons.Add(new PvpSeasonPointsInfo { Type = type, Points = score?.Points ?? InitialPoints(battle.Mode) });
        }

        return new PvpFightResult
        {
            OpponentUserId = battle.DefenderUserId,
            OpponentName = opponentName,
            Won = battle.AttackerWon,
            Points = battle.Points,
            BloodCoins = ShopRules.BloodCoinsFor(battle.Mode, battle.AttackerWon),
            Seasons = seasons,
            Arena = PvpArenaInfo.Current,
            Combat = combat
        };
    }

    /* ===== Сезоны ===== */

    // Очки, с которых игрок начинает сезон
    public static int InitialPoints(PvpMode mode) => mode == PvpMode.Ranked ? RankedArena.StartRating : 0;

    private static List<(PvpSeasonType Type, DateTime Start)> SeasonsOf(DateTime now) =>
        PvpSeasons.All.Select(type => (type, PvpSeasons.StartOf(type, now))).ToList();

    public async Task<List<PvpSeasonInfo>> GetSeasonsAsync(Guid userId, DateTime now, PvpMode mode)
    {
        var seasons = new List<PvpSeasonInfo>();
        foreach (var (type, start) in SeasonsOf(now))
        {
            var score = await pvp.GetScoreAsync(userId, mode, type, start);
            var top = await pvp.GetLeaderboardAsync(mode, type, start, LeaderboardSize);

            seasons.Add(new PvpSeasonInfo
            {
                Type = type,
                StartsAt = start,
                EndsAt = PvpSeasons.EndOf(type, start),
                Points = score?.Points ?? InitialPoints(mode),
                Wins = score?.Wins ?? 0,
                Losses = score?.Losses ?? 0,
                Rank = score == null ? null : await pvp.CountAheadAsync(score) + 1,
                Top = top
                    .Select((e, index) => new PvpLeaderboardEntryInfo
                    {
                        Rank = index + 1,
                        UserId = e.UserId,
                        Username = e.Username,
                        Points = e.Points,
                        Wins = e.Wins,
                        Losses = e.Losses,
                        IsMe = e.UserId == userId
                    })
                    .ToList()
            });
        }
        return seasons;
    }

    public async Task<List<PvpBattleInfo>> GetHistoryAsync(Guid userId, PvpMode mode) =>
        (await pvp.GetHistoryAsync(userId, mode, HistorySize)).Select(h => ToBattleInfo(h, userId)).ToList();

    private static PvpBattleInfo ToBattleInfo(PvpHistoryEntry entry, Guid userId)
    {
        var battle = entry.Battle;
        bool isAttack = battle.AttackerUserId == userId;
        bool ranked = battle.Mode == PvpMode.Ranked;
        return new PvpBattleInfo
        {
            Id = battle.Id,
            FoughtAt = AsUtc(battle.FoughtAt),
            IsAttack = isAttack,
            OpponentUserId = isAttack ? battle.DefenderUserId : battle.AttackerUserId,
            OpponentName = isAttack ? entry.DefenderName : entry.AttackerName,
            Won = isAttack == battle.AttackerWon,
            Points = isAttack ? battle.Points : 0,
            MyPower = isAttack ? battle.AttackerPower : battle.DefenderPower,
            OpponentPower = isAttack ? battle.DefenderPower : battle.AttackerPower,
            MyRating = ranked ? (isAttack ? battle.AttackerRating : battle.DefenderRating) : null,
            OpponentRating = ranked ? (isAttack ? battle.DefenderRating : battle.AttackerRating) : null
        };
    }

    private async Task<(User User, List<Hero> Squad)> RequirePlayerAsync(Guid userId)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new PvpException(PvpError.PlayerNotFound, "Player not found");
        return (user, await heroes.GetSquadAsync(user.PlayerId));
    }

    // БД возвращает даты без Kind — без Utc JSON уйдёт без "Z", и браузер прочитает его как местное время
    public static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
