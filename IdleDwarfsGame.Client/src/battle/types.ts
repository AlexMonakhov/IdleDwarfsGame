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
  | 'CombatEnd'
  // Арена изменила бой целиком: новая прибавка ко всему урону (damageBonusPercent)
  | 'ArenaBuff';

export type DamageType =
  | 'Physical'
  | 'Magical'
  | 'Fire'
  | 'Frost'
  | 'Lightning'
  | 'Poison'
  | 'Bleed'
  | 'True'
  | 'Dark'
  | 'Radiant';

export interface StatusEffectLogData {
  effectId: string;
  name: string;
  // Состояние статуса на цели после этого события: сколько ходов осталось, сколько наложений
  duration: number;
  stacks: number;
  // Сила за ход всех наложений вместе: урон, лечение или процент замедления (мороз)
  power?: number;
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
  // Уровень спелла 1..3, есть у записи SkillCast
  skillLevel?: number | null;
  damageType: DamageType;
  damage: number;
  heal: number;
  targetRemainingHp: number;
  targetRemainingHpAfter?: number | null;
  attackerEnergy?: number | null;
  isCritical: boolean;
  winningTeam?: number | null;
  // Для ArenaBuff: прибавка ко всему урону с этого раунда, в процентах
  damageBonusPercent?: number | null;
  text?: string | null;
  appliedEffects: StatusEffectLogData[];
  activeEffects: StatusEffectLogData[];
}

export interface CombatUnitInfo {
  entityId: number;
  heroId?: string | null;
  name: string;
  unitType: string;
  element: HeroElement;
  level: number;
  team: number;
  position: number;
  isBoss?: boolean;
  maxHp: number;
  hp: number;
  attack: number;
  speed: number;
  maxEnergy: number;
  energy: number;
  spells: string[];
  immunities: string[];
  aura?: string | null;
  auraEffect?: string | null;
  auraTarget?: AuraTarget | null;
  power: CombatPowerBreakdown;
}

// Боевая мощь (IdleDwarfsGame.Server/ECS/CombatPower.cs): total = stats + level + skills + passives + equipment
export interface CombatPowerBreakdown {
  total: number;
  stats: number;
  level: number;
  skills: number;
  passives: number;
  equipment: number;
}

export type AuraTarget = 'Allies' | 'Enemies' | 'All';

// Стихия героя (HeroElement.cs). На 1-м уровне у всех Basic
export type HeroElement = 'Basic' | 'Fire' | 'Frost' | 'Lightning' | 'Nature' | 'Dark' | 'Radiance';

// То, что нужно портрету: подходит и юнит боя, и герой из инвентаря
export type PortraitUnit = Pick<CombatUnitInfo,
  'name' | 'unitType' | 'element' | 'level' | 'maxHp' | 'attack' | 'speed' | 'spells' | 'immunities' | 'aura' | 'isBoss' | 'power'>;

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
