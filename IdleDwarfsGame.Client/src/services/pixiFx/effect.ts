import { Color, type Container } from 'pixi.js';
import type { EmitterConfig } from '@/services/particleService/types/interfaces/canvas-interfaces';

// Pixi-эффект одного эмиттера. Живёт, пока жив эмиттер в GlobalParticleService (он ведёт позицию, полёт и затухание),
// каждый кадр получает update. Геометрия строится один раз — в кадре меняются только transform, alpha и tint
export interface PixiEffect {
  readonly root: Container;
  // dt — секунды с прошлого кадра
  update(emitter: EmitterConfig, dt: number): void;
  destroy(): void;
}

export type PixiEffectFactory = (emitter: EmitterConfig) => PixiEffect;

// Прогресс одноразового эффекта 0..1 — как progressOf у canvas-стратегий
export const progressOf = (emitter: EmitterConfig) => {
  const started = emitter.travelStartedAt ?? performance.now();
  return Math.min(Math.max((performance.now() - started) / Math.max(emitter.travelDuration ?? 1, 1), 0), 1);
};

const tintCache = new Map<string, number>();

// CSS-цвет ('#ffd166', 'rgba(40, 32, 30, 1)') → число для tint
export const tintOf = (css: unknown, fallback = '#ffffff'): number => {
  const key = typeof css === 'string' && css ? css : fallback;
  let tint = tintCache.get(key);
  if (tint === undefined) {
    tint = new Color(key).toNumber();
    tintCache.set(key, tint);
  }
  return tint;
};
