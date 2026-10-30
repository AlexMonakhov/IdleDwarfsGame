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
    // Рейтинговая арена: рейтинг соперника и насколько он выше (+) или ниже (−) нашего; в обычной — null
    rating: number | null;
    ratingDiffPercent: number | null;
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
    // Рейтинг сторон до боя на рейтинговой арене; в обычной — null
    myRating: number | null;
    opponentRating: number | null;
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

// Закреплённый отряд рейтинговой арены — снимок на момент закрепления
export interface RankedSquadInfo {
    lockedAt: string;
    // С этого момента (следующие 00:00 UTC) отряд можно перезакрепить
    canChangeAt: string;
    power: number;
    units: CombatUnitInfo[];
}

export interface RankedOverview {
    serverTime: string;
    dayResetAt: string;
    // null — отряд ещё не закреплён, драться нельзя
    squad: RankedSquadInfo | null;
    canLockSquad: boolean;
    // Текущий отряд аккаунта — что попадёт в снимок при закреплении
    accountPower: number;
    accountSquad: CombatUnitInfo[];
    // Рейтинг сезона matchSeason, по которому идёт подбор, и окно подбора (5%, а если игроков мало — шире)
    myRating: number;
    // С него начинается каждый сезон
    startRating: number;
    matchSeason: PvpSeasonType;
    ratingSpreadPercent: number;
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
