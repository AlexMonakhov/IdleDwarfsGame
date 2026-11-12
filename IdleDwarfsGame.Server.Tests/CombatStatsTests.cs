using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class CombatStatsTests
{
    private static readonly EntityFactory Factory = new();

    private static Hero MakeHero(string name, int hp = 100, int attack = 10, int speed = 5) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        MaxHp = hp,
        Attack = attack,
        Speed = speed
    };

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

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Stats_MatchLogs(int floor)
    {
        for (int seed = 0; seed < 20; seed++)
        {
            var world = CreateSeededWorld(floor);
            var context = new CombatEngine(new Random(seed)).Run(world);
            var stats = context.Stats.Build();

            Assert.Equal(world.Entities.Count, stats.Count);

            // Полученный урон и лечение — ровно то, что в логах по цели
            foreach (var unit in stats)
            {
                Assert.Equal(context.Logs.Where(l => l.TargetId == unit.EntityId).Sum(l => l.Damage), unit.DamageTaken);
                Assert.Equal(context.Logs.Where(l => l.TargetId == unit.EntityId).Sum(l => l.Heal), unit.HealingReceived);
                Assert.Equal(unit.AttackDamage + unit.SkillDamage + unit.AuraDamage + unit.PassiveDamage, unit.DamageDealt);
                Assert.Equal(unit.SkillHealing + unit.AuraHealing + unit.PassiveHealing, unit.HealingDone);
            }

            // Весь урон бойцов по врагам расписан по ним; урон арены и плата своим HP — никому
            int dealt = stats.Sum(s => s.DamageDealt);
            int taken = stats.Sum(s => s.DamageTaken);
            Assert.True(dealt <= taken);
            Assert.Equal(context.Logs.Sum(l => l.Heal), stats.Sum(s => s.HealingDone));

            int deaths = context.Logs.Count(l => l.EventType == CombatEventType.Death);
            Assert.True(stats.Sum(s => s.Kills) <= deaths);
        }
    }

    [Fact]
    public void BasicAttack_CountsAsAttackDamage()
    {
        var world = new World();
        int hitter = Factory.CreateUnit(world, MakeHero("Hitter", attack: 30, speed: 10), 1, 1);
        int victim = Factory.CreateUnit(world, MakeHero("Victim", hp: 60, attack: 1, speed: 1), 2, 1);

        var context = new CombatEngine(new Random(1)).Run(world);
        var stats = context.Stats.Build().ToDictionary(s => s.EntityId);

        Assert.Equal(60, stats[victim].DamageTaken);
        Assert.Equal(stats[hitter].AttackDamage, stats[hitter].DamageDealt);
        Assert.True(stats[hitter].AttackDamage >= 60);
        Assert.Equal(1, stats[hitter].Kills);
    }

    [Fact]
    public void BurnTick_SplitsBetweenSpellAndAuraSources()
    {
        var world = new World();
        int caster = Factory.CreateUnit(world, MakeHero("Caster"), 1, 1);
        int bearer = Factory.CreateUnit(world, MakeHero("Bearer"), 1, 2);
        int victim = Factory.CreateUnit(world, MakeHero("Victim", hp: 1000), 2, 1);

        var context = new CombatContext();
        context.Stats.Register(caster, 1);
        context.Stats.Register(bearer, 1);
        context.Stats.Register(victim, 2);

        // Горение от спелла (30 за ход) и от ауры (10 за ход) складываются слоями в один статус
        new BurnEffect(turns: 2, damagePerTurn: 30).ApplyEffect(world, victim, context, caster);
        context.StatusKind = CombatStatKind.Aura;
        new BurnEffect(turns: 2, damagePerTurn: 10).ApplyEffect(world, victim, context, bearer);
        context.StatusKind = null;

        world.StatusEffects[victim].ActiveEffects.Single().OnTurnStart(world, victim, context);

        var stats = context.Stats.Build().ToDictionary(s => s.EntityId);
        Assert.Equal(40, stats[victim].DamageTaken);
        Assert.Equal(30, stats[caster].SkillDamage);
        Assert.Equal(10, stats[bearer].AuraDamage);
        Assert.Equal(0, stats[bearer].SkillDamage);
    }

    [Fact]
    public void ArenaBurn_IsTakenButNotDealtByAnyone()
    {
        var world = new World();
        int victim = Factory.CreateUnit(world, MakeHero("Victim", hp: 1000), 1, 1);
        int enemy = Factory.CreateUnit(world, MakeHero("Enemy"), 2, 1);

        var context = new CombatContext();
        context.Stats.Register(victim, 1);
        context.Stats.Register(enemy, 2);

        new BurnEffect(turns: 1, damagePerTurn: 25).ApplyEffect(world, victim, context);
        world.StatusEffects[victim].ActiveEffects.Single().OnTurnStart(world, victim, context);

        var stats = context.Stats.Build().ToDictionary(s => s.EntityId);
        Assert.Equal(25, stats[victim].DamageTaken);
        Assert.Equal(0, stats[enemy].DamageDealt);
    }

    [Fact]
    public void RegenAura_CountsAsAuraHealing()
    {
        var world = new World();
        int bearer = Factory.CreateUnit(world, MakeHero("Healer"), 1, 1);
        int ally = Factory.CreateUnit(world, MakeHero("Ally", hp: 100), 1, 2);
        Factory.CreateUnit(world, MakeHero("Enemy"), 2, 1);
        world.Health[ally].Current = 50;

        var context = new CombatContext();
        context.Stats.Register(bearer, 1);
        context.Stats.Register(ally, 1);
        context.StatusKind = CombatStatKind.Aura;
        new RegenEffect(turns: 1, healPerTurn: 20).ApplyEffect(world, ally, context, bearer);
        context.StatusKind = null;

        world.StatusEffects[ally].ActiveEffects.Single().OnTurnStart(world, ally, context);

        var stats = context.Stats.Build().ToDictionary(s => s.EntityId);
        Assert.Equal(20, stats[bearer].AuraHealing);
        Assert.Equal(20, stats[ally].HealingReceived);
    }
}
