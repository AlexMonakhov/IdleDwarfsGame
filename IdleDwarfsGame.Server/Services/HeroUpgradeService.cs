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

// Прокачка героя: золото и опыт аккаунта поднимают внутренний уровень героя (UpgradeLevel) с MinLevel до потолка ранга
// (HeroLevels.MaxUpgradeLevel: 10 / 20 / 50 / 100). Каждый уровень даёт +5% к статам ранга (HeroLevels.UpgradeFactor).
// Цена растёт как квадрат множителя статов: каждый следующий процент силы дороже предыдущего.
// Ранг поднимает слияние пяти дворфов одного ранга (AbilityService.FuseAsync): новый дворф начинает с MinLevel,
// но его статы равны максимуму прошлого ранга, а часть вложенного в слитых дворфов возвращается (FusionRefund)
public class HeroUpgradeService(
    IHeroesRepository heroes,
    IPlayerRepository players,
    IExpeditionRepository expeditions,
    IEntityFactory entityFactory,
    SquadPowerService squadPower,
    IActivityTracker activity)
{
    public const int MinLevel = 1;

    // Цена перехода с уровня L на L + 1: BaseGold × (множитель ранга × множитель уровня L)², опыт — ExperienceShare от золота.
    // Отряд из 5 дворфов до потолка ранга: 2-й ≈ 26 тыс. золота, 3-й ≈ 630 тыс., 4-й ≈ 39 млн
    public const double BaseGold = 60;
    public const double ExperienceShare = 0.5;

    // При слиянии игроку возвращается эта доля золота и опыта, вложенных в прокачку слитых дворфов
    public const double FusionRefund = 0.3;

    public static int MaxLevelFor(int rank) => HeroLevels.MaxUpgradeLevel(rank);

    public static int ClampLevel(int rank, int level) => Math.Clamp(level, MinLevel, MaxLevelFor(rank));

    public static double StatFactor(int level) => HeroLevels.UpgradeFactor(level);

    public static int ScaleStat(int baseValue, int level) =>
        (int)Math.Round(baseValue * StatFactor(level), MidpointRounding.AwayFromZero);

    // null — герой уже на максимальном уровне своего ранга
    public static HeroUpgradeCost? CostFor(int rank, int level)
    {
        if (level >= MaxLevelFor(rank))
            return null;

        double factor = HeroLevels.RankFactor(rank) * StatFactor(ClampLevel(rank, level));
        double gold = BaseGold * factor * factor;
        return new HeroUpgradeCost
        {
            Gold = (int)Math.Round(gold, MidpointRounding.AwayFromZero),
            Experience = (int)Math.Round(gold * ExperienceShare, MidpointRounding.AwayFromZero)
        };
    }

    // Сколько всего вложено в прокачку героя с первого уровня до текущего
    public static HeroUpgradeCost Invested(int rank, int level)
    {
        long gold = 0, experience = 0;
        for (int l = MinLevel; l < ClampLevel(rank, level); l++)
        {
            var cost = CostFor(rank, l)!;
            gold += cost.Gold;
            experience += cost.Experience;
        }
        return new HeroUpgradeCost { Gold = (int)Math.Min(int.MaxValue, gold), Experience = (int)Math.Min(int.MaxValue, experience) };
    }

    // Уровень, цена следующего и что он даст. Мощь следующего уровня считаем той же фабрикой, что и для боя
    public static HeroUpgradeInfo Describe(IEntityFactory factory, Hero hero)
    {
        int level = ClampLevel(hero.Level, hero.UpgradeLevel);
        var cost = CostFor(hero.Level, level);

        HeroUpgradePreview? next = null;
        if (cost != null)
        {
            var upgraded = hero.WithUpgradeLevel(level + 1);
            var gear = EquipmentCatalog.CollectGear(hero);
            next = new HeroUpgradePreview
            {
                MaxHp = ScaleStat(hero.MaxHp, level + 1) + gear.Stats.MaxHp,
                Attack = ScaleStat(hero.Attack, level + 1) + gear.Stats.Attack,
                Power = CombatPower.OfHero(factory, upgraded).Total
            };
        }

        return new HeroUpgradeInfo
        {
            Level = level,
            MaxLevel = MaxLevelFor(hero.Level),
            Cost = cost,
            Next = next,
            Invested = Invested(hero.Level, level)
        };
    }

    public async Task<HeroUpgradeResult> UpgradeAsync(Guid playerId, Guid heroId)
    {
        var hero = await heroes.GetByIdAsync(heroId);
        if (hero == null || hero.PlayerId != playerId)
            throw new HeroUpgradeException(HeroUpgradeError.HeroNotFound, "Hero not found");

        var cost = CostFor(hero.Level, hero.UpgradeLevel)
            ?? throw new HeroUpgradeException(HeroUpgradeError.MaxLevel, $"Hero is already at level {MaxLevelFor(hero.Level)}");

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

        await squadPower.RefreshIfInSquadAsync(playerId, [hero]);
        await activity.RecordAsync(playerId, ActivityKind.HeroUpgrade);

        var busy = await expeditions.GetBusyHeroIdsAsync(playerId);
        var mining = await heroes.GetMiningHeroIdsAsync(playerId);
        return new HeroUpgradeResult
        {
            Hero = HeroInfo.From(entityFactory, hero, busy.Contains(hero.Id), mining.Contains(hero.Id)),
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
