// Кузница (IdleDwarfsGame.Server/Controllers/Responses/ForgeResponses.cs, правила — Services/ForgeService.cs)
import type { EquipmentInfo } from '@/models/types/Equipment';
import type { Rarity } from '@/models/types/Inventory';
import type { ShopGemStack } from '@/models/types/Shop';

export interface ForgeOverview {
    // Только снятые предметы: надетые в кузницу не идут
    items: EquipmentInfo[];
    gems: ShopGemStack[];
    itemFusionSize: number;
    gemFusionSize: number;
    // Выше этой редкости не куётся (из легендарных артефакт не получить)
    maxRarity: Rarity;
}

export interface ForgeItemResult {
    item: EquipmentInfo;
    forge: ForgeOverview;
}

export interface ForgeGemResult {
    gem: string;
    rarity: Rarity;
    created: number;
    forge: ForgeOverview;
}

// Порядок редкостей — как в Rarity на сервере: следующая редкость = следующий элемент
export const RARITY_ORDER: Rarity[] = ['Common', 'Uncommon', 'Rare', 'Mythic', 'Legendary', 'Artifact'];

export const nextRarity = (rarity: Rarity): Rarity | null => RARITY_ORDER[RARITY_ORDER.indexOf(rarity) + 1] ?? null;

// Можно ли ковать из этой редкости: результат не выше maxRarity
export const canForgeFrom = (rarity: Rarity, maxRarity: Rarity) =>
    RARITY_ORDER.indexOf(rarity) < RARITY_ORDER.indexOf(maxRarity);
