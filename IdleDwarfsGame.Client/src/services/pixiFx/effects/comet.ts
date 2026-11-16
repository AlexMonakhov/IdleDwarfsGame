import { Container, Graphics, Sprite } from 'pixi.js';
import { GLOW_RADIUS, ellipseSprite, glowSprite, placeEllipse, radialTexture } from '../art';
import { ParticleSim, clamp01, countFor, fromFrames, paletteTint, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { endpoints } from '../geometry';

// Метеор (как CometStrategy): раскалённая глыба с огненным хвостом падает с ускорением от start к end.
// За ней дымный шлейф, с хвоста сыплются угли, вокруг зарево, земля под точкой падения разгорается.
// args: colors[] (угли), core, glow, size, tail (длина хвоста, px), smoke (дымный шлейф, по умолчанию есть)

const FIRE = ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'];

// Клин хвоста от ядра назад (по оси -x): закругление спереди, острый конец сзади; цвета — полосы от ядра к концу
const buildTail = (g: Graphics, width: number, tail: number, stops: [number, number, number][]) => {
  // Хвост длиной tail нарезан на полосы по stops: [доля длины, цвет, альфа]
  for (let i = 0; i < stops.length - 1; i++) {
    const [f0, c0, a0] = stops[i];
    const [f1] = stops[i + 1];
    const w0 = width * (1 - f0 * 0.9);
    const w1 = width * (1 - f1 * 0.9);
    const x0 = -tail * f0;
    const x1 = -tail * f1;
    g.poly([x0, -w0, x1, -w1, x1, w1, x0, w0]).fill({ color: c0, alpha: a0 });
  }
  g.circle(0, 0, width).fill({ color: stops[0][1], alpha: stops[0][2] });
};

export const createComet: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const palette = ((args.colors as string[] | undefined) ?? FIRE).map(c => tintOf(c));
  const size = Number(args.size ?? 13);
  const core = tintOf(args.core, '#fffbe0');
  const glow = tintOf(args.glow, '#ff6a00');
  const tailLength = Number(args.tail ?? 150);
  const withSmoke = args.smoke !== false;
  const { sx, sy, ex, ey } = endpoints(emitter);
  const angle = Math.atan2(ey - sy, ex - sx);
  const headAt = (raw: number) => {
    const t = raw * raw * 0.6 + raw * 0.4;
    return { x: sx + (ex - sx) * t, y: sy + (ey - sy) * t };
  };

  const root = new Container();
  const smoke = new ParticleSim({
    texture: 'disc', blend: 'normal',
    drag: fromFrames.factor(0.97), gravity: fromFrames.gravity(-0.01), shrink: fromFrames.factor(1.025),
    alpha: life => life * 0.3,
  });
  const lights = new Container();
  lights.blendMode = 'add';
  // Отсвет на земле: чем ближе метеор, тем ярче и шире
  const spot = ellipseSprite(radialTexture([[0, 0.75], [0.5, 0.4], [1, 0]]), tintOf('#ff7a1a'));
  const sparks = new ParticleSim({
    texture: 'glow',
    drag: fromFrames.factor(0.95), gravity: fromFrames.gravity(-0.03), shrink: fromFrames.factor(0.96),
    halo: 2.6,
    alpha: life => clamp01(life * 1.4) * 0.9,
    tint: paletteTint(palette),
  });
  const head = new Container();
  head.rotation = angle;
  const aura = glowSprite(glow);
  const outerTail = new Graphics();
  const innerTail = new Graphics();
  const halo = glowSprite(glow);
  const ball = new Sprite(radialTexture([[0, 1], [0.45, 1], [1, 1]]));
  ball.anchor.set(0.5);
  ball.tint = glow;
  const ballCore = glowSprite(core);
  head.addChild(aura, outerTail, innerTail, halo, ball, ballCore);
  lights.addChild(spot, sparks.container, head);
  root.addChild(smoke.container, lights);
  let builtLength = -1;

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      root.alpha = opacity;
      const raw = progressOf(e);
      const { x, y } = headAt(raw);

      if (raw < 1) {
        for (let i = countFor(4, dt); i > 0; i--) {
          const back = angle + Math.PI + rand(-0.4, 0.4);
          const v = fromFrames.speed(rand(1, 4));
          sparks.spawn({
            x: x + rand(-size * 0.6, size * 0.6), y: y + rand(-size * 0.6, size * 0.6),
            vx: Math.cos(back) * v, vy: Math.sin(back) * v,
            lifetime: fromFrames.lifetime(rand(0.025, 0.05)),
            size: rand(size * 0.3, size * 0.7),
          });
        }
        if (withSmoke && Math.random() < 1 - Math.pow(0.4, dt * 60)) {
          smoke.spawn({
            x: x + rand(-size * 0.4, size * 0.4), y: y + rand(-size * 0.4, size * 0.4),
            vx: fromFrames.speed(rand(-0.4, 0.4)), vy: fromFrames.speed(rand(-0.6, -0.1)),
            lifetime: fromFrames.lifetime(rand(0.012, 0.02)),
            size: rand(size * 0.8, size * 1.3),
            tint: tintOf('rgb(45, 30, 24)'),
          });
        }
      }
      sparks.step(dt);
      smoke.step(dt);

      spot.visible = head.visible = raw < 1;
      if (raw >= 1) return;
      const spotR = 40 + 90 * raw;
      placeEllipse(spot, ex, ey, spotR, spotR * 0.3);
      spot.alpha = raw;

      head.position.set(x, y);
      const flicker = 1 + Math.sin(performance.now() / 30) * 0.08;
      placeEllipse(aura, 0, 0, size * 7, size * 7, GLOW_RADIUS);
      aura.alpha = 0.8;
      // Хвост отрастает в начале полёта: перестраиваем, пока растёт; дальше только мерцание масштабом
      const length = tailLength * Math.min(1, 0.3 + raw * 2);
      if (Math.abs(length - builtLength) > 0.5) {
        outerTail.clear();
        buildTail(outerTail, size * 1.7, length, [[0, glow, 0.9], [0.4, tintOf('#e2401b'), 0.5], [1, tintOf('#8a1f0f'), 0]]);
        innerTail.clear();
        buildTail(innerTail, size * 0.8, length * 0.6, [[0, core, 1], [0.5, tintOf('#ffb347'), 0.6], [1, tintOf('#ff7a1a'), 0]]);
        builtLength = length;
      }
      outerTail.scale.set(1, flicker);
      innerTail.scale.set(1, flicker);
      placeEllipse(halo, 0, 0, size * 3.4, size * 3.4, GLOW_RADIUS);
      placeEllipse(ball, 0, 0, size * flicker, size * flicker);
      placeEllipse(ballCore, 0, 0, size * 1.2, size * 1.2, GLOW_RADIUS);
    },
    destroy() {
      sparks.destroy();
      smoke.destroy();
      root.destroy({ children: true });
    },
  };
};
