// Действия игрока, которые считаются за сутки (PlayerDailyActivity): по ним идут ежедневные задания (DailyQuests).
// Хранится строкой — значения не переименовывать без миграции, новые добавляются в конец
public enum ActivityKind
{
    // Любой бой кампании и победа в нём (перепрохождение пройденных уровней тоже считается)
    CampaignFight,
    CampaignWin,
    // Попытка этажа башни
    TowerFight,
    // Бой на арене (обычной или рейтинговой) и победа в нём
    PvpFight,
    PvpWin,
    GuildBossAttack,
    // Засчитанные клики по шахтам (а не пачки кликов)
    MineDig,
    MineCollect,
    ExpeditionStart,
    ExpeditionClaim,
    ChestOpen,
    HeroUpgrade,
    // Слияние предметов или камней в кузнице (камни — по числу слияний)
    ForgeFuse,
    ShopBuy
}
