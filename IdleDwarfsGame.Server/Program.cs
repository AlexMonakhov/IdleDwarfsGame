using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Енамы (EventType, DamageType...) отдаём строками — фронт сравнивает 'SkillCast', 'Attack' и т.д.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:8080") // Укажите http или https в зависимости от того, как открывается фронт
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Добавьте, если используете куки или авторизацию
        });
});
builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseSqlite("Data Source=game.db;Foreign Keys=True");
    if (builder.Environment.IsDevelopment())
    {
        options
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information);
    }
});

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt settings are missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Оставляем claim-ы как есть ("sub", "playerId"), без маппинга в длинные ClaimTypes
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IHeroesRepository, HeroesRepository>();
builder.Services.AddScoped<HeroUpgradeService>();
// Броски дропа и наград; в тестах подменяется на new Random(seed)
builder.Services.AddSingleton(Random.Shared);
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ItemDropService>();
builder.Services.AddScoped<ChestService>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<RewardService>();
builder.Services.AddScoped<ICombatScenario, TowerFloorScenario>();
builder.Services.AddScoped<ICombatScenario, BossScenario>();
builder.Services.AddScoped<ICombatScenario, CampaignScenario>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<CampaignService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IExpeditionRepository, ExpeditionRepository>();
builder.Services.AddScoped<ExpeditionService>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<FriendService>();
builder.Services.AddScoped<IWorldFactory, WorldFactory>();
builder.Services.AddScoped<ICombatEngine, CombatEngine>();
builder.Services.AddScoped<CombatService>();
builder.Services.AddScoped<IEntityFactory, EntityFactory>();
//builder.Services.AddScoped<IMonsterRepository, MonsterRepository>();

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    db.Database.Migrate();
}



app.UseDefaultFiles();
app.UseStaticFiles(); // явная подача статических файлов
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdleDwarfsGame API v1");
    });
}

app.UseCors("AllowMyFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
