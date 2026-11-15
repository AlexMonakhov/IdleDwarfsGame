using IdleDwarfsGame.Server.Enums;

// Письмо в почте игрока. Вложения уже в порядке письма (MailRules.Ordered)
public class MailInfo
{
    public Guid Id { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    // Вложения забраны (у письма без вложений — false)
    public bool IsClaimed { get; set; }
    public List<MailAttachmentInfo> Attachments { get; set; } = new();
}

// Вложение с превью для карточки: заполнено одно из Hero / Equipment / Gem / Ability / Currency
public class MailAttachmentInfo
{
    public Guid Id { get; set; }
    // Вложение с несколькими дворфами показывается карточкой на каждого: Index — номер дворфа (у остальных видов 0)
    public int Index { get; set; }
    public MailAttachmentKind Kind { get; set; }
    public long Amount { get; set; }

    // Дворф — ровно тот, что будет выдан
    public HeroInfo? Hero { get; set; }
    public EquipmentInfo? Equipment { get; set; }
    public ItemInfo? Gem { get; set; }
    public AbilityInfo? Ability { get; set; }
    public ChestInfo? Chest { get; set; }
    public MailCurrency? Currency { get; set; }
}

public class MailClaimResult
{
    public MailInfo Mail { get; set; } = new();
    // Ресурсы аккаунта уже с вложениями
    public PlayerResources? Resources { get; set; }
}

// ----- Админка -----

public class AdminPlayerInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int SquadPower { get; set; }
}

// Всё, что можно вложить в письмо, с превью для карточек
public class AdminMailCatalog
{
    // По дворфу на ранг и стихию (на 1-м ранге стихии нет). Key вложения — стихия, Level — ранг
    public List<HeroInfo> Heroes { get; set; } = new();
    public List<EquipmentInfo> Equipment { get; set; } = new();
    public List<ItemInfo> Gems { get; set; } = new();
    public List<AbilityInfo> Abilities { get; set; } = new();
    // Все сундуки, у которых есть таблица дропа: тип, уровень, стихия (у сундуков бесконечности)
    public List<ChestInfo> Chests { get; set; } = new();
    public List<MailCurrency> Currencies { get; set; } = new();
    // Для конструктора дворфа
    public List<HeroProfession> Professions { get; set; } = new();
    public List<string> Immunities { get; set; } = new();
}

// Превью дворфа из конструктора: Seed вложить в письмо — игрок получит ровно этого дворфа
public class AdminHeroPreview
{
    public int Seed { get; set; }
    public HeroInfo Hero { get; set; } = null!;
}
