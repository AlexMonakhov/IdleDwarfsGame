import { AlphaFilter, Application, Container, Graphics, Rectangle, Sprite } from 'pixi.js';
import type { EffectType, EmitterConfig } from '@/services/particleService/types/emitter';
import type { FxEnv, PixiEffect } from './effect';
import { PIXI_EFFECTS } from './effects';
import { FX_TEXTURE_NAMES, fxTexture } from './textures';
import { pixiFxSettings } from './settings';

export type FxLayer = 'background' | 'foreground';

// Сколько пикселей видеопамяти на канвас: выше — резче на ретине, но дороже видеокарте и композитору
const MAX_PIXELS = 4_500_000;

// Разрешение канваса: не больше 2× и не больше бюджета пикселей
const resolutionFor = (width: number, height: number) => {
  const area = Math.max(width * height, 1);
  return Math.max(0.5, Math.min(window.devicePixelRatio || 1, 2, Math.sqrt(MAX_PIXELS / area)));
};

// Что сцена каждый кадр обновляет помимо эффектов (анимация юнитов); dt — секунды
export type SceneTicker = (dt: number) => void;

// Сцена слоя (CanvasLayer): один WebGL-канвас размером со слой и три подслоя по глубине —
// эффекты под юнитами, сами юниты (сортируются по глубине), эффекты над юнитами.
// Один контекст вместо стопки канвасов: браузеру нечего склеивать, кроме одного слоя (замеры — docs/pixi-migration.md).
// Координаты — CSS-пиксели слоя без масштаба интерфейса, начало — левый верхний угол слоя.
// Жизнью эмиттеров управляет GlobalParticleService: он зовёт update/dispose, tick и render раз за кадр
export class PixiScene {
  readonly background = new Container();
  readonly units = new Container();
  readonly foreground = new Container();

  private readonly effects = new Map<string, PixiEffect>();
  private readonly tickers = new Set<SceneTicker>();
  // Пустой кадр после пустого не рисуем
  private drawn = true;
  private env: FxEnv;
  // Эффекты над юнитами сначала копятся в отдельной текстуре (между собой — сложением), а потом ложатся на сцену
  // обычным наложением — как прежний canvas-слой поверх юнитов. Если складывать их прямо с пикселями юнитов,
  // свет выжигает тело в белое: много свечения и не видно ни юнита, ни формы эффекта
  private readonly compose = new AlphaFilter({ alpha: 1, antialias: 'off' });

  private constructor(readonly app: Application, width: number, height: number) {
    this.env = { width, height };
    this.units.sortableChildren = true;
    app.stage.addChild(this.background, this.units, this.foreground);
    this.fitCompose();
  }

  private fitCompose() {
    this.compose.resolution = this.app.renderer.resolution;
    this.foreground.filterArea = new Rectangle(0, 0, this.env.width, this.env.height);
  }

  // Модуль грузится лениво (import()): Pixi попадает только в чанки страниц со сценой
  static async create(canvas: HTMLCanvasElement, width: number, height: number) {
    const app = new Application();
    await app.init({
      canvas,
      width,
      height,
      backgroundAlpha: 0,
      // MSAA спрайтам не нужен, а на большом канвасе стоит дорого
      antialias: false,
      resolution: resolutionFor(width, height),
      autoDensity: true,
      autoStart: false,
      preference: 'webgl',
      powerPreference: 'high-performance',
    });
    app.ticker.stop();
    PixiScene.prewarm(app);
    return new PixiScene(app, width, height);
  }

  // Невидимый кадр со всеми текстурами, обоими смешиваниями и Graphics: шейдеры компилируются и текстуры уходят
  // на видеокарту при создании сцены (экран загрузки), а не на первом заклинании — там это рывок в секунду и больше
  private static prewarm(app: Application) {
    const scene = new Container();
    scene.alpha = 0.001;
    for (const blend of ['add', 'normal'] as const) {
      const group = new Container();
      group.blendMode = blend;
      for (const name of FX_TEXTURE_NAMES) group.addChild(new Sprite(fxTexture(name)));
      group.addChild(new Graphics().circle(0, 0, 10).fill(0xffffff).moveTo(0, 0).lineTo(10, 10).stroke({ width: 2, color: 0xffffff }));
      scene.addChild(group);
    }
    app.renderer.render(scene);
    scene.destroy({ children: true });
    PixiScene.prewarmEffects(app);
    app.renderer.render(app.stage);
  }

  // Каждый эффект один раз на невидимом эмиттере: его заготовки (градиенты, геометрия, шейдеры) готовятся сейчас,
  // а не на первом заклинании боя — там это рывок в сотни миллисекунд
  private static prewarmEffects(app: Application) {
    const now = performance.now();
    const env = { width: 400, height: 300 };
    const scene = new Container();
    scene.alpha = 0.001;
    const effects = (Object.keys(PIXI_EFFECTS) as EffectType[]).flatMap(type => {
      const parts = type === 'aura-sphere' || type === 'explosion' ? ['back', 'front'] : [undefined];
      return parts.map(part => {
        const emitter: EmitterConfig = {
          id: `warm-${type}-${part ?? ''}`, type, x: 100, y: 150, startX: 50, startY: 150, endX: 300, endY: 150,
          travelStartedAt: now - 250, travelDuration: 1000, opacity: 1, padding: 40, args: part ? { part } : {},
        };
        const effect = PIXI_EFFECTS[type](emitter, env);
        effect.update(emitter, 1 / 60);
        effect.update(emitter, 1 / 60);
        scene.addChild(effect.root);
        return effect;
      });
    });
    app.renderer.render(scene);
    effects.forEach(effect => effect.destroy());
    scene.destroy({ children: true });
  }

  get size(): FxEnv {
    return this.env;
  }

  supports(type: EffectType) {
    return !!PIXI_EFFECTS[type] && (!pixiFxSettings.only || pixiFxSettings.only.has(type)) && !pixiFxSettings.skip.has(type);
  }

  // Создаёт эффект при первом кадре эмиттера и обновляет его. dt — секунды
  update(emitter: EmitterConfig, dt: number) {
    if (!this.supports(emitter.type)) {
      this.dispose(emitter.id);
      return;
    }
    let effect = this.effects.get(emitter.id);
    if (!effect) {
      const factory = PIXI_EFFECTS[emitter.type];
      if (!factory) return;
      effect = factory(emitter, this.env);
      (emitter.layer === 'foreground' ? this.foreground : this.background).addChild(effect.root);
      this.effects.set(emitter.id, effect);
    }
    effect.update(emitter, dt);
  }

  dispose(id: string) {
    const effect = this.effects.get(id);
    if (!effect) return;
    effect.destroy();
    this.effects.delete(id);
  }

  clear() {
    for (const id of [...this.effects.keys()]) this.dispose(id);
  }

  addTicker(ticker: SceneTicker) {
    this.tickers.add(ticker);
  }

  removeTicker(ticker: SceneTicker) {
    this.tickers.delete(ticker);
  }

  // Анимация юнитов и всё, что подписалось на кадр
  tick(dt: number) {
    this.tickers.forEach(ticker => ticker(dt));
  }

  // Рисует кадр, если на сцене что-то есть (и один пустой кадр после — стереть последнее)
  render() {
    const busy = this.background.children.length + this.units.children.length + this.foreground.children.length > 0;
    if (!busy && !this.drawn) return;
    // Пустому слою отдельная текстура не нужна
    const composed = this.foreground.children.length > 0;
    if (composed !== !!this.foreground.filters?.length) this.foreground.filters = composed ? [this.compose] : [];
    this.app.renderer.render(this.app.stage);
    this.drawn = busy;
  }

  resize(width: number, height: number) {
    this.env = { width, height };
    this.app.renderer.resize(width, height, resolutionFor(width, height));
    this.fitCompose();
    this.drawn = true;
  }

  destroy() {
    this.clear();
    this.tickers.clear();
    // Канвас — из шаблона Vue, его не удаляем
    this.app.destroy({ removeView: false }, { children: true });
  }
}
