public interface ICombatEngine
{
    // maxRounds — лимит раундов: на следующем бой заканчивается поражением игрока
    CombatContext Run(World world, int maxRounds = CombatContext.DefaultMaxRounds);
}
