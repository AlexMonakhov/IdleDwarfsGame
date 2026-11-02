import { reactive } from 'vue';
import { AuraTarget, CombatLogEntry, CombatResult, CombatUnitInfo, HeroElement } from './types';

export interface FloatingText {
  id: number;
  text: string;
  kind: 'damage' | 'heal' | 'status' | 'immune' | 'cast';
  damageType?: string;
  crit?: boolean;
  // Сколько снято или вылечено: чем больше доля от здоровья юнита, тем крупнее цифра
  amount?: number;
  // Свой цвет надписи, например цвет заклинания
  color?: string;
  // Дорожка над юнитом: -1, 0 или 1
  lane: number;
}

export type FloatingTextDetails = Pick<FloatingText, 'damageType' | 'crit' | 'amount' | 'color'>;

// Реплика в облачке над персонажем (см. DialogueService)
export interface Speech {
  id: number;
  text: string;
}

// Подсветка юнита, чей сейчас ход
export const TURN_COLOR = '#ffe8a3';

// Роль юнита в текущем действии — по ней подсвечивается, кто кого бьёт
export interface UnitFocus {
  role: 'attacker' | 'caster' | 'target';
  color?: string;
}

// Статус на юните (StatusEffectLogData с сервера): цифры на его иконке
export interface UnitStatus {
  // Сколько ходов ещё висит
  turns: number;
  // Сила за ход всех наложений: урон, лечение или процент замедления
  power: number;
  stacks: number;
}

// Состояние юнита на сцене. Меняется только BattlePlayer'ом по мере проигрывания логов.
export interface BattleUnit {
  id: number;
  name: string;
  unitType: string;
  element: HeroElement;
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
  // Эффект ауры (Burn, Frost, Regen...) и её цель — по ним выбирается цвет сферы
  auraEffect: string | null;
  auraTarget: AuraTarget | null;
  // Имя статуса -> его состояние: сервер держит на юните по одному статусу каждого вида, наложения складываются
  statuses: Record<string, UnitStatus>;
  floatingTexts: FloatingText[];
  focus: UnitFocus | null;
  speech: Speech | null;
}

export interface BattleState {
  units: Record<number, BattleUnit>;
  round: number;
  // Прибавка ко всему урону от арены (PvP), в процентах; 0 — нет
  damageBonus: number;
  winningTeam: number | null;
  playing: boolean;
  // Бой загружен, но ждёт, пока прогрузятся спрайт-листы юнитов
  preparing: boolean;
  finished: boolean;
  shownLogs: CombatLogEntry[];
}

export function toBattleUnit(info: CombatUnitInfo): BattleUnit {
  return {
    id: info.entityId,
    name: info.name,
    unitType: info.unitType,
    // Если спрайта под стихию/уровень нет, sprites.ts подберёт ближайший
    element: info.element ?? 'Basic',
    level: info.level,
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
    auraEffect: info.auraEffect ?? null,
    auraTarget: info.auraTarget ?? null,
    statuses: {},
    floatingTexts: [],
    focus: null,
    speech: null,
  };
}

export function createBattleState(): BattleState {
  return reactive<BattleState>({
    units: {},
    round: 0,
    damageBonus: 0,
    winningTeam: null,
    playing: false,
    preparing: false,
    finished: false,
    shownLogs: [],
  });
}

export function resetBattleState(state: BattleState, result: CombatResult) {
  state.units = Object.fromEntries(result.units.map(u => [u.entityId, toBattleUnit(u)]));
  state.round = 0;
  state.damageBonus = 0;
  state.winningTeam = null;
  state.playing = false;
  state.preparing = false;
  state.finished = false;
  state.shownLogs = [];
}

let floatingTextId = 0;

// Дорожка закрепляется за цифрой при появлении: новая встаёт рядом с предыдущей, и цифры не перекрывают друг друга
export function addFloatingText(unit: BattleUnit, lifetime: number, text: string, kind: FloatingText['kind'], details: FloatingTextDetails = {}) {
  const last = unit.floatingTexts[unit.floatingTexts.length - 1];
  const entry: FloatingText = { id: ++floatingTextId, text, kind, lane: last ? ((last.lane + 2) % 3) - 1 : 0, ...details };
  unit.floatingTexts.push(entry);
  setTimeout(() => {
    const index = unit.floatingTexts.findIndex(t => t.id === entry.id);
    if (index >= 0) unit.floatingTexts.splice(index, 1);
  }, lifetime);
}

// Эффект ауры юнита. Если сервер не прислал auraEffect, берём имя из строки описания ("Burn 4 → Enemies").
export function auraEffectOf(unit: BattleUnit): string | null {
  return unit.auraEffect ?? unit.aura?.split(' ')[0] ?? null;
}

export function teamUnits(state: BattleState, team: number): BattleUnit[] {
  return Object.values(state.units)
    .filter(u => u.team === team)
    .sort((a, b) => a.position - b.position);
}
