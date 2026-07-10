using System.Linq;

public class DamageSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        int attacker = context.CurrentActor;
        if (attacker == -1) return;

        int target = FindTarget(world, attacker);
        if (target == -1)
        {
            context.CombatEnded = true;
            return;
        }

        int damage = world.Attack[attacker].Damage;

        world.Health[target].Current -= damage;

        context.Logs.Add(
           new CombatLogEntry
           {
               AttackerId = attacker,
               TargetId = target,
               AttackerTeam = world.Team[attacker].TeamId,
               TargetTeam = world.Team[target].TeamId,
               ActionType = "BasicAttack",
               Damage = damage,
               TargetRemainingHp = Math.Max(0, world.Health[target].Current)
           }
        );
    }

    private int FindTarget(World world, int attacker)
    {
        int enemyTeam = world.Team[attacker].TeamId == 1 ? 2 : 1;

        return world.Entities
            .FirstOrDefault(e =>
                world.Team[e.Id].TeamId == enemyTeam &&
                world.Health[e.Id].Current > 0)?.Id ?? -1;
    }
}