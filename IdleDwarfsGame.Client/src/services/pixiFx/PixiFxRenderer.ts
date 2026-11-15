import { Application } from 'pixi.js';
import type { EffectType, EmitterConfig } from '@/services/particleService/types/interfaces/canvas-interfaces';
import type { PixiEffect } from './effect';
import { PIXI_EFFECTS } from './effects';

export type FxLayer = 'background' | 'foreground';

// Pixi-отрисовка эффектов слоя (CanvasLayer с pixi): два WebGL-канваса — под юнитами и над ними, как у canvas.
// Жизнью эмиттеров по-прежнему управляет GlobalParticleService: он зовёт update/dispose и render раз за кадр
// (свой ticker у Pixi выключен — один цикл на оба рендера). Координаты — те же пиксели слоя, что и у canvas
export class PixiFxRenderer {
  private readonly effects = new Map<string, { effect: PixiEffect; layer: FxLayer }>();
  private empty = true;

  private constructor(private readonly apps: Record<FxLayer, Application>) {}

  // Модуль грузится лениво (import()), поэтому Pixi попадает только в чанки страниц со слоем pixi
  static async create(background: HTMLCanvasElement, foreground: HTMLCanvasElement, width: number, height: number) {
    const make = async (canvas: HTMLCanvasElement) => {
      const app = new Application();
      await app.init({
        canvas,
        width,
        height,
        backgroundAlpha: 0,
        antialias: true,
        // Резче старого canvas (там 1:1 к CSS-пикселю), но не больше 2× — видеокарте этого хватает с запасом
        resolution: Math.min(window.devicePixelRatio || 1, 2),
        autoDensity: true,
        autoStart: false,
        preference: 'webgl',
      });
      app.ticker.stop();
      return app;
    };
    return new PixiFxRenderer({ background: await make(background), foreground: await make(foreground) });
  }

  supports(type: EffectType) {
    return !!PIXI_EFFECTS[type];
  }

  // Создаёт эффект при первом кадре эмиттера и обновляет его. dt — секунды
  update(emitter: EmitterConfig, dt: number) {
    let entry = this.effects.get(emitter.id);
    if (!entry) {
      const factory = PIXI_EFFECTS[emitter.type];
      if (!factory) return;
      const layer: FxLayer = emitter.layer === 'foreground' ? 'foreground' : 'background';
      entry = { effect: factory(emitter), layer };
      this.apps[layer].stage.addChild(entry.effect.root);
      this.effects.set(emitter.id, entry);
    }
    entry.effect.update(emitter, dt);
  }

  dispose(id: string) {
    const entry = this.effects.get(id);
    if (!entry) return;
    entry.effect.destroy();
    this.effects.delete(id);
  }

  clear() {
    for (const id of [...this.effects.keys()]) this.dispose(id);
  }

  // Пустой кадр после пустого не рисуем (как canvas не очищает пустые канвасы)
  render() {
    if (this.effects.size === 0 && this.empty) return;
    this.empty = this.effects.size === 0;
    this.apps.background.renderer.render(this.apps.background.stage);
    this.apps.foreground.renderer.render(this.apps.foreground.stage);
  }

  resize(width: number, height: number) {
    this.apps.background.renderer.resize(width, height);
    this.apps.foreground.renderer.resize(width, height);
    this.empty = false;
  }

  destroy() {
    this.clear();
    // Канвасы — из шаблона Vue, их не удаляем: removeView: false
    this.apps.background.destroy({ removeView: false });
    this.apps.foreground.destroy({ removeView: false });
  }
}
