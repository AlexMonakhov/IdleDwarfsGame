using System.Linq;

public class HealthSystem : ICombatSystem
{
    public void Execute(World world, CombatContext context)
    {
        ResolveLastStands(world, context);

        // Фиксируем смерти (один раз на юнита). Реакция предмета на смерть (раскол, перенос яда) может добить
        // кого-то ещё — повторяем, пока смерти не кончатся
        bool died;
        do
        {
            died = false;
            foreach (var entity in world.Entities)
            {
                var hp = world.Health[entity.Id];
                if (hp.Current < 0) hp.Current = 0;

                if (!world.Info.TryGetValue(entity.Id, out var info))
                    world.Info[entity.Id] = info = new UnitInfoComponent();

                if (hp.Current == 0 && !info.IsDead)
                {
                    info.IsDead = true;
                    died = true;
                    var lastStatuses = world.StatusEffects.TryGetValue(entity.Id, out var statuses)
                        ? statuses.ActiveEffects.ToList()
                        : [];
                    statuses?.ActiveEffects.Clear();

                    context.AddLog(new CombatLogEntry
                    {
                        EventType = CombatEventType.Death,
                        Phase = "Death",
                        TargetId = entity.Id,
                        TargetTeam = world.Team[entity.Id].TeamId,
                        ActionType = "Death",
                        TargetRemainingHp = 0
                    });

                    Avenge(world, entity.Id);
                    ItemReactions.Died(world, context, entity.Id, lastStatuses);
                }
            }
        } while (died);

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

    // «Последний рубеж» оставил героев с 1 HP (World.ApplyDamage) — лечим их на долю максимального HP
    private static void ResolveLastStands(World world, CombatContext context)
    {
        foreach (int id in world.PendingLastStands)
        {
            if (!world.IsAlive(id) || world.GetPassive(id, HeroPassive.LastStand) is not { } lastStand) continue;

            int healed = world.ApplyHeal(id, world.Health[id].Max * lastStand.Percent / 100);
            context.AddLog(new CombatLogEntry
            {
                EventType = CombatEventType.Heal,
                Phase = "Passive",
                AttackerId = id,
                TargetId = id,
                AttackerTeam = world.Team[id].TeamId,
                TargetTeam = world.Team[id].TeamId,
                ActionType = nameof(HeroPassive.LastStand),
                SkillName = nameof(HeroPassive.LastStand),
                Heal = healed,
                TargetRemainingHp = world.Health[id].Current
            });
        }
        world.PendingLastStands.Clear();
    }

    // «Мститель»: каждый погибший союзник прибавляет живым мстителям долю их стартовой атаки
    private static void Avenge(World world, int deadId)
    {
        int team = world.Team[deadId].TeamId;
        foreach (var ally in world.Entities.Where(e => e.Id != deadId && world.Team[e.Id].TeamId == team && world.IsAlive(e.Id)))
        {
            if (world.GetPassive(ally.Id, HeroPassive.Avenger) is not { } avenger) continue;
            var attack = world.Attack[ally.Id];
            attack.Damage += Math.Max(1, attack.Base * avenger.Percent / 100);
        }
    }
}
