import { reactive } from 'vue';
import { CombatLogEntry, CombatResult, CombatUnitInfo } from './types';

export interface FloatingText {
  id: number;
  text: string;
  kind: 'damage' | 'heal' | 'status' | 'immune';
  damageType?: string;
  crit?: boolean;
}

// Роль юнита в текущем действии — по ней подсвечивается, кто кого бьёт
export interface UnitFocus {
  role: 'attacker' | 'caster' | 'target';
  color?: string;
}

// Состояние юнита на сцене. Меняется только BattlePlayer'ом по мере проигрывания логов.
export interface BattleUnit {
  id: number;
  name: string;
  unitType: string;
  color: 'red' | 'blue';
  level: number;
  team: number;
  position: number;
  maxHp: number;
  hp: number;
  maxEnergy: number;
  energy: number;
  alive: boolean;
  spells: string[];
  immunities: string[];
  aura?: string | null;
  // Имя статуса -> оставшиеся ходы (несколько стаков одного эффекта объединяются)
  statuses: Record<string, number>;
  floatingTexts: FloatingText[];
  focus: UnitFocus | null;
}

export interface BattleState {
  units: Record<number, BattleUnit>;
  round: number;
  winningTeam: number | null;
  playing: boolean;
  finished: boolean;
  shownLogs: CombatLogEntry[];
}

const SPRITE_COLORS = ['red', 'blue'] as const;
const SPRITE_LEVELS = [4];

export function toBattleUnit(info: CombatUnitInfo): BattleUnit {
  const color = (SPRITE_COLORS as readonly string[]).includes(info.color)
    ? (info.color as BattleUnit['color'])
    : info.team === 1 ? 'red' : 'blue';

  return {
    id: info.entityId,
    name: info.name,
    unitType: info.unitType,
    color,
    // Пока нарисованы спрайты только для одного уровня
    level: SPRITE_LEVELS.includes(info.level) ? info.level : SPRITE_LEVELS[0],
    team: info.team,
    position: Math.min(Math.max(info.position, 1), 5),
    maxHp: info.maxHp,
    hp: info.hp,
    maxEnergy: info.maxEnergy,
    energy: info.energy,
    alive: info.hp > 0,
    spells: info.spells,
    immunities: info.immunities,
    aura: info.aura,
    statuses: {},
    floatingTexts: [],
    focus: null,
  };
}

export function createBattleState(): BattleState {
  return reactive<BattleState>({
    units: {},
    round: 0,
    winningTeam: null,
    playing: false,
    finished: false,
    shownLogs: [],
  });
}

export function resetBattleState(state: BattleState, result: CombatResult) {
  state.units = Object.fromEntries(result.units.map(u => [u.entityId, toBattleUnit(u)]));
  state.round = 0;
  state.winningTeam = null;
  state.playing = false;
  state.finished = false;
  state.shownLogs = [];
}

export function teamUnits(state: BattleState, team: number): BattleUnit[] {
  return Object.values(state.units)
    .filter(u => u.team === team)
    .sort((a, b) => a.position - b.position);
}
