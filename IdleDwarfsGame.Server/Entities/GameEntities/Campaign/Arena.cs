// Арена боя: фон на фронте + глобальный эффект, который висит на всех участниках боя.
// На аренах идут уровни кампании и бои с боссами гильдии
public class Arena
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Ключ фона на фронте ("lava", "ice")
    public string Background { get; set; } = string.Empty;

    // Стихия арены: по ней фронт красит карточки боссов гильдии и всё, что связано с ареной
    public HeroElement Element { get; set; } = HeroElement.Basic;

    // Эффект арены: имя статуса из StatusEffectFactory ("Burn", "Frost", ...). null — арена без эффекта.
    // Вешается на всех живых в начале каждого раунда.
    public string? EffectName { get; set; }
    public int EffectPower { get; set; }
    public int EffectDuration { get; set; } = 1;
}
