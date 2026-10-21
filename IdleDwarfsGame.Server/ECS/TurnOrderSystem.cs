using System.Linq;

public class TurnOrderSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        // Если очередь пуста — начался новый раунд
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
            context.CurrentRound++;
            context.IsNewRound = true; // Поднимаем флаг нового раунда
        }
        else
        {
            context.IsNewRound = false; // В рамках раунда флаг сброшен
        }

        var nextEntity = context.CurrentRoundOrder[0];
        context.CurrentRoundOrder.RemoveAt(0);
        context.CurrentActor = nextEntity.Id;

        // Сбрасываем TurnState для ходящего
        if (world.TurnState.ContainsKey(nextEntity.Id))
        {
            world.TurnState[nextEntity.Id].CanAct = true;
            world.TurnState[nextEntity.Id].ActionCompleted = false;
        }
    }
}