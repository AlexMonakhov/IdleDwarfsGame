using System.Collections.Generic;
using System.Numerics;
using IdleDwarfsGame.Server.Entities.GameEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Hero> Heroes => Set<Hero>();
    public DbSet<PlayerChest> PlayerChests => Set<PlayerChest>();
    public DbSet<ChestDropTable> ChestDropTables => Set<ChestDropTable>();
    public DbSet<Resourses> Resourses => Set<Resourses>();
    public DbSet<PlayerResourses> PlayerResourses => Set<PlayerResourses>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Arena> Arenas => Set<Arena>();
    public DbSet<CampaignStage> CampaignStages => Set<CampaignStage>();
    public DbSet<CampaignStageEnemy> CampaignStageEnemies => Set<CampaignStageEnemy>();
    public DbSet<PlayerCampaignProgress> PlayerCampaignProgress => Set<PlayerCampaignProgress>();
    public DbSet<Expedition> Expeditions => Set<Expedition>();
    public DbSet<ExpeditionHero> ExpeditionHeroes => Set<ExpeditionHero>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Player>()
            .Property(p => p.Id)
            .HasConversion<string>();

        modelBuilder.Entity<Hero>()
            .HasKey(h => h.Id);

        modelBuilder.Entity<Hero>()
            .HasIndex(h => h.PlayerId);

        // Списки храним строкой через запятую: "1,4" / "Burn,Stun"
        modelBuilder.Entity<Hero>()
            .Property(h => h.SpellIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList(),
                new ValueComparer<List<int>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                    v => v.ToList()));

        modelBuilder.Entity<Hero>()
            .Property(h => h.Immunities)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                new ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    v => v.ToList()));

        modelBuilder.Entity<Hero>()
            .Property(h => h.UnitType)
            .HasDefaultValue("Dwarf");

        // Стихию храним строкой ("Fire"), чтобы порядок значений в енаме можно было менять
        modelBuilder.Entity<Hero>()
            .Property(h => h.Element)
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasDefaultValue(HeroElement.Basic);

        modelBuilder.Entity<Player>()
            .HasData(HeroSeedData.Players.Append(CampaignSeedData.Player).ToArray());

        modelBuilder.Entity<Hero>()
            .HasData(HeroSeedData.Heroes.Concat(CampaignSeedData.Heroes).ToArray());

        modelBuilder.Entity<PlayerChest>()
            .HasKey(pc => pc.Id);
        
        modelBuilder.Entity<PlayerChest>()
            .HasOne(pc => pc.Player)
            .WithMany()
            .HasForeignKey(pc => pc.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PlayerChest>()
            .HasOne(pc => pc.ChestDropTable)
            .WithMany()
            .HasForeignKey(pc => pc.ChestDropTableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayerChest>()
            .Property(p => p.PlayerId)
            .HasConversion<string>();           

        modelBuilder.Entity<PlayerChest>()
            .Property(p => p.ChestDropTableId)
            .HasConversion<string>();

        modelBuilder.Entity<ChestDropTable>()
            .HasKey(cdt => cdt.Id);
        
        modelBuilder.Entity<ChestDropTable>()
            .OwnsMany(cdt => cdt.Entries, owned =>
            {
                owned.HasKey(e => e.Id);
            });

        modelBuilder.Entity<ChestDropTable>()
            .Property(p => p.Id)
            .HasConversion<string>();

        modelBuilder.Entity<Resourses>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<Resourses>()
            .Property(r => r.Id)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerResourses>()
            .HasKey(pr => pr.Id);

        modelBuilder.Entity<PlayerResourses>()
            .Property(pr => pr.Id)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerResourses>()
            .Property(pr => pr.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerResourses>()
            .Property(pr => pr.ResourseId)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Username)
            .HasMaxLength(24);

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(254);

        // Уникальность проверяем по нормализованным значениям, чтобы "Dwarf" и "dwarf" считались одним именем
        modelBuilder.Entity<User>()
            .HasIndex(u => u.NormalizedUsername)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Player)
            .WithOne()
            .HasForeignKey<User>(u => u.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Кампания =====
        modelBuilder.Entity<Arena>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<Arena>()
            .HasData(CampaignSeedData.Arenas);

        // Id уровня задаём сами — это его номер в кампании
        modelBuilder.Entity<CampaignStage>()
            .Property(s => s.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<CampaignStage>()
            .HasOne(s => s.Arena)
            .WithMany()
            .HasForeignKey(s => s.ArenaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CampaignStage>()
            .HasMany(s => s.Enemies)
            .WithOne()
            .HasForeignKey(e => e.StageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CampaignStage>()
            .HasData(CampaignSeedData.Stages);

        modelBuilder.Entity<CampaignStageEnemy>()
            .HasOne(e => e.Hero)
            .WithMany()
            .HasForeignKey(e => e.HeroId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CampaignStageEnemy>()
            .HasIndex(e => new { e.StageId, e.Position })
            .IsUnique();

        modelBuilder.Entity<CampaignStageEnemy>()
            .HasData(CampaignSeedData.StageEnemies);

        modelBuilder.Entity<PlayerCampaignProgress>()
            .HasKey(p => new { p.PlayerId, p.StageId });

        modelBuilder.Entity<PlayerCampaignProgress>()
            .Property(p => p.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerCampaignProgress>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(p => p.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayerCampaignProgress>()
            .HasOne<CampaignStage>()
            .WithMany()
            .HasForeignKey(p => p.StageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Экспедиции =====
        modelBuilder.Entity<Expedition>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Expedition>()
            .Property(e => e.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<Expedition>()
            .Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<Expedition>()
            .Property(e => e.TemplateKey)
            .HasMaxLength(32);

        modelBuilder.Entity<Expedition>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(e => e.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Одна доска — по одному заданию на место; заодно защищает от двойной генерации доски
        modelBuilder.Entity<Expedition>()
            .HasIndex(e => new { e.PlayerId, e.BoardStartsAt, e.Slot })
            .IsUnique();

        modelBuilder.Entity<Expedition>()
            .HasMany(e => e.Heroes)
            .WithOne()
            .HasForeignKey(h => h.ExpeditionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExpeditionHero>()
            .HasKey(h => new { h.ExpeditionId, h.HeroId });

        modelBuilder.Entity<ExpeditionHero>()
            .HasOne<Hero>()
            .WithMany()
            .HasForeignKey(h => h.HeroId)
            .OnDelete(DeleteBehavior.Cascade);

        // Герой может быть только в одном задании одновременно — даже при параллельных запросах
        modelBuilder.Entity<ExpeditionHero>()
            .HasIndex(h => h.HeroId)
            .IsUnique();
    }
}