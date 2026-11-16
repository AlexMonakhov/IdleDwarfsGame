import { Container, Sprite } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { clamp01, easeOut } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Вспышка света на юните (как FlareStrategy): пятно цвета эффекта, белое ядро и отсвет на земле.
// args: color, radius, intensity, ground (на сколько ниже точки лежит земля; 0 — без отсвета)

const glow = (tint: number) => {
  const sprite = new Sprite(fxTexture('glow'));
  sprite.anchor.set(0.5);
  sprite.tint = tint;
  sprite.blendMode = 'add';
  return sprite;
};

export const createFlare: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color = tintOf(args.color, '#ffd166');
  const radius = Number(args.radius ?? 110);
  const intensity = Number(args.intensity ?? 1);
  const ground = Number(args.ground ?? 0);

  const root = new Container();
  const light = glow(color);
  const core = glow(0xffffff);
  const floor = glow(color);
  root.addChild(light, core);
  if (ground > 0) root.addChild(floor);

  return {
    root,
    update(e) {
      const p = progressOf(e);
      // Резко вспыхивает и плавно гаснет
      const fade = p < 0.12 ? p / 0.12 : Math.pow(1 - (p - 0.12) / 0.88, 1.6);
      const alpha = (e.opacity ?? 1) * intensity * fade;
      root.visible = alpha > 0;
      const r = radius * (0.55 + 0.45 * easeOut(clamp01(p / 0.3)));

      light.position.set(e.x, e.y);
      light.alpha = alpha;
      light.scale.set(r / TEXTURE_FIGURE.glow);

      core.position.set(e.x, e.y);
      core.alpha = alpha * 0.85;
      core.scale.set((r * 0.35) / TEXTURE_FIGURE.glow);

      if (ground > 0) {
        const spread = r * 1.5;
        floor.position.set(e.x, e.y + ground);
        floor.alpha = alpha * 0.8;
        floor.scale.set(spread / TEXTURE_FIGURE.glow, (spread * 0.28) / TEXTURE_FIGURE.glow);
      }
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
