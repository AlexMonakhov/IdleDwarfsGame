import { Container, type Sprite } from 'pixi.js';
import { giveSprite, takeSprite } from '../spritePool';
import { FLAKE_RADIUS, flakeTexture, placeEllipse, radialTexture, ellipseSprite } from '../art';
import { clamp01, rand } from '../particles';
import { tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Холодная аура (как FrostAuraStrategy): морозная дымка и падающие снежинки, покачивающиеся на ветру.
// args: width, height, rate (шанс снежинки за кадр при 60 Гц), color

interface Flake {
  sprite: Sprite;
  vx: number;
  vy: number;
  life: number;
  rate: number;
  size: number;
  spin: number;
}

export const createFrostAura: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const width = Number(args.width ?? 70);
  const height = Number(args.height ?? 110);
  const rate = clamp01(Number(args.rate ?? 0.35));
  const color = tintOf(args.color, '#d6f3ff');

  const root = new Container();
  root.blendMode = 'add';
  const mist = ellipseSprite(radialTexture([[0, 0.18], [1, 0]]), tintOf('#7dd3fc'));
  const flakesLayer = new Container();
  root.addChild(mist, flakesLayer);
  const flakes: Flake[] = [];

  return {
    root,
    update(e, dt) {
      root.alpha = e.opacity ?? 1;
      placeEllipse(mist, e.x, e.y, width, width);
      if (Math.random() < 1 - Math.pow(1 - rate, dt * 60)) {
        const sprite = takeSprite(flakeTexture());
        sprite.tint = color;
        sprite.position.set(e.x + rand(-width / 2, width / 2), e.y - height / 2 + rand(0, height * 0.4));
        sprite.rotation = Math.random() * Math.PI;
        flakesLayer.addChild(sprite);
        flakes.push({ sprite, vx: rand(-0.2, 0.2) * 60, vy: rand(0.25, 0.6) * 60, life: 1, rate: rand(0.008, 0.014) * 60, size: rand(2.5, 5), spin: rand(-0.03, 0.03) * 60 });
      }
      const now = performance.now();
      for (let i = flakes.length - 1; i >= 0; i--) {
        const f = flakes[i];
        f.vx += Math.sin(now / 400 + f.size) * 0.01 * 60 * dt * 60;
        f.sprite.x += f.vx * dt;
        f.sprite.y += f.vy * dt;
        f.sprite.rotation += f.spin * dt;
        f.life -= f.rate * dt;
        if (f.life <= 0) {
          giveSprite(f.sprite);
          flakes.splice(i, 1);
          continue;
        }
        f.sprite.scale.set(f.size / FLAKE_RADIUS);
        f.sprite.alpha = Math.sin(Math.PI * clamp01(f.life));
      }
    },
    destroy() {
      flakes.forEach(f => giveSprite(f.sprite));
      root.destroy({ children: true });
    },
  };
};

