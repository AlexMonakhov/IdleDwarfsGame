// Снаряжение и пассивки героев
// (IdleDwarfsGame.Server/Controllers/Responses/EquipmentResponses.cs, ECS/Equipment)
import { AuraTarget, HeroElement } from '@/battle/types';
import { HeroInfo } from '@/models/types/Heroes';
import { Rarity } from '@/models/types/Inventory';

export type EquipmentType = 'Weapon' | 'Helmet' | 'Neck' | 'Ring';
export type EquipmentSlot = 'Weapon' | 'Helmet' | 'Neck' | 'Ring1' | 'Ring2';

export const EQUIPMENT_TYPES: EquipmentType[] = ['Weapon', 'Helmet', 'Neck', 'Ring'];

export const SLOT_TYPE: Record<EquipmentSlot, EquipmentType> = {
    Weapon: 'Weapon',
    Helmet: 'Helmet',
    Neck: 'Neck',
    Ring1: 'Ring',
    Ring2: 'Ring',
};

export interface ItemStats {
    maxHp: number;
    attack: number;
    speed: number;
    maxEnergy: number;
    startEnergy: number;
    energyPerAttack: number;
}

export type HeroPassive = 'SurvivalOfTheWeakest' | 'SharpTongue' | 'DoubleStrike';

// Текст эффекта — equipment.effects.<kind>; какие поля заполнены, зависит от вида
export interface ItemEffectInfo {
    kind: string;
    value: number;
    spell?: string | null;
    procSpell?: string | null;
    procLevel?: number | null;
    passive?: HeroPassive | null;
    aura?: string | null;
    auraTarget?: AuraTarget | null;
}

export interface EquipmentInfo {
    id: string;
    key: string;
    type: EquipmentType;
    rarity: Rarity;
    // Пусто — подходит любой стихии
    elements: HeroElement[];
    stats: ItemStats;
    effects: ItemEffectInfo[];
    // Оба null — предмет в инвентаре
    heroId: string | null;
    slot: EquipmentSlot | null;
}

// Пассивка героя с итоговыми значениями: уровень по рангу плюс кольца
export interface PassiveInfo {
    passive: HeroPassive;
    level: number;
    percent: number;
    turns: number;
    targets: number;
}

// Ответ на смену снаряжения: изменившиеся герои и все предметы игрока
export interface EquipmentChangeResult {
    heroes: HeroInfo[];
    items: EquipmentInfo[];
}

// Коды отказа сервера: по ним показываем текст equipment.errors.<code>
export type EquipmentError = 'ItemNotFound' | 'HeroNotFound' | 'WrongSlot' | 'ElementRestricted' | 'SlotChanged';

export const canWear = (item: EquipmentInfo, element: HeroElement): boolean =>
    item.elements.length === 0 || item.elements.includes(element);
