import { EffectType, EffectStrategy, EmitterConfig } from './types/interfaces/canvas-interfaces';
import { ParticlesStrategy, GodRaysStrategy, GodRaysRadianceStrategy } from './types/canvas-effects';

export class GlobalParticleService {
  private bgCanvas: HTMLCanvasElement | null = null;
  private fgCanvas: HTMLCanvasElement | null = null;
  private bgCtx: CanvasRenderingContext2D | null = null;
  private fgCtx: CanvasRenderingContext2D | null = null;
  private animationId: number | null = null;

  private emitters: Map<string, EmitterConfig> = new Map();

  private strategies: Record<EffectType, EffectStrategy> = {
    'particles': new ParticlesStrategy(),
    'god-rays': new GodRaysStrategy(),
    'god-rays-radiance': new GodRaysRadianceStrategy(),
  };

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
    this.emitters.set(config.id, config);
  }

  removeEmitter(id: string) {
    this.emitters.delete(id);
  }

  private startLoop() {
    const render = () => {
      if (!this.bgCtx || !this.fgCtx || !this.bgCanvas || !this.fgCanvas) return;

      // 1. Очищаем оба холста
      this.bgCtx.clearRect(0, 0, this.bgCanvas.width, this.bgCanvas.height);
      this.fgCtx.clearRect(0, 0, this.fgCanvas.width, this.fgCanvas.height);

      // 2. Отрисовываем эмиттеры на нужных слоях
      this.emitters.forEach((emitter) => {
        const strategy = this.strategies[emitter.type];
        if (strategy) {
          strategy.update(emitter);
          
          // Определяем, в какой контекст рисовать
          const targetCtx = emitter.layer === 'foreground' ? this.fgCtx! : this.bgCtx!;
          strategy.draw(targetCtx, emitter);
        }
      });

      this.animationId = requestAnimationFrame(render);
    };

    render();
  }

  destroy() {
    if (this.animationId) cancelAnimationFrame(this.animationId);
    this.emitters.clear();
  }
}