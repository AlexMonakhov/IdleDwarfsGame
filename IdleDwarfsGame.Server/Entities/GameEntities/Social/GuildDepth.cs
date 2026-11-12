// «Гонка глубины» — недельное соревнование гильдий (GuildDepthRules). У каждой гильдии своя шахта на неделю:
// уровень общего подъёмника (Level) ускоряет всех её дворфов. Строка создаётся лениво при первом заходе.
// Ключ (GuildId, WeekStart)
public class GuildDepthShaft
{
    public Guid GuildId { get; set; }
    public DateTime WeekStart { get; set; }
    public int Level { get; set; } = 1;
    // Растёт с каждым улучшением: из двух параллельных улучшений проходит только первое
    public int Version { get; set; }
}

// Дворф участника в шахте гонки. Один на аккаунт за неделю — ключ (WeekStart, UserId): выйти из гильдии и поставить
// второго дворфа в другой нельзя. GuildId — гильдия, где его поставили: на неё он и копает до конца недели.
// Добыча ленивая: Mined и Stored посчитаны на момент UpdatedAt, дальше растут по скорости
// BaseRate × множитель кирки (PickLevel) × множитель подъёмника гильдии.
// Mined — вся руда за неделю (очки гильдии), Stored — руда, ещё не обменянная на золото (склад ограничен)
public class GuildDepthDwarf
{
    public DateTime WeekStart { get; set; }
    public Guid UserId { get; set; }
    public Guid GuildId { get; set; }
    public Guid PlayerId { get; set; }

    // null — дворфа отозвали: вклад остаётся, добыча стоит
    public Guid? HeroId { get; set; }
    // Руда в час без улучшений (CommunityMineRules.DwarfRate на момент постановки)
    public double BaseRate { get; set; }
    public int PickLevel { get; set; } = 1;

    public double Mined { get; set; }
    public double Stored { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Когда дворф освобождается сам (конец гонки) — по нему проверка «занят ли герой»
    public DateTime ReleaseAt { get; set; }

    public int Version { get; set; }
}

// Полученная награда гонки. Ключ (UserId, WeekStart): одна на аккаунт за неделю
public class GuildDepthRewardClaim
{
    public Guid UserId { get; set; }
    public DateTime WeekStart { get; set; }
    public Guid GuildId { get; set; }
    public int Place { get; set; }
    public int HonorBadges { get; set; }
    public DateTime ClaimedAt { get; set; }
}
