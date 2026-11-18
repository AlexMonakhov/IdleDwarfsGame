// Начало раунда для бонусов стихий отряда: лечение «Корней мира», метка «Небесного суда», день и ночь «Равновесия»,
// резонанс «Призмы». Стоит после аур: ауры раунда уже наложены
public class SquadBonusSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        if (world.SquadBonuses.Count == 0 || !context.IsNewRound || context.CurrentActor == -1) return;
        SquadBonuses.RoundStarted(world, context);
    }
}
