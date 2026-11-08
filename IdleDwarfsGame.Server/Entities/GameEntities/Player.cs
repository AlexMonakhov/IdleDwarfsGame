public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rating { get; set; }

    // Общие на аккаунт ресурсы
    public long Gold { get; set; }
    public long Experience { get; set; }

    // Радужные монеты — редкая (донатная) валюта: пока ими платят за обновление магазинов сверх бесплатного.
    // Новый игрок получает ShopRules.StarterRainbowCoins
    public long RainbowCoins { get; set; }
    // Кровавые монеты — валюта подпольного рынка, даются за бои на арене (ShopRules.BloodCoinsFor)
    public long BloodCoins { get; set; }
    // Значки почёта — валюта лавки гильдии, даются за удары по боссам гильдии и их убийство (ShopRules)
    public long HonorBadges { get; set; }
    // Печати старейшин — валюта лавки старейшин, даются за ежедневные задания (DailyQuests)
    public long ElderSeals { get; set; }

    // Сила кирки — общий на все шахты запас кликов (MiningService.DigEnergyMax), восстанавливается со временем.
    // Посчитана на момент DigEnergyAt, дальше растёт лениво; DigEnergyAt == null — кирка полная
    public double DigEnergy { get; set; }
    public DateTime? DigEnergyAt { get; set; }

    // Улучшения кирки (MiningService.PickaxeMaxLevel): ёмкость — запас кликов, восстановление — кликов в час,
    // сила — сколько приносит клик
    public int PickaxeCapacityLevel { get; set; } = 1;
    public int PickaxeRecoveryLevel { get; set; } = 1;
    public int PickaxeStrengthLevel { get; set; } = 1;

    // Мощь отряда защиты — героев с местом в отряде, против них дерутся в PvP (CombatPower.OfSquad).
    // Хранится, чтобы подбор соперников не собирал отряды всех игроков на каждый запрос.
    // Пересчитывает SquadPowerService.RefreshAsync после всего, что меняет героев отряда; на старте сервера — у всех
    public int SquadPower { get; set; }
}
