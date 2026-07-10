using System.Linq;

public class TurnOrderSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        // Если раунд закончился — формируем новый
        if (context.CurrentRoundOrder.Count == 0)
        {
            var aliveOrdered = world.Entities
                .Where(e => world.Health[e.Id].Current > 0)
                .OrderByDescending(e => world.Speed[e.Id].Value)
                .ToList();

            if (aliveOrdered.Count == 0)
            {
                context.CombatEnded = true;
                return;
            }

            context.CurrentRoundOrder = aliveOrdered;
        }

        // Берём следующего
        var nextEntity = context.CurrentRoundOrder[0];
        context.CurrentRoundOrder.RemoveAt(0);

        context.CurrentActor = nextEntity.Id;
    }
}