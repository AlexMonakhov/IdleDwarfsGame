// Гильдии (IdleDwarfsGame.Server/Controllers/Responses/GuildResponses.cs)
import { CombatResult, CombatUnitInfo } from '@/battle/types';
import { RewardInfo } from '@/models/types/Inventory';

export type GuildRole = 'Member' | 'Leader';
export type GuildBossTier = 'Easy' | 'Medium' | 'Hard';

export interface GuildMemberInfo {
    userId: string;
    username: string;
    role: GuildRole;
    joinedAt: string;
    // Это сам игрок
    isMe: boolean;
    isOnline: boolean;
    // null — ни разу не заходил с тех пор, как появились сессии
    lastSeenAt: string | null;
}

// Заявка в гильдию, которую видит глава
export interface GuildJoinRequestInfo {
    userId: string;
    username: string;
    createdAt: string;
}

// Своя заявка в чужую гильдию
export interface GuildOutgoingRequestInfo {
    guildId: string;
    guildName: string;
    createdAt: string;
}

export interface GuildInfo {
    id: string;
    name: string;
    createdAt: string;
    myRole: GuildRole;
    maxMembers: number;
    // Сначала глава, затем те, кто в сети
    members: GuildMemberInfo[];
    // Только у главы
    incoming: GuildJoinRequestInfo[];
}

export interface GuildOverview {
    serverTime: string;
    // null — игрок не в гильдии
    guild: GuildInfo | null;
    outgoing: GuildOutgoingRequestInfo[];
    maxOutgoingRequests: number;
}

export interface GuildSearchResult {
    guildId: string;
    name: string;
    leaderName: string;
    memberCount: number;
    maxMembers: number;
    // Заявка в эту гильдию уже подана
    requested: boolean;
}

export interface GuildBossDamageInfo {
    userId: string;
    username: string;
    damage: number;
}

export interface GuildBossInfo {
    tier: GuildBossTier;
    unit: CombatUnitInfo;
    // Уровень, под который масштабирована атака босса
    level: number;
    // Фон арены боя (ключ, картинка — arenaBackground)
    background: string;
    maxHp: number;
    currentHp: number;
    defeatedAt: string | null;
    killedBy: string | null;
    // null — можно бить сейчас
    nextAttackAt: string | null;
    // Награду за этого босса сегодня уже забрали (в этой или другой гильдии)
    rewardClaimed: boolean;
    reward: RewardInfo;
    myDamage: number;
    // По убыванию урона
    damage: GuildBossDamageInfo[];
}

export interface GuildBossesOverview {
    serverTime: string;
    // Новые боссы — в 00:00 UTC
    resetAt: string;
    attackCooldownMinutes: number;
    bosses: GuildBossInfo[];
}

export interface GuildBossAttackResult {
    tier: GuildBossTier;
    damage: number;
    // Этот удар добил босса
    killed: boolean;
    bossHp: number;
    maxHp: number;
    nextAttackAt: string;
    // Фон арены боя (ключ, картинка — arenaBackground)
    background: string;
    combat: CombatResult;
}
