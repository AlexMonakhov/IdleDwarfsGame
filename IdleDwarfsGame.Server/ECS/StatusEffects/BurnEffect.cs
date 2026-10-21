public class BurnEffect : IStatusEffect
{
    public string EffectName => "Burn";
    public int RemainingTurns { get; set; }
    private int _damagePerTurn;

    public BurnEffect(int turns = 3, int damagePerTurn = 5)
    {
        RemainingTurns = turns;
        _damagePerTurn = damagePerTurn;
    }

    public void ApplyEffect(World world, int targetId, CombatContext context)
    {
        // 1. ПРОВЕРЯЕМ ИММУНИТЕТ
        if (world.Immunities.ContainsKey(targetId))
        {
            if (world.Immunities[targetId].ImmuneTo.Contains(EffectName))
            {
                // Герой невосприимчив! Можно даже записать это в лог для UI
                context.Logs.Add(new CombatLogEntry
                {
                    AttackerId = -1,
                    TargetId = targetId,
                    AttackerTeam = 0,
                    TargetTeam = world.Team[targetId].TeamId,
                    ActionType = "Immune (Burn)",
                    Damage = 0,
                    TargetRemainingHp = world.Health[targetId].Current
                });

                // Прерываем наложение эффекта
                return;
            }
        }

        // 2. СТАРАЯ ЛОГИКА (накладываем эффект, если иммунитета нет)
        if (!world.StatusEffects.ContainsKey(targetId))
        {
            world.StatusEffects[targetId] = new StatusEffectComponent();
        }
        world.StatusEffects[targetId].ActiveEffects.Add(this);
    }

    public void OnTurnStart(World world, int targetId, CombatContext context)
    {
        if (world.Health[targetId].Current <= 0)
            return;

        world.Health[targetId].Current -= _damagePerTurn;

        context.Logs.Add(new CombatLogEntry
        {
            AttackerId = -1, // Эффект, а не персонаж
            TargetId = targetId,
            AttackerTeam = 0,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = "Burn",
            Damage = _damagePerTurn,
            TargetRemainingHp = Math.Max(0, world.Health[targetId].Current)
        });

        RemainingTurns--;
    }

    public void OnTurnEnd(World world, int targetId, CombatContext context)
    {
    }
}
