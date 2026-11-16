import { Container, Sprite } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { ParticleSim, clamp01, countFor, fromFrames, paletteTint, rand } from '../particles';
import { alphaOf, progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Пламя на юните (как FlamesStrategy): аура горения или короткая вспышка огня при тике.
// args: width, height, rate, size, colors[], burst (языки рождаются только первую половину жизни), glow (цвет или false)

const FIRE_PALETTE = ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'];
const DEFAULT_GLOW = 'rgba(255, 120, 30, 0.35)';

export const createFlames: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const width = Number(args.width ?? 50);
  const height = Number(args.height ?? 70);
  const size = Number(args.size ?? 8);
  const rate = Number(args.rate ?? 2);

  const root = new Container();
  root.blendMode = 'add';

  // Тёплое мерцающее свечение вокруг
  const glowColor = args.glow === false ? null : (args.glow as string | undefined) ?? DEFAULT_GLOW;
  const glow = new Sprite(fxTexture('glow'));
  glow.anchor.set(0.5);
  glow.tint = tintOf(glowColor ?? DEFAULT_GLOW);
  glow.visible = glowColor !== null;
  const glowAlpha = alphaOf(glowColor ?? DEFAULT_GLOW);
  glow.scale.set((width * 1.3) / TEXTURE_FIGURE.glow);

  // Языки пламени — капли, вытянутые вверх
  const sim = new ParticleSim({
    texture: 'disc',
    drag: fromFrames.factor(0.99), gravity: fromFrames.gravity(-0.02), shrink: fromFrames.factor(0.975),
    stretch: [0.7, 1.25],
    alpha: life => clamp01(life * 1.4) * 0.8,
    tint: paletteTint(((args.colors as string[] | undefined) ?? FIRE_PALETTE).map(c => tintOf(c))),
  });
  root.addChild(glow, sim.container);

  return {
    root,
    update(e, dt) {
      root.alpha = e.opacity ?? 1;
      if (!args.burst || progressOf(e) < 0.5) {
        for (let i = countFor(rate, dt); i > 0; i--) {
          // Ближе к центру языки гуще
          const offset = (Math.random() + Math.random() - 1) * width * 0.5;
          sim.spawn({
            x: e.x + offset, y: e.y + rand(-height * 0.15, height * 0.25),
            vx: fromFrames.speed(rand(-0.25, 0.25) - offset * 0.004),
            vy: fromFrames.speed(-rand(0.7, 1.6) * (height / 70)),
            lifetime: fromFrames.lifetime(rand(0.025, 0.045)),
            size: rand(size * 0.6, size * 1.2),
          });
        }
      }
      sim.step(dt);
      if (glow.visible) {
        glow.position.set(e.x, e.y);
        glow.alpha = glowAlpha * (0.75 + Math.sin(performance.now() / 90) * 0.15 + Math.random() * 0.1);
      }
    },
    destroy() {
      sim.destroy();
      root.destroy({ children: true });
    },
  };
};
