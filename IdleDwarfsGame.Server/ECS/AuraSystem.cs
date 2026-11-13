public class AuraSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        int actor = context.CurrentActor;
        if (actor == -1) return;

        // Ищем всех живых персонажей, у которых есть аура
        var auraBearers = world.Entities
            .Where(e => world.IsAlive(e.Id) && world.Auras.ContainsKey(e.Id))
            .ToList();

        foreach (var bearer in auraBearers)
        {
            var aura = world.Auras[bearer.Id];

            // Проверяем условие триггера, затем интервал: аура может срабатывать не на каждом
            if (aura.Trigger == TriggerPhase.OnRoundStart && !context.IsNewRound) continue;
            if (aura.Trigger == TriggerPhase.OnTurnStart && bearer.Id != actor) continue;
            if (!aura.Fires()) continue;

            // Находим цели (например, всех живых союзников)
            var targets = GetTargets(world, bearer.Id, aura.TargetType);

            // Урон и лечение этих статусов в статистике — урон аурой
            context.StatusKind = CombatStatKind.Aura;
            try
            {
                foreach (var targetId in targets)
                {
                    // Новый экземпляр эффекта на каждую цель — одно наложение за срабатывание (docs/effect-rules.md)
                    aura.CreateEffect().ApplyEffect(world, targetId, context, bearer.Id);
                }
            }
            finally
            {
                context.StatusKind = null;
            }
        }
    }

    private List<int> GetTargets(World world, int bearerId, AuraTarget targetType)
    {
        int bearerTeam = world.Team[bearerId].TeamId;

        return world.Entities
            .Where(e => world.IsAlive(e.Id))
            .Where(e => targetType switch
            {
                AuraTarget.Allies => world.Team[e.Id].TeamId == bearerTeam,
                AuraTarget.Enemies => world.Team[e.Id].TeamId != bearerTeam,
                AuraTarget.All => true,
                _ => false
            })
            .Select(e => e.Id)
            .ToList();
    }
}
