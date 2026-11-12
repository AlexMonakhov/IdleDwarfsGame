using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
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
// За балансировщиком (Azure App Service) HTTPS заканчивается на прокси, к приложению приходит http + X-Forwarded-Proto.
// Без этого UseHttpsRedirection считал бы каждый запрос нешифрованным и редиректил по кругу
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddDbContext<GameDbContext>(options =>
{
    // Строку подключения можно переопределить переменной окружения ConnectionStrings__GameDb или user-secrets
    var connectionString = builder.Configuration.GetConnectionString("GameDb")
        ?? throw new InvalidOperationException("Connection string 'GameDb' is missing");
    options.UseSqlServer(connectionString);
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
// Ключ подписи есть только в appsettings.Development.json; на проде он задаётся переменной окружения Jwt__Key
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
    throw new InvalidOperationException("Jwt:Key is missing or shorter than 32 bytes. Set the Jwt__Key environment variable");

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
        options.Events = new JwtBearerEvents
        {
            // Подпись верна, но аккаунта уже нет (база пересоздана, аккаунт удалён) — отвечаем 401,
            // иначе первая же запись с этим UserId упадёт на внешнем ключе
            OnTokenValidated = async context =>
            {
                var sub = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                var db = context.HttpContext.RequestServices.GetRequiredService<GameDbContext>();
                if (!Guid.TryParse(sub, out var userId) || !await db.Users.AnyAsync(u => u.Id == userId))
                    context.Fail("User no longer exists");
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IHeroesRepository, HeroesRepository>();
builder.Services.AddScoped<HeroUpgradeService>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<EquipmentService>();
// Броски дропа и наград; в тестах подменяется на new Random(seed)
builder.Services.AddSingleton(Random.Shared);
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ItemDropService>();
builder.Services.AddScoped<ChestService>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<RewardService>();
builder.Services.AddScoped<ICombatScenario, BossScenario>();
builder.Services.AddScoped<ICombatScenario, CampaignScenario>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<CampaignService>();
builder.Services.AddScoped<ITowerRepository, TowerRepository>();
builder.Services.AddScoped<TowerService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IExpeditionRepository, ExpeditionRepository>();
builder.Services.AddScoped<ExpeditionService>();
builder.Services.AddScoped<IMiningRepository, MiningRepository>();
builder.Services.AddScoped<MiningService>();
builder.Services.AddScoped<IAbilityRepository, AbilityRepository>();
builder.Services.AddScoped<AbilityService>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<FriendService>();
builder.Services.AddScoped<IGuildRepository, GuildRepository>();
builder.Services.AddScoped<GuildService>();
builder.Services.AddScoped<IPvpRepository, PvpRepository>();
builder.Services.AddScoped<SquadPowerService>();
builder.Services.AddScoped<PvpService>();
builder.Services.AddScoped<RankedPvpService>();
builder.Services.AddScoped<IWorldFactory, WorldFactory>();
builder.Services.AddScoped<ICombatEngine, CombatEngine>();
builder.Services.AddScoped<CombatService>();
builder.Services.AddScoped<IEntityFactory, EntityFactory>();
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<ShopService>();
builder.Services.AddScoped<IForgeRepository, ForgeRepository>();
builder.Services.AddScoped<ForgeService>();
// Счётчики действий за сутки — по ним идут ежедневные задания
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityTracker, ActivityTracker>();
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
builder.Services.AddScoped<QuestService>();
// Лавка старейшин и лавка ивента недели (WorldEvents)
builder.Services.AddScoped<IFixedShopRepository, FixedShopRepository>();
builder.Services.AddScoped<FixedShopService>();
// Ивенты недели: мировые боссы и общие шахты (их месторождения ведёт ещё и фоновая задача)
builder.Services.AddScoped<IWorldBossRepository, WorldBossRepository>();
builder.Services.AddScoped<WorldBossService>();
builder.Services.AddScoped<ICommunityMineRepository, CommunityMineRepository>();
builder.Services.AddScoped<CommunityMineService>();
builder.Services.AddHostedService<WorldEventsTicker>();
// Гонка глубины гильдий, помощь друзьям, ставки на «Бой дня», глубинный спуск
builder.Services.AddScoped<GuildDepthService>();
builder.Services.AddScoped<FriendHelpService>();
builder.Services.AddScoped<ArenaBetService>();
builder.Services.AddScoped<DelveService>();
//builder.Services.AddScoped<IMonsterRepository, MonsterRepository>();

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

    // Бесплатная Azure SQL (serverless) засыпает без нагрузки и просыпается около минуты — пока она встаёт,
    // подключения отбиваются. Ждём её на старте, а не падаем: иначе App Service перезапускает приложение по кругу
    const int migrateAttempts = 12;
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (SqlException ex) when (attempt < migrateAttempts)
        {
            app.Logger.LogWarning("Database is not available yet (attempt {Attempt}/{Total}): {Message}", attempt, migrateAttempts, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }

    // Боты арены — соперники для PvP. Локально под ними можно войти (общий пароль), на проде пароли случайные
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    await TestAccountsSeed.SeedAsync(db, hasher, app.Environment.IsDevelopment() ? TestAccountsSeed.Password : null);
    if (app.Environment.IsDevelopment())
        await EquipmentDevSeed.SeedAsync(db);

    // Мощь отрядов хранится в БД; правки баланса меняют формулу, поэтому на старте пересчитываем у всех
    await scope.ServiceProvider.GetRequiredService<SquadPowerService>().RefreshAllAsync();
}


app.UseForwardedHeaders();

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
