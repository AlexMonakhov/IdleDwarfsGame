using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Брокк — наставник из обучения, служебный аккаунт. Каждый новый игрок при регистрации сразу у него в друзьях
// (AuthController), а в отряде Брокка — тот же огненный дворф, что бьётся за новичка в обучающем бою
// (TutorialBattle.Mentor): его можно взять наёмником в кампанию.
// Добавляется при старте сервера (Program.cs), если его ещё нет. Войти под ним нельзя — пароль случайный.
// Заявку Брокку принимать некому, поэтому она сразу делает друзьями (FriendService), а лимит друзей на него не действует.
// В подборе соперников арены его нет (PvpRepository)
public static class MentorAccount
{
    public static readonly Guid UserId = Guid.Parse("7e7e7e7e-0000-4000-8000-000000000101");
    public static readonly Guid PlayerId = Guid.Parse("7e7e7e7e-0000-4000-8000-000000000102");
    public static readonly Guid HeroId = Guid.Parse("7e7e7e7e-0000-4000-8000-000000000103");

    public const string Username = "Brokk";
    private const string Email = "brokk@mentor.local";

    public static bool Is(Guid userId) => userId == UserId;

    public static async Task SeedAsync(GameDbContext context, IPasswordHasher<User> hasher, SquadPowerService squadPower)
    {
        if (await context.Users.AnyAsync(u => u.Id == UserId))
            return;

        var normalizedUsername = UserRepository.Normalize(Username);
        // Имя мог занять настоящий игрок до появления наставника — тогда не трогаем его
        if (await context.Users.AnyAsync(u => u.NormalizedUsername == normalizedUsername))
            return;

        var player = new Player { Id = PlayerId, Name = Username, Rating = 0 };
        var user = new User
        {
            Id = UserId,
            Username = Username,
            NormalizedUsername = normalizedUsername,
            Email = Email,
            NormalizedEmail = UserRepository.Normalize(Email),
            CreatedAt = DateTime.UtcNow,
            PlayerId = player.Id,
            Player = player
        };
        user.PasswordHash = hasher.HashPassword(user, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

        // Тот же дворф, что в обучающем бою, — но своя строка в БД, первым в отряде
        var hero = TutorialBattle.Mentor(player.Id, slot: 1);
        hero.Id = HeroId;

        context.Players.Add(player);
        context.Users.Add(user);
        context.Heroes.Add(hero);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await squadPower.RefreshAsync(player.Id);
    }
}
