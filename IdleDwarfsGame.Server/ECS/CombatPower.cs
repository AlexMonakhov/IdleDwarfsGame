// Боевая мощь: одно число, по которому до боя видно, кто сильнее.
//
// Считается по ECS-сущности, т.е. ровно по тем значениям, с которыми юнит выйдет в бой:
// масштабирование уровнем, множитель HP босса (а позже и предметы) уже применены фабрикой.
//
// Модель: юнит наносит Offense урона за раунд, лечит Healing за раунд и выдерживает Hp урона.
// Мощь = (Hp + (Offense + Healing) × FightRounds) / Scale — сколько юнит выдержит и сколько сделает за типичный бой.
// Она аддитивна: мощь отряда — сумма мощи героев, а +N атаки всегда даёт одинаковую прибавку.
// Scale только сжимает число для показа: стартовый отряд ~850, дворфы 4-го ранга 100-го уровня ~89 тыс.
// Ориентир по симуляции: при мощи отряда не ниже мощи врагов бой обычно выигран, в пределах ±10% — риск.
//
// Точный шанс победы даёт Forecast: он прогоняет настоящий CombatEngine на фиксированных сидах.
public static class CombatPower
{
    // Типичная длина боя в раундах: вес урона относительно HP, окно для лечения и стартовой энергии
    public const int FightRounds = 8;

    // Во сколько раз мощь меньше «сырой» оценки (HP + урон за бой). На неё же рассчитана награда экспедиций
    // (ExpeditionService.GoldPerPowerPerHour): поменяв масштаб, поправь и её
    public const double Scale = 10;

    // «Средний» бой для мощи вне конкретного боя: сколько целей у массовых спеллов и аур
    public const int ReferenceTargets = 3;

    // Сколько боёв прогоняет прогноз. Сиды фиксированы, поэтому прогноз одинаков при каждом запросе
    public const int ForecastFights = 50;

    // Каждое очко скорости — шанс ударить раньше и успеть до смерти
    private const double InitiativePerSpeed = 0.02;

    // Часть лечения уходит в оверхил
    private const double HealingEfficiency = 0.6;

    // Ход врага, пропущенный из-за стана, стоит примерно половины атаки кастера (часть станов — в иммунитет)
    private const double StunnedTurnValue = 0.5;

    // Насколько иммунитет повышает живучесть (доля от HP)
    private static readonly Dictionary<string, double> ImmunityValue = new()
    {
        ["Stun"] = 0.08,
        ["Freeze"] = 0.06,
        ["Burn"] = 0.04,
        ["Frost"] = 0.04,
        ["Poison"] = 0.04,
        ["Curse"] = 0.04,
        ["Shock"] = 0.04,
        ["Bleed"] = 0.04
    };

    // «Выживает слабейший» работает не против всех: примерно половина ударов — от тех, кто сильнее героя
    private const double SurvivalUptime = 0.5;

    // Доля урона, который приходится по врагам ниже порога добивания (Executioner)
    private const double ExecuteShare = 0.25;
    // Сколько базовых атак врага в среднем приходится на героя за раунд (Thorns): по передней линии бьют чаще
    private const double HitsTakenPerRound = 0.7;
    // В среднем бою мститель успевает застать гибель одного-двух союзников ближе к концу — половина бонуса на весь бой
    private const double AvengerUptime = 0.5;
    // Доля урона героя, которая идёт через статусы (ElementMastery)
    private const double DotShare = 0.15;

    public static CombatPowerBreakdown Of(World world, int entityId)
    {
        // Слои разбивки получаются отключением частей профиля; мощь каждого слоя — прибавка к предыдущему
        var full = Profile(world, entityId);
        var noGear = WithoutGear(full, world, entityId);
        // Рунный резонанс вшит в прирост энергии — без пассивок его снимаем
        int resonance = noGear.Passives.GetValueOrDefault(HeroPassive.RuneResonance).Percent;
        var noPassives = noGear with { Immunities = [], Aura = null, Passives = NoPassives, EnergyGain = noGear.EnergyGain - resonance };
        var noSkills = noPassives with { Spells = [] };

        // Уровень боя и прокачка героя вместе дают слой Level
        var info = world.Info.GetValueOrDefault(entityId);
        double levelFactor = BalanceCalculator.Factor(info?.StatLevel ?? 1) * HeroUpgradeService.StatFactor(info?.UpgradeLevel ?? 1);
        var baseStats = noSkills with { MaxHp = noSkills.MaxHp / levelFactor, Attack = noSkills.Attack / levelFactor };

        int stats = Power(baseStats);
        int withLevel = Power(noSkills);
        int withSkills = Power(noPassives);
        int withPassives = Power(noGear);
        int total = Power(full);

        return new CombatPowerBreakdown
        {
            Total = total,
            Stats = stats,
            Level = withLevel - stats,
            Skills = withSkills - withLevel,
            Passives = withPassives - withSkills,
            Equipment = total - withPassives
        };
    }

    private static readonly IReadOnlyDictionary<HeroPassive, PassiveValues> NoPassives = new Dictionary<HeroPassive, PassiveValues>();

    // Тот же юнит, но без предметов: их статы вычитаем, ауру и пассивки берём исходные
    private static UnitProfile WithoutGear(UnitProfile unit, World world, int id)
    {
        if (world.Gear.GetValueOrDefault(id) is not { } gear)
            return unit;

        return unit with
        {
            MaxHp = unit.MaxHp - gear.Stats.MaxHp,
            Attack = unit.Attack - gear.Stats.Attack,
            Speed = unit.Speed - gear.Stats.Speed,
            MaxEnergy = unit.MaxEnergy - gear.Stats.MaxEnergy,
            StartEnergy = Math.Max(0, unit.StartEnergy - gear.Stats.StartEnergy),
            // Кольца могли добавить резонанса: возвращаем его к значению без колец
            EnergyGain = unit.EnergyGain - gear.Stats.EnergyPerAttack
                - unit.Passives.GetValueOrDefault(HeroPassive.RuneResonance).Percent
                + (world.Passives.GetValueOrDefault(id)?.Base.GetValueOrDefault(HeroPassive.RuneResonance).Percent ?? 0),
            Aura = gear.BaseAura,
            Passives = world.Passives.GetValueOrDefault(id)?.Base ?? NoPassives,
            Gear = null
        };
    }

    // Мощь героя вне боя (инвентарь): собираем его той же фабрикой, что и для боя
    public static CombatPowerBreakdown OfHero(IEntityFactory factory, Hero hero)
    {
        var world = new World();
        return Of(world, factory.CreateUnit(world, hero, teamId: 1, position: 1));
    }

    // Мощь отряда — сумма мощи героев, собранных той же фабрикой, что и для боя. Боя не проводит: чистая формула
    public static int OfSquad(IEntityFactory factory, IEnumerable<Hero> squad)
    {
        var world = new World();
        return factory.CreateSquad(world, squad, teamId: 1).Sum(id => Of(world, id).Total);
    }

    // Прогноз боя. buildWorld собирает мир так же, как перед настоящим боем (отряд, враги, арена) —
    // каждый прогон получает свежий мир, потому что бой меняет его состояние
    public static BattleForecast Forecast(Func<World> buildWorld, int playerTeam = 1)
    {
        var world = buildWorld();
        var forecast = new BattleForecast
        {
            PlayerPower = TeamPower(world, team => team == playerTeam),
            EnemyPower = TeamPower(world, team => team != playerTeam)
        };

        int wins = Enumerable.Range(0, ForecastFights)
            .Count(seed => new CombatEngine(new Random(seed)).Run(buildWorld()).WinningTeam == playerTeam);
        forecast.WinChance = (double)wins / ForecastFights;

        return forecast;
    }

    private static int TeamPower(World world, Func<int, bool> isTeam) => world.Entities
        .Where(e => world.IsAlive(e.Id) && isTeam(world.Team[e.Id].TeamId))
        .Sum(e => Of(world, e.Id).Total);

    private static int Power(UnitProfile unit)
    {
        var (offense, healing, hp) = Evaluate(unit);
        return (int)Math.Round((hp + (offense + healing * HealingEfficiency) * FightRounds) / Scale);
    }

    private static (double Offense, double Healing, double Hp) Evaluate(UnitProfile unit)
    {
        double basicAttack = BasicAttack(unit);
        double offense = basicAttack;
        double healing = 0;

        // Скилы: часть ходов юнит кастует вместо базовой атаки (SpellSystem выбирает случайный доступный)
        var spells = unit.Spells.Where(s => s.EnergyCost <= unit.MaxEnergy).ToList();
        if (spells.Count > 0)
        {
            double castRate = CastRate(unit, spells.Average(s => s.EnergyCost));
            var casts = spells.Select(s => EstimateCast(s, unit)).ToList();

            double sorcerer = 1 + unit.Passives.GetValueOrDefault(HeroPassive.Sorcerer).Percent / 100.0;
            offense = (1 - castRate) * basicAttack + castRate * casts.Average(c => Damage(c, unit.Attack)) * sorcerer;
            healing += castRate * casts.Average(c => c.Healing);
        }

        if (unit.Aura is { } aura)
        {
            var perRound = AuraPerRound(aura);
            offense += Damage(perRound, unit.Attack);
            healing += perRound.Healing;
        }

        double Passive(HeroPassive passive) => unit.Passives.GetValueOrDefault(passive).Percent / 100.0;

        offense *= 1 + Passive(HeroPassive.Executioner) * ExecuteShare
                     + Passive(HeroPassive.Avenger) * AvengerUptime
                     + Passive(HeroPassive.ElementMastery) * DotShare;
        offense += unit.Attack * Passive(HeroPassive.Thorns) * HitsTakenPerRound;
        healing += offense * Passive(HeroPassive.Vampirism);
        offense *= 1 + InitiativePerSpeed * unit.Speed;

        double immunityBonus = unit.Immunities.Sum(name => ImmunityValue.GetValueOrDefault(name));
        double survivalBonus = Passive(HeroPassive.SurvivalOfTheWeakest) * SurvivalUptime;
        double stoneSkin = Passive(HeroPassive.StoneSkin);
        double hp = unit.MaxHp * (1 + immunityBonus + survivalBonus + Passive(HeroPassive.LastStand)) / Math.Max(0.2, 1 - stoneSkin);
        return (offense, healing, hp);
    }

    // Урон одной базовой атаки с пассивками: крит, раскол, второй удар с шансом и горение от каждого удара
    private static double BasicAttack(UnitProfile unit)
    {
        var doubleStrike = unit.Passives.GetValueOrDefault(HeroPassive.DoubleStrike);
        var tongue = unit.Passives.GetValueOrDefault(HeroPassive.SharpTongue);
        var cleave = unit.Passives.GetValueOrDefault(HeroPassive.Cleave);
        double crit = 1 + unit.Passives.GetValueOrDefault(HeroPassive.CriticalStrike).Percent / 100.0;

        double doubleChance = doubleStrike.Percent / 100.0;
        double cleaveHits = Math.Min(cleave.Targets, ReferenceTargets - 1);
        double hits = 1 + cleaveHits + doubleChance * doubleStrike.Targets;
        double damage = unit.Attack * (1 + cleaveHits * cleave.Percent / 100.0 + doubleChance * doubleStrike.Targets) * crit;
        double burn = hits * tongue.Targets * tongue.Turns * unit.Attack * tongue.Percent / 100;
        return damage + burn;
    }

    // Польза каста с учётом предметов. Estimate на одного врага — это урон по одной цели: из него получаем доп. цели и срабатывания
    private static SpellEstimate EstimateCast(ISpell spell, UnitProfile unit)
    {
        int attack = (int)Math.Round(unit.Attack);
        var estimate = spell.Estimate(attack, ReferenceTargets);
        if (unit.Gear is not { } gear || !gear.Spells.TryGetValue(spell.Name, out var modifier))
            return estimate;

        double perTarget = spell.Estimate(attack, 1).Damage;
        int targets = perTarget > 0 ? (int)Math.Round(estimate.Damage / perTarget) : 0;
        int extra = perTarget > 0 ? Math.Min(modifier.ExtraTargets, Math.Max(0, ReferenceTargets - targets)) : 0;

        double damage = (estimate.Damage + extra * perTarget) * (100 + modifier.DamagePercent) / 100;
        damage += (targets + extra) * modifier.Procs.Sum(p => attack * p.DamagePercent / 100.0 * p.Chance / 100.0);
        return estimate with { Damage = damage };
    }

    private static double Damage(SpellEstimate estimate, double attack) =>
        estimate.Damage + estimate.StunnedTurns * StunnedTurnValue * attack;

    // Цикл «копим энергию базовыми атаками → кастуем»: на каст уходит cost / gain атак (остаток энергии переносится).
    // Стартовая энергия даёт лишние касты в начале боя — размазываем их по длине боя
    private static double CastRate(UnitProfile unit, double cost)
    {
        double regular = unit.EnergyGain > 0 ? 1 / (1 + cost / unit.EnergyGain) : 0;
        double opening = Math.Min(unit.StartEnergy, unit.MaxEnergy) / cost / FightRounds;
        return Math.Min(1, regular + opening);
    }

    // Аура срабатывает раз в Interval раундов. Слоистый статус тикает каждым наложением Duration ходов —
    // в среднем Power × Duration / Interval на цель за раунд. Проклятие живёт столько, сколько задало первое наложение:
    // за цикл из Duration раундов набирает 1, 2, … Duration наложений — в среднем Power × (Duration + 1) / 2.
    // Кровотечение у наложения длительности не имеет — один тик на раунд от одного носителя
    private static SpellEstimate AuraPerRound(AuraComponent aura)
    {
        // Аура на всех задевает и своих — в среднем бою это ноль пользы
        double rate = 1.0 / Math.Max(1, aura.Interval);
        double hostile = (aura.TargetType == AuraTarget.Enemies ? ReferenceTargets : 0) * rate;
        double friendly = (aura.TargetType == AuraTarget.Allies ? ReferenceTargets : 0) * rate;
        double ticks = aura.Power * aura.EffectName switch
        {
            "Curse" => (aura.Duration + 1) / 2.0,
            // У наложения кровотечения длительности нет: одна аура — тик за раунд (с другими аурами крови копится до 5,5×)
            "Bleed" => 1,
            _ => aura.Duration
        };

        return aura.EffectName switch
        {
            "Burn" or "Poison" or "Curse" or "Shock" or "Bleed" => new SpellEstimate(Damage: hostile * ticks),
            "Regen" => new SpellEstimate(Healing: friendly * ticks),
            // Больше одного хода за раунд стан не отнимет
            "Stun" or "Freeze" => new SpellEstimate(StunnedTurns: hostile),
            _ => default
        };
    }

    private static UnitProfile Profile(World world, int id)
    {
        var energy = world.Energy.GetValueOrDefault(id);
        var spells = world.Spells.GetValueOrDefault(id)?.SpellIds
            .Select(SpellRegistry.All.GetValueOrDefault)
            .OfType<ISpell>()
            .ToList();

        return new UnitProfile(
            MaxHp: world.Health[id].Max,
            Attack: world.Attack[id].Damage,
            Speed: world.Speed[id].Value,
            MaxEnergy: energy?.Max ?? 0,
            StartEnergy: energy?.Current ?? 0,
            EnergyGain: (energy?.GainPerAttack ?? 0) + (world.GetPassive(id, HeroPassive.RuneResonance)?.Percent ?? 0),
            Spells: spells ?? [],
            Immunities: world.Immunities.GetValueOrDefault(id)?.ImmuneTo ?? [],
            Aura: world.Auras.GetValueOrDefault(id),
            Passives: world.Passives.GetValueOrDefault(id)?.Active ?? NoPassives,
            Gear: world.Gear.GetValueOrDefault(id));
    }

    // Всё, что влияет на мощь юнита
    private record UnitProfile(
        double MaxHp,
        double Attack,
        int Speed,
        int MaxEnergy,
        int StartEnergy,
        int EnergyGain,
        IReadOnlyList<ISpell> Spells,
        IReadOnlyList<string> Immunities,
        AuraComponent? Aura,
        IReadOnlyDictionary<HeroPassive, PassiveValues> Passives,
        GearComponent? Gear);
}

// Мощь и её разбивка по источникам: Total = Stats + Level + Skills + Passives + Equipment
public class CombatPowerBreakdown
{
    public int Total { get; set; }
    // Базовые HP / атака / скорость
    public int Stats { get; set; }
    // Прибавка от масштабирования статов уровнем боя и прокачкой героя
    public int Level { get; set; }
    // Заклинания (с учётом того, как часто на них хватает энергии)
    public int Skills { get; set; }
    // Иммунитеты, аура и пассивные способности
    public int Passives { get; set; }
    // Надетые предметы: их статы и всё, что они усилили
    public int Equipment { get; set; }
}

public class BattleForecast
{
    public int PlayerPower { get; set; }
    public int EnemyPower { get; set; }
    // Доля побед в CombatPower.ForecastFights прогонах настоящего боя (0..1)
    public double WinChance { get; set; }
}
