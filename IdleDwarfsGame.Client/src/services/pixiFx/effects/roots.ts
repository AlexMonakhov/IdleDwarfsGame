import { Container, Graphics } from 'pixi.js';
import { ellipseSprite, placeEllipse, radialTexture } from '../art';
import { ParticleSim, clamp01, easeOut, fromFrames, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Цепкие корни (как RootsStrategy): из земли под целью вырастают лозы, спиралью обвивают её и увядают.
// Участки лозы «за спиной» юнита бледнее — видно, что она обвивает тело.
// args: count, height, width, thick, color, light (прожилка), leaf

const LEAF_AT = [0.35, 0.6, 0.85];
const THORN_AT = [0.2, 0.48, 0.72];
const SEGMENTS = 24;

export const createRoots: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const height = Number(args.height ?? 120);
  const width = Number(args.width ?? 60);
  const color = tintOf(args.color, '#3f6212');
  const light = tintOf(args.light, '#84cc16');
  const leaf = tintOf(args.leaf, '#4ade80');
  const vines = Array.from({ length: Number(args.count ?? 5) }, (_, i) => ({
    side: i % 2 ? 1 : -1,
    amp: width * rand(0.35, 0.55),
    height: height * rand(0.7, 1),
    turns: rand(1.2, 1.8),
    phase: rand(0, Math.PI),
    thick: rand(5, 8) * Number(args.thick ?? 1),
    delay: i * 0.04,
  }));

  const root = new Container();
  const soil = ellipseSprite(radialTexture([[0, 0.7], [1, 0]]), tintOf('#3f2a14'), 'normal');
  const g = new Graphics();
  // Комья земли, выбитые лозами
  const dirt = new ParticleSim({
    texture: 'disc', blend: 'normal',
    drag: fromFrames.factor(0.97), gravity: fromFrames.gravity(0.22),
    alpha: life => clamp01(life * 1.5),
  });
  for (let i = 0; i < 16; i++) {
    dirt.spawn({
      x: emitter.x + rand(-width * 0.5, width * 0.5), y: emitter.y + rand(-4, 4),
      vx: fromFrames.speed(rand(-1.5, 1.5)), vy: fromFrames.speed(-rand(2, 4.5)),
      lifetime: fromFrames.lifetime(rand(0.025, 0.04)),
      size: rand(1, 2), tint: Math.random() < 0.5 ? tintOf('#5b3a1e') : tintOf('#3f2a14'),
    });
  }
  root.addChild(soil, g, dirt.container);
  let settled = false;

  const draw = (x: number, y: number, p: number, wither: number) => {
    g.clear();
    for (const vine of vines) {
      const grow = easeOut(clamp01((p - vine.delay) / 0.35));
      if (grow <= 0) continue;
      const angle = (s: number) => s * Math.PI * vine.turns + vine.phase;
      const at = (s: number) => ({ x: x + vine.side * vine.amp * Math.sin(angle(s)) * (1 - 0.35 * s), y: y - s * vine.height });

      for (let k = 0; k < SEGMENTS * grow; k++) {
        const s0 = k / SEGMENTS;
        const s1 = Math.min((k + 1) / SEGMENTS, grow);
        const a = at(s0);
        const b = at(s1);
        const behind = Math.cos(angle(s0)) * vine.side < 0;
        const alpha = behind ? 0.45 : 1;
        const w = vine.thick * (1 - s0 * 0.8);
        g.moveTo(a.x, a.y).lineTo(b.x, b.y).stroke({ width: w, color, alpha, cap: 'round' });
        g.moveTo(a.x - 1, a.y).lineTo(b.x - 1, b.y).stroke({ width: Math.max(w * 0.3, 0.8), color: light, alpha, cap: 'round' });
      }
      for (const s of THORN_AT) {
        if (s > grow) continue;
        const point = at(s);
        const out = vine.side * (Math.cos(angle(s)) >= 0 ? 1 : -1);
        const size = vine.thick * (1 - s * 0.6);
        g.poly([point.x + out * size * 1.6, point.y - size * 0.6, point.x, point.y - size * 0.5, point.x, point.y + size * 0.5]).fill(color);
      }
      for (const s of LEAF_AT) {
        if (s > grow) continue;
        const point = at(s);
        const rotation = vine.side * (0.7 + s * 0.4);
        const cos = Math.cos(rotation);
        const sin = Math.sin(rotation);
        // Лист — эллипс, повёрнутый вдоль лозы: точки эллипса считаем сами (Graphics не поворачивает фигуру)
        const rx = 6 * (1 - wither * 0.5);
        const pts: number[] = [];
        for (let i = 0; i < 12; i++) {
          const a = (i / 12) * Math.PI * 2;
          const lx = vine.side * 5 + Math.cos(a) * rx;
          const ly = Math.sin(a) * 2.6;
          pts.push(point.x + lx * cos - ly * sin, point.y + lx * sin + ly * cos);
        }
        g.poly(pts).fill(leaf);
      }
    }
  };

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      const p = progressOf(e);
      const wither = clamp01((p - 0.75) / 0.25);
      placeEllipse(soil, e.x, e.y, width * 0.9, 12);
      soil.alpha = opacity * (1 - wither);
      // Лозы перестраиваем, пока растут и пока увядают листья; в середине жизни картинка стоит
      const growing = vines.some(v => p - v.delay < 0.35);
      if (growing || wither > 0 || !settled) {
        draw(e.x, e.y, p, wither);
        settled = !growing && wither === 0;
      }
      g.alpha = opacity * (1 - wither);
      dirt.step(dt);
      dirt.container.alpha = opacity;
    },
    destroy() {
      dirt.destroy();
      root.destroy({ children: true });
    },
  };
};
