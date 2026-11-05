// Контракт с сервером (IdleDwarfsGame.Server/Controllers/Responses/CampaignResponses.cs)
import { CombatResult, CombatUnitInfo, HeroElement } from '@/battle/types';

export type StageType = 'Mobs' | 'Boss';
export type StageStatus = 'Available' | 'Locked';

export interface ArenaInfo {
    id: number;
    name: string;
    description: string;
    // Ключ фона арены ("lava", "ice")
    background: string;
    // Стихия арены — по ней красятся карточки боссов гильдии
    element: HeroElement;
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

// Пройденный уровень в списке — без врагов и прогноза: номер, название, арена и награда, если её не забрали
export interface ClearedStageInfo {
    id: number;
    name: string;
    type: StageType;
    // Ключ фона арены (arenaBackground)
    background: string;
    element: HeroElement;
    // Незабранная награда (null — уже забрана). Что в ней — видно только после получения
    rewardId: string | null;
}

export interface CampaignOverview {
    // null — кампания пройдена целиком
    currentStageId: number | null;
    // Пройденные уровни по порядку: с незабранной наградой и последний пройденный (над текущим).
    // Награды не мешают идти дальше
    clearedStages: ClearedStageInfo[];
    // Сколько всего незабранных наград — подсветка «Забрать всё»
    unclaimedRewards: number;
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
