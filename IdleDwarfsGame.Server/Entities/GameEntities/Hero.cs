public class Hero
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Внешний вид (для фронта): спрайт выбирается по типу, уровню и стихии
    public string UnitType { get; set; } = "Dwarf";
    // Профессия дворфа задаёт базовые статы (ProfessionBook); у монстров её нет
    public HeroProfession? Profession { get; set; }
    public HeroElement Element { get; set; } = HeroElement.Basic;
    // HeroLevels.Min..HeroLevels.Max
    public int Level { get; set; } = 1;
    // Внутренний уровень прокачки за золото и опыт: 1..HeroLevels.MaxUpgradeLevel(Level), растит HP и атаку.
    // Не путать с Level: тот меняет вид и способности, а этот сбрасывается при повышении Level
    public int UpgradeLevel { get; set; } = 1;

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
    // Процент (AuraBook): урон или лечение за ход — доля атаки носителя, у мороза — процент замедления
    public int AuraPower { get; set; }
    public int AuraDuration { get; set; } = 1;
    // Аура срабатывает раз в столько своих триггеров (раундов или ходов носителя): 1 — каждый, 2 — через один
    public int AuraInterval { get; set; } = 1;
    // Уровень ауры из AuraBook (1..AuraBook.MaxLevel): с ним аура извлекается из дворфа
    public int AuraLevel { get; set; } = AuraBook.MinLevel;

    // PassiveComponent: пассивка дворфа и её уровень (1..PassiveBook.MaxLevel)
    public List<HeroPassive> Passives { get; set; } = new();
    public int PassiveLevel { get; set; } = PassiveBook.MinLevel;

    // Надетые предметы. Попадают и в снимок отряда рейтинговой арены (RankedSquad)
    public List<PlayerEquipment> Equipment { get; set; } = new();

    // Копия для расчётов «а что будет на другом уровне прокачки»; в БД не попадает
    public Hero WithUpgradeLevel(int upgradeLevel)
    {
        var copy = (Hero)MemberwiseClone();
        copy.UpgradeLevel = upgradeLevel;
        return copy;
    }
}
