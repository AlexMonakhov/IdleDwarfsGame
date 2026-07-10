using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IPlayerChestRepository, PlayerChestRepository>();
builder.Services.AddScoped<IItemFactory, ItemFactory>();
builder.Services.AddScoped<IItemDropService, ItemDropService>();
builder.Services.AddScoped<ICombatScenario, TowerFloorScenario>();
builder.Services.AddScoped<ICombatScenario, BossScenario>();
builder.Services.AddScoped<IWorldFactory, WorldFactory>();
builder.Services.AddScoped<ICombatEngine, CombatEngine>();
builder.Services.AddScoped<CombatService>();
builder.Services.AddScoped<IRewardService, RewardService>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DwarfMiningIdleGame API v1");
    });
}

app.UseCors("AllowMyFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
