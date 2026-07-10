public class CombatEngine : ICombatEngine
{
    private List<ICombatSystem> _systems;

    public CombatEngine()
    {
        _systems = new List<ICombatSystem>
        {
            new TurnOrderSystem(),
            new DamageSystem(),
            new HealthSystem()
        };
    }

    public CombatContext Run(World world)
    {
        var context = new CombatContext();

        while (!context.CombatEnded)
        {
            foreach (var system in _systems)
            {
                system.Execute(world, context);

                if (context.CombatEnded)
                    break;
            }
        }

        return context;
    }
}

//public class CombatSystem
//{
//    //public List<CombatLogEntry> RunCombat(World world)
//    //{
//    //    var logs = new List<CombatLogEntry>();

//    //    while (TeamAlive(world, 1) && TeamAlive(world, 2))
//    //    {
//    //        var turnOrder = world.Entities
//    //            .Where(e => world.Health[e.Id].Current > 0)
//    //            .OrderByDescending(e => world.Speed[e.Id].Value)
//    //            .ToList();

//    //        foreach (var entity in turnOrder)
//    //        {
//    //            if (world.Health[entity.Id].Current <= 0)
//    //                continue;

//    //            int attackerTeam = world.Team[entity.Id].TeamId;
//    //            int enemyTeam = attackerTeam == 1 ? 2 : 1;

//    //            var target = world.Entities
//    //                .Where(e => world.Team[e.Id].TeamId == enemyTeam
//    //                            && world.Health[e.Id].Current > 0)
//    //                .FirstOrDefault();

//    //            if (target == null)
//    //                break;

//    //            int damage = world.Attack[entity.Id].Damage;

//    //            world.Health[target.Id].Current -= damage;

//    //            logs.Add(new CombatLogEntry
//    //            {
//    //                AttackerId = entity.Id,
//    //                TargetId = target.Id,
//    //                AttackerTeam = attackerTeam,
//    //                TargetTeam = enemyTeam,
//    //                ActionType = "BasicAttack",
//    //                Damage = damage,
//    //                TargetRemainingHp =
//    //                    Math.Max(0, world.Health[target.Id].Current)
//    //            });
//    //        }
//    //    }

//    //    return logs;
//    //}

//    //private bool TeamAlive(World world, int teamId)
//    //{
//    //    return world.Entities.Any(e =>
//    //        world.Team[e.Id].TeamId == teamId &&
//    //        world.Health[e.Id].Current > 0);
//    //}
//}