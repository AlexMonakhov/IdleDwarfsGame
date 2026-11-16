namespace IdleDwarfsGame.Server.Enums;

// В ChestDropTables хранится числом — значения не переставлять
public enum ChestType
{
    Hero = 0,
    Gem = 1,
    Infinity = 2,
    Equipment = 3,
    // Стартовый сундук обучения: 25 дворфов 1-го ранга, пятеро сразу в отряде (Onboarding.StarterHeroes)
    Starter = 4
}
