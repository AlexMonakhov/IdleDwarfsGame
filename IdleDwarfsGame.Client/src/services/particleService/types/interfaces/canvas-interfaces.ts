export type EffectType = 'particles' | 'god-rays' | 'god-rays-radiance';

export type FadeDirection = 'in' | 'out';

export interface EmitterConfig {
  id: string;
  x: number;
  y: number;
  type: EffectType;
  padding?: number;
  layer?: 'background' | 'foreground'; // Новое свойство
  args?: Record<string, any>;
  // opacity is used to fade out emitters when they are cancelled
  opacity?: number;
  // internal fade metadata (set by service when removeEmitter is called with duration)
  fadeStart?: number;
  fadeDuration?: number;
  fadeDirection?: FadeDirection;
  // internal movement metadata for projectile-like effects
  startX?: number;
  startY?: number;
  endX?: number;
  endY?: number;
  travelStartedAt?: number;
  travelDuration?: number;
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
