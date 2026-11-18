using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// Бонусы стихий отряда: какой бонус даёт состав и как работают механики (SquadBonusBook, SquadBonuses).
// Сила всех бонусов в бою — SquadBonusSimulation
public class SquadBonusTests
{
    private static readonly EntityFactory Factory = new();

    private const HeroElement Fire = HeroElement.Fire, Frost = HeroElement.Frost, Lightning = HeroElement.Lightning,
        Nature = HeroElement.Nature, Dark = HeroElement.Dark, Radiance = HeroElement.Radiance, Blood = HeroElement.Blood,
        Basic = HeroElement.Basic;

    // ===== Состав → бонус =====

    [Theory]
    [InlineData(new[] { Fire, Fire, Fire, Fire, Fire }, SquadBonusId.Fire, 3)]
    [InlineData(new[] { Frost, Frost, Frost, Frost, Fire }, SquadBonusId.Frost, 2)]
    [InlineData(new[] { Blood, Blood, Blood, Fire, Dark }, SquadBonusId.Blood, 1)]
    [InlineData(new[] { Blood, Blood, Blood, Basic, Basic }, SquadBonusId.Blood, 1)]
    [InlineData(new[] { Fire, Fire, Fire, Frost, Frost }, SquadBonusId.FireFrost, 1)]
    [InlineData(new[] { Frost, Frost, Frost, Fire, Fire }, SquadBonusId.FireFrost, 1)]
    [InlineData(new[] { Radiance, Blood, Radiance, Blood, Radiance }, SquadBonusId.RadianceBlood, 1)]
    [InlineData(new[] { Fire, Frost, Lightning, Nature, Dark }, SquadBonusId.Prism, 1)]
    public void Composition_GivesExactlyOneBonus(HeroElement[] elements, SquadBonusId id, int tier)
    {
        var bonus = SquadBonusBook.Resolve(elements)!;
        Assert.Equal(id, bonus.Id);
        Assert.Equal(tier, bonus.Tier);
    }

    [Theory]
    [InlineData(new[] { Fire, Fire, Frost, Frost, Dark })]          // 2 + 2 + 1
    [InlineData(new[] { Fire, Fire, Frost, Nature, Dark })]         // 2 + 1 + 1 + 1
    [InlineData(new[] { Fire, Frost, Lightning, Nature, Basic })]   // 4 разных + Basic — не призма
    [InlineData(new[] { Basic, Basic, Basic, Basic, Basic })]
    public void OtherCompositions_GiveNoBonus(HeroElement[] elements) =>
        Assert.Null(SquadBonusBook.Resolve(elements));

    [Fact]
    public void Catalog_HasSevenMonoOnThreeTiers_TwentyOnePairs_AndPrism()
    {
        var all = SquadBonusBook.All().ToList();
        Assert.Equal(21, all.Count(b => b.Shape == SquadBonusShape.Mono));
        Assert.Equal(21, all.Count(b => b.Shape == SquadBonusShape.Pair));
        Assert.Single(all, b => b.Shape == SquadBonusShape.Prism);
        Assert.Equal(all.Count, all.Distinct().Count());
    }

    [Fact]
    public void SquadGetsBonus_MonstersDoNot()
    {
        var (world, squad, enemies) = Battle([Fire, Fire, Fire, Frost, Frost]);
        Assert.Equal(SquadBonusId.FireFrost, world.SquadBonuses[1].Id);
        Assert.False(world.SquadBonuses.ContainsKey(2));

        // Стихия считается только у дворфов: пять ледяных големов, собранных отрядом, бонуса не дают
        var golems = Enumerable.Range(1, 5).Select(i => { var h = Dwarf(Frost, i); h.UnitType = "Golem"; return h; }).ToList();
        var other = new World();
        Factory.CreateSquad(other, golems, teamId: 2);
        Assert.Empty(other.SquadBonuses);
    }

    // ===== Ауры: раз в 3 раунда, кровотечение — каждый раунд =====

    [Fact]
    public void Auras_FireEveryThreeRounds_ExceptBleed()
    {
        foreach (var effect in AuraBook.Effects)
        {
            var definition = AuraBook.Find(effect)!;
            Assert.Equal(effect == "Bleed" ? 1 : AuraBook.DefaultInterval, definition.Interval);
        }

        var burn = AuraFirings("Burn", 7);
        Assert.Equal(new[] { 1, 4, 7 }, burn);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7 }, AuraFirings("Bleed", 7));
    }

    [Fact]
    public void AuraIntervalArtifact_TurnsThreeRoundsIntoTwo()
    {
        var hero = Dwarf(Fire, 1);
        hero.Equipment = [new PlayerEquipment { Id = Guid.NewGuid(), ItemKey = "WarlordHelm" }];
        var world = new World();
        int id = Factory.CreateUnit(world, hero, 1, 1);
        Assert.Equal(2, world.Auras[id].Interval);
    }

    // Средняя сила ауры за раунд (сила × ходы / интервал) растёт к 3-му уровню втрое
    [Theory]
    [InlineData("Burn")]
    [InlineData("Shock")]
    [InlineData("Regen")]
    public void AuraAveragePerRound_TriplesByLevelThree(string effect)
    {
        var aura = AuraBook.Find(effect)!;
        double PerRound(int level) => (double)AuraBook.Power(aura, level) * AuraBook.Duration(aura, level) / aura.Interval;
        Assert.Equal(3 * PerRound(1), PerRound(3), 3);
    }

    // ===== Механики =====

    [Fact]
    public void FireAndIce_BurnAndFrostExplode_OncePerRound()
    {
        var (world, squad, enemies) = Battle([Fire, Fire, Fire, Frost, Frost], enemyCount: 3);
        var context = new CombatContext();
        int target = enemies[0];

        new BurnEffect(2, 10).ApplyEffect(world, target, context, squad[0]);
        new FrostEffect(2, 20).ApplyEffect(world, target, context, squad[3]);

        // Оба статуса сняты, урон получили все враги: цель — полностью, остальные — долю
        Assert.DoesNotContain(world.StatusEffects[target].ActiveEffects, e => e.EffectName is "Burn" or "Frost");
        var hits = context.Logs.Where(l => l.Phase == SquadBonuses.Phase && l.Damage > 0).ToList();
        Assert.Equal(3, hits.Count);
        int attack = world.Attack[squad[0]].Damage;
        Assert.Equal(attack * SquadBonusBook.FireFrostAttackCap / 100, hits.Single(h => h.TargetId == target).Damage);

        // Второй раз в том же раунде — не взрывается
        new BurnEffect(2, 10).ApplyEffect(world, enemies[1], context, squad[0]);
        new FrostEffect(2, 20).ApplyEffect(world, enemies[1], context, squad[3]);
        Assert.Equal(3, context.Logs.Count(l => l.Phase == SquadBonuses.Phase && l.Damage > 0));
    }

    [Fact]
    public void Permafrost_FrostOverThreshold_FreezesTarget()
    {
        var (world, squad, enemies) = Battle([Frost, Frost, Frost, Frost, Frost]);
        var context = new CombatContext();
        // Порог III — 25%: 15% мороза × (1 + 100%) = 30%
        new FrostEffect(2, 15).ApplyEffect(world, enemies[0], context, squad[0]);

        var statuses = world.StatusEffects[enemies[0]].ActiveEffects;
        Assert.Contains(statuses, e => e is FreezeEffect);
        Assert.DoesNotContain(statuses, e => e is FrostEffect);
    }

    [Fact]
    public void BloodIce_BleedOnFrozenTarget_StacksAtFullStrength()
    {
        var (world, squad, enemies) = Battle([Frost, Frost, Frost, Blood, Blood]);
        var context = new CombatContext();
        int target = enemies[0];
        new BleedEffect(100).ApplyEffect(world, target, context, squad[3]);
        new BleedEffect(100).ApplyEffect(world, target, context, squad[3]);
        // Без мороза второй стак — 90%
        Assert.Equal(190, Bleed(world, target).Power);

        new FrostEffect(2, 10).ApplyEffect(world, target, context, squad[0]);
        new BleedEffect(100).ApplyEffect(world, target, context, squad[3]);
        Assert.Equal(290, Bleed(world, target).Power);
        Assert.Equal(3, Bleed(world, target).Stacks);
    }

    [Fact]
    public void RootsOfTheWorld_RevivesFirstFallenDwarf_OncePerFight_EvenOnTierOne()
    {
        var (world, squad, _) = Battle([Nature, Nature, Nature, Fire, Dark]);
        world.ApplyDamage(squad[0], int.MaxValue / 2);
        new HealthSystem().Execute(world, new CombatContext());
        Assert.Equal(world.Health[squad[0]].Max * SquadBonusBook.NatureRevivePercent[0] / 100 + 1, world.Health[squad[0]].Current);
    }

    [Fact]
    public void RootsOfTheWorld_RevivesFirstFallenDwarf_OncePerFight()
    {
        var (world, squad, _) = Battle([Nature, Nature, Nature, Nature, Nature]);
        var context = new CombatContext();

        world.ApplyDamage(squad[0], int.MaxValue / 2);
        Assert.True(world.IsAlive(squad[0]));
        new HealthSystem().Execute(world, context);
        Assert.Equal(world.Health[squad[0]].Max * SquadBonusBook.NatureRevivePercent[2] / 100 + 1, world.Health[squad[0]].Current);

        world.ApplyDamage(squad[1], int.MaxValue / 2);
        Assert.False(world.IsAlive(squad[1]));
    }

    [Fact]
    public void Halo_OverhealBecomesShield_ThatTakesDamageFirst()
    {
        var (world, squad, _) = Battle([Radiance, Radiance, Radiance, Radiance, Radiance]);
        int id = squad[0];
        int max = world.Health[id].Max;

        world.ApplyHeal(id, max);
        int shield = world.Shields[id];
        Assert.Equal(max * SquadBonusBook.RadianceShieldCap[2] / 100, shield);

        world.ApplyDamage(id, shield + 10);
        Assert.Equal(0, world.Shields[id]);
        Assert.Equal(max - 10, world.Health[id].Current);
    }

    [Fact]
    public void Rot_PoisonOnCursedTarget_HoldsAtMostTwoTurns()
    {
        var (world, squad, enemies) = Battle([Nature, Nature, Nature, Dark, Dark]);
        var context = new CombatContext();
        int target = enemies[0];
        new CurseEffect(10, 1).ApplyEffect(world, target, context, squad[3]);
        new PoisonEffect(1, 10).ApplyEffect(world, target, context, squad[0]);

        // Яд на 1 ход тикает 3 раза: 2 хода удержания и свой
        int ticks = 0;
        for (int turn = 0; turn < 5; turn++)
        {
            var tick = new CombatContext { CurrentActor = target };
            new StatusEffectSystem(EffectPhase.Start).Execute(world, tick);
            ticks += tick.Logs.Count(l => l.EventType == CombatEventType.StatusTick && l.ActionType == "Poison");
        }
        Assert.Equal(3, ticks);
    }

    [Fact]
    public void Overload_EveryCastOnTierThree_StrikesWithLightning()
    {
        var (world, squad, enemies) = Battle([Lightning, Lightning, Lightning, Lightning, Lightning]);
        var context = new CombatContext();
        SquadBonuses.SpellCast(world, context, squad[0]);
        SquadBonuses.SpellCast(world, context, squad[1]);
        Assert.Equal(2, context.Logs.Count(l => l.Phase == SquadBonuses.Phase && l.DamageType == DamageType.Lightning && l.Damage > 0));
    }

    [Fact]
    public void Balance_NightRoundsHitHarder_DayRoundsHeal()
    {
        var (world, squad, enemies) = Battle([Dark, Dark, Dark, Radiance, Radiance]);
        world.Health[squad[0]].Current = 1;

        var night = new CombatContext { CurrentRound = 1, IsNewRound = true, CurrentActor = squad[0] };
        new SquadBonusSystem().Execute(world, night);
        Assert.Equal(SquadBonusBook.DarkRadianceNightPercent, SquadBonuses.HitPercent(world, squad[0], enemies[0]));
        Assert.Equal(1, world.Health[squad[0]].Current);

        var day = new CombatContext { CurrentRound = 2, IsNewRound = true, CurrentActor = squad[0] };
        new SquadBonusSystem().Execute(world, day);
        Assert.Equal(0, SquadBonuses.HitPercent(world, squad[0], enemies[0]));
        Assert.True(world.Health[squad[0]].Current > 1);
    }

    [Fact]
    public void BloodDebt_DamageGrowsWithMissingSquadHp_UpToCap()
    {
        var (world, squad, enemies) = Battle([Blood, Blood, Blood, Blood, Blood]);
        int baseBonus = SquadBonusBook.BloodBaseDamagePercent[2];
        Assert.Equal(baseBonus, SquadBonuses.HitPercent(world, squad[0], enemies[0]));
        foreach (int id in squad) world.Health[id].Current = world.Health[id].Max / 2;
        Assert.Equal(baseBonus + 50 / SquadBonusBook.BloodMissingPerPercent[2], SquadBonuses.HitPercent(world, squad[0], enemies[0]));
        foreach (int id in squad) world.Health[id].Current = 1;
        int rage = Math.Min(SquadBonusBook.BloodDamageCap[2], 100 / SquadBonusBook.BloodMissingPerPercent[2]);
        Assert.Equal(baseBonus + rage, SquadBonuses.HitPercent(world, squad[0], enemies[0]));
    }

    [Fact]
    public void BonusDamage_CountsAsBonusInStats()
    {
        var (world, squad, enemies) = Battle([Fire, Fire, Fire, Frost, Frost]);
        var context = new CombatContext();
        foreach (var id in world.Entities) context.Stats.Register(id.Id, world.Team[id.Id].TeamId);
        new BurnEffect(2, 10).ApplyEffect(world, enemies[0], context, squad[0]);
        new FrostEffect(2, 20).ApplyEffect(world, enemies[0], context, squad[3]);

        // Взрыв — от того, кто замкнул пару (наложил мороз)
        var stats = context.Stats.Build().Single(s => s.EntityId == squad[3]);
        Assert.True(stats.BonusDamage > 0);
        Assert.Equal(stats.BonusDamage, stats.DamageDealt);
    }

    [Fact]
    public void CombatPower_OfSquad_IncludesBonus()
    {
        var mono = Enumerable.Range(0, 5).Select(i => Dwarf(Fire, i + 1)).ToList();
        int withBonus = CombatPower.OfSquad(Factory, mono);
        int alone = mono.Sum(h => CombatPower.OfHero(Factory, h).Total);
        Assert.Equal(alone * (100 + SquadBonusBook.PowerPercent(SquadBonusBook.Mono(Fire, 3))) / 100.0, withBonus, alone * 0.01);
    }

    [Fact]
    public void FullFight_WithEveryBonus_EndsWithoutErrors()
    {
        foreach (var bonus in SquadBonusBook.All())
        {
            var squad = SquadBonusSimulation.Squad(SquadBonusSimulation.Composition(bonus, 0), seed: 1);
            var fight = SquadBonusSimulation.Run(squad, SquadBonusSimulation.Opponent.Mobs, seed: 1, bonus: true);
            Assert.True(fight.Damage > 0, bonus.ToString());
        }
    }

    // 🔥 Горящий враг при смерти передаёт следующему долю оставшегося горения (по уровню бонуса)
    [Theory]
    [InlineData(new[] { Fire, Fire, Fire, Fire, Fire }, 3)]
    [InlineData(new[] { Fire, Fire, Fire, Fire, Frost }, 2)]
    [InlineData(new[] { Fire, Fire, Fire, Frost, Dark }, 1)]
    public void UndyingFlame_BurnJumpsFromDeadEnemy_ByTier(HeroElement[] elements, int tier)
    {
        int share = SquadBonusBook.FireSpreadPercent[tier - 1];
        var (world, squad, enemies) = Battle(elements, enemyCount: 2);
        var context = new CombatContext();
        new BurnEffect(3, 10).ApplyEffect(world, enemies[0], context, squad[0]);

        world.ApplyDamage(enemies[0], int.MaxValue / 2);
        new HealthSystem().Execute(world, context);

        var burn = world.StatusEffects[enemies[1]].ActiveEffects.OfType<BurnEffect>().Single();
        Assert.Equal(3, burn.RemainingTurns);
        Assert.Equal((int)Math.Round(10 * share / 100.0, MidpointRounding.AwayFromZero), burn.Power);
    }

    // ===== Помощники =====

    private static Hero Dwarf(HeroElement element, int place)
    {
        var hero = new Hero
        {
            Id = Guid.NewGuid(),
            Name = $"{element} {place}",
            UnitType = "Dwarf",
            Element = element,
            Level = 4,
            MaxHp = 10000,
            Attack = 1000,
            Speed = 5,
            MaxEnergy = 100,
            EnergyPerAttack = 20,
            PlaceInSquad = place
        };
        if (AuraBook.Find(AuraBook.EffectFor(element)) is { } aura)
            AuraBook.Apply(hero, aura, AuraBook.MinLevel);
        return hero;
    }

    private static (World World, List<int> Squad, List<int> Enemies) Battle(HeroElement[] elements, int enemyCount = 1)
    {
        var world = new World();
        var squad = Factory.CreateSquad(world, elements.Select((e, i) => Dwarf(e, i + 1)), teamId: 1);
        var enemies = Enumerable.Range(0, enemyCount).Select(i => Factory.CreateUnit(world, new Hero
        {
            Id = Guid.NewGuid(), Name = $"Enemy {i}", UnitType = "Golem", Level = 4,
            MaxHp = 1_000_000, Attack = 100, Speed = 1, MaxEnergy = 100, EnergyPerAttack = 20
        }, teamId: 2, position: i + 1)).ToList();
        return (world, squad, enemies);
    }

    private static BleedEffect Bleed(World world, int id) => world.StatusEffects[id].ActiveEffects.OfType<BleedEffect>().Single();

    // Раунды, в которые аура сработала
    private static List<int> AuraFirings(string effect, int rounds)
    {
        var hero = Dwarf(Fire, 1);
        AuraBook.Apply(hero, AuraBook.Find(effect)!, 1);
        var world = new World();
        int bearer = Factory.CreateUnit(world, hero, 1, 1);
        Factory.CreateUnit(world, Dwarf(Basic, 1), 2, 1);
        Factory.CreateUnit(world, Dwarf(Basic, 2), 1, 2);

        var fired = new List<int>();
        for (int round = 1; round <= rounds; round++)
        {
            var context = new CombatContext { CurrentActor = bearer, IsNewRound = true, CurrentRound = round };
            new AuraSystem().Execute(world, context);
            if (context.Logs.Any(l => l.EventType == CombatEventType.StatusApplied && l.AttackerId == bearer)) fired.Add(round);
        }
        return fired;
    }
}
