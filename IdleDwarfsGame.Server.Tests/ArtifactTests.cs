using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// Артефакты на спеллы (SpellArtifactEffects): по тесту на каждую механику и прогон полного боя со всеми артефактами
public class ArtifactTests
{
    private static readonly EntityFactory Factory = new();

    private const int Fireball1 = 1, Lightning1 = 4, HealingRune1 = 7, Meteor1 = 13, FrostBolt1 = 16, ChainLightning1 = 25,
        Thunderclap1 = 28, VenomThorns1 = 31, EntanglingRoots1 = 34, ShadowBolt1 = 37, LifeDrain1 = 40, Hex1 = 43,
        PrismRay1 = 46, RadiantNova1 = 49, Dawnlight1 = 52, SoulHarvest1 = 55, Hemorrhage1 = 58, BloodPrice1 = 61;

    // Random.Next(max) и NextDouble всегда дают одно значение: так шансы проверяются без случайности
    private sealed class FixedRandom(int roll) : Random
    {
        public override int Next(int maxValue) => Math.Min(roll, Math.Max(0, maxValue - 1));
        public override int Next(int minValue, int maxValue) => Math.Clamp(roll, minValue, Math.Max(minValue, maxValue - 1));
        public override int Next() => roll;
        protected override double Sample() => roll / 100.0;
        public override double NextDouble() => Sample();
    }

    private static Hero MakeHero(HeroElement element, List<int>? spells = null, string? item = null, int attack = 100, int hp = 100000) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test",
        Element = element,
        Level = 3,
        MaxHp = hp,
        Attack = attack,
        Speed = 10,
        SpellIds = spells ?? [],
        Equipment = item == null ? [] : [new PlayerEquipment { Id = Guid.NewGuid(), ItemKey = item }]
    };

    private sealed record Arena(World World, int Hero, List<int> Enemies, List<int> Allies);

    // Герой с предметом, союзники-манекены и враги-манекены с 100000 HP и атакой 1
    private static Arena MakeArena(HeroElement element, int spell, string? item, int enemies = 3, int allies = 0)
    {
        var world = new World();
        int hero = Factory.CreateUnit(world, MakeHero(element, [spell], item), teamId: 1, position: 1);
        var allyIds = Enumerable.Range(2, allies)
            .Select(place => Factory.CreateUnit(world, MakeHero(HeroElement.Basic, attack: 1), teamId: 1, position: place))
            .ToList();
        var enemyIds = Enumerable.Range(1, enemies)
            .Select(place => Factory.CreateUnit(world, MakeHero(HeroElement.Basic, attack: 1), teamId: 2, position: place))
            .ToList();
        return new Arena(world, hero, enemyIds, allyIds);
    }

    private static List<CombatLogEntry> Casts(CombatContext context, string spell) =>
        context.Logs.Where(l => l.EventType == CombatEventType.SkillCast && l.SkillName == spell).ToList();

    private static List<CombatLogEntry> Hits(CombatContext context, string spell) =>
        context.Logs.Where(l => l.EventType == CombatEventType.Damage && l.SkillName == spell).ToList();

    private static T? Status<T>(World world, int unit) where T : class, IStatusEffect =>
        world.StatusEffects.TryGetValue(unit, out var c) ? c.ActiveEffects.OfType<T>().FirstOrDefault() : null;

    private static void RunTurnStart(World world, CombatContext context, int unit)
    {
        context.CurrentActor = unit;
        new StatusEffectSystem(EffectPhase.Start).Execute(world, context);
    }

    // ===== Огонь =====

    [Fact]
    public void PhoenixStaff_RecastsFireballOnKill_UpToTwoTimes()
    {
        var a = MakeArena(HeroElement.Fire, Fireball1, "PhoenixStaff", enemies: 4);
        a.Enemies.ForEach(e => a.World.Health[e].Current = 1);
        var context = new CombatContext();

        SpellRegistry.All[Fireball1].Execute(a.World, a.Hero, context);

        // Каст и два повтора: каждый убивает, цепочка обрывается на втором повторе
        Assert.Equal(3, Casts(context, "Fireball").Count);
        Assert.Single(a.Enemies, a.World.IsAlive);
        // Повтор — отдельное действие с 70% урона
        Assert.Equal(3, Casts(context, "Fireball").Select(c => c.ActionId).Distinct().Count());
    }

    [Fact]
    public void PhoenixStaff_NoKill_NoRecast()
    {
        var a = MakeArena(HeroElement.Fire, Fireball1, "PhoenixStaff");
        var context = new CombatContext();

        SpellRegistry.All[Fireball1].Execute(a.World, a.Hero, context);

        Assert.Single(Casts(context, "Fireball"));
    }

    [Fact]
    public void VolcanoHeart_MeteorDetonatesBurns()
    {
        var a = MakeArena(HeroElement.Fire, Meteor1, "VolcanoHeart");
        var context = new CombatContext();
        new BurnEffect(turns: 3, damagePerTurn: 10).ApplyEffect(a.World, a.Enemies[2], context);

        SpellRegistry.All[Meteor1].Execute(a.World, a.Hero, context);

        // 3 хода × 10 × 150% — разом, горение снято
        var blast = Assert.Single(Hits(context, "Flame Wave"));
        Assert.Equal(a.Enemies[2], blast.TargetId);
        Assert.Equal(45, blast.Damage);
        Assert.Null(Status<BurnEffect>(a.World, a.Enemies[2]));
        Assert.Contains(context.Logs, l => l.EventType == CombatEventType.StatusExpired && l.TargetId == a.Enemies[2] && l.ActionType == "Burn");
    }

    // ===== Лёд =====

    [Fact]
    public void WinterShard_FrostBoltFreezesAlreadyFrostedTarget()
    {
        var a = MakeArena(HeroElement.Frost, FrostBolt1, "WinterShard", enemies: 1);
        var context = new CombatContext();
        var bolt = SpellRegistry.All[FrostBolt1];

        bolt.Execute(a.World, a.Hero, context);
        Assert.Null(Status<FreezeEffect>(a.World, a.Enemies[0]));

        bolt.Execute(a.World, a.Hero, context);
        Assert.NotNull(Status<FreezeEffect>(a.World, a.Enemies[0]));
    }

    [Fact]
    public void GlacierShackles_FreezeShattersOnNeighboursWhenItEnds()
    {
        var a = MakeArena(HeroElement.Frost, 22, "GlacierShackles");
        var context = new CombatContext();
        new FreezeEffect(1).ApplyEffect(a.World, a.Enemies[1], context, a.Hero);

        RunTurnStart(a.World, context, a.Enemies[1]);

        int attack = a.World.Attack[a.Hero].Damage;
        var shards = Hits(context, "Frost Bolt");
        Assert.Equal(new[] { a.Enemies[0], a.Enemies[2] }, shards.Select(l => l.TargetId).OrderBy(id => id));
        Assert.All(shards, l => Assert.Equal(attack * 80 / 100, l.Damage));
    }

    [Fact]
    public void GlacierShackles_FrozenUnitDeathShatters()
    {
        var a = MakeArena(HeroElement.Frost, 22, "GlacierShackles");
        var context = new CombatContext();
        new FreezeEffect(1).ApplyEffect(a.World, a.Enemies[0], context, a.Hero);
        a.World.Health[a.Enemies[0]].Current = 0;

        new HealthSystem().Execute(a.World, context);

        // У крайнего в линии один сосед
        var shard = Assert.Single(Hits(context, "Frost Bolt"));
        Assert.Equal(a.Enemies[1], shard.TargetId);
    }

    [Fact]
    public void GlacierShackles_IgnoresFreezeFromOthers()
    {
        var a = MakeArena(HeroElement.Frost, 22, "GlacierShackles");
        var context = new CombatContext();
        new FreezeEffect(1).ApplyEffect(a.World, a.Enemies[1], context);

        RunTurnStart(a.World, context, a.Enemies[1]);

        Assert.Empty(Hits(context, "Frost Bolt"));
    }

    // ===== Молния =====

    [Fact]
    public void ThunderHammer_EveryThirdLightningCastsTwice()
    {
        var a = MakeArena(HeroElement.Lightning, Lightning1, "ThunderHammer");
        var context = new CombatContext();
        var lightning = SpellRegistry.All[Lightning1];

        lightning.Execute(a.World, a.Hero, context);
        lightning.Execute(a.World, a.Hero, context);
        Assert.Equal(2, Casts(context, "Lightning").Count);

        lightning.Execute(a.World, a.Hero, context);
        Assert.Equal(4, Casts(context, "Lightning").Count);
    }

    [Fact]
    public void StormCoil_ChainLightningRefundsEnergyPerHit()
    {
        var a = MakeArena(HeroElement.Lightning, ChainLightning1, "StormCoil");
        a.World.Energy[a.Hero].Current = 0;
        var context = new CombatContext();

        SpellRegistry.All[ChainLightning1].Execute(a.World, a.Hero, context);

        // Chain Lightning 1 бьёт двоих: 2 × 4
        Assert.Equal(8, a.World.Energy[a.Hero].Current);
        var gain = Assert.Single(context.Logs, l => l.ActionType == ItemEffectActions.EnergyGain);
        Assert.Equal(8, gain.AppliedEffects[0].Power);
        Assert.Equal(8, gain.TargetEnergy);
    }

    [Fact]
    public void ThunderHorn_StunnedTargetsGetLightning()
    {
        var a = MakeArena(HeroElement.Lightning, Thunderclap1, "ThunderHorn");
        var context = new CombatContext { Random = new FixedRandom(0) };

        SpellRegistry.All[Thunderclap1].Execute(a.World, a.Hero, context);

        int attack = a.World.Attack[a.Hero].Damage;
        var bolts = Hits(context, "Lightning");
        Assert.Equal(a.Enemies, bolts.Select(l => l.TargetId).OrderBy(id => id).ToList());
        Assert.All(bolts, l => Assert.Equal(attack * 120 / 100, l.Damage));
    }

    [Fact]
    public void ThunderHorn_NoStun_NoLightning()
    {
        var a = MakeArena(HeroElement.Lightning, Thunderclap1, "ThunderHorn");
        var context = new CombatContext { Random = new FixedRandom(99) };

        SpellRegistry.All[Thunderclap1].Execute(a.World, a.Hero, context);

        Assert.Empty(Hits(context, "Lightning"));
    }

    // ===== Природа =====

    [Fact]
    public void LifeTreeSeed_OverhealBecomesVenomThorns()
    {
        var a = MakeArena(HeroElement.Nature, HealingRune1, "LifeTreeSeed");
        var context = new CombatContext();

        SpellRegistry.All[HealingRune1].Execute(a.World, a.Hero, context);

        // Все здоровы: руна (200% атаки) целиком уходит в избыток
        var thorn = Assert.Single(Hits(context, "Venom Thorns"));
        Assert.Equal((int)(a.World.Attack[a.Hero].Damage * 2.0), thorn.Damage);
        Assert.NotNull(Status<PoisonEffect>(a.World, thorn.TargetId));
    }

    [Fact]
    public void LifeTreeSeed_NoOverheal_NoThorns()
    {
        var a = MakeArena(HeroElement.Nature, HealingRune1, "LifeTreeSeed");
        a.World.Health[a.Hero].Current = 1000;
        var context = new CombatContext();

        SpellRegistry.All[HealingRune1].Execute(a.World, a.Hero, context);

        Assert.Empty(Hits(context, "Venom Thorns"));
    }

    [Fact]
    public void WyvernSting_PoisonSpreadsToNeighboursOnDeath()
    {
        var a = MakeArena(HeroElement.Nature, VenomThorns1, "WyvernSting", enemies: 4);
        var context = new CombatContext();
        new PoisonEffect(turns: 3, damagePerTurn: 10).ApplyEffect(a.World, a.Enemies[1], context, a.Hero);
        a.World.Health[a.Enemies[1]].Current = 0;

        new HealthSystem().Execute(a.World, context);

        // Двое ближайших: по бокам от погибшего, дальний — нет
        Assert.Equal(30, Status<PoisonEffect>(a.World, a.Enemies[0])!.RemainingDamage);
        Assert.Equal(30, Status<PoisonEffect>(a.World, a.Enemies[2])!.RemainingDamage);
        Assert.Null(Status<PoisonEffect>(a.World, a.Enemies[3]));
        Assert.Single(Casts(context, "Venom Thorns"));
    }

    [Fact]
    public void ElderGroveStaff_RootedEnemyHealsMostWoundedAlly()
    {
        var a = MakeArena(HeroElement.Nature, EntanglingRoots1, "ElderGroveStaff", allies: 2);
        a.World.Health[a.Allies[1]].Current = 1000;
        var context = new CombatContext();

        SpellRegistry.All[EntanglingRoots1].Execute(a.World, a.Hero, context);

        var heal = Assert.Single(context.Logs, l => l.EventType == CombatEventType.Heal);
        Assert.Equal(a.Allies[1], heal.TargetId);
        Assert.Equal(a.World.Attack[a.Hero].Damage * 120 / 100, heal.Heal);
        Assert.Equal("Healing Rune", heal.SkillName);
    }

    // ===== Тьма =====

    [Fact]
    public void AbyssalSigil_MarkedTargetTakesMoreSpellDamage()
    {
        var a = MakeArena(HeroElement.Dark, ShadowBolt1, "AbyssalSigil", enemies: 1);
        var context = new CombatContext();
        var bolt = SpellRegistry.All[ShadowBolt1];

        bolt.Execute(a.World, a.Hero, context);
        bolt.Execute(a.World, a.Hero, context);

        int first = (int)(a.World.Attack[a.Hero].Damage * 1.75);
        var hits = Hits(context, "Shadow Bolt");
        Assert.Equal(first, hits[0].Damage);
        Assert.Equal(first * 130 / 100, hits[1].Damage);
        Assert.NotNull(Status<MarkEffect>(a.World, a.Enemies[0]));
    }

    [Fact]
    public void CurseDoll_CurseTickTriggersShadowBolt()
    {
        var a = MakeArena(HeroElement.Dark, Hex1, "CurseDoll");
        var context = new CombatContext { Random = new FixedRandom(0) };
        new CurseEffect(turns: 3, damagePerTurn: 10).ApplyEffect(a.World, a.Enemies[0], context, a.Hero);

        RunTurnStart(a.World, context, a.Enemies[0]);

        var bolt = Assert.Single(Hits(context, "Shadow Bolt"));
        Assert.Equal(a.Enemies[0], bolt.TargetId);
        Assert.Equal(a.World.Attack[a.Hero].Damage * 60 / 100, bolt.Damage);
    }

    [Fact]
    public void CurseDoll_FailedRoll_NoBolt()
    {
        var a = MakeArena(HeroElement.Dark, Hex1, "CurseDoll");
        var context = new CombatContext { Random = new FixedRandom(99) };
        new CurseEffect(turns: 3, damagePerTurn: 10).ApplyEffect(a.World, a.Enemies[0], context, a.Hero);

        RunTurnStart(a.World, context, a.Enemies[0]);

        Assert.Empty(Hits(context, "Shadow Bolt"));
    }

    [Fact]
    public void ReaperScythe_ConsumedCurseHealsSquad()
    {
        var a = MakeArena(HeroElement.Dark, SoulHarvest1, "ReaperScythe", enemies: 1, allies: 1);
        a.World.Health[a.Hero].Current = 1000;
        a.World.Health[a.Allies[0]].Current = 1000;
        var context = new CombatContext();
        new CurseEffect(turns: 3, damagePerTurn: 20).ApplyEffect(a.World, a.Enemies[0], context, a.Hero);

        SpellRegistry.All[SoulHarvest1].Execute(a.World, a.Hero, context);

        // Снято 60 урона проклятия → 50% = 30, поровну на двоих
        var heals = context.Logs.Where(l => l.EventType == CombatEventType.Heal).ToList();
        Assert.Equal(2, heals.Count);
        Assert.All(heals, h => Assert.Equal(15, h.Heal));
    }

    // ===== Свет =====

    [Fact]
    public void PrismLens_TriggersRandomElementBolt()
    {
        var a = MakeArena(HeroElement.Radiance, PrismRay1, "PrismLens");
        var context = new CombatContext { Random = new FixedRandom(0) };

        SpellRegistry.All[PrismRay1].Execute(a.World, a.Hero, context);

        // Первый в списке — Fireball: 70% атаки и горение
        var fire = Assert.Single(Hits(context, "Fireball"));
        Assert.Equal(a.World.Attack[a.Hero].Damage * 70 / 100, fire.Damage);
        Assert.NotNull(Status<BurnEffect>(a.World, fire.TargetId));
    }

    [Fact]
    public void SunDisc_CleansesWorstStatusAndStrikesBack()
    {
        var a = MakeArena(HeroElement.Radiance, RadiantNova1, "SunDisc", allies: 1);
        var context = new CombatContext { Random = new FixedRandom(0) };
        new BurnEffect(2, 10).ApplyEffect(a.World, a.Allies[0], context);
        new StunEffect(1).ApplyEffect(a.World, a.Allies[0], context);

        SpellRegistry.All[RadiantNova1].Execute(a.World, a.Hero, context);

        // Снимается одно — самое опасное (оглушение), горение остаётся
        Assert.Null(Status<StunEffect>(a.World, a.Allies[0]));
        Assert.NotNull(Status<BurnEffect>(a.World, a.Allies[0]));
        var strike = Assert.Single(Hits(context, "Prism Ray"));
        Assert.Equal(a.World.Attack[a.Hero].Damage * 50 / 100, strike.Damage);
    }

    [Fact]
    public void MorningStar_LowAllyTriggersRadiantNova()
    {
        var a = MakeArena(HeroElement.Radiance, Dawnlight1, "MorningStar", allies: 1);
        a.World.Health[a.Allies[0]].Current = a.World.Health[a.Allies[0]].Max / 5;
        var context = new CombatContext();

        SpellRegistry.All[Dawnlight1].Execute(a.World, a.Hero, context);

        int attack = a.World.Attack[a.Hero].Damage;
        var nova = Hits(context, "Radiant Nova");
        Assert.Equal(a.Enemies, nova.Select(l => l.TargetId).OrderBy(id => id).ToList());
        Assert.All(nova, l => Assert.Equal(attack * 40 / 100, l.Damage));
    }

    [Fact]
    public void MorningStar_HealthySquad_NoNova()
    {
        var a = MakeArena(HeroElement.Radiance, Dawnlight1, "MorningStar", allies: 1);
        var context = new CombatContext();

        SpellRegistry.All[Dawnlight1].Execute(a.World, a.Hero, context);

        Assert.Empty(Hits(context, "Radiant Nova"));
    }

    // ===== Кровь =====

    [Fact]
    public void BloodMoonFang_LifeDrainStrikesAgainBelowHalfHp()
    {
        var a = MakeArena(HeroElement.Blood, LifeDrain1, "BloodMoonFang", enemies: 1);
        a.World.Health[a.Enemies[0]].Max = 20000;
        a.World.Health[a.Enemies[0]].Current = 10400;
        var context = new CombatContext();

        SpellRegistry.All[LifeDrain1].Execute(a.World, a.Hero, context);

        // После удара (200% атаки) цель ниже половины HP — повтор 60%, он тоже лечит
        int first = (int)(a.World.Attack[a.Hero].Damage * 2.0);
        var hits = Hits(context, "Life Drain");
        Assert.Equal(new[] { first, first * 60 / 100 }, hits.Select(h => h.Damage));
        Assert.Equal(2, context.Logs.Count(l => l.EventType == CombatEventType.Heal));
    }

    [Fact]
    public void ThousandCutsDagger_BleedBurstsAtFiveStacks()
    {
        var a = MakeArena(HeroElement.Blood, Hemorrhage1, "ThousandCutsDagger", enemies: 1);
        var context = new CombatContext();
        for (int i = 0; i < 4; i++)
            new BleedEffect(10).ApplyEffect(a.World, a.Enemies[0], context, a.Hero);

        SpellRegistry.All[Hemorrhage1].Execute(a.World, a.Hero, context);

        var bleed = Status<BleedEffect>(a.World, a.Enemies[0])!;
        Assert.Equal(1, bleed.Stacks);
        var hits = Hits(context, "Hemorrhage");
        Assert.Equal(2, hits.Count);
        // Стаки 10, 9, 8, 7 и пятый — кровотечение Hemorrhage (20% атаки) × 60%: лопается урон трёх тиков
        int fifth = 6 * (int)(a.World.Attack[a.Hero].Damage * 0.2) / 10;
        Assert.Equal((10 + 9 + 8 + 7 + fifth) * 3, hits[1].Damage);
    }

    [Fact]
    public void SacrificialAltar_PaidHpHealsAllies()
    {
        var a = MakeArena(HeroElement.Blood, BloodPrice1, "SacrificialAltar", allies: 1);
        a.World.Health[a.Allies[0]].Current = 1000;
        var context = new CombatContext();

        SpellRegistry.All[BloodPrice1].Execute(a.World, a.Hero, context);

        // Плата — 10% максимального HP кастера, всё уходит единственному союзнику
        int price = a.World.Health[a.Hero].Max / 10;
        var heal = Assert.Single(context.Logs, l => l.EventType == CombatEventType.Heal);
        Assert.Equal(a.Allies[0], heal.TargetId);
        Assert.Equal(price, heal.Heal);
    }

    // ===== Каталог и бой целиком =====

    private static IEnumerable<(EquipmentDefinition Item, SpellBoundEffect Effect)> ArtifactSpellEffects() =>
        EquipmentCatalog.All
            .Where(i => i.Rarity == Rarity.Artifact)
            .SelectMany(i => i.Effects.OfType<SpellBoundEffect>().Select(e => (i, e)));

    [Fact]
    public void EveryArtifact_TargetsAnExistingSpellOfItsElement()
    {
        foreach (var (item, effect) in ArtifactSpellEffects())
        {
            var spell = SpellRegistry.All.Values.FirstOrDefault(s => s.Name == effect.Spell);
            Assert.True(spell != null, $"{item.Key}: нет спелла {effect.Spell}");
            var element = ElementSpellBook.ElementOf(ElementSpellBook.FamilyOf(spell!.Id)!.Value);
            Assert.Contains(element, item.Elements);
        }
    }

    [Fact]
    public void EverySpell_HasExactlyOneArtifact()
    {
        var bySpell = ArtifactSpellEffects().GroupBy(x => x.Effect.Spell).ToDictionary(g => g.Key, g => g.Select(x => x.Item.Key).Distinct().Count());
        var families = ElementSpellBook.Elements.SelectMany(ElementSpellBook.FamiliesOf).Select(id => SpellRegistry.All[id].Name).ToList();
        Assert.All(families, name => Assert.Equal(1, bySpell.GetValueOrDefault(name)));
    }

    [Fact]
    public void EveryArtifact_RaisesSpellPower()
    {
        foreach (var (item, effect) in ArtifactSpellEffects())
        {
            var spell = SpellRegistry.All.Values.First(s => s.Name == effect.Spell && s.Level == 1);
            var element = item.Elements[0];
            // Атака предмета сама поднимает мощь — сравниваем с тем же предметом без эффектов
            var plain = PowerWith(MakeHero(element, [spell.Id]), item.Stats);
            var geared = CombatPower.OfHero(Factory, MakeHero(element, [spell.Id], item.Key)).Total;
            Assert.True(geared > plain, $"{item.Key}: мощь {geared} не больше {plain}");
        }
    }

    // Мощь героя со статами предмета, но без его эффектов
    private static int PowerWith(Hero hero, ItemStats stats)
    {
        hero.MaxHp += stats.MaxHp;
        hero.Attack += stats.Attack;
        hero.Speed += stats.Speed;
        return CombatPower.OfHero(Factory, hero).Total;
    }

    // Полный бой: отряд с артефактом против такого же отряда. Бой доходит до конца, все срабатывания корректны
    [Fact]
    public void FullCombat_WithEveryArtifact_Finishes()
    {
        foreach (var (item, effect) in ArtifactSpellEffects())
        {
            var family = SpellRegistry.All.Values.Where(s => s.Name == effect.Spell).Select(s => s.Id).ToList();
            var element = item.Elements[0];
            for (int seed = 0; seed < 3; seed++)
            {
                var world = new World();
                for (int place = 1; place <= 3; place++)
                {
                    Factory.CreateUnit(world, MakeHero(element, family, item.Key, hp: 2000), teamId: 1, position: place);
                    Factory.CreateUnit(world, MakeHero(element, family, item.Key, hp: 2000), teamId: 2, position: place);
                }

                var context = new CombatEngine(new Random(seed)).Run(world);

                Assert.True(context.CombatEnded, $"{item.Key}: бой не закончился (сид {seed})");
                Assert.Contains(context.Logs, l => l.EventType == CombatEventType.SkillCast && l.SkillName == effect.Spell);
                // Мёртвые не действуют: после смерти юнит не кастует и не бьёт
                var deaths = context.Logs.Where(l => l.EventType == CombatEventType.Death).ToDictionary(l => l.TargetId, l => l.Sequence);
                Assert.DoesNotContain(context.Logs, l =>
                    l.EventType is CombatEventType.SkillCast or CombatEventType.Attack
                    && deaths.TryGetValue(l.AttackerId, out int died) && l.Sequence > died);
            }
        }
    }
}
