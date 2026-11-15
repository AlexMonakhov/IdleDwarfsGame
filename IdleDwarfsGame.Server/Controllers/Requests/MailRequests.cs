using IdleDwarfsGame.Server.Enums;

// Письмо из админки. Что значат поля вложения — см. MailAttachment
public class SendMailRequest
{
    public Guid PlayerId { get; set; }
    public string? Sender { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public List<MailAttachmentRequest>? Attachments { get; set; }
}

public class MailAttachmentRequest
{
    public MailAttachmentKind Kind { get; set; }
    public string? Key { get; set; }
    public int Level { get; set; } = 1;
    public Rarity Rarity { get; set; }
    public AbilityKind? AbilityKind { get; set; }
    public long Amount { get; set; } = 1;
    // Дворф: seed из превью админки (null — случайный) и конструктор (null — как из сундука)
    public int? Seed { get; set; }
    public MailHeroSpec? HeroSpec { get; set; }
}

// Превью дворфа для конструктора админки
public class HeroPreviewRequest
{
    public int Level { get; set; } = 1;
    public string? Element { get; set; }
    public int? Seed { get; set; }
    public MailHeroSpec? HeroSpec { get; set; }
}
