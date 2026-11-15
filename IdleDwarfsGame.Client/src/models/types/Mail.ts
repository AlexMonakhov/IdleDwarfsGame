// Почта игрока и админка (IdleDwarfsGame.Server/Controllers/Responses/MailResponses.cs)
import type { AbilityInfo, AbilityKind } from '@/models/types/Abilities';
import type { EquipmentInfo } from '@/models/types/Equipment';
import type { PlayerResources } from '@/models/types/Expeditions';
import type { HeroInfo, HeroProfession } from '@/models/types/Heroes';
import type { ChestInfo, ItemInfo, Rarity } from '@/models/types/Inventory';

// Порядок в письме всегда такой: дворфы, предметы, камни, способности, сундуки, валюты
export type MailAttachmentKind = 'Hero' | 'Equipment' | 'Gem' | 'Ability' | 'Chest' | 'Currency';
export type MailCurrency = 'Gold' | 'Experience' | 'RainbowCoins' | 'BloodCoins' | 'HonorBadges' | 'ElderSeals' | 'EventCurrency';

// Вложение с превью: заполнено поле своего вида
export interface MailAttachmentInfo {
    id: string;
    // Вложение с несколькими дворфами приходит карточкой на каждого: номер дворфа (у остальных видов 0)
    index: number;
    kind: MailAttachmentKind;
    amount: number;
    hero?: HeroInfo | null;
    equipment?: EquipmentInfo | null;
    gem?: ItemInfo | null;
    ability?: AbilityInfo | null;
    chest?: ChestInfo | null;
    currency?: MailCurrency | null;
}

export interface MailInfo {
    id: string;
    sender: string;
    title: string;
    body: string;
    createdAt: string;
    isRead: boolean;
    // Вложения забраны
    isClaimed: boolean;
    attachments: MailAttachmentInfo[];
}

export interface MailClaimResult {
    mail: MailInfo;
    resources: PlayerResources | null;
}

// ----- Админка -----

export interface AdminPlayerInfo {
    id: string;
    name: string;
    rating: number;
    squadPower: number;
}

export interface AdminMailCatalog {
    heroes: HeroInfo[];
    equipment: EquipmentInfo[];
    gems: ItemInfo[];
    abilities: AbilityInfo[];
    // Все сундуки с таблицей дропа; key вложения — тип, у бесконечности «Infinity:Fire»
    chests: ChestInfo[];
    currencies: MailCurrency[];
    // Для конструктора дворфа
    professions: HeroProfession[];
    immunities: string[];
}

// Дворф из конструктора админки: null — выпадает случайно из seed (как из сундука).
// customAbilities: false — заклинание, пассивка и аура случайные; true — ровно заданные (null — нет).
// Иммунитеты — всегда только заданные
export interface MailHeroSpec {
    name: string | null;
    profession: HeroProfession | null;
    customAbilities: boolean;
    spellFamily: number | null;
    spellLevel: number;
    passive: string | null;
    passiveLevel: number;
    aura: string | null;
    auraLevel: number;
    immunities: string[];
}

export interface AdminHeroPreview {
    seed: number;
    hero: HeroInfo;
}

export interface HeroPreviewRequest {
    level: number;
    element: string;
    seed: number | null;
    heroSpec: MailHeroSpec | null;
}

// Вложение в запросе на отправку. Что значат поля — см. MailAttachment на сервере
export interface MailAttachmentRequest {
    kind: MailAttachmentKind;
    key: string;
    level: number;
    rarity: Rarity;
    abilityKind: AbilityKind | null;
    amount: number;
    // Дворф: seed из превью (игрок получит его) и конструктор
    seed?: number | null;
    heroSpec?: MailHeroSpec | null;
}

export interface SendMailRequest {
    playerId: string;
    sender: string | null;
    title: string;
    body: string;
    attachments: MailAttachmentRequest[];
}
