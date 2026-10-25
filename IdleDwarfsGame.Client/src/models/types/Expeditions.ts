// Экспедиции (IdleDwarfsGame.Server/Controllers/Responses/ExpeditionResponses.cs)

export type ExpeditionStatus = 'Available' | 'InProgress' | 'Ready' | 'Claimed';

export interface PlayerResources {
    gold: number;
    experience: number;
}

export interface ExpeditionReward {
    gold: number;
    experience: number;
}

// Награда как функция мощи (длительность уже учтена): gold = round(baseGold + goldPerPower × мощь)
export interface ExpeditionRates {
    baseGold: number;
    goldPerPower: number;
    baseExperience: number;
    experiencePerPower: number;
}

export interface ExpeditionInfo {
    id: string;
    slot: number;
    templateKey: string;
    durationHours: number;
    status: ExpeditionStatus;
    startedAt: string | null;
    endsAt: string | null;
    rates: ExpeditionRates;
    // Мощь отправленных героев и зафиксированная награда; у доступных заданий — 0
    power: number;
    reward: ExpeditionReward;
    heroIds: string[];
}

export interface ExpeditionBoard {
    serverTime: string;
    refreshAt: string;
    maxHeroes: number;
    expeditions: ExpeditionInfo[];
}

export interface ExpeditionClaimResult {
    reward: ExpeditionReward;
    resources: PlayerResources;
}

// Та же формула, что ExpeditionService.RewardFor на сервере — превью награды, пока игрок подбирает героев
export const estimateReward = (rates: ExpeditionRates, power: number): ExpeditionReward => ({
    gold: Math.round(rates.baseGold + rates.goldPerPower * power),
    experience: Math.round(rates.baseExperience + rates.experiencePerPower * power),
});
