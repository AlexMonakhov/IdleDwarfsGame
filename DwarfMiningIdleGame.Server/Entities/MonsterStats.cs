public class MonsterStats
{
    public int MaxHp { get; set; }

    public int Attack { get; set; }

    public int Defense { get; set; }

    public int Speed { get; set; }

    public int CritChance { get; set; }

    public int CritDamage { get; set; }

    public int Accuracy { get; set; }

    public int Evasion { get; set; }

    public Guid TeamId { get; set; }

    public List<Guid> SkillIds { get; set; } = new();
}