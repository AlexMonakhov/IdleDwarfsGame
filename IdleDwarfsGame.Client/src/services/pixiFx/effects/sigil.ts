import { Container, Sprite } from 'pixi.js';
import { ellipseSprite, placeEllipse, radialTexture, sigilBase, sigilHexagram, sigilRunes } from '../art';
import { ParticleSim, clamp01, easeOut, fromFrames, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Порча (как SigilStrategy): над головой цели проступает печать с глазом, вращается и обрушивается на неё,
// с печати стекают капли тьмы. Слои печати со свечением нарисованы один раз (art.ts) — в кадре только поворот.
// args: color, accent, radius, lift, drop, slam (доля жизни начала падения), ornate (внешнее кольцо рун)

const layerSprite = ({ texture, half }: { texture: Sprite['texture']; half: number }) => {
  const sprite = new Sprite(texture);
  sprite.anchor.set(0.5);
  sprite.width = sprite.height = half * 2;
  return sprite;
};

export const createSigil: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color: string = args.color ?? '#a855f7';
  const accent: string = args.accent ?? '#f0abfc';
  const r = Number(args.radius ?? 30);
  const lift = Number(args.lift ?? 40);
  const drop = Number(args.drop ?? 70);
  const slam = Number(args.slam ?? 0.65);

  const root = new Container();
  const drips = new ParticleSim({
    texture: 'disc', blend: 'normal',
    gravity: fromFrames.gravity(0.04),
    stretch: [0.6, 1],
    alpha: life => clamp01(life),
  });
  const seal = new Container();
  // Тёмная подложка — печать читается на любом фоне
  const backing = ellipseSprite(radialTexture([[0, 0.6], [1, 0]]), tintOf('#1e0538'), 'normal');
  const glowing = new Container();
  glowing.blendMode = 'add';
  const base = layerSprite(sigilBase(color, accent, r));
  const hexagram = layerSprite(sigilHexagram(color, r));
  const runes = args.ornate ? layerSprite(sigilRunes(color, accent, r)) : null;
  glowing.addChild(base, hexagram, ...(runes ? [runes] : []));
  seal.addChild(backing, glowing);
  root.addChild(drips.container, seal);
  placeEllipse(backing, 0, 0, r * 1.3, r * 1.3);

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      const p = progressOf(e);
      const appear = easeOut(clamp01(p / 0.25));
      const fall = clamp01((p - slam) / 0.12);
      const x = e.x;
      const y = e.y - lift + fall * fall * (lift + drop);
      const scale = (0.4 + 0.6 * appear) * (1 - 0.4 * fall);
      const alpha = p > slam + 0.12 ? 0 : appear;
      const spin = performance.now() / 700 + (1 - appear) * 2;

      if (alpha > 0.5 && fall <= 0 && Math.random() < 1 - Math.pow(0.65, dt * 60)) {
        drips.spawn({
          x: x + rand(-r * 0.6, r * 0.6), y: y + r * 0.5,
          vx: 0, vy: fromFrames.speed(rand(0.3, 0.8)),
          lifetime: fromFrames.lifetime(rand(0.02, 0.035)),
          size: rand(1.5, 3), tint: tintOf(color),
        });
      }
      drips.step(dt);
      drips.container.alpha = opacity;

      seal.visible = alpha > 0;
      seal.position.set(x, y);
      seal.scale.set(scale);
      seal.alpha = opacity * alpha;
      hexagram.rotation = spin;
      if (runes) runes.rotation = -spin * 1.4;
    },
    destroy() {
      drips.destroy();
      root.destroy({ children: true });
    },
  };
};
