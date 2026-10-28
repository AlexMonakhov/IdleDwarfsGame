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
  Dark: '#a855f7',
  Radiant: '#fff7d6',
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

// Эффект на весь каст: над группой целей (туча бурана) или на кастере (вспышка)
export interface CastFx extends FxSpec {
  at: 'caster' | 'targets';
}

export interface SpellVisual {
  // Свечение кастера и цвет магического круга под ним
  color: string;
  // Снаряд к цели; duration — время полёта
  projectile?: FxSpec;
  // Мгновенный луч к цели (молния)
  beam?: FxSpec;
  // Откуда идут снаряд и луч: от кастера, с неба над целью или от предыдущей цели (цепь перескакивает)
  from?: 'caster' | 'sky' | 'chain';
  // Для from: 'sky' — на сколько выше цели и на сколько ближе к кастеру начинается полёт, px
  sky?: { height: number; dx?: number };
  // Залп: несколько снарядов в цель подряд, у каждого своё маленькое попадание
  volley?: { count: number; interval: number; spread?: number; hit?: FxSpec[] };
  // Волна от кастера (огненный вал, кольцо света): одна на каст, тянется дальше самой дальней цели.
  // Цель поражается, когда волна до неё дошла; args.travel — доля жизни волны на проход
  wave?: FxSpec;
  cast?: CastFx[];
  // Пауза до первой доставки (туча успевает собраться), мс
  windup?: number;
  // Цели поражаются по очереди с таким шагом, мс
  stagger?: number;
  // Эффекты на цели с начала доставки (метка удара, рост темницы, печать над головой)
  telegraph?: FxSpec[];
  // Через сколько мс после начала доставки засчитывается попадание — для луча и заклинаний без снаряда
  hitDelay?: number;
  impact: FxSpec[];
  // Визуал идёт в лечимых союзников (Рассвет), а не только в раненых врагов
  support?: boolean;
  // Лечение кастера или союзников, идущее вместе с уроном, срабатывает через столько мс после попаданий
  healDelay?: number;
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
const SHADOW = ['#f3e8ff', '#c084fc', '#9333ea', '#6b21a8', '#3b0764', '#1e0538'];
// Радуга бриллианта: частица проходит весь спектр за жизнь
const PRISM = ['#ffffff', '#ff7ad9', '#a78bfa', '#6bd5ff', '#6bff95', '#ffd166', '#ff6b6b'];
const LEAF = ['#f0fdf4', '#bbf7d0', '#4ade80', '#16a34a', '#14532d'];

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
  Dark: [
    { type: 'explosion', duration: 650, args: { colors: SHADOW, count: 22, radius: 45, speed: 3.5, gravity: -0.05, smoke: true, smokeColor: 'rgba(30, 5, 56, 1)' } },
  ],
  Radiant: [
    { type: 'explosion', duration: 550, args: { colors: PRISM, count: 26, radius: 50, speed: 5, gravity: 0, ring: '#ffffff' } },
    { type: 'sparkles', duration: 600, args: { color: '#fff7d6', burst: 6, rate: 0.3, once: true, width: 50, height: 70, shape: 'star' } },
  ],
};

export type SpellLevel = 1 | 2 | 3;
type SpellVisualOf = (level: SpellLevel) => SpellVisual;

// Значение для уровня заклинания: [1-й, 2-й, 3-й]
const lvl = <T>(level: SpellLevel, values: [T, T, T]): T => values[level - 1];

// Эффекты, которые добавляются начиная с уровня min
const since = (level: SpellLevel, min: SpellLevel, specs: FxSpec[]): FxSpec[] => (level >= min ? specs : []);

const TOPAZ = ['#ffffff', '#fff7d6', '#fde68a', '#ffb300', '#b45309'];

// Искры, которые после попадания разлетаются дугой и осыпаются
const sparkShower = (colors: string[], count: number): FxSpec => ({
  type: 'explosion',
  duration: 750,
  args: { colors, count, radius: 30, speed: 7, gravity: 0.22, ring: false },
});

// Всплеск при попадании заклинания 3-го уровня: густой веер частиц цвета стихии, осыпающиеся искры
// и звёздочки (у лечения — плюсики) — сразу видно, что спелл прокачан до предела
const masteryBurst = (colors: string[], accent: string, shape: 'star' | 'plus' = 'star'): FxSpec[] => [
  { type: 'explosion', duration: 900, padding: 120, args: { colors, count: 60, radius: 80, speed: 8.5, gravity: 0, ring: false } },
  sparkShower(colors, 30),
  { type: 'sparkles', duration: 1000, args: { color: accent, burst: 16, rate: 0.5, once: true, width: 90, height: 120, shape } },
];

const HEAL_PALETTE = ['#f0fdf4', '#bbf7d0', '#4ade80', '#16a34a'];
const DAWN_PALETTE = ['#fffbe0', '#ffe27a', '#ffd166', '#f59e0b'];
const DRAIN_PALETTE = ['#fecdd3', '#f43f5e', '#be185d', '#6b21a8', '#3b0764'];

// Огненный шар по дуге. 2 — шар крупнее, взрыв мощнее; 3 — ещё крупнее, на земле остаются языки пламени
const FIREBALL: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Fire,
  projectile: {
    type: 'projectile',
    duration: 600,
    args: { core: '#fff1a8', glow: '#ff6a00', trail: FIRE, size: lvl(level, [13, 15, 17]), arc: 50, trailRate: lvl(level, [4, 5, 7]) },
  },
  impact: [
    { type: 'explosion', duration: 850, padding: 80, args: { colors: FIRE, count: lvl(level, [48, 58, 70]), radius: lvl(level, [70, 78, 88]), speed: 6, smoke: true } },
    groundPulse('#ff7a1a', lvl(level, [60, 70, 80])),
    ...since(level, 3, [{ type: 'flames', duration: 700, anchor: 'feet', args: { burst: true, rate: 4, width: 70, height: 50, size: 8 } }]),
    ...since(level, 3, masteryBurst(FIRE, '#ffe27a')),
  ],
});

// Ледяной осколок. 2 — крупнее, с искрами инея; 3 — ещё крупнее, под целью вспыхивает ледяной круг
const FROST_BOLT: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Frost,
  projectile: {
    type: 'projectile',
    duration: 520,
    args: { core: '#ffffff', glow: '#7dd3fc', trail: ICE, size: lvl(level, [10, 12, 14]), arc: 15, trailRate: lvl(level, [3, 4, 5]), rise: 0, shape: 'shard' },
  },
  impact: [
    { type: 'explosion', duration: 700, args: { colors: ICE, count: lvl(level, [30, 36, 44]), radius: 55, speed: 5, shards: true, gravity: 0.1 } },
    groundPulse('#7dd3fc'),
    ...since(level, 2, [{ type: 'sparkles', duration: 700, args: { color: '#ffffff', burst: 8, rate: 0.3, once: true, width: 55, height: 80, shape: 'star' } }]),
    ...since(level, 3, [{ type: 'ring', anchor: 'feet', layer: 'background', duration: 800, args: { color: '#bae6fd', radius: 55, mode: 'rune', width: 4 } }]),
    ...since(level, 3, masteryBurst(ICE, '#ffffff')),
  ],
});

// Молния от кастера. 2 — после попадания разлетаются искры; 3 — молния толще и ветвится, искр много, по цели бегут дуги
const LIGHTNING: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Lightning,
  beam: { type: 'lightning', duration: 380, args: { color: '#9be7ff', jitter: 20, width: lvl(level, [10, 10, 16]), branches: lvl(level, [0, 0, 3]) } },
  hitDelay: 120,
  impact: [
    { type: 'explosion', duration: 500, args: { colors: ELECTRIC, count: 30, radius: 60, speed: 7, gravity: 0 } },
    groundPulse('#9be7ff', 50, 500),
    ...since(level, 2, [sparkShower(ELECTRIC, lvl(level, [0, 14, 36]))]),
    ...since(level, 3, [{ type: 'arcs', duration: 600, args: { color: '#9be7ff', count: 3, width: 55, height: 90 } }]),
    ...since(level, 3, masteryBurst(ELECTRIC, '#e0fbff')),
  ],
});

// Запасной визуал магического урона
const ARCANE_BOLT: SpellVisual = {
  color: DAMAGE_COLORS.Magical,
  projectile: {
    type: 'projectile',
    duration: 550,
    args: { core: '#f5d0fe', glow: '#a855f7', trail: ARCANE, size: 11, arc: 30, trailRate: 3, rise: 0 },
  },
  impact: HIT_VISUALS.Magical,
};

/* ===== Огонь ===== */
// Огненный вал катится по земле от кастера через врагов: каждый вспыхивает, когда вал до него докатился.
// 2 — вал выше и гуще; 3 — ещё выше, с искрами угольков на целях (поджигает)
const FLAME_WAVE: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Fire,
  wave: {
    type: 'fire-wall',
    anchor: 'feet',
    duration: 1100,
    padding: 120,
    args: { colors: FIRE, height: lvl(level, [100, 120, 145]), width: lvl(level, [40, 46, 54]), rate: lvl(level, [7, 9, 12]), travel: 0.75 },
  },
  impact: [
    { type: 'flames', duration: 650, args: { burst: true, rate: 6, width: 70, height: lvl(level, [85, 100, 125]), size: 10, glow: 'rgba(255, 90, 0, 0.55)' } },
    groundPulse('#ff7a1a', 50, 550),
    ...since(level, 3, [{ type: 'sparkles', duration: 900, args: { color: '#ffb347', burst: 12, rate: 0.4, once: true, width: 60, height: 90, shape: 'dot', rise: 1.4 } }]),
    ...since(level, 3, masteryBurst(FIRE, '#ffe27a')),
  ],
});

// Метеор падает с неба почти отвесно, с длинным огненным хвостом, а земля под целью разгорается его светом.
// Удар — вспышка, столб огня и ударная волна. 2 — на земле остаётся гореть пламя (поджог);
// 3 — глыба крупнее, хвост длиннее, летят обломки, волна двойная и всё вокруг озаряет зарево
const METEOR: SpellVisualOf = level => ({
  color: '#ff4d00',
  from: 'sky',
  sky: { height: 460, dx: 110 },
  projectile: {
    type: 'comet',
    anchor: 'feet',
    duration: 750,
    padding: 220,
    args: { colors: FIRE, core: '#fffbe0', glow: '#ff6a00', size: lvl(level, [11, 13, 16]), tail: lvl(level, [120, 150, 190]) },
  },
  impact: [
    {
      type: 'explosion',
      anchor: 'feet',
      duration: 1000,
      padding: 120,
      args: { colors: FIRE, count: lvl(level, [44, 56, 72]), radius: lvl(level, [75, 90, 110]), speed: 7, gravity: -0.06, smoke: true, flash: '#fff7d6', debris: lvl(level, [0, 6, 16]) },
    },
    { type: 'flames', anchor: 'feet', duration: 900, args: { burst: true, rate: 9, width: 50, height: lvl(level, [150, 180, 220]), size: 12, glow: 'rgba(255, 110, 20, 0.55)' } },
    groundPulse('#ff4d00', lvl(level, [85, 100, 125]), 800),
    ...since(level, 2, [{ type: 'flames', anchor: 'feet', duration: 1400, delay: 200, args: { burst: true, rate: 3, width: 110, height: 45, size: 8 } }]),
    ...since(level, 3, [
      { ...groundPulse('#ffb347', 150, 900), delay: 160 },
      { type: 'screen-flash', duration: 600, args: { color: '#ffb347', alpha: 0.22 } },
    ]),
    ...since(level, 3, masteryBurst(FIRE, '#fff7d6')),
  ],
});

/* ===== Мороз ===== */
// Над врагами собирается снежная туча, из неё по ветру бьют ледяные иглы.
// 1 — по две иглы в цель, 2 — по три, 3 — туча шире и гуще, по четыре иглы покрупнее
const BLIZZARD: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Frost,
  cast: [{
    at: 'targets',
    type: 'storm',
    anchor: 'head',
    duration: 1700,
    padding: 220,
    args: { lift: 70, margin: lvl(level, [140, 170, 210]), density: lvl(level, [0.7, 1, 1.6]), fall: 260, wind: 2.2 },
  }],
  windup: 350,
  stagger: 90,
  from: 'sky',
  sky: { height: 170, dx: 60 },
  projectile: {
    type: 'projectile',
    duration: 320,
    args: { core: '#ffffff', glow: '#bae6fd', trail: ICE, size: lvl(level, [6, 7, 9]), arc: 0, trailRate: 2, rise: 0, shape: 'shard' },
  },
  volley: {
    count: lvl(level, [2, 3, 4]),
    interval: 120,
    spread: 22,
    hit: [{ type: 'explosion', duration: 400, args: { colors: ICE, count: 8, radius: 25, speed: 2.5, shards: true, gravity: 0.12, ring: false } }],
  },
  impact: [
    { type: 'explosion', duration: 650, args: { colors: ICE, count: 18, radius: 45, speed: 3.5, shards: true, gravity: 0.1, ring: false } },
    { type: 'sparkles', duration: 900, args: { color: '#e0f6ff', burst: lvl(level, [10, 14, 20]), rate: 0.5, once: true, width: 70, height: 100, shape: 'dot', rise: -0.8 } },
    ...since(level, 3, masteryBurst(ICE, '#ffffff')),
  ],
});

// Без снаряда: под целью загорается ледяной круг, из-под ног вырастает частокол шипов и вмораживает её.
// Темница держится и раскалывается, дальше цель остаётся в обледенении (статус Freeze).
// 2 — шипов больше; 3 — шипы шире и массивнее, вокруг встаёт второе кольцо пониже,
// а в миг, когда темница смыкается, во все стороны брызжет ледяная крошка
const GLACIAL_PRISON: SpellVisualOf = level => ({
  color: '#bae6fd',
  stagger: 160,
  telegraph: [
    { type: 'ring', anchor: 'feet', layer: 'background', duration: 1500, args: { color: '#bae6fd', radius: lvl(level, [60, 60, 75]), mode: 'rune', width: 4 } },
    {
      type: 'ice-prison',
      anchor: 'feet',
      duration: 1500,
      padding: 160,
      args: {
        radius: 38,
        height: lvl(level, [125, 125, 145]),
        count: lvl(level, [9, 13, 13]),
        thick: lvl(level, [1, 1, 1.6]),
        outer: lvl(level, [0, 0, 10]),
        burst: lvl(level, [0, 0, 36]),
        shatter: 0.78,
      },
    },
  ],
  hitDelay: 330,
  impact: [
    { type: 'explosion', duration: 600, anchor: 'feet', args: { colors: ICE, count: lvl(level, [22, 26, 34]), radius: 50, speed: 4, shards: true, gravity: 0.15, ring: false } },
    { type: 'sparkles', duration: 900, args: { color: '#ffffff', burst: 10, rate: 0.3, once: true, width: 70, height: 110, shape: 'star' } },
    ...since(level, 3, masteryBurst(ICE, '#ffffff')),
  ],
});

/* ===== Молния ===== */
// Разряд бьёт в ближайшего врага и перескакивает на следующего; по телу каждого трещат дуги.
// 2 — разряд толще и ветвистее; 3 — ещё толще, на каждой цели сыплются искры (электрошок)
const CHAIN_LIGHTNING: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Lightning,
  beam: { type: 'lightning', duration: 300, args: { color: '#7dd3fc', jitter: 22, width: lvl(level, [8, 10, 13]), branches: lvl(level, [1, 2, 3]) } },
  from: 'chain',
  stagger: 150,
  hitDelay: 60,
  impact: [
    { type: 'explosion', duration: 450, args: { colors: ELECTRIC, count: 20, radius: 45, speed: 6, gravity: 0 } },
    { type: 'arcs', duration: 550, args: { color: '#9be7ff', count: lvl(level, [2, 3, 4]), width: 55, height: 90 } },
    ...since(level, 3, [sparkShower(ELECTRIC, 18)]),
    ...since(level, 3, masteryBurst(ELECTRIC, '#e0fbff')),
  ],
});

// Небо вспыхивает, и на каждого врага сверху обрушивается толстая жёлтая молния (цвет топаза); от удара — волна по земле.
// 2 — молнии толще, вспышка ярче; 3 — молнии ещё толще и ветвистее, волна шире и летят искры
const THUNDERCLAP: SpellVisualOf = level => ({
  color: '#fde68a',
  cast: [{ at: 'caster', type: 'screen-flash', duration: 700, args: { color: '#fde68a', alpha: lvl(level, [0.2, 0.3, 0.42]) } }],
  windup: 120,
  stagger: 70,
  from: 'sky',
  sky: { height: 520 },
  beam: { type: 'lightning', anchor: 'feet', duration: 380, args: { color: '#fde68a', jitter: 24, width: lvl(level, [12, 16, 21]), branches: lvl(level, [2, 3, 5]) } },
  hitDelay: 80,
  impact: [
    { type: 'explosion', duration: 550, anchor: 'feet', args: { colors: TOPAZ, count: 30, radius: 60, speed: 7, gravity: 0.1 } },
    groundPulse('#ffd166', lvl(level, [80, 95, 120]), 650),
    { type: 'arcs', duration: 600, args: { color: '#fde68a', count: 2, width: 60, height: 95 } },
    ...since(level, 3, [sparkShower(TOPAZ, 24)]),
    ...since(level, 3, masteryBurst(TOPAZ, '#fff7d6')),
  ],
});

/* ===== Природа ===== */
// Под союзником вспыхивает зелёная руна, из неё поднимаются плюсики.
// 2 — руна шире, по земле идёт волна; 3 — сверху падает мягкий зелёный столб света
const HEALING_RUNE: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Heal,
  support: true,
  telegraph: [
    { type: 'ring', anchor: 'feet', layer: 'background', duration: 1000, args: { color: '#4ade80', radius: lvl(level, [50, 60, 68]), mode: 'rune', width: 4 } },
    ...since(level, 3, [{ type: 'light-pillar', anchor: 'feet', duration: 1100, padding: 120, args: { color: '#4ade80', core: '#dcfce7', width: 60, height: 300 } }]),
  ],
  hitDelay: 280,
  impact: [
    { type: 'sparkles', duration: 900, args: { color: '#86efac', burst: lvl(level, [8, 12, 16]), rate: 0.4, once: true, width: 60, height: 90, shape: 'plus' } },
    ...since(level, 2, [groundPulse('#4ade80', 65, 700)]),
    ...since(level, 3, masteryBurst(HEAL_PALETTE, '#86efac', 'plus')),
  ],
});

// Залп шипов в каждую цель: каждый брызгает ядом, после залпа на цели — ядовитое облако и капли.
// 1 — два шипа в цель, 2 — три, 3 — четыре, облако гуще
const VENOM_THORNS: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Poison,
  projectile: {
    type: 'projectile',
    duration: 420,
    args: { core: '#ecfccb', glow: '#65a30d', trail: POISON, size: 7, arc: 30, trailRate: 2, rise: -0.02, shape: 'shard' },
  },
  volley: {
    count: lvl(level, [2, 3, 4]),
    interval: 90,
    spread: 18,
    hit: [{ type: 'explosion', duration: 380, args: { colors: POISON, count: 8, radius: 22, speed: 2.5, gravity: 0.15, ring: false } }],
  },
  stagger: 60,
  impact: [
    {
      type: 'explosion',
      duration: 600,
      args: { colors: POISON, count: lvl(level, [20, 24, 32]), radius: 40, speed: 3, gravity: -0.06, ring: false, smoke: true, smokeColor: 'rgba(54, 83, 20, 1)' },
    },
    { type: 'sparkles', duration: 900, args: { color: '#a3e635', burst: lvl(level, [6, 10, 14]), rate: 0.3, once: true, width: 50, height: 70, shape: 'dot', rise: -0.7 } },
    ...since(level, 3, masteryBurst(POISON, '#d9f99d')),
  ],
});

// Без снаряда: под целью вспыхивает круг, из земли вырастают лозы с шипами, обвивают цель и держат, пока не увянут.
// 1 — три лозы, 2 — пять, 3 — семь лоз потолще и выше, ядовито-зелёные, от цели поднимаются пузырьки яда
const ENTANGLING_ROOTS: SpellVisualOf = level => ({
  color: '#22c55e',
  stagger: 150,
  telegraph: [
    { type: 'ring', anchor: 'feet', layer: 'background', duration: 1300, args: { color: '#16a34a', radius: 55, mode: 'rune', width: 4 } },
    {
      type: 'roots',
      anchor: 'feet',
      duration: 1400,
      padding: 140,
      args: { count: lvl(level, [3, 5, 7]), height: lvl(level, [110, 125, 140]), width: 62, thick: lvl(level, [1, 1, 1.3]), leaf: lvl(level, ['#4ade80', '#4ade80', '#a3e635']) },
    },
  ],
  hitDelay: 420,
  impact: [
    { type: 'explosion', duration: 700, anchor: 'feet', args: { colors: LEAF, count: 22, radius: 45, speed: 3.5, shards: true, gravity: -0.06, ring: false } },
    ...since(level, 3, [{ type: 'sparkles', duration: 1000, args: { color: '#a3e635', burst: 10, rate: 0.4, once: true, width: 55, height: 90, shape: 'dot', rise: 0.8 } }]),
    ...since(level, 3, masteryBurst(LEAF, '#bbf7d0')),
  ],
});

/* ===== Тьма ===== */
// Сгусток тьмы с дымным шлейфом. 2 — крупнее, под целью расходится круг тьмы; 3 — ещё крупнее, цель охватывает тёмное пламя
const SHADOW_BOLT: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Dark,
  projectile: {
    type: 'projectile',
    duration: 520,
    args: { core: '#f3e8ff', glow: '#7e22ce', trail: SHADOW, size: lvl(level, [10, 12, 14]), arc: 20, trailRate: lvl(level, [3, 4, 6]), rise: 0.02 },
  },
  impact: [
    { type: 'explosion', duration: 750, padding: 70, args: { colors: SHADOW, count: lvl(level, [28, 34, 42]), radius: 55, speed: 4.5, smoke: true, smokeColor: 'rgba(30, 5, 56, 1)' } },
    ...since(level, 2, [groundPulse('#7e22ce', 55)]),
    ...since(level, 3, [{ type: 'flames', duration: 750, args: { burst: true, rate: 5, width: 55, height: 95, size: 9, colors: SHADOW, glow: 'rgba(126, 34, 206, 0.45)' } }]),
    ...since(level, 3, masteryBurst(SHADOW, '#e9d5ff')),
  ],
});

// Багровая лента тянется к цели, по ней к кастеру текут сгустки жизни; кастер лечится, когда они долетели.
// 2 — лента шире, сгустков больше; 3 — три пряди и плотный поток
const LIFE_DRAIN: SpellVisualOf = level => ({
  color: '#be185d',
  beam: {
    type: 'drain',
    duration: 1100,
    padding: 60,
    args: { color: '#e11d48', glow: '#be185d', core: '#fecdd3', amplitude: lvl(level, [10, 14, 18]), rate: lvl(level, [0.5, 0.7, 1]), strands: lvl(level, [2, 2, 3]) },
  },
  hitDelay: 260,
  healDelay: 450,
  impact: [
    { type: 'explosion', duration: 600, args: { colors: ['#fecdd3', '#f43f5e', '#be185d', '#6b21a8', '#3b0764'], count: lvl(level, [18, 22, 30]), radius: 40, speed: 3, gravity: -0.1, ring: false } },
    ...since(level, 3, masteryBurst(DRAIN_PALETTE, '#fecdd3')),
  ],
});

// Над каждым врагом по очереди проступает печать с глазом — висит, вращается и обрушивается на цель облаком тьмы.
// 2 — печать крупнее; 3 — вокруг печати второе кольцо рун, вращающееся навстречу, облако гуще
const HEX: SpellVisualOf = level => ({
  color: '#9333ea',
  stagger: 140,
  telegraph: [{
    type: 'sigil',
    anchor: 'head',
    duration: 1000,
    padding: 90,
    args: { color: '#a855f7', accent: '#f0abfc', radius: lvl(level, [24, 28, 32]), lift: 45, drop: 75, slam: 0.62, ornate: level === 3 },
  }],
  hitDelay: 740,
  impact: [
    {
      type: 'explosion',
      duration: 700,
      args: { colors: SHADOW, count: lvl(level, [18, 22, 30]), radius: 45, speed: 3, gravity: -0.06, ring: '#a855f7', smoke: true, smokeColor: 'rgba(59, 7, 100, 1)' },
    },
    { type: 'sparkles', duration: 800, args: { color: '#c084fc', burst: lvl(level, [6, 8, 12]), rate: 0.3, once: true, width: 50, height: 80, shape: 'star' } },
    ...since(level, 3, masteryBurst(SHADOW, '#f0abfc')),
  ],
});

/* ===== Сияние ===== */
// Почти прямой белый луч и радужные искры на цели. 2 — луч толще; 3 — ещё толще, на цели радужный венец искр
const PRISM_RAY: SpellVisualOf = level => ({
  color: DAMAGE_COLORS.Radiant,
  beam: { type: 'lightning', duration: 450, args: { color: '#ffffff', jitter: 3, width: lvl(level, [8, 10, 14]) } },
  hitDelay: 100,
  impact: [
    ...HIT_VISUALS.Radiant,
    groundPulse('#ff7ad9', 50, 550),
    ...since(level, 3, [
      { type: 'explosion', duration: 700, args: { colors: PRISM, count: 24, radius: 70, speed: 5.5, gravity: 0, ring: '#ff7ad9' } },
      { type: 'sparkles', duration: 800, args: { color: '#ffffff', burst: 10, rate: 0.3, once: true, width: 70, height: 90, shape: 'star' } },
    ]),
    ...since(level, 3, masteryBurst(PRISM, '#ffffff')),
  ],
});

// От кастера по всему полю расходится кольцо света: враги вспыхивают, когда их задело.
// 2 — кольцо несёт исцеление (в искрах зелёные); 3 — за первым кольцом идёт второе, искр больше
const RADIANT_NOVA: SpellVisualOf = level => ({
  color: '#fff7d6',
  wave: {
    type: 'nova',
    duration: 1100,
    padding: 200,
    args: {
      colors: level >= 2 ? [...PRISM, '#4ade80', '#86efac'] : PRISM,
      color: '#fde68a',
      accent: '#ffffff',
      travel: 0.7,
      sparks: lvl(level, [2, 3, 5]),
      echo: level === 3,
    },
  },
  impact: [
    { type: 'explosion', duration: 650, args: { colors: PRISM, count: lvl(level, [24, 30, 38]), radius: 55, speed: 5, gravity: 0, ring: '#ffffff' } },
    { type: 'sparkles', duration: 800, args: { color: '#ffffff', burst: 10, rate: 0.4, once: true, width: 70, height: 90, shape: 'star' } },
    ...since(level, 3, masteryBurst(PRISM, '#ffffff')),
  ],
});

// На каждого союзника по очереди падает столб рассветного света; лечение — когда луч раскрылся у земли.
// 2 — столбы шире; 3 — ещё шире, искр больше, под ногами вспыхивает солнечный круг
const DAWNLIGHT: SpellVisualOf = level => ({
  color: '#ffd166',
  support: true,
  stagger: 120,
  telegraph: [
    {
      type: 'light-pillar',
      anchor: 'feet',
      duration: 1300,
      padding: 120,
      args: { color: '#ffd166', core: '#fffbeb', width: lvl(level, [60, 80, 100]), height: 340, rate: lvl(level, [0.4, 0.6, 1]) },
    },
    ...since(level, 3, [{ type: 'ring', anchor: 'feet', layer: 'background', duration: 1200, args: { color: '#ffd166', radius: 65, mode: 'rune', width: 4 } }]),
  ],
  hitDelay: 300,
  impact: [
    { type: 'sparkles', duration: 900, args: { color: '#ffe8a3', burst: lvl(level, [8, 10, 14]), rate: 0.4, once: true, width: 60, height: 100, shape: 'star' } },
    groundPulse('#ffd166', 60, 700),
    ...since(level, 3, masteryBurst(DAWN_PALETTE, '#ffe8a3', 'plus')),
  ],
});

// Сначала ищем по имени заклинания (SkillName с сервера), затем по типу урона
export const SPELL_VISUALS: Record<string, SpellVisualOf> = {
  Fireball: FIREBALL,
  'Flame Wave': FLAME_WAVE,
  Meteor: METEOR,
  'Frost Bolt': FROST_BOLT,
  Blizzard: BLIZZARD,
  'Glacial Prison': GLACIAL_PRISON,
  Lightning: LIGHTNING,
  'Chain Lightning': CHAIN_LIGHTNING,
  Thunderclap: THUNDERCLAP,
  'Healing Rune': HEALING_RUNE,
  'Venom Thorns': VENOM_THORNS,
  'Entangling Roots': ENTANGLING_ROOTS,
  'Shadow Bolt': SHADOW_BOLT,
  'Life Drain': LIFE_DRAIN,
  Hex: HEX,
  'Prism Ray': PRISM_RAY,
  'Radiant Nova': RADIANT_NOVA,
  Dawnlight: DAWNLIGHT,
  Fire: FIREBALL,
  Frost: FROST_BOLT,
  Poison: VENOM_THORNS,
  Dark: SHADOW_BOLT,
  Radiant: PRISM_RAY,
  Magical: () => ARCANE_BOLT,
};

// Уровень заклинания 1..3 (SkillLevel в логе каста); без уровня — первый
export const resolveSpellVisual = (skillName: string | undefined, damageType: string, level?: number | null): SpellVisual => {
  const of = (skillName && SPELL_VISUALS[skillName]) || SPELL_VISUALS[damageType];
  return of ? of(Math.min(Math.max(Math.round(level ?? 1), 1), 3) as SpellLevel) : ARCANE_BOLT;
};

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
  // Обледенение (Glacial Prison): юнит пропускает ход — полная заморозка анимации и плотная ледяная окраска
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
  // Яд: зелёные пузырьки поднимаются с юнита, силуэт мутно-зелёный
  Poison: {
    aura: [{ type: 'sparkles', duration: 0, args: { color: '#a3e635', rate: 0.25, width: 46, height: 70, shape: 'dot', rise: 0.6 } }],
    tint: { color: '#84cc16', alpha: 0.25, pulse: 1.2 },
    tick: HIT_VISUALS.Poison,
    tickFlash: '#a3e635',
    priority: 2,
  },
  // Проклятие: юнит тлеет фиолетовым пламенем тьмы
  Curse: {
    aura: [{ type: 'flames', duration: 0, args: { width: 40, height: 70, rate: 0.8, size: 6, colors: SHADOW, glow: 'rgba(126, 34, 206, 0.35)' } }],
    tint: { color: '#7e22ce', alpha: 0.3, pulse: 1.5 },
    tick: HIT_VISUALS.Dark,
    tickFlash: '#a855f7',
    priority: 2,
  },
  // Электрошок: по юниту пробегают искры, силуэт мерцает голубым
  Shock: {
    aura: [{ type: 'sparkles', duration: 0, args: { color: '#9be7ff', rate: 0.35, width: 50, height: 80, shape: 'star', rise: 0.2 } }],
    tint: { color: '#9be7ff', alpha: 0.2, pulse: 6 },
    tick: HIT_VISUALS.Lightning,
    tickFlash: '#e0fbff',
    priority: 1,
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
const AURA_SHADOW = aura({ color: '#7e22ce', glow: '#a855f7', accent: '#f3e8ff', palette: SHADOW, mote: 'dot', drift: -0.5, period: 2.4 }, 'dot');
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
  Curse: AURA_SHADOW,
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
