using System.ComponentModel.DataAnnotations;

public class SendFriendRequestRequest
{
    // Имя игрока, как при регистрации (регистр не важен)
    [Required(ErrorMessage = "Введите имя игрока")]
    [StringLength(24, ErrorMessage = "Имя не длиннее 24 символов")]
    public string Username { get; set; } = string.Empty;
}
