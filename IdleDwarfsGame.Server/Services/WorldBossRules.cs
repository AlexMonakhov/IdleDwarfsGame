// Правила ивента «Мировые боссы» (WorldEvents). Боссы идут по очереди: этап 1, 2, 3… — пока идёт ивент.
// Внешность и арена — те же варианты, что у боссов гильдии (GuildBosses), по кругу от лёгких к сложным.
// Уровень растёт на LevelPerStage за этап; HP — от числа активных игроков: HpPerActivePlayer на каждого за последние
// сутки, ×HpGrowth за этап. Так одному игроку на свежем сервере босс по силам, а сотне — не на пять минут.
// Запас HP рассчитан на 4–8 ударов от каждого активного игрока: удар отряда середины игры — 10–30 тыс. урона за бой
// (см. GuildBosses: лёгкий босс гильдии теряет ~10 тыс. за попытку отряда 1-го ранга).
//
// Награда — валюта ивента (клыки):
//   за каждый удар — AttackCurrency, кто бы ни победил;
//   за убитого босса каждому, кто его бил, — KillBase(этап) плюс доля SharePool(этап) по урону и LastHitBonus добившему.
// Активный игрок за ивент (6 суток, удар раз в AttackCooldown) набирает ~2 000–3 000 клыков, вместе с заданиями —
// героя ивента и ещё что-то (цены — FixedShops)
public static class WorldBossRules
{
    public const int FightRounds = GuildBosses.FightRounds;

    public static readonly TimeSpan AttackCooldown = TimeSpan.FromMinutes(30);

    public const int BaseLevel = 10;
    public const int LevelPerStage = 3;
    public const int MaxLevel = 50;

    public const int HpPerActivePlayer = 80_000;
    public const double HpGrowth = 1.3;
    public const int MinHp = 50_000;
    // Health в бою — int: запас не должен переполнить его
    public const int MaxHp = 1_000_000_000;

    // Активными считаются игроки, сделавшие хоть что-то за эти сутки (PlayerDailyActivity)
    public static readonly TimeSpan ActiveWindow = TimeSpan.FromDays(1);

    public const int AttackCurrency = 20;
    public const int LastHitBonus = 50;

    public const int LeaderboardSize = 10;

    public static readonly GuildBossVariant[] Variants = GuildBosses.All.SelectMany(d => d.Variants).ToArray();

    public static GuildBossVariant VariantOf(int stage) => Variants[(Math.Max(1, stage) - 1) % Variants.Length];

    public static Arena ArenaOf(int stage) => GuildBosses.ArenaOf(VariantOf(stage));

    public static int LevelOf(int stage) => Math.Min(MaxLevel, BaseLevel + LevelPerStage * (Math.Max(1, stage) - 1));

    public static int HpOf(int stage, int activePlayers)
    {
        double hp = HpPerActivePlayer * (double)Math.Max(1, activePlayers) * Math.Pow(HpGrowth, Math.Max(1, stage) - 1);
        return (int)Math.Clamp(Math.Round(hp), MinHp, MaxHp);
    }

    // Каждому, кто бил убитого босса
    public static int KillBase(int stage) => Math.Min(200, 60 + 10 * (Math.Max(1, stage) - 1));

    // Делится между участниками по урону
    public static int SharePool(int stage) => 300 + 50 * (Math.Max(1, stage) - 1);

    public static int KillReward(int stage, long damage, int maxHp, bool lastHit)
    {
        if (damage <= 0)
            return 0;
        double share = maxHp > 0 ? Math.Clamp((double)damage / maxHp, 0, 1) : 0;
        return KillBase(stage) + (int)Math.Round(SharePool(stage) * share) + (lastHit ? LastHitBonus : 0);
    }
}
