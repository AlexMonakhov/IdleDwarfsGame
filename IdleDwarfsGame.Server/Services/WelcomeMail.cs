// Письмо новичку при регистрации (AuthController): стартовые радужные монеты приходят в почту, а не сразу на счёт.
// Тексты писем хранятся готовыми строками, поэтому язык выбираем при отправке — по языку клиента при регистрации
public static class WelcomeMail
{
    public const string Sender = MentorAccount.Username;

    public static string Title(string? locale) => IsRussian(locale)
        ? "Добро пожаловать!"
        : "Welcome!";

    public static string Body(string? locale) => IsRussian(locale)
        ? $"Рад, что ты с нами! Держи {ShopRules.StarterRainbowCoins} радужных монет на первое время — ими можно обновлять магазины. А если в кампании станет туго, бери меня наёмником: я у тебя в друзьях. Брокк"
        : $"Glad to have you here! Take {ShopRules.StarterRainbowCoins} rainbow coins to get started — they refresh the shops. And if the Campaign gets tough, hire me as a mercenary: I'm in your friends list. Brokk";

    public static List<MailAttachment> Attachments() =>
    [
        new MailAttachment { Kind = MailAttachmentKind.Currency, Key = nameof(MailCurrency.RainbowCoins), Amount = ShopRules.StarterRainbowCoins }
    ];

    private static bool IsRussian(string? locale) => locale?.StartsWith("ru", StringComparison.OrdinalIgnoreCase) ?? false;
}
