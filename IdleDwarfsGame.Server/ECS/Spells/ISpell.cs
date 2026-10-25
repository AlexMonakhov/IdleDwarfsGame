public interface ISpell
{
    int Id { get; }
    string Name { get; }
    int Level { get; }
    int EnergyCost { get; } // Цена каста

    void Execute(World world, int casterId, CombatContext context);

    // Ожидаемая польза одного каста — из неё складывается боевая мощь (CombatPower)
    SpellEstimate Estimate(int casterAttack, int enemyCount);
}

// Польза каста по всем целям сразу: урон (с учётом доп. эффектов), лечение и сколько ходов врагов отнято станом
public readonly record struct SpellEstimate(double Damage = 0, double Healing = 0, double StunnedTurns = 0);
