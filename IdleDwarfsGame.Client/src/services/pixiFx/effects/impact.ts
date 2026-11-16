import { Container, Sprite } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { ParticleSim, fromFrames, rand } from '../particles';
import { tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Удар (как ImpactStrategy): расходящееся кольцо за 400 мс и разлёт искр. args: color

export const createImpact: PixiEffectFactory = (emitter): PixiEffect => {
  const color = tintOf(emitter.args?.color, '#ffd166');
  const padding = emitter.padding ?? 40;
  const started = emitter.travelStartedAt ?? performance.now();

  const root = new Container();
  root.blendMode = 'add';
  const ring = new Sprite(fxTexture('ring'));
  ring.anchor.set(0.5);
  ring.tint = color;
  ring.position.set(emitter.x, emitter.y);
  const sparks = new ParticleSim({ texture: 'disc', drag: fromFrames.factor(0.94) });
  root.addChild(ring, sparks.container);

  for (let i = 0; i < 18; i++) {
    const angle = Math.random() * Math.PI * 2;
    const speed = fromFrames.speed(rand(2, 6));
    sparks.spawn({
      x: emitter.x, y: emitter.y,
      vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed,
      lifetime: fromFrames.lifetime(rand(0.03, 0.06)),
      size: rand(2, 5),
      tint: color,
    });
  }

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      const progress = Math.min((performance.now() - started) / 400, 1);
      ring.alpha = opacity * (1 - progress);
      ring.scale.set((padding * (0.3 + progress)) / TEXTURE_FIGURE.ring);
      sparks.container.alpha = opacity;
      sparks.step(dt);
    },
    destroy() {
      sparks.destroy();
      root.destroy({ children: true });
    },
  };
};
