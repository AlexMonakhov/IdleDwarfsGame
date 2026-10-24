using System.Linq;

public class TurnOrderSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        // Убираем из очереди тех, кто погиб раньше в этом раунде
        context.CurrentRoundOrder.RemoveAll(e => !world.IsAlive(e.Id));

        // Если очередь пуста — начинаем новый раунд
        if (context.CurrentRoundOrder.Count == 0)
        {
            var aliveOrdered = world.Entities
                .Where(e => world.IsAlive(e.Id))
                .OrderByDescending(e => world.Speed[e.Id].Value)
                .ThenBy(e => world.Team[e.Id].TeamId)
                .ThenBy(e => world.GetPosition(e.Id))
                .ToList();

            if (aliveOrdered.Count == 0)
            {
                context.CurrentActor = -1;
                context.CombatEnded = true;
                return;
            }

            context.CurrentRoundOrder = aliveOrdered;
            context.CurrentRound++;
            context.IsNewRound = true; // Поднимаем флаг нового раунда
        }
        else
        {
            context.IsNewRound = false; // В начале любого хода сбросим
        }

        var nextEntity = context.CurrentRoundOrder[0];
        context.CurrentRoundOrder.RemoveAt(0);
        context.CurrentActor = nextEntity.Id;
        context.NextAction();

        // Сбрасываем TurnState для текущего
        if (world.TurnState.ContainsKey(nextEntity.Id))
        {
            world.TurnState[nextEntity.Id].CanAct = true;
            world.TurnState[nextEntity.Id].ActionCompleted = false;
        }
    }
}
