public class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rating { get; set; }

    // Общие на аккаунт ресурсы
    public long Gold { get; set; }
    public long Experience { get; set; }
}
