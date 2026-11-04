// Сундуки, предметы и награды
// (IdleDwarfsGame.Server/Controllers/Responses/InventoryResponses.cs, RewardResponses.cs)
import { HeroElement } from '@/battle/types';
import { EquipmentInfo } from '@/models/types/Equipment';
import { PlayerResources } from '@/models/types/Expeditions';
import { HeroInfo } from '@/models/types/Heroes';

export type ChestType = 'Hero' | 'Gem' | 'Infinity' | 'Equipment';
export type ItemType = 'Equipment' | 'Material' | 'Gem' | 'Hero';
export type Rarity = 'Common' | 'Uncommon' | 'Rare' | 'Mythic' | 'Legendary' | 'Artifact';

export interface ChestInfo {
    id: string;
    type: ChestType;
    level: number;
    // Стихия сундука бесконечности; 'Basic' — сундук без стихии
    element: HeroElement;
}

// Стопка предметов: в инвентаре quantity — сколько всего, в открытом сундуке — сколько выпало
export interface ItemInfo {
    itemType: ItemType;
    // Для гемов — камень: 'Ruby', 'Sapphire', 'Topaz', 'Emerald', 'Diamond', 'Amethyst', 'Garnet'
    subType: string;
    rarity: Rarity;
    level: number;
    quantity: number;
}

export interface InventoryInfo {
    // Закрытые сундуки
    chests: ChestInfo[];
    items: ItemInfo[];
}

export interface ChestOpenResult {
    chest: ChestInfo;
    items: ItemInfo[];
    heroes: HeroInfo[];
    equipment: EquipmentInfo[];
}

export interface RewardChestInfo {
    type: ChestType;
    level: number;
    element: HeroElement;
}

export interface RewardInfo {
    gold: number;
    experience: number;
    chests: RewardChestInfo[];
    // Предметы сразу в инвентарь (камни башни)
    items: ItemInfo[];
}

export interface RewardClaimResult {
    reward: RewardInfo;
    // Ресурсы аккаунта уже с наградой
    resources: PlayerResources;
}
