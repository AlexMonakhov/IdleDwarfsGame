// Стихия героя. У каждой стихии свой камень и цвет — по цвету фронт выбирает спрайт.
// Герои 1-го уровня стихии не имеют: они все Basic, стихия выбирается при улучшении.
public enum HeroElement
{
    Basic,
    Fire,      // рубин, красный
    Frost,     // сапфир, синий
    Lightning, // топаз, жёлтый
    Nature,    // изумруд, зелёный
    Dark,      // аметист, фиолетовый
    Radiance,  // бриллиант, радужный
    Blood      // гранат, малиновый
}

public static class HeroLevels
{
    public const int Min = 1;
    public const int Max = 5;
    // Выше этого ранга героев пока не выдаём: для 5-го ранга ещё нет спрайтов
    public const int Available = 4;

    // На минимальном уровне стихии нет, на остальных — есть
    public static HeroElement NormalizeElement(int level, HeroElement element) =>
        level <= Min ? HeroElement.Basic : element;

    // Потолок прокачки (Hero.UpgradeLevel) по рангу: 1-й ранг почти не качают — это материал для слияния и шахт
    public static int MaxUpgradeLevel(int rank) => rank switch
    {
        <= 1 => 10,
        2 => 20,
        3 => 50,
        4 => 100,
        _ => 150
    };

    // Каждый уровень прокачки даёт +5% к статам ранга (HeroUpgradeService.StatFactor)
    public const double StatBonusPerUpgradeLevel = 0.05;

    public static double UpgradeFactor(int upgradeLevel) => 1 + StatBonusPerUpgradeLevel * (Math.Max(1, upgradeLevel) - 1);

    // Множитель статов ранга к статам профессии. Первый уровень нового ранга равен максимуму предыдущего:
    // слияние не отнимает силу, а даёт новый потолок и способности.
    // 1 → ×1, 2 → ×1.45, 3 → ×2.83, 4 → ×9.75, 5 → ×58
    public static double RankFactor(int rank)
    {
        double factor = 1;
        for (int r = Min; r < Math.Clamp(rank, Min, Max); r++)
            factor *= UpgradeFactor(MaxUpgradeLevel(r));
        return factor;
    }
}
