import { Container, type Sprite } from 'pixi.js';
import { giveSprite, takeSprite } from '../spritePool';
import { ellipseSprite, placeEllipse, radialTexture } from '../art';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { clamp01, countFor, easeOut, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { envelope } from '../geometry';

// Буран над группой целей (как StormStrategy): клубящаяся туча и косой снег под ней.
// args: spread (ширина группы), margin, lift (на сколько выше эмиттера туча), fall (высота снегопада),
//       wind, direction (1 — ветер вправо, -1 — влево), density

interface Flake {
  sprite: Sprite;
  vx: number;
  vy: number;
  life: number;
  rate: number;
}

export const createStorm: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const width = Number(args.spread ?? 0) + Number(args.margin ?? 160);
  const lift = Number(args.lift ?? 70);
  const wind = Number(args.wind ?? 2.2) * Number(args.direction ?? 1);
  const fall = Number(args.fall ?? 260);
  const density = Number(args.density ?? 1);

  const root = new Container();
  // Туча — обычным смешиванием, чтобы затемняла фон
  const cloud = new Container();
  const puffTexture = radialTexture([[0, 0.85], [0.6, 0.6], [1, 0]]);
  const count = Math.max(6, Math.round(width / 28));
  const puffs = Array.from({ length: count }, (_, i) => {
    const sprite = ellipseSprite(puffTexture, tintOf('#475569'), 'normal');
    cloud.addChild(sprite);
    return {
      sprite,
      x: emitter.x - width / 2 + (width * (i + 0.5)) / count + rand(-10, 10),
      dy: rand(-14, 10),
      vx: rand(-0.15, 0.15) * 60,
      size: rand(26, 40),
      seed: Math.random() * Math.PI * 2,
    };
  });
  const lights = new Container();
  lights.blendMode = 'add';
  // Холодный отсвет на брюхе тучи
  const belly = ellipseSprite(radialTexture([[0, 0.35], [1, 0]]), tintOf('#7dd3fc'));
  const snowLayer = new Container();
  lights.addChild(belly, snowLayer);
  root.addChild(cloud, lights);
  const snow: Flake[] = [];
  // Снежинка — короткий штрих по направлению падения: точка-диск, вытянутая вдоль скорости
  const angle = Math.atan2(rand(5, 8), wind);

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      const p = progressOf(e);
      const env = envelope(p, 0.15, 0.2);
      const swell = 0.6 + 0.4 * easeOut(clamp01(p / 0.2));
      const time = performance.now();
      const cloudY = e.y - lift;

      cloud.alpha = opacity * env * 0.9;
      for (const puff of puffs) {
        puff.x += puff.vx * dt;
        placeEllipse(puff.sprite, puff.x + Math.sin(time / 700 + puff.seed) * 4, cloudY + puff.dy, puff.size * 1.3 * swell, puff.size * 0.7 * swell);
      }
      lights.alpha = opacity * env;
      placeEllipse(belly, e.x, cloudY + 16, width * 0.5, 24);

      if (p > 0.12 && p < 0.85) {
        for (let i = countFor((width / 50) * density, dt); i > 0; i--) {
          const vy = rand(5, 8);
          const sprite = takeSprite(fxTexture('disc'));
          sprite.tint = Math.random() < 0.3 ? tintOf('#bae6fd') : 0xffffff;
          const size = rand(1.2, 2.6);
          sprite.rotation = angle;
          // Штрих длиной как у canvas (скорость × 2.5 кадра), толщиной size
          sprite.scale.set((Math.hypot(wind, vy) * 2.5) / (TEXTURE_FIGURE.disc * 2), size / (TEXTURE_FIGURE.disc * 2));
          sprite.position.set(e.x + rand(-width / 2, width / 2) - wind * 20, cloudY + rand(0, 15));
          snowLayer.addChild(sprite);
          snow.push({ sprite, vx: wind * rand(0.8, 1.2) * 60, vy: vy * 60, life: 1, rate: (vy / fall) * 60 });
        }
      }
      for (let i = snow.length - 1; i >= 0; i--) {
        const s = snow[i];
        s.sprite.x += s.vx * dt;
        s.sprite.y += s.vy * dt;
        s.life -= s.rate * dt;
        if (s.life <= 0) {
          giveSprite(s.sprite);
          snow.splice(i, 1);
          continue;
        }
        s.sprite.alpha = Math.sin(Math.PI * clamp01(s.life)) * 0.9;
      }
    },
    destroy() {
      snow.forEach(s => giveSprite(s.sprite));
      root.destroy({ children: true });
    },
  };
};
