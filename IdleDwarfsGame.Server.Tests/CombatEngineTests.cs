using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class CombatEngineTests
{
    private readonly ITestOutputHelper _output;

    public CombatEngineTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly EntityFactory Factory = new();

    private static World CreateSeededWorld(int floor)
    {
        var world = new World();
        var heroes = HeroSeedData.Heroes;
        var squad = heroes.Where(h => h.PlayerId == HeroSeedData.DemoPlayerId && h.PlaceInSquad != null).OrderBy(h => h.PlaceInSquad);
        var enemies = heroes.Where(h => h.PlayerId == HeroSeedData.EnemyPlayerId).OrderBy(h => h.PlaceInSquad);

        Factory.CreateSquad(world, squad, teamId: 1);
        Factory.CreateSquad(world, enemies, teamId: 2, level: floor);
        world.AddGlobalEffect(new LavaArenaEffect());
        return world;
    }

    private static Hero MakeHero(string name, int hp = 100, int attack = 10, int speed = 5) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        MaxHp = hp,
        Attack = attack,
        Speed = speed
    };

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void SeededFight_EndsAndLogsReplayConsistently(int floor)
    {
        int wins = 0;
        var rounds = new List<int>();

        for (int seed = 0; seed < 50; seed++)
        {
            var world = CreateSeededWorld(floor);
            var units = CombatSnapshot.Units(world);
            var context = new CombatEngine(new Random(seed)).Run(world);

            Assert.True(context.CombatEnded);
            Assert.Equal(CombatEventType.CombatStart, context.Logs.First().EventType);
            Assert.Equal(CombatEventType.CombatEnd, context.Logs.Last().EventType);
            Assert.Single(context.Logs, l => l.EventType == CombatEventType.CombatEnd);
            Assert.Equal(Enumerable.Range(0, context.Logs.Count), context.Logs.Select(l => l.Sequence));

            AssertLogsReplay(units, context.Logs);

            if (context.Win) wins++;
            rounds.Add(context.CurrentRound);
        }

        _output.WriteLine($"Floor {floor}: player wins {wins}/50, rounds avg {rounds.Average():0.0} (min {rounds.Min()}, max {rounds.Max()})");
    }

    // Проигрываем бой только по логам, как это делает фронт, и сверяем HP с тем, что прислал сервер
    internal static void AssertLogsReplay(List<CombatUnitInfo> units, List<CombatLogEntry> logs)
    {
        var hp = units.ToDictionary(u => u.EntityId, u => u.Hp);
        var maxHp = units.ToDictionary(u => u.EntityId, u => u.MaxHp);
        var dead = new HashSet<int>();

        foreach (var log in logs)
        {
            switch (log.EventType)
            {
                case CombatEventType.Attack:
                case CombatEventType.Damage:
                case CombatEventType.StatusTick:
                    if (log.EventType != CombatEventType.StatusTick)
                        Assert.DoesNotContain(log.AttackerId, dead);
                    Assert.DoesNotContain(log.TargetId, dead);
                    hp[log.TargetId] = Math.Max(0, hp[log.TargetId] - log.Damage);
                    Assert.Equal(hp[log.TargetId], log.TargetRemainingHp);
                    break;

                case CombatEventType.SkillCast:
                    Assert.DoesNotContain(log.AttackerId, dead);
                    Assert.NotNull(log.SkillId);
                    break;

                case CombatEventType.Heal:
                    Assert.DoesNotContain(log.TargetId, dead);
                    hp[log.TargetId] = Math.Min(maxHp[log.TargetId], hp[log.TargetId] + log.Heal);
                    Assert.Equal(hp[log.TargetId], log.TargetRemainingHp);
                    break;

                case CombatEventType.Death:
                    Assert.Equal(0, hp[log.TargetId]);
                    Assert.True(dead.Add(log.TargetId), $"Unit {log.TargetId} died twice");
                    break;
            }
        }

        // Все, у кого HP дошло до нуля, должны получить запись о смерти
        Assert.All(hp.Where(p => p.Value == 0), p => Assert.Contains(p.Key, dead));
    }

    [Fact]
    public void DeadUnitsNeverAct()
    {
        var world = new World();
        Factory.CreateUnit(world, MakeHero("Glass cannon", hp: 5, attack: 1, speed: 1), 1, 1);
        Factory.CreateUnit(world, MakeHero("Tank", hp: 500, attack: 50, speed: 1), 1, 2);
        Factory.CreateUnit(world, MakeHero("Fast killer", hp: 100, attack: 10, speed: 10), 2, 1);

        var context = new CombatEngine(new Random(1)).Run(world);

        // Glass cannon умирает от первого удара Fast killer и больше не появляется атакующим
        var deathIndex = context.Logs.FindIndex(l => l.EventType == CombatEventType.Death && l.TargetId == 1);
        Assert.True(deathIndex > 0);
        Assert.DoesNotContain(context.Logs.Skip(deathIndex), l => l.AttackerId == 1);
    }

    [Fact]
    public void StunnedUnitSkipsTurn()
    {
        var world = new World();
        int attacker = Factory.CreateUnit(world, MakeHero("A", speed: 10), 1, 1);
        int victim = Factory.CreateUnit(world, MakeHero("B", speed: 1), 2, 1);

        var context = new CombatContext();
        new StunEffect(1).ApplyEffect(world, victim, context, attacker);

        // Ход жертвы
        context.CurrentActor = victim;
        world.TurnState[victim].CanAct = true;
        new StatusEffectSystem(EffectPhase.Start).Execute(world, context);
        new DamageSystem().Execute(world, context);

        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.Stunned && l.AttackerId == victim);
        Assert.DoesNotContain(context.Logs, l => l.EventType == CombatEventType.Attack);
        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.StatusExpired && l.TargetId == victim);
    }

    [Fact]
    public void ImmuneUnitIgnoresEffect()
    {
        var world = new World();
        var hero = MakeHero("Fireproof");
        hero.Immunities = ["Burn"];
        int id = Factory.CreateUnit(world, hero, 1, 1);

        var context = new CombatContext();
        new BurnEffect(2, 10).ApplyEffect(world, id, context);

        Assert.Empty(world.StatusEffects[id].ActiveEffects);
        Assert.Single(context.Logs, l => l.EventType == CombatEventType.Immune);
    }

    [Fact]
    public void AuraCreatesSeparateEffectPerTarget()
    {
        var world = new World();
        var bearer = MakeHero("Healer");
        bearer.AuraEffect = "Regen";
        bearer.AuraTarget = AuraTarget.Allies;
        bearer.AuraTrigger = TriggerPhase.OnRoundStart;
        bearer.AuraPower = 5;
        bearer.AuraDuration = 2;
        int a = Factory.CreateUnit(world, bearer, 1, 1);
        int b = Factory.CreateUnit(world, MakeHero("Ally"), 1, 2);
        Factory.CreateUnit(world, MakeHero("Enemy"), 2, 1);

        var context = new CombatContext { CurrentActor = a, IsNewRound = true };
        new AuraSystem().Execute(world, context);

        var effectA = Assert.Single(world.StatusEffects[a].ActiveEffects);
        var effectB = Assert.Single(world.StatusEffects[b].ActiveEffects);
        Assert.NotSame(effectA, effectB);

        effectA.RemainingTurns--;
        Assert.Equal(2, effectB.RemainingTurns);
    }

    [Fact]
    public void FactoryMapsHeroFieldsToComponents()
    {
        var world = new World();
        var hero = MakeHero("Mage", hp: 100, attack: 10);
        hero.MaxEnergy = 80;
        hero.StartEnergy = 30;
        hero.EnergyPerAttack = 15;
        hero.SpellIds = [1, 4, 999];
        hero.Immunities = ["Stun"];

        int id = Factory.CreateUnit(world, hero, 2, 3, level: 2);

        Assert.Equal(120, world.Health[id].Max);
        Assert.Equal(12, world.Attack[id].Damage);
        Assert.Equal(30, world.Energy[id].Current);
        Assert.Equal(80, world.Energy[id].Max);
        Assert.Equal(15, world.Energy[id].GainPerAttack);
        Assert.Equal(new[] { 1, 4 }, world.Spells[id].SpellIds); // неизвестный спелл отброшен
        Assert.Contains("Stun", world.Immunities[id].ImmuneTo);
        Assert.Equal(3, world.Info[id].Position);
        Assert.Equal(2, world.Team[id].TeamId);
    }

    [Fact]
    public void SpellCastIsLoggedBeforeItsHits()
    {
        var world = new World();
        var mage = MakeHero("Mage", attack: 10, speed: 10);
        mage.SpellIds = [2]; // Fireball 2
        mage.StartEnergy = 100;
        Factory.CreateUnit(world, mage, 1, 1);
        Factory.CreateUnit(world, MakeHero("E1", hp: 500), 2, 1);
        Factory.CreateUnit(world, MakeHero("E2", hp: 500), 2, 2);

        var context = new CombatEngine(new Random(3)).Run(world);

        var cast = context.Logs.First(l => l.EventType == CombatEventType.SkillCast);
        var hits = context.Logs.Where(l => l.EventType == CombatEventType.Damage && l.ActionId == cast.ActionId).ToList();

        Assert.Equal("Fireball", cast.SkillName);
        Assert.Equal(2, hits.Count);
        Assert.All(hits, h => Assert.True(h.Sequence > cast.Sequence));
        Assert.All(hits, h => Assert.Equal(DamageType.Fire, h.DamageType));
    }
}
