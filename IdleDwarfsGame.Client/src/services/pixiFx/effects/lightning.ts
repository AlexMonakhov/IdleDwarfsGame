import { Container } from 'pixi.js';
import { tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { BOLT_REBUILD_MS, jaggedPath } from '../geometry';
import { Strand } from '../strand';

// Молния (как LightningStrategy): ломаная от start к end с отростками, перестраивается раз в BOLT_REBUILD_MS
// (на 120 Гц не мерцает вдвое чаще). Каждый слой — нить (strand.ts): ореол шире и тусклее, разряд, белое ядро.
// args: color, jitter, width (толщина свечения), branches

const SEGMENTS = 12;
const FORK_SEGMENTS = 4;

export const createLightning: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color = tintOf(args.color, '#9be7ff');
  const glow = Number(args.width ?? 10);
  const jitter = Number(args.jitter ?? 18);
  const branches = Number(args.branches ?? 0);
  const layers = [[glow * 3, color, 0.1], [glow * 1.8, color, 0.22], [glow, color, 1], [glow * 0.3, 0xffffff, 1]] as const;

  const root = new Container();
  const main = layers.map(([width, tint, alpha]) => new Strand(SEGMENTS + 1, width, tint, alpha));
  const forks = Array.from({ length: branches }, () =>
    layers.map(([width, tint, alpha]) => new Strand(FORK_SEGMENTS + 1, width * 0.45, tint, alpha)));
  root.addChild(...main.map(s => s.rope), ...forks.flat().map(s => s.rope));
  let rebuildAt = 0;

  const rebuild = (sx: number, sy: number, ex: number, ey: number) => {
    const points = jaggedPath(sx, sy, ex, ey, SEGMENTS, jitter);
    main.forEach(s => s.set(points));
    const length = Math.hypot(ex - sx, ey - sy);
    for (const fork of forks) {
      const from = points[1 + Math.floor(Math.random() * (points.length - 3))];
      const angle = Math.atan2(ey - sy, ex - sx) + (Math.random() < 0.5 ? -1 : 1) * (0.4 + Math.random() * 0.6);
      const reach = length * (0.12 + Math.random() * 0.15);
      const path = jaggedPath(from.x, from.y, from.x + Math.cos(angle) * reach, from.y + Math.sin(angle) * reach, FORK_SEGMENTS, jitter * 0.4);
      fork.forEach(s => s.set(path));
    }
  };

  return {
    root,
    update(e) {
      root.alpha = e.opacity ?? 1;
      const now = performance.now();
      if (now < rebuildAt) return;
      rebuild(e.startX ?? e.x, e.startY ?? e.y, e.endX ?? e.x, e.endY ?? e.y);
      rebuildAt = now + BOLT_REBUILD_MS;
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
