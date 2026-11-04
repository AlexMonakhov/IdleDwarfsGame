import { CombatPowerBreakdown, HeroElement } from '@/battle/types';
import { EquipmentInfo, PassiveInfo } from '@/models/types/Equipment';
import { PlayerResources } from '@/models/types/Expeditions';
import type { AbilityInfo } from '@/models/types/Abilities';

// Профессия дворфа (IdleDwarfsGame.Server/Enums/HeroProfession.cs)
export type HeroProfession = 'Defender' | 'Warrior' | 'Axeman' | 'Miner' | 'Scout';

// Герой в инвентаре (IdleDwarfsGame.Server/Controllers/Responses/HeroResponses.cs)
export interface HeroInfo {
    id: string;
    name: string;
    unitType: string;
    element: HeroElement;
    // Профессия задаёт базовые статы дворфа; у монстров её нет
    profession?: HeroProfession | null;
    level: number;
    // null — герой не в отряде
    placeInSquad: number | null;
    // Герой в экспедиции: виден, но не может участвовать в других активностях
    onExpedition: boolean;
    // Дворф работает в шахте — так же занят, как в экспедиции
    inMine: boolean;
    maxHp: number;
    attack: number;
    speed: number;
    maxEnergy: number;
    spells: string[];
    immunities: string[];
    aura?: string | null;
    passives: PassiveInfo[];
    // Что можно извлечь, распылив дворфа: заклинания, пассивки, аура (иммунитеты не извлекаются)
    abilities: AbilityInfo[];
    // Надетые предметы; maxHp, attack и остальное уже с их прибавкой
    equipment: EquipmentInfo[];
    power: CombatPowerBreakdown;
    upgrade: HeroUpgradeInfo;
}

// Прокачка героя за золото и опыт аккаунта (IdleDwarfsGame.Server/Services/HeroUpgradeService.cs).
// Это внутренний уровень 1..maxLevel, а не level героя: тот меняет вид и способности
export interface HeroUpgradeInfo {
    level: number;
    maxLevel: number;
    // null — уровень максимальный
    cost: HeroUpgradeCost | null;
    // Статы и мощь после следующего уровня
    next: HeroUpgradePreview | null;
    // Сколько вложено в прокачку: при слиянии вернётся FUSION_REFUND от этого
    invested: HeroUpgradeCost;
}

export interface HeroUpgradeCost {
    gold: number;
    experience: number;
}

export interface HeroUpgradePreview {
    maxHp: number;
    attack: number;
    power: number;
}

export interface HeroUpgradeResult {
    hero: HeroInfo;
    resources: PlayerResources;
}

// Коды отказа сервера: по ним показываем текст heroUpgrade.errors.<code>
export type HeroUpgradeError = 'HeroNotFound' | 'MaxLevel' | 'NotEnoughGold' | 'NotEnoughExperience' | 'LevelChanged';

export const SQUAD_SIZE = 5;

// Герой в экспедиции или в шахте: виден в списках, но в отряд и на задания не идёт
export const isBusy = (hero: HeroInfo) => hero.onExpedition || hero.inMine;

// Состав отряда по слотам: [i] — id героя на месте i + 1, null — место пустое
export const squadSlotIds = (heroes: HeroInfo[]): (string | null)[] => {
    const slots: (string | null)[] = Array(SQUAD_SIZE).fill(null);
    heroes.forEach(h => {
        if (h.placeInSquad && h.placeInSquad <= SQUAD_SIZE) slots[h.placeInSquad - 1] = h.id;
    });
    return slots;
};

export type HeroInformation = {
    icon: string,
    name: string,
    level: string,
    rarity: string
}