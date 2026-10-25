// Стихия героя. У каждой стихии свой камень и цвет — по цвету фронт выбирает спрайт.
// Герои 1-го уровня стихии не имеют: они все Basic, стихия выбирается при улучшении.
public enum HeroElement
{
    Basic,
    Fire,      // рубин, красный
    Frost,     // сапфир, синий
    Lightning, // топаз, жёлтый
    Nature     // изумруд, зелёный
}

public static class HeroLevels
{
    public const int Min = 1;
    public const int Max = 5;

    // На минимальном уровне стихии нет, на остальных — есть
    public static HeroElement NormalizeElement(int level, HeroElement element) =>
        level <= Min ? HeroElement.Basic : element;
}
