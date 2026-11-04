public class EntityFactory : IEntityFactory
{
    public int CreateUnit(World world, Hero hero, int teamId, int position, int level = 1)
    {
        var entity = world.CreateEntity();
        var gear = EquipmentCatalog.CollectGear(hero);
        int upgradeLevel = HeroUpgradeService.ClampLevel(hero.Level, hero.UpgradeLevel);
        // Сначала бонус прокачки героя, поверх него — масштабирование уровнем боя, сверху — предметы
        int maxHp = BalanceCalculator.ScaleStat(HeroUpgradeService.ScaleStat(hero.MaxHp, upgradeLevel), level) + gear.Stats.MaxHp;

        world.Info[entity.Id] = new UnitInfoComponent
        {
            HeroId = hero.Id,
            Name = hero.Name,
            UnitType = hero.UnitType,
            Element = hero.Element,
            Level = hero.Level,
            StatLevel = Math.Max(1, level),
            UpgradeLevel = upgradeLevel,
            Profession = hero.Profession,
            Position = position
        };

        world.Health[entity.Id] = new HealthComponent
        {
            Max = maxHp,
            Current = maxHp
        };

        world.Attack[entity.Id] = new AttackComponent
        {
            Damage = BalanceCalculator.ScaleStat(HeroUpgradeService.ScaleStat(hero.Attack, upgradeLevel), level) + gear.Stats.Attack
        };

        world.Speed[entity.Id] = new SpeedComponent
        {
            Value = hero.Speed + gear.Stats.Speed
        };

        world.Team[entity.Id] = new TeamComponent
        {
            TeamId = teamId
        };

        world.Energy[entity.Id] = new EnergyComponent
        {
            Max = hero.MaxEnergy + gear.Stats.MaxEnergy,
            Current = Math.Min(hero.StartEnergy + gear.Stats.StartEnergy, hero.MaxEnergy + gear.Stats.MaxEnergy),
            GainPerAttack = hero.EnergyPerAttack + gear.Stats.EnergyPerAttack
        };

        // Неизвестные Id спеллов отбрасываем, чтобы кривые данные в БД не ломали бой
        world.Spells[entity.Id] = new SpellComponent
        {
            SpellIds = hero.SpellIds.Where(SpellRegistry.All.ContainsKey).ToList()
        };

        if (hero.Immunities.Count > 0)
        {
            world.Immunities[entity.Id] = new ImmunityComponent
            {
                ImmuneTo = hero.Immunities.ToList()
            };
        }

        // Сила ауры — доля итоговой атаки юнита (с уровнем боя и предметами)
        int attack = world.Attack[entity.Id].Damage;
        gear.BaseAura = BaseAura(hero, attack);
        if (WithGear(gear.BaseAura, gear, attack) is { } aura)
            world.Auras[entity.Id] = aura;

        if (hero.Passives.Count > 0)
            world.Passives[entity.Id] = CreatePassives(hero, gear);

        if (hero.Equipment.Count > 0)
            world.Gear[entity.Id] = gear;

        world.StatusEffects[entity.Id] = new StatusEffectComponent();
        world.TurnState[entity.Id] = new TurnStateComponent
        {
            CanAct = true,
            ActionCompleted = false
        };

        return entity.Id;
    }

    // Итоговая аура героя с атакой attack с учётом предметов: у самого героя её может и не быть, тогда её даёт шлем
    public static AuraComponent? ResolveAura(Hero hero, GearComponent gear, int attack) =>
        WithGear(BaseAura(hero, attack), gear, attack);

    // Пассивки героя на их уровне, сверху — прибавка колец
    public static PassiveComponent CreatePassives(Hero hero, GearComponent gear)
    {
        var component = new PassiveComponent();
        int level = hero.PassiveLevel;
        foreach (var passive in hero.Passives.Distinct())
        {
            var values = PassiveBook.Values(passive, level);
            component.Base[passive] = values;
            component.Active[passive] = values + gear.PassiveBonuses.GetValueOrDefault(passive);
        }
        return component;
    }

    // AuraPower героя — процент (AuraBook): урон или лечение за ход — доля атаки, у мороза — процент замедления
    private static AuraComponent? BaseAura(Hero hero, int attack) =>
        string.IsNullOrWhiteSpace(hero.AuraEffect)
            ? null
            : new AuraComponent
            {
                EffectName = hero.AuraEffect,
                TargetType = hero.AuraTarget,
                Trigger = hero.AuraTrigger,
                Power = AuraBook.Resolve(hero.AuraEffect, hero.AuraPower, attack),
                Duration = Math.Max(1, hero.AuraDuration),
                Interval = Math.Max(1, hero.AuraInterval)
            };

    private static AuraComponent? WithGear(AuraComponent? aura, GearComponent gear, int attack)
    {
        if (aura == null && gear.AuraGrant is { } grant)
        {
            aura = new AuraComponent
            {
                EffectName = grant.Effect,
                TargetType = grant.Target,
                Trigger = TriggerPhase.OnRoundStart,
                Power = AuraBook.Resolve(grant.Effect, grant.Power, attack),
                Duration = grant.Duration,
                Interval = grant.Interval
            };
        }
        if (aura == null) return null;

        return new AuraComponent
        {
            EffectName = aura.EffectName,
            TargetType = aura.TargetType,
            Trigger = aura.Trigger,
            // С округлением: у слабых аур отбрасывание дроби съело бы весь бонус (3 × 120% = 3)
            Power = (int)Math.Round(aura.Power * (100 + gear.AuraPowerPercent) / 100.0, MidpointRounding.AwayFromZero),
            Duration = aura.Duration + gear.AuraDurationBonus,
            Interval = Math.Max(1, aura.Interval - gear.AuraIntervalReduction)
        };
    }

    public List<int> CreateSquad(World world, IEnumerable<Hero> heroes, int teamId, int level = 1)
    {
        return heroes
            .Select((hero, index) => CreateUnit(world, hero, teamId, hero.PlaceInSquad ?? index + 1, level))
            .ToList();
    }
}
