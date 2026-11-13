// Эффекты артефактов на спеллы. Каждый — параметризованная механика: новый артефакт собирается из них в каталоге
// (EquipmentCatalog) без нового кода, текст на фронте — equipment.effects.<Kind>.
// Срабатывания, которые идут отдельным действием, показываются в бою как каст существующего спелла (VisualSpellId)

// Когда спелл повторяется (SpellEchoEffect)
public enum EchoTrigger
{
    // Каст кого-то убил — повтор по случайному живому врагу; Value — сколько раз цепочка может продолжиться
    OnKill,
    // Каждый Value-й каст спелла повторяется сразу, цели выбираются заново
    EveryNthCast,
    // Цели, у которых после каста меньше Value % HP, получают повтор
    TargetBelowHp
}

// Повтор того же спелла с уроном DamagePercent от его обычного урона и со своими статусами
public record SpellEchoEffect(string Spell, int DamagePercent, EchoTrigger Trigger, int Value) : SpellBoundEffect(Spell)
{
    // Доля кастов, после которых срабатывает повтор при убийстве или пороге HP (оценка для мощи)
    private const double KillShare = 0.25;
    private const double LowHpShare = 0.35;

    public override void AfterCast(CastScope cast)
    {
        var (world, casterId, context, spell) = (cast.World, cast.CasterId, cast.Context, cast.Spell);
        switch (Trigger)
        {
            case EchoTrigger.OnKill:
            {
                bool killed = cast.Killed.Count > 0;
                for (int chain = 0; killed && chain < Value; chain++)
                {
                    var enemies = world.GetAliveEnemies(casterId);
                    if (enemies.Count == 0) return;
                    int target = enemies[context.Random.Next(enemies.Count)].Id;
                    killed = spell.Echo(world, casterId, context, [target], DamagePercent).Killed.Count > 0;
                }
                break;
            }
            case EchoTrigger.EveryNthCast:
                if (Value > 0 && cast.Modifier is { } modifier && modifier.CastCount % Value == 0)
                    spell.Echo(world, casterId, context, null, DamagePercent);
                break;
            case EchoTrigger.TargetBelowHp:
            {
                var low = cast.Targets
                    .Where(t => world.IsAlive(t) && world.Health[t].Current * 100 < world.Health[t].Max * Value)
                    .ToList();
                if (low.Count > 0)
                    spell.Echo(world, casterId, context, low, DamagePercent);
                break;
            }
        }
    }

    public override SpellEstimate Estimate(CastEstimate cast)
    {
        double share = DamagePercent / 100.0;
        return Trigger switch
        {
            EchoTrigger.OnKill => new(Damage: KillShare * cast.PerTarget * share),
            EchoTrigger.EveryNthCast => new(
                Damage: cast.Base.Damage * share / Math.Max(1, Value),
                Healing: cast.Base.Healing * share / Math.Max(1, Value),
                StunnedTurns: cast.Base.StunnedTurns / Math.Max(1, Value)),
            _ => new(Damage: LowHpShare * cast.PerTarget * share, Healing: LowHpShare * cast.Base.Healing * share)
        };
    }

    public override ItemEffectInfo Info => Trigger switch
    {
        EchoTrigger.OnKill => new() { Kind = "EchoOnKill", Spell = Spell, Value = DamagePercent, Count = Value },
        EchoTrigger.EveryNthCast => new() { Kind = "EchoEveryNth", Spell = Spell, Value = DamagePercent, Count = Value },
        _ => new() { Kind = "EchoBelowHp", Spell = Spell, Value = DamagePercent, Threshold = Value }
    };
}

// После каста каждый живой враг со статусом Status (урон за ход) взрывается: весь оставшийся урон статуса × Percent %
// наносится сразу, статус снимается
public record DetonateStatusEffect(string Spell, string Status, int Percent, int VisualSpellId) : SpellBoundEffect(Spell)
{
    public override void AfterCast(CastScope cast)
    {
        if (SpellRegistry.All.GetValueOrDefault(VisualSpellId) is not BaseSpell visual) return;
        var (world, casterId, context) = (cast.World, cast.CasterId, cast.Context);

        var hits = world.GetAliveEnemies(casterId)
            .Select(e => (Target: e.Id, Effect: ItemEffectActions.FindStatus(world, e.Id, Status)))
            .Where(x => x.Effect is { RemainingDamage: > 0 })
            .Select(x => (x.Target, Damage: x.Effect!.RemainingDamage * Percent / 100))
            .ToList();
        if (hits.Count == 0) return;

        visual.TriggerHits(world, casterId, context, hits, applyEffects: false,
            afterHit: target => ItemEffectActions.RemoveStatus(world, context, casterId, target, Status, visual));
    }

    // Статус от союзников и своих спеллов в среднем висит на половине целей с остатком около 20% атаки
    public override SpellEstimate Estimate(CastEstimate cast) =>
        new(Damage: CombatPower.ReferenceTargets * 0.5 * cast.Attack * 0.2 * Percent / 100.0);

    public override ItemEffectInfo Info => new() { Kind = "DetonateStatus", Spell = Spell, Status = Status, Value = Percent };
}

// Цель, на которой до каста уже был статус From, получает статус To на Turns ходов (мороз → заморозка)
public record UpgradeStatusEffect(string Spell, string From, string To, int Turns) : SpellBoundEffect(Spell)
{
    // Доля целей, на которых From уже висит
    private const double Uptime = 0.4;

    public override void AfterCast(CastScope cast)
    {
        foreach (int target in cast.Targets.Where(t => cast.World.IsAlive(t) && cast.HadStatus(t, From)))
            StatusEffectFactory.Create(To, 0, Turns).ApplyEffect(cast.World, target, cast.Context, cast.CasterId);
    }

    public override SpellEstimate Estimate(CastEstimate cast) =>
        ItemEffectActions.SkipsTurn(To) ? new(StunnedTurns: cast.Targets * Uptime * Turns) : default;

    public override ItemEffectInfo Info => new() { Kind = "UpgradeStatus", Spell = Spell, Status = From, ToStatus = To, Value = Turns };
}

// Статус Status, наложенный носителем, раскалывается: когда он спадает или носителя статуса убивают —
// соседи по линии получают DamagePercent % атаки (визуал — VisualSpellId, без его статусов)
public record ShatterEffect(string Spell, string Status, int DamagePercent, int VisualSpellId) : SpellBoundEffect(Spell)
{
    public override void OnStatusExpired(World world, CombatContext context, int ownerId, int targetId, StatusEffectBase status)
    {
        if (status.EffectName == Status) Shatter(world, context, ownerId, targetId);
    }

    public override void OnDeath(World world, CombatContext context, int ownerId, int deadId, IReadOnlyList<StatusEffectBase> statuses)
    {
        if (statuses.Any(s => s.EffectName == Status && s.HasSource(ownerId)))
            Shatter(world, context, ownerId, deadId);
    }

    private void Shatter(World world, CombatContext context, int ownerId, int unitId)
    {
        if (SpellRegistry.All.GetValueOrDefault(VisualSpellId) is not BaseSpell visual) return;
        int damage = world.Attack[ownerId].Damage * DamagePercent / 100;
        var hits = ItemEffectActions.Neighbours(world, unitId).Select(n => (n, damage)).ToList();
        visual.TriggerHits(world, ownerId, context, hits, applyEffects: false);
    }

    // Почти каждый наложенный статус раскалывается — в среднем по полтора соседа
    public override SpellEstimate Estimate(CastEstimate cast) =>
        new(Damage: Math.Max(1, cast.Base.StunnedTurns) * 1.5 * cast.Attack * DamagePercent / 100.0);

    public override ItemEffectInfo Info => new() { Kind = "Shatter", Spell = Spell, Status = Status, Value = DamagePercent };
}

// Каст возвращает кастеру энергию: PerHit за каждого задетого врага и PerKill за каждого убитого
public record EnergyRefundEffect(string Spell, int PerHit, int PerKill) : SpellBoundEffect(Spell)
{
    public override void AfterCast(CastScope cast)
    {
        int gain = cast.Targets.Count * PerHit + cast.Killed.Count * PerKill;
        ItemEffectActions.GainEnergy(cast.World, cast.Context, cast.CasterId, gain);
    }

    // Возвращённая энергия — доля следующего каста
    public override SpellEstimate Estimate(CastEstimate cast)
    {
        double share = (cast.Targets * PerHit + 0.25 * PerKill) / Math.Max(1, cast.Spell.EnergyCost);
        return new(Damage: cast.Base.Damage * share, Healing: cast.Base.Healing * share, StunnedTurns: cast.Base.StunnedTurns * share);
    }

    public override ItemEffectInfo Info => new() { Kind = "EnergyRefund", Spell = Spell, Value = PerHit, Count = PerKill };
}

// После каста срабатывает случайный спелл из списка по случайному живому врагу, урон — DamagePercent % атаки
public record SpellProcPoolEffect(string Spell, int[] SpellIds, int DamagePercent) : SpellBoundEffect(Spell)
{
    public override void AfterCast(CastScope cast)
    {
        var pool = SpellIds.Select(id => SpellRegistry.All.GetValueOrDefault(id)).OfType<BaseSpell>().ToList();
        var enemies = cast.World.GetAliveEnemies(cast.CasterId);
        if (pool.Count == 0 || enemies.Count == 0) return;

        var spell = pool[cast.Context.Random.Next(pool.Count)];
        int target = enemies[cast.Context.Random.Next(enemies.Count)].Id;
        spell.CastTriggered(cast.World, cast.CasterId, cast.Context, [target], DamagePercent);
    }

    // Статусы снарядов (горение, яд, мороз) — примерно ещё треть удара
    public override SpellEstimate Estimate(CastEstimate cast) => new(Damage: cast.Attack * DamagePercent / 100.0 * 1.3);

    // Массив в записи сравнивается по ссылке — для каталога это не важно, эффекты не сравниваются
    public override ItemEffectInfo Info => new()
    {
        Kind = "SpellProcPool",
        Spell = Spell,
        Value = DamagePercent,
        ProcSpells = SpellIds.Select(id => SpellRegistry.All.TryGetValue(id, out var s) ? s.Name : $"#{id}").ToList()
    };
}

// Лечение каста сверх максимума HP становится уроном: спелл ProcSpellId по случайному врагу с уроном Percent % от избытка
public record OverhealProcEffect(string Spell, int ProcSpellId, int Percent) : SpellBoundEffect(Spell)
{
    // Какая часть лечения в среднем уходит в избыток
    private const double OverhealShare = 0.35;

    public override void AfterCast(CastScope cast)
    {
        if (cast.Overheal <= 0 || SpellRegistry.All.GetValueOrDefault(ProcSpellId) is not BaseSpell proc) return;
        var enemies = cast.World.GetAliveEnemies(cast.CasterId);
        if (enemies.Count == 0) return;

        int target = enemies[cast.Context.Random.Next(enemies.Count)].Id;
        proc.TriggerHits(cast.World, cast.CasterId, cast.Context, [(target, cast.Overheal * Percent / 100)], applyEffects: true);
    }

    public override SpellEstimate Estimate(CastEstimate cast) => new(Damage: cast.Base.Healing * OverhealShare * Percent / 100.0);

    public override ItemEffectInfo Info
    {
        get
        {
            var proc = SpellRegistry.All[ProcSpellId];
            return new() { Kind = "OverhealProc", Spell = Spell, ProcSpell = proc.Name, ProcLevel = proc.Level, Value = Percent };
        }
    }
}

// Когда умирает юнит со статусом Status от носителя, оставшиеся слои статуса переходят на Targets ближайших к нему союзников
public record SpreadOnDeathEffect(string Spell, string Status, int Targets, int VisualSpellId) : SpellBoundEffect(Spell)
{
    public override void OnDeath(World world, CombatContext context, int ownerId, int deadId, IReadOnlyList<StatusEffectBase> statuses)
    {
        if (statuses.FirstOrDefault(s => s.EffectName == Status && s.HasSource(ownerId)) is not LayeredStatusEffect status) return;
        if (SpellRegistry.All.GetValueOrDefault(VisualSpellId) is not BaseSpell visual) return;

        var spread = ItemEffectActions.Nearest(world, deadId, Targets)
            .Select(target => (target, Effect: (IStatusEffect?)status.CopyRemaining()))
            .Where(x => x.Effect != null)
            .Select(x => (x.target, x.Effect!))
            .ToList();
        visual.TriggerStatuses(world, ownerId, context, spread);
    }

    // Кто-то с ядом носителя умирает примерно в каждом третьем касте — остаток яда около 30% атаки на соседа
    public override SpellEstimate Estimate(CastEstimate cast) => new(Damage: Targets * cast.Attack * 0.3 / 3);

    public override ItemEffectInfo Info => new() { Kind = "SpreadOnDeath", Spell = Spell, Status = Status, Value = Targets };
}

// За каждую цель, на которую каст наложил Status, самый раненый союзник (разные) лечится на HealPercent % атаки
public record HealAlliesPerStatusEffect(string Spell, string Status, int HealPercent, int VisualSpellId) : SpellBoundEffect(Spell)
{
    public override void AfterCast(CastScope cast)
    {
        if (SpellRegistry.All.GetValueOrDefault(VisualSpellId) is not BaseSpell visual) return;
        var world = cast.World;
        int count = cast.Targets.Count(t => cast.Applied(t, Status));
        if (count == 0) return;

        int heal = world.Attack[cast.CasterId].Damage * HealPercent / 100;
        var heals = world.GetAliveAllies(cast.CasterId)
            .OrderBy(a => (double)world.Health[a.Id].Current / world.Health[a.Id].Max)
            .Take(count)
            .Select(a => (a.Id, heal))
            .ToList();
        visual.TriggerHeals(world, cast.CasterId, cast.Context, heals);
    }

    public override SpellEstimate Estimate(CastEstimate cast) => new(Healing: cast.Base.StunnedTurns * cast.Attack * HealPercent / 100.0);

    public override ItemEffectInfo Info => new() { Kind = "HealPerStatus", Spell = Spell, Status = Status, Value = HealPercent };
}

// Цели каста получают метку (MarkEffect): урон любых заклинаний по ним больше на Percent % в течение Turns ходов
public record MarkTargetsEffect(string Spell, int Percent, int Turns) : SpellBoundEffect(Spell)
{
    // По метке успевают ударить заклинаниями один-два союзника, в среднем на урон около 150% атаки
    private const double MarkedSpellDamage = 1.5;

    public override void AfterCast(CastScope cast)
    {
        foreach (int target in cast.Targets.Where(cast.World.IsAlive))
            new MarkEffect(Turns, Percent).ApplyEffect(cast.World, target, cast.Context, cast.CasterId);
    }

    public override SpellEstimate Estimate(CastEstimate cast) =>
        new(Damage: cast.Targets * cast.Attack * MarkedSpellDamage * Percent / 100.0 * Turns / 2.0);

    public override ItemEffectInfo Info => new() { Kind = "MarkTargets", Spell = Spell, Value = Percent, Count = Turns };
}

// Каждый тик статуса Status от носителя с шансом Chance вызывает спелл ProcSpellId по этой цели (DamagePercent % атаки)
public record StatusTickProcEffect(string Spell, string Status, int Chance, int ProcSpellId, int DamagePercent) : SpellBoundEffect(Spell)
{
    public override void OnStatusTick(World world, CombatContext context, int ownerId, int targetId, StatusEffectBase status)
    {
        if (status.EffectName != Status || !world.IsAlive(targetId)) return;
        if (context.Random.Next(100) >= Chance) return;
        if (SpellRegistry.All.GetValueOrDefault(ProcSpellId) is BaseSpell proc)
            proc.CastTriggered(world, ownerId, context, [targetId], DamagePercent);
    }

    // Статус каста тикает в среднем два хода на каждой цели
    public override SpellEstimate Estimate(CastEstimate cast) =>
        new(Damage: cast.Targets * 2 * Chance / 100.0 * cast.Attack * DamagePercent / 100.0);

    public override ItemEffectInfo Info
    {
        get
        {
            var proc = SpellRegistry.All[ProcSpellId];
            return new()
            {
                Kind = "StatusTickProc", Spell = Spell, Status = Status, Chance = Chance,
                ProcSpell = proc.Name, ProcLevel = proc.Level, Value = DamagePercent
            };
        }
    }
}

// Percent % урона проклятий, которые снял каст (Soul Harvest), делится лечением поровну на всех живых союзников
public record ConsumedCurseHealEffect(string Spell, int Percent) : SpellBoundEffect(Spell)
{
    public override void AfterCast(CastScope cast) =>
        ItemEffectActions.HealAlliesEvenly(cast, cast.CurseConsumed * Percent / 100, includeCaster: true);

    // Снятое проклятие в среднем около 40% атаки на цель
    public override SpellEstimate Estimate(CastEstimate cast) => new(Healing: cast.Targets * cast.Attack * 0.4 * Percent / 100.0);

    public override ItemEffectInfo Info => new() { Kind = "ConsumedCurseHeal", Spell = Spell, Value = Percent };
}

// С каждого живого союзника снимается до Count негативных статусов; за каждый снятый — DamagePercent % атаки
// по случайному врагу (визуал — VisualSpellId)
public record CleanseAlliesEffect(string Spell, int Count, int DamagePercent, int VisualSpellId) : SpellBoundEffect(Spell)
{
    public override void AfterCast(CastScope cast)
    {
        var (world, casterId, context) = (cast.World, cast.CasterId, cast.Context);
        // Снятие — отдельное действие: в группе каста фронт счёл бы союзников целями и пустил в них снаряд
        if (world.GetAliveAllies(casterId).Any(a => ItemEffectActions.NegativeStatuses(world, a.Id).Any()))
            context.NextAction();

        int cleansed = 0;
        foreach (var ally in world.GetAliveAllies(casterId))
        {
            var negative = ItemEffectActions.NegativeStatuses(world, ally.Id).Take(Count).ToList();
            foreach (var status in negative)
                ItemEffectActions.RemoveStatus(world, context, casterId, ally.Id, status.EffectName, cast.Spell);
            cleansed += negative.Count;
        }
        if (cleansed == 0 || SpellRegistry.All.GetValueOrDefault(VisualSpellId) is not BaseSpell visual) return;

        var enemies = world.GetAliveEnemies(casterId);
        if (enemies.Count == 0) return;
        int damage = world.Attack[casterId].Damage * DamagePercent / 100;
        var hits = Enumerable.Range(0, cleansed)
            .Select(_ => enemies[context.Random.Next(enemies.Count)].Id)
            .GroupBy(id => id)
            .Select(g => (g.Key, damage * g.Count()))
            .ToList();
        visual.TriggerHits(world, casterId, context, hits, applyEffects: false);
    }

    // В среднем бою на союзниках висит один-два негативных статуса; каждый снятый — ещё и спасённый урон
    public override SpellEstimate Estimate(CastEstimate cast) =>
        new(Damage: 1.5 * cast.Attack * DamagePercent / 100.0, Healing: 1.5 * cast.Attack * 0.2);

    public override ItemEffectInfo Info => new() { Kind = "CleanseAllies", Spell = Spell, Value = DamagePercent, Count = Count };
}

// Цель каста, на которой Stacks и больше стаков Status, лопается: урон за Ticks тиков сразу, остаётся один стак
public record StackBurstEffect(string Spell, string Status, int Stacks, int Ticks) : SpellBoundEffect(Spell)
{
    // Доля кастов, после которых на цели набирается порог стаков
    private const double BurstShare = 0.3;

    public override void AfterCast(CastScope cast)
    {
        var (world, casterId, context) = (cast.World, cast.CasterId, cast.Context);
        foreach (int target in cast.Targets.Where(world.IsAlive).ToList())
        {
            if (ItemEffectActions.FindStatus(world, target, Status) is not { } status || status.Stacks < Stacks) continue;

            int damage = status.Power * Ticks;
            status.TrimStacks(1);
            ItemEffectActions.LogStatusState(world, context, casterId, target, status);
            cast.Spell.ExtraHit(world, casterId, target, damage, context);
        }
    }

    // Порог набирается не каждый каст; лопается урон около трёх тиков по ~60% атаки
    public override SpellEstimate Estimate(CastEstimate cast) => new(Damage: BurstShare * cast.Targets * cast.Attack * 0.6 * Ticks);

    public override ItemEffectInfo Info => new() { Kind = "StackBurst", Spell = Spell, Status = Status, Count = Stacks, Value = Ticks };
}

// HP, которым кастер заплатил за каст, × Percent % делится лечением поровну на остальных живых союзников
public record PaidHpHealEffect(string Spell, int Percent) : SpellBoundEffect(Spell)
{
    public override void AfterCast(CastScope cast) =>
        ItemEffectActions.HealAlliesEvenly(cast, cast.HpPaid * Percent / 100, includeCaster: false);

    // Blood Price платит 10% максимального HP — около атаки кастера × 0,8 у дворфов
    public override SpellEstimate Estimate(CastEstimate cast) => new(Healing: cast.Attack * 0.8 * Percent / 100.0);

    public override ItemEffectInfo Info => new() { Kind = "PaidHpHeal", Spell = Spell, Value = Percent };
}

// Общие действия эффектов предметов: поиск и снятие статусов, соседи по линии, энергия, лечение отряда
public static class ItemEffectActions
{
    // Негативные статусы по важности снятия: сначала те, что отнимают ход
    private static readonly string[] NegativeOrder = ["Freeze", "Stun", "Curse", "Bleed", "Poison", "Burn", "Shock", MarkEffect.Name, "Frost"];

    public const string EnergyGain = "EnergyGain";

    public static StatusEffectBase? FindStatus(World world, int unitId, string name) =>
        world.StatusEffects.TryGetValue(unitId, out var component)
            ? component.ActiveEffects.OfType<StatusEffectBase>().FirstOrDefault(e => e.EffectName == name && e.RemainingTurns > 0)
            : null;

    public static IEnumerable<StatusEffectBase> NegativeStatuses(World world, int unitId) =>
        world.StatusEffects.TryGetValue(unitId, out var component)
            ? component.ActiveEffects.OfType<StatusEffectBase>()
                .Where(e => e.RemainingTurns > 0 && NegativeOrder.Contains(e.EffectName))
                .OrderBy(e => Array.IndexOf(NegativeOrder, e.EffectName))
                .ToList()
            : [];

    public static bool SkipsTurn(string status) => status is "Stun" or "Freeze";

    // Снять статус спеллом spell: лог StatusExpired в фазе Action — фронт показывает его в группе каста
    public static void RemoveStatus(World world, CombatContext context, int casterId, int targetId, string name, BaseSpell spell)
    {
        if (FindStatus(world, targetId, name) is not { } status) return;
        world.StatusEffects[targetId].ActiveEffects.Remove(status);
        if (!world.IsAlive(targetId)) return;

        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.StatusExpired,
            AttackerId = casterId,
            AttackerTeam = world.Team[casterId].TeamId,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = status.EffectName,
            SkillId = spell.Id,
            SkillName = spell.Name,
            TargetRemainingHp = world.Health[targetId].Current,
            ActiveEffects = { status.ToLogData() }
        });
    }

    // Состояние статуса после изменения кастом (сброс стаков): фронт перерисует иконку
    public static void LogStatusState(World world, CombatContext context, int casterId, int targetId, StatusEffectBase status) =>
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.StatusApplied,
            AttackerId = casterId,
            AttackerTeam = world.Team[casterId].TeamId,
            TargetId = targetId,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = status.EffectName,
            TargetRemainingHp = world.Health[targetId].Current,
            AppliedEffects = { status.ToLogData() }
        });

    // Ближайшие живые соседи по линии с обеих сторон
    public static List<int> Neighbours(World world, int unitId)
    {
        int position = world.GetPosition(unitId);
        var line = Line(world, unitId);
        return new[]
            {
                line.LastOrDefault(id => world.GetPosition(id) < position, -1),
                line.FirstOrDefault(id => world.GetPosition(id) > position, -1)
            }
            .Where(id => id != -1)
            .ToList();
    }

    // count ближайших живых союзников юнита по расстоянию в линии
    public static List<int> Nearest(World world, int unitId, int count)
    {
        int position = world.GetPosition(unitId);
        return Line(world, unitId)
            .OrderBy(id => Math.Abs(world.GetPosition(id) - position))
            .ThenBy(id => world.GetPosition(id))
            .Take(count)
            .ToList();
    }

    // Живые юниты команды unitId, кроме него самого, по позиции
    private static List<int> Line(World world, int unitId)
    {
        int team = world.Team[unitId].TeamId;
        return world.Entities
            .Where(e => e.Id != unitId && world.Team[e.Id].TeamId == team && world.IsAlive(e.Id))
            .OrderBy(e => world.GetPosition(e.Id))
            .Select(e => e.Id)
            .ToList();
    }

    // +amount энергии (не выше максимума). Лог — StatusApplied «EnergyGain»: фронт обновит полоску и покажет цифру
    public static void GainEnergy(World world, CombatContext context, int unitId, int amount)
    {
        if (amount <= 0 || !world.Energy.TryGetValue(unitId, out var energy)) return;
        int before = energy.Current;
        energy.Current = Math.Min(energy.Max, energy.Current + amount);
        int gained = energy.Current - before;
        if (gained <= 0) return;

        // Отдельное действие в фазе Passive: в группе каста фронт пустил бы снаряд в самого кастера,
        // а без фазы мог принять запись за срабатывание ауры
        context.NextAction();
        context.AddLog(new CombatLogEntry
        {
            EventType = CombatEventType.StatusApplied,
            Phase = "Passive",
            AttackerId = unitId,
            AttackerTeam = world.Team[unitId].TeamId,
            TargetId = unitId,
            TargetTeam = world.Team[unitId].TeamId,
            ActionType = EnergyGain,
            TargetRemainingHp = world.Health[unitId].Current,
            TargetEnergy = energy.Current,
            AppliedEffects = { new StatusEffectLogData { EffectId = "energygain", Name = EnergyGain, Duration = 0, Power = gained } }
        });
    }

    // Лечение total поровну на живых союзников кастера — в группе этого же каста
    public static void HealAlliesEvenly(CastScope cast, int total, bool includeCaster)
    {
        var allies = cast.World.GetAliveAllies(cast.CasterId).Where(a => includeCaster || a.Id != cast.CasterId).ToList();
        if (total <= 0 || allies.Count == 0) return;
        int each = total / allies.Count;
        if (each <= 0) return;
        foreach (var ally in allies)
            cast.Spell.HealTarget(cast.World, cast.CasterId, ally.Id, each, cast.Context);
    }
}

// Реакции эффектов предметов на события боя вне каста: тик статуса, его окончание, смерть юнита.
// Каждый носитель предмета (живой) реагирует только на статусы, которые наложил сам (StatusEffectBase.HasSource)
public static class ItemReactions
{
    private static List<(int Owner, SpellBoundEffect Effect)> Effects(World world) => world.Gear
        .Where(g => world.IsAlive(g.Key))
        .SelectMany(g => g.Value.Spells.Values.SelectMany(m => m.Effects).Select(e => (g.Key, e)))
        .ToList();

    public static void StatusTicked(World world, CombatContext context, int targetId, IStatusEffect status)
    {
        if (status is not StatusEffectBase effect || world.Gear.Count == 0) return;
        foreach (var (owner, item) in Effects(world).Where(x => effect.HasSource(x.Owner)))
            Run(context, () => item.OnStatusTick(world, context, owner, targetId, effect));
    }

    public static void StatusExpired(World world, CombatContext context, int targetId, IStatusEffect status)
    {
        if (status is not StatusEffectBase effect || world.Gear.Count == 0) return;
        foreach (var (owner, item) in Effects(world).Where(x => effect.HasSource(x.Owner)))
            Run(context, () => item.OnStatusExpired(world, context, owner, targetId, effect));
    }

    public static void Died(World world, CombatContext context, int deadId, IReadOnlyList<IStatusEffect> statuses)
    {
        if (world.Gear.Count == 0) return;
        var effects = statuses.OfType<StatusEffectBase>().ToList();
        if (effects.Count == 0) return;
        foreach (var (owner, item) in Effects(world))
            Run(context, () => item.OnDeath(world, context, owner, deadId, effects));
    }

    // Реакция с логами — отдельное действие: следующие записи хода не должны попасть в её группу каста на фронте
    private static void Run(CombatContext context, Action reaction)
    {
        int logs = context.Logs.Count;
        reaction();
        if (context.Logs.Count > logs) context.NextAction();
    }
}
