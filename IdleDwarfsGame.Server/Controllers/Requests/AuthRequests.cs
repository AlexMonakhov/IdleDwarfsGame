using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required(ErrorMessage = "Введите имя")]
    [StringLength(24, MinimumLength = 3, ErrorMessage = "Имя должно быть от 3 до 24 символов")]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "Имя может содержать только латиницу, цифры, _ и -")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите почту")]
    [EmailAddress(ErrorMessage = "Некорректная почта")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "Пароль должен быть не короче 6 символов")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    // Имя пользователя или почта
    [Required(ErrorMessage = "Введите имя или почту")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    public string Password { get; set; } = string.Empty;
}

public class AuthUserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid PlayerId { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthUserResponse User { get; set; } = new();
}
