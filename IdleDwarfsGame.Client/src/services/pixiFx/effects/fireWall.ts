import { Container, Graphics } from 'pixi.js';
import { GLOW_RADIUS, glowSprite, placeEllipse } from '../art';
import { ParticleSim, clamp01, countFor, fromFrames, paletteTint, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { endpoints } from '../geometry';

// Огненный вал (как FireWallStrategy): стена пламени катится по земле от start к end. Языки — вытянутые мягкие огни,
// над стеной дым и угли, у основания зарево, позади вала полоса земли ещё догорает.
// args: colors[], height, width (толщина вала), rate, travel (доля жизни на проход, дальше догорает)

const FIRE = ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'];
// Язык пламени висит над точкой рождения: центр мягкого огня на 0.2 его высоты (2.8 × size) выше
const FLAME_LIFT = 0.56;

export const createFireWall: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const height = Number(args.height ?? 110);
  const width = Number(args.width ?? 46);
  const rate = Number(args.rate ?? 9);
  const travel = Number(args.travel ?? 0.75);
  const palette = ((args.colors as string[] | undefined) ?? FIRE).map(c => tintOf(c));
  const scale = height / 110;

  const front = () => {
    const { sx, sy, ex, ey } = endpoints(emitter);
    const t = clamp01(progressOf(emitter) / travel);
    return { x: sx + (ex - sx) * t, y: sy + (ey - sy) * t, t, sx, dir: Math.sign(ex - sx) || 1 };
  };

  const root = new Container();
  const smoke = new ParticleSim({
    texture: 'disc', blend: 'normal',
    drag: fromFrames.factor(0.98), gravity: fromFrames.gravity(-0.005), shrink: fromFrames.factor(1.02),
    alpha: life => life * 0.28,
  });
  // Опалённая полоса земли: от кастера к фронту разгорается
  const scorch = new Graphics();
  const tall = glowSprite(tintOf('#ffb347'));
  const base = glowSprite(tintOf('#ff7a1a'));
  // Языки пламени: мягкий огонь, вытянутый вверх (спрайт смещён вверх от точки рождения)
  const flames = new ParticleSim({
    texture: 'glow',
    drag: fromFrames.factor(0.985), gravity: fromFrames.gravity(-0.03), shrink: fromFrames.factor(0.975),
    stretch: [1.5, 2.8],
    alpha: life => clamp01(life * 1.4) * 0.75,
    tint: paletteTint(palette),
  });
  const embers = new ParticleSim({
    texture: 'glow',
    drag: fromFrames.factor(0.99), gravity: fromFrames.gravity(0.02), shrink: fromFrames.factor(0.99),
    halo: 3,
    alpha: life => clamp01(life * 1.5),
    tint: life => paletteTint(palette)(0.5 + life * 0.4),
  });
  const additive = new Container();
  additive.blendMode = 'add';
  additive.addChild(scorch, tall, base, flames.container, embers.container);
  root.addChild(smoke.container, additive);

  return {
    root,
    update(e, dt) {
      root.alpha = e.opacity ?? 1;
      const f = front();
      const p = progressOf(e);
      const fade = p > travel ? 1 - (p - travel) / (1 - travel) : 1;

      if (f.t < 1) {
        for (let i = countFor(rate, dt); i > 0; i--) {
          // У самого фронта языки гуще и выше, позади — догорают пониже
          const back = Math.random() ** 2 * width * 2.2;
          const tallness = 1 - (back / (width * 2.2)) * 0.6;
          const size = rand(10, 17) * tallness * Math.sqrt(scale);
          flames.spawn({
            x: f.x - f.dir * back + rand(-width * 0.3, width * 0.3), y: f.y + rand(-6, 6) - size * FLAME_LIFT,
            vx: fromFrames.speed(f.dir * rand(0.4, 1.4)), vy: fromFrames.speed(-rand(1.4, 3.2) * scale * tallness),
            lifetime: fromFrames.lifetime(rand(0.022, 0.04)),
            size,
          });
        }
        if (Math.random() < 1 - Math.pow(0.5, dt * 60)) {
          embers.spawn({
            x: f.x + rand(-width * 0.5, width * 0.5), y: f.y - rand(0, height * 0.4),
            vx: fromFrames.speed(f.dir * rand(0.3, 1.5) + rand(-0.5, 0.5)), vy: fromFrames.speed(-rand(2.5, 5) * scale),
            lifetime: fromFrames.lifetime(rand(0.012, 0.022)),
            size: rand(2, 4),
          });
        }
        if (Math.random() < 1 - Math.pow(0.65, dt * 60)) {
          smoke.spawn({
            x: f.x - f.dir * rand(0, width), y: f.y - height * rand(0.6, 0.9),
            vx: fromFrames.speed(rand(-0.3, 0.3)), vy: fromFrames.speed(-rand(0.6, 1.4)),
            lifetime: fromFrames.lifetime(rand(0.01, 0.018)),
            size: rand(14, 24) * Math.sqrt(scale),
            tint: tintOf('rgb(40, 26, 20)'),
          });
        }
      }
      // Пройденная полоса догорает низкими язычками, пока эффект не начал гаснуть
      if (Math.abs(f.x - f.sx) > 1 && p < 0.9) {
        for (let i = 0; i < Math.ceil(rate * 0.25 * dt * 60); i++) {
          const size = rand(6, 10);
          flames.spawn({
            x: f.sx + (f.x - f.sx) * Math.random(), y: f.y + rand(-4, 4) - size * FLAME_LIFT,
            vx: fromFrames.speed(rand(-0.3, 0.3)), vy: fromFrames.speed(-rand(0.8, 1.8) * scale),
            lifetime: fromFrames.lifetime(rand(0.04, 0.07)),
            size,
          });
        }
      }
      flames.step(dt);
      embers.step(dt);
      smoke.step(dt);

      scorch.clear();
      if (Math.abs(f.x - f.sx) > 1) {
        scorch.ellipse((f.sx + f.x) / 2, f.y, Math.abs(f.x - f.sx) / 2, 18).fill({ color: tintOf('#ff7a1a'), alpha: 0.25 * fade });
      }
      tall.visible = base.visible = f.t < 1;
      if (f.t < 1) {
        placeEllipse(tall, f.x, f.y - height * 0.4, width * 1.8, height * 0.75, GLOW_RADIUS);
        tall.alpha = fade * 0.5;
        placeEllipse(base, f.x, f.y, width * 3, width * 0.8, GLOW_RADIUS);
        base.alpha = fade * 0.9;
      }
    },
    destroy() {
      flames.destroy();
      embers.destroy();
      smoke.destroy();
      root.destroy({ children: true });
    },
  };
};
