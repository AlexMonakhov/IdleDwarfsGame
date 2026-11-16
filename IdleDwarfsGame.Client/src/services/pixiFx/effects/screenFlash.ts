import { Sprite, Texture } from 'pixi.js';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Вспышка на весь экран (как ScreenFlashStrategy): два коротких всполоха, свет сверху (к низу слабее).
// args: color, alpha

let gradient: Texture | null = null;
// Вертикальный градиент: сверху полная яркость, внизу 15%
const gradientTexture = () => {
  if (gradient) return gradient;
  const canvas = document.createElement('canvas');
  canvas.width = 4;
  canvas.height = 128;
  const ctx = canvas.getContext('2d');
  if (ctx) {
    const g = ctx.createLinearGradient(0, 0, 0, 128);
    g.addColorStop(0, 'rgba(255,255,255,1)');
    g.addColorStop(1, 'rgba(255,255,255,0.15)');
    ctx.fillStyle = g;
    ctx.fillRect(0, 0, 4, 128);
  }
  gradient = Texture.from(canvas);
  return gradient;
};

export const createScreenFlash: PixiEffectFactory = (emitter, env): PixiEffect => {
  const sprite = new Sprite(gradientTexture());
  sprite.tint = tintOf(emitter.args?.color, '#fde68a');
  sprite.blendMode = 'add';
  const strength = Number(emitter.args?.alpha ?? 0.35);

  return {
    root: sprite,
    update(e) {
      const p = progressOf(e);
      const bump = (center: number, spread: number) => Math.exp(-(((p - center) / spread) ** 2));
      const k = Math.max(bump(0.08, 0.07), bump(0.32, 0.06) * 0.6);
      sprite.visible = k >= 0.01;
      sprite.alpha = (e.opacity ?? 1) * k * strength;
      sprite.width = env.width;
      sprite.height = env.height;
    },
    destroy() {
      sprite.destroy();
    },
  };
};
