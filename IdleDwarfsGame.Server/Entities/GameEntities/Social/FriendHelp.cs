// Помощь другу в шахте: «постучать киркой» — склад его шахты сразу пополняется (FriendHelpRules.HelpHours работы).
// Ключ (HelperUserId, TargetUserId, Day): одному другу — одна помощь в сутки (00:00 UTC)
public class FriendMineHelp
{
    public Guid HelperUserId { get; set; }
    public Guid TargetUserId { get; set; }
    public DateTime Day { get; set; }
    public Guid MineId { get; set; }
    // Сколько ресурса добавилось на склад
    public double Amount { get; set; }
    public long HelperGold { get; set; }
    public DateTime HelpedAt { get; set; }
}

// Наёмник: герой из отряда друга, взятый на один бой кампании. Ключ (BorrowerUserId, LenderUserId, Day):
// у одного друга — один наёмник в сутки. LenderRewarded — получил ли друг печати старейшин (их число в сутки ограничено)
public class FriendHire
{
    public Guid BorrowerUserId { get; set; }
    public Guid LenderUserId { get; set; }
    public DateTime Day { get; set; }
    public Guid HeroId { get; set; }
    public string HeroName { get; set; } = string.Empty;
    public int StageId { get; set; }
    public bool LenderRewarded { get; set; }
    public DateTime HiredAt { get; set; }
}

// «Бой дня» на арене: два сильнейших отряда сервера (снимки на начало окна), на исход ставят кровавые монеты.
// Ключ — начало окна (ArenaBetRules.WindowOf). Бой проводится после конца окна с сидом Seed — его можно пересмотреть
public class ArenaBetMatch
{
    public DateTime WindowStart { get; set; }
    public Guid AUserId { get; set; }
    public Guid BUserId { get; set; }
    public string AName { get; set; } = string.Empty;
    public string BName { get; set; } = string.Empty;
    // Отряды — JSON героев, как у рейтинговой арены (RankedSquad.Serialize)
    public string ASquadJson { get; set; } = "[]";
    public string BSquadJson { get; set; } = "[]";
    public int APower { get; set; }
    public int BPower { get; set; }
    public int Seed { get; set; }

    // 1 — победил A, 2 — B; null — бой ещё не проведён
    public int? Winner { get; set; }
    public int Rounds { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

// Ставка игрока на «Бой дня». Ключ (WindowStart, UserId): одна ставка на бой
public class ArenaBet
{
    public DateTime WindowStart { get; set; }
    public Guid UserId { get; set; }
    public Guid PlayerId { get; set; }
    // 1 — на A, 2 — на B
    public int Side { get; set; }
    public long Amount { get; set; }
    public DateTime PlacedAt { get; set; }
    // Выплата после боя (0 — проиграла); null — ещё не забрана
    public long? Payout { get; set; }
    public DateTime? ClaimedAt { get; set; }
}
