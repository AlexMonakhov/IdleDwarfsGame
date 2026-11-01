// Внешний вид героя внутри стихии. На механику не влияет, по нему фронт выбирает спрайт.
// Violet — светло-фиолетовый вариант героя тьмы, у остальных стихий вариантов нет
public enum HeroAppearance
{
    Default,
    Violet
}

public static class HeroAppearances
{
    // Вариант есть только у тьмы: на других стихиях поле не значит ничего и сбрасывается
    public static HeroAppearance Normalize(HeroElement element, HeroAppearance appearance) =>
        element == HeroElement.Dark ? appearance : HeroAppearance.Default;

    // Новый герой тьмы получает любой из двух вариантов с равным шансом
    public static HeroAppearance Random(HeroElement element, Random random) =>
        element == HeroElement.Dark && random.Next(2) == 0 ? HeroAppearance.Violet : HeroAppearance.Default;
}
