// Гонка глубины гильдий, помощь друзьям, ставки на «Бой дня», глубинный спуск
// (IdleDwarfsGame.Server/Controllers/Responses/GuildDepthResponses.cs, SocialResponses.cs, DelveResponses.cs)
import type { CombatResult, CombatUnitInfo, HeroElement } from '@/battle/types';
import type { PlayerResources } from '@/models/types/Expeditions';
import type { CampaignFightResult } from '@/models/types/Campaign';
import type { HeroInfo } from '@/models/types/Heroes';
import type { EquipmentInfo } from '@/models/types/Equipment';
import type { AbilityInfo } from '@/models/types/Abilities';

/* ===== Гонка глубины ===== */

export interface GuildDepthDwarfInfo {
    heroId: string | null;
    name: string | null;
    rank: number;
    element: HeroElement;
    profession: string | null;
    baseRate: number;
    rate: number;
    pickLevel: number;
    maxPickLevel: number;
    pickFactor: number;
    nextPickFactor: number;
    pickCostGold: number | null;
    mined: number;
    stored: number;
    storage: number;
    storedGold: number;
    goldPerHour: number;
}

export interface GuildDepthMemberInfo {
    userId: string;
    username: string;
    heroName: string | null;
    rank: number;
    element: HeroElement;
    pickLevel: number;
    rate: number;
    mined: number;
    isMe: boolean;
}

export interface GuildDepthStandingInfo {
    guildId: string;
    name: string;
    score: number;
    members: number;
    place: number;
    honorBadges: number;
    isMine: boolean;
}

export interface GuildDepthOverview {
    serverTime: string;
    weekStart: string;
    endsAt: string;
    nextWeekAt: string;
    active: boolean;
    shaftLevel: number;
    maxShaftLevel: number;
    shaftFactor: number;
    nextShaftFactor: number;
    shaftCostGold: number | null;
    myDwarf: GuildDepthDwarfInfo | null;
    placedElsewhere: boolean;
    guildScore: number;
    guildPlace: number;
    guildCount: number;
    members: GuildDepthMemberInfo[];
    leaderboard: GuildDepthStandingInfo[];
    rewards: { fromPlace: number; honorBadges: number }[];
    pending: { weekStart: string; place: number; honorBadges: number } | null;
    resources: PlayerResources;
}

/* ===== Друзья ===== */

export interface FriendMineInfo {
    mineId: string;
    plot: number;
    type: string;
    level: number;
    dwarves: number;
    stored: number;
    capacity: number;
    helpAmount: number;
}

export interface FriendMinesOverview {
    friendId: string;
    friendName: string;
    helpHours: number;
    helpedToday: boolean;
    helpsGiven: number;
    maxHelpsGiven: number;
    friendHelpsReceived: number;
    maxHelpsReceived: number;
    mines: FriendMineInfo[];
}

export interface FriendHelpResult {
    added: number;
    gold: number;
    resources: PlayerResources;
    mines: FriendMinesOverview;
}

export interface MercenaryFriendInfo {
    userId: string;
    username: string;
    hiredToday: boolean;
    heroes: { heroId: string; unit: CombatUnitInfo }[];
}

export interface MercenaryOverview {
    hiresLeft: number;
    maxHires: number;
    lenderElderSeals: number;
    friends: MercenaryFriendInfo[];
}

export interface MercenaryFightResult {
    fight: CampaignFightResult;
    mercenaryName: string;
    slot: number;
}

/* ===== «Бой дня» ===== */

export interface ArenaBetSideInfo {
    userId: string;
    name: string;
    power: number;
    pool: number;
    bettors: number;
    multiplier: number;
    units: CombatUnitInfo[];
}

export interface ArenaBetMatchInfo {
    windowStart: string;
    closesAt: string;
    open: boolean;
    a: ArenaBetSideInfo;
    b: ArenaBetSideInfo;
    winner: 1 | 2 | null;
    rounds: number;
    myBet: { side: 1 | 2; amount: number; payout: number | null; claimed: boolean } | null;
}

export interface ArenaBetOverview {
    serverTime: string;
    current: ArenaBetMatchInfo | null;
    previous: ArenaBetMatchInfo | null;
    claimable: number;
    minBet: number;
    maxBet: number;
    houseCut: number;
    resources: PlayerResources;
}

export interface ArenaBetReplay {
    aName: string;
    bName: string;
    background: string;
    combat: CombatResult;
}

/* ===== Глубинный спуск ===== */

export type DelveNodeType = 'Fight' | 'Elite' | 'Camp' | 'Treasure' | 'Boss';
export type DelveRunStatus = 'Active' | 'Dead' | 'Retreated';
export type DelvePending = 'None' | 'Relic' | 'Camp' | 'Reward';
export type DelveRewardKind = 'Element' | 'Spell' | 'Item' | 'Passive' | 'Aura' | 'LevelUp' | 'SpellUp' | 'PassiveUp' | 'AuraUp';

export interface DelveNodeInfo {
    floor: number;
    column: number;
    type: DelveNodeType;
}

export interface DelvePassiveValues {
    percent: number;
    turns: number;
    targets: number;
}

export interface DelveDwarfInfo {
    heroId: string;
    name: string;
    element: HeroElement;
    level: number;
    position: number;
    // Доля HP 0..1; 0 — пал
    hp: number;
    unit: CombatUnitInfo;
    // Дворф в формате алтаря: карточка героя, его предмет и способности
    hero: HeroInfo;
    spellId: number | null;
    spellName: string | null;
    spellLevel: number;
    itemKey: string | null;
    passive: string | null;
    passiveLevel: number;
    passiveValues: DelvePassiveValues | null;
    auraEffect: string | null;
    auraLevel: number;
}

export interface DelveRewardInfo {
    kind: DelveRewardKind;
    key: string | null;
    level: number;
    spellId: number | null;
    spellName: string | null;
    element: HeroElement | null;
    rarity: string | null;
    passiveValues: DelvePassiveValues | null;
    // Предмет и способность — в формате склада и алтаря
    item: EquipmentInfo | null;
    ability: AbilityInfo | null;
    // Награду надевают на выбранного дворфа — только из fits
    needsTarget: boolean;
    fits: string[];
}

export interface DelveRunInfo {
    id: string;
    status: DelveRunStatus;
    floor: number;
    column: number;
    pending: DelvePending;
    offer: string[];
    rewards: DelveRewardInfo[];
    relics: string[];
    dwarves: DelveDwarfInfo[];
    // Весь следующий этаж и узел через этаж под текущим столбцом
    next: DelveNodeInfo[];
    peek: DelveNodeInfo | null;
    goldEarned: number;
    experienceEarned: number;
    rainbowCoinsEarned: number;
    revived: boolean;
    canRevive: boolean;
}

export interface DelveOverview {
    serverTime: string;
    periodStart: string;
    nextRunAt: string;
    columns: number;
    bossEvery: number;
    reviveCost: number;
    run: DelveRunInfo | null;
    canStart: boolean;
    bestFloor: number;
    leaderboard: { place: number; userId: string; username: string; floor: number; isMe: boolean }[];
    relics: { key: string; rare: boolean; max: number | null }[];
}

export interface DelveMoveResult {
    node: DelveNodeType;
    combat: CombatResult | null;
    background: string | null;
    won: boolean | null;
    gold: number;
    experience: number;
    rainbowCoins: number;
    overview: DelveOverview;
}

export interface DelveDeclineResult {
    gold: number;
    experience: number;
    overview: DelveOverview;
}
