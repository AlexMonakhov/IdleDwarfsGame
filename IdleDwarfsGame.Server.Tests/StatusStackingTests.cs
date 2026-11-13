using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// Сложение повторных наложений статусов: у каждого вида своё правило (StatusEffectBase.Absorb)
public class StatusStackingTests
{
    private static readonly EntityFactory Factory = new();

    private static Hero MakeHero(string name, int hp = 1000) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        MaxHp = hp,
        Attack = 20,
        Speed = 5,
        MaxEnergy = 100,
        StartEnergy = 0,
        EnergyPerAttack = 20
    };

    private static (World world, int id) Target()
    {
        var world = new World();
        int id = Factory.CreateUnit(world, MakeHero("Target"), 2, 1);
        Factory.CreateUnit(world, MakeHero("Enemy"), 1, 1);
        return (world, id);
    }

    // Один ход носителя: статусы начала хода и конца; возвращает урон от статусов за ход
    private static int PassTurn(World world, int id)
    {
        var context = new CombatContext { CurrentActor = id };
        new StatusEffectSystem(EffectPhase.Start).Execute(world, context);
        new StatusEffectSystem(EffectPhase.End).Execute(world, context);
        return context.Logs.Where(l => l.EventType == CombatEventType.StatusTick).Sum(l => l.Damage);
    }

    private static List<int> DamageByTurn(World world, int id, int turns) =>
        Enumerable.Range(0, turns).Select(_ => PassTurn(world, id)).ToList();

    [Fact]
    public void Burn_EveryApplicationIsIndependentLayer()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new BurnEffect(3, 15).ApplyEffect(world, id, context);
        new BurnEffect(2, 10).ApplyEffect(world, id, context);
        new BurnEffect(1, 5).ApplyEffect(world, id, context);

        Assert.Single(world.StatusEffects[id].ActiveEffects);
        Assert.Equal(new[] { 30, 25, 15, 0 }, DamageByTurn(world, id, 4));
        Assert.Empty(world.StatusEffects[id].ActiveEffects);
    }

    [Fact]
    public void Poison_StacksLikeBurn()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new PoisonEffect(3, 15).ApplyEffect(world, id, context);
        new PoisonEffect(1, 5).ApplyEffect(world, id, context);

        Assert.Equal(new[] { 20, 15, 15, 0 }, DamageByTurn(world, id, 4));
    }

    [Fact]
    public void Bleed_EachStackAddsTurn_AndFollowingStacksHitLessTheMoreStacks()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new BleedEffect(15).ApplyEffect(world, id, context);
        new BleedEffect(10).ApplyEffect(world, id, context);
        new BleedEffect(5).ApplyEffect(world, id, context);

        // Стаки [15, 9 (90% от 10), 4 (80% от 5)]: каждый тик уходит самый новый вместе со своей прибавкой
        Assert.Equal(new[] { 28, 24 }, DamageByTurn(world, id, 2));

        // Остался стак 15: новое наложение на 15 даёт ещё стак и +13 (90%, вниз)
        new BleedEffect(15).ApplyEffect(world, id, context);
        Assert.Equal(new[] { 28, 15, 0 }, DamageByTurn(world, id, 3));
    }

    [Fact]
    public void Bleed_AddedDamage_ShrinksByTenPercentPerStack_UpToTenStacks()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new BleedEffect(10).ApplyEffect(world, id, context);
        var bleed = world.StatusEffects[id].ActiveEffects.OfType<BleedEffect>().Single();

        // Прибавка от наложения на 10 урона при стольких стаках до него
        var added = new List<(int StacksBefore, int Extra)>();
        for (int i = 0; i < 12; i++)
        {
            int stacks = bleed.Stacks, power = bleed.Power;
            new BleedEffect(10).ApplyEffect(world, id, context);
            added.Add((stacks, bleed.Power - power));
        }

        Assert.Equal((5, 5), added[4]);
        Assert.Equal((7, 3), added[6]);
        Assert.Equal((9, 1), added[8]);
        // На 10-м стаке — ни урона, ни стака
        Assert.Equal((10, 0), added[9]);
        Assert.Equal(BleedEffect.MaxStacks, bleed.Stacks);
        Assert.Equal(BleedEffect.MaxStacks, bleed.RemainingTurns);
        Assert.Equal(10 + 9 + 8 + 7 + 6 + 5 + 4 + 3 + 2 + 1, bleed.Power);
    }

    [Fact]
    public void Curse_FirstApplicationSetsDuration_DamageSumsFully()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new CurseEffect(1, 5).ApplyEffect(world, id, context);
        new CurseEffect(3, 15).ApplyEffect(world, id, context);
        Assert.Equal(new[] { 20, 0 }, DamageByTurn(world, id, 2));

        (world, id) = Target();
        new CurseEffect(3, 15).ApplyEffect(world, id, context);
        new CurseEffect(2, 10).ApplyEffect(world, id, context);
        new CurseEffect(1, 5).ApplyEffect(world, id, context);
        Assert.Equal(new[] { 30, 30, 30, 0 }, DamageByTurn(world, id, 4));
    }

    [Fact]
    public void Frost_DealsNoDamage_AndSlowsEnergyByStackedPercent()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new FrostEffect(2, 30).ApplyEffect(world, id, context);
        new FrostEffect(1, 20).ApplyEffect(world, id, context);

        int Attack()
        {
            int before = world.Energy[id].Current;
            var turn = new CombatContext { CurrentActor = id };
            world.TurnState[id].CanAct = true;
            world.TurnState[id].ActionCompleted = false;
            new StatusEffectSystem(EffectPhase.Start).Execute(world, turn);
            new DamageSystem().Execute(world, turn);
            new StatusEffectSystem(EffectPhase.End).Execute(world, turn);
            Assert.DoesNotContain(turn.Logs, l => l.EventType == CombatEventType.StatusTick && l.Damage > 0);
            return world.Energy[id].Current - before;
        }

        Assert.Equal(10, Attack()); // −50%
        Assert.Equal(14, Attack()); // −30%: слой на 1 ход истёк
        Assert.Equal(20, Attack()); // мороз спал
    }

    [Fact]
    public void Stun_RefreshesToLongerDuration()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new StunEffect(1).ApplyEffect(world, id, context);
        new StunEffect(2).ApplyEffect(world, id, context);

        var stun = Assert.Single(world.StatusEffects[id].ActiveEffects);
        Assert.Equal(2, stun.RemainingTurns);
    }

    [Fact]
    public void Aura_FiresOncePerInterval()
    {
        var world = new World();
        var bearer = MakeHero("Pyromancer");
        bearer.AuraEffect = "Burn";
        bearer.AuraTarget = AuraTarget.Enemies;
        bearer.AuraTrigger = TriggerPhase.OnRoundStart;
        bearer.AuraPower = 5;
        bearer.AuraInterval = 2;
        int bearerId = Factory.CreateUnit(world, bearer, 1, 1);
        Factory.CreateUnit(world, MakeHero("Enemy"), 2, 1);

        var fired = Enumerable.Range(0, 5).Select(_ =>
        {
            var context = new CombatContext { CurrentActor = bearerId, IsNewRound = true };
            new AuraSystem().Execute(world, context);
            return context.Logs.Any(l => l.EventType == CombatEventType.StatusApplied);
        }).ToList();

        Assert.Equal(new[] { true, false, true, false, true }, fired);
    }

    [Fact]
    public void PeriodicStatus_StacksAtMostToLimit()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        for (int i = 0; i < 5; i++)
        {
            new BleedEffect(10).ApplyEffect(world, id, context, stackLimit: AuraComponent.StackLimit);
            new BurnEffect(5, 4).ApplyEffect(world, id, context, stackLimit: AuraComponent.StackLimit);
        }

        var effects = world.StatusEffects[id].ActiveEffects.Cast<StatusEffectBase>().ToDictionary(e => e.EffectName);
        // Кровотечение: 3 стака и 3 хода, 10 + 9 + 8
        Assert.Equal(3, effects["Bleed"].Stacks);
        Assert.Equal(3, effects["Bleed"].RemainingTurns);
        Assert.Equal(27, effects["Bleed"].Power);
        // Горение: 3 слоя, каждый срезан до 3 ходов
        Assert.Equal(3, effects["Burn"].Stacks);
        Assert.Equal(3, effects["Burn"].RemainingTurns);
        Assert.Equal(12, effects["Burn"].Power);
    }

    // Бой: target против носителей аур (атака 100, сила ауры — процент атаки), урон статуса aura по target за раунд
    private static List<int> AuraTicksByRound(string aura, int duration, int[] powers, int rounds)
    {
        var world = new World();
        for (int i = 0; i < powers.Length; i++)
        {
            var bearer = MakeHero($"Bearer {i}", hp: 100_000);
            bearer.Attack = 100;
            bearer.EnergyPerAttack = 0;
            bearer.AuraEffect = aura;
            bearer.AuraTarget = AuraTarget.Enemies;
            bearer.AuraTrigger = TriggerPhase.OnRoundStart;
            bearer.AuraPower = powers[i];
            bearer.AuraDuration = duration;
            Factory.CreateUnit(world, bearer, 1, i + 1);
        }
        var target = MakeHero("Target", hp: 1_000_000);
        target.Attack = 1;
        target.EnergyPerAttack = 0;
        int targetId = Factory.CreateUnit(world, target, 2, 1);

        var context = new CombatEngine(new Random(1)).Run(world, maxRounds: rounds);
        return context.Logs
            .Where(l => l.EventType == CombatEventType.StatusTick && l.TargetId == targetId && l.ActionType == aura)
            .Select(l => l.Damage)
            .ToList();
    }

    private static (int Turns, int Power, int Stacks) CurseOn(World world, int id)
    {
        var curse = world.StatusEffects[id].ActiveEffects.OfType<CurseEffect>().Single();
        return (curse.RemainingTurns, curse.Power, curse.Stacks);
    }

    [Fact]
    public void Curse_FirstApplicationFixesTurns_LaterOnesAddFullDamage()
    {
        var (world, id) = Target();
        var context = new CombatContext();

        // 1-й скилл: 5 урона на 3 хода
        new CurseEffect(3, 5).ApplyEffect(world, id, context);
        Assert.Equal((3, 5, 1), CurseOn(world, id));
        // 2-й: 10 на 4 хода — ходы остаются от первого, урон прибавляется целиком
        new CurseEffect(4, 10).ApplyEffect(world, id, context);
        Assert.Equal((3, 15, 2), CurseOn(world, id));
        // 3-й: 15 на 3 хода
        new CurseEffect(3, 15).ApplyEffect(world, id, context);
        Assert.Equal((3, 30, 3), CurseOn(world, id));
        Assert.Equal(new[] { 30, 30, 30, 0 }, DamageByTurn(world, id, 4));
    }

    [Fact]
    public void Curse_AddedAfterTick_KeepsRemainingTurns()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new CurseEffect(3, 5).ApplyEffect(world, id, context);
        new CurseEffect(4, 10).ApplyEffect(world, id, context);

        // Один раз сработало: осталось 2 хода по 15
        Assert.Equal(new[] { 15 }, DamageByTurn(world, id, 1));
        Assert.Equal((2, 15, 2), CurseOn(world, id));

        // 3-й скилл на 3 хода по 15 — 2 хода по 30
        new CurseEffect(3, 15).ApplyEffect(world, id, context);
        Assert.Equal((2, 30, 3), CurseOn(world, id));
        Assert.Equal(new[] { 30, 30, 0 }, DamageByTurn(world, id, 3));
    }

    [Fact]
    public void DarkAura_BurnsEnergy_InsteadOfCursing()
    {
        Assert.Equal(EnergyBurnEffect.Name, AuraBook.EffectFor(HeroElement.Dark));
        var definition = AuraBook.Find(EnergyBurnEffect.Name)!;
        Assert.Equal(new[] { 3, 4, 5 }, new[] { 1, 2, 3 }.Select(level => AuraBook.Power(definition, level)));

        var world = new World();
        var bearer = MakeHero("Dark");
        AuraBook.Apply(bearer, definition, 3);
        int bearerId = Factory.CreateUnit(world, bearer, 1, 1);
        var target = MakeHero("Target");
        target.StartEnergy = 12;
        int targetId = Factory.CreateUnit(world, target, 2, 1);

        var context = new CombatContext { CurrentActor = bearerId, IsNewRound = true };
        new AuraSystem().Execute(world, context);

        // Сила — единицы энергии, а не процент атаки: 12 - 5
        Assert.Equal(7, world.Energy[targetId].Current);
        Assert.False(world.StatusEffects.TryGetValue(targetId, out var statuses) && statuses.ActiveEffects.Count > 0);
        var log = Assert.Single(context.Logs, l => l.TargetId == targetId);
        Assert.Equal(CombatEventType.StatusApplied, log.EventType);
        Assert.Equal(EnergyBurnEffect.Name, log.AppliedEffects[0].Name);
        Assert.Equal(5, log.AppliedEffects[0].Power);
        Assert.Equal(7, log.TargetEnergy);

        // Энергии меньше силы — сгорает сколько есть, ниже нуля не уходит
        world.Energy[targetId].Current = 2;
        context = new CombatContext { CurrentActor = bearerId, IsNewRound = true };
        new AuraSystem().Execute(world, context);
        Assert.Equal(0, world.Energy[targetId].Current);
        Assert.Equal(2, context.Logs.Single(l => l.TargetId == targetId).AppliedEffects[0].Power);
    }

    [Fact]
    public void Auras_HaveNoStackLimit()
    {
        // Пять аур горения на 1 ход — все пять слоёв
        Assert.Equal(new[] { 25, 25 }, AuraTicksByRound("Burn", 1, [5, 5, 5, 5, 5], 2));
    }

    [Fact]
    public void BleedAura_StacksUpToTen_AndDamageStopsAtCeiling()
    {
        // Две ауры по 10: за раунд +2 стака, тик снимает самый новый — стаки растут до 10,
        // урон упирается в 10 + 9 + … + 1 = 55 и дальше не растёт
        Assert.Equal(new[] { 19, 27, 34, 40, 45, 49, 52, 54, 55, 55, 55, 55 }, AuraTicksByRound("Bleed", 1, [10, 10], 12));
        // Одна аура: стак накладывается и тут же сходит — ровно её сила каждый раунд
        Assert.Equal(new[] { 10, 10, 10, 10 }, AuraTicksByRound("Bleed", 1, [10], 4));
    }

    // Правило docs/effect-rules.md: источник кровотечения кладёт ровно один стак за наложение —
    // даже с «Мастером стихии», который другим эффектам добавляет ход
    [Fact]
    public void Bleed_EveryApplicationAddsExactlyOneStack()
    {
        var world = new World();
        var caster = MakeHero("Master");
        caster.Passives = [HeroPassive.ElementMastery];
        caster.PassiveLevel = PassiveBook.MaxLevel;
        int casterId = Factory.CreateUnit(world, caster, 1, 1);
        int targetId = Factory.CreateUnit(world, MakeHero("Target"), 2, 1);
        var context = new CombatContext();

        for (int i = 1; i <= 5; i++)
        {
            new BleedEffect(10).ApplyEffect(world, targetId, context, casterId);
            var bleed = world.StatusEffects[targetId].ActiveEffects.OfType<BleedEffect>().Single();
            Assert.Equal(i, bleed.Stacks);
            Assert.Equal(i, bleed.RemainingTurns);
        }
    }

    [Fact]
    public void Combat_IsLostWhenRoundLimitPasses()
    {
        var world = new World();
        Factory.CreateUnit(world, MakeHero("Wall A", hp: 100_000), 1, 1);
        Factory.CreateUnit(world, MakeHero("Wall B", hp: 100_000), 2, 1);

        var context = new CombatEngine(new Random(1)).Run(world, maxRounds: 3);

        var end = context.Logs.Last();
        Assert.Equal(CombatEventType.CombatEnd, end.EventType);
        Assert.Equal("RoundLimitReached", end.ActionType);
        Assert.Equal(2, end.WinningTeam);
        Assert.Equal(3, context.Logs.Where(l => l.EventType == CombatEventType.RoundStart).Max(l => l.Round));
    }

    [Fact]
    public void Combat_DefaultsTo99Rounds()
    {
        Assert.Equal(99, CombatContext.DefaultMaxRounds);
        Assert.Equal(99, new CombatContext().MaxRounds);
    }

    [Fact]
    public void AppliedLog_CarriesCombinedState()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new BurnEffect(3, 15).ApplyEffect(world, id, context);
        new BurnEffect(2, 10).ApplyEffect(world, id, context);

        var state = context.Logs.Last(l => l.EventType == CombatEventType.StatusApplied).AppliedEffects.Single();
        Assert.Equal(3, state.Duration);
        Assert.Equal(25, state.Power);
        Assert.Equal(2, state.Stacks);
    }
}
