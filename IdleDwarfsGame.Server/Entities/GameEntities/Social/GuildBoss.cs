public enum GuildBossTier { Easy, Medium, Hard }

// Босс гильдии на сутки (Day — 00:00 UTC). HP общее на всю гильдию: каждый удар участника отнимает от CurrentHp.
// Строки создаются лениво при первом заходе в сутки; новый день — новые строки с полным HP
public class GuildBoss
{
    public Guid Id { get; set; }
    public Guid GuildId { get; set; }
    public GuildBossTier Tier { get; set; }
    public DateTime Day { get; set; }

    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }

    // Кто и когда добил; null — босс ещё жив
    public DateTime? DefeatedAt { get; set; }
    public Guid? KilledByUserId { get; set; }
}

// Удар участника по боссу — для вклада в урон
public class GuildBossAttack
{
    public Guid Id { get; set; }
    public Guid BossId { get; set; }
    public Guid UserId { get; set; }
    public int Damage { get; set; }
    public DateTime AttackedAt { get; set; }
}

// Когда игрок снова может бить босса этого уровня. Привязано к аккаунту, а не к гильдии:
// переход в другую гильдию откат не сбрасывает. Ключ (UserId, Tier)
public class GuildBossCooldown
{
    public Guid UserId { get; set; }
    public GuildBossTier Tier { get; set; }
    public DateTime ReadyAt { get; set; }
}

// Полученная награда за убитого босса. Ключ (UserId, Tier, Day): за босса каждого уровня — одна награда в сутки
// на аккаунт, в какой бы гильдии он ни был. Поэтому нет внешнего ключа на босса: при роспуске гильдии
// боссы удаляются, а отметка о награде должна остаться
public class GuildBossRewardClaim
{
    public Guid UserId { get; set; }
    public GuildBossTier Tier { get; set; }
    public DateTime Day { get; set; }

    public Guid GuildId { get; set; }
    public Guid BossId { get; set; }
    public Guid RewardId { get; set; }
    public DateTime ClaimedAt { get; set; }
}
