import { EffectType, EmitterConfig, Point } from '@/services/particleService/types/emitter';
import { frameScale, tickFrameClock } from '@/services/particleService/frameClock';
import type { PixiScene } from '@/services/pixiFx/PixiScene';

// Эффекты слоя (CanvasLayer): жизнь эмиттеров — появление, полёт от start к end, следование за юнитом (follow),
// затухание — и один цикл кадров на слой. Рисует их Pixi-сцена (services/pixiFx): эффект по типу эмиттера
// (pixiFx/effects), там же анимация юнитов. Координаты эмиттеров — CSS-пиксели слоя без масштаба интерфейса,
// от его левого верхнего угла (toLocal переводит в них экранные точки)
export class GlobalParticleService {
  private animationId: number | null = null;
  private emitters: Map<string, EmitterConfig> = new Map();
  private scene: PixiScene | null = null;
  // Элемент слоя: от его прямоугольника считаются координаты
  private space: HTMLElement | null = null;

  // Замер по типам эффектов (включает нагрузочный тест арены): сколько эмиттеров живо и сколько мс JS за кадр
  // уходит на их обновление (сглажено). Растр на видеокарте сюда не попадает — для него типы можно скрыть (hidden)
  // и смотреть, как меняется FPS
  profiling = false;
  readonly profile = new Map<EffectType, { emitters: number; ms: number }>();
  // Весь кадр сервиса вместе с анимацией юнитов и отрисовкой сцены, мс (сглажено)
  frameMs = 0;
  // Эти типы обновляются, но не рисуются
  readonly hidden = new Set<EffectType>();

  init(space: HTMLElement) {
    this.space = space;
    this.startLoop();
  }

  attachScene(scene: PixiScene | null) {
    this.scene?.clear();
    this.scene = scene;
  }

  get pixiScene() {
    return this.scene;
  }

  // Экранная точка → координаты слоя. Слой масштабирован вместе с интерфейсом (.ui-stage, UseUiScale.ts)
  toLocal(clientX: number, clientY: number): Point {
    const rect = this.space?.getBoundingClientRect();
    if (!rect || !this.space) return { x: clientX, y: clientY };
    const k = this.scale();
    return { x: (clientX - rect.left) * k, y: (clientY - rect.top) * k };
  }

  // Во сколько раз размер на экране меньше размера в координатах слоя
  scale(): number {
    const width = this.space?.getBoundingClientRect().width;
    return this.space && width ? this.space.offsetWidth / width : 1;
  }

  hasEmitter(id: string) {
    return this.emitters.has(id);
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

  // С fadeDurationMs эмиттер гаснет столько миллисекунд и только потом удаляется; без — удаляется сразу
  removeEmitter(id: string, fadeDurationMs?: number) {
    const emitter = this.emitters.get(id);
    if (!emitter) return;

    if (fadeDurationMs && fadeDurationMs > 0) {
      emitter.fadeStart = performance.now();
      emitter.fadeDuration = fadeDurationMs;
      emitter.fadeDirection = 'out';
      emitter.opacity = emitter.opacity ?? 1;
    } else {
      this.emitters.delete(id);
      this.scene?.dispose(id);
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

    if (emitter.travelStartedAt === undefined || emitter.travelDuration === undefined) return;

    const t = Math.min((now - emitter.travelStartedAt) / emitter.travelDuration, 1);
    if (emitter.startX !== undefined && emitter.endX !== undefined) emitter.x = emitter.startX + (emitter.endX - emitter.startX) * t;
    if (emitter.startY !== undefined && emitter.endY !== undefined) emitter.y = emitter.startY + (emitter.endY - emitter.startY) * t;
  }

  // Появление и затухание; true — эмиттер догас и его пора удалить
  private updateFade(emitter: EmitterConfig, now: number): boolean {
    if (emitter.fadeStart === undefined || emitter.fadeDuration === undefined) return false;
    const progress = Math.min((now - emitter.fadeStart) / emitter.fadeDuration, 1);
    emitter.opacity = emitter.fadeDirection === 'in' ? progress : 1 - progress;
    if (progress < 1) return false;
    if (emitter.fadeDirection === 'out') return true;
    emitter.opacity = 1;
    emitter.fadeStart = undefined;
    emitter.fadeDuration = undefined;
    emitter.fadeDirection = undefined;
    return false;
  }

  private startLoop() {
    const render = (frameTime: number = performance.now()) => {
      tickFrameClock(frameTime);
      const scene = this.scene;
      if (!scene) {
        this.animationId = requestAnimationFrame(render);
        return;
      }

      const frameStarted = this.profiling ? performance.now() : 0;
      const dt = frameScale / 60;
      const now = performance.now();
      const frame = this.profiling ? new Map<EffectType, { emitters: number; ms: number }>() : null;
      const toRemove: string[] = [];

      this.emitters.forEach(emitter => {
        this.updateEmitterPosition(emitter, now);
        if (this.updateFade(emitter, now)) {
          toRemove.push(emitter.id);
          return;
        }
        const started = frame ? performance.now() : 0;
        if (this.hidden.has(emitter.type)) scene.dispose(emitter.id);
        else scene.update(emitter, dt);
        if (frame) {
          const stat = frame.get(emitter.type) ?? { emitters: 0, ms: 0 };
          stat.emitters++;
          stat.ms += performance.now() - started;
          frame.set(emitter.type, stat);
        }
      });

      for (const id of toRemove) {
        this.emitters.delete(id);
        scene.dispose(id);
      }

      scene.tick(dt);
      scene.render();

      if (frame) {
        this.recordProfile(frame);
        this.frameMs = this.frameMs * 0.9 + (performance.now() - frameStarted) * 0.1;
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
    this.scene?.clear();
    this.scene = null;
    this.emitters.clear();
  }
}
