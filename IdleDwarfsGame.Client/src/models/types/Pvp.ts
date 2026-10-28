// PvP (IdleDwarfsGame.Server/Controllers/Responses/PvpResponses.cs)
import { CombatResult, CombatUnitInfo } from '@/battle/types';
import { BattleForecast } from '@/models/types/Campaign';

export type PvpSeasonType = 'Daily' | 'Weekly' | 'Monthly';

// Арена PvP: каждые intervalRounds полных раундов весь урон растёт ещё на damageBonusPercent
export interface PvpArenaInfo {
    name: string;
    description: string;
    // Ключ фона (arenaBackground)
    background: string;
    intervalRounds: number;
    damageBonusPercent: number;
    maxRounds: number;
}

export interface PvpOpponentInfo {
    userId: string;
    username: string;
    power: number;
    // + — соперник сильнее, − — слабее, в процентах от нашей мощи
    powerDiffPercent: number;
    squad: CombatUnitInfo[];
    forecast: BattleForecast;
    // Очки сезона за победу и за поражение
    winPoints: number;
    lossPoints: number;
}

export interface PvpLeaderboardEntryInfo {
    rank: number;
    userId: string;
    username: string;
    points: number;
    wins: number;
    losses: number;
    isMe: boolean;
}

export interface PvpSeasonInfo {
    type: PvpSeasonType;
    startsAt: string;
    endsAt: string;
    points: number;
    wins: number;
    losses: number;
    // null — в этом сезоне ещё не дрался
    rank: number | null;
    top: PvpLeaderboardEntryInfo[];
}

// Бой из истории — со своей стороны
export interface PvpBattleInfo {
    id: string;
    foughtAt: string;
    // false — на наш отряд напали
    isAttack: boolean;
    opponentUserId: string;
    opponentName: string;
    won: boolean;
    // Изменение своих очков (у защиты — 0)
    points: number;
    myPower: number;
    opponentPower: number;
}

export interface PvpOverview {
    serverTime: string;
    // 00:00 UTC: побеждённые возвращаются в подбор, начинается новый дневной сезон
    dayResetAt: string;
    myPower: number;
    squad: CombatUnitInfo[];
    // Окно мощи подбора: 10%, а если игроков мало — шире
    powerSpreadPercent: number;
    opponentCount: number;
    opponents: PvpOpponentInfo[];
    beatenToday: number;
    seasons: PvpSeasonInfo[];
    history: PvpBattleInfo[];
    arena: PvpArenaInfo;
}

export interface PvpSeasonPointsInfo {
    type: PvpSeasonType;
    points: number;
}

export interface PvpFightResult {
    opponentUserId: string;
    opponentName: string;
    won: boolean;
    // Изменение очков во всех сезонах
    points: number;
    seasons: PvpSeasonPointsInfo[];
    arena: PvpArenaInfo;
    combat: CombatResult;
}
