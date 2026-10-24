using Microsoft.EntityFrameworkCore;

public class UserRepository(GameDbContext context) : IUserRepository
{
    public static string Normalize(string value) => value.Trim().ToUpperInvariant();

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .Include(u => u.Player)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    // Входить можно как по имени, так и по почте
    public async Task<User?> GetByLoginAsync(string login)
    {
        var normalized = Normalize(login);
        return await context.Users
            .Include(u => u.Player)
            .FirstOrDefaultAsync(u => u.NormalizedUsername == normalized || u.NormalizedEmail == normalized);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var normalized = Normalize(username);
        return await context.Users.AnyAsync(u => u.NormalizedUsername == normalized);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var normalized = Normalize(email);
        return await context.Users.AnyAsync(u => u.NormalizedEmail == normalized);
    }

    public async Task CreateAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }
}
