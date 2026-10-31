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
    SquadChangeCooldown
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
        var (_, squad) = await RequirePlayerAsync(userId);
        int myPower = SquadPower(squad);
        var beaten = await pvp.GetBeatenAsync(userId, day, PvpMode.Casual);
        var pick = await PickOpponentsAsync(userId, myPower, day, beaten);

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
            Opponents = pick.Opponents.Select(o => ToOpponentInfo(o.Candidate.UserId, o.Candidate.Username, o.Candidate.Squad, o.Power, squad, myPower)).ToList(),
            BeatenToday = beaten.Count,
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

        // Драться можно только с теми, кого предложил подбор, — иначе через API можно было бы выбрать самого слабого
        int myPower = SquadPower(squad);
        var pick = await PickOpponentsAsync(userId, myPower, day, beaten);
        var opponent = pick.Opponents.FirstOrDefault(o => o.Candidate.UserId == opponentUserId);
        if (opponent == null)
        {
            if (await pvp.GetCandidateAsync(opponentUserId) == null)
                throw new PvpException(PvpError.OpponentNotFound, "Opponent not found");
            throw new PvpException(PvpError.NotAnOpponent, "This player is not among your opponents");
        }

        var scenario = new PvpScenario(entityFactory, opponent.Candidate.Squad);
        var combat = await combatService.StartPvpFight(user.PlayerId, scenario);
        bool won = combat.WinningTeam == 1;
        int points = PvpRating.DeltaFor(won, myPower, opponent.Power);

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
            DefenderPower = opponent.Power,
            Points = points
        };
        return await RecordFightAsync(battle, opponent.Candidate.Username, combat);
    }

    /* ===== Подбор ===== */

    private record Rated(PvpCandidate Candidate, int Power);

    private async Task<PvpMatchmaking.Pick<Rated>> PickOpponentsAsync(Guid userId, int myPower, DateTime day, HashSet<Guid> beaten)
    {
        if (myPower <= 0)
            return new PvpMatchmaking.Pick<Rated>([], PvpMatchmaking.StartSpreadPercent);

        var candidates = (await pvp.GetCandidatesAsync(userId))
            .Where(c => !beaten.Contains(c.UserId))
            .Select(c => new Rated(c, SquadPower(c.Squad)))
            .Where(c => c.Power > 0);

        return PvpMatchmaking.Choose(candidates, c => c.Candidate.UserId, c => c.Power, myPower, PvpMatchmaking.SeedFor(userId, day));
    }

    // Мощь отряда — сумма мощи героев, собранных той же фабрикой, что и для боя
    public int SquadPower(IEnumerable<Hero> squad)
    {
        var world = new World();
        return entityFactory.CreateSquad(world, squad, teamId: 1).Sum(id => CombatPower.Of(world, id).Total);
    }

    // Соперник с прогнозом боя; очки за бой — по мощи (рейтинговая арена потом пересчитывает их по рейтингу)
    public PvpOpponentInfo ToOpponentInfo(Guid opponentUserId, string username, IReadOnlyList<Hero> opponentSquad, int opponentPower,
        IReadOnlyList<Hero> mySquad, int myPower)
    {
        var scenario = new PvpScenario(entityFactory, opponentSquad);

        // Мир прогноза собирается как настоящий бой: наш отряд, отряд соперника, арена PvP
        World BuildWorld()
        {
            var world = new World();
            entityFactory.CreateSquad(world, mySquad, teamId: 1);
            scenario.SpawnDefenders(world);
            world.AddGlobalEffect(new RisingFuryArenaEffect());
            return world;
        }

        var defenders = new World();
        scenario.SpawnDefenders(defenders);

        return new PvpOpponentInfo
        {
            UserId = opponentUserId,
            Username = username,
            Power = opponentPower,
            PowerDiffPercent = Math.Round(PvpMatchmaking.DiffPercent(myPower, opponentPower), 1),
            Squad = CombatSnapshot.Units(defenders),
            Forecast = CombatPower.Forecast(BuildWorld),
            WinPoints = PvpRating.WinFor(myPower, opponentPower),
            LossPoints = PvpRating.LossFor(myPower, opponentPower)
        };
    }

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

    // SQLite возвращает даты без Kind — без Utc JSON уйдёт без "Z", и браузер прочитает его как местное время
    public static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
