using System.ComponentModel.DataAnnotations;

public class ExtractAbilityRequest
{
    [Required]
    public Guid HeroId { get; set; }

    [Required]
    public AbilityKind Kind { get; set; }

    // Как в AbilityInfo.Key: семейство заклинания, название пассивки или эффект ауры
    [Required]
    [MaxLength(32)]
    public string Key { get; set; } = string.Empty;
}

public class InsertAbilityRequest
{
    [Required]
    public Guid HeroId { get; set; }
}

public class CombineAbilitiesRequest
{
    [Required]
    public Guid FirstId { get; set; }

    [Required]
    public Guid SecondId { get; set; }
}

public class FuseHeroesRequest
{
    [Required]
    public List<Guid> HeroIds { get; set; } = new();
}
