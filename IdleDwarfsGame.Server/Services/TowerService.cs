// Бесконечная башня (правила — InfinityTower): этажи проходятся по порядку, отряд любой — тот, что стоит сейчас.
// Награда за этаж пишется вместе с прогрессом и попадает на аккаунт только по кнопке, как в кампании;
// пропуск до лучшего результата прошлых недель засчитывает этажи и сразу выдаёт их общую награду
public class TowerService(
    ITowerRepository tower,
    IHeroesRepository heroes,
    IEntityFactory entityFactory,
    CombatService combatService,
    IRewardRepository rewards,
    RewardService rewardService,
    TimeProvider time
)
{
    // Сколько заблокированных этажей показываем после текущего
    public const int LockedFloorsPreview = 4;

    public async Task<TowerOverview> GetOverviewAsync(Guid playerId)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var week = InfinityTower.WeekOf(now);
        int cleared = await tower.GetFloorAsync(playerId, week);
        int best = await tower.GetBestBeforeAsync(playerId, week);
        int current = cleared + 1;

        // Превью отряда собираем той же фабрикой, что и в бою, чтобы статы совпадали
        var squad = await heroes.GetSquadAsync(playerId);
        var squadWorld = new World();
        entityFactory.CreateSquad(squadWorld, squad, teamId: 1);

        var pendingReward = await rewards.GetUnclaimedAsync(playerId, RewardSource.Tower);

        return new TowerOverview
        {
            WeekStart = week,
            WeekEndsAt = InfinityTower.EndOf(week),
            Rotation = InfinityTower.RotationOf(week),
            FloorsPerBlock = InfinityTower.FloorsPerBlock,
            ClearedFloor = cleared,
            CurrentFloor = current,
            BestFloor = best,
            SkipToFloor = best > cleared ? best : null,
            SkipReward = best > cleared ? RewardInfo.From(TowerRewards.ForFloors(playerId, week, current, best, now)) : null,
            PendingRewardId = pendingReward?.Id,
            Squad = CombatSnapshot.Units(squadWorld),
            Floors = Enumerable.Range(current, 1 + LockedFloorsPreview)
                .Select(floor => ToFloorInfo(playerId, week, floor, floor == current ? StageStatus.Available : StageStatus.Locked, squad, now))
                .ToList()
        };
    }

    public async Task<TowerFightResult> FightAsync(Guid playerId, int floor)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var week = InfinityTower.WeekOf(now);
        int cleared = await tower.GetFloorAsync(playerId, week);

        // Пройденные этажи не перепроходятся: за них ничего не дают, а прогноз и так виден
        if (floor != cleared + 1)
            throw new InvalidOperationException($"Tower floor {floor} is locked");

        // Кнопка боя заменяется на «Забрать награды», пока награду не заберут
        if (await rewards.GetUnclaimedAsync(playerId, RewardSource.Tower) != null)
            throw new InvalidOperationException("Claim the reward for the cleared floor first");

        var towerFloor = InfinityTower.Floor(week, floor);
        var combat = await combatService.StartTowerFight(playerId, new TowerFloorScenario(entityFactory, towerFloor));

        // Награда пишется вместе с прогрессом, но на аккаунт попадёт лишь при получении
        bool clearedNow = combat.WinningTeam == 1
            && await tower.AdvanceAsync(playerId, week, cleared, floor, TowerRewards.For(playerId, week, floor, now), now);

        return new TowerFightResult
        {
            Floor = floor,
            Cleared = clearedNow,
            CurrentFloor = clearedNow ? floor + 1 : floor,
            Element = towerFloor.Element,
            Arena = ArenaInfo.From(towerFloor.Arena),
            Combat = combat
        };
    }

    public async Task<TowerSkipResult> SkipAsync(Guid playerId)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var week = InfinityTower.WeekOf(now);
        int cleared = await tower.GetFloorAsync(playerId, week);
        int best = await tower.GetBestBeforeAsync(playerId, week);

        if (best <= cleared)
            throw new InvalidOperationException("Nothing to skip: the best floor of previous weeks is already reached");

        // Иначе награда за последний этаж потерялась бы в списке незабранных
        if (await rewards.GetUnclaimedAsync(playerId, RewardSource.Tower) != null)
            throw new InvalidOperationException("Claim the reward for the cleared floor first");

        // Награды пропущенных этажей — по стихиям этой недели
        var reward = TowerRewards.ForFloors(playerId, week, cleared + 1, best, now);
        if (!await tower.AdvanceAsync(playerId, week, cleared, best, reward, now))
            throw new InvalidOperationException("Tower progress has just changed, try again");

        var claim = await rewardService.ClaimAsync(playerId, reward.Id);
        return new TowerSkipResult { SkippedTo = best, Reward = claim.Reward, Resources = claim.Resources };
    }

    private TowerFloorInfo ToFloorInfo(Guid playerId, DateTime week, int floor, StageStatus status, List<Hero> squad, DateTime now)
    {
        var towerFloor = InfinityTower.Floor(week, floor);

        // Мир превью собирается как настоящий бой (отряд, враги, арена) — на нём же прогоняется прогноз
        World BuildWorld()
        {
            var world = new World();
            entityFactory.CreateSquad(world, squad, teamId: 1);
            new TowerFloorScenario(entityFactory, towerFloor).SpawnEnemies(world);
            if (ArenaStatusEffect.FromArena(towerFloor.Arena) is { } arenaEffect)
                world.AddGlobalEffect(arenaEffect);
            return world;
        }

        return new TowerFloorInfo
        {
            Floor = floor,
            Element = towerFloor.Element,
            Type = towerFloor.Type,
            Status = status,
            Arena = ArenaInfo.From(towerFloor.Arena),
            Enemies = CombatSnapshot.Units(BuildWorld()).Where(u => u.Team == 2).ToList(),
            Forecast = CombatPower.Forecast(BuildWorld),
            Reward = RewardInfo.From(TowerRewards.For(playerId, week, floor, now))
        };
    }
}
