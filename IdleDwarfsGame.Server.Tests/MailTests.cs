using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdleDwarfsGame.Server.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class MailTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _connection;
    private readonly GameDbContext _context;
    private readonly MailService _service;
    private readonly Guid _playerId = Guid.NewGuid();

    public MailTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _context = new GameDbContext(new DbContextOptionsBuilder<GameDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();

        _context.Players.Add(new Player { Id = _playerId, Name = "Tester", Gold = 100 });
        _context.Players.Add(new Player { Id = Guid.NewGuid(), Name = "Another" });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        _service = new MailService(_context, new EntityFactory(), new PlayerRepository(_context), new TestClock(Now), new Random(1));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Send_OrdersAttachments_HeroesItemsGemsAbilitiesCurrencies()
    {
        var mail = await _service.SendAsync(_playerId, null, "Gift", "Hello", AllKinds());

        Assert.Equal(MailRules.DefaultSender, mail.Sender);
        Assert.False(mail.IsRead);
        Assert.False(mail.IsClaimed);
        // Два дворфа одного вложения — две карточки
        Assert.Equal(
            [MailAttachmentKind.Hero, MailAttachmentKind.Hero, MailAttachmentKind.Equipment, MailAttachmentKind.Gem,
             MailAttachmentKind.Ability, MailAttachmentKind.Ability, MailAttachmentKind.Currency, MailAttachmentKind.Currency],
            mail.Attachments.Select(a => a.Kind));
        Assert.Equal([0, 1], mail.Attachments.Take(2).Select(a => a.Index));
        // Заклинание раньше пассивки
        Assert.Equal(AbilityKind.Spell, mail.Attachments[4].Ability!.Kind);
        Assert.Equal(AbilityKind.Passive, mail.Attachments[5].Ability!.Kind);
        Assert.Equal(3, mail.Attachments[0].Hero!.Level);
        Assert.Equal(HeroElement.Frost, mail.Attachments[0].Hero!.Element);
    }

    [Fact]
    public async Task Claim_GrantsEverything_Once()
    {
        var sent = await _service.SendAsync(_playerId, "GM", "Gift", "Hello", AllKinds());
        var previewHeroes = sent.Attachments.Where(a => a.Kind == MailAttachmentKind.Hero).Select(a => a.Hero!).ToList();

        var read = await _service.MarkReadAsync(_playerId, sent.Id);
        Assert.True(read.IsRead);
        Assert.False(read.IsClaimed);

        var claim = await _service.ClaimAsync(_playerId, sent.Id);
        Assert.True(claim.Mail.IsClaimed);
        Assert.Equal(100 + 5_000, claim.Resources!.Gold);
        Assert.Equal(7, claim.Resources.RainbowCoins);

        var heroes = await _context.Heroes.AsNoTracking().Where(h => h.PlayerId == _playerId).ToListAsync();
        Assert.Equal(2, heroes.Count);
        // Выданные дворфы — ровно те, что были в превью письма
        foreach (var preview in previewHeroes)
            Assert.Contains(heroes, h => h.Name == preview.Name && h.MaxHp == preview.MaxHp && h.Profession == preview.Profession);

        Assert.Equal(1, await _context.PlayerEquipment.CountAsync(e => e.PlayerId == _playerId && e.ItemKey == "IronHelm"));
        var gems = await _context.PlayerItems.AsNoTracking().SingleAsync(i => i.PlayerId == _playerId);
        Assert.Equal((nameof(GemType.Ruby), Rarity.Rare, 25), (gems.SubType, gems.Rarity, gems.Quantity));
        Assert.Equal(2, await _context.PlayerAbilities.CountAsync(a => a.PlayerId == _playerId));

        await Assert.ThrowsAsync<MailException>(() => _service.ClaimAsync(_playerId, sent.Id));
        Assert.Equal(100 + 5_000, (await _context.Players.AsNoTracking().SingleAsync(p => p.Id == _playerId)).Gold);
    }

    [Fact]
    public async Task OtherPlayer_CannotReadOrClaim()
    {
        var sent = await _service.SendAsync(_playerId, null, "Gift", "", AllKinds());
        var stranger = (await _context.Players.AsNoTracking().SingleAsync(p => p.Name == "Another")).Id;

        await Assert.ThrowsAsync<MailException>(() => _service.ClaimAsync(stranger, sent.Id));
        Assert.Empty(await _service.GetMailAsync(stranger));
        Assert.Single(await _service.GetMailAsync(_playerId));
    }

    [Fact]
    public async Task MailWithoutAttachments_CannotBeClaimed()
    {
        var sent = await _service.SendAsync(_playerId, null, "News", "Just text", []);
        await Assert.ThrowsAsync<MailException>(() => _service.ClaimAsync(_playerId, sent.Id));
    }

    [Theory]
    [InlineData(MailAttachmentKind.Hero, "Frost", 5, 1)]
    [InlineData(MailAttachmentKind.Hero, "Basic", 2, 1)]
    [InlineData(MailAttachmentKind.Equipment, "NoSuchItem", 1, 1)]
    [InlineData(MailAttachmentKind.Gem, "Ruby", 1, 0)]
    [InlineData(MailAttachmentKind.Currency, "Diamonds", 1, 10)]
    public async Task Send_RejectsInvalidAttachments(MailAttachmentKind kind, string key, int level, long amount)
    {
        var attachment = new MailAttachment { Kind = kind, Key = key, Level = level, Amount = amount, Rarity = Rarity.Common };
        await Assert.ThrowsAsync<MailException>(() => _service.SendAsync(_playerId, null, "Bad", "", [attachment]));
    }

    [Fact]
    public async Task Constructor_EmptyRankFourDwarf_HasNoAbilitiesAndNoImmunity()
    {
        var spec = new MailHeroSpec { CustomAbilities = true, Name = "Empty One", Profession = HeroProfession.Defender };
        var attachment = new MailAttachment { Kind = MailAttachmentKind.Hero, Key = "Fire", Level = 4, Amount = 3, HeroSpec = spec };
        var sent = await _service.SendAsync(_playerId, null, "Blank", "", [attachment]);
        await _service.ClaimAsync(_playerId, sent.Id);

        var heroes = await _context.Heroes.AsNoTracking().Where(h => h.PlayerId == _playerId).ToListAsync();
        Assert.Equal(3, heroes.Count);
        Assert.All(heroes, h =>
        {
            Assert.Equal(("Empty One", 4, HeroElement.Fire, HeroProfession.Defender), (h.Name, h.Level, h.Element, h.Profession));
            Assert.Empty(h.SpellIds);
            Assert.Empty(h.Passives);
            Assert.Null(h.AuraEffect);
            Assert.Empty(h.Immunities);
        });
    }

    [Fact]
    public void Constructor_ExactAbilities_AndImmunityOnlyWhenGiven()
    {
        int family = ElementSpellBook.FamiliesOf(HeroElement.Frost)[1];
        var passive = Enum.GetValues<HeroPassive>()[1];
        var spec = new MailHeroSpec
        {
            CustomAbilities = true, SpellFamily = family, SpellLevel = 3, Passive = passive, PassiveLevel = 2,
            Aura = "Regen", AuraLevel = 3, Immunities = ["Stun"]
        };
        var hero = MailHeroes.Create(_playerId, 4, "Frost", 42, spec);
        Assert.Equal([SpellRegistry.IdOf(family, 3)], hero.SpellIds);
        Assert.Equal([passive], hero.Passives);
        Assert.Equal(2, hero.PassiveLevel);
        Assert.Equal(("Regen", 3), (hero.AuraEffect, hero.AuraLevel));
        Assert.Equal(["Stun"], hero.Immunities);

        // Способности случайные, но иммунитет у дворфа из конструктора — только заданный: ни одного за 500 seed
        var randomAbilities = new MailHeroSpec();
        Assert.All(Enumerable.Range(0, 500), seed => Assert.Empty(MailHeroes.Create(_playerId, 4, "Dark", seed, randomAbilities).Immunities));
    }

    [Fact]
    public async Task PreviewSeed_GivesTheSameDwarfAsTheMail()
    {
        var spec = new MailHeroSpec { Profession = HeroProfession.Scout };
        var preview = _service.PreviewHero(3, "Nature", null, spec);
        var attachment = new MailAttachment { Kind = MailAttachmentKind.Hero, Key = "Nature", Level = 3, Seed = preview.Seed, HeroSpec = spec };
        var sent = await _service.SendAsync(_playerId, null, "Same", "", [attachment]);

        var inMail = sent.Attachments.Single().Hero!;
        Assert.Equal((preview.Hero.Name, preview.Hero.MaxHp, preview.Hero.Spells.Count), (inMail.Name, inMail.MaxHp, inMail.Spells.Count));
        Assert.Equal(HeroProfession.Scout.ToString(), inMail.Profession?.ToString());
    }

    [Theory]
    [InlineData("Fire", true)]   // заклинание льда огненному дворфу
    [InlineData("", true)]       // заклинание без выбранной стихии
    [InlineData("Frost", false)]
    public void Constructor_SpellMustMatchElement(string element, bool rejected)
    {
        var spec = new MailHeroSpec { CustomAbilities = true, SpellFamily = ElementSpellBook.FamiliesOf(HeroElement.Frost)[0] };
        if (rejected)
            Assert.Throws<MailException>(() => MailHeroes.Validate(spec, 3, element));
        else
            MailHeroes.Validate(spec, 3, element);
    }

    [Fact]
    public async Task Chests_GoToTheWarehouseClosed_BeforeCurrenciesInTheMail()
    {
        List<MailAttachment> attachments =
        [
            new() { Kind = MailAttachmentKind.Currency, Key = "Gold", Amount = 10 },
            new() { Kind = MailAttachmentKind.Chest, Key = "Infinity:Frost", Level = 2, Amount = 2 },
            new() { Kind = MailAttachmentKind.Chest, Key = "Hero", Level = 5 },
        ];
        var sent = await _service.SendAsync(_playerId, null, "Chests", "", attachments);
        Assert.Equal([MailAttachmentKind.Chest, MailAttachmentKind.Chest, MailAttachmentKind.Currency], sent.Attachments.Select(a => a.Kind));
        Assert.Equal((ChestType.Infinity, 2, HeroElement.Frost), (sent.Attachments[0].Chest!.Type, sent.Attachments[0].Chest!.Level, sent.Attachments[0].Chest!.Element));

        await _service.ClaimAsync(_playerId, sent.Id);

        var inventory = new InventoryRepository(_context);
        var chests = await inventory.GetClosedChestsAsync(_playerId);
        Assert.Equal(3, chests.Count);
        Assert.Equal(2, chests.Count(c => c.ChestDropTable.ChestType == ChestType.Infinity && c.ChestDropTable.Element == HeroElement.Frost));
        Assert.Single(chests, c => c.ChestDropTable.ChestType == ChestType.Hero && c.ChestDropTable.ChestLevel == 5);
    }

    [Theory]
    [InlineData("Infinity", 1)]        // у сундука бесконечности нужна стихия
    [InlineData("Infinity:Fire", 4)]   // таких уровней у бесконечности нет
    [InlineData("Hero:Fire", 1)]       // у сундука героев стихии нет
    [InlineData("Treasure", 1)]
    [InlineData("Gem", 9)]
    public async Task Chests_OnlyThoseWithADropTable(string key, int level)
    {
        var attachment = new MailAttachment { Kind = MailAttachmentKind.Chest, Key = key, Level = level };
        await Assert.ThrowsAsync<MailException>(() => _service.SendAsync(_playerId, null, "Bad", "", [attachment]));
    }

    [Fact]
    public async Task FindPlayers_ByPartOfName()
    {
        var found = await _service.FindPlayersAsync("est");
        Assert.Equal("Tester", Assert.Single(found).Name);
        Assert.Empty(await _service.FindPlayersAsync("  "));
    }

    [Fact]
    public void Catalog_ListsEverythingThatCanBeSent()
    {
        var catalog = _service.Catalog();
        // 1-й ранг без стихии + 3 ранга × 7 стихий
        Assert.Equal(1 + 3 * 7, catalog.Heroes.Count);
        Assert.Equal(EquipmentCatalog.All.Count, catalog.Equipment.Count);
        Assert.Equal(Enum.GetValues<GemType>().Length * MailRules.GemRarities.Length, catalog.Gems.Count);
        Assert.Contains(catalog.Abilities, a => a.Kind == AbilityKind.Spell && a.Level == AbilityService.MaxLevel);
        Assert.Contains(catalog.Abilities, a => a.Kind == AbilityKind.Passive);
        Assert.Contains(catalog.Abilities, a => a.Kind == AbilityKind.Aura);
        Assert.Contains(MailCurrency.EventCurrency, catalog.Currencies);
        Assert.Equal(LootSeedData.TablesWithEntries().Count, catalog.Chests.Count);
    }

    private static List<MailAttachment> AllKinds() =>
    [
        // Нарочно вперемешку: письмо само расставит по порядку
        new() { Kind = MailAttachmentKind.Currency, Key = "Gold", Amount = 5_000 },
        new() { Kind = MailAttachmentKind.Ability, AbilityKind = AbilityKind.Passive, Key = Enum.GetValues<HeroPassive>()[0].ToString(), Level = 2 },
        new() { Kind = MailAttachmentKind.Gem, Key = "Ruby", Rarity = Rarity.Rare, Amount = 25 },
        new() { Kind = MailAttachmentKind.Hero, Key = "Frost", Level = 3, Amount = 2 },
        new() { Kind = MailAttachmentKind.Ability, AbilityKind = AbilityKind.Spell, Key = ElementSpellBook.FamiliesOf(HeroElement.Fire)[0].ToString(), Level = 1 },
        new() { Kind = MailAttachmentKind.Equipment, Key = "IronHelm" },
        new() { Kind = MailAttachmentKind.Currency, Key = "RainbowCoins", Amount = 7 },
    ];

    private sealed class TestClock(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(now);
    }
}
