// Контракт с сервером (IdleDwarfsGame.Server/ECS/ECSEntities.cs). Енамы приходят строками.

export type CombatEventType =
  | 'CombatStart'
  | 'RoundStart'
  | 'Attack'
  | 'SkillCast'
  | 'Damage'
  | 'StatusApplied'
  | 'StatusTick'
  | 'StatusExpired'
  | 'Heal'
  | 'Stunned'
  | 'Immune'
  | 'Death'
  | 'CombatEnd';

export type DamageType =
  | 'Physical'
  | 'Magical'
  | 'Fire'
  | 'Frost'
  | 'Lightning'
  | 'Poison'
  | 'Bleed'
  | 'True';

export interface StatusEffectLogData {
  effectId: string;
  name: string;
  duration: number;
  stacks: number;
  tickDamage?: number | null;
  damageType?: DamageType | null;
  description?: string | null;
}

export interface CombatLogEntry {
  sequence: number;
  actionId: number;
  attackerId: number;
  targetId: number;
  attackerTeam: number;
  targetTeam: number;
  actionType: string;
  round: number;
  eventType: CombatEventType;
  phase: string;
  skillId?: number | null;
  skillName: string;
  damageType: DamageType;
  damage: number;
  heal: number;
  targetRemainingHp: number;
  targetRemainingHpAfter?: number | null;
  attackerEnergy?: number | null;
  isCritical: boolean;
  winningTeam?: number | null;
  text?: string | null;
  appliedEffects: StatusEffectLogData[];
  activeEffects: StatusEffectLogData[];
}

export interface CombatUnitInfo {
  entityId: number;
  heroId?: string | null;
  name: string;
  unitType: string;
  color: string;
  level: number;
  team: number;
  position: number;
  maxHp: number;
  hp: number;
  attack: number;
  speed: number;
  maxEnergy: number;
  energy: number;
  spells: string[];
  immunities: string[];
  aura?: string | null;
}

export interface CombatRewards {
  gold: number;
  experience: number;
}

export interface CombatResult {
  units: CombatUnitInfo[];
  logs: CombatLogEntry[];
  winningTeam: number;
  rounds: number;
  rewards?: CombatRewards | null;
}
