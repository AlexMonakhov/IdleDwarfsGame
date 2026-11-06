// Магазины (IdleDwarfsGame.Server/Controllers/Responses/ShopResponses.cs, правила — Services/ShopRules.cs)
import type { EquipmentInfo } from '@/models/types/Equipment';
import type { HeroInfo } from '@/models/types/Heroes';
import type { Rarity } from '@/models/types/Inventory';
import type { PlayerResources } from '@/models/types/Expeditions';

// Market — рынок (золото и камни), Pvp — подпольный рынок (кровавые монеты), Guild — лавка гильдии (значки почёта)
export type ShopKind = 'Market' | 'Pvp' | 'Guild';
export type ShopOfferKind = 'Item' | 'Hero';

// Gold и камни — на рынке, currency — своя валюта подпольного рынка или лавки гильдии
export interface ShopPrice {
    gold: number;
    currency: number;
    gem: string | null;
    gemRarity: Rarity;
    gems: number;
}

export interface ShopOfferInfo {
    id: string;
    kind: ShopOfferKind;
    slot: number;
    rarity: Rarity;
    locked: boolean;
    sold: boolean;
    price: ShopPrice;
    item: EquipmentInfo | null;
    hero: HeroInfo | null;
}

export interface ShopGemStack {
    gem: string;
    rarity: Rarity;
    quantity: number;
}

export interface ShopWallet {
    gold: number;
    rainbowCoins: number;
    bloodCoins: number;
    honorBadges: number;
    gems: ShopGemStack[];
}

export interface ShopOverview {
    shop: ShopKind;
    items: ShopOfferInfo[];
    heroes: ShopOfferInfo[];
    wallet: ShopWallet;
    resources: PlayerResources;
    // Бесплатное обновление сегодня ещё не потрачено; иначе — rerollCost радужных монет
    freeReroll: boolean;
    rerollCost: number;
    refreshAt: string;
    serverTime: string;
}

export interface ShopBuyResult {
    shop: ShopOverview;
    hero: HeroInfo | null;
    item: EquipmentInfo | null;
}

// Коды отказа сервера (ShopError) — тексты в shop.errors.<code>
export type ShopError =
    | 'UnknownShop'
    | 'OfferNotFound'
    | 'AlreadySold'
    | 'NotEnoughGold'
    | 'NotEnoughGems'
    | 'NotEnoughCurrency'
    | 'NotEnoughRainbowCoins'
    | 'Changed';
