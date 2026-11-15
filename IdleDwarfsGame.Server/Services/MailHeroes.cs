using IdleDwarfsGame.Server.Enums;

// Дворфы из писем. Основа — HeroGenerator с seed вложения (то же, что выпало бы из сундука), поверх — заданное
// в конструкторе админки (MailHeroSpec). Один и тот же seed всегда даёт того же дворфа: превью в письме и в админке
// совпадает с выданным
public static class MailHeroes
{
    public const int MaxNameLength = 32;

    public static Hero Create(Guid playerId, int rank, string element, int seed, MailHeroSpec? spec)
    {
        var hero = HeroGenerator.Create(playerId, rank, element, new Random(seed));
        if (spec == null)
            return hero;

        if (!string.IsNullOrWhiteSpace(spec.Name))
            hero.Name = spec.Name.Trim();

        if (spec.Profession is { } profession)
        {
            var stats = ProfessionBook.Of(profession);
            hero.Profession = profession;
            hero.MaxHp = HeroGenerator.ScaleToRank(stats.MaxHp, hero.Level);
            hero.Attack = HeroGenerator.ScaleToRank(stats.Attack, hero.Level);
            hero.Speed = stats.Speed;
        }

        if (spec.CustomAbilities)
        {
            hero.SpellIds = spec.SpellFamily is { } family ? [SpellRegistry.IdOf(family, spec.SpellLevel)] : [];

            hero.Passives = spec.Passive is { } passive ? [passive] : [];
            hero.PassiveLevel = spec.Passive != null ? spec.PassiveLevel : PassiveBook.MinLevel;

            if (AuraBook.Find(spec.Aura) is { } aura)
                AuraBook.Apply(hero, aura, spec.AuraLevel);
            else
            {
                hero.AuraEffect = null;
                hero.AuraPower = 0;
                hero.AuraLevel = AuraBook.MinLevel;
            }
        }

        // Иммунитет у дворфа из конструктора никогда не случайный: только тот, что положил админ
        hero.Immunities = spec.Immunities.Distinct().ToList();
        return hero;
    }

    // Проверка дворфа из конструктора. element — стихия вложения (пусто — случайная)
    public static void Validate(MailHeroSpec spec, int rank, string element)
    {
        if (spec.Name?.Trim().Length > MaxNameLength)
            throw new MailException($"Hero name is longer than {MaxNameLength}");
        if (spec.Profession is { } profession && !Enum.IsDefined(profession))
            throw new MailException($"Unknown profession {profession}");

        var unknown = spec.Immunities.FirstOrDefault(i => !HeroGenerator.Immunities.Contains(i));
        if (unknown != null)
            throw new MailException($"Unknown immunity {unknown}");

        if (!spec.CustomAbilities)
            return;

        if (spec.SpellFamily is { } family)
        {
            // Заклинание — только своей стихии, как и при вставке способности (AbilityService)
            if (!Enum.TryParse<HeroElement>(element, out var heroElement) || rank <= HeroLevels.Min
                || !ElementSpellBook.FamiliesOf(heroElement).Contains(family))
                throw new MailException("The spell must belong to the dwarf's element (choose an element first)");
            if (spec.SpellLevel is < 1 or > AbilityService.MaxLevel || !SpellRegistry.All.ContainsKey(SpellRegistry.IdOf(family, spec.SpellLevel)))
                throw new MailException($"Spell level must be 1..{AbilityService.MaxLevel}");
        }

        if (spec.Passive is { } passive && (!Enum.IsDefined(passive) || spec.PassiveLevel is < PassiveBook.MinLevel or > AbilityService.MaxLevel))
            throw new MailException($"Unknown passive {passive} or level {spec.PassiveLevel}");

        if (spec.Aura != null && (AuraBook.Find(spec.Aura) == null || spec.AuraLevel is < AuraBook.MinLevel or > AuraBook.MaxLevel))
            throw new MailException($"Unknown aura {spec.Aura} or level {spec.AuraLevel}");
    }
}
