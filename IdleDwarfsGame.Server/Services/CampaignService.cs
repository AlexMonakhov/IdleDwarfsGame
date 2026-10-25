public class CampaignService(
    ICampaignRepository campaign,
    IHeroesRepository heroes,
    IEntityFactory entityFactory,
    IEnumerable<ICombatScenario> scenarios,
    CombatService combatService
)
{
    // Сколько заблокированных уровней показываем после текущего
    public const int LockedStagesPreview = 4;

    private CampaignScenario Scenario => scenarios.OfType<CampaignScenario>().First();

    public async Task<CampaignOverview> GetOverviewAsync(Guid playerId)
    {
        var stageIds = await campaign.GetStageIdsAsync();
        var cleared = await campaign.GetClearedStageIdsAsync(playerId);
        var currentStageId = FindCurrentStage(stageIds, cleared);

        var visibleIds = currentStageId == null
            ? new List<int>()
            : stageIds.SkipWhile(id => id != currentStageId).Take(1 + LockedStagesPreview).ToList();
        var stages = await campaign.GetStagesAsync(visibleIds);

        // Превью отряда собираем той же фабрикой, что и в бою, чтобы статы совпадали
        var squad = await heroes.GetSquadAsync(playerId);
        var squadWorld = new World();
        entityFactory.CreateSquad(squadWorld, squad, teamId: 1);

        return new CampaignOverview
        {
            CurrentStageId = currentStageId,
            ClearedCount = stageIds.Count(cleared.Contains),
            TotalStages = stageIds.Count,
            Squad = CombatSnapshot.Units(squadWorld),
            Stages = stages
                .Select(s => ToStageInfo(s, s.Id == currentStageId ? StageStatus.Available : StageStatus.Locked, squad))
                .ToList()
        };
    }

    public async Task<CampaignFightResult> FightAsync(Guid playerId, int stageId)
    {
        var stage = await campaign.GetStageAsync(stageId)
            ?? throw new InvalidOperationException($"Campaign stage {stageId} not found");

        var stageIds = await campaign.GetStageIdsAsync();
        var cleared = await campaign.GetClearedStageIdsAsync(playerId);

        // Перепроходить можно уже пройденные уровни, вперёд текущего — нельзя
        if (!cleared.Contains(stageId) && FindCurrentStage(stageIds, cleared) != stageId)
            throw new InvalidOperationException($"Campaign stage {stageId} is locked");

        var combat = await combatService.StartCampaignFight(playerId, stage);

        bool firstClear = false;
        if (combat.WinningTeam == 1)
        {
            firstClear = await campaign.MarkClearedAsync(playerId, stageId);
            cleared.Add(stageId);
        }

        return new CampaignFightResult
        {
            StageId = stageId,
            FirstClear = firstClear,
            CurrentStageId = FindCurrentStage(stageIds, cleared),
            Arena = ToArenaInfo(stage.Arena),
            Combat = combat
        };
    }

    // Текущий уровень — первый по порядку непройденный
    private static int? FindCurrentStage(List<int> stageIds, HashSet<int> cleared)
    {
        foreach (var id in stageIds)
        {
            if (!cleared.Contains(id))
                return id;
        }
        return null;
    }

    private CampaignStageInfo ToStageInfo(CampaignStage stage, StageStatus status, List<Hero> squad)
    {
        // Мир превью собирается как настоящий бой (отряд, враги, арена) — на нём же прогоняется прогноз
        World BuildWorld()
        {
            var world = new World();
            entityFactory.CreateSquad(world, squad, teamId: 1);
            Scenario.SpawnEnemies(world, stage);
            if (ArenaStatusEffect.FromArena(stage.Arena) is { } arenaEffect)
                world.AddGlobalEffect(arenaEffect);
            return world;
        }

        return new CampaignStageInfo
        {
            Id = stage.Id,
            Name = stage.Name,
            Description = stage.Description,
            Type = stage.Type,
            EnemyLevel = stage.EnemyLevel,
            Status = status,
            Arena = ToArenaInfo(stage.Arena),
            Enemies = CombatSnapshot.Units(BuildWorld()).Where(u => u.Team == 2).ToList(),
            Forecast = CombatPower.Forecast(BuildWorld)
        };
    }

    private static ArenaInfo ToArenaInfo(Arena arena) => new()
    {
        Id = arena.Id,
        Name = arena.Name,
        Description = arena.Description,
        Background = arena.Background,
        EffectName = arena.EffectName,
        EffectPower = arena.EffectPower,
        EffectDuration = arena.EffectDuration
    };
}
