public class Hero
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Внешний вид (для фронта)
    public string UnitType { get; set; } = "Dwarf";
    public string Color { get; set; } = "red";
    public int Level { get; set; } = 1;

    // HealthComponent / AttackComponent / SpeedComponent
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Speed { get; set; }

    // Позиция в отряде (1 — передняя линия). null — герой не в отряде
    public int? PlaceInSquad { get; set; }

    // EnergyComponent
    public int MaxEnergy { get; set; } = 100;
    public int StartEnergy { get; set; }
    public int EnergyPerAttack { get; set; } = 20;

    // SpellComponent: Id спеллов из SpellRegistry
    public List<int> SpellIds { get; set; } = new();

    // ImmunityComponent: названия эффектов ("Burn", "Stun", ...)
    public List<string> Immunities { get; set; } = new();

    // AuraComponent: AuraEffect == null — ауры нет
    public string? AuraEffect { get; set; }
    public AuraTarget AuraTarget { get; set; }
    public TriggerPhase AuraTrigger { get; set; } = TriggerPhase.OnRoundStart;
    public int AuraPower { get; set; }
    public int AuraDuration { get; set; } = 1;
}
