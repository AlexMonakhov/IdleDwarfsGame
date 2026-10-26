import { CombatPowerBreakdown, HeroElement } from '@/battle/types';
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

export type HeroInformation = {
    icon: string,
    name: string,
    level: string,
    rarity: string
}