using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// Пассивки дворфов (PassiveBook): каждая проверяется на одном ударе, касте или смерти
public class PassiveTests
{
    private static readonly EntityFactory Factory = new();

    // Случайность, при которой срабатывает любой шанс
    private sealed class AlwaysRandom : Random
    {
        public override int Next(int maxValue) => 0;
        public override double NextDouble() => 0;
    }

    private static Hero Unit(int attack = 100, int hp = 100_000, HeroPassive? passive = null, int level = 1, List<int>? spells = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test",
        Element = HeroElement.Fire,
        Level = 3,
        MaxHp = hp,
        Attack = attack,
        Speed = 5,
        SpellIds = spells ?? [],
        Passives = passive is { } p ? [p] : [],
        PassiveLevel = level
    };

    private static (World World, int Hero, List<int> Enemies) Arena(Hero hero, int enemies = 3, Hero? enemy = null)
    {
        var world = new World();
        int heroId = Factory.CreateUnit(world, hero, teamId: 1, position: 1);
        var enemyIds = Enumerable.Range(1, enemies)
            .Select(place => Factory.CreateUnit(world, enemy ?? Unit(attack: 1), teamId: 2, position: place))
            .ToList();
        return (world, heroId, enemyIds);
    }

    private static CombatContext Attack(World world, int attacker, bool lucky = false)
    {
        var context = new CombatContext { CurrentActor = attacker, Random = lucky ? new AlwaysRandom() : new Random(1) };
        new DamageSystem().Execute(world, context);
        return context;
    }

    private static List<CombatLogEntry> Hits(CombatContext context) =>
        context.Logs.Where(l => l.EventType == CombatEventType.Attack).ToList();

    [Fact]
    public void AllPassives_HaveValuesOnEveryLevel()
    {
        foreach (var passive in Enum.GetValues<HeroPassive>())
            for (int level = PassiveBook.MinLevel; level <= PassiveBook.MaxLevel; level++)
                Assert.True(PassiveBook.Values(passive, level).Percent > 0, $"{passive} {level}");
    }

    [Fact]
    public void CriticalStrike_DoublesTheHit()
    {
        var (world, id, _) = Arena(Unit(passive: HeroPassive.CriticalStrike));

        var hit = Assert.Single(Hits(Attack(world, id, lucky: true)));

        Assert.Equal(200, hit.Damage);
        Assert.True(hit.IsCritical);
    }

    [Fact]
    public void Cleave_HitsTheNextEnemyInLine()
    {
        var (world, id, enemies) = Arena(Unit(passive: HeroPassive.Cleave, level: 2));

        var hits = Hits(Attack(world, id));

        Assert.Equal([enemies[0], enemies[1]], hits.Select(h => h.TargetId));
        Assert.Equal(45, hits[1].Damage);
        Assert.Equal(nameof(HeroPassive.Cleave), hits[1].SkillName);
    }

    [Fact]
    public void Executioner_HitsLowTargetsHarder()
    {
        var (world, id, enemies) = Arena(Unit(passive: HeroPassive.Executioner), enemies: 1);
        world.Health[enemies[0]].Current = world.Health[enemies[0]].Max / 5;

        Assert.Equal(130, Assert.Single(Hits(Attack(world, id))).Damage);
    }

    [Fact]
    public void StoneSkin_ReducesIncomingDamage()
    {
        var world = new World();
        int attacker = Factory.CreateUnit(world, Unit(), 1, 1);
        int target = Factory.CreateUnit(world, Unit(passive: HeroPassive.StoneSkin, level: 3), 2, 1);

        Assert.Equal(84, world.ResolveHit(attacker, target, 100));
    }

    [Fact]
    public void RuneResonance_AddsEnergyPerAttack()
    {
        var (world, id, _) = Arena(Unit(passive: HeroPassive.RuneResonance, level: 3));

        Attack(world, id);

        Assert.Equal(20 + 12, world.Energy[id].Current);
    }

    [Fact]
    public void Vampirism_HealsForShareOfDamage()
    {
        var (world, id, _) = Arena(Unit(passive: HeroPassive.Vampirism, hp: 1000));
        world.Health[id].Current = 500;

        var context = Attack(world, id);

        Assert.Equal(510, world.Health[id].Current);
        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.Heal && l.SkillName == nameof(HeroPassive.Vampirism));
    }

    [Fact]
    public void Thorns_HurtTheAttacker()
    {
        var world = new World();
        int attacker = Factory.CreateUnit(world, Unit(), 1, 1);
        int target = Factory.CreateUnit(world, Unit(attack: 200, passive: HeroPassive.Thorns), 2, 1);

        var context = Attack(world, attacker);

        var reflected = Assert.Single(context.Logs, l => l.EventType == CombatEventType.Damage);
        Assert.Equal(target, reflected.AttackerId);
        Assert.Equal(attacker, reflected.TargetId);
        Assert.Equal(40, reflected.Damage);
    }

    [Fact]
    public void Sorcerer_BoostsSpellDamage()
    {
        const int prismRay1 = 46;
        var (world, id, _) = Arena(Unit(passive: HeroPassive.Sorcerer, spells: [prismRay1]), enemies: 1);
        var context = new CombatContext();

        SpellRegistry.All[prismRay1].Execute(world, id, context);

        // 190% × 115%
        Assert.Equal(190 * 115 / 100, context.Logs.Single(l => l.EventType == CombatEventType.Damage).Damage);
    }

    [Fact]
    public void LastStand_SavesOnceAndHeals()
    {
        var world = new World();
        int attacker = Factory.CreateUnit(world, Unit(attack: 5000), 1, 1);
        int hero = Factory.CreateUnit(world, Unit(hp: 1000, passive: HeroPassive.LastStand), 2, 1);
        var context = Attack(world, attacker);

        new HealthSystem().Execute(world, context);

        Assert.True(world.IsAlive(hero));
        Assert.Equal(1 + 200, world.Health[hero].Current);
        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.Heal && l.SkillName == nameof(HeroPassive.LastStand));

        world.TurnState[attacker].ActionCompleted = false;
        Attack(world, attacker);
        Assert.False(world.IsAlive(hero));
    }

    [Fact]
    public void Avenger_GainsAttackWhenAllyDies()
    {
        var world = new World();
        int avenger = Factory.CreateUnit(world, Unit(passive: HeroPassive.Avenger), 1, 1);
        int ally = Factory.CreateUnit(world, Unit(hp: 100), 1, 2);
        Factory.CreateUnit(world, Unit(), 2, 1);

        world.ApplyDamage(ally, 1000);
        new HealthSystem().Execute(world, new CombatContext());

        Assert.Equal(120, world.Attack[avenger].Damage);
    }

    [Fact]
    public void ElementMastery_EmpowersOwnDamageOverTime()
    {
        var world = new World();
        int caster = Factory.CreateUnit(world, Unit(passive: HeroPassive.ElementMastery), 1, 1);
        int target = Factory.CreateUnit(world, Unit(), 2, 1);
        var context = new CombatContext();

        new BurnEffect(turns: 2, damagePerTurn: 50).ApplyEffect(world, target, context, caster);

        var burn = context.Logs.Single(l => l.EventType == CombatEventType.StatusApplied).AppliedEffects[0];
        Assert.Equal(60, burn.Power);
        Assert.Equal(3, burn.Duration);
    }

    [Fact]
    public void DoubleStrike_IsAChance()
    {
        // Шанс первого уровня — 20%: из 200 ударов второй замах бывает примерно у 40
        int doubles = Enumerable.Range(0, 200).Count(seed =>
        {
            var (w, h, _) = Arena(Unit(passive: HeroPassive.DoubleStrike));
            var context = new CombatContext { CurrentActor = h, Random = new Random(seed) };
            new DamageSystem().Execute(w, context);
            return Hits(context).Count == 2;
        });

        Assert.InRange(doubles, 20, 70);
    }

    [Fact]
    public void Power_GrowsWithEveryPassive()
    {
        int plain = CombatPower.OfHero(Factory, Unit(hp: 1000, spells: [46])).Total;
        foreach (var passive in Enum.GetValues<HeroPassive>().Where(p => p != HeroPassive.SurvivalOfTheWeakest))
        {
            var power = CombatPower.OfHero(Factory, Unit(hp: 1000, spells: [46], passive: passive, level: 3));
            Assert.True(power.Total > plain, passive.ToString());
            Assert.True(power.Passives > 0, passive.ToString());
        }
    }
}
