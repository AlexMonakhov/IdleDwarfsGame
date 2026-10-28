using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// Заклинания стихий, новые статусы, арены и варианты боссов гильдии
public class SpellBookTests
{
    private static readonly EntityFactory Factory = new();

    private static Hero MakeHero(string name, int hp = 300, int attack = 20, int speed = 5) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        MaxHp = hp,
        Attack = attack,
        Speed = speed
    };

    [Fact]
    public void EveryElement_HasThreeSpellFamiliesOnMaxRank()
    {
        foreach (var element in Enum.GetValues<HeroElement>().Where(e => e != HeroElement.Basic))
        {
            var spells = ElementSpellBook.SpellsFor(element, HeroLevels.Max).Select(id => SpellRegistry.All[id]).ToList();

            Assert.Equal(3, spells.Select(s => s.Name).Distinct().Count());
            Assert.All(spells, s => Assert.Equal(3, s.Level));
        }
    }

    [Fact]
    public void SpellBook_OpensSpellsWithRank()
    {
        Assert.Empty(ElementSpellBook.SpellsFor(HeroElement.Basic, HeroLevels.Max));
        Assert.Empty(ElementSpellBook.SpellsFor(HeroElement.Fire, 1));

        var counts = Enumerable.Range(HeroLevels.Min, HeroLevels.Max).Select(rank => ElementSpellBook.SpellsFor(HeroElement.Dark, rank).Count).ToList();
        Assert.Equal(new[] { 0, 1, 2, 3, 3 }, counts);
        Assert.Equal(new[] { 38, 40 }, ElementSpellBook.SpellsFor(HeroElement.Dark, 3)); // Shadow Bolt 2, Life Drain 1
    }

    [Fact]
    public void GeneratedHero_GetsSpellsOfItsElement()
    {
        var hero = HeroGenerator.Create(Guid.NewGuid(), rank: 4, element: "Radiance", new Random(5));

        Assert.Equal(HeroElement.Radiance, hero.Element);
        Assert.Equal(ElementSpellBook.SpellsFor(HeroElement.Radiance, 4), hero.SpellIds);
        Assert.Empty(HeroGenerator.Create(Guid.NewGuid(), rank: 1, element: "", new Random(5)).SpellIds);
    }

    [Fact]
    public void RegistryIds_MatchSpellIds()
    {
        Assert.All(SpellRegistry.All, pair => Assert.Equal(pair.Key, pair.Value.Id));
        Assert.Equal(54, SpellRegistry.All.Count);
    }

    // Каждый спелл каждого уровня кастуется в настоящем бою, а бой проигрывается по логам без расхождений
    [Fact]
    public void EverySpell_CastsAndReplays()
    {
        foreach (var spell in SpellRegistry.All.Values)
        {
            var world = new World();
            var caster = MakeHero("Caster", speed: 10);
            caster.SpellIds = [spell.Id];
            caster.StartEnergy = 100;
            caster.EnergyPerAttack = 100;
            Factory.CreateUnit(world, caster, 1, 1);
            Factory.CreateUnit(world, MakeHero("Ally", hp: 150), 1, 2);
            for (int position = 1; position <= 4; position++)
                Factory.CreateUnit(world, MakeHero($"E{position}", hp: 400, attack: 5, speed: 1), 2, position);

            var units = CombatSnapshot.Units(world);
            var context = new CombatEngine(new Random(7)).Run(world);

            Assert.Contains(context.Logs, l => l.EventType == CombatEventType.SkillCast && l.SkillId == spell.Id);
            CombatEngineTests.AssertLogsReplay(units, context.Logs);

            var estimate = spell.Estimate(20, 3);
            Assert.True(estimate.Damage + estimate.Healing + estimate.StunnedTurns > 0, SpellRegistry.Describe(spell.Id));
        }
    }

    [Fact]
    public void LifeDrain_HealsCaster()
    {
        var world = new World();
        var caster = MakeHero("Vampire", hp: 200);
        caster.SpellIds = [42]; // Life Drain 3
        caster.StartEnergy = 100;
        int casterId = Factory.CreateUnit(world, caster, 1, 1);
        Factory.CreateUnit(world, MakeHero("Victim"), 2, 1);
        world.Health[casterId].Current = 100;

        var context = new CombatContext { CurrentActor = casterId };
        world.TurnState[casterId].CanAct = true;
        new SpellSystem(SpellRegistry.All, new Random(1)).Execute(world, context);

        var heal = Assert.Single(context.Logs, l => l.EventType == CombatEventType.Heal);
        Assert.Equal(casterId, heal.TargetId);
        Assert.True(heal.Heal > 0);
    }

    [Fact]
    public void FreezeSkipsTurn_ButStunImmunityDoesNotHelp()
    {
        var world = new World();
        var victim = MakeHero("Stun-proof");
        victim.Immunities = ["Stun"];
        int victimId = Factory.CreateUnit(world, victim, 2, 1);
        int attacker = Factory.CreateUnit(world, MakeHero("Frost mage"), 1, 1);

        var context = new CombatContext();
        new FreezeEffect(1).ApplyEffect(world, victimId, context, attacker);

        context.CurrentActor = victimId;
        world.TurnState[victimId].CanAct = true;
        new StatusEffectSystem(EffectPhase.Start).Execute(world, context);
        new DamageSystem().Execute(world, context);

        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.Stunned && l.AttackerId == victimId);
        Assert.DoesNotContain(context.Logs, l => l.EventType == CombatEventType.Attack);
    }

    [Theory]
    [InlineData("Poison", DamageType.Poison)]
    [InlineData("Curse", DamageType.Dark)]
    [InlineData("Shock", DamageType.Lightning)]
    public void DamageOverTime_TicksWithItsDamageType(string effectName, DamageType damageType)
    {
        var world = new World();
        int id = Factory.CreateUnit(world, MakeHero("Target", hp: 100), 1, 1);

        var context = new CombatContext { CurrentActor = id };
        StatusEffectFactory.Create(effectName, power: 7, duration: 1).ApplyEffect(world, id, context);
        new StatusEffectSystem(EffectPhase.Start).Execute(world, context);

        var tick = Assert.Single(context.Logs, l => l.EventType == CombatEventType.StatusTick);
        Assert.Equal(7, tick.Damage);
        Assert.Equal(damageType, tick.DamageType);
        Assert.Equal(93, world.Health[id].Current);
    }

    [Fact]
    public void EveryArena_HasElementAndValidEffect()
    {
        var arenas = CampaignSeedData.Arenas;

        Assert.Equal(arenas.Length, arenas.Select(a => a.Id).Distinct().Count());
        Assert.All(arenas, a => Assert.NotEqual(HeroElement.Basic, a.Element));
        Assert.All(arenas, a => Assert.NotNull(ArenaStatusEffect.FromArena(a)));
        // По арене на стихию
        Assert.Equal(Enum.GetValues<HeroElement>().Length - 1, arenas.Select(a => a.Element).Distinct().Count());
    }

    [Fact]
    public void GuildBossVariants_MatchTheirArenaElement_AndRotateDaily()
    {
        var templates = GuildBossSeedData.Heroes.ToDictionary(h => h.Id);
        foreach (var definition in GuildBosses.All)
        {
            Assert.All(definition.Variants, v => Assert.Equal(GuildBosses.ArenaOf(v).Element, templates[v.TemplateHeroId].Element));

            var day = new DateTime(2026, 10, 27, 0, 0, 0, DateTimeKind.Utc);
            var seen = Enumerable.Range(0, definition.Variants.Length)
                .Select(offset => definition.VariantOn(day.AddDays(offset)))
                .ToHashSet();
            Assert.Equal(definition.Variants.Length, seen.Count);
            // Внутри суток вариант не меняется
            Assert.Equal(definition.VariantOn(day), definition.VariantOn(day.AddHours(23)));
        }
    }
}
