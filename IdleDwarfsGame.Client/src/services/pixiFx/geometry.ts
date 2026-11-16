// Геометрия эффектов без отрисовки

export type BoltPath = { x: number; y: number }[];

// Ломаная между двумя точками со случайным смещением узлов (молнии, дуги)
export const jaggedPath = (sx: number, sy: number, ex: number, ey: number, segments: number, jitter: number): BoltPath => {
  const points = [{ x: sx, y: sy }];
  for (let i = 1; i < segments; i++) {
    const t = i / segments;
    points.push({
      x: sx + (ex - sx) * t + (Math.random() - 0.5) * jitter * 2,
      y: sy + (ey - sy) * t + (Math.random() - 0.5) * jitter * 2,
    });
  }
  points.push({ x: ex, y: ey });
  return points;
};

// Как часто перестраиваются молнии и дуги (3 кадра при 60 Гц: на 120 Гц не мерцают вдвое чаще), мс
export const BOLT_REBUILD_MS = 50;

// Концы эмиттера: откуда и куда
export const endpoints = (e: { x: number; y: number; startX?: number; startY?: number; endX?: number; endY?: number }) => ({
  sx: e.startX ?? e.x,
  sy: e.startY ?? e.y,
  ex: e.endX ?? e.x,
  ey: e.endY ?? e.y,
});

// Плавное появление за fadeIn и угасание за fadeOut (доли жизни)
export const envelope = (p: number, fadeIn: number, fadeOut: number) =>
  Math.min(Math.min(Math.max(p / fadeIn, 0), 1), Math.min(Math.max((1 - p) / fadeOut, 0), 1));
