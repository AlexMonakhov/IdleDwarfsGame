import type { EffectType } from '@/services/particleService/types/interfaces/canvas-interfaces';
import type { PixiEffectFactory } from '../effect';
import { createExplosion } from './explosion';
import { createSparkles } from './sparkles';
import { createFlare } from './flare';
import { createRing } from './ring';

// Типы эффектов, уже перенесённые на Pixi. Остальные пока рисуют canvas-стратегии (docs/pixi-migration.md)
export const PIXI_EFFECTS: Partial<Record<EffectType, PixiEffectFactory>> = {
  explosion: createExplosion,
  sparkles: createSparkles,
  flare: createFlare,
  ring: createRing,
};
