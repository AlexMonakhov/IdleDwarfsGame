using System.ComponentModel.DataAnnotations;

public class BuildMineRequest
{
    [Required]
    public MineType Type { get; set; }
}

public class DigMineRequest
{
    // Клики с прошлой пачки
    [Range(1, MiningService.MaxClicksPerBatch)]
    public int Clicks { get; set; }
}

public class UpgradeMineRequest
{
    [Required]
    public MineUpgrade Upgrade { get; set; }
}

public class AssignMineDwarfRequest
{
    [Required]
    public Guid HeroId { get; set; }
}
