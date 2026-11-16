import { Container, Sprite } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { ParticleSim, clamp01, easeOut, fromFrames, paletteTint, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Взрыв (как ExplosionStrategy): вспышка, ударная волна, разлёт искр или осколков, дым и обломки камня.
// args: colors[], count, radius, speed, ring (цвет или false), flash, smoke, smokeColor, shards, gravity, debris,
//       part: 'back' — за юнитом вспышка, волна, дым, обломки и большая часть искр; 'front' — остальные искры

const FIRE_PALETTE = ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'];
const BACK_SHARE = 0.65;

export const createExplosion: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const palette = ((args.colors as string[] | undefined) ?? FIRE_PALETTE).map(c => tintOf(c));
  const total = Number(args.count ?? 36);
  const share = args.part === 'back' ? BACK_SHARE : args.part === 'front' ? 1 - BACK_SHARE : 1;
  const behind = args.part !== 'front';
  const shards = !!args.shards;
  const speed = Number(args.speed ?? 5);
  const radius = Number(args.radius ?? emitter.padding ?? 60);
  const x = emitter.x;
  const y = emitter.y;

  const root = new Container();

  // Дым — обычным смешиванием, чтобы затемнял фон
  const smoke = new ParticleSim({
    texture: 'smoke', blend: 'normal',
    drag: fromFrames.factor(0.97), gravity: fromFrames.gravity(-0.01), shrink: fromFrames.factor(1.012),
    alpha: life => life * 0.35,
  });
  // Обломки — тёмный камень, взлетают веером и падают
  const debris = new ParticleSim({
    texture: 'debris', blend: 'normal',
    drag: fromFrames.factor(0.985), gravity: fromFrames.gravity(0.28),
    alpha: life => clamp01(life * 2),
  });
  // Искры — мягкие огоньки (или осколки), цвет по палитре от жизни
  const sparks = new ParticleSim({
    texture: shards ? 'shard' : 'glow',
    drag: fromFrames.factor(0.92), gravity: fromFrames.gravity(Number(args.gravity ?? -0.04)), shrink: fromFrames.factor(0.985),
    // Огонёк — мягкое пятно в 2.6 раза больше «тела» искры, как у canvas-спрайта свечения
    halo: shards ? 1 : 2.6,
    alpha: life => clamp01(life * 1.3),
    tint: paletteTint(palette),
  });

  // Вспышка: цвет вспышки и белое ядро; волна — кольцо
  const flash = new Sprite(fxTexture('glow'));
  flash.anchor.set(0.5);
  flash.position.set(x, y);
  flash.tint = tintOf(args.flash, (args.colors as string[] | undefined)?.[1] ?? FIRE_PALETTE[1]);
  flash.blendMode = 'add';
  const core = new Sprite(fxTexture('glow'));
  core.anchor.set(0.5);
  core.position.set(x, y);
  core.blendMode = 'add';
  const ring = new Sprite(fxTexture('ring'));
  ring.anchor.set(0.5);
  ring.position.set(x, y);
  ring.tint = tintOf(args.ring, (args.colors as string[] | undefined)?.[2] ?? FIRE_PALETTE[2]);
  ring.blendMode = 'add';
  const showRing = behind && args.ring !== false;

  root.addChild(smoke.container, debris.container);
  if (behind) root.addChild(flash, core);
  if (showRing) root.addChild(ring);
  root.addChild(sparks.container);

  const count = Math.round(total * share);
  for (let i = 0; i < count; i++) {
    const angle = Math.random() * Math.PI * 2;
    // Без совсем медленных искр: они висели бы на теле юнита
    const v = fromFrames.speed(rand(0.45, 1) * speed);
    sparks.spawn({
      x, y,
      vx: Math.cos(angle) * v, vy: Math.sin(angle) * v,
      lifetime: fromFrames.lifetime(rand(0.02, 0.045)),
      size: shards ? rand(4, 9) : rand(2.5, 7),
      rotation: angle,
      spin: shards ? fromFrames.spin(rand(-0.2, 0.2)) : 0,
    });
  }
  if (args.smoke && behind) {
    const smokeTint = tintOf(args.smokeColor, 'rgb(40, 32, 30)');
    for (let i = 0; i < Math.round(total / 3); i++) {
      smoke.spawn({
        x: x + rand(-12, 12), y: y + rand(-12, 12),
        vx: fromFrames.speed(rand(-0.6, 0.6)), vy: fromFrames.speed(rand(-1.2, -0.3)),
        lifetime: fromFrames.lifetime(rand(0.012, 0.022)),
        size: rand(10, 18),
        tint: smokeTint,
      });
    }
  }
  if (behind) {
    for (let i = 0; i < Number(args.debris ?? 0); i++) {
      const angle = -Math.PI / 2 + rand(-1.1, 1.1);
      const v = fromFrames.speed(rand(3, 7));
      debris.spawn({
        x: x + rand(-10, 10), y: y + rand(-6, 6),
        vx: Math.cos(angle) * v, vy: Math.sin(angle) * v,
        lifetime: fromFrames.lifetime(rand(0.014, 0.022)),
        size: rand(3, 6.5),
        tint: 0x2a1810,
        rotation: Math.random() * Math.PI,
        spin: fromFrames.spin(rand(-0.25, 0.25)),
      });
    }
  }

  return {
    root,
    update(e, dt) {
      root.alpha = e.opacity ?? 1;
      sparks.step(dt);
      smoke.step(dt);
      debris.step(dt);
      const p = progressOf(e);

      // Вспышка — в первой трети
      if (behind) {
        const k = p < 0.3 ? 1 - p / 0.3 : 0;
        const flashRadius = radius * (0.5 + p * 1.5);
        flash.visible = core.visible = k > 0;
        flash.alpha = k;
        flash.scale.set(flashRadius / TEXTURE_FIGURE.glow);
        core.alpha = k;
        core.scale.set((flashRadius * 0.4) / TEXTURE_FIGURE.glow);
      }
      if (showRing) {
        const ringP = easeOut(Math.min(p / 0.6, 1));
        ring.alpha = 1 - ringP;
        ring.scale.set((radius * (0.2 + ringP)) / TEXTURE_FIGURE.ring);
      }
    },
    destroy() {
      sparks.destroy();
      smoke.destroy();
      debris.destroy();
      root.destroy({ children: true });
    },
  };
};
