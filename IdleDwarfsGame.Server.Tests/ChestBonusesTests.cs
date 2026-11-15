using System;
using System.Linq;
using IdleDwarfsGame.Server.Enums;
using Xunit;

public class ChestBonusesTests
{
    private static readonly PlayerProgress Player = new(99, Stage(100), false);

    [Fact]
    public void EveryTable_GivesOneToFiveMainCards()
    {
        // Карт открытия всегда пять: основных не больше пяти и хотя бы одна (иначе сундук «пустой»)
        foreach (var table in LootSeedData.TablesWithEntries())
        {
            var groups = table.Entries.GroupBy(e => e.ItemType).ToList();
            int max = table.Rolls * groups.Count;
            bool guaranteed = groups.Any(g => g.Sum(e => e.Probability) >= 1 - 1e-9);

            Assert.True(max <= ChestBonuses.CardCount, $"{table.ChestType} {table.ChestLevel} {table.Element}: up to {max} main cards");
            Assert.True(guaranteed, $"{table.ChestType} {table.ChestLevel} {table.Element}: can drop nothing");
        }
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 3)]
    [InlineData(5, 0)]
    [InlineData(7, 0)]
    public void Bonuses_FillUpToFiveCards(int mainCards, int bonuses)
    {
        Assert.Equal(bonuses, ChestBonuses.CountFor(mainCards));
        Assert.Equal(bonuses, ChestBonuses.Roll(bonuses, 3, Player, eventActive: true, new Random(1)).Count);
    }

    [Fact]
    public void RareCurrencies_OnlyInHighLevelChests_AndEventCurrencyOnlyDuringEvent()
    {
        var random = new Random(7);
        for (int level = 1; level < ChestBonuses.RareCurrencyMinLevel; level++)
        {
            var low = ChestBonuses.Roll(5_000, level, Player, eventActive: true, random);
            Assert.DoesNotContain(low, b => b.Kind is ChestBonusKind.RainbowCoins or ChestBonusKind.EventCurrency);
        }

        var noEvent = ChestBonuses.Roll(5_000, 5, Player, eventActive: false, random);
        Assert.DoesNotContain(noEvent, b => b.Kind == ChestBonusKind.EventCurrency);
        Assert.Contains(noEvent, b => b.Kind == ChestBonusKind.RainbowCoins);

        var withEvent = ChestBonuses.Roll(5_000, 5, Player, eventActive: true, random);
        Assert.Contains(withEvent, b => b.Kind == ChestBonusKind.EventCurrency);
    }

    [Fact]
    public void RainbowCoins_AreRare()
    {
        const int iterations = 100_000;
        var bonuses = ChestBonuses.Roll(iterations, 5, Player, eventActive: false, new Random(3));
        double share = bonuses.Count(b => b.Kind == ChestBonusKind.RainbowCoins) / (double)iterations;
        Assert.InRange(share, ChestBonuses.RainbowCoinChance[5] - 0.005, ChestBonuses.RainbowCoinChance[5] + 0.005);
        Assert.All(bonuses.Where(b => b.Kind == ChestBonusKind.RainbowCoins), b => Assert.InRange(b.Amount, 1, 2));
    }

    [Fact]
    public void GoldAndExperience_ScaleWithThePlayersStage()
    {
        var early = new PlayerProgress(9, Stage(10), false);
        var late = new PlayerProgress(299, Stage(300), false);

        double AverageGold(PlayerProgress player) => ChestBonuses.Roll(20_000, 3, player, false, new Random(11))
            .Where(b => b.Kind == ChestBonusKind.Gold).Average(b => b.Amount);

        Assert.True(AverageGold(late) > AverageGold(early) * 10);
        // Одна бонусная карта — около «уровня кампании» (0.6 + 0.2 × уровень сундука, ±40%, иногда джекпот)
        double stageGold = CampaignRewards.BaseFor(Stage(300)).Gold;
        Assert.InRange(AverageGold(late), stageGold * 0.9, stageGold * 1.6);
    }

    [Fact]
    public void WithoutCampaignStage_GoldTurnsIntoGems()
    {
        var bonuses = ChestBonuses.Roll(2_000, 2, new PlayerProgress(0, null, false), false, new Random(5));
        Assert.All(bonuses, b => Assert.Equal(ChestBonusKind.Gems, b.Kind));
        Assert.All(bonuses, b => Assert.True(Enum.TryParse<GemType>(b.SubType, out _)));
    }

    private static CampaignStage Stage(int id) =>
        new() { Id = id, Type = StageType.Mobs, EnemyLevel = id <= CampaignGenerator.HandcraftedStages ? id : CampaignGenerator.EnemyLevelFor(id) };
}
