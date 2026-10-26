// Причина отказа в прокачке — фронт показывает по коду переведённый текст (heroUpgrade.errors.<code>)
public enum HeroUpgradeError
{
    HeroNotFound,
    MaxLevel,
    NotEnoughGold,
    NotEnoughExperience,
    // Героя прокачали параллельным запросом (двойной клик) — цена уже другая
    LevelChanged
}

public class HeroUpgradeException(HeroUpgradeError code, string message) : InvalidOperationException(message)
{
    public HeroUpgradeError Code { get; } = code;
}

// Прокачка героя: золото и опыт аккаунта поднимают внутренний уровень героя (UpgradeLevel) с MinLevel до MaxLevel.
// Каждый уровень даёт +StatBonusPerLevel к базовым HP и атаке. Цена растёт с уровнем медленнее квадрата
// и линейно — с рангом героя (Level 1..5): прокачка героя высокого ранга дороже.
// Следующий шаг (позже): 5 героев одного вида на MaxLevel → ранг героя выше, а UpgradeLevel снова MinLevel
public class HeroUpgradeService(
    IHeroesRepository heroes,
    IPlayerRepository players,
    IExpeditionRepository expeditions,
    IEntityFactory entityFactory)
{
    public const int MinLevel = 1;
    public const int MaxLevel = 20;

    // +8% базовых HP и атаки за уровень: на MaxLevel герой в 2.52 раза сильнее, чем на первом
    public const double StatBonusPerLevel = 0.08;

    // Цена перехода с уровня L на L + 1: Base × ранг × L^CostExponent.
    // Стартовый герой (ранг 1) до MaxLevel стоит ≈ 12 000 золота и 6 000 опыта
    public const double BaseGold = 30;
    public const double BaseExperience = 15;
    public const double CostExponent = 1.3;

    public static int ClampLevel(int level) => Math.Clamp(level, MinLevel, MaxLevel);

    public static double StatFactor(int level) => 1 + StatBonusPerLevel * (ClampLevel(level) - MinLevel);

    public static int ScaleStat(int baseValue, int level) =>
        (int)Math.Round(baseValue * StatFactor(level), MidpointRounding.AwayFromZero);

    // null — герой уже на максимальном уровне
    public static HeroUpgradeCost? CostFor(int rank, int level)
    {
        if (level >= MaxLevel)
            return null;

        double scale = Math.Max(HeroLevels.Min, rank) * Math.Pow(ClampLevel(level), CostExponent);
        return new HeroUpgradeCost
        {
            Gold = (int)Math.Round(BaseGold * scale, MidpointRounding.AwayFromZero),
            Experience = (int)Math.Round(BaseExperience * scale, MidpointRounding.AwayFromZero)
        };
    }

    // Уровень, цена следующего и что он даст. Мощь следующего уровня считаем той же фабрикой, что и для боя
    public static HeroUpgradeInfo Describe(IEntityFactory factory, Hero hero)
    {
        int level = ClampLevel(hero.UpgradeLevel);
        var cost = CostFor(hero.Level, level);

        HeroUpgradePreview? next = null;
        if (cost != null)
        {
            var upgraded = hero.WithUpgradeLevel(level + 1);
            next = new HeroUpgradePreview
            {
                MaxHp = ScaleStat(hero.MaxHp, level + 1),
                Attack = ScaleStat(hero.Attack, level + 1),
                Power = CombatPower.OfHero(factory, upgraded).Total
            };
        }

        return new HeroUpgradeInfo { Level = level, MaxLevel = MaxLevel, Cost = cost, Next = next };
    }

    public async Task<HeroUpgradeResult> UpgradeAsync(Guid playerId, Guid heroId)
    {
        var hero = await heroes.GetByIdAsync(heroId);
        if (hero == null || hero.PlayerId != playerId)
            throw new HeroUpgradeException(HeroUpgradeError.HeroNotFound, "Hero not found");

        var cost = CostFor(hero.Level, hero.UpgradeLevel)
            ?? throw new HeroUpgradeException(HeroUpgradeError.MaxLevel, $"Hero is already at level {MaxLevel}");

        var resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources();
        EnsureAffordable(resources, cost);

        switch (await heroes.UpgradeAsync(hero, cost))
        {
            case HeroUpgradeOutcome.LevelChanged:
                throw new HeroUpgradeException(HeroUpgradeError.LevelChanged, "Hero level has just changed, try again");
            case HeroUpgradeOutcome.NotEnoughResources:
                // Ресурсы потратили параллельным запросом — перечитываем, чтобы назвать, чего не хватило
                EnsureAffordable(await players.GetResourcesAsync(playerId) ?? new PlayerResources(), cost);
                throw new HeroUpgradeException(HeroUpgradeError.NotEnoughGold, "Not enough resources");
        }

        var busy = await expeditions.GetBusyHeroIdsAsync(playerId);
        return new HeroUpgradeResult
        {
            Hero = HeroInfo.From(entityFactory, hero, busy.Contains(hero.Id)),
            Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources()
        };
    }

    private static void EnsureAffordable(PlayerResources resources, HeroUpgradeCost cost)
    {
        if (resources.Gold < cost.Gold)
            throw new HeroUpgradeException(HeroUpgradeError.NotEnoughGold, $"Not enough gold: {resources.Gold} of {cost.Gold}");
        if (resources.Experience < cost.Experience)
            throw new HeroUpgradeException(HeroUpgradeError.NotEnoughExperience, $"Not enough experience: {resources.Experience} of {cost.Experience}");
    }
}

public enum HeroUpgradeOutcome
{
    Upgraded,
    NotEnoughResources,
    LevelChanged
}
