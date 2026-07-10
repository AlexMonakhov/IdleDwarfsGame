using System.Linq;

public class HealthSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        foreach (var entity in world.Entities)
        {
            if (world.Health[entity.Id].Current <= 0)
                world.Health[entity.Id].Current = 0;
        }

        bool team1Alive = world.Entities.Any(e =>
            world.Team[e.Id].TeamId == 1 &&
            world.Health[e.Id].Current > 0);

        bool team2Alive = world.Entities.Any(e =>
            world.Team[e.Id].TeamId == 2 &&
            world.Health[e.Id].Current > 0);

        if (!team1Alive || !team2Alive)
        {
            context.CombatEnded = true;
            context.Logs.Add(new CombatLogEntry());
            context.Win = team1Alive;
        }
    }
}