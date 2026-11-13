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
    public void EveryElement_HasItsSpellFamilies()
    {
        foreach (var element in Enum.GetValues<HeroElement>().Where(e => e != HeroElement.Basic))
        {
            var spells = ElementSpellBook.FamiliesOf(element).Select(id => SpellRegistry.All[id]).ToList();

            Assert.Equal(3, spells.Select(s => s.Name).Distinct().Count());
            Assert.All(spells, s => Assert.Equal(1, s.Level));
            Assert.All(spells, s => Assert.Equal(element, ElementSpellBook.ElementOf(s.Id)));
        }
        Assert.Empty(ElementSpellBook.FamiliesOf(HeroElement.Basic));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    [InlineData(5, 3)]
    public void SpellLevel_GrowsWithRank(int rank, int level)
    {
        Assert.Equal(level, ElementSpellBook.SpellLevelFor(rank));
    }

    [Fact]
    public void FamilyOf_FindsFirstLevelOfEverySpell()
    {
        Assert.Equal(37, ElementSpellBook.FamilyOf(39)); // Shadow Bolt 3
        Assert.Equal(40, ElementSpellBook.FamilyOf(40)); // Life Drain 1
        Assert.Null(ElementSpellBook.FamilyOf(999));
    }

    // Дворф знает одно заклинание своей стихии, его уровень — по рангу
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void GeneratedHero_GetsOneSpellOfItsElement(int rank)
    {
        var random = new Random(5);
        for (int i = 0; i < 50; i++)
        {
            var hero = HeroGenerator.Create(Guid.NewGuid(), rank, element: "Radiance", random);

            var spell = SpellRegistry.All[Assert.Single(hero.SpellIds)];
            Assert.Equal(rank - 1, spell.Level);
            Assert.Equal(HeroElement.Radiance, ElementSpellBook.ElementOf(ElementSpellBook.FamilyOf(spell.Id)!.Value));
        }
        Assert.Empty(HeroGenerator.Create(Guid.NewGuid(), rank: 1, element: "", random).SpellIds);
    }

    // Случайная стихия героя из сундука: выпадают все, и каждый герой получает заклинание своей стихии
    [Fact]
    public void GeneratedHero_RandomElementCoversEveryElement()
    {
        var random = new Random(5);
        var heroes = Enumerable.Range(0, 500).Select(_ => HeroGenerator.Create(Guid.NewGuid(), rank: 4, element: "", random)).ToList();

        Assert.Equal(Enum.GetValues<HeroElement>().Where(e => e != HeroElement.Basic).Order(), heroes.Select(h => h.Element).Distinct().Order());
        Assert.All(heroes, h => Assert.Contains(ElementSpellBook.FamilyOf(Assert.Single(h.SpellIds))!.Value, ElementSpellBook.FamiliesOf(h.Element)));
    }

    [Fact]
    public void RegistryIds_MatchSpellIds()
    {
        Assert.All(SpellRegistry.All, pair => Assert.Equal(pair.Key, pair.Value.Id));
        Assert.Equal(63, SpellRegistry.All.Count);
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

    // Один каст спелла кастером в позиции 1 против врагов с заданным HP
    private static (World world, CombatContext context, int casterId, List<int> enemies) CastOnce(int spellId, int casterHp = 200, int enemies = 3)
    {
        var world = new World();
        var caster = MakeHero("Caster", hp: casterHp);
        caster.SpellIds = [spellId];
        caster.StartEnergy = 100;
        int casterId = Factory.CreateUnit(world, caster, 1, 1);
        var enemyIds = Enumerable.Range(1, enemies).Select(p => Factory.CreateUnit(world, MakeHero($"E{p}", hp: 1000), 2, p)).ToList();
        return (world, new CombatContext { CurrentActor = casterId }, casterId, enemyIds);
    }

    private static void Cast(World world, CombatContext context, int casterId)
    {
        world.TurnState[casterId].CanAct = true;
        new SpellSystem(SpellRegistry.All, new Random(1)).Execute(world, context);
    }

    [Fact]
    public void SoulHarvest_PrefersCursed_DetonatesAndRemovesCurse()
    {
        var (world, context, casterId, enemies) = CastOnce(57); // Soul Harvest 3: три цели, возврат энергии
        int cursed = enemies[2];
        new CurseEffect(turns: 3, damagePerTurn: 10).ApplyEffect(world, cursed, new CombatContext(), casterId);

        Cast(world, context, casterId);

        var hits = context.Logs.Where(l => l.EventType == CombatEventType.Damage).ToList();
        int baseDamage = (int)(20 * 1.9);
        Assert.Equal(baseDamage + 30, Assert.Single(hits, h => h.TargetId == cursed).Damage);
        Assert.All(hits.Where(h => h.TargetId != cursed), h => Assert.Equal(baseDamage, h.Damage));
        Assert.Empty(world.StatusEffects[cursed].ActiveEffects);
        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.StatusExpired && l.TargetId == cursed);
        // 100 - 45 за каст + 10 за снятое проклятие
        Assert.Equal(65, world.Energy[casterId].Current);
    }

    [Fact]
    public void SoulHarvest_FirstPicksCursedEnemies()
    {
        var (world, context, casterId, enemies) = CastOnce(55, enemies: 4); // две цели
        new CurseEffect().ApplyEffect(world, enemies[1], new CombatContext(), casterId);
        new CurseEffect().ApplyEffect(world, enemies[3], new CombatContext(), casterId);

        Cast(world, context, casterId);

        var targets = context.Logs.Where(l => l.EventType == CombatEventType.Damage).Select(l => l.TargetId).Order();
        Assert.Equal(new[] { enemies[1], enemies[3] }, targets);
    }

    [Fact]
    public void Hemorrhage_BleedsItsTargets()
    {
        var (world, context, casterId, _) = CastOnce(60); // Hemorrhage 3: три цели
        Cast(world, context, casterId);

        var hits = context.Logs.Where(l => l.EventType == CombatEventType.Damage).ToList();
        Assert.Equal(3, hits.Count);
        Assert.All(hits, h => Assert.Equal(DamageType.Bleed, h.DamageType));
        Assert.All(hits, h => Assert.IsType<BleedEffect>(Assert.Single(world.StatusEffects[h.TargetId].ActiveEffects)));
    }

    [Fact]
    public void BloodPrice_CasterPaysHp_BeforeHittingEveryEnemy()
    {
        var (world, context, casterId, enemies) = CastOnce(63, casterHp: 200); // Blood Price 3
        Cast(world, context, casterId);

        var damage = context.Logs.Where(l => l.EventType == CombatEventType.Damage).ToList();
        Assert.Equal(casterId, damage[0].TargetId);
        Assert.Equal(20, damage[0].Damage);
        Assert.Equal(180, world.Health[casterId].Current);
        Assert.Equal(enemies, damage.Skip(1).Select(d => d.TargetId));
        Assert.All(enemies, id => Assert.IsType<BleedEffect>(Assert.Single(world.StatusEffects[id].ActiveEffects)));
    }

    [Fact]
    public void BloodPrice_NeverKillsCaster()
    {
        var (world, context, casterId, _) = CastOnce(61);
        world.Health[casterId].Current = 5;
        Cast(world, context, casterId);

        Assert.Equal(1, world.Health[casterId].Current);
        world.Health[casterId].Current = 1;
        var again = new CombatContext { CurrentActor = casterId };
        world.Energy[casterId].Current = 100;
        Cast(world, again, casterId);
        Assert.Equal(1, world.Health[casterId].Current);
        Assert.DoesNotContain(again.Logs, l => l.EventType == CombatEventType.Damage && l.TargetId == casterId);
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

    [Fact]
    public void HeroInfo_DescribesSpellCostDamageAndEnergyGain()
    {
        var hero = MakeHero("Caster", attack: 100);
        hero.SpellIds = [2, 7]; // Fireball 2, Healing Rune 1
        hero.EnergyPerAttack = 20;

        var info = HeroInfo.From(Factory, hero);

        Assert.Equal(20, info.EnergyPerAttack);
        var fireball = info.SpellDetails[0];
        Assert.Equal("Fireball 2", fireball.Spell);
        Assert.Equal(20, fireball.EnergyCost);
        // 85% атаки + горение 2 хода по 10%
        Assert.Equal(105, fireball.Damage);
        Assert.Equal(3, fireball.Targets);

        var rune = info.SpellDetails[1];
        Assert.Equal(0, rune.Damage);
        Assert.True(rune.Healing > 0);
    }
}
