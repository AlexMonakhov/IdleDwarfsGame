// Враги бесконечной башни: на каждую стихию четыре моба и босс. В БД их нет — этажи собираются в коде (InfinityTower).
// Статы задаёт роль, поэтому все стихии одинаково сложны и отличаются заклинаниями, аурами и ареной.
// Статы — первого уровня врагов, как у шаблонов кампании (CampaignGenerator): дальше их растит уровень врагов
// этажа (InfinityTower.EnemyLevelFor), а уровень заклинаний — SpellLevelFor
public static class TowerSeedData
{
    // Кто есть кто в отряде врагов этажа (InfinityTower.Lineups)
    public enum Role { Guard, Striker, Caster, Mystic, Boss }

    private record RoleStats(int MaxHp, int Attack, int Speed, int StartEnergy, int EnergyPerAttack);

    private static readonly Dictionary<Role, RoleStats> Stats = new()
    {
        // Передняя линия: толстый и медленный, иммунен к статусу своей стихии (танк кампании)
        [Role.Guard] = new(1000, 70, 3, 0, 20),
        // Быстрый и хрупкий: бьёт первым
        [Role.Striker] = new(520, 85, 9, 0, 20),
        // Заклинание стихии
        [Role.Caster] = new(500, 80, 6, 20, 25),
        // Аура стихии 1-го уровня: статус на весь отряд игрока (у сияния — лечение своих)
        [Role.Mystic] = new(640, 75, 5, 0, 20),
        // Своё заклинание стихии на уровень выше, аура 2-го уровня, иммунитет к оглушению.
        // HP ещё умножается (InfinityTower.BossHpMultiplier)
        [Role.Boss] = new(1400, 120, 5, 20, 25)
    };

    private const int MysticAuraLevel = 1;
    private const int BossAuraLevel = 2;

    private record Monster(string Name, string UnitType);

    // Аура мистика и босса — аура стихии (AuraBook): у сияния — регенерация своих, у остальных — дебафф врагов,
    // к нему иммунен страж. CasterSpell / BossSpell — семейство заклинания кастера и босса (Id первого уровня, SpellRegistry).
    // Заклинание у героя одно (кд нет — каст, как только хватает энергии), у босса оно своё, не как у кастера.
    // Monsters: Guard, Striker, Caster, Mystic, Boss
    private record Theme(int CasterSpell, int BossSpell, Monster[] Monsters);

    private static readonly Dictionary<HeroElement, Theme> Themes = new()
    {
        [HeroElement.Fire] = new(1, 13, // Fireball / Meteor
        [
            new("Cinder Orc", "Orc"), new("Ember Goblin", "Goblin"), new("Flame Shaman", "Goblin"),
            new("Ash Skeleton", "Skeleton"), new("Pyrrhax the Furnace Lord", "Ogre")
        ]),
        [HeroElement.Frost] = new(16, 22, // Frost Bolt / Glacial Prison
        [
            new("Glacier Troll", "Troll"), new("Frostfang Goblin", "Goblin"), new("Rime Mage", "Skeleton"),
            new("Snow Yeti", "Yeti"), new("Skadra the Winter Queen", "Yeti")
        ]),
        [HeroElement.Nature] = new(31, 34, // Venom Thorns / Entangling Roots
        [
            new("Moss Troll", "Troll"), new("Thorn Goblin", "Goblin"), new("Grove Shaman", "Goblin"),
            new("Spore Golem", "Golem"), new("Old Rootheart", "Troll")
        ]),
        [HeroElement.Lightning] = new(4, 28, // Lightning / Thunderclap
        [
            new("Storm Ogre", "Ogre"), new("Spark Goblin", "Goblin"), new("Thunder Shaman", "Goblin"),
            new("Static Golem", "Golem"), new("Voltrak the Stormbringer", "Ogre")
        ]),
        [HeroElement.Dark] = new(37, 43, // Shadow Bolt / Hex
        [
            new("Grave Orc", "Orc"), new("Shade Goblin", "Goblin"), new("Bone Warlock", "Skeleton"),
            new("Hex Skeleton", "Skeleton"), new("Morthul the Hollow King", "Skeleton")
        ]),
        [HeroElement.Radiance] = new(46, 52, // Prism Ray / Dawnlight
        [
            new("Crystal Golem", "Golem"), new("Prism Goblin", "Goblin"), new("Lightbound Mage", "Skeleton"),
            new("Shard Golem", "Golem"), new("Aurelion the Prism Colossus", "Golem")
        ]),
        [HeroElement.Blood] = new(40, 61, // Life Drain / Blood Price
        [
            new("Gore Orc", "Orc"), new("Bloodfang Goblin", "Goblin"), new("Hemomancer", "Skeleton"),
            new("Flayed Troll", "Troll"), new("Vharga the Crimson Maw", "Ogre")
        ])
    };

    public static IReadOnlyCollection<HeroElement> Elements => Themes.Keys;

    // Шаблон врага: spellLevel — уровень заклинаний на этом этаже (у босса на один выше)
    public static Hero Create(HeroElement element, Role role, int spellLevel)
    {
        var theme = Themes[element];
        var monster = theme.Monsters[(int)role];
        var stats = Stats[role];
        var aura = AuraBook.Find(AuraBook.EffectFor(element))!;

        var hero = new Hero
        {
            // Стабильный Id: один и тот же враг на любом этаже и в любую неделю
            Id = Guid.Parse($"55555555-0000-4000-8000-{(int)element:D6}{(int)role:D6}"),
            PlayerId = Guid.Empty,
            Name = monster.Name, UnitType = monster.UnitType, Element = element, Level = HeroLevels.Available,
            MaxHp = stats.MaxHp, Attack = stats.Attack, Speed = stats.Speed,
            MaxEnergy = 100, StartEnergy = stats.StartEnergy, EnergyPerAttack = stats.EnergyPerAttack
        };

        // Регенерация — не дебафф, иммунитет к ней бессмыслен
        List<string> immunity = aura.Target == AuraTarget.Allies ? [] : [aura.Effect];

        switch (role)
        {
            case Role.Guard:
                hero.Immunities = immunity;
                break;
            case Role.Caster:
                hero.SpellIds = [SpellRegistry.IdOf(theme.CasterSpell, spellLevel)];
                break;
            case Role.Mystic:
                AuraBook.Apply(hero, aura, MysticAuraLevel);
                break;
            case Role.Boss:
                hero.SpellIds = [SpellRegistry.IdOf(theme.BossSpell, spellLevel + 1)];
                hero.Immunities = [.. immunity, "Stun"];
                AuraBook.Apply(hero, aura, BossAuraLevel);
                break;
        }
        return hero;
    }
}
