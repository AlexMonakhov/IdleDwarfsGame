using System.ComponentModel.DataAnnotations;

public class StartExpeditionRequest
{
    // Герои по ячейкам задания
    [Required]
    [MinLength(1, ErrorMessage = "Отправьте хотя бы одного героя")]
    [MaxLength(ExpeditionService.MaxHeroes, ErrorMessage = "В задание можно отправить не больше 4 героев")]
    public List<Guid> HeroIds { get; set; } = new();
}
