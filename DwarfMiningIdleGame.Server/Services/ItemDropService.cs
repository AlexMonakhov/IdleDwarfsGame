using System;
using System.Linq;
using DwarfMiningIdleGame.Server.Enums;

public class ItemDropService(IPlayerChestRepository tableRepository, IItemFactory itemFactory) : IItemDropService
{
    private readonly Random random = new();

    public async Task<IReadOnlyCollection<ILootItem>> OpenChestAsync(int chestLevel, ChestType chestType)
    {
        var table = await tableRepository.GetChestDropTableByLevelAndTypeAsync(chestLevel, chestType);
        
        if (table == null)
            throw new InvalidOperationException($"Chest drop table for level {chestLevel} and type {chestType} not found.");
        var result = new List<ILootItem>();

        // Группируем entries по ItemType
        var groupedByType = table.Entries.GroupBy(e => e.ItemType).ToList();

        // Для каждого типа предмета выбираем по одному entry
        foreach (var typeGroup in groupedByType)
        {
            var selectedEntry = SelectEntryByProbability(typeGroup.ToList());

            if (selectedEntry != null)
            {
                int amount = random.Next(selectedEntry.MinAmount, selectedEntry.MaxAmount + 1);

                for (int i = 0; i < amount; i++)
                {
                    var item = itemFactory.Create(
                        selectedEntry.ItemType,
                        selectedEntry.Rarity,
                        selectedEntry.ItemLevel,
                        selectedEntry.SubType);

                    result.Add(item);
                }
            }
        }

        return result;
    }


    private DropEntry? SelectEntryByProbability(IReadOnlyCollection<DropEntry> entries)
    {
        if (!entries.Any())
            return null;

        // Вычисляем сумму всех вероятностей
        double totalProbability = entries.Sum(e => e.Probability);

        // Если сумма вероятностей не равна 1.0, нормализуем
        bool needsNormalization = Math.Abs(totalProbability - 1.0) > 0.001;

        // Группируем entries по вероятности
        var groupedByProbability = entries
            .GroupBy(e => e.Probability)
            .OrderByDescending(g => g.Key)
            .ToList();

        // Создаём список групп с нормализованными вероятностями
        var probabilityGroups = groupedByProbability
            .Select(g => new
            {
                Probability = g.Key,
                NormalizedProb = needsNormalization ? g.Key / totalProbability : g.Key,
                Entries = g.ToList()
            })
            .ToList();

        // Метод "колеса фортуны": выбираем группу вероятности
        double roll = random.NextDouble();
        double cumulative = 0;

        foreach (var group in probabilityGroups)
        {
            cumulative += group.NormalizedProb;
            if (roll <= cumulative)
            {
                // Группа выбрана! Выбираем случайный entry из этой группы
                return group.Entries[random.Next(group.Entries.Count)];
            }
        }

        // Гарантия: если ничего не выбралось, выбираем случайный entry
        // из первой (самой вероятной) группы
        var firstGroup = probabilityGroups.First();
        return firstGroup.Entries[random.Next(firstGroup.Entries.Count)];
    }
}





