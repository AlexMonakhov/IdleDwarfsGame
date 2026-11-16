import { Container, Sprite } from 'pixi.js';
import { Strand } from '../strand';
import { GLOW_RADIUS, ellipseSprite, placeEllipse, radialTexture } from '../art';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { clamp01, easeOut, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { endpoints } from '../geometry';

// Похищение жизни (как DrainStrategy): от кастера (start) к цели (end) тянется извивающаяся лента из нескольких
// прядей, по ней к кастеру текут сгустки жизни. args: color, glow, core, amplitude, rate, strands

interface Orb {
  t: number;
  speed: number;
  strand: number;
  size: number;
  halo: Sprite;
  core: Sprite;
}

export const createDrain: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color = tintOf(args.color, '#e11d48');
  const glow = tintOf(args.glow, '#be185d');
  const core = tintOf(args.core, '#fecdd3');
  const strands = Number(args.strands ?? 2);
  const amplitude = Number(args.amplitude ?? 14);
  const rate = clamp01(Number(args.rate ?? 0.7));

  const at = (t: number, strand: number, time: number) => {
    const { sx, sy, ex, ey } = endpoints(emitter);
    const dx = ex - sx;
    const dy = ey - sy;
    const length = Math.hypot(dx, dy) || 1;
    const amp = amplitude * Math.sin(Math.PI * t);
    const wave = Math.sin(t * Math.PI * 3 + time / 90 + strand * Math.PI * 2) * amp;
    return { x: sx + dx * t - (dy / length) * wave, y: sy + dy * t + (dx / length) * wave };
  };

  const root = new Container();
  root.blendMode = 'add';
  const vortex = ellipseSprite(radialTexture([[0, 0.5], [1, 0]]), color);
  const absorb = ellipseSprite(radialTexture([[0, 0.35], [1, 0]]), core);
  // Пряди — нити (strand.ts) по 31 точке: лента извивается, в кадре меняются только координаты
  const ribbons = Array.from({ length: strands }, () =>
    ([[14, color, 0.18], [5, glow, 0.6], [1.8, core, 0.9]] as const).map(([width, tint, alpha]) => new Strand(31, width, tint, alpha)));
  const ribbonLayer = new Container();
  ribbonLayer.addChild(...ribbons.flat().map(s => s.rope));
  const orbsLayer = new Container();
  root.addChild(vortex, absorb, ribbonLayer, orbsLayer);
  const orbs: Orb[] = [];

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      const p = progressOf(e);
      const reach = easeOut(clamp01(p / 0.2));
      const fade = clamp01((1 - p) / 0.2);
      const time = performance.now();
      const { sx, sy, ex, ey } = endpoints(e);

      placeEllipse(vortex, ex, ey, 34, 40);
      placeEllipse(absorb, sx, sy, 26, 30);
      vortex.alpha = absorb.alpha = opacity * fade * reach;

      ribbonLayer.alpha = opacity * fade;
      ribbons.forEach((layers, s) => {
        for (let i = 0; i <= 30; i++) {
          const point = at((i / 30) * reach, s / strands, time);
          layers.forEach(strand => strand.setXY(i, point.x, point.y));
        }
        layers.forEach(strand => strand.commit());
      });

      if (p > 0.15 && p < 0.85 && Math.random() < 1 - Math.pow(1 - rate, dt * 60)) {
        const halo = new Sprite(fxTexture('glow'));
        halo.anchor.set(0.5);
        halo.tint = color;
        const coreSprite = new Sprite(fxTexture('disc'));
        coreSprite.anchor.set(0.5);
        coreSprite.tint = core;
        orbsLayer.addChild(halo, coreSprite);
        orbs.push({ t: 1, speed: rand(0.018, 0.03) * 60, strand: Math.random(), size: rand(2.5, 4.5), halo, core: coreSprite });
      }
      for (let i = orbs.length - 1; i >= 0; i--) {
        const orb = orbs[i];
        orb.t -= orb.speed * dt;
        if (orb.t <= 0) {
          orb.halo.destroy();
          orb.core.destroy();
          orbs.splice(i, 1);
          continue;
        }
        const point = at(orb.t, orb.strand, time);
        orb.halo.position.set(point.x, point.y);
        orb.halo.scale.set((orb.size * 3.5) / GLOW_RADIUS);
        orb.core.position.set(point.x, point.y);
        orb.core.scale.set((orb.size * 0.6) / TEXTURE_FIGURE.disc);
        orb.halo.alpha = orb.core.alpha = opacity * fade;
      }
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
