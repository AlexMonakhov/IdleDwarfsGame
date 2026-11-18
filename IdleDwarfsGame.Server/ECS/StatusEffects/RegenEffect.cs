// Регенерация: лечит в начале хода носителя, наложения — независимые слои
public class RegenEffect : LayeredStatusEffect
{
    public override string EffectName => "Regen";

    public RegenEffect(int turns = 1, int healPerTurn = 5) : base(turns, healPerTurn)
    {
    }

    public override void OnTurnStart(World world, int targetId, CombatContext context)
    {
        int heal = Power;
        var weights = OriginWeights();
        PassTurn();
        if (!world.IsAlive(targetId)) return;

        // Лог пишем и без лечения (полное HP) — по нему фронт обновляет оставшиеся ходы
        int healed = world.ApplyHeal(targetId, heal);
        var log = TickLog(world, targetId);
        if (healed > 0)
        {
            log.EventType = CombatEventType.Heal;
            log.Heal = healed;
            log.Shares = CombatStatsTracker.Split(healed, weights);
        }
        context.AddLog(log);
        // Источники — снимок до снятия слоёв: на последнем тике своих слоёв уже нет
        if (world.SquadBonuses.Count > 0) DuringTick(weights, () => SquadBonuses.RegenTicked(world, targetId, this));
    }
}
