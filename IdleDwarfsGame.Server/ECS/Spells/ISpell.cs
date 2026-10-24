public interface ISpell
{
    int Id { get; }
    string Name { get; }
    int Level { get; }
    int EnergyCost { get; } // Цена каста

    void Execute(World world, int casterId, CombatContext context);
}
