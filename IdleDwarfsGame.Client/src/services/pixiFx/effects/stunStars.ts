import { Container, Graphics, Sprite } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Звёздочки, кружащие над головой оглушённого (как StunStarsStrategy): эллипс-орбита и звёзды,
// дальние — меньше и тусклее, ближние рисуются поверх. args: count, radiusX, radiusY, color, size, speed

export const createStunStars: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const count = Number(args.count ?? 3);
  const rx = Number(args.radiusX ?? 28);
  const ry = Number(args.radiusY ?? 9);
  const size = Number(args.size ?? 7);
  const speed = Number(args.speed ?? 0.004);
  const color = tintOf(args.color, '#ffe066');

  const root = new Container();
  const orbit = new Graphics().ellipse(0, 0, rx, ry).stroke({ width: 1, color, alpha: 0.35 });
  const starsLayer = new Container();
  starsLayer.sortableChildren = true;
  const stars = Array.from({ length: count }, () => {
    const star = new Sprite(fxTexture('star5'));
    star.anchor.set(0.5);
    star.tint = color;
    starsLayer.addChild(star);
    return star;
  });
  root.addChild(orbit, starsLayer);

  return {
    root,
    update(e) {
      root.alpha = e.opacity ?? 1;
      orbit.position.set(e.x, e.y);
      const time = performance.now() * speed;
      stars.forEach((star, i) => {
        const a = time + (i * Math.PI * 2) / count;
        const depth = (Math.sin(a) + 1) / 2;
        star.position.set(e.x + Math.cos(a) * rx, e.y + Math.sin(a) * ry);
        star.rotation = a * 1.5;
        star.scale.set((size * (0.65 + depth * 0.55)) / TEXTURE_FIGURE.star5);
        star.alpha = 0.55 + depth * 0.45;
        star.zIndex = depth;
      });
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
