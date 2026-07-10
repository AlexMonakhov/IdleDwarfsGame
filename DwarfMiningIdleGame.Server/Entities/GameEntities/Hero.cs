public class Hero
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Speed { get; set; }
    public int? PlaceInSquad { get; set; }
}