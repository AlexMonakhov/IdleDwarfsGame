// Причина отказа — фронт показывает по коду переведённый текст (abilities.errors.<code>)
public enum AbilityError
{
    HeroNotFound,
    // Дворф в экспедиции или в шахте
    HeroBusy,
    // Последнего дворфа не распылить
    LastHero,
    // У дворфа нет такой способности (или она не из книг, как иммунитеты)
    NothingToExtract,
    AbilityNotFound,
    // Заклинание вставляется только дворфу его стихии
    ElementMismatch,
    // Сливать можно только две одинаковые способности
    CannotCombine,
    // Сумма уровней выше MaxLevel
    MaxLevel,
    // Для слияния нужно ровно FusionSize разных дворфов
    WrongHeroCount,
    DifferentRanks,
    // Выше HeroLevels.Available дворфов пока не бывает
    MaxRank,
    // Дворфа или способность изменили параллельным запросом (двойной клик) — данные уже другие
    Changed
}

public class AbilityException(AbilityError code, string message) : InvalidOperationException(message)
{
    public AbilityError Code { get; } = code;
}

// Распыление дворфов:
//   извлечение — дворф распыляется, а одна его способность (заклинание, пассивка или аура) остаётся у игрока;
//     иммунитет не извлекается, остальные способности дворфа пропадают;
//   вставка — извлечённая способность заменяет такую же по виду у другого дворфа (прежняя пропадает);
//     заклинание — только дворфу своей стихии;
//   слияние способностей — две одинаковые дают одну с суммой уровней: 1 + 1 = 2, 1 + 2 = 3, выше MaxLevel нельзя;
//   слияние дворфов — FusionSize дворфов одного ранга дают дворфа рангом выше, игроку возвращается часть вложенного
//     в их прокачку (HeroUpgradeService.FusionRefund). Стихия — как у случайного из них
//     (4 огненных и 1 ледяной — 80% огонь, 20% лёд), у дворфов 1-го ранга стихии нет — тогда любая с равным шансом
public class AbilityService(
    IAbilityRepository abilities,
    IHeroesRepository heroes,
    IExpeditionRepository expeditions,
    IEntityFactory entityFactory,
    IPlayerRepository players,
    SquadPowerService squadPower,
    Random random)
{
    public const int MaxLevel = 3;
    public const int FusionSize = 5;

    public async Task<List<AbilityInfo>> GetMyAsync(Guid playerId) =>
        ToInfo(await abilities.GetByPlayerAsync(playerId));

    public async Task<AbilityActionResult> ExtractAsync(Guid playerId, Guid heroId, AbilityKind kind, string key)
    {
        var hero = await GetHeroAsync(playerId, heroId);
        await EnsureFreeAsync(playerId, [hero]);

        var source = AbilityInfo.Of(hero).FirstOrDefault(a => a.Kind == kind && a.Key == key)
            ?? throw new AbilityException(AbilityError.NothingToExtract, $"{hero.Name} has no {kind} '{key}' to extract");

        if ((await heroes.GetByPlayerAsync(playerId)).Count <= 1)
            throw new AbilityException(AbilityError.LastHero, "The last dwarf can't be dissolved");

        var ability = new PlayerAbility
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Kind = kind,
            Key = key,
            Level = source.Level
        };
        if (!await abilities.ExtractAsync(hero, ability))
            throw new AbilityException(AbilityError.Changed, "The dwarf has just changed, try again");

        await squadPower.RefreshIfInSquadAsync(playerId, [hero]);
        return await ResultAsync(playerId, null);
    }

    public async Task<AbilityActionResult> InsertAsync(Guid playerId, Guid abilityId, Guid heroId)
    {
        var ability = await GetAbilityAsync(playerId, abilityId);
        var info = AbilityInfo.From(ability)
            ?? throw new AbilityException(AbilityError.AbilityNotFound, $"Unknown {ability.Kind} '{ability.Key}'");
        var hero = await GetHeroAsync(playerId, heroId);

        // Заклинание — из книги своей стихии: другому дворфу его не вставить. Остальные ограничения появятся здесь позже
        if (info.Element is { } element && hero.Element != element)
            throw new AbilityException(AbilityError.ElementMismatch, $"{info.SpellName} can be inserted only into a {element} dwarf");

        Apply(hero, ability);

        if (!await abilities.InsertAsync(ability, hero))
            throw new AbilityException(AbilityError.Changed, "The ability has just been used, try again");

        await squadPower.RefreshIfInSquadAsync(playerId, [hero]);
        return await ResultAsync(playerId, hero);
    }

    public async Task<AbilityActionResult> CombineAsync(Guid playerId, Guid firstId, Guid secondId)
    {
        if (firstId == secondId)
            throw new AbilityException(AbilityError.CannotCombine, "An ability can't be combined with itself");

        var first = await GetAbilityAsync(playerId, firstId);
        var second = await GetAbilityAsync(playerId, secondId);
        if (first.Kind != second.Kind || first.Key != second.Key)
            throw new AbilityException(AbilityError.CannotCombine, "Only two copies of the same ability can be combined");

        int level = first.Level + second.Level;
        if (level > MaxLevel)
            throw new AbilityException(AbilityError.MaxLevel, $"Combined level {level} is above {MaxLevel}");

        var result = new PlayerAbility { Id = Guid.NewGuid(), PlayerId = playerId, Kind = first.Kind, Key = first.Key, Level = level };
        if (!await abilities.CombineAsync(first, second, result))
            throw new AbilityException(AbilityError.Changed, "The ability has just been used, try again");

        return await ResultAsync(playerId, null);
    }

    public async Task<AbilityActionResult> FuseAsync(Guid playerId, IReadOnlyList<Guid> heroIds)
    {
        if (heroIds.Count != FusionSize || heroIds.Distinct().Count() != FusionSize)
            throw new AbilityException(AbilityError.WrongHeroCount, $"Fusion needs {FusionSize} different dwarfs");

        var owned = (await heroes.GetByPlayerAsync(playerId)).ToDictionary(h => h.Id);
        var fused = heroIds.Select(id => owned.GetValueOrDefault(id)).ToList();
        if (fused.Any(h => h == null))
            throw new AbilityException(AbilityError.HeroNotFound, "Hero not found");

        var dwarfs = fused.OfType<Hero>().ToList();
        int rank = dwarfs[0].Level;
        if (dwarfs.Any(h => h.Level != rank))
            throw new AbilityException(AbilityError.DifferentRanks, "All dwarfs must be of the same rank");
        if (rank >= HeroLevels.Available)
            throw new AbilityException(AbilityError.MaxRank, $"Dwarfs above rank {HeroLevels.Available} are not available yet");
        await EnsureFreeAsync(playerId, dwarfs);

        var element = FusionElement(dwarfs, random);
        var result = HeroGenerator.Create(playerId, rank + 1, element.ToString(), random);
        // Новый дворф встаёт на первое место в отряде, которое освободили распылённые
        result.PlaceInSquad = dwarfs.Min(h => h.PlaceInSquad);

        var refund = FusionRefund(dwarfs);
        if (!await abilities.FuseAsync(playerId, heroIds, result, refund))
            throw new AbilityException(AbilityError.Changed, "One of the dwarfs has just changed, try again");

        await squadPower.RefreshIfInSquadAsync(playerId, dwarfs);
        var outcome = await ResultAsync(playerId, result);
        outcome.Refund = refund;
        outcome.Resources = await players.GetResourcesAsync(playerId) ?? new PlayerResources();
        return outcome;
    }

    // Возврат за прокачку слитых дворфов: доля HeroUpgradeService.FusionRefund от вложенного в них
    public static HeroUpgradeCost FusionRefund(IEnumerable<Hero> dwarfs)
    {
        long gold = 0, experience = 0;
        foreach (var hero in dwarfs)
        {
            var invested = HeroUpgradeService.Invested(hero.Level, hero.UpgradeLevel);
            gold += invested.Gold;
            experience += invested.Experience;
        }
        return new HeroUpgradeCost
        {
            Gold = (int)Math.Round(gold * HeroUpgradeService.FusionRefund, MidpointRounding.AwayFromZero),
            Experience = (int)Math.Round(experience * HeroUpgradeService.FusionRefund, MidpointRounding.AwayFromZero)
        };
    }

    // Стихия случайного из распыляемых дворфов: шанс стихии — её доля среди них. Без стихий — любая поровну
    private static HeroElement FusionElement(IReadOnlyList<Hero> dwarfs, Random random)
    {
        var elements = dwarfs.Select(h => h.Element).Where(e => e != HeroElement.Basic).ToList();
        if (elements.Count > 0)
            return elements[random.Next(elements.Count)];

        var all = ElementSpellBook.Elements.ToList();
        return all[random.Next(all.Count)];
    }

    // Способность заменяет такую же по виду: заклинание — все заклинания дворфа, пассивка — его пассивки
    private static void Apply(Hero hero, PlayerAbility ability)
    {
        switch (ability.Kind)
        {
            case AbilityKind.Spell when int.TryParse(ability.Key, out int family):
                hero.SpellIds = [SpellRegistry.IdOf(family, ability.Level)];
                break;
            case AbilityKind.Passive when Enum.TryParse<HeroPassive>(ability.Key, out var passive):
                hero.Passives = [passive];
                hero.PassiveLevel = ability.Level;
                break;
            case AbilityKind.Aura when AuraBook.Find(ability.Key) is { } aura:
                AuraBook.Apply(hero, aura, ability.Level);
                break;
            default:
                throw new AbilityException(AbilityError.AbilityNotFound, $"Unknown {ability.Kind} '{ability.Key}'");
        }
    }

    private async Task<Hero> GetHeroAsync(Guid playerId, Guid heroId)
    {
        var hero = await heroes.GetByIdAsync(heroId);
        if (hero == null || hero.PlayerId != playerId)
            throw new AbilityException(AbilityError.HeroNotFound, "Hero not found");
        return hero;
    }

    private async Task<PlayerAbility> GetAbilityAsync(Guid playerId, Guid abilityId) =>
        await abilities.GetAsync(playerId, abilityId)
            ?? throw new AbilityException(AbilityError.AbilityNotFound, "Ability not found");

    // Дворфы в экспедиции и в шахте заняты — их не распылить
    private async Task EnsureFreeAsync(Guid playerId, IReadOnlyList<Hero> dwarfs)
    {
        var busy = await expeditions.GetBusyHeroIdsAsync(playerId);
        busy.UnionWith(await heroes.GetMiningHeroIdsAsync(playerId));
        if (dwarfs.FirstOrDefault(h => busy.Contains(h.Id)) is { } taken)
            throw new AbilityException(AbilityError.HeroBusy, $"{taken.Name} is on an expedition or in a mine");
    }

    private async Task<AbilityActionResult> ResultAsync(Guid playerId, Hero? hero)
    {
        HeroInfo? info = null;
        if (hero != null)
        {
            var busy = await expeditions.GetBusyHeroIdsAsync(playerId);
            var mining = await heroes.GetMiningHeroIdsAsync(playerId);
            info = HeroInfo.From(entityFactory, hero, busy.Contains(hero.Id), mining.Contains(hero.Id));
        }

        return new AbilityActionResult { Abilities = await GetMyAsync(playerId), Hero = info };
    }

    // Способности не из книг (убрали из кода) в список не попадают: показать их нечем
    private static List<AbilityInfo> ToInfo(IEnumerable<PlayerAbility> items) => items
        .Select(AbilityInfo.From)
        .OfType<AbilityInfo>()
        .OrderBy(a => a.Kind)
        .ThenBy(a => a.Key)
        .ThenByDescending(a => a.Level)
        .ToList();
}
