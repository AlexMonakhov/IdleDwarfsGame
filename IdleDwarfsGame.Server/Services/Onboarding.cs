using IdleDwarfsGame.Server.Enums;

// Стартовое обучение новичка (docs/tutorial-plan.md, «Стартовое обучение — порядок»): обучающий бой → стартовый сундук →
// склад → алтарь → экспедиция → шахта → алтарь → слияние. Сценарий шагов — на фронте, здесь — правила и суммы.
//
// Идёт, пока у игрока есть строка PlayerTutorial с ключом Key без статуса: её создаёт регистрация (у старых аккаунтов
// строки нет — обучение у них не идёт). Пока оно идёт, действуют исключения, и каждое — один раз (ключ Grants.*):
//   - награда 1-го уровня — стартовый сундук и золото с опытом ровно на улучшение героя до 2-го уровня;
//   - обучающая экспедиция на ExpeditionSeconds — вне доски, в награду золото ровно на улучшение шахты до 2-го уровня;
//   - первый сбор золотой шахты с дворфом — не меньше MineGold: хватает на следующее улучшение героя.
// Пропуск выдаёт всё, что ещё не выдано (OnboardingService.SkipAsync)
public static class Onboarding
{
    public const string Key = "onboarding";

    // Выдачи «один раз»: отметка DoneAt у строки с этим ключом
    public static class Grants
    {
        public const string StageReward = "onboarding.stage";
        public const string Heroes = "onboarding.heroes";
        public const string Expedition = "onboarding.expedition";
        public const string Mine = "onboarding.mine";
    }

    // Здания, через которые обучение провело игрока: в конце (и при пропуске) они отмечаются знакомыми —
    // «!» и знакомство не нужны
    public static readonly string[] CoveredActivities = ["campaign", "warehouse", "altar", "expeditions", "mine"];

    public const int StarterHeroCount = 25;
    public const int ExpeditionSeconds = 5;
    // Шаблон обучающей экспедиции (название и описание — на фронте, expeditions.templates.tutorial)
    public const string ExpeditionTemplate = "tutorial";
    public const MineType MineKind = MineType.Gold;
    public const int MinePlot = 1;

    private static HeroUpgradeCost UpgradeCost(int level) => HeroUpgradeService.CostFor(HeroLevels.Min, level)!;

    // Награда 1-го уровня: улучшение героя 1 → 2
    public static int StageGold => UpgradeCost(1).Gold;
    public static int StageExperience => UpgradeCost(1).Experience;

    // Экспедиция: улучшение шахты до 2-го уровня (с него в шахту садится дворф) и опыт на следующее улучшение героя
    public static int ExpeditionGold => MiningService.UpgradeCost(MineKind, MineUpgrade.Level, 1).Gold;
    public static int ExpeditionExperience => UpgradeCost(2).Experience;

    // Первый сбор шахты: золото на улучшение героя 2 → 3
    public static int MineGold => UpgradeCost(2).Gold;

    public static PlayerReward StageReward(Guid playerId, int stageId, DateTime now)
    {
        var rewardId = Guid.NewGuid();
        return new PlayerReward
        {
            Id = rewardId,
            PlayerId = playerId,
            Source = RewardSource.Campaign,
            SourceId = stageId,
            Gold = StageGold,
            Experience = StageExperience,
            Chests = [new PlayerRewardChest { Id = Guid.NewGuid(), RewardId = rewardId, ChestType = ChestType.Starter, ChestLevel = 1 }],
            CreatedAt = now
        };
    }

    // Содержимое стартового сундука: отряд по профессиям (как раньше выдавался при регистрации) и запас 1-го ранга
    public static List<Hero> StarterHeroes(Guid playerId, Random random)
    {
        var heroes = StarterSquad.Create(playerId, random);
        while (heroes.Count < StarterHeroCount)
            heroes.Add(HeroGenerator.Create(playerId, HeroLevels.Min, string.Empty, random));
        return heroes;
    }

    // Строка, с которой у нового игрока начинается обучение
    public static PlayerTutorial StartRow(Guid playerId, DateTime now) =>
        new() { PlayerId = playerId, Key = Key, UpdatedAt = now };
}
