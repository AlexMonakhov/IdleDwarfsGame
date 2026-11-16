import { Container, Sprite } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE, type FxTextureName } from '../textures';
import { ParticleSim, countFor, fromFrames, paletteTint, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Снаряд (как ProjectileStrategy): летит от start к end по дуге с ускорением, светящееся ядро и шлейф.
// Позицию снаряда пишет в emitter.x/y — по ним за ним следуют другие эффекты.
// args: core, glow, trail[], size, arc (высота дуги), shape: 'orb' | 'shard', trailRate, rise

const FIRE_PALETTE = ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'];

const sprite = (name: FxTextureName, tint: number) => {
  const s = new Sprite(fxTexture(name));
  s.anchor.set(0.5);
  s.tint = tint;
  return s;
};

export const createProjectile: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const sx = emitter.startX ?? emitter.x;
  const sy = emitter.startY ?? emitter.y;
  const ex = emitter.endX ?? emitter.x;
  const ey = emitter.endY ?? emitter.y;
  const size = Number(args.size ?? 12);
  const arc = Number(args.arc ?? 0);
  const rate = Number(args.trailRate ?? 3);
  const glow = tintOf(args.glow, '#ff7a1a');
  const core = tintOf(args.core, '#fff3b0');
  const shard = args.shape === 'shard';

  // Траектория с дугой и небольшим ускорением к цели
  const at = (raw: number) => {
    const t = raw * (0.6 + 0.4 * raw);
    return { x: sx + (ex - sx) * t, y: sy + (ey - sy) * t - Math.sin(Math.PI * t) * arc };
  };

  const root = new Container();
  root.blendMode = 'add';
  const trail = new ParticleSim({
    texture: 'disc',
    drag: fromFrames.factor(0.95), gravity: fromFrames.gravity(-Number(args.rise ?? 0.05)), shrink: fromFrames.factor(0.96),
    alpha: life => life * 0.85,
    tint: paletteTint(((args.trail as string[] | undefined) ?? FIRE_PALETTE).map(c => tintOf(c))),
  });

  // Тело снаряда: ореол, само тело (шар или осколок) и раскалённое белое ядро
  const head = new Container();
  const halo = sprite('glow', glow);
  const body = shard ? sprite('lance', glow) : sprite('glow', glow);
  const inner = shard ? null : sprite('glow', core);
  const hot = sprite('glow', 0xffffff);
  head.addChild(halo, body, ...(inner ? [inner] : []), hot);
  root.addChild(trail.container, head);

  let angle = 0;
  return {
    root,
    update(e, dt) {
      root.alpha = e.opacity ?? 1;
      const raw = progressOf(e);
      const { x, y } = at(raw);
      const ahead = at(Math.min(raw + 0.02, 1));
      if (ahead.x !== x || ahead.y !== y) angle = Math.atan2(ahead.y - y, ahead.x - x);
      e.x = x;
      e.y = y;

      if (raw < 1) {
        const back = angle + Math.PI;
        for (let i = countFor(rate, dt); i > 0; i--) {
          const spread = rand(-0.5, 0.5);
          const speed = fromFrames.speed(rand(0.6, 2));
          trail.spawn({
            x: x + rand(-size * 0.4, size * 0.4), y: y + rand(-size * 0.4, size * 0.4),
            vx: Math.cos(back + spread) * speed, vy: Math.sin(back + spread) * speed,
            lifetime: fromFrames.lifetime(rand(0.035, 0.07)),
            size: rand(size * 0.35, size * 0.8),
          });
        }
      }
      trail.step(dt);

      head.visible = raw < 1;
      if (!head.visible) return;
      const r = size * (1 + Math.sin(performance.now() / 35) * 0.12);
      head.position.set(x, y);
      halo.scale.set((r * 4.5) / TEXTURE_FIGURE.glow);
      if (shard) {
        body.rotation = angle;
        body.scale.set(r / TEXTURE_FIGURE.lance);
      } else {
        body.scale.set((r * 1.6) / TEXTURE_FIGURE.glow);
        inner?.scale.set((r * 0.9) / TEXTURE_FIGURE.glow);
      }
      hot.alpha = 0.8;
      hot.scale.set((r * 1.3) / TEXTURE_FIGURE.glow);
    },
    destroy() {
      trail.destroy();
      root.destroy({ children: true });
    },
  };
};
