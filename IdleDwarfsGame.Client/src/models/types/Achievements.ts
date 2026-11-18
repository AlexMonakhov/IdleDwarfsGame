// Достижения и календарь входов (IdleDwarfsGame.Server/Controllers/Responses/AchievementResponses.cs,
// правила — Services/Achievements.cs и Services/LoginCalendar.cs)
import type { PlayerResources } from '@/models/types/Expeditions';
import type { RewardInfo } from '@/models/types/Inventory';
import type { FrameEffect } from '@/models/types/Account';

export interface GrantedReward {
    reward: RewardInfo;
    rainbowCoins: number;
    elderSeals: number;
    frames: FrameEffect[];
}

/* ===== Достижения ===== */

export type AchievementMetric =
    | 'Activity'
    | 'CampaignStages'
    | 'TowerBestFloor'
    | 'HeroesOwned'
    | 'Friends'
    | 'LoginDays'
    | 'LoginBestStreak';

export interface AchievementTierInfo {
    target: number;
    reached: boolean;
    claimed: boolean;
    reward: RewardInfo;
    rainbowCoins: number;
    elderSeals: number;
    // Рамка аватара за эту ступень
    frame: FrameEffect | null;
}

export interface AchievementInfo {
    // Название и текст — achievements.list.<key>
    key: string;
    metric: AchievementMetric;
    progress: number;
    claimedTiers: number;
    reachedTiers: number;
    tiers: AchievementTierInfo[];
}

export interface AchievementBoard {
    achievements: AchievementInfo[];
    // Ступеней, которые можно забрать сейчас — значок на кнопке
    claimable: number;
    claimedTiers: number;
    totalTiers: number;
    unlockedFrames: FrameEffect[];
    resources: PlayerResources;
}

export interface AchievementClaimResult {
    board: AchievementBoard;
    granted: GrantedReward;
}

/* ===== Календарь входов ===== */

export interface LoginDayInfo {
    day: number;
    claimed: boolean;
    today: boolean;
    big: boolean;
    reward: RewardInfo;
    rainbowCoins: number;
    elderSeals: number;
}

export interface LoginCalendarInfo {
    cycle: number;
    cycleLength: number;
    day: number;
    claimedToday: boolean;
    claimedDays: number;
    streak: number;
    bestStreak: number;
    totalDays: number;
    // Пропуск оборвал серию — календарь начался заново
    broken: boolean;
    missedDaysAllowed: number;
    resetAt: string;
    serverTime: string;
    days: LoginDayInfo[];
    resources: PlayerResources;
}

export interface LoginClaimResult {
    calendar: LoginCalendarInfo;
    granted: GrantedReward;
}

// Коды отказа сервера (AchievementError) — тексты в achievements.errors.<code>
export type AchievementError = 'UnknownAchievement' | 'NothingToClaim' | 'AlreadyClaimed' | 'Changed';
