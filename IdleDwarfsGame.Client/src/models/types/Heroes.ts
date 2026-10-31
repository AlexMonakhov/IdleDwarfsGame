import { CombatPowerBreakdown, HeroElement } from '@/battle/types';
import { EquipmentInfo, PassiveInfo } from '@/models/types/Equipment';
import { PlayerResources } from '@/models/types/Expeditions';

// Герой в инвентаре (IdleDwarfsGame.Server/Controllers/Responses/HeroResponses.cs)
export interface HeroInfo {
    id: string;
    name: string;
    unitType: string;
    element: HeroElement;
    level: number;
    // null — герой не в отряде
    placeInSquad: number | null;
    // Герой в экспедиции: виден, но не может участвовать в других активностях
    onExpedition: boolean;
    maxHp: number;
    attack: number;
    speed: number;
    maxEnergy: number;
    spells: string[];
    immunities: string[];
    aura?: string | null;
    passives: PassiveInfo[];
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