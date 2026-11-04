using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Способности дворфов по рангу, извлечение, вставка и слияние способностей, слияние дворфов
public class AbilityTests : IDisposable
{
    private static readonly EntityFactory Factory = new();

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly AbilityService _service;
    private readonly Guid _playerId = Guid.NewGuid();

    public AbilityTests()
    {
        // In-memory SQLite живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Tester" });
        _context.Heroes.AddRange(StarterSquad.Create(_playerId, new Random(1)));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        _service = new AbilityService(
            new AbilityRepository(_context), new HeroesRepository(_context), new ExpeditionRepository(_context), Factory,
            new PlayerRepository(_context), new Random(3));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static async Task<AbilityError> ErrorOf(Func<Task> action) =>
        (await Assert.ThrowsAsync<AbilityException>(action)).Code;

    private async Task<Hero> AddHero(int rank, HeroElement element = HeroElement.Fire, int seed = 1, int? place = null)
    {
        var hero = HeroGenerator.Create(_playerId, rank, element.ToString(), new Random(seed));
        hero.PlaceInSquad = place;
        _context.Heroes.Add(hero);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return hero;
    }

    private async Task<PlayerAbility> AddAbility(AbilityKind kind, string key, int level)
    {
        var ability = new PlayerAbility { Id = Guid.NewGuid(), PlayerId = _playerId, Kind = kind, Key = key, Level = level };
        _context.PlayerAbilities.Add(ability);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return ability;
    }

    private Task<Hero?> Stored(Guid heroId) => _context.Heroes.AsNoTracking().FirstOrDefaultAsync(h => h.Id == heroId);

    /* ===== Способности по рангу ===== */

    [Fact]
    public void RankOne_HasNothing()
    {
        var hero = HeroGenerator.Create(_playerId, 1, "Fire", new Random(1));

        Assert.Empty(hero.SpellIds);
        Assert.Empty(hero.Passives);
        Assert.Null(hero.AuraEffect);
        Assert.Empty(hero.Immunities);
        Assert.All(StarterSquad.Create(_playerId), h => Assert.Empty(AbilityInfo.Of(h)));
    }

    [Theory]
    [InlineData(2, 1, 0)]
    [InlineData(3, 2, 2)]
    [InlineData(4, 3, 3)]
    public void Rank_GivesSpellAndPassiveOfItsLevels(int rank, int spellLevel, int maxPassiveLevel)
    {
        var random = new Random(7);
        var heroes = Enumerable.Range(0, 300).Select(_ => HeroGenerator.Create(_playerId, rank, string.Empty, random)).ToList();

        Assert.All(heroes, h => Assert.Equal(spellLevel, SpellRegistry.All[Assert.Single(h.SpellIds)].Level));
        if (maxPassiveLevel == 0)
        {
            Assert.All(heroes, h => Assert.Empty(h.Passives));
            return;
        }
        Assert.All(heroes, h => Assert.Single(h.Passives));
        // Уровень пассивки случайный — выпадают все от 1 до максимума ранга
        Assert.Equal(Enumerable.Range(1, maxPassiveLevel), heroes.Select(h => h.PassiveLevel).Distinct().Order());
    }

    [Fact]
    public void AuraAndImmunity_ComeOnRankFour()
    {
        var random = new Random(9);
        Assert.All(Enumerable.Range(0, 200).Select(_ => HeroGenerator.Create(_playerId, 3, string.Empty, random)), h =>
        {
            Assert.Null(h.AuraEffect);
            Assert.Empty(h.Immunities);
        });

        var heroes = Enumerable.Range(0, 1000).Select(_ => HeroGenerator.Create(_playerId, 4, string.Empty, random)).ToList();
        Assert.All(heroes, h =>
        {
            Assert.Equal(AuraBook.EffectFor(h.Element), h.AuraEffect);
            Assert.Equal(AuraBook.MinLevel, h.AuraLevel);
            // Несколько иммунитетов у дворфа не бывает
            Assert.True(h.Immunities.Count <= 1);
        });

        // Иммунитет редкий: примерно у каждого десятого
        int immune = heroes.Count(h => h.Immunities.Count == 1);
        Assert.InRange(immune, 50, 150);
    }

    /* ===== Извлечение ===== */

    [Fact]
    public async Task Extract_DissolvesDwarf_AndKeepsChosenAbility()
    {
        var hero = await AddHero(4, HeroElement.Frost);
        var spell = SpellRegistry.All[hero.SpellIds[0]];
        string family = ElementSpellBook.FamilyOf(spell.Id)!.Value.ToString();

        var result = await _service.ExtractAsync(_playerId, hero.Id, AbilityKind.Spell, family);

        var ability = Assert.Single(result.Abilities);
        Assert.Equal(AbilityKind.Spell, ability.Kind);
        Assert.Equal(3, ability.Level);
        Assert.Equal(spell.Id, ability.SpellId);
        Assert.Equal(HeroElement.Frost, ability.Element);
        Assert.Null(await Stored(hero.Id));
    }

    [Fact]
    public async Task Extract_ReturnsDwarfItemsToInventory()
    {
        var hero = await AddHero(4);
        var item = new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = _playerId, ItemKey = "Test", HeroId = hero.Id, Slot = EquipmentSlot.Weapon };
        _context.PlayerEquipment.Add(item);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await _service.ExtractAsync(_playerId, hero.Id, AbilityKind.Aura, hero.AuraEffect!);

        var stored = await _context.PlayerEquipment.AsNoTracking().SingleAsync(e => e.Id == item.Id);
        Assert.Null(stored.HeroId);
        Assert.Null(stored.Slot);
    }

    [Fact]
    public async Task Extract_RejectsMissingAbility_BusyDwarf_AndLastDwarf()
    {
        var hero = await AddHero(2);
        // Пассивки у дворфа 2-го ранга нет, иммунитет не извлекается вовсе
        Assert.Equal(AbilityError.NothingToExtract, await ErrorOf(() => _service.ExtractAsync(_playerId, hero.Id, AbilityKind.Passive, "DoubleStrike")));
        Assert.Equal(AbilityError.HeroNotFound, await ErrorOf(() => _service.ExtractAsync(Guid.NewGuid(), hero.Id, AbilityKind.Spell, "1")));

        _context.Mines.Add(new Mine { Id = Guid.NewGuid(), PlayerId = _playerId, Plot = 1, Type = MineType.Gold, HeroId = hero.Id, UpdatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        string family = ElementSpellBook.FamilyOf(hero.SpellIds[0])!.Value.ToString();
        Assert.Equal(AbilityError.HeroBusy, await ErrorOf(() => _service.ExtractAsync(_playerId, hero.Id, AbilityKind.Spell, family)));

        // Остался один дворф — его не распылить, иначе игрок остался бы без отряда
        await _context.Mines.ExecuteDeleteAsync();
        await _context.Heroes.Where(h => h.PlayerId == _playerId && h.Id != hero.Id).ExecuteDeleteAsync();
        Assert.Equal(AbilityError.LastHero, await ErrorOf(() => _service.ExtractAsync(_playerId, hero.Id, AbilityKind.Spell, family)));
        Assert.NotNull(await Stored(hero.Id));
    }

    /* ===== Вставка ===== */

    [Fact]
    public async Task Insert_ReplacesAbilityOfSameKind_AndUsesItUp()
    {
        var hero = await AddHero(3, HeroElement.Fire);
        var spell = await AddAbility(AbilityKind.Spell, "13", 3);          // Meteor 3
        var passive = await AddAbility(AbilityKind.Passive, "DoubleStrike", 3);
        var aura = await AddAbility(AbilityKind.Aura, "Regen", 2);

        await _service.InsertAsync(_playerId, spell.Id, hero.Id);
        await _service.InsertAsync(_playerId, passive.Id, hero.Id);
        var result = await _service.InsertAsync(_playerId, aura.Id, hero.Id);

        Assert.Empty(result.Abilities);
        var stored = (await Stored(hero.Id))!;
        Assert.Equal(new[] { 15 }, stored.SpellIds);
        Assert.Equal(new[] { HeroPassive.DoubleStrike }, stored.Passives);
        Assert.Equal(3, stored.PassiveLevel);
        Assert.Equal("Regen", stored.AuraEffect);
        Assert.Equal(AuraTarget.Allies, stored.AuraTarget);
        Assert.Equal(5, stored.AuraPower);
        Assert.Equal(2, stored.AuraLevel);

        // В ответе герой уже с новыми способностями
        Assert.Equal(3, result.Hero!.Passives.Single().Level);
        Assert.Contains(result.Hero.Abilities, a => a.Kind == AbilityKind.Aura && a.Level == 2);

        Assert.Equal(AbilityError.AbilityNotFound, await ErrorOf(() => _service.InsertAsync(_playerId, spell.Id, hero.Id)));
    }

    // Заклинание — только дворфу своей стихии; пассивки и ауры вставляются любому
    [Fact]
    public async Task Insert_SpellOfAnotherElement_IsRejected()
    {
        var fire = await AddHero(3, HeroElement.Fire);
        var basic = await AddHero(1, seed: 2);
        var flameWave = await AddAbility(AbilityKind.Spell, "10", 2);

        var frost = await AddHero(3, HeroElement.Frost, seed: 3);
        Assert.Equal(AbilityError.ElementMismatch, await ErrorOf(() => _service.InsertAsync(_playerId, flameWave.Id, frost.Id)));
        Assert.Equal(AbilityError.ElementMismatch, await ErrorOf(() => _service.InsertAsync(_playerId, flameWave.Id, basic.Id)));

        // Отказ не тратит способность и не меняет дворфа
        Assert.Single(await _service.GetMyAsync(_playerId));
        Assert.Equal(frost.SpellIds, (await Stored(frost.Id))!.SpellIds);

        await _service.InsertAsync(_playerId, flameWave.Id, fire.Id);
        Assert.Equal(new[] { 11 }, (await Stored(fire.Id))!.SpellIds);

        var aura = await AddAbility(AbilityKind.Aura, "Burn", 1);
        await _service.InsertAsync(_playerId, aura.Id, frost.Id);
        Assert.Equal("Burn", (await Stored(frost.Id))!.AuraEffect);
    }

    // Извлечённая и вставленная обратно способность — та же: уровень не теряется
    [Fact]
    public async Task ExtractThenInsert_KeepsLevel()
    {
        var donor = await AddHero(4, HeroElement.Nature, seed: 2);
        var target = await AddHero(2, HeroElement.Fire, seed: 3);

        var extracted = await _service.ExtractAsync(_playerId, donor.Id, AbilityKind.Passive, donor.Passives[0].ToString());
        var ability = Assert.Single(extracted.Abilities);
        Assert.Equal(donor.PassiveLevel, ability.Level);

        var result = await _service.InsertAsync(_playerId, ability.Id!.Value, target.Id);

        var passive = Assert.Single(result.Hero!.Passives);
        Assert.Equal(donor.Passives[0], passive.Passive);
        Assert.Equal(donor.PassiveLevel, passive.Level);
    }

    /* ===== Слияние способностей ===== */

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(1, 2, 3)]
    [InlineData(2, 1, 3)]
    public async Task Combine_SumsLevels(int first, int second, int expected)
    {
        var a = await AddAbility(AbilityKind.Passive, "SharpTongue", first);
        var b = await AddAbility(AbilityKind.Passive, "SharpTongue", second);

        var result = await _service.CombineAsync(_playerId, a.Id, b.Id);

        var combined = Assert.Single(result.Abilities);
        Assert.Equal(expected, combined.Level);
        Assert.Equal(PassiveBook.Values(HeroPassive.SharpTongue, expected).Percent, combined.Percent);
    }

    [Fact]
    public async Task Combine_RejectsDifferentAbilities_AndLevelsAboveMax()
    {
        var fire = await AddAbility(AbilityKind.Spell, "1", 1);
        var frost = await AddAbility(AbilityKind.Spell, "16", 1);
        var two = await AddAbility(AbilityKind.Spell, "1", 2);
        var otherTwo = await AddAbility(AbilityKind.Spell, "1", 2);

        Assert.Equal(AbilityError.CannotCombine, await ErrorOf(() => _service.CombineAsync(_playerId, fire.Id, frost.Id)));
        Assert.Equal(AbilityError.CannotCombine, await ErrorOf(() => _service.CombineAsync(_playerId, fire.Id, fire.Id)));
        Assert.Equal(AbilityError.MaxLevel, await ErrorOf(() => _service.CombineAsync(_playerId, two.Id, otherTwo.Id)));
        Assert.Equal(4, (await _service.GetMyAsync(_playerId)).Count);
    }

    /* ===== Слияние дворфов ===== */

    [Fact]
    public async Task Fuse_FiveRankOneDwarfs_GiveRankTwoOfAnyElement()
    {
        var starters = await _context.Heroes.AsNoTracking().Where(h => h.PlayerId == _playerId).ToListAsync();

        var result = await _service.FuseAsync(_playerId, starters.Select(h => h.Id).ToList());

        var hero = result.Hero!;
        Assert.Equal(2, hero.Level);
        Assert.NotEqual(HeroElement.Basic, hero.Element);
        Assert.Single(hero.Spells);
        // Встаёт на первое освободившееся место в отряде
        Assert.Equal(1, hero.PlaceInSquad);
        Assert.Equal(1, await _context.Heroes.CountAsync(h => h.PlayerId == _playerId));
    }

    [Fact]
    public async Task Fuse_RefundsPartOfTheUpgradeInvestment()
    {
        var starters = await _context.Heroes.Where(h => h.PlayerId == _playerId).ToListAsync();
        starters[0].UpgradeLevel = 10;
        starters[1].UpgradeLevel = 5;
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var invested = HeroUpgradeService.Invested(1, 10).Gold + HeroUpgradeService.Invested(1, 5).Gold;
        var result = await _service.FuseAsync(_playerId, starters.Select(h => h.Id).ToList());

        int expected = (int)Math.Round(invested * HeroUpgradeService.FusionRefund, MidpointRounding.AwayFromZero);
        Assert.Equal(expected, result.Refund!.Gold);
        Assert.True(result.Refund.Experience > 0);
        Assert.Equal(expected, result.Resources!.Gold);
        Assert.Equal(expected, (await _context.Players.AsNoTracking().SingleAsync(p => p.Id == _playerId)).Gold);
    }

    [Fact]
    public async Task Fuse_RankOneElements_AreEquallyLikely()
    {
        var counts = new Dictionary<HeroElement, int>();
        for (int i = 0; i < 280; i++)
        {
            var ids = new List<Guid>();
            for (int n = 0; n < AbilityService.FusionSize; n++)
                ids.Add((await AddHero(1, seed: i * 10 + n)).Id);

            var element = (await _service.FuseAsync(_playerId, ids)).Hero!.Element;
            counts[element] = counts.GetValueOrDefault(element) + 1;
        }

        // 7 стихий по 40 в среднем; редкие из сундуков (тьма, кровь, сияние) тут не реже остальных
        Assert.Equal(7, counts.Count);
        Assert.All(counts.Values, c => Assert.InRange(c, 18, 65));
    }

    [Fact]
    public async Task Fuse_ElementFollowsShareOfDwarfs()
    {
        int fire = 0;
        const int runs = 200;
        for (int i = 0; i < runs; i++)
        {
            var ids = new List<Guid>();
            for (int n = 0; n < 4; n++)
                ids.Add((await AddHero(2, HeroElement.Fire, seed: i * 10 + n)).Id);
            ids.Add((await AddHero(2, HeroElement.Frost, seed: i * 10 + 9)).Id);

            var hero = (await _service.FuseAsync(_playerId, ids)).Hero!;
            Assert.Equal(3, hero.Level);
            Assert.Contains(hero.Element, new[] { HeroElement.Fire, HeroElement.Frost });
            if (hero.Element == HeroElement.Fire) fire++;
        }

        // 4 огненных и 1 ледяной — 80% огонь
        Assert.InRange(fire, 140, 185);

        var sameElement = new List<Guid>();
        for (int n = 0; n < 5; n++)
            sameElement.Add((await AddHero(3, HeroElement.Dark, seed: n)).Id);
        Assert.Equal(HeroElement.Dark, (await _service.FuseAsync(_playerId, sameElement)).Hero!.Element);
    }

    [Fact]
    public async Task Fuse_RejectsWrongCount_MixedRanks_MaxRank_AndBusyDwarfs()
    {
        var rankTwo = new List<Guid>();
        for (int n = 0; n < 5; n++)
            rankTwo.Add((await AddHero(2, seed: n)).Id);

        Assert.Equal(AbilityError.WrongHeroCount, await ErrorOf(() => _service.FuseAsync(_playerId, rankTwo.Take(4).ToList())));
        Assert.Equal(AbilityError.WrongHeroCount, await ErrorOf(() => _service.FuseAsync(_playerId, [.. rankTwo.Take(4), rankTwo[0]])));

        var mixed = rankTwo.Take(4).Append((await AddHero(3)).Id).ToList();
        Assert.Equal(AbilityError.DifferentRanks, await ErrorOf(() => _service.FuseAsync(_playerId, mixed)));

        var rankFour = new List<Guid>();
        for (int n = 0; n < 5; n++)
            rankFour.Add((await AddHero(HeroLevels.Available, seed: n)).Id);
        Assert.Equal(AbilityError.MaxRank, await ErrorOf(() => _service.FuseAsync(_playerId, rankFour)));

        _context.Mines.Add(new Mine { Id = Guid.NewGuid(), PlayerId = _playerId, Plot = 1, Type = MineType.Gold, HeroId = rankTwo[2], UpdatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        Assert.Equal(AbilityError.HeroBusy, await ErrorOf(() => _service.FuseAsync(_playerId, rankTwo)));

        // Ни один отказ никого не распылил
        Assert.All(rankTwo, id => Assert.True(_context.Heroes.Any(h => h.Id == id)));
    }
}
