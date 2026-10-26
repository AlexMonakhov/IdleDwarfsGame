// Контракт с сервером (IdleDwarfsGame.Server/Controllers/Responses/CampaignResponses.cs)
import { CombatResult, CombatUnitInfo } from '@/battle/types';

export type StageType = 'Mobs' | 'Boss';
export type StageStatus = 'Available' | 'Locked';

export interface ArenaInfo {
    id: number;
    name: string;
    description: string;
    // Ключ фона арены ("lava", "ice")
    background: string;
    // Имя статуса ("Burn", "Frost"), который арена вешает на всех в начале раунда
    effectName?: string | null;
    effectPower: number;
    effectDuration: number;
}

export interface CampaignStageInfo {
    id: number;
    name: string;
    description: string;
    type: StageType;
    enemyLevel: number;
    status: StageStatus;
    arena: ArenaInfo;
    enemies: CombatUnitInfo[];
    forecast: BattleForecast;
}

// Мощь отряда против мощи врагов уровня и шанс победы (доля побед в прогонах настоящего боя на сервере)
export interface BattleForecast {
    playerPower: number;
    enemyPower: number;
    // 0..1
    winChance: number;
}

export interface CampaignOverview {
    // null — кампания пройдена целиком
    currentStageId: number | null;
    // Незабранная награда за пройденный уровень: пока она есть, вместо кнопки боя — «Забрать награды»
    pendingRewardId: string | null;
    clearedCount: number;
    totalStages: number;
    squad: CombatUnitInfo[];
    stages: CampaignStageInfo[];
}

export interface CampaignFightResult {
    stageId: number;
    // Первое прохождение: за него выдана награда, её забирают в кампании
    firstClear: boolean;
    currentStageId: number | null;
    arena: ArenaInfo;
    combat: CombatResult;
}
