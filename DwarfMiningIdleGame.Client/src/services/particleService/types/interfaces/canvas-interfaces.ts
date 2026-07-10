export type EffectType = 'particles' | 'god-rays' | 'god-rays-radiance';

export interface EmitterConfig {
  id: string;
  x: number;
  y: number;
  type: EffectType;
  padding?: number;
  layer?: 'background' | 'foreground'; // Новое свойство
}

export interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  maxLife: number;
  size: number;
  color: string;
}

export interface EffectStrategy {
  update(emitter: EmitterConfig): void;
  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig): void;
}