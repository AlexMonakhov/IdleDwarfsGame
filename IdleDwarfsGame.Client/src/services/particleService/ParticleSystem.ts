import { EffectType, EffectStrategy, EmitterConfig } from '@/services/particleService/types/interfaces/canvas-interfaces';
import { ParticlesStrategy, GodRaysStrategy, GodRaysRadianceStrategy, LightningStrategy, ImpactStrategy } from '@/services/particleService/types/canvas-effects';
import {
  ProjectileStrategy,
  ExplosionStrategy,
  SlashStrategy,
  FlamesStrategy,
  FrostAuraStrategy,
  StunStarsStrategy,
  SparklesStrategy,
  RingStrategy,
  AuraSphereStrategy,
  AuraBurstStrategy,
  FlareStrategy,
} from '@/services/particleService/types/combat-effects';
import {
  FireWallStrategy,
  StormStrategy,
  ScreenFlashStrategy,
  IcePrisonStrategy,
  RootsStrategy,
  DrainStrategy,
  SigilStrategy,
  NovaStrategy,
  LightPillarStrategy,
  ArcsStrategy,
  CometStrategy,
} from '@/services/particleService/types/spell-effects';

export class GlobalParticleService {
  private bgCanvas: HTMLCanvasElement | null = null;
  private fgCanvas: HTMLCanvasElement | null = null;
  private bgCtx: CanvasRenderingContext2D | null = null;
  private fgCtx: CanvasRenderingContext2D | null = null;
  private animationId: number | null = null;

  private emitters: Map<string, EmitterConfig> = new Map();

  // Замер по типам эффектов (включает нагрузочный тест арены): сколько эмиттеров живо и сколько мс за кадр
  // уходит на их update + draw (сглажено). Время только JS: растр на видеокарте сюда не попадает —
  // для него типы можно скрыть (hidden) и смотреть, как меняется FPS
  profiling = false;
  readonly profile = new Map<EffectType, { emitters: number; ms: number }>();
  // Весь кадр сервиса вместе с очисткой канвасов, мс (сглажено)
  frameMs = 0;
  // Эти типы обновляются, но не рисуются
  readonly hidden = new Set<EffectType>();

  private strategies: Record<EffectType, EffectStrategy> = {
    'particles': new ParticlesStrategy(),
    'god-rays': new GodRaysStrategy(),
    'god-rays-radiance': new GodRaysRadianceStrategy(),
    'lightning': new LightningStrategy(),
    'impact': new ImpactStrategy(),
    'projectile': new ProjectileStrategy(),
    'explosion': new ExplosionStrategy(),
    'slash': new SlashStrategy(),
    'flames': new FlamesStrategy(),
    'frost-aura': new FrostAuraStrategy(),
    'stun-stars': new StunStarsStrategy(),
    'sparkles': new SparklesStrategy(),
    'ring': new RingStrategy(),
    'aura-sphere': new AuraSphereStrategy(),
    'aura-burst': new AuraBurstStrategy(),
    'fire-wall': new FireWallStrategy(),
    'storm': new StormStrategy(),
    'screen-flash': new ScreenFlashStrategy(),
    'ice-prison': new IcePrisonStrategy(),
    'roots': new RootsStrategy(),
    'drain': new DrainStrategy(),
    'sigil': new SigilStrategy(),
    'nova': new NovaStrategy(),
    'light-pillar': new LightPillarStrategy(),
    'arcs': new ArcsStrategy(),
    'comet': new CometStrategy(),
    'flare': new FlareStrategy(),
  };

  // Подменить/добавить стратегию отрисовки (например, для нового эффекта статуса)
  registerStrategy(type: EffectType, strategy: EffectStrategy) {
    this.strategies[type] = strategy;
  }

  hasEmitter(id: string) {
    return this.emitters.has(id);
  }

  getCanvas(layer: 'background' | 'foreground'): HTMLCanvasElement | null {
    return layer === 'foreground' ? this.fgCanvas : this.bgCanvas;
  }

  /**
   * Инициализация сервиса с двумя канвасами
   */
  init(bgCanvas: HTMLCanvasElement, fgCanvas: HTMLCanvasElement) {
    this.bgCanvas = bgCanvas;
    this.fgCanvas = fgCanvas;
    this.bgCtx = bgCanvas.getContext('2d');
    this.fgCtx = fgCanvas.getContext('2d');

    if (!this.bgCtx || !this.fgCtx) {
      console.error('Canvas contexts could not be initialized');
      return;
    }

    this.startLoop();
  }

  addEmitter(config: EmitterConfig) {
    const emitter = this.emitters.get(config.id);

    const nextConfig: EmitterConfig = {
      ...config,
      startX: config.startX ?? config.x,
      startY: config.startY ?? config.y,
      endX: config.endX ?? config.x,
      endY: config.endY ?? config.y,
      travelStartedAt: config.travelStartedAt ?? performance.now(),
      travelDuration: config.travelDuration ?? 400,
    };

    if (emitter && emitter.fadeDirection === 'out' && emitter.fadeDuration) {
      const resumed: EmitterConfig = {
        ...emitter,
        ...nextConfig,
        opacity: 0,
        fadeStart: performance.now(),
        fadeDuration: 200,
        fadeDirection: 'in',
      };
      this.emitters.set(config.id, resumed);
      return;
    }

    const finalEmitter: EmitterConfig = config.instant
      ? { ...nextConfig, opacity: 1 }
      : {
        ...nextConfig,
        opacity: config.opacity ?? 0,
        fadeStart: performance.now(),
        fadeDuration: 200,
        fadeDirection: 'in',
      };

    this.emitters.set(config.id, finalEmitter);
  }

  // Обновить поля живого эмиттера без перезапуска (например, координаты после ресайза окна)
  updateEmitter(id: string, patch: Partial<EmitterConfig>) {
    const emitter = this.emitters.get(id);
    if (emitter) Object.assign(emitter, patch);
  }

  /**
   * Remove emitter. If fadeDurationMs is provided, the emitter will be faded out over that many milliseconds
   * before being removed. If omitted, emitter is removed immediately.
   */
  removeEmitter(id: string, fadeDurationMs?: number) {
    const emitter = this.emitters.get(id);
    if (!emitter) return;

    if (fadeDurationMs && fadeDurationMs > 0) {
      emitter.fadeStart = performance.now();
      emitter.fadeDuration = fadeDurationMs;
      emitter.fadeDirection = 'out';
      emitter.opacity = emitter.opacity ?? 1;
      this.emitters.set(id, emitter);
    } else {
      this.emitters.delete(id);
      this.strategies[emitter.type]?.disposeEmitter?.(id);
    }
  }

  private updateEmitterPosition(emitter: EmitterConfig, now: number) {
    if (emitter.follow) {
      const point = emitter.follow();
      if (point) {
        emitter.x = point.x;
        emitter.y = point.y;
      }
      return;
    }

    if (emitter.travelStartedAt === undefined || emitter.travelDuration === undefined) {
      return;
    }

    const elapsed = now - emitter.travelStartedAt;
    const t = Math.min(elapsed / emitter.travelDuration, 1);

    if (emitter.startX !== undefined && emitter.endX !== undefined) {
      emitter.x = emitter.startX + (emitter.endX - emitter.startX) * t;
    }

    if (emitter.startY !== undefined && emitter.endY !== undefined) {
      emitter.y = emitter.startY + (emitter.endY - emitter.startY) * t;
    }

    if (t >= 1) {
      emitter.x = emitter.endX ?? emitter.x;
      emitter.y = emitter.endY ?? emitter.y;
    }
  }

  private startLoop() {
    const render = () => {
      if (!this.bgCtx || !this.fgCtx || !this.bgCanvas || !this.fgCanvas) return;

      const frameStarted = this.profiling ? performance.now() : 0;
      this.bgCtx.clearRect(0, 0, this.bgCanvas.width, this.bgCanvas.height);
      this.fgCtx.clearRect(0, 0, this.fgCanvas.width, this.fgCanvas.height);

      const now = performance.now();
      const toRemove: string[] = [];
      const frame = this.profiling ? new Map<EffectType, { emitters: number; ms: number }>() : null;
      this.emitters.forEach((emitter) => {
        this.updateEmitterPosition(emitter, now);

        if (emitter.fadeStart !== undefined && emitter.fadeDuration !== undefined) {
          const progress = Math.min((now - emitter.fadeStart) / emitter.fadeDuration, 1);
          if (emitter.fadeDirection === 'in') {
            emitter.opacity = progress;
          } else {
            emitter.opacity = 1 - progress;
          }

          if (progress >= 1) {
            if (emitter.fadeDirection === 'in') {
              emitter.opacity = 1;
              emitter.fadeStart = undefined;
              emitter.fadeDuration = undefined;
              emitter.fadeDirection = undefined;
            } else {
              emitter.opacity = 0;
              toRemove.push(emitter.id);
              return;
            }
          }
        }

        const strategy = this.strategies[emitter.type];
        if (strategy) {
          const started = frame ? performance.now() : 0;
          strategy.update(emitter);

          if (!this.hidden.has(emitter.type)) {
            const targetCtx = emitter.layer === 'foreground' ? this.fgCtx! : this.bgCtx!;
            strategy.draw(targetCtx, emitter);
          }

          if (frame) {
            const stat = frame.get(emitter.type) ?? { emitters: 0, ms: 0 };
            stat.emitters++;
            stat.ms += performance.now() - started;
            frame.set(emitter.type, stat);
          }
        }
      });

      if (frame) {
        this.recordProfile(frame);
        this.frameMs = this.frameMs * 0.9 + (performance.now() - frameStarted) * 0.1;
      }

      for (const id of toRemove) {
        const emitter = this.emitters.get(id);
        if (!emitter) continue;
        this.strategies[emitter.type]?.disposeEmitter?.(id);
        this.emitters.delete(id);
      }

      this.animationId = requestAnimationFrame(render);
    };

    render();
  }

  // Время сглаживается, чтобы цифры не прыгали каждый кадр; тип без эмиттеров плавно уходит в ноль
  private recordProfile(frame: Map<EffectType, { emitters: number; ms: number }>) {
    for (const type of new Set([...this.profile.keys(), ...frame.keys()])) {
      const current = frame.get(type) ?? { emitters: 0, ms: 0 };
      const previous = this.profile.get(type);
      this.profile.set(type, {
        emitters: current.emitters,
        ms: previous ? previous.ms * 0.9 + current.ms * 0.1 : current.ms,
      });
    }
  }

  destroy() {
    if (this.animationId) cancelAnimationFrame(this.animationId);
    this.emitters.clear();
  }
}
