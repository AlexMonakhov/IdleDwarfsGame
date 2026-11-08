// Ежедневные задания (IdleDwarfsGame.Server/Controllers/Responses/QuestResponses.cs, правила — Services/DailyQuests.cs)
import type { PlayerResources } from '@/models/types/Expeditions';
import type { RewardInfo } from '@/models/types/Inventory';

export type ActivityKind =
    | 'CampaignFight'
    | 'CampaignWin'
    | 'TowerFight'
    | 'PvpFight'
    | 'PvpWin'
    | 'GuildBossAttack'
    | 'MineDig'
    | 'MineCollect'
    | 'ExpeditionStart'
    | 'ExpeditionClaim'
    | 'ChestOpen'
    | 'HeroUpgrade'
    | 'ForgeFuse'
    | 'ShopBuy';

export type QuestDifficulty = 'Easy' | 'Medium' | 'Hard';

// CommunityMines — общие шахты, WorldBosses — мировые боссы (Services/WorldEvents.cs)
export type WorldEventKind = 'CommunityMines' | 'WorldBosses';

export interface QuestInfo {
    id: string;
    slot: number;
    // Шаблон задания — текст в quests.templates.<key>
    key: string;
    kind: ActivityKind;
    difficulty: QuestDifficulty;
    target: number;
    progress: number;
    completed: boolean;
    claimed: boolean;
    points: number;
    elderSeals: number;
    eventCurrency: number;
    gold: number;
    experience: number;
}

export interface QuestMilestoneInfo {
    index: number;
    points: number;
    reached: boolean;
    claimed: boolean;
    reward: RewardInfo;
    elderSeals: number;
    eventCurrency: number;
}

export interface QuestBoard {
    day: string;
    resetAt: string;
    serverTime: string;
    points: number;
    maxPoints: number;
    freeReroll: boolean;
    rerollCost: number;
    // Сколько наград можно забрать сейчас — для значка на кнопке
    claimable: number;
    quests: QuestInfo[];
    milestones: QuestMilestoneInfo[];
    // null — ивент сейчас не идёт
    event: { kind: WorldEventKind; endsAt: string } | null;
    resources: PlayerResources;
}

export interface QuestClaimResult {
    board: QuestBoard;
    reward: RewardInfo;
    elderSeals: number;
    eventCurrency: number;
}

// Коды отказа сервера (QuestError) — тексты в quests.errors.<code>
export type QuestError =
    | 'QuestNotFound'
    | 'NotCompleted'
    | 'AlreadyClaimed'
    | 'UnknownMilestone'
    | 'MilestoneNotReached'
    | 'NotEnoughRainbowCoins'
    | 'Changed';
