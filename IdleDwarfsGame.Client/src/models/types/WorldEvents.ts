// Активности ивентов недели (IdleDwarfsGame.Server/Controllers/Responses/WorldBossResponses.cs и CommunityMineResponses.cs,
// правила — Services/WorldBossRules.cs и CommunityMineRules.cs)
import type { CombatResult, CombatUnitInfo, HeroElement } from '@/battle/types';
import type { ArenaInfo } from '@/models/types/Campaign';
import type { PlayerResources } from '@/models/types/Expeditions';
import type { WorldEventInfo } from '@/models/types/FixedShop';

/* ===== Мировые боссы ===== */

export interface WorldBossDamageInfo {
    playerId: string;
    name: string;
    damage: number;
}

export interface WorldBossInfo {
    id: string;
    stage: number;
    unit: CombatUnitInfo;
    level: number;
    background: string;
    arena: ArenaInfo | null;
    maxHp: number;
    currentHp: number;
    defeatedAt: string | null;
    killedBy: string | null;
    myDamage: number;
    // Клыки, если босса убьют сейчас — по нынешнему урону игрока
    myKillReward: number;
    top: WorldBossDamageInfo[];
}

export interface WorldBossPendingReward {
    bossId: string;
    stage: number;
    damage: number;
    lastHit: boolean;
    currency: number;
}

export interface WorldBossOverview {
    event: WorldEventInfo;
    // Бить можно: идёт ивент мировых боссов
    open: boolean;
    boss: WorldBossInfo | null;
    nextAttackAt: string | null;
    attackCooldownMinutes: number;
    attackCurrency: number;
    killBase: number;
    sharePool: number;
    lastHitBonus: number;
    pending: WorldBossPendingReward[];
    leaderboard: WorldBossDamageInfo[];
    myEventDamage: number;
    myRank: number;
    serverTime: string;
}

export interface WorldBossAttackResult {
    stage: number;
    damage: number;
    killed: boolean;
    bossHp: number;
    maxHp: number;
    currency: number;
    nextAttackAt: string;
    background: string;
    arena: ArenaInfo;
    combat: CombatResult;
}

export interface WorldBossClaimResult {
    currency: number;
    overview: WorldBossOverview;
}

/* ===== Общие шахты ===== */

export type DepositKind = 'Copper' | 'Iron' | 'Silver' | 'Gold' | 'Mithril';

export interface DepositDwarfInfo {
    heroId: string;
    name: string;
    rank: number;
    element: HeroElement;
    profession: string | null;
    rate: number;
}

export interface DepositContributorInfo {
    playerId: string;
    name: string;
    contribution: number;
    share: number;
}

export interface DepositInfo {
    id: string;
    index: number;
    kind: DepositKind;
    size: number;
    // Медь — только для дворфов до этого ранга
    maxRank: number | null;
    oreTotal: number;
    // На момент serverTime обзора; дальше фронт досчитывает по rate
    oreMined: number;
    rate: number;
    speedLevel: number;
    maxSpeedLevel: number;
    speedFactor: number;
    nextSpeedFactor: number;
    dwarves: number;
    participants: number;
    etaAt: string | null;
    spawnedAt: string;
    exhaustedAt: string | null;
    closedAt: string | null;
    myDwarves: DepositDwarfInfo[];
    myContribution: number;
    myShare: number;
    myReward: number;
    // Цена следующего улучшения — одна для всех, зависит от вида жилы
    upgradeGold: number | null;
    upgradePoints: number;
    myUpgrades: number;
    maxUpgradesPerPlayer: number;
    top: DepositContributorInfo[];
}

export interface DepositPendingReward {
    depositId: string;
    index: number;
    kind: DepositKind;
    contribution: number;
    share: number;
    currency: number;
}

export interface CommunityMinesOverview {
    event: WorldEventInfo;
    open: boolean;
    deposits: DepositInfo[];
    nextSpawnAt: string | null;
    maxOpen: number;
    maxDwarvesPerPlayer: number;
    minShare: number;
    pending: DepositPendingReward[];
    leaderboard: DepositContributorInfo[];
    myContribution: number;
    myRank: number;
    serverTime: string;
    resources: PlayerResources;
}

export interface CommunityMinesClaimResult {
    currency: number;
    overview: CommunityMinesOverview;
}
