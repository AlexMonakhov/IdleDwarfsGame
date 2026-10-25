import { CombatPowerBreakdown, HeroElement } from '@/battle/types';

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
}

export const SQUAD_SIZE = 5;

export type HeroInformation = {
    icon: string,
    name: string,
    level: string,
    rarity: string
}