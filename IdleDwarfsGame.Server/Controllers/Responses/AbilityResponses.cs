// Способность: извлечённая (Id есть) или на дворфе (Id null) — её можно извлечь, распылив дворфа
public class AbilityInfo
{
    public Guid? Id { get; set; }
    public AbilityKind Kind { get; set; }
    // Spell — Id первого уровня семейства, Passive — HeroPassive, Aura — эффект (AuraBook)
    public string Key { get; set; } = string.Empty;
    public int Level { get; set; }
    public int MaxLevel { get; set; } = AbilityService.MaxLevel;

    // Заклинание: Id спелла этого уровня, название и стихия, в книге которой оно лежит
    public int? SpellId { get; set; }
    public string? SpellName { get; set; }
    public HeroElement? Element { get; set; }

    // Пассивка: значения на этом уровне, без колец
    public int Percent { get; set; }
    public int Turns { get; set; }
    public int Targets { get; set; }

    // Аура: на кого действует, сила за ход и сколько ходов держится
    public AuraTarget? AuraTarget { get; set; }
    public int Power { get; set; }
    public int Duration { get; set; }

    // null — способность не из книг (неизвестный спелл, пассивка или аура): её не показать и не извлечь
    public static AbilityInfo? From(PlayerAbility ability) => Describe(ability.Kind, ability.Key, ability.Level, ability.Id);

    public static AbilityInfo? Describe(AbilityKind kind, string key, int level, Guid? id = null)
    {
        level = Math.Clamp(level, 1, AbilityService.MaxLevel);
        var info = new AbilityInfo { Id = id, Kind = kind, Key = key, Level = level };

        switch (kind)
        {
            case AbilityKind.Spell:
                if (!int.TryParse(key, out int family) || !SpellRegistry.All.TryGetValue(SpellRegistry.IdOf(family, level), out var spell)
                    || ElementSpellBook.FamilyOf(spell.Id) != family)
                    return null;
                info.SpellId = spell.Id;
                info.SpellName = spell.Name;
                info.Element = ElementSpellBook.ElementOf(family);
                return info;

            case AbilityKind.Passive:
                if (!Enum.TryParse<HeroPassive>(key, out var passive) || !Enum.IsDefined(passive))
                    return null;
                var values = PassiveBook.Values(passive, level);
                info.Percent = values.Percent;
                info.Turns = values.Turns;
                info.Targets = values.Targets;
                return info;

            case AbilityKind.Aura:
                if (AuraBook.Find(key) is not { } aura)
                    return null;
                info.AuraTarget = aura.Target;
                info.Power = AuraBook.Power(aura, level);
                info.Duration = aura.Duration;
                return info;

            default:
                return null;
        }
    }

    // Что можно извлечь из дворфа: его заклинания, пассивки и аура (аура шлема — не его). Иммунитеты не извлекаются
    public static List<AbilityInfo> Of(Hero hero)
    {
        var spells = hero.SpellIds
            .Select(id => ElementSpellBook.FamilyOf(id) is { } family
                ? Describe(AbilityKind.Spell, family.ToString(), SpellRegistry.All[id].Level)
                : null);
        var passives = hero.Passives
            .Distinct()
            .Select(p => Describe(AbilityKind.Passive, p.ToString(), hero.PassiveLevel));
        var aura = Describe(AbilityKind.Aura, hero.AuraEffect ?? string.Empty, hero.AuraLevel);

        return spells.Concat(passives).Append(aura).OfType<AbilityInfo>().ToList();
    }
}

// Ответ на действие со способностями: все извлечённые способности игрока и затронутый дворф —
// получивший способность или новый, полученный слиянием
public class AbilityActionResult
{
    public List<AbilityInfo> Abilities { get; set; } = new();
    public HeroInfo? Hero { get; set; }
}
