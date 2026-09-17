public interface ISpell
{
    int Id { get; }
    string Name { get; }
    int Level { get; }
    int EnergyCost { get; } // Новое поле!

    void Execute(World world, int casterId, CombatContext context);
}
