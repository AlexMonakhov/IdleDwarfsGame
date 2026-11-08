// Лавки с постоянным ассортиментом и ивент недели
// (IdleDwarfsGame.Server/Controllers/Responses/FixedShopResponses.cs, правила — Services/FixedShops.cs и WorldEvents.cs)
import type { EquipmentInfo } from '@/models/types/Equipment';
import type { HeroInfo } from '@/models/types/Heroes';
import type { PlayerResources } from '@/models/types/Expeditions';
import type { Rarity, RewardChestInfo, RewardInfo } from '@/models/types/Inventory';
import type { WorldEventKind } from '@/models/types/Quests';

// Elder — лавка старейшин (печати заданий), Event — лавка ивента недели (его валюта)
export type FixedShopKind = 'Elder' | 'Event';
export type FixedRewardKind = 'Hero' | 'Item' | 'Chest' | 'Gold' | 'RainbowCoins';

export interface FixedOfferInfo {
    key: string;
    kind: FixedRewardKind;
    rarity: Rarity;
    price: number;
    limit: number;
    bought: number;
    hero: HeroInfo | null;
    item: EquipmentInfo | null;
    // Сундук — уровня текущей главы кампании
    chest: RewardChestInfo | null;
    gold: number;
    rainbowCoins: number;
}

export interface WorldEventInfo {
    kind: WorldEventKind;
    startsAt: string;
    // Конец активной части: дальше валюта не копится, работает только лавка
    endsAt: string;
    // Конец недели: остаток валюты сгорает, начинается следующий ивент
    shopClosesAt: string;
    active: boolean;
    nextKind: WorldEventKind;
    currency: number;
    earned: number;
}

export interface FixedShopOverview {
    shop: FixedShopKind;
    offers: FixedOfferInfo[];
    currency: number;
    resetAt: string;
    serverTime: string;
    event: WorldEventInfo | null;
    resources: PlayerResources;
}

export interface FixedShopBuyResult {
    shop: FixedShopOverview;
    hero: HeroInfo | null;
    item: EquipmentInfo | null;
    reward: RewardInfo | null;
    rainbowCoins: number;
}

// Коды отказа сервера (FixedShopError) — тексты в fixedShop.errors.<code>
export type FixedShopError =
    | 'UnknownShop'
    | 'OfferNotFound'
    | 'LimitReached'
    | 'NotEnoughCurrency'
    | 'ShopClosed'
    | 'Changed';
