public class EntityFactory(IPlayerRepository playerRepository) : IEntityFactory
{
    public void CreateMonster(World world, List<MonsterStats> stats)
    {
        

        foreach(var monster in stats)
        {
            var entity = world.CreateEntity();

            world.Health[entity.Id] = new HealthComponent
            {
                Current = monster.MaxHp,
                Max = monster.MaxHp
            };

            world.Attack[entity.Id] = new AttackComponent
            {
                Damage = monster.Attack
            };

            world.Speed[entity.Id] = new SpeedComponent
            {
                Value = monster.Speed
            };

            world.Team[entity.Id] = new TeamComponent
            {
                TeamId = 2
            };
        }
        
    }

    public async Task AddPlayerSquad(World world, Guid playerId)
    {
        var heroes = await playerRepository.GetHeroes(playerId);

        foreach (var hero in heroes)
        {
            var entity = world.CreateEntity();

            world.Health[entity.Id] = new HealthComponent
            {
                Max = hero.MaxHp,
                Current = hero.MaxHp
            };

            world.Attack[entity.Id] = new AttackComponent
            {
                Damage = hero.Attack
            };

            world.Speed[entity.Id] = new SpeedComponent
            {
                Value = hero.Speed
            };

            world.Team[entity.Id] = new TeamComponent
            {
                TeamId = 1
            };
        }
    }

    public void CreatePlayer(World world, List<Hero> heroes)
    {
        foreach (var hero in heroes)
        {
            var entity = world.CreateEntity();

            world.Health[entity.Id] = new HealthComponent
            {
                Max = hero.MaxHp,
                Current = hero.MaxHp
            };

            world.Attack[entity.Id] = new AttackComponent
            {
                Damage = hero.Attack
            };

            world.Speed[entity.Id] = new SpeedComponent
            {
                Value = hero.Speed
            };

            world.Team[entity.Id] = new TeamComponent
            {
                TeamId = 1
            };
        }
    }

    public void CreateBoss(World world, MonsterStats stats)
    {
        throw new NotImplementedException();
    }
}