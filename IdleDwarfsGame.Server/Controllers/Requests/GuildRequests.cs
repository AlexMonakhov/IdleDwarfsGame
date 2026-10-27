using System.ComponentModel.DataAnnotations;

public class CreateGuildRequest
{
    [Required(ErrorMessage = "Введите название гильдии")]
    [StringLength(GuildService.MaxNameLength, ErrorMessage = "Название не длиннее 24 символов")]
    public string Name { get; set; } = string.Empty;
}
