using System.ComponentModel.DataAnnotations;

public class SaveSquadRequest
{
    // Слоты отряда по порядку (0 — передняя линия), null — слот пустой
    [Required]
    [MaxLength(StarterSquad.Size, ErrorMessage = "В отряде не больше 5 мест")]
    public List<Guid?> Slots { get; set; } = new();
}
