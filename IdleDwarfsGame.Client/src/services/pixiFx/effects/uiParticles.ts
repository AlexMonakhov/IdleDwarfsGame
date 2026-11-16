import { ParticleSim, fromFrames, rand } from '../particles';
import { tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Частицы вокруг элемента интерфейса (как ParticlesStrategy): до 50 огоньков разлетаются во все стороны.
// С from/to — точка рождения едет от start к end. args: speed, color

export const createUiParticles: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const speed = Number(args.speed ?? 2);
  const tint = tintOf(args.color, 'rgb(232, 250, 70)');
  const travels = emitter.startX !== emitter.endX || emitter.startY !== emitter.endY;
  const sim = new ParticleSim({ texture: 'disc', alpha: () => 1 });

  return {
    root: sim.container,
    update(e, dt) {
      sim.container.alpha = e.opacity ?? 1;
      for (let i = Math.max(1, Math.round(dt * 60)); i > 0 && sim.count < 50; i--) {
        sim.spawn({
          x: e.x, y: e.y,
          vx: fromFrames.speed((Math.random() - 0.5) * speed), vy: fromFrames.speed((Math.random() - 0.5) * speed),
          lifetime: fromFrames.lifetime(rand(0.01, 0.03)),
          size: travels ? rand(2, 6) : rand(1, 4),
          tint,
        });
      }
      sim.step(dt);
    },
    destroy() {
      sim.destroy();
    },
  };
};
