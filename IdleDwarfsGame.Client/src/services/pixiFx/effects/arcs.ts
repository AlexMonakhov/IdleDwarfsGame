import { Container } from 'pixi.js';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { BOLT_REBUILD_MS, jaggedPath } from '../geometry';
import { rand } from '../particles';
import { Strand } from '../strand';

// Электрические дуги по телу цели (как ArcsStrategy): перестраиваются раз в BOLT_REBUILD_MS, нити — strand.ts.
// args: color, count, width, height

const SEGMENTS = 5;

export const createArcs: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color = tintOf(args.color, '#9be7ff');
  const count = Number(args.count ?? 3);
  const rx = Number(args.width ?? 50) / 2;
  const ry = Number(args.height ?? 80) / 2;
  const root = new Container();
  const arcs = Array.from({ length: count }, () =>
    ([[9, color, 0.2], [3, color, 1], [1, 0xffffff, 1]] as const).map(([width, tint, alpha]) => new Strand(SEGMENTS + 1, width, tint, alpha)));
  root.addChild(...arcs.flat().map(s => s.rope));
  let rebuildAt = 0;

  return {
    root,
    update(e) {
      const now = performance.now();
      if (now >= rebuildAt) {
        for (const arc of arcs) {
          const a = Math.random() * Math.PI * 2;
          const b = a + rand(0.6, 1.6) * (Math.random() < 0.5 ? -1 : 1);
          const path = jaggedPath(e.x + Math.cos(a) * rx, e.y + Math.sin(a) * ry, e.x + Math.cos(b) * rx, e.y + Math.sin(b) * ry, SEGMENTS, 7);
          arc.forEach(s => s.set(path));
        }
        rebuildAt = now + BOLT_REBUILD_MS;
      }
      const flicker = Math.random() > 0.15 ? 1 : 0.3;
      root.alpha = (e.opacity ?? 1) * (1 - progressOf(e)) * flicker;
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
