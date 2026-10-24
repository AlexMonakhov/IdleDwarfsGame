using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
public class AuthController(
    GameDbContext context,
    IUserRepository userRepository,
    ITokenService tokenService,
    IPasswordHasher<User> passwordHasher) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await userRepository.UsernameExistsAsync(username))
            ModelState.AddModelError(nameof(request.Username), "Это имя уже занято");
        if (await userRepository.EmailExistsAsync(email))
            ModelState.AddModelError(nameof(request.Email), "Эта почта уже зарегистрирована");
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Каждому аккаунту — свой игрок, к которому потом привязываются герои, сундуки и т.д.
        var player = new Player { Id = Guid.NewGuid(), Name = username, Rating = 0 };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            NormalizedUsername = UserRepository.Normalize(username),
            Email = email,
            NormalizedEmail = UserRepository.Normalize(email),
            CreatedAt = DateTime.UtcNow,
            PlayerId = player.Id,
            Player = player
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await context.Players.AddAsync(player);
        await userRepository.CreateAsync(user);

        return Ok(BuildResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await userRepository.GetByLoginAsync(request.Login);
        if (user == null)
            return Unauthorized(new { message = "Неверное имя или пароль" });

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Неверное имя или пароль" });

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await context.SaveChangesAsync();
        }

        return Ok(BuildResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthUserResponse>> Me()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        return Ok(ToUserResponse(user));
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = tokenService.CreateToken(user);
        return new AuthResponse { Token = token, ExpiresAt = expiresAt, User = ToUserResponse(user) };
    }

    private static AuthUserResponse ToUserResponse(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        PlayerId = user.PlayerId
    };
}
