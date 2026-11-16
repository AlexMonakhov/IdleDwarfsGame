import { Container, Graphics } from 'pixi.js';
import { clamp01, easeOut } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Росчерк удара оружием (как SlashStrategy): серп, который прочерчивается за первые 40% жизни и гаснет.
// Серп перестраивается только пока прочерчивается, дальше лишь гаснет (alpha).
// args: color, direction (1 — слева направо, -1 — справа налево), radius, width, count (2 — крест для крита)

const SEGMENTS = 24;
const START = -Math.PI * 0.95;
const END = -Math.PI * 0.05;

// Серп от from до to: толщина сходит на нет к обоим концам; три слоя — ореол, цвет, белая сердцевина
const drawCrescent = (g: Graphics, radius: number, to: number, width: number, color: number) => {
  g.clear();
  const centerY = radius * 0.35;
  for (const [scale, tint, alpha] of [[2.5, color, 0.2], [1, color, 1], [0.3, 0xffffff, 1]] as const) {
    const thickness = width * scale;
    const points: number[] = [];
    for (let i = 0; i <= SEGMENTS * 2; i++) {
      const t = i <= SEGMENTS ? i / SEGMENTS : 2 - i / SEGMENTS;
      const edge = i <= SEGMENTS ? 1 : -1;
      const angle = START + (to - START) * t;
      const r = radius + edge * (thickness / 2) * Math.sin(Math.PI * t);
      points.push(Math.cos(angle) * r, centerY + Math.sin(angle) * r);
    }
    g.poly(points).fill({ color: tint, alpha });
  }
};

export const createSlash: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color = tintOf(args.color, '#ffd166');
  const direction = Number(args.direction ?? 1);
  const radius = Number(args.radius ?? 55);
  const width = Number(args.width ?? 14);
  const count = Number(args.count ?? 1);

  const root = new Container();
  root.blendMode = 'add';
  root.position.set(emitter.x, emitter.y);
  root.scale.x = direction;
  const strokes = Array.from({ length: count }, (_, n) => {
    const g = new Graphics();
    g.rotation = n === 0 ? -0.45 : 0.55;
    root.addChild(g);
    return { g, sweep: -1 };
  });

  return {
    root,
    update(e) {
      const p = progressOf(e);
      strokes.forEach((stroke, n) => {
        const local = clamp01((p - n * 0.15) / (1 - n * 0.15));
        stroke.g.visible = local > 0;
        if (local <= 0) return;
        const sweep = easeOut(Math.min(local / 0.4, 1));
        if (sweep !== stroke.sweep) {
          drawCrescent(stroke.g, radius, START + (END - START) * sweep, width, color);
          stroke.sweep = sweep;
        }
        stroke.g.alpha = (e.opacity ?? 1) * (local < 0.4 ? 1 : 1 - (local - 0.4) / 0.6);
      });
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
