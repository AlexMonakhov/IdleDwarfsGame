using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using DwarfMiningIdleGame.Server.Enums;

public class ItemDropServiceTests
{
    [Fact]
    public async Task OpenChest_1000Times_AnalyzeDropsByProbability()
    {
        // Arrange
        const int chestLevel = 1;
        const ChestType chestType = ChestType.Hero;
        const int iterations = 1000;

        // Create mock for IItemFactory
        var itemFactoryMock = new Mock<IItemFactory>();
        itemFactoryMock
            .Setup(f => f.Create(It.IsAny<ItemType>(), It.IsAny<Rarity>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns((ItemType type, Rarity rarity, int level, string subType) =>
            {
                var mock = new Mock<ILootItem>();
                mock.Setup(l => l.ItemType).Returns(type);
                mock.Setup(l => l.Rarity).Returns(rarity);
                mock.Setup(l => l.Level).Returns(level);
                mock.Setup(l => l.SubType).Returns(subType);
                return mock.Object;
            });

        // Create test DropEntry for Equipment - matching the controller
        var equipmentEntries = new List<DropEntry>
        {
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = nameof(EquipmentSubType.Weapon),
                Rarity = Rarity.Common,
                ItemLevel = 1,
                Probability = 0.40,
                MinAmount = 1,
                MaxAmount = 1
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = nameof(EquipmentSubType.Armor),
                Rarity = Rarity.Common,
                ItemLevel = 1,
                Probability = 0.30,
                MinAmount = 1,
                MaxAmount = 1
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = nameof(EquipmentSubType.Armor),
                Rarity = Rarity.Legendary,
                ItemLevel = 1,
                Probability = 0.15,
                MinAmount = 1,
                MaxAmount = 1
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = nameof(EquipmentSubType.Accessory),
                Rarity = Rarity.Uncommon,
                ItemLevel = 1,
                Probability = 0.05,
                MinAmount = 1,
                MaxAmount = 1
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = nameof(EquipmentSubType.Armor),
                Rarity = Rarity.Uncommon,
                ItemLevel = 1,
                Probability = 0.05,
                MinAmount = 1,
                MaxAmount = 1
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = nameof(EquipmentSubType.Weapon),
                Rarity = Rarity.Uncommon,
                ItemLevel = 1,
                Probability = 0.05,
                MinAmount = 1,
                MaxAmount = 1
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = nameof(EquipmentSubType.Armor),
                Rarity = Rarity.Rare,
                ItemLevel = 1,
                Probability = 0.02,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        // Create test DropEntry for Crystals - matching the controller
        var crystalEntries = new List<DropEntry>
        {
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Crystal,
                SubType = nameof(CrystalSubType.Ruby),
                Rarity = Rarity.Common,
                ItemLevel = 1,
                Probability = 0.60,
                MinAmount = 2,
                MaxAmount = 5
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Crystal,
                SubType = nameof(CrystalSubType.Emerald),
                Rarity = Rarity.Uncommon,
                ItemLevel = 1,
                Probability = 0.25,
                MinAmount = 1,
                MaxAmount = 2
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Crystal,
                SubType = nameof(CrystalSubType.Sapphire),
                Rarity = Rarity.Rare,
                ItemLevel = 1,
                Probability = 0.07,
                MinAmount = 1,
                MaxAmount = 1
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Crystal,
                SubType = nameof(CrystalSubType.Diamond),
                Rarity = Rarity.Rare,
                ItemLevel = 1,
                Probability = 0.07,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        var allEntries = equipmentEntries;

        // Create ChestDropTable
        var dropTable = new ChestDropTable
        {
            Id = Guid.NewGuid(),
            ChestLevel = chestLevel,
            ChestType = chestType,
            Entries = allEntries
        };

        // Create mock for IPlayerChestRepository
        var repositoryMock = new Mock<IPlayerChestRepository>();
        repositoryMock
            .Setup(r => r.GetChestDropTableByLevelAndTypeAsync(chestLevel, chestType))
            .ReturnsAsync(dropTable);

        var itemDropService = new ItemDropService(repositoryMock.Object, itemFactoryMock.Object);

        // Act - Track drops by probability
        var dropsByProbability = new Dictionary<double, Dictionary<string, int>>();
        
        // Initialize the dictionary with all unique probabilities
        foreach (var entry in allEntries)
        {
            if (!dropsByProbability.ContainsKey(entry.Probability))
            {
                dropsByProbability[entry.Probability] = new Dictionary<string, int>();
            }
        }

        for (int i = 0; i < iterations; i++)
        {
            var lootItems = await itemDropService.OpenChestAsync(chestLevel, chestType);
            
            foreach (var item in lootItems)
            {
                // Find which entry this item came from
                var matchingEntry = allEntries.FirstOrDefault(e => 
                    e.ItemType == item.ItemType && 
                    e.SubType == item.SubType && 
                    e.Rarity == item.Rarity);

                if (matchingEntry != null)
                {
                    double prob = matchingEntry.Probability;
                    string key = $"{item.ItemType}_{item.SubType}_{item.Rarity}";

                    if (!dropsByProbability[prob].ContainsKey(key))
                    {
                        dropsByProbability[prob][key] = 0;
                    }

                    dropsByProbability[prob][key]++;
                }
            }
        }

        // Output results grouped by probability
        var output = $"""
            ===== DROP STATISTICS BY PROBABILITY (1000 CHESTS) =====
            Total chests opened: {iterations}
            Note: If 5 entries have 0.05 probability, when 5% triggers,
            one random entry from these 5 is selected. Total = 5% of all drops.
            
            """;

        foreach (var probGroup in dropsByProbability.OrderByDescending(x => x.Key))
        {
            double probability = probGroup.Key;
            var items = probGroup.Value;
            var totalDropsForProb = items.Values.Sum();
            var expectedCount = iterations * probability;

            output += $"📊 Probability: {probability * 100:F1}%\n";
            output += $"   Number of entries with this probability: {items.Count}\n";
            output += $"   Total drops: {totalDropsForProb}\n";
            output += $"   Expected: ~{expectedCount:F0} drops\n";
            output += $"   Per entry average: ~{expectedCount / items.Count:F1} drops\n";

            foreach (var item in items.OrderByDescending(x => x.Value))
            {
                var percentage = totalDropsForProb > 0 
                    ? (double)item.Value / totalDropsForProb * 100 
                    : 0;
                output += $"   ├─ {item.Key}: {item.Value} ({percentage:F2}% of this probability group)\n";
            }

            output += "\n";
        }

        // Output results
        System.Console.WriteLine(output);

        // Basic assertions
        Assert.True(dropsByProbability.Count > 0, "Should have drops from different probabilities");
        Assert.True(dropsByProbability.Values.Sum(x => x.Values.Sum()) > 0, "Should have total drops");
    }

    [Fact]
    public async Task OpenChest_1000Times_DetailedEquipmentAnalysis()
    {
        // Arrange
        const int chestLevel = 1;
        const ChestType chestType = ChestType.Hero;
        const int iterations = 1000;

        var itemFactoryMock = new Mock<IItemFactory>();
        itemFactoryMock
            .Setup(f => f.Create(It.IsAny<ItemType>(), It.IsAny<Rarity>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns((ItemType type, Rarity rarity, int level, string subType) =>
            {
                var mock = new Mock<ILootItem>();
                mock.Setup(l => l.ItemType).Returns(type);
                mock.Setup(l => l.Rarity).Returns(rarity);
                mock.Setup(l => l.Level).Returns(level);
                mock.Setup(l => l.SubType).Returns(subType);
                return mock.Object;
            });

        var equipmentEntries = new[]
        {
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = "Armor",
                Rarity = Rarity.Common,
                ItemLevel = 1,
                Probability = 0.4,
                MinAmount = 1,
                MaxAmount = 2
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = "Weapon",
                Rarity = Rarity.Uncommon,
                ItemLevel = 1,
                Probability = 0.35,
                MinAmount = 1,
                MaxAmount = 1
            },
            new DropEntry
            {
                Id = Guid.NewGuid(),
                ItemType = ItemType.Equipment,
                SubType = "Accessory",
                Rarity = Rarity.Rare,
                ItemLevel = 1,
                Probability = 0.25,
                MinAmount = 1,
                MaxAmount = 3
            }
        };

        var dropTable = new ChestDropTable
        {
            Id = Guid.NewGuid(),
            ChestLevel = chestLevel,
            ChestType = chestType,
            Entries = equipmentEntries
        };

        // Create mock for IPlayerChestRepository
        var repositoryMock = new Mock<IPlayerChestRepository>();
        repositoryMock
            .Setup(r => r.GetChestDropTableByLevelAndTypeAsync(chestLevel, chestType))
            .ReturnsAsync(dropTable);

        var itemDropService = new ItemDropService(repositoryMock.Object, itemFactoryMock.Object);

        // Act
        var equipmentDropsByTypeAndRarity = new Dictionary<(string subType, Rarity rarity), int>();
        int totalChests = 0;

        for (int i = 0; i < iterations; i++)
        {
            totalChests++;
            var lootItems = await itemDropService.OpenChestAsync(chestLevel, chestType);
            
            foreach (var item in lootItems)
            {
                if (item.ItemType == ItemType.Equipment)
                {
                    var key = (item.SubType, item.Rarity);
                    if (!equipmentDropsByTypeAndRarity.ContainsKey(key))
                    {
                        equipmentDropsByTypeAndRarity[key] = 0;
                    }
                    equipmentDropsByTypeAndRarity[key]++;
                }
            }
        }

        // Assert & Output
        var groupedBySubType = equipmentDropsByTypeAndRarity
            .GroupBy(x => x.Key.subType)
            .OrderBy(g => g.Key)
            .ToList();

        var output = $"""
            ===== DETAILED DROP STATISTICS (1000 CHESTS) =====
            Total chests opened: {totalChests}
            
            """;

        foreach (var subTypeGroup in groupedBySubType)
        {
            var subTypeTotal = subTypeGroup.Sum(x => x.Value);
            output += $"📦 {subTypeGroup.Key}:\n";

            foreach (var item in subTypeGroup.OrderBy(x => x.Key.rarity))
            {
                var count = item.Value;
                var percentage = (double)count / subTypeTotal * 100;
                output += $"    ✦ {item.Key.rarity}: {count} ({percentage:F2}%)\n";
            }

            output += "\n";
        }

        System.Console.WriteLine(output);
        Assert.True(equipmentDropsByTypeAndRarity.Count > 0, "Should have Equipment drops");
    }
}
