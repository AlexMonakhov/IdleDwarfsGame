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
    public void Bleed_EachStackAddsTurn_AndFollowingStacksHitHalf()
    {
        var (world, id) = Target();
        var context = new CombatContext();
        new BleedEffect(15).ApplyEffect(world, id, context);
        new BleedEffect(10).ApplyEffect(world, id, context);
        new BleedEffect(5).ApplyEffect(world, id, context);

        // 15 + 5 + 3 (2.5 вверх) на три хода
        Assert.Equal(new[] { 23, 23 }, DamageByTurn(world, id, 2));

        // Остался один стак: новое наложение на 15 даёт ещё стак и +8
        new BleedEffect(15).ApplyEffect(world, id, context);
        Assert.Equal(new[] { 31, 31, 0 }, DamageByTurn(world, id, 3));
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
        // Кровотечение: 3 стака и 3 хода, 10 + 5 + 5
        Assert.Equal(3, effects["Bleed"].Stacks);
        Assert.Equal(3, effects["Bleed"].RemainingTurns);
        Assert.Equal(20, effects["Bleed"].Power);
        // Горение: 3 слоя, каждый срезан до 3 ходов
        Assert.Equal(3, effects["Burn"].Stacks);
        Assert.Equal(3, effects["Burn"].RemainingTurns);
        Assert.Equal(12, effects["Burn"].Power);
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
