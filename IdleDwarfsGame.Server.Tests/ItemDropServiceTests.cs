using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using IdleDwarfsGame.Server.Enums;

public class ItemDropServiceTests
{
    private const int Iterations = 100_000;
    // Допуск на частоту при 100 000 бросков (σ ≈ 0.0016 для p = 0.4)
    private const double Tolerance = 0.01;

    private readonly ITestOutputHelper _output;

    public ItemDropServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Pick_EntriesWithEqualProbability_EachGetTheirOwnChance()
    {
        // Раньше записи с одинаковым шансом склеивались в одну группу: три записи по 5% делили 5% на троих,
        // а недобранные броски уходили самой частой записи
        var entries = new List<DropEntry>
        {
            Gem(GemType.Ruby, 0.40),
            Gem(GemType.Sapphire, 0.30),
            Gem(GemType.Topaz, 0.15),
            Gem(GemType.Emerald, 0.05),
            Gem(GemType.Diamond, 0.05),
            Gem(GemType.Amethyst, 0.05)
        };

        var frequency = PickFrequencies(entries);

        foreach (var entry in entries)
            Assert.InRange(frequency[entry.SubType], entry.Probability - Tolerance, entry.Probability + Tolerance);
    }

    [Fact]
    public void Pick_RemainderUpToOne_IsEmptyRoll()
    {
        var entries = new List<DropEntry> { Gem(GemType.Ruby, 0.25), Gem(GemType.Diamond, 0.05) };

        var frequency = PickFrequencies(entries);

        Assert.InRange(frequency[nameof(GemType.Ruby)], 0.25 - Tolerance, 0.25 + Tolerance);
        Assert.InRange(frequency[nameof(GemType.Diamond)], 0.05 - Tolerance, 0.05 + Tolerance);
        Assert.InRange(frequency[""], 0.70 - Tolerance, 0.70 + Tolerance);
    }

    [Fact]
    public void Pick_SumAboveOne_IsNormalized()
    {
        var entries = new List<DropEntry> { Gem(GemType.Ruby, 1.5), Gem(GemType.Sapphire, 0.5) };

        var frequency = PickFrequencies(entries);

        Assert.InRange(frequency[nameof(GemType.Ruby)], 0.75 - Tolerance, 0.75 + Tolerance);
        Assert.False(frequency.ContainsKey(""));
        Assert.Equal(entries[1], ItemDropService.Pick(entries, 0.9999999999));
    }

    [Fact]
    public void Roll_EachItemTypeGroupRollsSeparately_AndSameDropsStack()
    {
        var table = new ChestDropTable
        {
            Rolls = 3,
            Entries =
            [
                Gem(GemType.Ruby, 1, min: 2, max: 2),
                new DropEntry { ItemType = ItemType.Hero, ItemLevel = 2, Rarity = Rarity.Uncommon, Probability = 1, MinAmount = 1, MaxAmount = 1 }
            ]
        };

        var drops = new ItemDropService(new Random(1)).Roll(table);

        Assert.Equal(2, drops.Count);
        Assert.Equal(6, drops.Single(d => d.ItemType == ItemType.Gem).Amount);
        Assert.Equal(3, drops.Single(d => d.ItemType == ItemType.Hero).Amount);
    }

    [Fact]
    public void SeededTables_AreValid_AndCoverEveryChestTheCampaignGives()
    {
        var tables = LootSeedData.TablesWithEntries();

        foreach (var type in new[] { ChestType.Gem, ChestType.Hero })
        {
            for (int level = 1; level <= LootSeedData.MaxChestLevel; level++)
                Assert.Single(tables, t => t.ChestType == type && t.ChestLevel == level);
        }
        Assert.Equal(LootSeedData.MaxChestLevel, CampaignRewards.ChestLevelFor(1000));

        foreach (var table in tables)
        {
            Assert.True(table.Rolls >= 1);
            foreach (var group in table.Entries.GroupBy(e => e.ItemType))
            {
                // Шансы задуманы с остатком «ничего»: больше 1 — значит, где-то опечатка
                Assert.True(group.Sum(e => e.Probability) <= 1 + 1e-9, $"{table.ChestType} {table.ChestLevel}: sum > 1");
            }

            foreach (var entry in table.Entries)
            {
                Assert.True(entry.Probability > 0);
                Assert.InRange(entry.MinAmount, 1, entry.MaxAmount);
                if (entry.ItemType == ItemType.Gem)
                    Assert.True(Enum.TryParse<GemType>(entry.SubType, out _), $"Unknown gem {entry.SubType}");
                if (entry.ItemType == ItemType.Hero)
                    Assert.InRange(entry.ItemLevel, HeroLevels.Min, HeroLevels.Max);
            }
        }
    }

    [Fact]
    public void SeededTables_DropStatistics()
    {
        // Не проверка, а справка для балансировки: сколько в среднем даёт сундук
        var service = new ItemDropService(new Random(7));
        const int chests = 10_000;

        foreach (var table in LootSeedData.TablesWithEntries())
        {
            var totals = new Dictionary<string, int>();
            for (int i = 0; i < chests; i++)
            {
                foreach (var drop in service.Roll(table))
                {
                    string key = drop.ItemType == ItemType.Hero
                        ? $"Hero rank {drop.Level}"
                        : $"{drop.Rarity} {drop.SubType}";
                    totals[key] = totals.GetValueOrDefault(key) + drop.Amount;
                }
            }

            _output.WriteLine($"{table.ChestType} chest lvl {table.ChestLevel} (rolls {table.Rolls}), per chest:");
            foreach (var (key, total) in totals.OrderByDescending(t => t.Value))
                _output.WriteLine($"   {key}: {total / (double)chests:F2}");
        }
    }

    private static Dictionary<string, double> PickFrequencies(List<DropEntry> entries)
    {
        var random = new Random(42);
        var counts = new Dictionary<string, int>();
        for (int i = 0; i < Iterations; i++)
        {
            // Пустой бросок считаем под ключом ""
            string key = ItemDropService.Pick(entries, random.NextDouble())?.SubType ?? "";
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
        return counts.ToDictionary(c => c.Key, c => c.Value / (double)Iterations);
    }

    private static DropEntry Gem(GemType gem, double probability, int min = 1, int max = 1) => new()
    {
        ItemType = ItemType.Gem,
        SubType = gem.ToString(),
        Rarity = Rarity.Common,
        ItemLevel = 1,
        Probability = probability,
        MinAmount = min,
        MaxAmount = max
    };
}
