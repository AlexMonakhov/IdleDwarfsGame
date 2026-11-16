import { Graphics } from 'pixi.js';
import { tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Божественные лучи за элементом интерфейса (как GodRaysStrategy и GodRaysRadianceStrategy).
// god-rays — 15 секторов-лучей, вращаются и пульсируют; radiance — 65 тонких лучей, дышащих каждый в своём ритме

export const createGodRays: PixiEffectFactory = (emitter): PixiEffect => {
  const radius = emitter.padding || 40;
  const g = new Graphics();
  g.blendMode = 'add';
  for (let i = 0; i < 15; i++) {
    const start = (i * Math.PI * 2) / 15;
    g.moveTo(0, 0).arc(0, 0, radius, start, start + 0.2).lineTo(0, 0).fill({ color: tintOf('rgb(21, 14, 220)'), alpha: 0.8 });
  }
  return {
    root: g,
    update(e, dt) {
      g.position.set(e.x, e.y);
      g.rotation += 0.005 * 60 * dt;
      g.alpha = (e.opacity ?? 1) * (Math.sin(Date.now() / 1000) * 0.2 + 0.8);
    },
    destroy() {
      g.destroy();
    },
  };
};

export const createGodRaysRadiance: PixiEffectFactory = (): PixiEffect => {
  const g = new Graphics();
  g.blendMode = 'add';
  const offsets = Array.from({ length: 65 }, () => Math.random() * 100);
  const color = tintOf('rgba(250, 192, 68, 1)');
  let angle = 0;
  return {
    root: g,
    update(e, dt) {
      const pulse = Math.sin(Date.now() / 1000) * 0.1 + 0.8;
      const time = Date.now() / 1000;
      angle += 0.001 * 60 * dt;
      // Длина каждого луча меняется каждый кадр — 65 треугольников перестраиваются (только на тестовой странице)
      g.clear();
      for (let i = 0; i < 65; i++) {
        const start = (i * Math.PI * 2.5) / 65;
        const end = start + 0.6;
        const mid = start + 0.3;
        const outer = (50 + Math.sin(time + offsets[i]) * 20 + 20) * pulse;
        g.poly([30 * Math.cos(start), 30 * Math.sin(start), 30 * Math.cos(end), 30 * Math.sin(end), outer * Math.cos(mid), outer * Math.sin(mid)]);
      }
      g.fill({ color, alpha: 0.9 });
      g.position.set(e.x, e.y);
      g.rotation = angle;
      g.alpha = (e.opacity ?? 1) * pulse;
    },
    destroy() {
      g.destroy();
    },
  };
};
