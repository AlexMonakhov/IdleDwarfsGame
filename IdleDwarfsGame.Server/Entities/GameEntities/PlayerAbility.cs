// Что за способность: заклинание, пассивка или аура. Иммунитеты не извлекаются
public enum AbilityKind
{
    Spell,
    Passive,
    Aura
}

// Способность, извлечённая из дворфа (AbilityService): лежит у игрока, пока её не вставят другому дворфу
// или не сольют с такой же. Как и предметы, каждая штука — своя строка
public class PlayerAbility
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public AbilityKind Kind { get; set; }

    // Spell — Id первого уровня семейства (SpellRegistry), Passive — название HeroPassive, Aura — эффект из AuraBook
    public string Key { get; set; } = string.Empty;
    // 1..AbilityService.MaxLevel
    public int Level { get; set; } = 1;
}
