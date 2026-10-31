using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class EquipmentTests : IDisposable
{
    private const int FlameWave1 = 10;
    private const int MeteorLevel1 = 13;

    private static readonly EntityFactory Factory = new();

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly EquipmentService _service;
    private readonly Guid _playerId = Guid.NewGuid();

    public EquipmentTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Tester" });
        _context.SaveChanges();

        _service = new EquipmentService(
            new EquipmentRepository(_context), new HeroesRepository(_context), new ExpeditionRepository(_context), Factory);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static Hero MakeHero(
        HeroElement element = HeroElement.Fire,
        int attack = 100,
        int hp = 100000,
        int speed = 10,
        int rank = 3,
        int energy = 0,
        List<int>? spells = null,
        List<HeroPassive>? passives = null,
        params string[] items) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test",
        Element = element,
        Level = rank,
        MaxHp = hp,
        Attack = attack,
        Speed = speed,
        StartEnergy = energy,
        SpellIds = spells ?? [],
        Passives = passives ?? [],
        Equipment = items.Select(key => new PlayerEquipment { Id = Guid.NewGuid(), ItemKey = key }).ToList()
    };

    private static Hero MakeDummy(int attack = 1) => MakeHero(attack: attack, speed: 1);

    private static (World World, int Hero, List<int> Enemies) Arena(Hero hero, int enemies = 3)
    {
        var world = new World();
        int heroId = Factory.CreateUnit(world, hero, teamId: 1, position: 1);
        var enemyIds = Enumerable.Range(1, enemies)
            .Select(place => Factory.CreateUnit(world, MakeDummy(), teamId: 2, position: place))
            .ToList();
        return (world, heroId, enemyIds);
    }

    // ===== Спеллы =====

    [Fact]
    public void ArmageddonBlade_AddsTargetAndMeteorOnEachTarget()
    {
        var hero = MakeHero(spells: [FlameWave1], items: "ArmageddonBlade");
        var (world, heroId, enemies) = Arena(hero);
        int attack = world.Attack[heroId].Damage;
        var context = new CombatContext();

        SpellRegistry.All[FlameWave1].Execute(world, heroId, context);

        // Flame Wave 1 бьёт двоих, клинок добавляет третьего
        var wave = context.Logs.Where(l => l.EventType == CombatEventType.Damage && l.SkillName == "Flame Wave").ToList();
        Assert.Equal(enemies, wave.Select(l => l.TargetId).OrderBy(id => id).ToList());

        // Метеор — отдельное действие с одним кастом на все цели, 50% атаки и 1 уровень
        var cast = Assert.Single(context.Logs, l => l.EventType == CombatEventType.SkillCast && l.SkillName == "Meteor");
        Assert.Equal(MeteorLevel1, cast.SkillId);
        var meteors = context.Logs.Where(l => l.EventType == CombatEventType.Damage && l.SkillName == "Meteor").ToList();
        Assert.Equal(enemies, meteors.Select(l => l.TargetId).OrderBy(id => id).ToList());
        Assert.All(meteors, l =>
        {
            Assert.Equal(attack / 2, l.Damage);
            Assert.Equal(cast.ActionId, l.ActionId);
        });
        Assert.NotEqual(wave[0].ActionId, cast.ActionId);
    }

    [Fact]
    public void SpellModifier_DoesNothingForOtherSpells()
    {
        const int fireball1 = 1;
        var hero = MakeHero(spells: [fireball1], items: "ArmageddonBlade");
        var (world, heroId, _) = Arena(hero);
        var context = new CombatContext();

        SpellRegistry.All[fireball1].Execute(world, heroId, context);

        Assert.DoesNotContain(context.Logs, l => l.SkillName == "Meteor");
        Assert.Single(context.Logs, l => l.EventType == CombatEventType.Damage);
    }

    [Fact]
    public void Proc_SkipsTargetsKilledByTheSpell()
    {
        var hero = MakeHero(spells: [FlameWave1], items: "ArmageddonBlade");
        var (world, heroId, enemies) = Arena(hero);
        world.Health[enemies[0]].Current = 1;
        var context = new CombatContext();

        SpellRegistry.All[FlameWave1].Execute(world, heroId, context);

        var meteors = context.Logs.Where(l => l.EventType == CombatEventType.Damage && l.SkillName == "Meteor").ToList();
        Assert.Equal(2, meteors.Count);
        Assert.DoesNotContain(meteors, l => l.TargetId == enemies[0]);
    }

    [Fact]
    public void SpellDamageItem_RaisesDamage()
    {
        const int fireball1 = 1;
        var plain = Arena(MakeHero(spells: [fireball1]), enemies: 1);
        var geared = Arena(MakeHero(spells: [fireball1], items: "EmberMace"), enemies: 1);
        var plainContext = new CombatContext();
        var gearedContext = new CombatContext();

        SpellRegistry.All[fireball1].Execute(plain.World, plain.Hero, plainContext);
        SpellRegistry.All[fireball1].Execute(geared.World, geared.Hero, gearedContext);

        int plainDamage = plainContext.Logs.Single(l => l.EventType == CombatEventType.Damage).Damage;
        int gearedDamage = gearedContext.Logs.Single(l => l.EventType == CombatEventType.Damage).Damage;
        // +6 атаки и +20% урона спелла
        Assert.Equal((int)(106 * 1.2) * 120 / 100, gearedDamage);
        Assert.True(gearedDamage > plainDamage);
    }

    // ===== Статы и ауры =====

    [Fact]
    public void ItemStats_AddOnTopOfHero()
    {
        var hero = MakeHero(items: ["WarlordHelm", "HeartOfTheMountain"]);
        var (world, id, _) = Arena(hero);

        Assert.Equal(100000 + 60 + 40, world.Health[id].Max);
        Assert.Equal(103, world.Attack[id].Damage);
        Assert.Equal(20, world.Energy[id].Current);
        Assert.Equal(25, world.Energy[id].GainPerAttack);
    }

    [Fact]
    public void Helmet_GrantsAuraToHeroWithoutOne_AndBoostsExisting()
    {
        var plain = MakeHero(items: "EmberCrown");
        var granted = Arena(plain).World.Auras.Values.Single();
        Assert.Equal("Burn", granted.EffectName);
        Assert.Equal(4, granted.Power); // 3 × 120% с округлением

        var own = MakeHero(items: "WarlordHelm");
        own.AuraEffect = "Regen";
        own.AuraTarget = AuraTarget.Allies;
        own.AuraPower = 10;
        own.AuraInterval = 2;
        var boosted = Arena(own).World.Auras.Values.Single();
        Assert.Equal("Regen", boosted.EffectName);
        Assert.Equal(14, boosted.Power);
        Assert.Equal(1, boosted.Interval);
    }

    [Fact]
    public void GrantedAura_DoesNotReplaceOwnAura()
    {
        var hero = MakeHero(items: "EmberCrown");
        hero.AuraEffect = "Regen";
        hero.AuraPower = 10;

        var aura = Arena(hero).World.Auras.Values.Single();

        Assert.Equal("Regen", aura.EffectName);
        Assert.Equal(12, aura.Power);
    }

    // ===== Пассивки =====

    [Fact]
    public void Survival_ReducesDamageOnlyFromStrongerAttackers()
    {
        var world = new World();
        int weak = Factory.CreateUnit(world, MakeHero(attack: 10, rank: 3, passives: [HeroPassive.SurvivalOfTheWeakest]), 1, 1);
        int strong = Factory.CreateUnit(world, MakeHero(attack: 100), 2, 1);
        int weaker = Factory.CreateUnit(world, MakeHero(attack: 5), 2, 2);

        // Ранг 3 — второй уровень пассивки, -10%
        Assert.Equal(90, world.ResolveHit(strong, weak, 100));
        Assert.Equal(100, world.ResolveHit(weaker, weak, 100));
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(3, 10)]
    [InlineData(5, 15)]
    public void PassiveLevel_GrowsWithRank(int rank, int percent)
    {
        var world = new World();
        int id = Factory.CreateUnit(world, MakeHero(rank: rank, passives: [HeroPassive.SurvivalOfTheWeakest]), 1, 1);

        Assert.Equal(percent, world.GetPassive(id, HeroPassive.SurvivalOfTheWeakest)!.Value.Percent);
    }

    [Fact]
    public void Rings_BoostPassive()
    {
        var world = new World();
        int id = Factory.CreateUnit(
            world,
            MakeHero(rank: 1, passives: [HeroPassive.SurvivalOfTheWeakest], items: ["RingOfTheUnderdog", "RingOfTheStoneSkin"]),
            1, 1);

        Assert.Equal(5 + 3 + 5, world.GetPassive(id, HeroPassive.SurvivalOfTheWeakest)!.Value.Percent);
    }

    [Fact]
    public void Ring_DoesNothingWithoutThePassive()
    {
        var world = new World();
        int id = Factory.CreateUnit(world, MakeHero(passives: [HeroPassive.DoubleStrike], items: "RingOfTheUnderdog"), 1, 1);

        Assert.Null(world.GetPassive(id, HeroPassive.SurvivalOfTheWeakest));
    }

    private static CombatContext BasicAttack(World world, int attacker)
    {
        var context = new CombatContext { CurrentActor = attacker };
        new DamageSystem().Execute(world, context);
        return context;
    }

    [Fact]
    public void SharpTongue_BurnsOnEachBasicAttack()
    {
        var (world, id, enemies) = Arena(MakeHero(rank: 3, passives: [HeroPassive.SharpTongue]));

        var context = BasicAttack(world, id);

        // Ранг 3 — 20% от атаки на 1 ход
        var applied = Assert.Single(context.Logs, l => l.EventType == CombatEventType.StatusApplied);
        Assert.Equal(enemies[0], applied.TargetId);
        Assert.Equal("Passive", applied.Phase);
        Assert.Equal(20, applied.AppliedEffects[0].Power);
        Assert.Equal(1, applied.AppliedEffects[0].Duration);
    }

    [Fact]
    public void SharpTongue_RingsAddTurnsAndTargets()
    {
        var hero = MakeHero(rank: 1, passives: [HeroPassive.SharpTongue], items: ["RingOfLingeringWords", "RingOfWildfire"]);
        var (world, id, enemies) = Arena(hero);

        var context = BasicAttack(world, id);

        var burns = context.Logs.Where(l => l.EventType == CombatEventType.StatusApplied).ToList();
        Assert.Equal(enemies.Take(2), burns.Select(l => l.TargetId));
        Assert.All(burns, l => Assert.Equal(2, l.AppliedEffects[0].Duration));
        // (100 + 2 + 4) × (10% + 10%)
        Assert.All(burns, l => Assert.Equal(21, l.AppliedEffects[0].Power));
    }

    [Fact]
    public void DoubleStrike_HitsTwice_AsSeparateActions_WithOneEnergyGain()
    {
        var (world, id, enemies) = Arena(MakeHero(rank: 5, passives: [HeroPassive.DoubleStrike]));

        var context = BasicAttack(world, id);

        var attacks = context.Logs.Where(l => l.EventType == CombatEventType.Attack).ToList();
        Assert.Equal(2, attacks.Count);
        Assert.All(attacks, l => Assert.Equal(enemies[0], l.TargetId));
        Assert.Equal(100, attacks[1].Damage); // на 3 уровне пассивки второй удар в полную силу
        Assert.Equal(nameof(HeroPassive.DoubleStrike), attacks[1].SkillName);
        Assert.NotEqual(attacks[0].ActionId, attacks[1].ActionId);
        Assert.Equal(20, world.Energy[id].Current);
    }

    [Fact]
    public void DoubleStrike_RingOfCleaving_HitsNextEnemyToo()
    {
        var (world, id, enemies) = Arena(MakeHero(rank: 5, passives: [HeroPassive.DoubleStrike], items: "RingOfCleaving"));

        var context = BasicAttack(world, id);

        var attacks = context.Logs.Where(l => l.EventType == CombatEventType.Attack).ToList();
        Assert.Equal([enemies[0], enemies[0], enemies[1]], attacks.Select(l => l.TargetId));
    }

    [Fact]
    public void DoubleStrike_StopsWhenNoEnemiesLeft()
    {
        var (world, id, enemies) = Arena(MakeHero(rank: 5, passives: [HeroPassive.DoubleStrike]), enemies: 1);
        world.Health[enemies[0]].Current = 1;

        var context = BasicAttack(world, id);

        Assert.Single(context.Logs, l => l.EventType == CombatEventType.Attack);
    }

    [Fact]
    public void FullFight_WithAllEffects_Finishes()
    {
        var world = new World();
        Factory.CreateUnit(world, MakeHero(rank: 5, hp: 500, attack: 40, spells: [12], passives: [HeroPassive.SharpTongue],
            items: ["ArmageddonBlade", "EmberCrown", "RingOfWildfire", "RingOfLingeringWords", "StormAmulet"]), 1, 1);
        Factory.CreateUnit(world, MakeHero(rank: 5, hp: 500, attack: 40, passives: [HeroPassive.DoubleStrike], items: "RingOfCleaving"), 2, 1);
        Factory.CreateUnit(world, MakeHero(rank: 5, hp: 500, attack: 40, passives: [HeroPassive.SurvivalOfTheWeakest]), 2, 2);

        var result = new CombatEngine(new Random(1)).Run(world);

        Assert.True(result.CombatEnded);
    }

    // ===== Боевая мощь =====

    [Fact]
    public void Power_EquipmentIsItsOwnLayer()
    {
        var plain = MakeHero(hp: 100, attack: 10, spells: [FlameWave1], passives: [HeroPassive.SharpTongue]);
        var geared = MakeHero(hp: 100, attack: 10, spells: [FlameWave1], passives: [HeroPassive.SharpTongue], items: "ArmageddonBlade");

        var plainPower = CombatPower.OfHero(Factory, plain);
        var gearedPower = CombatPower.OfHero(Factory, geared);

        Assert.Equal(0, plainPower.Equipment);
        Assert.True(gearedPower.Equipment > 0);
        Assert.Equal(plainPower.Total, gearedPower.Total - gearedPower.Equipment);
        Assert.Equal(plainPower.Passives, gearedPower.Passives);
        Assert.Equal(gearedPower.Total, gearedPower.Stats + gearedPower.Level + gearedPower.Skills + gearedPower.Passives + gearedPower.Equipment);
    }

    [Fact]
    public void Power_RingCountsOnlyWithItsPassive()
    {
        var withPassive = CombatPower.OfHero(Factory, MakeHero(hp: 100, attack: 10, passives: [HeroPassive.SharpTongue], items: "RingOfWildfire"));
        var withoutPassive = CombatPower.OfHero(Factory, MakeHero(hp: 100, attack: 10, passives: [HeroPassive.DoubleStrike], items: "RingOfWildfire"));

        Assert.True(withPassive.Equipment > withoutPassive.Equipment);
    }

    // ===== Каталог и дроп =====

    [Fact]
    public void Catalog_ReferencesExistingSpells_AndKeysFitColumn()
    {
        var spellNames = SpellRegistry.All.Values.Select(s => s.Name).ToHashSet();

        foreach (var item in EquipmentCatalog.All)
        {
            Assert.True(item.Key.Length <= 32, item.Key);
            foreach (var info in item.Effects.Select(e => e.Info))
            {
                if (info.Spell != null) Assert.Contains(info.Spell, spellNames);
                if (info.ProcSpell != null) Assert.Contains(info.ProcSpell, spellNames);
            }
        }
    }

    [Fact]
    public void Catalog_HasEveryRarityAndType()
    {
        foreach (var rarity in Enum.GetValues<Rarity>())
            Assert.Contains(EquipmentCatalog.All, i => i.Rarity == rarity);
        foreach (var type in Enum.GetValues<EquipmentType>())
            Assert.Contains(EquipmentCatalog.All, i => i.Type == type);
    }

    [Fact]
    public void EquipmentChests_DropOnlyCatalogItems_WithCatalogRarity()
    {
        var tables = LootSeedData.TablesWithEntries().Where(t => t.ChestType == IdleDwarfsGame.Server.Enums.ChestType.Equipment).ToList();
        Assert.Equal(LootSeedData.MaxChestLevel, tables.Count);

        foreach (var table in tables)
        {
            Assert.Equal(1.0, table.Entries.Sum(e => e.Probability), precision: 6);
            foreach (var entry in table.Entries)
            {
                var item = EquipmentCatalog.Find(entry.SubType);
                Assert.NotNull(item);
                Assert.Equal(item!.Rarity, entry.Rarity);
            }
        }
    }

    // ===== Надевание =====

    private async Task<Hero> AddHeroAsync(HeroElement element = HeroElement.Fire)
    {
        var hero = MakeHero(element: element);
        hero.PlayerId = _playerId;
        _context.Heroes.Add(hero);
        await _context.SaveChangesAsync();
        return hero;
    }

    private async Task<PlayerEquipment> AddItemAsync(string key, Guid? playerId = null)
    {
        var item = new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = playerId ?? _playerId, ItemKey = key };
        _context.PlayerEquipment.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    private async Task<EquipmentError> EquipErrorAsync(Guid itemId, Guid heroId, EquipmentSlot? slot = null)
    {
        var error = await Assert.ThrowsAsync<EquipmentException>(() => _service.EquipAsync(_playerId, itemId, heroId, slot));
        return error.Code;
    }

    [Fact]
    public async Task Equip_RaisesHeroStats_AndUnequipRestoresThem()
    {
        var hero = await AddHeroAsync();
        var axe = await AddItemAsync("RustyAxe");

        var equipped = await _service.EquipAsync(_playerId, axe.Id, hero.Id, null);

        var info = Assert.Single(equipped.Heroes);
        Assert.Equal(102, info.Attack);
        Assert.Equal("RustyAxe", Assert.Single(info.Equipment).Key);
        var listed = Assert.Single(equipped.Items);
        Assert.Equal(hero.Id, listed.HeroId);
        Assert.Equal(EquipmentSlot.Weapon, listed.Slot);

        var removed = await _service.UnequipAsync(_playerId, axe.Id);

        Assert.Equal(100, Assert.Single(removed.Heroes).Attack);
        Assert.Null(Assert.Single(removed.Items).HeroId);
    }

    [Fact]
    public async Task Equip_RejectsWrongElement()
    {
        // Меч льда и тьмы огненному дворфу не подходит, ледяному и тёмному — подходит
        var sword = await AddItemAsync("BlackIceSword");

        Assert.Equal(EquipmentError.ElementRestricted, await EquipErrorAsync(sword.Id, (await AddHeroAsync(HeroElement.Fire)).Id));
        Assert.Equal(EquipmentError.ElementRestricted, await EquipErrorAsync(sword.Id, (await AddHeroAsync(HeroElement.Basic)).Id));

        await _service.EquipAsync(_playerId, sword.Id, (await AddHeroAsync(HeroElement.Frost)).Id, null);
        await _service.EquipAsync(_playerId, sword.Id, (await AddHeroAsync(HeroElement.Dark)).Id, null);
    }

    [Fact]
    public async Task Equip_RejectsWrongSlot()
    {
        var hero = await AddHeroAsync();
        var axe = await AddItemAsync("RustyAxe");

        Assert.Equal(EquipmentError.WrongSlot, await EquipErrorAsync(axe.Id, hero.Id, EquipmentSlot.Ring1));
    }

    [Fact]
    public async Task Equip_RejectsForeignHeroAndItem()
    {
        var hero = await AddHeroAsync();
        var axe = await AddItemAsync("RustyAxe");

        var strangerId = Guid.NewGuid();
        _context.Players.Add(new Player { Id = strangerId, Name = "Stranger" });
        await _context.SaveChangesAsync();
        var stranger = MakeHero();
        stranger.PlayerId = strangerId;
        _context.Heroes.Add(stranger);
        var foreignItem = await AddItemAsync("RustyAxe", strangerId);

        Assert.Equal(EquipmentError.HeroNotFound, await EquipErrorAsync(axe.Id, stranger.Id));
        Assert.Equal(EquipmentError.ItemNotFound, await EquipErrorAsync(foreignItem.Id, hero.Id));
    }

    [Fact]
    public async Task Rings_FillBothSlots_ThenReplaceFirst()
    {
        var hero = await AddHeroAsync();
        var first = await AddItemAsync("CopperRing");
        var second = await AddItemAsync("CopperRing");
        var third = await AddItemAsync("TwinBand");

        await _service.EquipAsync(_playerId, first.Id, hero.Id, null);
        await _service.EquipAsync(_playerId, second.Id, hero.Id, null);
        var result = await _service.EquipAsync(_playerId, third.Id, hero.Id, null);

        Assert.Equal(EquipmentSlot.Ring1, result.Items.Single(i => i.Id == third.Id).Slot);
        Assert.Equal(EquipmentSlot.Ring2, result.Items.Single(i => i.Id == second.Id).Slot);
        Assert.Null(result.Items.Single(i => i.Id == first.Id).HeroId);
        Assert.Equal(2, Assert.Single(result.Heroes).Equipment.Count);
    }

    [Fact]
    public async Task Equip_ReplacesItemInOccupiedSlot()
    {
        var hero = await AddHeroAsync();
        var axe = await AddItemAsync("RustyAxe");
        var hammer = await AddItemAsync("DwarvenHammer");

        await _service.EquipAsync(_playerId, axe.Id, hero.Id, null);
        var result = await _service.EquipAsync(_playerId, hammer.Id, hero.Id, null);

        Assert.Null(result.Items.Single(i => i.Id == axe.Id).HeroId);
        Assert.Equal("DwarvenHammer", Assert.Single(Assert.Single(result.Heroes).Equipment).Key);
    }

    [Fact]
    public async Task Equip_MovesItemBetweenHeroes_AndReturnsBoth()
    {
        var first = await AddHeroAsync();
        var second = await AddHeroAsync();
        var axe = await AddItemAsync("RustyAxe");

        await _service.EquipAsync(_playerId, axe.Id, first.Id, null);
        var result = await _service.EquipAsync(_playerId, axe.Id, second.Id, null);

        Assert.Equal(2, result.Heroes.Count);
        Assert.Equal(102, result.Heroes.Single(h => h.Id == second.Id).Attack);
        Assert.Equal(100, result.Heroes.Single(h => h.Id == first.Id).Attack);
    }

    [Fact]
    public async Task Ring_MovesBetweenItsOwnSlots()
    {
        var hero = await AddHeroAsync();
        var ring = await AddItemAsync("CopperRing");

        await _service.EquipAsync(_playerId, ring.Id, hero.Id, EquipmentSlot.Ring1);
        var result = await _service.EquipAsync(_playerId, ring.Id, hero.Id, EquipmentSlot.Ring2);

        Assert.Equal(EquipmentSlot.Ring2, Assert.Single(result.Items).Slot);
    }

    [Fact]
    public async Task EquippedItems_SurviveReloadAndRankedSnapshot()
    {
        var hero = await AddHeroAsync();
        hero.PlaceInSquad = 1;
        await _context.SaveChangesAsync();
        var blade = await AddItemAsync("ArmageddonBlade");
        await _service.EquipAsync(_playerId, blade.Id, hero.Id, null);

        _context.ChangeTracker.Clear();
        var squad = await new HeroesRepository(_context).GetSquadAsync(_playerId);
        Assert.Equal("ArmageddonBlade", Assert.Single(Assert.Single(squad).Equipment).ItemKey);

        var snapshot = new RankedSquad { HeroesJson = RankedSquad.Serialize(squad) }.Heroes();
        Assert.Equal("ArmageddonBlade", Assert.Single(Assert.Single(snapshot).Equipment).ItemKey);

        var world = new World();
        Factory.CreateSquad(world, snapshot, teamId: 1);
        Assert.Contains("Flame Wave", world.Gear.Values.Single().Spells.Keys);
    }

    [Fact]
    public void OldRankedSnapshot_WithoutNewFields_StillLoads()
    {
        var hero = Assert.Single(new RankedSquad { HeroesJson = """[{"Name":"Old","MaxHp":10}]""" }.Heroes());

        Assert.Empty(hero.Equipment);
        Assert.Empty(hero.Passives);
    }

    [Fact]
    public async Task OpenedEquipmentChest_PutsItemIntoInventory()
    {
        var table = _context.ChestDropTables.First(t => t.ChestType == IdleDwarfsGame.Server.Enums.ChestType.Equipment);
        var chest = new IdleDwarfsGame.Server.Entities.GameEntities.PlayerChest
        {
            Id = Guid.NewGuid(),
            PlayerId = _playerId,
            ChestDropTableId = table.Id
        };
        _context.PlayerChests.Add(chest);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var service = new ChestService(new InventoryRepository(_context), new ItemDropService(new Random(1)), Factory, new Random(1));
        var opened = await service.OpenAsync(_playerId, chest.Id);

        Assert.Empty(opened.Items);
        var dropped = Assert.Single(opened.Equipment);
        Assert.Equal(dropped.Id, Assert.Single(await _service.GetMyAsync(_playerId)).Id);
    }

    [Fact]
    public void NewHeroes_GetAPassive()
    {
        Assert.All(StarterSquad.Create(Guid.NewGuid(), new Random(1)), h => Assert.Single(h.Passives));
        Assert.Single(HeroGenerator.Create(Guid.NewGuid(), 3, string.Empty, new Random(1)).Passives);
    }
}
