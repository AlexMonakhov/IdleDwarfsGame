using System.ComponentModel.DataAnnotations;

public class SaveSquadRequest
{
    // Слоты отряда по порядку (0 — передняя линия), null — слот пустой
    [Required]
    [MaxLength(StarterSquad.Size, ErrorMessage = "В отряде не больше 5 мест")]
    public List<Guid?> Slots { get; set; } = new();
}

public class RenameHeroRequest
{
    // Новое имя дворфа: пробелы по краям обрезаются, длина 1..HeroesController.MaxNameLength
    [Required]
    public string Name { get; set; } = string.Empty;
}
