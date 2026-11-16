// Какое действие игрока значит «попробовал активность»: первое такое действие отмечает её ключ в PlayerTutorial.DoneAt.
// Ключи — те же, что у активностей на фронте (src/tutorial/activities.ts). Активности без своего действия
// (почта, друзья, ивент…) фронт считает освоенными, как только игрок с ними познакомился
public static class TutorialActivities
{
    public static string? KeyFor(ActivityKind kind) => kind switch
    {
        ActivityKind.CampaignWin => "campaign",
        ActivityKind.TowerFight => "tower",
        ActivityKind.PvpFight => "pvp",
        ActivityKind.GuildBossAttack => "guild",
        ActivityKind.MineDig => "mine",
        ActivityKind.ExpeditionStart => "expeditions",
        ActivityKind.ChestOpen => "warehouse",
        ActivityKind.HeroUpgrade => "altar",
        ActivityKind.ForgeFuse => "forge",
        ActivityKind.ShopBuy => "shop",
        _ => null
    };
}
