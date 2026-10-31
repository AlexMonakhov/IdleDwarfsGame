public class EntityFactory : IEntityFactory
{
    public int CreateUnit(World world, Hero hero, int teamId, int position, int level = 1)
    {
        var entity = world.CreateEntity();
        var gear = EquipmentCatalog.CollectGear(hero);
        // Сначала бонус прокачки героя, поверх него — масштабирование уровнем боя, сверху — предметы
        int maxHp = BalanceCalculator.ScaleStat(HeroUpgradeService.ScaleStat(hero.MaxHp, hero.UpgradeLevel), level) + gear.Stats.MaxHp;

        world.Info[entity.Id] = new UnitInfoComponent
        {
            HeroId = hero.Id,
            Name = hero.Name,
            UnitType = hero.UnitType,
            Element = hero.Element,
            Level = hero.Level,
            StatLevel = Math.Max(1, level),
            UpgradeLevel = HeroUpgradeService.ClampLevel(hero.UpgradeLevel),
            Position = position
        };

        world.Health[entity.Id] = new HealthComponent
        {
            Max = maxHp,
            Current = maxHp
        };

        world.Attack[entity.Id] = new AttackComponent
        {
            Damage = BalanceCalculator.ScaleStat(HeroUpgradeService.ScaleStat(hero.Attack, hero.UpgradeLevel), level) + gear.Stats.Attack
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

        gear.BaseAura = BaseAura(hero, level);
        if (WithGear(gear.BaseAura, gear) is { } aura)
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

    // Итоговая аура героя с учётом предметов: у самого героя её может и не быть, тогда её даёт шлем
    public static AuraComponent? ResolveAura(Hero hero, GearComponent gear, int level = 1) =>
        WithGear(BaseAura(hero, level), gear);

    // Пассивки героя: уровень по рангу, сверху — прибавка колец
    public static PassiveComponent CreatePassives(Hero hero, GearComponent gear)
    {
        var component = new PassiveComponent();
        int level = PassiveBook.LevelFor(hero.Level);
        foreach (var passive in hero.Passives.Distinct())
        {
            var values = PassiveBook.Values(passive, level);
            component.Base[passive] = values;
            component.Active[passive] = values + gear.PassiveBonuses.GetValueOrDefault(passive);
        }
        return component;
    }

    private static AuraComponent? BaseAura(Hero hero, int level) =>
        string.IsNullOrWhiteSpace(hero.AuraEffect)
            ? null
            : new AuraComponent
            {
                EffectName = hero.AuraEffect,
                TargetType = hero.AuraTarget,
                Trigger = hero.AuraTrigger,
                Power = BalanceCalculator.ScaleStat(hero.AuraPower, level),
                Duration = Math.Max(1, hero.AuraDuration),
                Interval = Math.Max(1, hero.AuraInterval)
            };

    private static AuraComponent? WithGear(AuraComponent? aura, GearComponent gear)
    {
        if (aura == null && gear.AuraGrant is { } grant)
        {
            aura = new AuraComponent
            {
                EffectName = grant.Effect,
                TargetType = grant.Target,
                Trigger = TriggerPhase.OnRoundStart,
                Power = grant.Power,
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
