// Предмет игрока вместе с описанием из каталога
public class EquipmentInfo
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public EquipmentType Type { get; set; }
    public Rarity Rarity { get; set; }
    // Пусто — подходит любой стихии
    public List<HeroElement> Elements { get; set; } = new();
    public ItemStats Stats { get; set; } = new();
    public List<ItemEffectInfo> Effects { get; set; } = new();
    // Оба null — предмет в инвентаре
    public Guid? HeroId { get; set; }
    public EquipmentSlot? Slot { get; set; }

    public static EquipmentInfo? From(PlayerEquipment item)
    {
        var definition = EquipmentCatalog.Find(item.ItemKey);
        return definition == null
            ? null
            : new EquipmentInfo
            {
                Id = item.Id,
                Key = definition.Key,
                Type = definition.Type,
                Rarity = definition.Rarity,
                Elements = definition.Elements.ToList(),
                Stats = definition.Stats,
                Effects = definition.Effects.Select(e => e.Info).ToList(),
                HeroId = item.HeroId,
                Slot = item.Slot
            };
    }
}

// Пассивка героя с итоговыми значениями: уровень по рангу плюс кольца
public class PassiveInfo
{
    public HeroPassive Passive { get; set; }
    public int Level { get; set; }
    public int Percent { get; set; }
    public int Turns { get; set; }
    public int Targets { get; set; }
}

// Ответ на смену снаряжения: герои, у которых она что-то изменила, и все предметы игрока
public class EquipmentChangeResult
{
    public List<HeroInfo> Heroes { get; set; } = new();
    public List<EquipmentInfo> Items { get; set; } = new();
}
