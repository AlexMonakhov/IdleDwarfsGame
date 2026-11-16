import { Container, Graphics, Sprite } from 'pixi.js';
import { ellipseSprite, placeEllipse, radialTexture } from '../art';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { ParticleSim, clamp01, fromFrames, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { endpoints } from '../geometry';

// Сияющая вспышка (как NovaStrategy): от кастера расходится кольцо света — светлая оболочка, лучи и волна по земле.
// Радиус растёт линейно до |endX - startX| за долю жизни travel: BattleFx по тому же закону считает, когда задело цель.
// args: colors[] (искры на фронте), color, accent, travel, groundOffset, sparks (искр за кадр), echo (вторая волна)

const PRISM = ['#ffffff', '#ff7ad9', '#a78bfa', '#6bd5ff', '#6bff95', '#ffd166'];

export const createNova: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color = tintOf(args.color, '#fde68a');
  const accent = tintOf(args.accent, '#ffffff');
  const travel = Number(args.travel ?? 0.7);
  const groundOffset = Number(args.groundOffset ?? 60);
  const sparksPerFrame = Number(args.sparks ?? 3);
  const palette = ((args.colors as string[] | undefined) ?? PRISM).map(c => tintOf(c));
  const { sx, sy, ex } = endpoints(emitter);
  const maxRadius = Math.abs(ex - sx) || 200;
  const radiusAt = (p: number) => maxRadius * clamp01(p / travel);

  const root = new Container();
  root.blendMode = 'add';
  // Вспышка на кастере
  const flashOuter = ellipseSprite(radialTexture([[0, 0.4], [0.4, 0.4], [1, 0]]), color);
  const flashCore = ellipseSprite(radialTexture([[0, 0.8], [0.4, 0], [1, 0]]), accent);
  // Двенадцать лучей из центра: геометрия один раз (единичной длины), в кадре — масштаб и поворот
  const rays = new Graphics();
  for (let i = 0; i < 12; i++) {
    const a = ((i + 1) * Math.PI) / 6;
    const cos = Math.cos(a);
    const sin = Math.sin(a);
    const tip = (dy: number) => [cos - dy * sin, sin + dy * cos];
    rays.poly([0, 0, ...tip(-0.05), ...tip(0.05)]).fill({ color: accent, alpha: 0.3 });
  }
  rays.position.set(sx, sy);
  // Оболочки: светлый край и кольцо на земле (с echo — вторая, слабее)
  const shellTexture = radialTexture([[0, 0], [0.78, 0.05], [0.94, 0.55], [1, 0]]);
  const shells = (args.echo ? [[1, 1], [0.72, 0.55]] : [[1, 1]]).map(([scale, strength]) => {
    const shell = ellipseSprite(shellTexture, color);
    const ring = new Sprite(fxTexture('ring'));
    ring.anchor.set(0.5);
    ring.tint = color;
    root.addChild(shell, ring);
    return { scale, strength, shell, ring };
  });
  const sparks = new ParticleSim({ texture: 'star', drag: fromFrames.factor(0.98), alpha: life => clamp01(life * 1.3) });
  const halos = new ParticleSim({ texture: 'glow', drag: fromFrames.factor(0.98), halo: 3, alpha: life => clamp01(life * 1.3) });
  root.addChildAt(rays, 0);
  root.addChildAt(flashOuter, 0);
  root.addChildAt(flashCore, 1);
  root.addChild(halos.container, sparks.container);
  placeEllipse(flashOuter, sx, sy, 90, 110);
  placeEllipse(flashCore, sx, sy, 90, 110);

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      root.alpha = opacity;
      const p = progressOf(e);
      const r = radiusAt(p);
      const fade = p < travel ? 1 : 1 - (p - travel) / (1 - travel);

      flashOuter.alpha = flashCore.alpha = p < 0.25 ? 1 - p / 0.25 : 0;
      rays.visible = r > 1;
      rays.scale.set(r);
      rays.rotation = performance.now() / 3000;
      rays.alpha = fade;

      for (const { scale, strength, shell, ring } of shells) {
        const radius = r * scale;
        placeEllipse(shell, sx, sy, radius, radius * 0.62);
        shell.alpha = fade * strength;
        ring.position.set(sx, sy + groundOffset);
        ring.scale.set(Math.max(radius, 1) / TEXTURE_FIGURE.ring, (Math.max(radius, 1) * 0.25) / TEXTURE_FIGURE.ring);
        ring.alpha = fade * strength;
      }

      if (p < travel) {
        for (let i = 0; i < Math.round(sparksPerFrame * dt * 60); i++) {
          const a = Math.random() * Math.PI * 2;
          const init = {
            x: sx + Math.cos(a) * r, y: sy + Math.sin(a) * r * 0.62,
            vx: fromFrames.speed(Math.cos(a) * 0.4), vy: fromFrames.speed(Math.sin(a) * 0.3 - 0.3),
            lifetime: fromFrames.lifetime(rand(0.03, 0.05)),
            size: rand(2, 4),
            rotation: Math.random() * Math.PI,
          };
          halos.spawn({ ...init, tint: palette[Math.floor(Math.random() * palette.length)] });
          sparks.spawn({ ...init, size: init.size * 1.2 });
        }
      }
      halos.step(dt);
      sparks.step(dt);
    },
    destroy() {
      halos.destroy();
      sparks.destroy();
      root.destroy({ children: true });
    },
  };
};
