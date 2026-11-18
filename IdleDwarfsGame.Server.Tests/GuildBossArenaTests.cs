using System;
using System.Linq;
using Xunit;

// Эффект арены по боссу с запасом HP (босс гильдии, мировой босс) считается от его макс. HP по статам, а не от запаса:
// иначе Regen в Crystal Sanctum (3%) лечил сложного босса на 225 000 HP за раунд и съедал почти весь урон отряда
public class GuildBossArenaTests
{
    private static readonly EntityFactory Factory = new();

    public static TheoryData<GuildBossTier, int> Variants()
    {
        var data = new TheoryData<GuildBossTier, int>();
        foreach (var definition in GuildBosses.All)
            for (int i = 0; i < definition.Variants.Length; i++)
                data.Add(definition.Tier, i);
        return data;
    }

    private static (World World, int BossId, ArenaStatusEffect Arena) Fight(GuildBossTier tier, int variantIndex, int currentHp)
    {
        var definition = GuildBosses.Get(tier);
        var variant = definition.Variants[variantIndex];
        var template = GuildBossSeedData.Heroes.First(h => h.Id == variant.TemplateHeroId);
        var world = new World();
        Factory.CreateSquad(world, BalanceSimulation.Squad(3, 25), teamId: 1);
        int bossId = new GuildBossScenario(Factory, template, definition, currentHp).SpawnBoss(world);
        var arena = ArenaStatusEffect.FromArena(GuildBosses.ArenaOf(variant))!;
        world.AddGlobalEffect(arena);
        return (world, bossId, arena);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ArenaPower_OnBoss_IsPercentOfNominalHp(GuildBossTier tier, int variantIndex)
    {
        var definition = GuildBosses.Get(tier);
        var (world, bossId, arena) = Fight(tier, variantIndex, definition.MaxHp);
        var hp = world.Health[bossId];

        Assert.Equal(definition.MaxHp, hp.Max);
        Assert.NotNull(hp.NominalMax);
        Assert.True(hp.NominalMax < definition.MaxHp / 10);
        if (arena.EffectName != "Frost")
            Assert.Equal(Math.Max(1, (int)Math.Round(hp.NominalMax!.Value * arena.Power / 100.0, MidpointRounding.AwayFromZero)),
                arena.PowerFor(world, bossId));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void BossHpLoss_StaysCloseToSquadDamage(GuildBossTier tier, int variantIndex)
    {
        var definition = GuildBosses.Get(tier);
        // Босса уже подбили: лечение арены не должно поднимать его выше, чем он вышел, и съедать урон отряда
        int start = definition.MaxHp - definition.MaxHp / 10;
        var (world, bossId, arena) = Fight(tier, variantIndex, start);

        var context = new CombatEngine(new Random(3)).Run(world, GuildBosses.FightRounds);
        var stats = context.Stats.Build();
        int squadDamage = stats.Where(s => s.Team == 1).Sum(s => s.DamageDealt);
        int lost = start - world.Health[bossId].Current;
        // Арена за бой — не больше раунда-двух ударов отряда в любую сторону
        int arenaPerFight = arena.PowerFor(world, bossId) * AuraComponent.StackLimit * GuildBosses.FightRounds;

        Assert.True(squadDamage > 0);
        Assert.InRange(lost, squadDamage - arenaPerFight, squadDamage + arenaPerFight);
        Assert.True(arenaPerFight < squadDamage, $"arena {arenaPerFight} vs squad {squadDamage}");
    }
}
