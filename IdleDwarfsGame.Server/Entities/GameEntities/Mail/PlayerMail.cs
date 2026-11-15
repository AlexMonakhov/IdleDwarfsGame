using IdleDwarfsGame.Server.Enums;

// Письмо игроку (пока пишет только админ — AdminController). Вложения выдаются один раз (MailService.ClaimAsync):
// ReadAt — игрок открыл письмо, ClaimedAt — забрал вложения; после этого письмо остаётся, а вложения показываются серыми
public class PlayerMail
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }

    public string Sender { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<MailAttachment> Attachments { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    // null — вложения ещё не забраны (у письма без вложений так и остаётся null)
    public DateTime? ClaimedAt { get; set; }
}

// Вид вложения; порядок в письме всегда такой: дворфы, предметы, камни, способности, сундуки, валюты (MailRules.KindOrder).
// В БД хранится строкой — значения можно дописывать
public enum MailAttachmentKind { Hero, Equipment, Gem, Ability, Currency, Chest }

// Валюты аккаунта, которые можно вложить в письмо. EventCurrency — в кошелёк ивента текущей недели
public enum MailCurrency { Gold, Experience, RainbowCoins, BloodCoins, HonorBadges, ElderSeals, EventCurrency }

// Одно вложение письма. Что значат поля, зависит от вида:
//   Hero      — Key: стихия (пусто — случайная), Level: ранг, Amount: сколько дворфов, Seed: превью = выданный дворф;
//   Equipment — Key: ключ предмета (EquipmentCatalog), Amount: сколько штук;
//   Gem       — Key: камень (GemType), Rarity: ступень, Amount: сколько камней;
//   Ability   — AbilityKind + Key (как у PlayerAbility), Level, Amount: сколько штук;
//   Chest     — Key: тип сундука, у сундука бесконечности через двоеточие стихия ("Infinity:Fire"), Level: уровень,
//               Amount: сколько сундуков (ложатся на склад закрытыми);
//   Currency  — Key: MailCurrency, Amount: сколько
public class MailAttachment
{
    public Guid Id { get; set; }
    public Guid MailId { get; set; }

    public MailAttachmentKind Kind { get; set; }
    public string Key { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public Rarity Rarity { get; set; }
    public AbilityKind? AbilityKind { get; set; }
    public long Amount { get; set; } = 1;
    public int Seed { get; set; }
    // Порядок внутри своего вида — как собрал админ
    public int Position { get; set; }
    // Дворф, собранный в админке (MailHeroes); null — как из сундука: всё, кроме ранга и стихии, случайное
    public MailHeroSpec? HeroSpec { get; set; }
}

// Дворф из конструктора админки. Незаданное (null) выпадает случайно из seed вложения, как у дворфа из сундука.
// CustomAbilities: false — заклинание, пассивка и аура случайные; true — ровно заданные ниже, null — способности этого
// вида нет (так собирается «пустой» дворф). Иммунитеты у дворфа из конструктора всегда только заданные — не случайные
public class MailHeroSpec
{
    public string? Name { get; set; }
    public HeroProfession? Profession { get; set; }

    public bool CustomAbilities { get; set; }
    // Семейство заклинания стихии дворфа (Id первого уровня, ElementSpellBook)
    public int? SpellFamily { get; set; }
    public int SpellLevel { get; set; } = 1;
    public HeroPassive? Passive { get; set; }
    public int PassiveLevel { get; set; } = 1;
    // Эффект ауры (AuraBook)
    public string? Aura { get; set; }
    public int AuraLevel { get; set; } = 1;
    public List<string> Immunities { get; set; } = new();
}
