import { Container, Sprite } from 'pixi.js';
import { MARK_RADIUS, groundMarkTextures, type GroundMarkKind } from '../art';
import { clamp01, easeOut, rand } from '../particles';
import { progressOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// След на земле под целью после попадания (как GroundMarkStrategy): быстро расползается, держится и тает к концу,
// раскалённая (сияющая) часть гаснет за первую половину. Поворот у каждого следа свой.
// args: kind (burn | frost | stain | glow), color, radius, squash, intensity

export const createGroundMark: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const textures = groundMarkTextures((args.kind ?? 'burn') as GroundMarkKind, args.color ?? '#ff7a1a');
  const radius = Number(args.radius ?? 60);
  const intensity = Number(args.intensity ?? 1);

  // Внешний контейнер сплющивает след по земле, внутренний поворачивает уже в сплющенном пространстве
  const root = new Container();
  root.scale.y = Number(args.squash ?? 0.32);
  const turn = new Container();
  turn.rotation = rand(0, Math.PI * 2);
  root.addChild(turn);
  const base = new Sprite(textures.base);
  base.anchor.set(0.5);
  base.blendMode = textures.additive ? 'add' : 'normal';
  const glow = new Sprite(textures.glow);
  glow.anchor.set(0.5);
  glow.blendMode = 'add';
  turn.addChild(base, glow);

  return {
    root,
    update(e) {
      const p = progressOf(e);
      const opacity = (e.opacity ?? 1) * intensity;
      const spread = 0.6 + 0.4 * easeOut(clamp01(p / 0.12));
      const linger = p < 0.65 ? 1 : 1 - (p - 0.65) / 0.35;
      const heat = Math.pow(1 - clamp01(p / 0.5), 1.5);
      root.position.set(e.x, e.y);
      turn.scale.set((radius / MARK_RADIUS) * spread);
      base.alpha = opacity * linger * (textures.additive ? 0.7 : 1);
      glow.visible = heat > 0.01;
      glow.alpha = opacity * heat;
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
