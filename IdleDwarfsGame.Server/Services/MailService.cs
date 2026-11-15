using IdleDwarfsGame.Server.Entities.GameEntities;
using IdleDwarfsGame.Server.Enums;
using Microsoft.EntityFrameworkCore;

// Почта игрока и отправка писем из админки.
// Письмо создаётся одной записью с вложениями (SendAsync). Открыл письмо — ReadAt (MarkReadAsync).
// Забрал вложения — ClaimedAt и выдача одной транзакцией (ClaimAsync): дворфы, предметы и способности — новыми строками,
// камни — в стопки, валюты — на аккаунт, валюта ивента — в кошелёк ивента текущей недели. Повторно забрать нельзя
public class MailService(GameDbContext db, IEntityFactory entityFactory, IPlayerRepository players, TimeProvider clock, Random random)
{
    // Сколько писем показывать в почте (новые сверху)
    public const int MailboxSize = 100;

    public async Task<List<MailInfo>> GetMailAsync(Guid playerId)
    {
        var mails = await db.PlayerMails
            .AsNoTracking()
            .Include(m => m.Attachments)
            .Where(m => m.PlayerId == playerId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(MailboxSize)
            .ToListAsync();
        return mails.Select(Describe).ToList();
    }

    public async Task<MailInfo> MarkReadAsync(Guid playerId, Guid mailId)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        await db.PlayerMails
            .Where(m => m.Id == mailId && m.PlayerId == playerId && m.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.ReadAt, now));
        return Describe(await LoadAsync(playerId, mailId));
    }

    public async Task<MailClaimResult> ClaimAsync(Guid playerId, Guid mailId)
    {
        var mail = await LoadAsync(playerId, mailId);
        if (mail.Attachments.Count == 0)
            throw new MailException("The mail has no attachments");
        if (mail.ClaimedAt != null)
            throw new MailException("Attachments are already claimed");

        var now = clock.GetUtcNow().UtcDateTime;
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            // Условный апдейт: из двух одновременных нажатий «Забрать» выдаст только первое
            int claimed = await db.PlayerMails
                .Where(m => m.Id == mailId && m.PlayerId == playerId && m.ClaimedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.ClaimedAt, now)
                    .SetProperty(m => m.ReadAt, m => m.ReadAt ?? now));
            if (claimed == 0)
                throw new MailException("Attachments are already claimed");

            await GrantAsync(playerId, mail.Attachments, now);
            await transaction.CommitAsync();
        }
        db.ChangeTracker.Clear();

        return new MailClaimResult
        {
            Mail = Describe(await LoadAsync(playerId, mailId)),
            Resources = await players.GetResourcesAsync(playerId)
        };
    }

    // ----- Админка -----

    public async Task<List<AdminPlayerInfo>> FindPlayersAsync(string query, int limit = 20)
    {
        query = query.Trim();
        if (query.Length == 0)
            return [];
        return await db.Players
            .AsNoTracking()
            .Where(p => p.Name.Contains(query))
            .OrderBy(p => p.Name.Length)
            .ThenBy(p => p.Name)
            .Take(limit)
            .Select(p => new AdminPlayerInfo { Id = p.Id, Name = p.Name, Rating = p.Rating, SquadPower = p.SquadPower })
            .ToListAsync();
    }

    public async Task<MailInfo> SendAsync(Guid playerId, string? sender, string title, string body, IReadOnlyList<MailAttachment> attachments)
    {
        if (!await db.Players.AnyAsync(p => p.Id == playerId))
            throw new MailException("Player not found");

        sender = string.IsNullOrWhiteSpace(sender) ? MailRules.DefaultSender : sender.Trim();
        title = title.Trim();
        body = body.Trim();
        if (title.Length == 0)
            throw new MailException("Title is required");
        if (sender.Length > MailRules.MaxSenderLength || title.Length > MailRules.MaxTitleLength || body.Length > MailRules.MaxBodyLength)
            throw new MailException("Sender, title or body is too long");
        if (attachments.Count > MailRules.MaxAttachments)
            throw new MailException($"No more than {MailRules.MaxAttachments} attachments");

        var mailId = Guid.NewGuid();
        var mail = new PlayerMail
        {
            Id = mailId,
            PlayerId = playerId,
            Sender = sender,
            Title = title,
            Body = body,
            CreatedAt = clock.GetUtcNow().UtcDateTime,
            Attachments = attachments
                .Select((a, index) => MailRules.Normalize(new MailAttachment
                {
                    Id = Guid.NewGuid(),
                    MailId = mailId,
                    Kind = a.Kind,
                    Key = a.Key?.Trim() ?? string.Empty,
                    Level = a.Level,
                    Rarity = a.Rarity,
                    AbilityKind = a.AbilityKind,
                    Amount = a.Amount,
                    // Seed из превью конструктора — игрок получит того же дворфа, что видел админ
                    Seed = a.Seed != 0 ? a.Seed : random.Next(),
                    Position = index,
                    HeroSpec = a.HeroSpec
                }))
                .ToList()
        };

        db.PlayerMails.Add(mail);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return Describe(mail);
    }

    // Письма игрока для админки — как их видит игрок
    public Task<List<MailInfo>> GetMailForAdminAsync(Guid playerId) => GetMailAsync(playerId);

    // Всё, что можно вложить, с превью
    public AdminMailCatalog Catalog()
    {
        var catalog = new AdminMailCatalog
        {
            Equipment = EquipmentCatalog.All
                .Select(item => EquipmentInfo.From(new PlayerEquipment { Id = Guid.NewGuid(), ItemKey = item.Key }))
                .OfType<EquipmentInfo>()
                .ToList(),
            Gems = Enum.GetValues<GemType>()
                .SelectMany(gem => MailRules.GemRarities.Select(rarity => new ItemInfo
                {
                    ItemType = ItemType.Gem, SubType = gem.ToString(), Rarity = rarity, Level = 1, Quantity = 1
                }))
                .ToList(),
            Chests = LootSeedData.TablesWithEntries()
                .OrderBy(t => t.ChestType).ThenBy(t => t.ChestLevel).ThenBy(t => t.Element)
                .Select(t => new ChestInfo { Id = t.Id, Type = t.ChestType, Level = t.ChestLevel, Element = t.Element })
                .ToList(),
            Currencies = Enum.GetValues<MailCurrency>().ToList(),
            Professions = ProfessionBook.SquadOrder.ToList(),
            Immunities = HeroGenerator.Immunities.ToList()
        };

        // Дворф каждого ранга и стихии; превью — с постоянным seed, чтобы каталог не прыгал между запросами
        for (int rank = HeroLevels.Min; rank <= HeroLevels.Available; rank++)
        {
            var elements = rank == HeroLevels.Min
                ? [string.Empty]
                : InfinityTower.ElementOrder.Select(e => e.ToString()).ToArray();
            foreach (var element in elements)
                catalog.Heroes.Add(HeroInfo.From(entityFactory, HeroGenerator.Create(Guid.Empty, rank, element, new Random(rank * 31 + element.Length))));
        }

        // Способности всех уровней: заклинания стихий, пассивки, ауры
        var levels = Enumerable.Range(1, AbilityService.MaxLevel).ToArray();
        var spells = ElementSpellBook.Elements
            .SelectMany(ElementSpellBook.FamiliesOf)
            .SelectMany(family => levels.Select(level => AbilityInfo.Describe(AbilityKind.Spell, family.ToString(), level)));
        var passives = Enum.GetValues<HeroPassive>()
            .SelectMany(passive => levels.Select(level => AbilityInfo.Describe(AbilityKind.Passive, passive.ToString(), level)));
        var auras = AuraBook.Effects
            .SelectMany(effect => levels.Select(level => AbilityInfo.Describe(AbilityKind.Aura, effect, level)));
        catalog.Abilities = spells.Concat(passives).Concat(auras)
            .OfType<AbilityInfo>()
            .Where(a => a.Level >= 1)
            .ToList();

        return catalog;
    }

    // Дворф из конструктора с проверкой — как его увидит и получит игрок. Без seed — новый случайный
    public AdminHeroPreview PreviewHero(int level, string element, int? seed, MailHeroSpec? spec)
    {
        var attachment = MailRules.Normalize(new MailAttachment
        {
            Kind = MailAttachmentKind.Hero, Key = element.Trim(), Level = level, Amount = 1, HeroSpec = spec
        });
        int actualSeed = seed ?? random.Next();
        var hero = MailHeroes.Create(Guid.Empty, attachment.Level, attachment.Key, actualSeed, attachment.HeroSpec);
        return new AdminHeroPreview { Seed = actualSeed, Hero = HeroInfo.From(entityFactory, hero) };
    }

    // ----- Внутреннее -----

    private async Task<PlayerMail> LoadAsync(Guid playerId, Guid mailId) =>
        await db.PlayerMails
            .AsNoTracking()
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == mailId && m.PlayerId == playerId)
        ?? throw new MailException("Mail not found");

    private async Task GrantAsync(Guid playerId, IReadOnlyList<MailAttachment> attachments, DateTime now)
    {
        foreach (var attachment in attachments)
        {
            switch (attachment.Kind)
            {
                case MailAttachmentKind.Hero:
                    for (int i = 0; i < attachment.Amount; i++)
                        db.Heroes.Add(CreateHero(playerId, attachment, i));
                    break;
                case MailAttachmentKind.Equipment:
                    for (int i = 0; i < attachment.Amount; i++)
                        db.PlayerEquipment.Add(new PlayerEquipment { Id = Guid.NewGuid(), PlayerId = playerId, ItemKey = attachment.Key });
                    break;
                case MailAttachmentKind.Chest:
                    MailRules.TryParseChest(attachment.Key, out var chestType, out var chestElement);
                    var tableId = LootSeedData.TableId(chestType, attachment.Level, chestElement);
                    for (int i = 0; i < attachment.Amount; i++)
                        db.PlayerChests.Add(new PlayerChest { Id = Guid.NewGuid(), PlayerId = playerId, ChestDropTableId = tableId, IsOpened = false });
                    break;
                case MailAttachmentKind.Ability:
                    for (int i = 0; i < attachment.Amount; i++)
                        db.PlayerAbilities.Add(new PlayerAbility
                        {
                            Id = Guid.NewGuid(), PlayerId = playerId, Kind = attachment.AbilityKind!.Value, Key = attachment.Key, Level = attachment.Level
                        });
                    break;
            }
        }

        // Одинаковые камни из разных вложений — одной стопкой: две новые строки одной стопки в одной транзакции не сохранить
        var gems = attachments
            .Where(a => a.Kind == MailAttachmentKind.Gem)
            .GroupBy(a => (a.Key, a.Rarity))
            .Select(g => new LootDrop(ItemType.Gem, g.Key.Key, g.Key.Rarity, 1, (int)Math.Min(int.MaxValue, g.Sum(a => a.Amount))));
        foreach (var gem in gems)
            await PlayerItemStacks.AddAsync(db, playerId, gem);

        await db.SaveChangesAsync();

        long Sum(MailCurrency currency) => attachments
            .Where(a => a.Kind == MailAttachmentKind.Currency && a.Key == currency.ToString())
            .Sum(a => a.Amount);

        long gold = Sum(MailCurrency.Gold), experience = Sum(MailCurrency.Experience), rainbow = Sum(MailCurrency.RainbowCoins),
            blood = Sum(MailCurrency.BloodCoins), honor = Sum(MailCurrency.HonorBadges), seals = Sum(MailCurrency.ElderSeals);
        if (gold > 0 || experience > 0 || rainbow > 0 || blood > 0 || honor > 0 || seals > 0)
        {
            await db.Players
                .Where(p => p.Id == playerId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Gold, p => p.Gold + gold)
                    .SetProperty(p => p.Experience, p => p.Experience + experience)
                    .SetProperty(p => p.RainbowCoins, p => p.RainbowCoins + rainbow)
                    .SetProperty(p => p.BloodCoins, p => p.BloodCoins + blood)
                    .SetProperty(p => p.HonorBadges, p => p.HonorBadges + honor)
                    .SetProperty(p => p.ElderSeals, p => p.ElderSeals + seals));
        }

        // Валюта ивента — в кошелёк ивента этой недели (сгорит вместе с ним, как и заработанная)
        await EventWallets.AddAsync(db, playerId, WorldEvents.Of(now).StartsAt, Sum(MailCurrency.EventCurrency));
    }

    // Дворф вложения: seed + номер — превью письма показывает ровно первого из них
    private static Hero CreateHero(Guid playerId, MailAttachment attachment, int index) =>
        MailHeroes.Create(playerId, attachment.Level, attachment.Key, unchecked(attachment.Seed + index), attachment.HeroSpec);

    private MailInfo Describe(PlayerMail mail) => new()
    {
        Id = mail.Id,
        Sender = mail.Sender,
        Title = mail.Title,
        Body = mail.Body,
        CreatedAt = mail.CreatedAt,
        IsRead = mail.ReadAt != null,
        IsClaimed = mail.ClaimedAt != null,
        Attachments = MailRules.Ordered(mail.Attachments).SelectMany(a => DescribeAttachment(mail.PlayerId, a)).ToList()
    };

    // Вложение с несколькими дворфами — карточка на каждого: игрок заранее видит, кого получит
    private IEnumerable<MailAttachmentInfo> DescribeAttachment(Guid playerId, MailAttachment attachment)
    {
        if (attachment.Kind == MailAttachmentKind.Hero)
        {
            return Enumerable.Range(0, (int)attachment.Amount).Select(index => new MailAttachmentInfo
            {
                Id = attachment.Id,
                Index = index,
                Kind = attachment.Kind,
                Amount = 1,
                Hero = HeroInfo.From(entityFactory, CreateHero(playerId, attachment, index))
            });
        }

        var info = new MailAttachmentInfo { Id = attachment.Id, Kind = attachment.Kind, Amount = attachment.Amount };
        switch (attachment.Kind)
        {
            case MailAttachmentKind.Equipment:
                info.Equipment = EquipmentInfo.From(new PlayerEquipment { Id = attachment.Id, PlayerId = playerId, ItemKey = attachment.Key });
                break;
            case MailAttachmentKind.Gem:
                info.Gem = new ItemInfo
                {
                    ItemType = ItemType.Gem, SubType = attachment.Key, Rarity = attachment.Rarity, Level = 1,
                    Quantity = (int)Math.Min(int.MaxValue, attachment.Amount)
                };
                break;
            case MailAttachmentKind.Ability:
                info.Ability = AbilityInfo.Describe(attachment.AbilityKind ?? AbilityKind.Spell, attachment.Key, attachment.Level, attachment.Id);
                break;
            case MailAttachmentKind.Chest:
                if (MailRules.TryParseChest(attachment.Key, out var chestType, out var chestElement))
                    info.Chest = new ChestInfo { Id = attachment.Id, Type = chestType, Level = attachment.Level, Element = chestElement };
                break;
            case MailAttachmentKind.Currency:
                info.Currency = Enum.TryParse<MailCurrency>(attachment.Key, out var currency) ? currency : null;
                break;
        }
        return [info];
    }
}
