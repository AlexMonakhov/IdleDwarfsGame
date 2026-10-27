// Арт рубашки карты (кристалл). Рисуется SVG-строкой, один раз растеризуется в картинку (на каждый цвет и стиль)
// и отдается в CSS: каждый осколок показывает свой кусок одной и той же готовой картинки.

const toDataUri = (svg: string) => `data:image/svg+xml,${encodeURIComponent(svg)}`;
const toUrl = (svg: string) => `url("${toDataUri(svg)}")`;

// Координаты рисунка: сама карта — 0..W x 0..H, вокруг запас BLEED под торчащие кристаллы
const W = 200;
const H = 300;
const BLEED = 8;

// Размер растра (~x2 от размера карты, чтобы было четко на hi-dpi)
const WIDTH = Math.round((W + 2 * BLEED) * 2.1);
const HEIGHT = Math.round((H + 2 * BLEED) * 2.1);

/**
 * Как разместить картинку относительно карты (в % от карты) и где внутри картинки сама карта (в % от картинки).
 * Используется компонентом для слоя породы и светящегося ядра.
 */
export const ART_LAYOUT = {
  left: `${(-BLEED / W) * 100}%`,
  top: `${(-BLEED / H) * 100}%`,
  width: `${((W + 2 * BLEED) / W) * 100}%`,
  height: `${((H + 2 * BLEED) / H) * 100}%`,
  insetX: `${(BLEED / (W + 2 * BLEED)) * 100}%`,
  insetY: `${(BLEED / (H + 2 * BLEED)) * 100}%`,
  cardWidth: `${(W / (W + 2 * BLEED)) * 100}%`,
  cardHeight: `${(H / (H + 2 * BLEED)) * 100}%`,
};

type P = [number, number];
type Rand = () => number;
const C: P = [W / 2, H / 2];

/** Стили рубашки */
export type CardBackVariant =
  | 'large'    // крупные грани, сходящиеся к точке
  | 'crystal'  // мелкие грани, сходящиеся к центру
  | 'cracked'  // кристалл, расколотый на куски-пирамидки
  | 'emerald'; // ступенчатая огранка

interface Palette {
  deep: string;
  mid: string;
  light: string;
  color: string;
}

/** Грань: многоугольник + базовая яркость 0..1 */
interface Facet {
  pts: P[];
  base: number;
}

interface VariantDef {
  seed: number;
  center: P;    // откуда идет свет
  noise: number; // разброс яркости граней
  frame: boolean; // ледяной скос по краю
  facets: (rand: Rand, center: P) => Facet[];
}

const lerp = (a: P, b: P, t: number): P => [a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t];
const fmt = (p: P) => `${p[0].toFixed(1)},${p[1].toFixed(1)}`;
const poly = (pts: P[], fill: string, extra = '') => `<polygon points='${pts.map(fmt).join(' ')}' fill='${fill}' ${extra}/>`;
const centroid = (pts: P[]): P => [
  pts.reduce((s, p) => s + p[0], 0) / pts.length,
  pts.reduce((s, p) => s + p[1], 0) / pts.length,
];
const area = (pts: P[]) =>
  Math.abs(pts.reduce((s, p, i) => {
    const q = pts[(i + 1) % pts.length];
    return s + p[0] * q[1] - q[0] * p[1];
  }, 0)) / 2;

/** Детерминированный рандом, чтобы рубашка всегда была одинаковой */
function seededRandom(seed: number): Rand {
  return () => {
    seed = (seed + 0x6d2b79f5) | 0;
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

/** Смешивает два hex-цвета (t = доля цвета b) */
function mixHex(a: string, b: string, t: number): string {
  const parse = (hex: string) => {
    let h = hex.trim().replace('#', '');
    if (h.length === 3) h = h.split('').map(c => c + c).join('');
    const n = parseInt(h, 16);
    return Number.isNaN(n) || h.length !== 6 ? null : [(n >> 16) & 255, (n >> 8) & 255, n & 255];
  };
  const ca = parse(a), cb = parse(b);
  if (!ca || !cb) return a;
  return '#' + ca.map((v, i) => Math.round(v + (cb[i] - v) * t).toString(16).padStart(2, '0')).join('');
}

/** Яркость 0..1 -> цвет грани (темный -> средний -> светлый) */
function shade(pal: Palette, l: number): string {
  const v = Math.min(1, Math.max(0, l));
  return v < 0.5 ? mixHex(pal.deep, pal.mid, v / 0.5) : mixHex(pal.mid, pal.light, (v - 0.5) / 0.5);
}

// ===================== Геометрия стилей =====================

/** Точки по периметру карты (углы + случайно сдвинутые точки на сторонах) */
function perimeterPoints(rand: Rand, short: number, long: number): P[] {
  const pts: P[] = [];
  const side = (from: P, to: P, count: number) => {
    for (let i = 0; i < count; i++) {
      const t = (i + (i === 0 ? 0 : (rand() - 0.5) * 0.6)) / count;
      pts.push(lerp(from, to, t));
    }
  };
  side([0, 0], [W, 0], short);
  side([W, 0], [W, H], long);
  side([W, H], [0, H], short);
  side([0, H], [0, 0], long);
  return pts;
}

/**
 * Сектора от центра к краям. rings — промежуточные кольца [минимум, разброс] в долях радиуса,
 * каждое кольцо режет сектор на две треугольные грани.
 */
function radialFacets(rand: Rand, center: P, short: number, long: number, rings: [number, number][]): Facet[] {
  const outer = perimeterPoints(rand, short, long);
  const levels = rings.map(([min, span]) => outer.map(p => lerp(center, p, min + rand() * span)));
  levels.push(outer);

  const facets: Facet[] = [];
  for (let i = 0; i < outer.length; i++) {
    const j = (i + 1) % outer.length;
    facets.push({ pts: [center, levels[0][i], levels[0][j]], base: 0.68 });

    for (let k = 0; k < levels.length - 1; k++) {
      const a = levels[k], b = levels[k + 1];
      const swap = rand() < 0.5;
      const base = 0.56 - k * 0.12;
      facets.push({ pts: [a[i], b[i], b[j]], base: swap ? base : base - 0.18 });
      facets.push({ pts: [a[i], b[j], a[j]], base: swap ? base - 0.18 : base });
    }
  }
  return facets;
}

/** Режет выпуклый многоугольник прямой (точка p, направление d) на два */
function splitPolygon(pts: P[], p: P, d: P): [P[], P[]] {
  const side = (v: P) => d[0] * (v[1] - p[1]) - d[1] * (v[0] - p[0]);
  const left: P[] = [], right: P[] = [];
  pts.forEach((a, i) => {
    const b = pts[(i + 1) % pts.length];
    const sa = side(a), sb = side(b);
    (sa >= 0 ? left : right).push(a);
    if ((sa > 0 && sb < 0) || (sa < 0 && sb > 0)) {
      const x = lerp(a, b, sa / (sa - sb));
      left.push(x);
      right.push(x);
    }
  });
  return [left, right];
}

/** Карта колется прямыми на куски, каждый кусок — пирамидка с гранями, освещенными сверху-слева */
function crackedFacets(rand: Rand): Facet[] {
  let pieces: P[][] = [[[0, 0], [W, 0], [W, H], [0, H]]];

  for (let guard = 0; pieces.length < 11 && guard < 200; guard++) {
    pieces.sort((a, b) => area(b) - area(a));
    const [piece, ...rest] = pieces;
    const c = centroid(piece);
    const p: P = [c[0] + (rand() - 0.5) * 16, c[1] + (rand() - 0.5) * 24];
    const angle = rand() * Math.PI;
    const [a, b] = splitPolygon(piece, p, [Math.cos(angle), Math.sin(angle)]);
    if (a.length >= 3 && b.length >= 3 && area(a) > 300 && area(b) > 300) {
      pieces = [...rest, a, b];
    }
  }

  const light: P = [-0.55, -0.83];
  const facets: Facet[] = [];
  for (const piece of pieces) {
    // Вершина пирамидки чуть смещена от центра куска
    const apex = lerp(centroid(piece), piece[Math.floor(rand() * piece.length)], 0.1 + rand() * 0.25);
    const tone = (rand() - 0.5) * 0.15;
    piece.forEach((v, k) => {
      const next = piece[(k + 1) % piece.length];
      const mid = lerp(v, next, 0.5);
      const len = Math.hypot(mid[0] - apex[0], mid[1] - apex[1]) || 1;
      const facing = ((mid[0] - apex[0]) * light[0] + (mid[1] - apex[1]) * light[1]) / len;
      facets.push({ pts: [apex, v, next], base: 0.5 + facing * 0.32 + tone });
    });
  }
  return facets;
}

/** Ступенчатая огранка: вложенные восьмиугольники, грани освещены по направлению */
function emeraldFacets(): Facet[] {
  const steps = [0, 15, 31, 48, 66];
  const octagon = (d: number): P[] => {
    const c = 30 - d * 0.25;
    return [
      [d + c, d], [W - d - c, d], [W - d, d + c], [W - d, H - d - c],
      [W - d - c, H - d], [d + c, H - d], [d, H - d - c], [d, d + c],
    ];
  };
  // верх, угол справа-сверху, право, угол справа-снизу, низ, ...
  const dirBase = [0.95, 0.72, 0.42, 0.28, 0.2, 0.38, 0.68, 0.88];

  // Углы карты за внешним восьмиугольником
  const o = octagon(0);
  const facets: Facet[] = [
    { pts: [[0, 0], o[0], o[7]], base: 0.9 },
    { pts: [[W, 0], o[2], o[1]], base: 0.6 },
    { pts: [[W, H], o[4], o[3]], base: 0.22 },
    { pts: [[0, H], o[6], o[5]], base: 0.45 },
  ];
  for (let k = 0; k < steps.length - 1; k++) {
    const a = octagon(steps[k]), b = octagon(steps[k + 1]);
    for (let s = 0; s < 8; s++) {
      const t = (s + 1) % 8;
      // Соседние ступени отражают свет по-разному
      facets.push({ pts: [a[s], a[t], b[t], b[s]], base: dirBase[s] + (k % 2 ? -0.2 : 0.05) });
    }
  }

  // Площадка в центре: веер из 8 треугольников, отражает свет мягче и наоборот ступеням
  const table = octagon(steps[steps.length - 1]);
  const tc = centroid(table);
  for (let s = 0; s < 8; s++) {
    facets.push({ pts: [tc, table[s], table[(s + 1) % 8]], base: 0.4 + dirBase[(s + 4) % 8] * 0.4 });
  }
  return facets;
}

const VARIANTS: Record<CardBackVariant, VariantDef> = {
  large: {
    seed: 21, center: [94, 132], noise: 0.22, frame: true,
    facets: (rand, center) => radialFacets(rand, center, 3, 4, [[0.38, 0.25]]),
  },
  crystal: {
    seed: 1337, center: C, noise: 0.3, frame: true,
    facets: (rand, center) => radialFacets(rand, center, 6, 9, [[0.3, 0.25], [0.62, 0.2]]),
  },
  cracked: {
    seed: 7, center: C, noise: 0.12, frame: true,
    facets: rand => crackedFacets(rand),
  },
  emerald: {
    seed: 1, center: C, noise: 0.05, frame: false,
    facets: () => emeraldFacets(),
  },
};

// ===================== Отрисовка =====================

/** Цвета граней с учетом света (без палитры — только контуры, для маски) */
function renderFacets(facets: Facet[], pal: Palette | null, rand: Rand, center: P, noise: number): string {
  return facets.map(f => {
    if (!pal) return poly(f.pts, 'none');
    const [cx, cy] = centroid(f.pts);
    const dist = Math.hypot(cx - center[0], (cy - center[1]) * 0.7);
    const l = f.base
      + (rand() - 0.5) * noise
      + Math.max(0, 1 - dist / 110) * 0.2           // ярче к центру
      + ((W - cx) / W + (H - cy) / H - 1) * 0.12;   // свет сверху-слева
    return poly(f.pts, shade(pal, l));
  }).join('');
}

// ===================== Ледяная кромка =====================

interface FrameGeometry {
  outer: P[];   // рваный внешний силуэт (местами торчит за карту)
  inner: P[];   // неровный внутренний край
  normals: P[]; // наружная нормаль в каждой точке
  spikes: P[][]; // кристаллы, врезающиеся внутрь карты [a, вершина, b]
}

/** Точка на периметре карты по длине дуги s + наружная нормаль */
function rectPoint(s: number): { p: P; n: P } {
  if (s < W) return { p: [s, 0], n: [0, -1] };
  if (s < W + H) return { p: [W, s - W], n: [1, 0] };
  if (s < 2 * W + H) return { p: [W - (s - W - H), H], n: [0, 1] };
  return { p: [0, H - (s - 2 * W - H)], n: [-1, 0] };
}

const CORNERS: { s: number; p: P; n: P }[] = [
  { s: 0, p: [0, 0], n: [-1, -1] },
  { s: W, p: [W, 0], n: [1, -1] },
  { s: W + H, p: [W, H], n: [1, 1] },
  { s: 2 * W + H, p: [0, H], n: [-1, 1] },
];

/** Параметры ледяной кромки (в единицах рисунка: карта 200 x 300) */
export interface FrameOptions {
  step: number;           // средний шаг между кусками: больше — крупнее куски
  protrudeChance: number; // доля кусков, торчащих наружу (0..1)
  protrude: number;       // насколько торчат наружу, максимум BLEED
  depth: number;          // средняя толщина кромки
  depthJitter: number;    // разброс толщины
  spikeChance: number;    // доля кристаллов, врезающихся внутрь (0..1)
  spikeLength: number;    // максимальная длина кристаллов внутрь
  seed?: number;          // другой seed — другой рисунок кромки
}

export const DEFAULT_FRAME: FrameOptions = {
  step: 21,
  protrudeChance: 0.3,
  protrude: 8,
  depth: 14.5,
  depthJitter: 9,
  spikeChance: 0.15,
  spikeLength: 20,
};

/** Все настройки рубашки одним объектом (проп `back` у CardComponent) */
export interface CardBackOptions {
  variant?: CardBackVariant;     // стиль граней, по умолчанию 'large'
  seed?: number;                 // рисунок граней (у каждого стиля свой seed по умолчанию)
  frame?: Partial<FrameOptions>; // ледяная кромка, не заданное — из DEFAULT_FRAME
  // Готовая картинка вместо генерации (url, например import из assets). Холст как у запеченных:
  // карта с запасом BLEED вокруг (216 x 316 в единицах рисунка). variant/seed/frame тогда не используются
  image?: string;
  tint?: boolean; // перекрасить картинку в цвет редкости (яркость сохраняется, оттенок — от цвета)
}

interface ResolvedBack {
  def: VariantDef;
  seed: number;
  frame: FrameOptions;
  key: string; // для кешей
}

function resolveBack(back?: CardBackOptions): ResolvedBack {
  const variant = back?.variant ?? 'large';
  const def = VARIANTS[variant] ?? VARIANTS.large;
  const seed = back?.seed ?? def.seed;
  const frame: FrameOptions = { ...DEFAULT_FRAME, ...back?.frame };
  return { def, seed, frame, key: JSON.stringify({ variant, seed, frame }) };
}

function frameGeometry(rand: Rand, opts: FrameOptions): FrameGeometry {
  const perimeter = 2 * (W + H);
  const step = Math.max(4, opts.step);
  const samples: { p: P; n: P }[] = [];
  CORNERS.forEach((corner, c) => {
    samples.push(corner);
    const end = c < 3 ? CORNERS[c + 1].s : perimeter;
    for (let s = corner.s + step * (0.5 + rand() * 0.5); s < end - step * 0.5; s += step * (0.67 + rand() * 0.67)) {
      samples.push(rectPoint(s));
    }
  });

  const maxOut = Math.min(opts.protrude, BLEED);
  const outer: P[] = [];
  const inner: P[] = [];
  samples.forEach(({ p, n }) => {
    // Иногда кристалл торчит наружу, в остальном край почти ровный
    const out = rand() < opts.protrudeChance
      ? Math.min(3, maxOut) + rand() * Math.max(0, maxOut - 3)
      : rand() * Math.min(1.5, maxOut);
    const depth = Math.max(2, opts.depth + (rand() - 0.5) * opts.depthJitter);
    outer.push([p[0] + n[0] * out, p[1] + n[1] * out]);
    inner.push([p[0] - n[0] * depth, p[1] - n[1] * depth]);
  });

  const spikes: P[][] = [];
  inner.forEach((a, i) => {
    if (rand() >= opts.spikeChance) return;
    const j = (i + 1) % inner.length;
    const b = inner[j];
    // У угловой точки нормаль диагональная — берем нормаль соседа на стороне
    const n = samples[i].n[0] !== 0 && samples[i].n[1] !== 0 ? samples[j].n : samples[i].n;
    const mid = lerp(a, b, 0.3 + rand() * 0.4);
    const len = opts.spikeLength * (0.4 + rand() * 0.6);
    spikes.push([a, [mid[0] - n[0] * len, mid[1] - n[1] * len], b]);
  });

  return { outer, inner, normals: samples.map(s => s.n), spikes };
}

const LIGHT: P = [-0.55, -0.83]; // свет сверху-слева
const facing = (n: P) => (n[0] * LIGHT[0] + n[1] * LIGHT[1]) / Math.hypot(n[0], n[1]);

/** Ледяная кромка: сколы между внешним силуэтом и внутренним краем + торчащие внутрь кристаллы */
function iceFrame(pal: Palette, geo: FrameGeometry, rand: Rand): string {
  const { outer, inner, normals, spikes } = geo;
  let out = '';

  outer.forEach((o, i) => {
    const j = (i + 1) % outer.length;
    const base = 0.6 + facing(normals[i]) * 0.3 + (rand() - 0.5) * 0.2;
    const tilt = 0.08 + rand() * 0.14; // одна половина скола светлее, другая темнее
    if (rand() < 0.5) {
      out += poly([o, outer[j], inner[j]], shade(pal, base + tilt));
      out += poly([o, inner[j], inner[i]], shade(pal, base - tilt));
    } else {
      out += poly([o, outer[j], inner[i]], shade(pal, base + tilt));
      out += poly([outer[j], inner[j], inner[i]], shade(pal, base - tilt));
    }
  });

  let spikeLines = '';
  for (const [a, apex, b] of spikes) {
    const mid = lerp(a, b, 0.5);
    out += poly([a, apex, mid], shade(pal, 0.85 + (rand() - 0.5) * 0.15));
    out += poly([mid, apex, b], shade(pal, 0.42 + (rand() - 0.5) * 0.15));
    spikeLines += `<polyline points='${[a, apex, b].map(fmt).join(' ')}'/>`;
  }

  return `
    <g stroke='#fff' stroke-opacity='0.35' stroke-width='0.5' stroke-linejoin='round'>${out}</g>
    <polygon points='${inner.map(fmt).join(' ')}' fill='none' stroke='#fff' stroke-opacity='0.7' stroke-width='0.9' stroke-linejoin='round'/>
    <g fill='none' stroke='#fff' stroke-opacity='0.6' stroke-width='0.7' stroke-linejoin='round'>${spikeLines}</g>`;
}

/** Силуэт рубашки: рваный для стилей с кромкой, скругленный прямоугольник для остальных */
function silhouette(frame: FrameGeometry | null, attrs: string): string {
  return frame
    ? `<polygon points='${frame.outer.map(fmt).join(' ')}' ${attrs}/>`
    : `<rect width='${W}' height='${H}' rx='10' ${attrs}/>`;
}

const frameFor = (back: ResolvedBack) =>
  back.def.frame ? frameGeometry(seededRandom(back.frame.seed ?? back.seed + 2), back.frame) : null;

// Картинка рисуется с запасом BLEED вокруг карты, чтобы кристаллы могли торчать наружу
const svgRoot = (body: string, size = '') =>
  `<svg xmlns='http://www.w3.org/2000/svg' viewBox='${-BLEED} ${-BLEED} ${W + 2 * BLEED} ${H + 2 * BLEED}' preserveAspectRatio='none' ${size}>${body}</svg>`;

function buildCardBackSvg(color: string, back: ResolvedBack): string {
  const { def, seed } = back;
  // Средний тон — почти чистый цвет редкости, чтобы кристалл был насыщенным, а не серым
  const pal: Palette = {
    deep: mixHex(color, '#0b1020', 0.5),
    mid: mixHex(color, '#ffffff', 0.12),
    light: mixHex(color, '#ffffff', 0.7),
    color,
  };
  const facets = def.facets(seededRandom(seed), def.center);
  const frame = frameFor(back);

  return svgRoot(`
    <defs>
      <clipPath id='sil'>${silhouette(frame, '')}</clipPath>
      <radialGradient id='core' cx='${def.center[0] / W}' cy='${def.center[1] / H}' r='0.35'>
        <stop offset='0' stop-color='#fff' stop-opacity='0.55'/>
        <stop offset='0.4' stop-color='${pal.light}' stop-opacity='0.25'/>
        <stop offset='1' stop-color='${color}' stop-opacity='0'/>
      </radialGradient>
      <linearGradient id='gloss' x1='0' y1='0' x2='1' y2='1'>
        <stop offset='0' stop-color='#fff' stop-opacity='0.28'/>
        <stop offset='0.35' stop-color='#fff' stop-opacity='0'/>
        <stop offset='0.6' stop-color='#fff' stop-opacity='0'/>
        <stop offset='0.75' stop-color='#fff' stop-opacity='0.12'/>
        <stop offset='1' stop-color='#fff' stop-opacity='0'/>
      </linearGradient>
    </defs>
    <g clip-path='url(#sil)'>
      <rect x='${-BLEED}' y='${-BLEED}' width='${W + 2 * BLEED}' height='${H + 2 * BLEED}' fill='${pal.deep}'/>
      <g stroke='#fff' stroke-opacity='0.3' stroke-width='0.5' stroke-linejoin='round'>
        ${renderFacets(facets, pal, seededRandom(seed + 1), def.center, def.noise)}
      </g>
      <rect width='${W}' height='${H}' fill='url(#core)' style='mix-blend-mode:screen'/>
      <rect x='${-BLEED}' y='${-BLEED}' width='${W + 2 * BLEED}' height='${H + 2 * BLEED}' fill='url(#gloss)'/>
      ${frame ? iceFrame(pal, frame, seededRandom(seed + 3)) : ''}
    </g>
    ${silhouette(frame, `fill='none' stroke='${pal.deep}' stroke-width='1.4' stroke-linejoin='round'`)}`,
    `width='${WIDTH}' height='${HEIGHT}'`);
}

async function loadImage(src: string): Promise<HTMLImageElement> {
  const img = new Image();
  img.src = src;
  await img.decode();
  return img;
}

/** Рисует картинку на canvas (draw — что еще сделать поверх) и отдает webp-blob */
async function toWebp(img: HTMLImageElement, width: number, height: number, draw?: (ctx: CanvasRenderingContext2D) => void): Promise<Blob> {
  const canvas = document.createElement('canvas');
  canvas.width = width;
  canvas.height = height;
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('no 2d context');
  ctx.drawImage(img, 0, 0, width, height);
  draw?.(ctx);

  const blob = await new Promise<Blob | null>(resolve => canvas.toBlob(resolve, 'image/webp', 0.9));
  if (!blob) throw new Error('toBlob failed');
  return blob;
}

const blobUrl = (blob: Blob) => `url("${URL.createObjectURL(blob)}")`;

async function rasterize(svg: string): Promise<string> {
  const src = toDataUri(svg);
  try {
    return blobUrl(await toWebp(await loadImage(src), WIDTH, HEIGHT));
  } catch {
    // Если браузер не дал растеризовать — используем SVG напрямую (медленнее, но работает)
    return `url("${src}")`;
  }
}

/** Перекрашивает готовую картинку в цвет редкости: яркость своя, оттенок и насыщенность — от цвета */
async function tintImage(src: string, color: string): Promise<string> {
  try {
    const img = await loadImage(src);
    return blobUrl(await toWebp(img, img.naturalWidth, img.naturalHeight, ctx => {
      ctx.globalCompositeOperation = 'color';
      ctx.fillStyle = color;
      ctx.fillRect(0, 0, img.naturalWidth, img.naturalHeight);
      // Заливка закрасила и прозрачные места — возвращаем прозрачность исходной картинки
      ctx.globalCompositeOperation = 'destination-in';
      ctx.drawImage(img, 0, 0);
    }));
  } catch {
    return `url("${src}")`;
  }
}

const backCache = new Map<string, Promise<string>>();

/** Готовая картинка рубашки для цвета редкости и настроек (CSS url(...)), кешируется */
export function getCardBack(color: string, options?: CardBackOptions): Promise<string> {
  if (options?.image && !options.tint) return Promise.resolve(`url("${options.image}")`);

  const key = options?.image ? `image|${options.image}|${color}` : `${resolveBack(options).key}|${color}`;
  let result = backCache.get(key);
  if (!result) {
    result = options?.image
      ? tintImage(options.image, color)
      : rasterize(buildCardBackSvg(color, resolveBack(options)));
    backCache.set(key, result);
  }
  return result;
}

/** SVG-исходник рубашки (для экспорта и доработки в редакторе) */
export function getCardBackSvg(color: string, options?: CardBackOptions): string {
  return buildCardBackSvg(color, resolveBack(options));
}

/** Скачивает рубашку файлом: .svg (редактируемый вектор) или .webp (как ее видит игра) */
export async function downloadCardBack(color: string, options: CardBackOptions | undefined, fileName: string, format: 'svg' | 'webp'): Promise<void> {
  const svg = getCardBackSvg(color, options);
  const blob = format === 'svg'
    ? new Blob([svg], { type: 'image/svg+xml' })
    : await toWebp(await loadImage(toDataUri(svg)), WIDTH, HEIGHT);

  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = `${fileName}.${format}`;
  link.click();
  URL.revokeObjectURL(url);
}

// Для готовой картинки граней нет — пульсирует мягкое свечение из центра
const IMAGE_GLOW_MASK = toUrl(svgRoot(`
  <defs>
    <radialGradient id='c' cx='0.5' cy='0.5' r='0.45'>
      <stop offset='0' stop-color='#fff' stop-opacity='0.7'/>
      <stop offset='1' stop-color='#fff' stop-opacity='0'/>
    </radialGradient>
  </defs>
  <rect width='${W}' height='${H}' fill='url(#c)'/>`));

const glowCache = new Map<string, string>();

/** Рёбра граней + мягкий центр — маска для пульсирующего свечения на целой карте */
export function getCardGlowMask(options?: CardBackOptions): string {
  if (options?.image) return IMAGE_GLOW_MASK;
  const back = resolveBack(options);
  const key = back.key;
  let mask = glowCache.get(key);
  if (!mask) {
    const { def, seed } = back;
    const facets = def.facets(seededRandom(seed), def.center);
    const frame = frameFor(back);
    // Свечение только внутри кромки, чтобы рёбра под ней не просвечивали
    const area = frame
      ? `<polygon points='${frame.inner.map(fmt).join(' ')}'/>`
      : silhouette(null, '');
    mask = toUrl(svgRoot(`
      <defs>
        <filter id='b' x='-5%' y='-5%' width='110%' height='110%'><feGaussianBlur stdDeviation='1.2'/></filter>
        <radialGradient id='c' cx='${def.center[0] / W}' cy='${def.center[1] / H}' r='0.3'>
          <stop offset='0' stop-color='#fff' stop-opacity='0.8'/>
          <stop offset='1' stop-color='#fff' stop-opacity='0'/>
        </radialGradient>
        <clipPath id='a'>${area}</clipPath>
      </defs>
      <g clip-path='url(#a)'>
        <rect width='${W}' height='${H}' fill='url(#c)'/>
        <g filter='url(#b)' stroke='#fff' stroke-width='1.2' stroke-linejoin='round'>
          ${renderFacets(facets, null, seededRandom(0), def.center, 0)}
        </g>
      </g>`));
    glowCache.set(key, mask);
  }
  return mask;
}

const silhouetteCache = new Map<string, string>();

/** Силуэт рубашки — маска для эффектов поверх нее (чтобы не рисовать в прозрачных местах вокруг) */
export function getCardSilhouetteMask(options?: CardBackOptions): string {
  // У готовой картинки силуэт — ее собственная прозрачность
  if (options?.image) return `url("${options.image}")`;
  const back = resolveBack(options);
  const key = back.key;
  let mask = silhouetteCache.get(key);
  if (!mask) {
    mask = toUrl(svgRoot(silhouette(frameFor(back), `fill='#fff'`)));
    silhouetteCache.set(key, mask);
  }
  return mask;
}
