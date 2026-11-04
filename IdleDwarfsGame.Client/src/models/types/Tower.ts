// Контракт с сервером (IdleDwarfsGame.Server/Controllers/Responses/TowerResponses.cs)
import { CombatResult, CombatUnitInfo, HeroElement } from '@/battle/types';
import { ArenaInfo, BattleForecast, StageStatus, StageType } from '@/models/types/Campaign';
import { PlayerResources } from '@/models/types/Expeditions';
import { RewardInfo } from '@/models/types/Inventory';

export interface TowerFloorInfo {
    floor: number;
    element: HeroElement;
    type: StageType;
    status: StageStatus;
    arena: ArenaInfo;
    enemies: CombatUnitInfo[];
    forecast: BattleForecast;
    // Награда за этаж фиксированная — её видно заранее
    reward: RewardInfo;
}

export interface TowerOverview {
    // Неделя башни (UTC, ISO): с понедельника до следующего
    weekStart: string;
    weekEndsAt: string;
    // Стихии блоков по floorsPerBlock этажей: первая — у этажей 1–5
    rotation: HeroElement[];
    floorsPerBlock: number;
    // 0 — на этой неделе ещё ничего не пройдено
    clearedFloor: number;
    currentFloor: number;
    // Лучший результат прошлых недель; 0 — его нет
    bestFloor: number;
    // До какого этажа можно пропустить разом; null — пропуск недоступен
    skipToFloor: number | null;
    skipReward: RewardInfo | null;
    // Незабранная награда за этаж: пока она есть, вместо боя — «Забрать награды»
    pendingRewardId: string | null;
    squad: CombatUnitInfo[];
    floors: TowerFloorInfo[];
}

export interface TowerFightResult {
    floor: number;
    // Этаж пройден этим боем: награда выдана, её забирают в окне башни
    cleared: boolean;
    currentFloor: number;
    element: HeroElement;
    arena: ArenaInfo;
    combat: CombatResult;
}

export interface TowerSkipResult {
    skippedTo: number;
    reward: RewardInfo;
    resources: PlayerResources;
}
