// Геометрия кристалла для экрана загрузки. Всё в координатах 0..100 (viewBox SVG), центр в (50, 50).

export interface Pt { x: number; y: number }

export interface Facet {
  id: number
  points: Pt[]
  cx: number
  cy: number
  // Светлота грани: > 0 — светлее (к белому), < 0 — темнее (к черному). Свет падает слева сверху
  shade: number
  // Очередь сборки 0..1: грань с меньшим значением встает на место раньше
  order: number
  // Куда грань отлетает, пока кристалл расколот
  scatter: { x: number; y: number; rotate: number }
}

export type GemShape = 'brilliant' | 'crystal';
export type CrystalVariant = 'assemble' | 'heal' | 'shards';

// Детерминированный рандом: одинаковый seed — одинаковый раскол
export function createRandom(seed: number): () => number {
  let a = seed >>> 0;
  return () => {
    a = (a + 0x6d2b79f5) >>> 0;
    let t = a;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export const toPoints = (pts: Pt[]) => pts.map(p => `${p.x.toFixed(2)},${p.y.toFixed(2)}`).join(' ');

const LIGHT = { x: -0.6, y: -0.8 };

const ring = (n: number, r: number, offsetDeg: number, sx = 1, sy = 1): Pt[] =>
  Array.from({ length: n }, (_, i) => {
    const a = ((i * 360) / n + offsetDeg - 90) * (Math.PI / 180);
    return { x: 50 + Math.cos(a) * r * sx, y: 50 + Math.sin(a) * r * sy };
  });

// Внешний контур и внутренняя площадка (table) огранки
function gemRings(shape: GemShape): { outer: Pt[]; inner: Pt[] } {
  if (shape === 'crystal') {
    // Вытянутый шестигранный кристалл с острыми концами сверху и снизу
    return {
      outer: [
        { x: 50, y: 2 }, { x: 84, y: 23 }, { x: 84, y: 77 },
        { x: 50, y: 98 }, { x: 16, y: 77 }, { x: 16, y: 23 },
      ],
      inner: [
        { x: 50, y: 24 }, { x: 64, y: 35 }, { x: 64, y: 65 },
        { x: 50, y: 76 }, { x: 36, y: 65 }, { x: 36, y: 35 },
      ],
    };
  }
  // Круглый бриллиант (вид сверху): площадка повернута на полшага относительно рундиста
  const n = 10;
  return { outer: ring(n, 47, 0), inner: ring(n, 25, 180 / n) };
}

export function gemOutline(shape: GemShape): Pt[] {
  return gemRings(shape).outer;
}

const centroid = (pts: Pt[]): Pt => ({
  x: pts.reduce((s, p) => s + p.x, 0) / pts.length,
  y: pts.reduce((s, p) => s + p.y, 0) / pts.length,
});

function makeScatter(c: Pt, rand: () => number, strength: number) {
  const dx = c.x - 50, dy = c.y - 50;
  const len = Math.hypot(dx, dy) || 1;
  const dist = strength * (0.6 + rand() * 0.6);
  const jitter = (rand() - 0.5) * 0.8;
  const ang = Math.atan2(dy, dx) + jitter;
  return { x: Math.cos(ang) * dist + (dx / len) * 2, y: Math.sin(ang) * dist + (dy / len) * 2, rotate: (rand() - 0.5) * 70 };
}

/** Грани огранки: веер треугольников из центра площадки + кольцо треугольников до рундиста */
export function gemFacets(shape: GemShape, rand: () => number): Facet[] {
  const { outer, inner } = gemRings(shape);
  const n = outer.length;
  const center = { x: 50, y: 50 };
  const tris: { pts: Pt[]; kind: 'table' | 'crownA' | 'crownB' }[] = [];

  for (let i = 0; i < n; i++) {
    const j = (i + 1) % n;
    tris.push({ pts: [center, inner[i], inner[j]], kind: 'table' });
    tris.push({ pts: [outer[i], outer[j], inner[i]], kind: 'crownA' });
    tris.push({ pts: [inner[i], outer[j], inner[j]], kind: 'crownB' });
  }

  return tris.map((t, id) => {
    const c = centroid(t.pts);
    const dx = c.x - 50, dy = c.y - 50;
    const len = Math.hypot(dx, dy) || 1;
    const dot = (dx / len) * LIGHT.x + (dy / len) * LIGHT.y;
    const shade = t.kind === 'table'
      ? dot * 0.18 + (rand() - 0.5) * 0.12 + 0.08
      : dot * 0.38 + (t.kind === 'crownB' ? 0.1 : -0.06) + (rand() - 0.5) * 0.1;
    return {
      id,
      points: t.pts,
      cx: c.x,
      cy: c.y,
      shade: Math.max(-0.55, Math.min(0.55, shade)),
      order: rand(),
      scatter: makeScatter(c, rand, 22),
    };
  });
}

/** Осколки как у CardComponent: сетка со сдвинутыми узлами. Собираются от центра к краям */
export function gridShards(cols: number, rows: number, rand: () => number): Facet[] {
  const cw = 100 / cols, ch = 100 / rows;
  const pts: Pt[][] = [];
  for (let i = 0; i <= rows; i++) {
    const row: Pt[] = [];
    for (let j = 0; j <= cols; j++) {
      let x = j * cw, y = i * ch;
      if (i > 0 && i < rows && j > 0 && j < cols) {
        x += (rand() - 0.5) * cw * 0.9;
        y += (rand() - 0.5) * ch * 0.9;
      }
      row.push({ x, y });
    }
    pts.push(row);
  }

  const result: Facet[] = [];
  let id = 0;
  for (let i = 0; i < rows; i++) {
    for (let j = 0; j < cols; j++) {
      const quad = [pts[i][j], pts[i][j + 1], pts[i + 1][j + 1], pts[i + 1][j]];
      // Каждый четырехугольник режем по диагонали — граней больше, сколы острее
      const tris = rand() < 0.5
        ? [[quad[0], quad[1], quad[2]], [quad[0], quad[2], quad[3]]]
        : [[quad[0], quad[1], quad[3]], [quad[1], quad[2], quad[3]]];
      for (const tri of tris) {
        const c = centroid(tri);
        const dist = Math.hypot(c.x - 50, c.y - 50) / 70;
        result.push({
          id: id++,
          points: tri,
          cx: c.x,
          cy: c.y,
          shade: ((c.x - 50) * LIGHT.x + (c.y - 50) * LIGHT.y) / 140 + (rand() - 0.5) * 0.35,
          order: Math.min(1, dist * 0.8 + rand() * 0.25),
          scatter: makeScatter(c, rand, 8),
        });
      }
    }
  }
  return result;
}

/** Трещины из точки удара: ломаные до края + короткие ответвления */
export function generateCracks(rand: () => number, count = 6): string[] {
  const ox = 50 + (rand() - 0.5) * 18;
  const oy = 46 + (rand() - 0.5) * 18;
  const paths: string[] = [];

  const walk = (x: number, y: number, a: number, maxLen: number, branch: boolean) => {
    let d = `M${x.toFixed(1)} ${y.toFixed(1)}`;
    let len = 0;
    while (Math.hypot(x - 50, y - 50) < 52 && len < maxLen) {
      a += (rand() - 0.5) * 0.8;
      const step = 4 + rand() * 6;
      x += Math.cos(a) * step;
      y += Math.sin(a) * step;
      len += step;
      d += ` L${x.toFixed(1)} ${y.toFixed(1)}`;
      if (branch && rand() < 0.22) walk(x, y, a + (rand() < 0.5 ? 1 : -1) * (0.5 + rand() * 0.6), 10 + rand() * 14, false);
    }
    paths.push(d);
  };

  for (let k = 0; k < count; k++) {
    walk(ox, oy, (k / count) * Math.PI * 2 + (rand() - 0.5) * 0.7, 999, true);
  }
  return paths;
}

// ---------------- Вытянутый кристалл-полоса (CrystalBar). Координаты 0..400 × 0..60 ----------------

export const BAR_W = 400;
export const BAR_H = 60;
const BAR_TIP = 16;

export function barOutline(): Pt[] {
  return [
    { x: 0, y: 30 }, { x: BAR_TIP, y: 3 }, { x: BAR_W - BAR_TIP, y: 3 },
    { x: BAR_W, y: 30 }, { x: BAR_W - BAR_TIP, y: 57 }, { x: BAR_TIP, y: 57 },
  ];
}

/** Осколки по сетке со сдвинутыми узлами; фронт кристаллизации рваный */
export function barShards(rand: () => number, cols = 26, rows = 3): Facet[] {
  const cw = BAR_W / cols, ch = BAR_H / rows;
  const pts: Pt[][] = [];
  for (let i = 0; i <= rows; i++) {
    const row: Pt[] = [];
    for (let j = 0; j <= cols; j++) {
      let x = j * cw, y = i * ch;
      if (i > 0 && i < rows && j > 0 && j < cols) { x += (rand() - 0.5) * cw * 0.9; y += (rand() - 0.5) * ch * 0.8; }
      else if (j > 0 && j < cols) x += (rand() - 0.5) * cw * 0.6;
      row.push({ x, y });
    }
    pts.push(row);
  }
  const result: Facet[] = [];
  let id = 0;
  for (let i = 0; i < rows; i++) {
    for (let j = 0; j < cols; j++) {
      const q = [pts[i][j], pts[i][j + 1], pts[i + 1][j + 1], pts[i + 1][j]];
      const tris = rand() < 0.5 ? [[q[0], q[1], q[2]], [q[0], q[2], q[3]]] : [[q[0], q[1], q[3]], [q[1], q[2], q[3]]];
      for (const tri of tris) {
        const c = centroid(tri);
        result.push({
          id: id++,
          points: tri,
          cx: c.x,
          cy: c.y,
          shade: (30 - c.y) / 110 + (rand() - 0.5) * 0.4,
          order: Math.min(1, Math.max(0, c.x / BAR_W + (rand() - 0.5) * 0.12)),
          scatter: { x: (rand() - 0.5) * 6, y: (rand() - 0.5) * 8, rotate: (rand() - 0.5) * 40 },
        });
      }
    }
  }
  return result;
}

export interface Spike {
  id: number
  // Основание по центру, на нем кристалл растет вверх
  x: number
  angle: number
  back: boolean
  order: number
  faces: { points: Pt[]; shade: number }[]
}

/** Друза: ряд шестигранных кристаллов, растущих из основания полосы. Задний ряд выше и темнее */
export function barSpikes(rand: () => number): Spike[] {
  const spikes: Spike[] = [];
  const base = 58;
  const make = (x: number, w: number, h: number, back: boolean) => {
    const shoulder = base - h + w * 0.55;
    const tip = { x, y: base - h };
    const bl = { x: x - w / 2, y: base }, br = { x: x + w / 2, y: base };
    const sl = { x: x - w / 2, y: shoulder }, sr = { x: x + w / 2, y: shoulder };
    const ridgeBottom = { x: x + w * 0.08, y: base }, ridgeTop = { x: x + w * 0.08, y: shoulder + w * 0.1 };
    spikes.push({
      id: spikes.length,
      x,
      angle: (rand() - 0.5) * (back ? 22 : 34),
      back,
      order: Math.min(1, Math.max(0, x / BAR_W + (rand() - 0.5) * 0.04)),
      faces: [
        { points: [bl, sl, ridgeTop, ridgeBottom], shade: 0.22 + (rand() - 0.5) * 0.1 },
        { points: [ridgeBottom, ridgeTop, sr, br], shade: -0.22 + (rand() - 0.5) * 0.1 },
        { points: [sl, tip, ridgeTop], shade: 0.42 },
        { points: [ridgeTop, tip, sr], shade: 0 },
      ],
    });
  };
  for (let i = 0; i < 13; i++) make(14 + (i / 12) * 372 + (rand() - 0.5) * 14, 18 + rand() * 8, 40 + rand() * 16, true);
  for (let i = 0; i < 17; i++) make(8 + (i / 16) * 384 + (rand() - 0.5) * 12, 14 + rand() * 8, 20 + rand() * 18, false);
  // Задний ряд рисуем первым
  return spikes.sort((a, b) => Number(b.back) - Number(a.back));
}
