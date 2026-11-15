using IdleDwarfsGame.Server.Enums;

public enum ChestBonusKind { Gold, Experience, Gems, RainbowCoins, EventCurrency }

// Доп. награда сундука. Rarity — только цвет карты на клиенте; SubType — камень у Gems
public record ChestBonus(ChestBonusKind Kind, int Amount, Rarity Rarity, string SubType = "");

// Доп. награды при открытии сундука: карт всегда CardCount, основные награды (таблица дропа) занимают сколько выпало,
// остальное добивается бонусами. Сундук с двумя дворфами — 3 бонуса, с одним предметом — 4.
// Золото и опыт — в «уровнях кампании» текущего этапа игрока (DailyQuests.GoldFor), чтобы бонус не обесценивался.
// Радужные монеты и валюта ивента — редкие, только в сундуках от RareCurrencyMinLevel; валюта ивента — только пока ивент идёт
public static class ChestBonuses
{
    public const int CardCount = 5;
    public const int RareCurrencyMinLevel = 4;

    // Веса обычных бонусов
    public const int GoldWeight = 40;
    public const int ExperienceWeight = 35;
    public const int GemsWeight = 25;

    // Шанс, что бонус-золото или опыт окажется «джекпотом» ×JackpotMultiplier (карта редкая)
    public const double JackpotChance = 0.08;
    public const int JackpotMultiplier = 3;

    // Шанс редкой валюты за одну бонусную карту по уровню сундука (индекс — уровень)
    public static readonly double[] RainbowCoinChance = [0, 0, 0, 0, 0.03, 0.05];
    public static readonly double[] EventCurrencyChance = [0, 0, 0, 0, 0.06, 0.08];

    private static readonly GemType[] ElementGems = [GemType.Ruby, GemType.Sapphire, GemType.Topaz, GemType.Emerald];

    public static int CountFor(int mainRewards) => Math.Max(0, CardCount - mainRewards);

    // Сколько «уровней кампании» стоит бонус-золото/опыт: растёт с уровнем сундука, ±40% случайно
    public static double StagesFor(int chestLevel, Random random) => (0.6 + 0.2 * chestLevel) * (0.6 + 0.8 * random.NextDouble());

    public static List<ChestBonus> Roll(int count, int chestLevel, PlayerProgress player, bool eventActive, Random random)
    {
        var bonuses = new List<ChestBonus>(count);
        for (int i = 0; i < count; i++)
            bonuses.Add(RollOne(chestLevel, player, eventActive, random));
        return bonuses;
    }

    private static ChestBonus RollOne(int chestLevel, PlayerProgress player, bool eventActive, Random random)
    {
        int level = Math.Clamp(chestLevel, 1, LootSeedData.MaxChestLevel);

        if (random.NextDouble() < RainbowCoinChance[level])
            return new ChestBonus(ChestBonusKind.RainbowCoins, random.Next(1, level - 2), Rarity.Legendary);
        if (eventActive && random.NextDouble() < EventCurrencyChance[level])
            return new ChestBonus(ChestBonusKind.EventCurrency, random.Next(10, 26) * (level - 3), Rarity.Mythic);

        int roll = random.Next(GoldWeight + ExperienceWeight + GemsWeight);
        if (roll < GoldWeight + ExperienceWeight)
        {
            bool gold = roll < GoldWeight;
            bool jackpot = random.NextDouble() < JackpotChance;
            double stages = StagesFor(level, random) * (jackpot ? JackpotMultiplier : 1);
            int amount = gold ? DailyQuests.GoldFor(stages, player) : DailyQuests.ExperienceFor(stages, player);
            // Без текущего этапа кампании золото считать не от чего — вместо него камни
            if (amount > 0)
                return new ChestBonus(gold ? ChestBonusKind.Gold : ChestBonusKind.Experience, amount, jackpot ? Rarity.Rare : Rarity.Common);
        }

        // Немного камней стихий; в сундуках повыше — иногда необычные
        var gem = ElementGems[random.Next(ElementGems.Length)];
        bool uncommon = level >= 3 && random.NextDouble() < 0.25;
        int gems = uncommon ? random.Next(1, level) : random.Next(1 + level, 3 + 2 * level);
        return new ChestBonus(ChestBonusKind.Gems, gems, uncommon ? Rarity.Uncommon : Rarity.Common, gem.ToString());
    }
}
