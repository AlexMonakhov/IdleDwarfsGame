// Эмиттер эффекта: тип (какой Pixi-эффект его рисует — services/pixiFx/effects), где он и как живёт.
// Жизнью эмиттеров управляет GlobalParticleService
export type EffectType =
  | 'particles'
  | 'god-rays'
  | 'god-rays-radiance'
  | 'lightning'
  | 'impact'
  | 'projectile'
  | 'explosion'
  | 'slash'
  | 'flames'
  | 'frost-aura'
  | 'stun-stars'
  | 'sparkles'
  | 'ring'
  | 'aura-sphere'
  | 'aura-burst'
  | 'fire-wall'
  | 'storm'
  | 'screen-flash'
  | 'ice-prison'
  | 'roots'
  | 'drain'
  | 'sigil'
  | 'nova'
  | 'light-pillar'
  | 'arcs'
  | 'comet'
  | 'flare'
  | 'ground-mark';

export type FadeDirection = 'in' | 'out';

export interface Point {
  x: number;
  y: number;
}

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
  // skip the fade-in (short-lived combat effects must be visible immediately)
  instant?: boolean;
  // привязка к движущемуся объекту: позиция пересчитывается каждый кадр (ауры статусов на юнитах)
  follow?: () => Point | null;
}
