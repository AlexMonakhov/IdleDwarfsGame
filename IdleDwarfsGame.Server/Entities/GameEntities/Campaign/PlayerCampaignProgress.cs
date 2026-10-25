// Пройденный игроком уровень кампании. Одна строка на (игрок, уровень) — первая победа.
public class PlayerCampaignProgress
{
    public Guid PlayerId { get; set; }
    public int StageId { get; set; }
    public DateTime ClearedAt { get; set; }
}
