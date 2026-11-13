public interface IStatusEffect
{
    string EffectName { get; }
    int RemainingTurns { get; set; }
    // stackLimit — потолок для периодических источников (ауры, арены): статус не копится больше стольких стаков и ходов
    void ApplyEffect(World world, int targetId, CombatContext context, int sourceId = -1, int? stackLimit = null);
    void OnTurnStart(World world, int targetId, CombatContext context);
    void OnTurnEnd(World world, int targetId, CombatContext context);
    StatusEffectLogData ToLogData();
}

// На юните висит не больше одного статуса каждого вида: повторное наложение складывается с уже висящим
// по правилу этого статуса (Absorb). Горение, яд, мороз копят независимые слои (LayeredStatusEffect),
// кровотечение и проклятие копят урон, оглушение продлевается
public abstract class StatusEffectBase : IStatusEffect
{
    public abstract string EffectName { get; }
    public int RemainingTurns { get; set; }
    // Сила за ход всех наложений вместе: урон, лечение или процент замедления
    public int Power { get; protected set; }
    public int Stacks { get; protected set; } = 1;
    protected virtual DamageType? EffectDamageType => null;

    // Чьё это наложение (боец и чем: спелл, аура, пассивка) — для статистики боя
    public StatOrigin Origin { get; private set; } = StatOrigin.Environment;
    // Источники урона или лечения за ход и их вес — сколько силы добавил каждый (слоистые статусы считают по слоям)
    private readonly Dictionary<StatOrigin, int> _origins = new();

    protected StatusEffectBase(int turns, int power)
    {
        RemainingTurns = turns;
        Power = power;
    }

    public void ApplyEffect(World world, int targetId, CombatContext context, int sourceId = -1, int? stackLimit = null)
    {
        if (!world.IsAlive(targetId)) return;

        // 1. Проверяем иммунитет
        if (world.Immunities.TryGetValue(targetId, out var immunity) && immunity.ImmuneTo.Contains(EffectName))
        {
            context.AddLog(new CombatLogEntry
            {
                EventType = CombatEventType.Immune,
                Phase = "Status",
                AttackerId = sourceId,
                AttackerTeam = sourceId > 0 ? world.Team[sourceId].TeamId : 0,
                TargetId = targetId,
                TargetTeam = world.Team[targetId].TeamId,
                ActionType = $"Immune ({EffectName})",
                TargetRemainingHp = world.Health[targetId].Current,
                AppliedEffects = { ToLogData() }
            });
            return;
        }

        // 2. «Мастер стихии» усиливает и продлевает урон за ход от своего источника
        if (EffectDamageType != null && sourceId > 0 && world.GetPassive(sourceId, HeroPassive.ElementMastery) is { } mastery)
            Empower(mastery.Percent, mastery.Turns);

        // Аура и пассивки помечают свои наложения (CombatContext.StatusKind); без пометки боец — значит спелл, без бойца — арена
        SetOrigin(sourceId > 0
            ? new StatOrigin(sourceId, context.StatusKind ?? CombatStatKind.Skill)
            : StatOrigin.Environment);

        // 3. Вешаем эффект или складываем с уже висящим того же вида
        if (!world.StatusEffects.TryGetValue(targetId, out var component))
        {
            component = new StatusEffectComponent();
            world.StatusEffects[targetId] = component;
        }

        var existing = component.ActiveEffects.OfType<StatusEffectBase>().FirstOrDefault(e => e.EffectName == EffectName);
        if (stackLimit is { } limit) LimitTurns(limit);

        if (existing == null) component.ActiveEffects.Add(this);
        // Упёрлись в потолок — наложение пропадает, но лог пишем: аура всё равно «сработала» по этой цели
        else if (stackLimit is not { } cap || existing.Stacks < cap) existing.Absorb(this);

        // В логе — итоговое состояние статуса на цели: фронт рисует по нему ходы и урон
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.StatusApplied,
            Phase = "Status",
            AttackerId = sourceId,
            AttackerTeam = sourceId > 0 ? world.Team[sourceId].TeamId : 0,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = EffectName,
            TargetRemainingHp = world.Health[targetId].Current,
            AppliedEffects = { (existing ?? this).ToLogData() }
        });
    }

    // Новое наложение того же статуса (added) поверх висящего (this)
    protected abstract void Absorb(StatusEffectBase added);

    protected virtual void SetOrigin(StatOrigin origin)
    {
        Origin = origin;
        _origins.Clear();
        _origins[origin] = Power;
    }

    // Наложение added добавило weight к силе за ход: его источник получает свою долю тиков
    protected void AddOrigin(StatusEffectBase added, int weight) =>
        _origins[added.Origin] = _origins.GetValueOrDefault(added.Origin) + weight;

    // Источники силы за ход и их вес сейчас
    protected virtual List<(StatOrigin Origin, int Weight)> OriginWeights() =>
        _origins.Select(kv => (kv.Key, kv.Value)).ToList();

    // Наложил ли этот боец хоть часть статуса (для эффектов предметов, которые реагируют на «свои» статусы)
    public bool HasSource(int unitId) => OriginWeights().Any(o => o.Origin.SourceId == unitId);

    // Сколько урона статус ещё нанесёт, если дотикает до конца (0 — статус без урона)
    public virtual int RemainingDamage => EffectDamageType != null ? Power * Math.Max(0, RemainingTurns) : 0;

    // Оставить только keep самых старых стаков; false — у статуса нет таких стаков (сбрасывается только кровотечение)
    public virtual bool TrimStacks(int keep) => false;

    // Сила наложения больше на percent, длится на turns ходов дольше
    protected virtual void Empower(int percent, int turns)
    {
        Power = (int)Math.Round(Power * (100 + percent) / 100.0, MidpointRounding.AwayFromZero);
        RemainingTurns += turns;
    }

    // Срезать собственную длительность наложения до limit ходов
    protected virtual void LimitTurns(int limit) => RemainingTurns = Math.Min(RemainingTurns, limit);

    public abstract void OnTurnStart(World world, int targetId, CombatContext context);

    public virtual void OnTurnEnd(World world, int targetId, CombatContext context)
    {
    }

    public StatusEffectLogData ToLogData() => new()
    {
        EffectId = EffectName.ToLowerInvariant(),
        Name = EffectName,
        Duration = RemainingTurns,
        Stacks = Stacks,
        Power = Power,
        TickDamage = EffectDamageType != null ? Power : null,
        DamageType = EffectDamageType
    };

    // Запись о срабатывании статуса; в ActiveEffects — состояние после этого хода
    protected CombatLogEntry TickLog(World world, int targetId) => new()
    {
        EventType = CombatEventType.StatusTick,
        Phase = "TurnStart",
        TargetId = targetId,
        TargetTeam = world.Team[targetId].TeamId,
        ActionType = EffectName,
        SkillName = EffectName,
        TargetRemainingHp = world.Health[targetId].Current,
        ActiveEffects = { ToLogData() }
    };

    // Тик урона: наносит damage и пишет лог с типом урона статуса.
    // weights — источники силы этого тика, если статус уже сменил состав (слои истекли до тика)
    protected void DealTickDamage(World world, int targetId, CombatContext context, int damage, DamageType damageType,
        List<(StatOrigin Origin, int Weight)>? weights = null)
    {
        damage = world.ScaleDamage(damage);
        int hpLeft = world.ApplyDamage(targetId, damage);
        var log = TickLog(world, targetId);
        log.Damage = damage;
        log.DamageType = damageType;
        log.TargetRemainingHp = hpLeft;
        log.Shares = CombatStatsTracker.Split(damage, weights ?? OriginWeights());
        context.AddLog(log);
    }
}

// Слоистый статус: каждое наложение — отдельный слой со своей силой и длительностью.
// За ход действуют все слои сразу, и каждый истекает сам — новое наложение не продлевает старые.
// Пример: 15 урона × 3 хода, 10 × 2, 5 × 1 → за первый ход 30, за второй 25, за третий 15
public abstract class LayeredStatusEffect : StatusEffectBase
{
    private sealed class Layer
    {
        public int Power;
        public int Turns;
        public StatOrigin Origin = StatOrigin.Environment;
    }

    private readonly List<Layer> _layers = new();

    protected LayeredStatusEffect(int turns, int power) : base(turns, power)
    {
        _layers.Add(new Layer { Power = power, Turns = turns });
    }

    protected override void Absorb(StatusEffectBase added)
    {
        _layers.AddRange(((LayeredStatusEffect)added)._layers);
        Sync();
    }

    protected override void LimitTurns(int limit)
    {
        foreach (var layer in _layers) layer.Turns = Math.Min(layer.Turns, limit);
        Sync();
    }

    protected override void SetOrigin(StatOrigin origin)
    {
        base.SetOrigin(origin);
        foreach (var layer in _layers) layer.Origin = origin;
    }

    protected override List<(StatOrigin Origin, int Weight)> OriginWeights() =>
        _layers.Select(l => (l.Origin, l.Power)).ToList();

    protected override void Empower(int percent, int turns)
    {
        foreach (var layer in _layers)
        {
            layer.Power = (int)Math.Round(layer.Power * (100 + percent) / 100.0, MidpointRounding.AwayFromZero);
            layer.Turns += turns;
        }
        Sync();
    }

    // Прошёл ход: каждый слой теряет ход, истёкшие снимаются
    protected void PassTurn()
    {
        foreach (var layer in _layers) layer.Turns--;
        _layers.RemoveAll(l => l.Turns <= 0);
        Sync();
    }

    public override int RemainingDamage => EffectDamageType != null ? _layers.Sum(l => l.Power * Math.Max(0, l.Turns)) : 0;

    // Копия оставшихся слоёв этого статуса — новое наложение того же вида (распространение на другого юнита)
    public LayeredStatusEffect? CopyRemaining()
    {
        var alive = _layers.Where(l => l.Turns > 0).ToList();
        if (alive.Count == 0) return null;

        var copy = (LayeredStatusEffect)StatusEffectFactory.Create(EffectName, alive[0].Power, alive[0].Turns);
        foreach (var layer in alive.Skip(1))
            copy._layers.Add(new Layer { Power = layer.Power, Turns = layer.Turns });
        copy.Sync();
        return copy;
    }

    // Статус висит, пока жив самый долгий слой
    private void Sync()
    {
        Power = _layers.Sum(l => l.Power);
        RemainingTurns = _layers.Count == 0 ? 0 : _layers.Max(l => l.Turns);
        Stacks = _layers.Count;
    }
}

// Урон за ход по сумме слоёв (горение, яд, электрошок)
public abstract class DamageOverTimeEffect : LayeredStatusEffect
{
    protected DamageOverTimeEffect(int turns, int damagePerTurn) : base(turns, damagePerTurn)
    {
    }

    protected abstract DamageType TickDamageType { get; }
    protected override DamageType? EffectDamageType => TickDamageType;

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        int damage = Power;
        // Источники — до снятия истёкших слоёв: этот тик ещё их
        var weights = OriginWeights();
        PassTurn();
        if (!world.IsAlive(targetId)) return;
        DealTickDamage(world, targetId, context, damage, TickDamageType, weights);
    }
}

public static class StatusEffectFactory
{
    // У кровотечения нет длительности: каждое наложение добавляет ход (см. BleedEffect)
    public static IStatusEffect Create(string name, int power, int duration) => name switch
    {
        "Burn" => new BurnEffect(duration, power),
        "Frost" => new FrostEffect(duration, power),
        "Stun" => new StunEffect(duration),
        "Regen" => new RegenEffect(duration, power),
        "Poison" => new PoisonEffect(duration, power),
        "Curse" => new CurseEffect(duration, power),
        // Мгновенный: длительности нет, power — сколько энергии сжечь
        EnergyBurnEffect.Name => new EnergyBurnEffect(power),
        "Shock" => new ShockEffect(duration, power),
        "Freeze" => new FreezeEffect(duration),
        "Bleed" => new BleedEffect(power),
        MarkEffect.Name => new MarkEffect(duration, power),
        _ => throw new ArgumentException($"Unknown status effect '{name}'")
    };
}
