// Боевая мощь: одно число, по которому до боя видно, кто сильнее.
//
// Считается по ECS-сущности, т.е. ровно по тем значениям, с которыми юнит выйдет в бой:
// масштабирование уровнем, множитель HP босса (а позже и предметы) уже применены фабрикой.
//
// Модель: юнит наносит Offense урона за раунд, лечит Healing за раунд и выдерживает Hp урона.
// Мощь = Hp + (Offense + Healing) × FightRounds — сколько юнит выдержит и сколько сделает за типичный бой.
// Она аддитивна: мощь отряда — сумма мощи героев, а +N атаки всегда даёт одинаковую прибавку.
// Ориентир по симуляции: при мощи отряда не ниже мощи врагов бой обычно выигран, в пределах ±10% — риск.
//
// Точный шанс победы даёт Forecast: он прогоняет настоящий CombatEngine на фиксированных сидах.
public static class CombatPower
{
    // Типичная длина боя в раундах: вес урона относительно HP, окно для лечения и стартовой энергии
    public const int FightRounds = 8;

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
        ["Burn"] = 0.04,
        ["Frost"] = 0.04
    };

    public static CombatPowerBreakdown Of(World world, int entityId)
    {
        // Слои разбивки получаются отключением частей профиля; мощь каждого слоя — прибавка к предыдущему.
        // Предметы появятся отдельным слоем поверх пассивок
        var full = Profile(world, entityId);
        var noPassives = full with { Immunities = [], Aura = null };
        var noSkills = noPassives with { Spells = [] };

        double levelFactor = BalanceCalculator.Factor(world.Info.GetValueOrDefault(entityId)?.StatLevel ?? 1);
        var baseStats = noSkills with { MaxHp = noSkills.MaxHp / levelFactor, Attack = noSkills.Attack / levelFactor };

        int stats = Power(baseStats);
        int withLevel = Power(noSkills);
        int withSkills = Power(noPassives);
        int total = Power(full);

        return new CombatPowerBreakdown
        {
            Total = total,
            Stats = stats,
            Level = withLevel - stats,
            Skills = withSkills - withLevel,
            Passives = total - withSkills
        };
    }

    // Мощь героя вне боя (инвентарь): собираем его той же фабрикой, что и для боя
    public static CombatPowerBreakdown OfHero(IEntityFactory factory, Hero hero)
    {
        var world = new World();
        return Of(world, factory.CreateUnit(world, hero, teamId: 1, position: 1));
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
        return (int)Math.Round(hp + (offense + healing * HealingEfficiency) * FightRounds);
    }

    private static (double Offense, double Healing, double Hp) Evaluate(UnitProfile unit)
    {
        double offense = unit.Attack;
        double healing = 0;

        // Скилы: часть ходов юнит кастует вместо базовой атаки (SpellSystem выбирает случайный доступный)
        var spells = unit.Spells.Where(s => s.EnergyCost <= unit.MaxEnergy).ToList();
        if (spells.Count > 0)
        {
            double castRate = CastRate(unit, spells.Average(s => s.EnergyCost));
            var casts = spells.Select(s => s.Estimate((int)Math.Round(unit.Attack), ReferenceTargets)).ToList();

            offense = (1 - castRate) * unit.Attack + castRate * casts.Average(c => Damage(c, unit.Attack));
            healing += castRate * casts.Average(c => c.Healing);
        }

        if (unit.Aura is { } aura)
        {
            var perRound = AuraPerRound(aura);
            offense += Damage(perRound, unit.Attack);
            healing += perRound.Healing;
        }

        offense *= 1 + InitiativePerSpeed * unit.Speed;

        double immunityBonus = unit.Immunities.Sum(name => ImmunityValue.GetValueOrDefault(name));
        return (offense, healing, unit.MaxHp * (1 + immunityBonus));
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

    // Аура срабатывает раз в раунд, а каждый наложенный эффект тикает Duration ходов —
    // в среднем это Power × Duration на цель за раунд
    private static SpellEstimate AuraPerRound(AuraComponent aura)
    {
        // Аура на всех задевает и своих — в среднем бою это ноль пользы
        int hostile = aura.TargetType == AuraTarget.Enemies ? ReferenceTargets : 0;
        int friendly = aura.TargetType == AuraTarget.Allies ? ReferenceTargets : 0;
        int ticks = aura.Power * aura.Duration;

        return aura.EffectName switch
        {
            "Burn" or "Frost" => new SpellEstimate(Damage: hostile * ticks),
            "Regen" => new SpellEstimate(Healing: friendly * ticks),
            // Больше одного хода за раунд стан не отнимет
            "Stun" => new SpellEstimate(StunnedTurns: hostile),
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
            EnergyGain: energy?.GainPerAttack ?? 0,
            Spells: spells ?? [],
            Immunities: world.Immunities.GetValueOrDefault(id)?.ImmuneTo ?? [],
            Aura: world.Auras.GetValueOrDefault(id));
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
        AuraComponent? Aura);
}

// Мощь и её разбивка по источникам: Total = Stats + Level + Skills + Passives
public class CombatPowerBreakdown
{
    public int Total { get; set; }
    // Базовые HP / атака / скорость
    public int Stats { get; set; }
    // Прибавка от масштабирования статов уровнем
    public int Level { get; set; }
    // Заклинания (с учётом того, как часто на них хватает энергии)
    public int Skills { get; set; }
    // Иммунитеты и аура
    public int Passives { get; set; }
}

public class BattleForecast
{
    public int PlayerPower { get; set; }
    public int EnemyPower { get; set; }
    // Доля побед в CombatPower.ForecastFights прогонах настоящего боя (0..1)
    public double WinChance { get; set; }
}
