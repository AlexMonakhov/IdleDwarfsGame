using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Тестовые аккаунты TestUser1..TestUser{Count} — соперники для PvP на локальной базе. Добавляются при старте сервера
// в Development, если их ещё нет (Program.cs). Под ними можно войти: пароль Password, почта testuser{n}@test.local.
// Отряд — 5 случайных героев 1–5 ранга со случайными статами и ровно одним заклинанием случайного уровня 1–3,
// он же закреплён на рейтинговой арене. Сид фиксированный: на чистой базе аккаунты получаются одинаковыми
public static class TestAccountsSeed
{
    public const int Count = 100;
    public const string UsernamePrefix = "TestUser";
    public const string Password = "test1234";
    private const int Seed = 20261028;

    // Разброс статов вокруг роли стартового отряда, усиленной рангом: так мощь отрядов разная даже при одинаковых рангах
    private const double MinStatJitter = 0.8;
    private const double MaxStatJitter = 1.25;

    public static async Task SeedAsync(GameDbContext context, IPasswordHasher<User> hasher)
    {
        var normalizedFirst = UserRepository.Normalize(UsernamePrefix + 1);
        if (!await context.Users.AnyAsync(u => u.NormalizedUsername == normalizedFirst))
            await SeedAccountsAsync(context, hasher);

        // Отдельным шагом: на базах, где аккаунты появились до рейтинговой арены, отрядов арены ещё нет
        await SeedRankedSquadsAsync(context);
    }

    // Каждый тестовый аккаунт закрепляет на рейтинговой арене свой текущий отряд
    private static async Task SeedRankedSquadsAsync(GameDbContext context)
    {
        var users = await context.Users
            .Where(u => u.Username.StartsWith(UsernamePrefix) && !context.RankedSquads.Any(s => s.UserId == u.Id))
            .Select(u => new { u.Id, u.PlayerId })
            .ToListAsync();
        if (users.Count == 0)
            return;

        var playerIds = users.Select(u => u.PlayerId).ToList();
        var squads = (await context.Heroes
                .AsNoTracking()
                .Where(h => h.PlaceInSquad != null && playerIds.Contains(h.PlayerId))
                .ToListAsync())
            .GroupBy(h => h.PlayerId)
            .ToDictionary(g => g.Key, g => g.OrderBy(h => h.PlaceInSquad).ToList());

        var now = DateTime.UtcNow;
        foreach (var user in users.Where(u => squads.ContainsKey(u.PlayerId)))
            context.RankedSquads.Add(new RankedSquad { UserId = user.Id, LockedAt = now, HeroesJson = RankedSquad.Serialize(squads[user.PlayerId]) });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private static async Task SeedAccountsAsync(GameDbContext context, IPasswordHasher<User> hasher)
    {
        var random = new Random(Seed);
        var now = DateTime.UtcNow;

        for (int n = 1; n <= Count; n++)
        {
            var username = UsernamePrefix + n;
            var email = $"testuser{n}@test.local";
            var normalizedUsername = UserRepository.Normalize(username);
            var normalizedEmail = UserRepository.Normalize(email);
            // Имя или почту мог занять настоящий игрок — такой номер пропускаем
            if (await context.Users.AnyAsync(u => u.NormalizedUsername == normalizedUsername || u.NormalizedEmail == normalizedEmail))
                continue;

            var player = new Player { Id = Guid.NewGuid(), Name = username, Rating = 0 };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                NormalizedUsername = normalizedUsername,
                Email = email,
                NormalizedEmail = normalizedEmail,
                CreatedAt = now,
                PlayerId = player.Id,
                Player = player
            };
            user.PasswordHash = hasher.HashPassword(user, Password);

            context.Players.Add(player);
            context.Users.Add(user);
            context.Heroes.AddRange(Enumerable.Range(1, StarterSquad.Size).Select(place => RandomHero(player.Id, place, random)));
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private static Hero RandomHero(Guid playerId, int place, Random random)
    {
        int rank = random.Next(HeroLevels.Min, HeroLevels.Max + 1);
        var hero = HeroGenerator.Create(playerId, rank, string.Empty, random);

        double hpJitter = MinStatJitter + random.NextDouble() * (MaxStatJitter - MinStatJitter);
        double attackJitter = MinStatJitter + random.NextDouble() * (MaxStatJitter - MinStatJitter);
        hero.MaxHp = (int)Math.Round(hero.MaxHp * hpJitter);
        hero.Attack = Math.Max(1, (int)Math.Round(hero.Attack * attackJitter));
        hero.Speed = Math.Max(1, hero.Speed + random.Next(-1, 2));
        hero.PlaceInSquad = place;
        hero.SpellIds = [RandomSpell(hero.Element, random)];
        return hero;
    }

    // Одно заклинание: семейство из книги стихии героя (у героя без стихии — любое), уровень 1–3 случайный
    private static int RandomSpell(HeroElement element, Random random)
    {
        // На 5-м ранге книга отдаёт все три семейства на 3-м уровне: Id первого уровня — на два меньше
        var families = ElementSpellBook.SpellsFor(element, HeroLevels.Max).Select(id => id - 2).ToList();
        if (families.Count == 0)
            families = SpellRegistry.All.Values.Where(s => s.Level == 1).Select(s => s.Id).ToList();

        return SpellRegistry.IdOf(families[random.Next(families.Count)], random.Next(1, 4));
    }
}
