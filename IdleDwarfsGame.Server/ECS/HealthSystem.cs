using System.Linq;

public class HealthSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        // Фиксируем смерти (один раз на юнита)
        foreach (var entity in world.Entities)
        {
            var hp = world.Health[entity.Id];
            if (hp.Current < 0) hp.Current = 0;

            if (!world.Info.TryGetValue(entity.Id, out var info))
                world.Info[entity.Id] = info = new UnitInfoComponent();

            if (hp.Current == 0 && !info.IsDead)
            {
                info.IsDead = true;
                if (world.StatusEffects.TryGetValue(entity.Id, out var statuses))
                    statuses.ActiveEffects.Clear();

                context.AddLog(new CombatLogEntry
                {
                    EventType = CombatEventType.Death,
                    Phase = "Death",
                    TargetId = entity.Id,
                    TargetTeam = world.Team[entity.Id].TeamId,
                    ActionType = "Death",
                    TargetRemainingHp = 0
                });
            }
        }

        bool team1Alive = world.Entities.Any(e => world.Team[e.Id].TeamId == 1 && world.IsAlive(e.Id));
        bool team2Alive = world.Entities.Any(e => world.Team[e.Id].TeamId == 2 && world.IsAlive(e.Id));

        if (!team1Alive || !team2Alive)
        {
            context.CombatEnded = true;
            context.Win = team1Alive;
            context.WinningTeam = team1Alive ? 1 : team2Alive ? 2 : 0;
            context.AddLog(new CombatLogEntry
            {
                EventType = CombatEventType.CombatEnd,
                Phase = "End",
                ActionType = "CombatEnd",
                WinningTeam = context.WinningTeam
            });
        }
    }
}
