using IdleDwarfsGame.Server.Enums;

public class MailException(string message) : Exception(message);

// Правила почты: лимиты письма и вложений, проверка вложения из админки, порядок вложений в письме.
// Порядок всегда один: дворфы, предметы, камни, способности (заклинания, пассивки, ауры), валюты
public static class MailRules
{
    public const int MaxSenderLength = 64;
    public const int MaxTitleLength = 120;
    public const int MaxBodyLength = 4000;
    public const int MaxAttachments = 50;

    public const string DefaultSender = "Administration";

    // Сколько штук за одно вложение: дворфов, предметов и способностей — отдельные строки, их немного;
    // камни и валюты — стопки
    public const int MaxHeroes = 20;
    public const int MaxEquipment = 50;
    public const int MaxAbilities = 20;
    public const int MaxChests = 50;
    public const long MaxGems = 1_000_000;
    public const long MaxCurrency = 1_000_000_000_000;

    // Ступени камней, которые бывают в игре
    public static readonly Rarity[] GemRarities = [Rarity.Common, Rarity.Uncommon, Rarity.Rare, Rarity.Mythic];

    public static readonly MailAttachmentKind[] KindOrder =
    [
        MailAttachmentKind.Hero, MailAttachmentKind.Equipment, MailAttachmentKind.Gem, MailAttachmentKind.Ability,
        MailAttachmentKind.Chest, MailAttachmentKind.Currency
    ];

    public static IEnumerable<MailAttachment> Ordered(IEnumerable<MailAttachment> attachments) => attachments
        .OrderBy(a => Array.IndexOf(KindOrder, a.Kind))
        .ThenBy(a => a.AbilityKind ?? AbilityKind.Spell)
        .ThenBy(a => a.Position);

    // Проверяет вложение и приводит его к виду для БД. Ошибка — MailException с причиной для админа
    public static MailAttachment Normalize(MailAttachment attachment)
    {
        switch (attachment.Kind)
        {
            case MailAttachmentKind.Hero:
                Require(attachment.Level is >= HeroLevels.Min and <= HeroLevels.Available, $"Hero rank must be {HeroLevels.Min}..{HeroLevels.Available}");
                Require(attachment.Key == string.Empty
                    || Enum.TryParse<HeroElement>(attachment.Key, out var element) && element != HeroElement.Basic && Enum.IsDefined(element),
                    $"Unknown element {attachment.Key}");
                RequireAmount(attachment, MaxHeroes);
                // На 1-м ранге стихии нет
                if (attachment.Level == HeroLevels.Min)
                    attachment.Key = string.Empty;
                if (attachment.HeroSpec != null)
                {
                    MailHeroes.Validate(attachment.HeroSpec, attachment.Level, attachment.Key);
                    attachment.HeroSpec.Name = string.IsNullOrWhiteSpace(attachment.HeroSpec.Name) ? null : attachment.HeroSpec.Name.Trim();
                }
                break;

            case MailAttachmentKind.Equipment:
                var item = EquipmentCatalog.Find(attachment.Key);
                Require(item != null, $"Unknown item {attachment.Key}");
                attachment.Rarity = item!.Rarity;
                attachment.Level = 1;
                RequireAmount(attachment, MaxEquipment);
                break;

            case MailAttachmentKind.Gem:
                Require(Enum.TryParse<GemType>(attachment.Key, out var gem) && Enum.IsDefined(gem), $"Unknown gem {attachment.Key}");
                Require(GemRarities.Contains(attachment.Rarity), $"Gems have no {attachment.Rarity} tier");
                attachment.Key = gem.ToString();
                attachment.Level = 1;
                RequireAmount(attachment, MaxGems);
                break;

            case MailAttachmentKind.Ability:
                Require(attachment.AbilityKind != null, "Ability kind is required");
                var ability = AbilityInfo.Describe(attachment.AbilityKind!.Value, attachment.Key, attachment.Level);
                Require(ability != null && ability.Level == attachment.Level, $"Unknown ability {attachment.AbilityKind} {attachment.Key} level {attachment.Level}");
                RequireAmount(attachment, MaxAbilities);
                break;

            case MailAttachmentKind.Chest:
                Require(TryParseChest(attachment.Key, out var chestType, out var chestElement), $"Unknown chest {attachment.Key}");
                // Сундук выдаётся только такой, для которого есть таблица дропа (иначе его не открыть)
                Require(LootSeedData.TablesWithEntries().Any(t => t.ChestType == chestType && t.ChestLevel == attachment.Level && t.Element == chestElement),
                    $"No {attachment.Key} chest of level {attachment.Level}");
                attachment.Key = ChestKey(chestType, chestElement);
                RequireAmount(attachment, MaxChests);
                break;

            case MailAttachmentKind.Currency:
                Require(Enum.TryParse<MailCurrency>(attachment.Key, out var currency) && Enum.IsDefined(currency), $"Unknown currency {attachment.Key}");
                attachment.Key = currency.ToString();
                attachment.Level = 1;
                RequireAmount(attachment, MaxCurrency);
                break;

            default:
                throw new MailException($"Unknown attachment kind {attachment.Kind}");
        }

        if (attachment.Kind != MailAttachmentKind.Ability)
            attachment.AbilityKind = null;
        if (attachment.Kind != MailAttachmentKind.Hero)
            attachment.HeroSpec = null;
        return attachment;
    }

    // Ключ сундука во вложении: тип, у сундука со стихией — "Infinity:Fire"
    public static string ChestKey(ChestType type, HeroElement element) =>
        element == HeroElement.Basic ? type.ToString() : $"{type}:{element}";

    public static bool TryParseChest(string key, out ChestType type, out HeroElement element)
    {
        element = HeroElement.Basic;
        var parts = key.Split(':', 2);
        if (!Enum.TryParse(parts[0], out type) || !Enum.IsDefined(type))
            return false;
        return parts.Length == 1 || Enum.TryParse(parts[1], out element) && Enum.IsDefined(element);
    }

    private static void RequireAmount(MailAttachment attachment, long max) =>
        Require(attachment.Amount >= 1 && attachment.Amount <= max, $"{attachment.Kind} amount must be 1..{max}");

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new MailException(message);
    }
}
