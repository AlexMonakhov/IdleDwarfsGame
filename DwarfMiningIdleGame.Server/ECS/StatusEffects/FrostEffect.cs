public class FrostEffect : IStatusEffect
{
    public string EffectName => "Frost";
    public int RemainingTurns { get; set; }

    public FrostEffect(int turns = 2)
    {
        RemainingTurns = turns;
    }

    public void ApplyEffect(World world, int targetId, CombatContext context)
    {
        if (!world.StatusEffects.ContainsKey(targetId))
        {
            world.StatusEffects[targetId] = new StatusEffectComponent();
        }
        world.StatusEffects[targetId].ActiveEffects.Add(this);
    }

    public void OnTurnStart(World world, int targetId, CombatContext context)
    {
        context.Logs.Add(new CombatLogEntry
        {
            AttackerId = -1,
            TargetId = targetId,
            AttackerTeam = 0,
            TargetTeam = world.Team[targetId].TeamId,
            ActionType = "Frost",
            Damage = 0,
            TargetRemainingHp = world.Health[targetId].Current
        });
        
        RemainingTurns--;
    }

    public void OnTurnEnd(World world, int targetId, CombatContext context)
    {
    }
}
