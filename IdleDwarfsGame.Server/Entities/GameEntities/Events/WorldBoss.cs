// Мировой босс ивента «Мировые боссы»: один на весь сервер. Убили — при следующем заходе появляется следующий этап,
// сильнее и с большим запасом HP (WorldBossRules). Ключ этапа — (EventStart, Stage): двух боссов одного этапа не бывает
public class WorldBoss
{
    public Guid Id { get; set; }
    public DateTime EventStart { get; set; }
    // Номер босса в ивенте, с 1
    public int Stage { get; set; }

    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public DateTime SpawnedAt { get; set; }

    // Кто и когда добил; null — босс ещё жив
    public DateTime? DefeatedAt { get; set; }
    public Guid? KilledByPlayerId { get; set; }
}

// Вклад игрока в босса: урон и число ударов. Ключ (BossId, PlayerId). Доля в награде за убийство — Damage / MaxHp
public class WorldBossContribution
{
    public Guid BossId { get; set; }
    public Guid PlayerId { get; set; }
    public long Damage { get; set; }
    public int Attacks { get; set; }
    public DateTime LastAttackAt { get; set; }
}

// Когда игрок снова может бить мирового босса
public class WorldBossCooldown
{
    public Guid PlayerId { get; set; }
    public DateTime ReadyAt { get; set; }
}

// Полученная награда за убитого босса. Ключ (BossId, PlayerId): за каждого босса — одна награда
public class WorldBossRewardClaim
{
    public Guid BossId { get; set; }
    public Guid PlayerId { get; set; }
    public int Currency { get; set; }
    public DateTime ClaimedAt { get; set; }
}
