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
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<PlayerItem> PlayerItems => Set<PlayerItem>();
    public DbSet<PlayerEquipment> PlayerEquipment => Set<PlayerEquipment>();
    public DbSet<PlayerReward> PlayerRewards => Set<PlayerReward>();
    public DbSet<PlayerRewardChest> PlayerRewardChests => Set<PlayerRewardChest>();
    public DbSet<PlayerRewardItem> PlayerRewardItems => Set<PlayerRewardItem>();
    public DbSet<Guild> Guilds => Set<Guild>();
    public DbSet<GuildMember> GuildMembers => Set<GuildMember>();
    public DbSet<GuildJoinRequest> GuildJoinRequests => Set<GuildJoinRequest>();
    public DbSet<GuildBoss> GuildBosses => Set<GuildBoss>();
    public DbSet<GuildBossAttack> GuildBossAttacks => Set<GuildBossAttack>();
    public DbSet<GuildBossCooldown> GuildBossCooldowns => Set<GuildBossCooldown>();
    public DbSet<GuildBossRewardClaim> GuildBossRewardClaims => Set<GuildBossRewardClaim>();
    public DbSet<PvpBattle> PvpBattles => Set<PvpBattle>();
    public DbSet<PvpSeasonScore> PvpSeasonScores => Set<PvpSeasonScore>();
    public DbSet<RankedSquad> RankedSquads => Set<RankedSquad>();
    public DbSet<PvpScout> PvpScouts => Set<PvpScout>();
    public DbSet<Mine> Mines => Set<Mine>();
    public DbSet<MineDwarf> MineDwarves => Set<MineDwarf>();
    public DbSet<PlayerAbility> PlayerAbilities => Set<PlayerAbility>();
    public DbSet<PlayerTowerProgress> PlayerTowerProgress => Set<PlayerTowerProgress>();
    public DbSet<ShopState> ShopStates => Set<ShopState>();
    public DbSet<ShopOffer> ShopOffers => Set<ShopOffer>();
    public DbSet<PlayerDailyActivity> PlayerDailyActivities => Set<PlayerDailyActivity>();
    public DbSet<DailyQuestState> DailyQuestStates => Set<DailyQuestState>();
    public DbSet<PlayerQuest> PlayerQuests => Set<PlayerQuest>();
    public DbSet<PlayerEventWallet> PlayerEventWallets => Set<PlayerEventWallet>();
    public DbSet<FixedShopPurchase> FixedShopPurchases => Set<FixedShopPurchase>();
    public DbSet<WorldBoss> WorldBosses => Set<WorldBoss>();
    public DbSet<WorldBossContribution> WorldBossContributions => Set<WorldBossContribution>();
    public DbSet<WorldBossCooldown> WorldBossCooldowns => Set<WorldBossCooldown>();
    public DbSet<WorldBossRewardClaim> WorldBossRewardClaims => Set<WorldBossRewardClaim>();
    public DbSet<CommunityDeposit> CommunityDeposits => Set<CommunityDeposit>();
    public DbSet<DepositWorker> DepositWorkers => Set<DepositWorker>();
    public DbSet<DepositDwarf> DepositDwarves => Set<DepositDwarf>();
    public DbSet<DepositRewardClaim> DepositRewardClaims => Set<DepositRewardClaim>();
    public DbSet<GuildDepthShaft> GuildDepthShafts => Set<GuildDepthShaft>();
    public DbSet<GuildDepthDwarf> GuildDepthDwarves => Set<GuildDepthDwarf>();
    public DbSet<GuildDepthRewardClaim> GuildDepthRewardClaims => Set<GuildDepthRewardClaim>();
    public DbSet<FriendMineHelp> FriendMineHelps => Set<FriendMineHelp>();
    public DbSet<FriendHire> FriendHires => Set<FriendHire>();
    public DbSet<ArenaBetMatch> ArenaBetMatches => Set<ArenaBetMatch>();
    public DbSet<ArenaBet> ArenaBets => Set<ArenaBet>();
    public DbSet<DelveRun> DelveRuns => Set<DelveRun>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .HasKey(p => p.Id);

        // Улучшения кирки начинаются с 1-го уровня — и у тех, кто уже был в базе до них
        modelBuilder.Entity<Player>().Property(p => p.PickaxeCapacityLevel).HasDefaultValue(1);
        modelBuilder.Entity<Player>().Property(p => p.PickaxeRecoveryLevel).HasDefaultValue(1);
        modelBuilder.Entity<Player>().Property(p => p.PickaxeStrengthLevel).HasDefaultValue(1);

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
            .Property(h => h.Passives)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<HeroPassive>).ToList(),
                new ValueComparer<List<HeroPassive>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                    v => v.ToList()));

        modelBuilder.Entity<Hero>()
            .Property(h => h.UnitType)
            .HasDefaultValue("Dwarf");

        // Уже существующие герои начинают прокачку с первого уровня
        modelBuilder.Entity<Hero>()
            .Property(h => h.UpgradeLevel)
            .HasDefaultValue(HeroUpgradeService.MinLevel);

        // Уровни пассивки и ауры старых героев выставляет миграция AddAbilities
        modelBuilder.Entity<Hero>()
            .Property(h => h.PassiveLevel)
            .HasDefaultValue(PassiveBook.MinLevel);

        modelBuilder.Entity<Hero>()
            .Property(h => h.AuraLevel)
            .HasDefaultValue(AuraBook.MinLevel);

        // Стихию храним строкой ("Fire"), чтобы порядок значений в енаме можно было менять
        modelBuilder.Entity<Hero>()
            .Property(h => h.Element)
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasDefaultValue(HeroElement.Basic);

        // Профессию храним строкой, как и стихию; у монстров её нет
        modelBuilder.Entity<Hero>()
            .Property(h => h.Profession)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<Player>()
            .HasData(HeroSeedData.Players.Append(CampaignSeedData.Player).Append(GuildBossSeedData.Player).ToArray());

        modelBuilder.Entity<Hero>()
            .HasData(HeroSeedData.Heroes.Concat(CampaignSeedData.Heroes).Concat(GuildBossSeedData.Heroes).ToArray());

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
                owned.HasData(LootSeedData.Entries);
            });

        modelBuilder.Entity<ChestDropTable>()
            .Property(p => p.Id)
            .HasConversion<string>();

        modelBuilder.Entity<ChestDropTable>()
            .Property(t => t.Rolls)
            .HasDefaultValue(1);

        modelBuilder.Entity<ChestDropTable>()
            .Property(t => t.Element)
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasDefaultValue(HeroElement.Basic);

        // Сундук определяется типом, уровнем и стихией — по ним награда находит таблицу
        modelBuilder.Entity<ChestDropTable>()
            .HasIndex(t => new { t.ChestType, t.ChestLevel, t.Element })
            .IsUnique();

        modelBuilder.Entity<ChestDropTable>()
            .HasData(LootSeedData.Tables);

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
            .Property(a => a.Element)
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasDefaultValue(HeroElement.Basic);

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

        // ===== Друзья =====
        modelBuilder.Entity<FriendRequest>()
            .HasKey(r => new { r.FromUserId, r.ToUserId });

        modelBuilder.Entity<FriendRequest>()
            .Property(r => r.FromUserId)
            .HasConversion<string>();

        modelBuilder.Entity<FriendRequest>()
            .Property(r => r.ToUserId)
            .HasConversion<string>();

        modelBuilder.Entity<FriendRequest>()
            .HasOne(r => r.From)
            .WithMany()
            .HasForeignKey(r => r.FromUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // SQL Server не допускает двух каскадных путей из Users в одну таблицу, поэтому второй ключ пары — без каскада.
        // Такие строки удаляет сам PlayerRepository.DeleteAsync (так же у дружбы и PvP-боёв)
        modelBuilder.Entity<FriendRequest>()
            .HasOne(r => r.To)
            .WithMany()
            .HasForeignKey(r => r.ToUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Входящие заявки игрока
        modelBuilder.Entity<FriendRequest>()
            .HasIndex(r => r.ToUserId);

        modelBuilder.Entity<Friendship>()
            .HasKey(f => new { f.UserId, f.FriendId });

        modelBuilder.Entity<Friendship>()
            .Property(f => f.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<Friendship>()
            .Property(f => f.FriendId)
            .HasConversion<string>();

        modelBuilder.Entity<Friendship>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Friend)
            .WithMany()
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.NoAction);

        // ===== Сессии =====
        modelBuilder.Entity<UserSession>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<UserSession>()
            .Property(s => s.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<UserSession>()
            .Property(s => s.UserAgent)
            .HasMaxLength(SessionService.MaxUserAgentLength);

        modelBuilder.Entity<UserSession>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // «Когда игрок был в сети» — последняя по LastSeenAt сессия
        modelBuilder.Entity<UserSession>()
            .HasIndex(s => new { s.UserId, s.LastSeenAt });

        // ===== Инвентарь =====
        modelBuilder.Entity<PlayerItem>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<PlayerItem>()
            .Property(i => i.PlayerId)
            .HasConversion<string>();

        // Енамы строкой, чтобы порядок значений можно было менять
        modelBuilder.Entity<PlayerItem>()
            .Property(i => i.ItemType)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PlayerItem>()
            .Property(i => i.Rarity)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PlayerItem>()
            .Property(i => i.SubType)
            .HasMaxLength(32);

        modelBuilder.Entity<PlayerItem>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(i => i.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Одна стопка на вид предмета
        modelBuilder.Entity<PlayerItem>()
            .HasIndex(i => new { i.PlayerId, i.ItemType, i.SubType, i.Rarity, i.Level })
            .IsUnique();

        // ===== Снаряжение =====
        modelBuilder.Entity<PlayerEquipment>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<PlayerEquipment>()
            .Property(e => e.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerEquipment>()
            .Property(e => e.ItemKey)
            .HasMaxLength(32);

        modelBuilder.Entity<PlayerEquipment>()
            .Property(e => e.Slot)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PlayerEquipment>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(e => e.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Герой исчез — его предметы возвращаются в инвентарь
        modelBuilder.Entity<Hero>()
            .HasMany(h => h.Equipment)
            .WithOne()
            .HasForeignKey(e => e.HeroId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PlayerEquipment>()
            .HasIndex(e => e.PlayerId);

        // В слоте героя один предмет. Лежащие в инвентаре (null) индекс не ограничивает:
        // для nullable-колонок EF добавляет в SQL Server фильтр IS NOT NULL (в SQLite NULL и так не равен NULL)
        modelBuilder.Entity<PlayerEquipment>()
            .HasIndex(e => new { e.HeroId, e.Slot })
            .IsUnique();

        // ===== Награды =====
        modelBuilder.Entity<PlayerReward>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<PlayerReward>()
            .Property(r => r.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerReward>()
            .Property(r => r.Source)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PlayerReward>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayerReward>()
            .HasMany(r => r.Chests)
            .WithOne()
            .HasForeignKey(c => c.RewardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Незабранные награды игрока
        modelBuilder.Entity<PlayerReward>()
            .HasIndex(r => new { r.PlayerId, r.Source, r.ClaimedAt });

        modelBuilder.Entity<PlayerRewardChest>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<PlayerRewardChest>()
            .Property(c => c.ChestType)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PlayerRewardChest>()
            .Property(c => c.Element)
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasDefaultValue(HeroElement.Basic);

        modelBuilder.Entity<PlayerReward>()
            .HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.RewardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayerRewardItem>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<PlayerRewardItem>()
            .Property(i => i.ItemType)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PlayerRewardItem>()
            .Property(i => i.Rarity)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PlayerRewardItem>()
            .Property(i => i.SubType)
            .HasMaxLength(32);

        // ===== Гильдии =====
        modelBuilder.Entity<Guild>()
            .HasKey(g => g.Id);

        modelBuilder.Entity<Guild>()
            .Property(g => g.Id)
            .HasConversion<string>();

        modelBuilder.Entity<Guild>()
            .Property(g => g.Name)
            .HasMaxLength(GuildService.MaxNameLength);

        modelBuilder.Entity<Guild>()
            .Property(g => g.NormalizedName)
            .HasMaxLength(GuildService.MaxNameLength);

        modelBuilder.Entity<Guild>()
            .HasIndex(g => g.NormalizedName)
            .IsUnique();

        modelBuilder.Entity<GuildMember>()
            .HasKey(m => m.UserId);

        modelBuilder.Entity<GuildMember>()
            .Property(m => m.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildMember>()
            .Property(m => m.GuildId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildMember>()
            .Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<GuildMember>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuildMember>()
            .HasOne(m => m.Guild)
            .WithMany()
            .HasForeignKey(m => m.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuildMember>()
            .HasIndex(m => m.GuildId);

        modelBuilder.Entity<GuildJoinRequest>()
            .HasKey(r => new { r.UserId, r.GuildId });

        modelBuilder.Entity<GuildJoinRequest>()
            .Property(r => r.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildJoinRequest>()
            .Property(r => r.GuildId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildJoinRequest>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuildJoinRequest>()
            .HasOne(r => r.Guild)
            .WithMany()
            .HasForeignKey(r => r.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        // Заявки в гильдию — для главы
        modelBuilder.Entity<GuildJoinRequest>()
            .HasIndex(r => r.GuildId);

        modelBuilder.Entity<GuildBoss>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<GuildBoss>()
            .Property(b => b.Id)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBoss>()
            .Property(b => b.GuildId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBoss>()
            .Property(b => b.KilledByUserId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBoss>()
            .Property(b => b.Tier)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<GuildBoss>()
            .HasOne<Guild>()
            .WithMany()
            .HasForeignKey(b => b.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один босс каждого уровня на гильдию в сутки — заодно защищает от двойного создания
        modelBuilder.Entity<GuildBoss>()
            .HasIndex(b => new { b.GuildId, b.Day, b.Tier })
            .IsUnique();

        modelBuilder.Entity<GuildBossAttack>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<GuildBossAttack>()
            .Property(a => a.Id)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBossAttack>()
            .Property(a => a.BossId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBossAttack>()
            .Property(a => a.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBossAttack>()
            .HasOne<GuildBoss>()
            .WithMany()
            .HasForeignKey(a => a.BossId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuildBossAttack>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuildBossAttack>()
            .HasIndex(a => a.BossId);

        modelBuilder.Entity<GuildBossCooldown>()
            .HasKey(c => new { c.UserId, c.Tier });

        modelBuilder.Entity<GuildBossCooldown>()
            .Property(c => c.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBossCooldown>()
            .Property(c => c.Tier)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<GuildBossCooldown>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GuildBossRewardClaim>()
            .HasKey(c => new { c.UserId, c.Tier, c.Day });

        modelBuilder.Entity<GuildBossRewardClaim>()
            .Property(c => c.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBossRewardClaim>()
            .Property(c => c.Tier)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<GuildBossRewardClaim>()
            .Property(c => c.GuildId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBossRewardClaim>()
            .Property(c => c.BossId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBossRewardClaim>()
            .Property(c => c.RewardId)
            .HasConversion<string>();

        modelBuilder.Entity<GuildBossRewardClaim>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== PvP =====
        modelBuilder.Entity<PvpBattle>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<PvpBattle>()
            .Property(b => b.Id)
            .HasConversion<string>();

        modelBuilder.Entity<PvpBattle>()
            .Property(b => b.AttackerUserId)
            .HasConversion<string>();

        modelBuilder.Entity<PvpBattle>()
            .Property(b => b.DefenderUserId)
            .HasConversion<string>();

        // Бои, сыгранные до рейтинговой арены, миграция AddRankedArena отметила обычными
        modelBuilder.Entity<PvpBattle>()
            .Property(b => b.Mode)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PvpBattle>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.AttackerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PvpBattle>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.DefenderUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Побеждённого сегодня соперника нельзя победить ещё раз на той же арене — даже двумя одновременными боями
        modelBuilder.Entity<PvpBattle>()
            .HasIndex(b => new { b.AttackerUserId, b.DefenderUserId, b.Day, b.Mode })
            .IsUnique()
            .HasFilter("[AttackerWon] = 1");

        // История боёв игрока: нападения и защиты
        modelBuilder.Entity<PvpBattle>()
            .HasIndex(b => new { b.AttackerUserId, b.FoughtAt });

        modelBuilder.Entity<PvpBattle>()
            .HasIndex(b => new { b.DefenderUserId, b.FoughtAt });

        modelBuilder.Entity<PvpSeasonScore>()
            .HasKey(s => new { s.UserId, s.Mode, s.Type, s.Start });

        modelBuilder.Entity<PvpSeasonScore>()
            .Property(s => s.UserId)
            .HasConversion<string>();

        // Mode — часть ключа, поэтому без значения по умолчанию в модели: старые очки миграция отметила обычными
        modelBuilder.Entity<PvpSeasonScore>()
            .Property(s => s.Mode)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PvpSeasonScore>()
            .Property(s => s.Type)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PvpSeasonScore>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Таблица лидеров сезона
        modelBuilder.Entity<PvpSeasonScore>()
            .HasIndex(s => new { s.Mode, s.Type, s.Start, s.Points });

        // Разведка: одна строка на соперника в сутки на арене; лимит считается по строкам за сутки
        modelBuilder.Entity<PvpScout>()
            .HasKey(s => new { s.UserId, s.Day, s.Mode, s.OpponentUserId });

        modelBuilder.Entity<PvpScout>()
            .Property(s => s.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<PvpScout>()
            .Property(s => s.OpponentUserId)
            .HasConversion<string>();

        modelBuilder.Entity<PvpScout>()
            .Property(s => s.Mode)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PvpScout>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один закреплённый отряд на игрока
        modelBuilder.Entity<RankedSquad>()
            .HasKey(s => s.UserId);

        modelBuilder.Entity<RankedSquad>()
            .Property(s => s.UserId)
            .HasConversion<string>();

        modelBuilder.Entity<RankedSquad>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Шахты =====
        modelBuilder.Entity<Mine>()
            .HasKey(m => m.Id);

        modelBuilder.Entity<Mine>()
            .Property(m => m.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<Mine>()
            .Property(m => m.Type)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<Mine>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(m => m.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Одна шахта на место — заодно защищает от двойной постройки
        modelBuilder.Entity<Mine>()
            .HasIndex(m => new { m.PlayerId, m.Plot })
            .IsUnique();

        // Дворфы шахты — как герои экспедиции: шахта снесена или герой исчез — строка уходит
        modelBuilder.Entity<Mine>()
            .HasMany(m => m.Dwarves)
            .WithOne()
            .HasForeignKey(d => d.MineId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MineDwarf>()
            .HasKey(d => new { d.MineId, d.HeroId });

        modelBuilder.Entity<MineDwarf>()
            .HasOne<Hero>()
            .WithMany()
            .HasForeignKey(d => d.HeroId)
            .OnDelete(DeleteBehavior.Cascade);

        // Дворф работает только в одной шахте — даже при параллельных запросах
        modelBuilder.Entity<MineDwarf>()
            .HasIndex(d => d.HeroId)
            .IsUnique();

        // Одно место в шахте — один дворф
        modelBuilder.Entity<MineDwarf>()
            .HasIndex(d => new { d.MineId, d.Slot })
            .IsUnique();

        // ===== Извлечённые способности =====
        modelBuilder.Entity<PlayerAbility>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<PlayerAbility>()
            .Property(a => a.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerAbility>()
            .Property(a => a.Kind)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<PlayerAbility>()
            .Property(a => a.Key)
            .HasMaxLength(32);

        modelBuilder.Entity<PlayerAbility>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(a => a.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayerAbility>()
            .HasIndex(a => a.PlayerId);

        // ===== Бесконечная башня =====
        // Одна строка на игрока в неделю — заодно защищает от двойного создания прогресса параллельными боями
        modelBuilder.Entity<PlayerTowerProgress>()
            .HasKey(p => new { p.PlayerId, p.WeekStart });

        modelBuilder.Entity<PlayerTowerProgress>()
            .Property(p => p.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerTowerProgress>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(p => p.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Магазины =====
        // Одна строка на игрока и магазин — заодно защищает от двойного создания полки параллельными заходами
        modelBuilder.Entity<ShopState>()
            .HasKey(s => new { s.PlayerId, s.Shop });

        modelBuilder.Entity<ShopState>()
            .Property(s => s.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<ShopState>()
            .Property(s => s.Shop)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<ShopState>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShopOffer>()
            .HasKey(o => o.Id);

        modelBuilder.Entity<ShopOffer>()
            .Property(o => o.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<ShopOffer>()
            .Property(o => o.Shop)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<ShopOffer>()
            .Property(o => o.Kind)
            .HasConversion<string>()
            .HasMaxLength(8);

        modelBuilder.Entity<ShopOffer>()
            .Property(o => o.Rarity)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<ShopOffer>()
            .Property(o => o.PriceGem)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<ShopOffer>()
            .Property(o => o.PriceGemRarity)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<ShopOffer>()
            .Property(o => o.ItemKey)
            .HasMaxLength(32);

        modelBuilder.Entity<ShopOffer>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(o => o.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Одно место на полке — один товар
        modelBuilder.Entity<ShopOffer>()
            .HasIndex(o => new { o.PlayerId, o.Shop, o.Kind, o.Slot })
            .IsUnique();

        // ===== Ежедневные задания =====
        modelBuilder.Entity<PlayerDailyActivity>()
            .HasKey(a => new { a.PlayerId, a.Day, a.Kind });

        modelBuilder.Entity<PlayerDailyActivity>()
            .Property(a => a.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerDailyActivity>()
            .Property(a => a.Kind)
            .HasConversion<string>()
            .HasMaxLength(32);

        modelBuilder.Entity<PlayerDailyActivity>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(a => a.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DailyQuestState>()
            .HasKey(s => s.PlayerId);

        modelBuilder.Entity<DailyQuestState>()
            .Property(s => s.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<DailyQuestState>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayerQuest>()
            .HasKey(q => q.Id);

        modelBuilder.Entity<PlayerQuest>()
            .Property(q => q.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerQuest>()
            .Property(q => q.Key)
            .HasMaxLength(32);

        modelBuilder.Entity<PlayerQuest>()
            .Property(q => q.Kind)
            .HasConversion<string>()
            .HasMaxLength(32);

        modelBuilder.Entity<PlayerQuest>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(q => q.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Одно место на доске — одно задание; заодно защищает от двойной генерации доски
        modelBuilder.Entity<PlayerQuest>()
            .HasIndex(q => new { q.PlayerId, q.Day, q.Slot })
            .IsUnique();

        // ===== Ивенты =====
        modelBuilder.Entity<PlayerEventWallet>()
            .HasKey(w => new { w.PlayerId, w.EventStart });

        modelBuilder.Entity<PlayerEventWallet>()
            .Property(w => w.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<PlayerEventWallet>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(w => w.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FixedShopPurchase>()
            .HasKey(p => new { p.PlayerId, p.Shop, p.Period, p.OfferKey });

        modelBuilder.Entity<FixedShopPurchase>()
            .Property(p => p.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<FixedShopPurchase>()
            .Property(p => p.Shop)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<FixedShopPurchase>()
            .Property(p => p.OfferKey)
            .HasMaxLength(32);

        modelBuilder.Entity<FixedShopPurchase>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(p => p.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Мировые боссы =====
        modelBuilder.Entity<WorldBoss>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<WorldBoss>()
            .Property(b => b.KilledByPlayerId)
            .HasConversion<string>();

        // Один босс на этап ивента — защищает от двойного появления следующего этапа
        modelBuilder.Entity<WorldBoss>()
            .HasIndex(b => new { b.EventStart, b.Stage })
            .IsUnique();

        modelBuilder.Entity<WorldBossContribution>()
            .HasKey(c => new { c.BossId, c.PlayerId });

        modelBuilder.Entity<WorldBossContribution>()
            .Property(c => c.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<WorldBossContribution>()
            .HasOne<WorldBoss>()
            .WithMany()
            .HasForeignKey(c => c.BossId)
            .OnDelete(DeleteBehavior.Cascade);

        // Босс с игроком не связан, так что два каскада сюда идут из разных таблиц — SQL Server это допускает
        modelBuilder.Entity<WorldBossContribution>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Урон игрока за ивент
        modelBuilder.Entity<WorldBossContribution>()
            .HasIndex(c => c.PlayerId);

        modelBuilder.Entity<WorldBossCooldown>()
            .HasKey(c => c.PlayerId);

        modelBuilder.Entity<WorldBossCooldown>()
            .Property(c => c.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<WorldBossCooldown>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorldBossRewardClaim>()
            .HasKey(r => new { r.BossId, r.PlayerId });

        modelBuilder.Entity<WorldBossRewardClaim>()
            .Property(r => r.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<WorldBossRewardClaim>()
            .HasOne<WorldBoss>()
            .WithMany()
            .HasForeignKey(r => r.BossId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorldBossRewardClaim>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Общие шахты =====
        modelBuilder.Entity<CommunityDeposit>()
            .HasKey(d => d.Id);

        modelBuilder.Entity<CommunityDeposit>()
            .Property(d => d.Kind)
            .HasConversion<string>()
            .HasMaxLength(16);

        modelBuilder.Entity<CommunityDeposit>()
            .Property(d => d.SpeedLevel)
            .HasDefaultValue(1);

        // Одно месторождение на номер ивента — защищает от двойного появления
        modelBuilder.Entity<CommunityDeposit>()
            .HasIndex(d => new { d.EventStart, d.Index })
            .IsUnique();

        modelBuilder.Entity<DepositWorker>()
            .HasKey(w => new { w.DepositId, w.PlayerId });

        modelBuilder.Entity<DepositWorker>()
            .Property(w => w.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<DepositWorker>()
            .HasOne<CommunityDeposit>()
            .WithMany()
            .HasForeignKey(w => w.DepositId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DepositWorker>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(w => w.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DepositWorker>()
            .HasIndex(w => w.PlayerId);

        // Ключ — дворф: он работает только на одном месторождении, даже при параллельных запросах
        modelBuilder.Entity<DepositDwarf>()
            .HasKey(d => d.HeroId);

        modelBuilder.Entity<DepositDwarf>()
            .Property(d => d.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<DepositDwarf>()
            .HasOne<CommunityDeposit>()
            .WithMany()
            .HasForeignKey(d => d.DepositId)
            .OnDelete(DeleteBehavior.Cascade);

        // Герой удаляется (слияние) — с месторождения он уходит. Связи с игроком нет: каскад от игрока идёт через героя
        modelBuilder.Entity<DepositDwarf>()
            .HasOne<Hero>()
            .WithMany()
            .HasForeignKey(d => d.HeroId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DepositDwarf>()
            .HasIndex(d => d.PlayerId);

        modelBuilder.Entity<DepositRewardClaim>()
            .HasKey(c => new { c.DepositId, c.PlayerId });

        modelBuilder.Entity<DepositRewardClaim>()
            .Property(c => c.PlayerId)
            .HasConversion<string>();

        modelBuilder.Entity<DepositRewardClaim>()
            .HasOne<CommunityDeposit>()
            .WithMany()
            .HasForeignKey(c => c.DepositId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DepositRewardClaim>()
            .HasOne<Player>()
            .WithMany()
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    
        ConfigureSocialAndDelve(modelBuilder);
    }

    // Гонка глубины гильдий, помощь друзьям, ставки на «Бой дня», глубинный спуск.
    // Id аккаунтов, игроков и гильдий — строкой, как и везде
    private static void ConfigureSocialAndDelve(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildDepthShaft>(e =>
        {
            e.HasKey(x => new { x.GuildId, x.WeekStart });
            e.Property(x => x.GuildId).HasConversion<string>();
            e.Property(x => x.Level).HasDefaultValue(1);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasOne<Guild>().WithMany().HasForeignKey(x => x.GuildId).OnDelete(DeleteBehavior.Cascade);
        });

        // Без внешнего ключа на гильдию: при роспуске вклад и места недели должны остаться
        modelBuilder.Entity<GuildDepthDwarf>(e =>
        {
            e.HasKey(x => new { x.WeekStart, x.UserId });
            e.Property(x => x.UserId).HasConversion<string>();
            e.Property(x => x.GuildId).HasConversion<string>();
            e.Property(x => x.PlayerId).HasConversion<string>();
            e.Property(x => x.PickLevel).HasDefaultValue(1);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasIndex(x => new { x.WeekStart, x.GuildId });
            e.HasIndex(x => x.HeroId);
            e.HasIndex(x => x.PlayerId);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuildDepthRewardClaim>(e =>
        {
            e.HasKey(x => new { x.UserId, x.WeekStart });
            e.Property(x => x.UserId).HasConversion<string>();
            e.Property(x => x.GuildId).HasConversion<string>();
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FriendMineHelp>(e =>
        {
            e.HasKey(x => new { x.HelperUserId, x.TargetUserId, x.Day });
            e.Property(x => x.HelperUserId).HasConversion<string>();
            e.Property(x => x.TargetUserId).HasConversion<string>();
            e.HasIndex(x => new { x.TargetUserId, x.Day });
            e.HasOne<User>().WithMany().HasForeignKey(x => x.HelperUserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.TargetUserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<FriendHire>(e =>
        {
            e.HasKey(x => new { x.BorrowerUserId, x.LenderUserId, x.Day });
            e.Property(x => x.BorrowerUserId).HasConversion<string>();
            e.Property(x => x.LenderUserId).HasConversion<string>();
            e.Property(x => x.HeroName).HasMaxLength(64);
            e.HasIndex(x => new { x.LenderUserId, x.Day });
            e.HasOne<User>().WithMany().HasForeignKey(x => x.BorrowerUserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.LenderUserId).OnDelete(DeleteBehavior.NoAction);
        });

        // Бой дня хранит снимки и имена — внешних ключей на игроков нет, удаление аккаунта бой не ломает
        modelBuilder.Entity<ArenaBetMatch>(e =>
        {
            e.HasKey(x => x.WindowStart);
            e.Property(x => x.AUserId).HasConversion<string>();
            e.Property(x => x.BUserId).HasConversion<string>();
            e.Property(x => x.AName).HasMaxLength(24);
            e.Property(x => x.BName).HasMaxLength(24);
        });

        modelBuilder.Entity<ArenaBet>(e =>
        {
            e.HasKey(x => new { x.WindowStart, x.UserId });
            e.Property(x => x.UserId).HasConversion<string>();
            e.Property(x => x.PlayerId).HasConversion<string>();
            e.HasIndex(x => x.UserId);
            e.HasOne<ArenaBetMatch>().WithMany().HasForeignKey(x => x.WindowStart).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DelveRun>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).HasConversion<string>();
            e.Property(x => x.PlayerId).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.Pending).HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasIndex(x => new { x.UserId, x.Day }).IsUnique();
            e.HasIndex(x => x.WeekStart);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppSetting>(e =>
        {
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(64);
            e.Property(x => x.Value).HasMaxLength(256);
        });
    }
}
