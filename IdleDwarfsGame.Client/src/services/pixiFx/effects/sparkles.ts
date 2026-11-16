import { ParticleSim, clamp01, fromFrames, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import type { FxTextureName } from '../textures';

// Поднимающиеся искры и плюсики (как SparklesStrategy): лечение, реген, бафы.
// args: color, width, height, rate (шанс искры за кадр при 60 Гц), burst (сколько сразу), shape: 'plus' | 'dot' | 'star',
//       rise, once (искры рождаются только первые 40% жизни эмиттера)

const SHAPES: Record<string, FxTextureName> = { plus: 'plus', dot: 'dot', star: 'star' };

export const createSparkles: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const width = Number(args.width ?? 60);
  const height = Number(args.height ?? 80);
  const rise = Number(args.rise ?? 1);
  const rate = Math.min(Math.max(Number(args.rate ?? 0.5), 0), 1);
  const tint = tintOf(args.color, '#4ade80');
  const once = args.once === true;

  const sim = new ParticleSim({
    texture: SHAPES[String(args.shape ?? 'plus')] ?? 'plus',
    drag: fromFrames.factor(0.99),
    alpha: life => Math.sin(Math.PI * clamp01(life)),
  });

  // Искра рождается вокруг текущей точки эмиттера (он может следовать за юнитом)
  const spawn = (x: number, y: number) => sim.spawn({
    x: x + rand(-width / 2, width / 2),
    y: y + rand(-height / 2, height / 2),
    vx: fromFrames.speed(rand(-0.2, 0.2)),
    vy: fromFrames.speed(-rand(0.5, 1.3) * rise),
    lifetime: fromFrames.lifetime(rand(0.014, 0.028)),
    size: rand(3, 6),
    tint,
  });

  for (let i = 0; i < Number(args.burst ?? 0); i++) spawn(emitter.x, emitter.y);

  return {
    root: sim.container,
    update(e, dt) {
      sim.container.alpha = e.opacity ?? 1;
      if (!once || progressOf(e) < 0.4) {
        // Шанс «за кадр при 60 Гц» → за прошедшее время
        if (Math.random() < 1 - Math.pow(1 - rate, dt * 60)) spawn(e.x, e.y);
      }
      sim.step(dt);
    },
    destroy() {
      sim.destroy();
    },
  };
};
