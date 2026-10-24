// Учётная запись для входа в игру. Игровые данные (рейтинг, герои) живут в Player.
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string NormalizedUsername { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
}
