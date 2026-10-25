// Арена кампании: фон на фронте + глобальный эффект, который висит на всех участниках боя
public class Arena
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Ключ фона на фронте ("lava", "ice")
    public string Background { get; set; } = string.Empty;

    // Эффект арены: имя статуса из StatusEffectFactory ("Burn", "Frost", ...). null — арена без эффекта.
    // Вешается на всех живых в начале каждого раунда.
    public string? EffectName { get; set; }
    public int EffectPower { get; set; }
    public int EffectDuration { get; set; } = 1;
}
