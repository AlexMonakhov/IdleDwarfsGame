public class EntityFactory : IEntityFactory
{
    public int CreateUnit(World world, Hero hero, int teamId, int position, int level = 1)
    {
        var entity = world.CreateEntity();
        int maxHp = BalanceCalculator.ScaleStat(hero.MaxHp, level);

        world.Info[entity.Id] = new UnitInfoComponent
        {
            HeroId = hero.Id,
            Name = hero.Name,
            UnitType = hero.UnitType,
            Element = hero.Element,
            Level = hero.Level,
            StatLevel = Math.Max(1, level),
            Position = position
        };

        world.Health[entity.Id] = new HealthComponent
        {
            Max = maxHp,
            Current = maxHp
        };

        world.Attack[entity.Id] = new AttackComponent
        {
            Damage = BalanceCalculator.ScaleStat(hero.Attack, level)
        };

        world.Speed[entity.Id] = new SpeedComponent
        {
            Value = hero.Speed
        };

        world.Team[entity.Id] = new TeamComponent
        {
            TeamId = teamId
        };

        world.Energy[entity.Id] = new EnergyComponent
        {
            Max = hero.MaxEnergy,
            Current = Math.Min(hero.StartEnergy, hero.MaxEnergy),
            GainPerAttack = hero.EnergyPerAttack
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

        if (!string.IsNullOrWhiteSpace(hero.AuraEffect))
        {
            world.Auras[entity.Id] = new AuraComponent
            {
                EffectName = hero.AuraEffect,
                TargetType = hero.AuraTarget,
                Trigger = hero.AuraTrigger,
                Power = BalanceCalculator.ScaleStat(hero.AuraPower, level),
                Duration = Math.Max(1, hero.AuraDuration)
            };
        }

        world.StatusEffects[entity.Id] = new StatusEffectComponent();
        world.TurnState[entity.Id] = new TurnStateComponent
        {
            CanAct = true,
            ActionCompleted = false
        };

        return entity.Id;
    }

    public List<int> CreateSquad(World world, IEnumerable<Hero> heroes, int teamId, int level = 1)
    {
        return heroes
            .Select((hero, index) => CreateUnit(world, hero, teamId, hero.PlaceInSquad ?? index + 1, level))
            .ToList();
    }
}
