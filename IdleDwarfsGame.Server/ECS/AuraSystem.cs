public class AuraSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        int actor = context.CurrentActor;
        if (actor == -1) return;

        // Ищем всех живых персонажей, у которых есть аура
        var auraBearers = world.Entities
            .Where(e => world.Health[e.Id].Current > 0 && world.Auras.ContainsKey(e.Id))
            .ToList();

        foreach (var bearer in auraBearers)
        {
            var aura = world.Auras[bearer.Id];

            // Проверяем условие триггера
            if (aura.Trigger == TriggerPhase.OnRoundStart && !context.IsNewRound) continue;
            if (aura.Trigger == TriggerPhase.OnTurnStart && bearer.Id != actor) continue;

            // Находим цели (союзники, враги или вообще все)
            var targets = GetTargets(world, bearer.Id, aura.TargetType);

            foreach (var targetId in targets)
            {
                // Используем наш стандартный механизм наложения эффектов!
                aura.EffectToApply.ApplyEffect(world, targetId, context);
            }
        }
    }

    private List<int> GetTargets(World world, int bearerId, AuraTarget targetType)
    {
        int bearerTeam = world.Team[bearerId].TeamId;

        return world.Entities
            .Where(e => world.Health[e.Id].Current > 0)
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