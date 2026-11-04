// Профессия дворфа: задаёт базовые HP, атаку и скорость (статы 1-го ранга, дальше их растят ранг и прокачка).
// Хранится строкой — значения можно переставлять, но не переименовывать без миграции.
// У монстров профессии нет (Hero.Profession == null).
// Позже профессия даст бонус вне боя: рудокоп — к добыче в шахте, разведчик — к наградам экспедиций (BALANCE_TODO.md)
public enum HeroProfession
{
    // Защитник: крепкий, медленный, бьёт слабо — стоит впереди
    Defender,
    Warrior,
    // Секироносец: самый сильный удар, хрупкий
    Axeman,
    Miner,
    // Разведчик: самый быстрый и хрупкий
    Scout
}

public static class ProfessionBook
{
    public record Stats(int MaxHp, int Attack, int Speed);

    // В среднем HP ≈ 8 атак: бой равных отрядов длится 4–6 раундов
    private static readonly Dictionary<HeroProfession, Stats> Base = new()
    {
        [HeroProfession.Defender] = new(1000, 75, 3),
        [HeroProfession.Warrior] = new(880, 100, 5),
        [HeroProfession.Axeman] = new(720, 125, 4),
        [HeroProfession.Miner] = new(800, 95, 6),
        [HeroProfession.Scout] = new(640, 110, 8)
    };

    // Порядок стартового отряда по слотам: впереди защитник, сзади разведчик
    public static readonly HeroProfession[] SquadOrder =
    [
        HeroProfession.Defender, HeroProfession.Warrior, HeroProfession.Axeman, HeroProfession.Miner, HeroProfession.Scout
    ];

    public static Stats Of(HeroProfession profession) => Base[profession];

    public static HeroProfession Random(Random random) => SquadOrder[random.Next(SquadOrder.Length)];
}
