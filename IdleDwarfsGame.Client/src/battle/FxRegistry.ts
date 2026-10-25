import { EffectType } from '@/services/particleService/types/interfaces/canvas-interfaces';
import type { AuraMote } from '@/services/particleService/types/combat-effects';
import type { AuraTarget } from './types';

// Описания боевых визуалов. Чтобы добавить новый эффект (статус, заклинание, тип урона),
// достаточно дописать запись сюда; при необходимости — новую canvas-стратегию в particleService.

export const DAMAGE_COLORS: Record<string, string> = {
  Physical: '#ffd166',
  Magical: '#c084fc',
  Fire: '#ff7a1a',
  Frost: '#7dd3fc',
  Lightning: '#9be7ff',
  Poison: '#84cc16',
  Bleed: '#ef4444',
  True: '#ffffff',
  Heal: '#4ade80',
};

// Точки на спрайте юнита (доля высоты кадра): кадр персонажа занимает примерно 27%..72% по высоте
export type Anchor = 'center' | 'head' | 'feet';
export const UNIT_ANCHORS: Record<Anchor, number> = {
  head: 0.2,
  center: 0.5,
  feet: 0.72,
};

export interface FxSpec {
  type: EffectType;
  // Длительность при скорости x1, мс. Для постоянных аур не используется.
  duration: number;
  anchor?: Anchor;
  // background — под юнитами (круги на земле), foreground — поверх
  layer?: 'foreground' | 'background';
  padding?: number;
  delay?: number;
  args?: Record<string, unknown>;
}

export interface SpellVisual {
  // Свечение кастера и цвет магического круга под ним
  color: string;
  // Снаряд от кастера к цели; duration — время полёта
  projectile?: FxSpec;
  // Мгновенный луч от кастера к цели (молния)
  beam?: FxSpec;
  // Через сколько мс после появления луча засчитывается попадание
  hitDelay?: number;
  impact: FxSpec[];
}

export interface SpriteTint {
  color: string;
  alpha: number;
  // Частота пульсации, Гц (0 — ровная окраска)
  pulse?: number;
}

export interface StatusVisual {
  // Постоянные эффекты, пока статус висит на юните
  aura?: FxSpec[];
  // Окраска силуэта
  tint?: SpriteTint;
  // Множитель скорости idle-анимации (0 — полная заморозка)
  animSpeed?: number;
  // Доп. CSS-класс на юните
  className?: string;
  // Эффект на тике статуса (урон от горения и т.п.)
  tick?: FxSpec[];
  // Цвет вспышки силуэта при уроне от тика
  tickFlash?: string;
  // При нескольких статусах окраску берём у статуса с наибольшим приоритетом
  priority?: number;
}

const FIRE = ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'];
const ICE = ['#ffffff', '#e0f6ff', '#bae6fd', '#7dd3fc', '#38bdf8', '#1e40af'];
const ARCANE = ['#ffffff', '#f5d0fe', '#e879f9', '#c084fc', '#9333ea', '#4c1d95'];
const ELECTRIC = ['#ffffff', '#e0fbff', '#9be7ff', '#38bdf8', '#6366f1'];
const POISON = ['#f7fee7', '#d9f99d', '#a3e635', '#65a30d', '#365314'];
const BLOOD = ['#fecaca', '#f87171', '#dc2626', '#991b1b', '#450a0a'];

const groundPulse = (color: string, radius = 55, duration = 650): FxSpec => ({
  type: 'ring',
  anchor: 'feet',
  layer: 'background',
  duration,
  args: { color, radius, mode: 'pulse', width: 5 },
});

// Попадание по типу урона (обычная атака, урон вне каста)
export const HIT_VISUALS: Record<string, FxSpec[]> = {
  Physical: [
    { type: 'slash', duration: 360, args: { color: '#ffe8a3', radius: 48, width: 12 } },
    { type: 'impact', duration: 420, padding: 40, args: { color: '#ffd166' } },
  ],
  Fire: [
    { type: 'explosion', duration: 600, args: { colors: FIRE, count: 26, radius: 45, speed: 4, smoke: true } },
  ],
  Frost: [
    { type: 'explosion', duration: 600, args: { colors: ICE, count: 18, radius: 45, speed: 4, shards: true, gravity: 0.08 } },
  ],
  Lightning: [
    { type: 'explosion', duration: 450, args: { colors: ELECTRIC, count: 22, radius: 50, speed: 6, gravity: 0 } },
  ],
  Magical: [
    { type: 'explosion', duration: 550, args: { colors: ARCANE, count: 24, radius: 45, speed: 4 } },
  ],
  Poison: [
    { type: 'explosion', duration: 600, args: { colors: POISON, count: 20, radius: 40, speed: 3, gravity: -0.06, ring: false, smoke: true, smokeColor: 'rgba(54, 83, 20, 1)' } },
  ],
  Bleed: [
    { type: 'explosion', duration: 600, args: { colors: BLOOD, count: 20, radius: 35, speed: 3.5, gravity: 0.18, ring: false } },
  ],
  True: [
    { type: 'impact', duration: 420, padding: 45, args: { color: '#ffffff' } },
  ],
};

const FIREBALL: SpellVisual = {
  color: DAMAGE_COLORS.Fire,
  projectile: {
    type: 'projectile',
    duration: 600,
    args: { core: '#fff1a8', glow: '#ff6a00', trail: FIRE, size: 13, arc: 50, trailRate: 4 },
  },
  impact: [
    { type: 'explosion', duration: 850, padding: 80, args: { colors: FIRE, count: 48, radius: 70, speed: 6, smoke: true } },
    groundPulse('#ff7a1a', 60),
  ],
};

const FROST_BOLT: SpellVisual = {
  color: DAMAGE_COLORS.Frost,
  projectile: {
    type: 'projectile',
    duration: 520,
    args: { core: '#ffffff', glow: '#7dd3fc', trail: ICE, size: 10, arc: 15, trailRate: 3, rise: 0, shape: 'shard' },
  },
  impact: [
    { type: 'explosion', duration: 700, args: { colors: ICE, count: 30, radius: 55, speed: 5, shards: true, gravity: 0.1 } },
    groundPulse('#7dd3fc'),
  ],
};

const LIGHTNING: SpellVisual = {
  color: DAMAGE_COLORS.Lightning,
  beam: { type: 'lightning', duration: 380, args: { color: '#9be7ff', jitter: 20 } },
  hitDelay: 120,
  impact: [
    { type: 'explosion', duration: 500, args: { colors: ELECTRIC, count: 30, radius: 60, speed: 7, gravity: 0 } },
    groundPulse('#9be7ff', 50, 500),
  ],
};

const ARCANE_BOLT: SpellVisual = {
  color: DAMAGE_COLORS.Magical,
  projectile: {
    type: 'projectile',
    duration: 550,
    args: { core: '#f5d0fe', glow: '#a855f7', trail: ARCANE, size: 11, arc: 30, trailRate: 3, rise: 0 },
  },
  impact: HIT_VISUALS.Magical,
};

// Сначала ищем по имени заклинания, затем по типу урона
export const SPELL_VISUALS: Record<string, SpellVisual> = {
  Fireball: FIREBALL,
  Lightning: LIGHTNING,
  Fire: FIREBALL,
  Frost: FROST_BOLT,
  Magical: ARCANE_BOLT,
};

export const resolveSpellVisual = (skillName: string | undefined, damageType: string): SpellVisual =>
  (skillName && SPELL_VISUALS[skillName]) || SPELL_VISUALS[damageType] || ARCANE_BOLT;

export const HEAL_VISUAL: FxSpec[] = [
  { type: 'sparkles', duration: 900, args: { color: '#4ade80', burst: 14, rate: 0.6, once: true, width: 60, height: 90 } },
  groundPulse('#4ade80', 55, 700),
];

// Ключ — имя статуса с сервера (StatusEffectBase.EffectName)
export const STATUS_VISUALS: Record<string, StatusVisual> = {
  Burn: {
    aura: [{ type: 'flames', duration: 0, args: { width: 44, height: 75, rate: 1.2, size: 7 } }],
    tint: { color: '#ff6a1a', alpha: 0.2, pulse: 2.5 },
    tick: [{ type: 'flames', duration: 750, args: { burst: true, rate: 7, width: 60, height: 110, size: 11, glow: 'rgba(255, 120, 30, 0.6)' } }],
    tickFlash: '#ff7a1a',
    priority: 2,
  },
  Frost: {
    aura: [{ type: 'frost-aura', duration: 0, args: { width: 70, height: 110, rate: 0.35 } }],
    tint: { color: '#7dd3fc', alpha: 0.38 },
    animSpeed: 0.5,
    tick: [{ type: 'explosion', duration: 600, args: { colors: ICE, count: 16, radius: 40, speed: 3, shards: true, gravity: 0.1 } }],
    tickFlash: '#bae6fd',
    priority: 3,
  },
  // Обледенение: заготовка под будущий статус — полная заморозка анимации и плотная ледяная окраска
  Freeze: {
    aura: [{ type: 'frost-aura', duration: 0, args: { width: 80, height: 120, rate: 0.6 } }],
    tint: { color: '#bae6fd', alpha: 0.6 },
    animSpeed: 0,
    className: 'status-frozen',
    tickFlash: '#e0f6ff',
    priority: 4,
  },
  Stun: {
    aura: [{ type: 'stun-stars', duration: 0, anchor: 'head', args: { count: 3, radiusX: 28, radiusY: 9 } }],
    animSpeed: 0.25,
    className: 'status-stunned',
    priority: 1,
  },
  Regen: {
    aura: [{ type: 'sparkles', duration: 0, args: { color: '#4ade80', rate: 0.12, width: 50, height: 80, shape: 'plus' } }],
  },
  Bleed: {
    tick: HIT_VISUALS.Bleed,
    tickFlash: '#dc2626',
  },
};

export interface AuraVisual {
  // Обод сферы, волны, вспышка носителя при срабатывании
  color: string;
  // Дымка внутри сферы и круг на земле
  glow: string;
  // Блик и ядра частиц
  accent: string;
  // Частицы всплеска при срабатывании
  palette: string[];
  mote: AuraMote;
  // Дрейф частиц: <0 поднимаются (угольки, плюсики), >0 опускаются (снежинки)
  drift: number;
  // Секунд между пульсациями
  period: number;
  // Эффект на цели, до которой дошла волна
  hit: FxSpec[];
}

const auraHit = (color: string, shape: 'plus' | 'star' | 'dot'): FxSpec[] => [
  groundPulse(color, 50, 600),
  { type: 'sparkles', duration: 700, args: { color, burst: 8, rate: 0.4, once: true, width: 50, height: 80, shape } },
];

const aura = (visual: Omit<AuraVisual, 'hit'>, hitShape: 'plus' | 'star' | 'dot'): AuraVisual => ({
  ...visual,
  hit: auraHit(visual.color, hitShape),
});

const AURA_FIRE = aura({ color: '#ff7a1a', glow: '#ffb347', accent: '#fff1a8', palette: FIRE, mote: 'ember', drift: -1.2, period: 1.6 }, 'dot');
const AURA_FROST = aura({ color: '#38bdf8', glow: '#7dd3fc', accent: '#e0f6ff', palette: ICE, mote: 'flake', drift: 0.6, period: 2.6 }, 'star');
const AURA_HEAL = aura({ color: '#22c55e', glow: '#4ade80', accent: '#dcfce7', palette: ['#f0fdf4', '#bbf7d0', '#4ade80', '#16a34a'], mote: 'plus', drift: -0.9, period: 2.2 }, 'plus');
const AURA_LIGHTNING = aura({ color: '#38bdf8', glow: '#9be7ff', accent: '#ffffff', palette: ELECTRIC, mote: 'spark', drift: -0.5, period: 1.2 }, 'star');
const AURA_ARCANE = aura({ color: '#a855f7', glow: '#c084fc', accent: '#f5d0fe', palette: ARCANE, mote: 'spark', drift: -0.6, period: 2 }, 'star');
const AURA_POISON = aura({ color: '#65a30d', glow: '#a3e635', accent: '#ecfccb', palette: POISON, mote: 'dot', drift: -0.5, period: 2.4 }, 'dot');
const AURA_DAMAGE = aura({ color: '#dc2626', glow: '#f87171', accent: '#fecaca', palette: BLOOD, mote: 'ember', drift: -0.7, period: 1.8 }, 'dot');
const AURA_BUFF = aura({ color: '#f59e0b', glow: '#ffd166', accent: '#fff7d6', palette: ['#fffbe0', '#ffe27a', '#ffd166', '#f59e0b'], mote: 'spark', drift: -0.7, period: 2.2 }, 'star');

// Ключ — имя эффекта ауры с сервера (Hero.AuraEffect)
export const AURA_VISUALS: Record<string, AuraVisual> = {
  Burn: AURA_FIRE,
  Frost: AURA_FROST,
  Freeze: AURA_FROST,
  Regen: AURA_HEAL,
  Heal: AURA_HEAL,
  Stun: AURA_LIGHTNING,
  Shock: AURA_LIGHTNING,
  Poison: AURA_POISON,
  Bleed: AURA_DAMAGE,
  Arcane: AURA_ARCANE,
};

// Неизвестный эффект: на врагов — «вредная» красная аура, на союзников — золотой бафф
export const resolveAuraVisual = (effect?: string | null, target?: AuraTarget | null): AuraVisual =>
  (effect && AURA_VISUALS[effect]) || (target === 'Allies' ? AURA_BUFF : AURA_DAMAGE);

// Сводный визуал нескольких статусов: окраска самого приоритетного, самая медленная анимация, все классы
export function combineStatusVisuals(names: string[]) {
  const visuals = names.map(n => STATUS_VISUALS[n]).filter((v): v is StatusVisual => !!v);
  const tinted = visuals
    .filter(v => v.tint)
    .sort((a, b) => (b.priority ?? 0) - (a.priority ?? 0))[0];

  return {
    tint: tinted?.tint ?? null,
    animSpeed: visuals.reduce((speed, v) => Math.min(speed, v.animSpeed ?? 1), 1),
    classes: visuals.map(v => v.className).filter((c): c is string => !!c),
  };
}
