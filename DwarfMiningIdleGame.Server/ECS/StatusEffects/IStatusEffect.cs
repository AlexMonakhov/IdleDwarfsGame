public interface IStatusEffect
{
    string EffectName { get; }
    int RemainingTurns { get; set; }
    void ApplyEffect(World world, int targetId, CombatContext context);
    void OnTurnStart(World world, int targetId, CombatContext context);
    void OnTurnEnd(World world, int targetId, CombatContext context);
}