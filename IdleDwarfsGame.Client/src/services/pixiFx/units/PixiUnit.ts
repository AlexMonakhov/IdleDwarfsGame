import { BlurFilter, ColorMatrixFilter, Container, Rectangle, Sprite, Texture, TextureSource } from 'pixi.js';
import type { ColorMatrix } from 'pixi.js';
import {
  getSpriteAtlas,
  getSpriteShift,
  getSpriteVariants,
  getSpriteZoom,
  type SpriteAnimation,
  type SpriteAtlas,
  type SpriteColor,
  type SpritePose,
} from '@/battle/sprites';
import { acquireSheet, releaseSheet, sheetHeight, sheetWidth, type SheetImage } from '@/battle/spriteCache';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { ellipseSprite, placeEllipse, radialTexture } from '../art';
import { tintOf } from '../effect';
import type { PixiScene } from '../PixiScene';
import { pixiFxSettings } from '../settings';
import { IDENTITY, brightness, multiply, overlay, saturate, solid } from './colorMatrix';

// Юнит арены в Pixi-сцене (вместо character-skeleton с отдельным канвасом и своим циклом кадров у каждого).
// Рисует тело (кадры атласа), тень и кольцо под ногами, свечение силуэта. Окраска статусов и вспышки ударов —
// копии кадра поверх тела с аддитивным смешиванием (без фильтров: фильтр — отдельный проход в текстуру на каждого
// юнита, а вспышки бывают у пол-отряда сразу). Фильтр цвета — только для серости мёртвого и вида оглушения/заморозки.
// Тряска, рывок и «вдох» ауры — трансформы.
// Где стоит юнит, задаёт его DOM-рамка (sprite-box в CharacterComponent): setBox — её прямоугольник в координатах сцены.
// Интерфейс над юнитом (имя, полоски, статусы, цифры) остаётся в DOM поверх сцены

export interface PixiUnitOptions {
  color: SpriteColor;
  level: number;
  unitType: string;
  animations: SpriteAnimation[];
  frameWidth: number;
  frameHeight: number;
  frameCount: number;
  fps: number;
  // Масштаб кадра (с ростом босса)
  scale: number;
  // Смотрит влево (правая команда)
  mirrored: boolean;
  // Во сколько раз юнит крупнее обычного по типу (getUnitSize): по нему растёт тень
  unitSize: number;
}

// Постоянная окраска силуэта (статусы, цель): цвет, сила и пульсация (раз в секунду)
export interface UnitTint {
  color: string;
  alpha: number;
  pulse?: number;
}

export type UnitFocus = 'attacker' | 'caster' | 'target' | null;

// Где ноги — в долях высоты рамки
const FEET_Y = 0.8;

interface Sheet {
  url: string;
  atlas: SpriteAtlas;
  zoom: number;
  // Копия атласа под размер на экране и кадры в ней; null — ещё готовится
  image: SheetImage | null;
  frames: (Texture | null)[];
  width: number;
  loading: Promise<void>;
}

// Текстура копии листа общая для всех юнитов: ключ — сама копия (её держит spriteCache)
const sources = new Map<SheetImage, { source: TextureSource; refs: number }>();

const acquireSource = (image: SheetImage) => {
  let entry = sources.get(image);
  if (!entry) {
    // Мимо глобального кеша Pixi (skipCache): он держит текстуру по самой копии листа и чистится только при уничтожении
    // текстуры, не её источника. Копию хранит spriteCache, и следующий юнит с тем же листом получал из кеша уже
    // уничтоженный источник (style = null) — рендер падал на «reading 'addressModeU'»
    entry = { source: Texture.from(image as HTMLCanvasElement, true).source, refs: 0 };
    entry.source.autoGenerateMipmaps = true;
    sources.set(image, entry);
  }
  entry.refs++;
  return entry.source;
};

const releaseSource = (image: SheetImage) => {
  const entry = sources.get(image);
  if (!entry) return;
  entry.refs--;
  if (entry.refs <= 0) {
    entry.source.destroy();
    sources.delete(image);
  }
};

// Короткие анимации тела по времени: смещение по x (px, в сторону взгляда) и масштаб
type Motion = { kind: 'lunge' | 'shake' | 'crit' | 'swell'; start: number; duration: number };

const easeOut = (t: number) => 1 - (1 - t) * (1 - t);

// Ключевые кадры CSS-анимаций прежнего юнита (character-component.vue): [доля, x, масштаб]
const SHAKE: [number, number, number][] = [[0, 0, 1], [0.2, -10, 1], [0.4, 8, 1], [0.6, -6, 1], [0.8, 4, 1], [1, 0, 1]];
const CRIT: [number, number, number][] = [[0, 0, 1], [0.15, -16, 0.95], [0.35, 12, 1.02], [0.55, -8, 1], [0.75, 5, 1], [1, 0, 1]];
const SWELL: [number, number, number][] = [[0, 0, 1], [0.35, 0, 1.08], [1, 0, 1]];

const keyframes = (frames: [number, number, number][], t: number): [number, number] => {
  for (let i = 1; i < frames.length; i++) {
    if (t <= frames[i][0]) {
      const [t0, x0, s0] = frames[i - 1];
      const [t1, x1, s1] = frames[i];
      const k = (t - t0) / (t1 - t0 || 1);
      return [x0 + (x1 - x0) * k, s0 + (s1 - s0) * k];
    }
  }
  return [0, 1];
};

export class PixiUnit {
  readonly root = new Container();
  private readonly shadow: Sprite;
  private readonly ring: Sprite;
  private readonly body = new Container();
  private readonly glow: Sprite;
  private readonly sprite: Sprite;
  private readonly look = new ColorMatrixFilter();
  private readonly glowColor = new ColorMatrixFilter();
  // Свечению хватает половинного разрешения и одного прохода размытия
  private readonly glowBlur = new BlurFilter({ strength: 6, quality: 1, resolution: 0.5 });

  private readonly sheets: Record<SpriteAnimation, Sheet[]> = { idle: [], walk: [], attack: [], victory: [], death: [] };
  private current: Sheet | null = null;
  private animation: SpriteAnimation = 'idle';
  private frame = 0;
  private lastFrameAt = performance.now();
  private disposed = false;

  // Где рамка юнита (sprite-box) в координатах сцены
  private box = { x: 0, y: 0, width: 0, height: 0 };

  private tint: UnitTint | null = null;
  private flashState: { color: number; alpha: number; start: number; duration: number } | null = null;
  private speed = 1;
  private paused = false;
  private pose: SpritePose | null = null;
  private focus: UnitFocus = null;
  private focusColor = 0xffd166;
  private statusClasses: string[] = [];
  private healUntil = 0;
  private motions: Motion[] = [];
  private lungeUntil = 0;

  constructor(private readonly scene: PixiScene, private readonly options: PixiUnitOptions) {
    this.shadow = ellipseSprite(radialTexture([[0, 0.6], [0.45, 0.35], [0.72, 0], [1, 0]]), 0x000000, 'normal');
    this.ring = new Sprite(fxTexture('ring'));
    this.ring.anchor.set(0.5);
    this.ring.blendMode = 'add';
    this.ring.visible = false;
    this.glow = new Sprite();
    this.glow.visible = false;
    this.glow.filters = [this.glowColor, this.glowBlur];
    // Обычное наложение, как у прежнего drop-shadow: сложением свечение просвечивало сквозь полупрозрачные части
    // кадра (росчерк оружия) и выбеливало их
    this.glowColor.resolution = 0.5;
    this.sprite = new Sprite();
    this.body.addChild(this.glow, this.sprite);
    this.body.scale.x = options.mirrored ? -1 : 1;
    this.root.addChild(this.shadow, this.ring, this.body);
    scene.units.addChild(this.root);
    this.loadSheets();
    this.playRest();
  }

  // ----- Листы -----

  private resolution() {
    return this.scene.app.renderer.resolution;
  }

  // Ширина копии атласа, у которой клетка кадра — столько пикселей экрана, сколько займёт на сцене
  private copyWidth(sheet: { atlas: SpriteAtlas; zoom: number }) {
    const cell = Math.ceil(this.options.frameWidth * this.options.scale * sheet.zoom * this.resolution());
    return Math.round((sheet.atlas.width * cell) / sheet.atlas.cell);
  }

  private loadSheets() {
    const { color, level, unitType } = this.options;
    for (const type of this.options.animations) {
      this.sheets[type] = getSpriteVariants(type, color, level, unitType).flatMap(url => {
        const atlas = getSpriteAtlas(url);
        if (!atlas) return [];
        const sheet: Sheet = { url, atlas, zoom: getSpriteZoom(url), image: null, frames: [], width: 0, loading: Promise.resolve() };
        sheet.width = this.copyWidth(sheet);
        sheet.loading = acquireSheet(atlas.url, sheet.width).then(image => {
          if (this.disposed) {
            releaseSheet(atlas.url, sheet.width);
            return;
          }
          const source = acquireSource(image);
          const sx = sheetWidth(image) / atlas.width;
          const sy = sheetHeight(image) / atlas.height;
          sheet.frames = atlas.frames.map(f => (f
            ? new Texture({ source, frame: new Rectangle(f[0] * sx, f[1] * sy, Math.max(1, f[2] * sx), Math.max(1, f[3] * sy)) })
            : null));
          sheet.image = image;
        }).catch(() => undefined);
        return [sheet];
      });
    }
  }

  // Все листы готовы, а их текстуры уже на видеокарте: арена ждёт этого перед началом боя
  ready(): Promise<void> {
    const all = (Object.keys(this.sheets) as SpriteAnimation[]).flatMap(type => this.sheets[type]);
    return Promise.all(all.map(sheet => sheet.loading)).then(() => {
      if (this.disposed) return;
      // Кадр каждого листа по разу — текстура уходит на видеокарту сейчас, а не посреди первого удара
      const warm = new Container();
      warm.alpha = 0.001;
      for (const sheet of all) {
        const texture = sheet.frames.find(Boolean);
        if (texture) warm.addChild(new Sprite(texture));
      }
      this.scene.app.renderer.render(warm);
      warm.destroy({ children: true });
    });
  }

  // ----- Анимации кадров (как character-skeleton) -----

  private setSheet(type: SpriteAnimation, variant?: number): boolean {
    const variants = this.sheets[type];
    if (!variants.length) return false;
    this.animation = type;
    this.current = variant ? variants[variant - 1] ?? variants[0] : variants[Math.floor(Math.random() * variants.length)];
    return true;
  }

  private playRest() {
    if (!(this.pose && this.setSheet(this.pose.animation, this.pose.variant))) this.setSheet('idle');
  }

  playAnimation(type: SpriteAnimation, variant?: number) {
    if (!this.setSheet(type, variant)) return;
    this.frame = 0;
    this.lastFrameAt = performance.now();
  }

  setPose(pose: SpritePose | null) {
    this.pose = pose;
    this.playAnimation(pose?.animation ?? 'idle', pose?.variant);
  }

  // Юнит умер — смерть (если нарисована), иначе замирает; ожил — снова покой
  setPaused(paused: boolean, immediate = false) {
    if (paused === this.paused) return;
    this.paused = paused;
    if (paused) {
      this.playAnimation('death');
      if (immediate && this.animation === 'death') this.frame = this.options.frameCount - 1;
    } else if (this.animation === 'death') {
      this.playAnimation('idle');
    }
  }

  setSpeed(speed: number) {
    this.speed = speed;
  }

  // ----- Внешний вид -----

  setBox(x: number, y: number, width: number, height: number) {
    this.box = { x, y, width, height };
  }

  setTint(tint: UnitTint | null) {
    this.tint = tint;
  }

  flash(color: string, alpha = 0.6, duration = 350) {
    this.flashState = { color: tintOf(color), alpha, start: performance.now(), duration };
  }

  setFocus(focus: UnitFocus, color: string) {
    this.focus = focus;
    this.focusColor = tintOf(color);
  }

  // Классы статусов (status-stunned, status-frozen) — свой вид силуэта
  setStatusClasses(classes: string[]) {
    this.statusClasses = classes;
  }

  lunge() {
    this.lungeUntil = performance.now() + 650;
  }

  shake(crit = false) {
    this.motions.push({ kind: crit ? 'crit' : 'shake', start: performance.now(), duration: crit ? 500 : 450 });
  }

  swell() {
    this.motions.push({ kind: 'swell', start: performance.now(), duration: 600 });
  }

  healGlow() {
    this.healUntil = performance.now() + 700;
  }

  // ----- Кадр -----

  private stepFrames(now: number) {
    const dying = this.animation === 'death';
    const speed = dying ? 1 : this.paused ? 0 : this.animation === 'idle' ? this.speed : 1;
    const frameDuration = 1000 / (this.options.fps || 10) / Math.max(speed, 0.0001);
    if (speed <= 0 || now - this.lastFrameAt < frameDuration) return;
    let next = this.frame + 1;
    if (next >= this.options.frameCount) {
      if (this.animation === 'attack') this.playRest();
      next = dying ? this.options.frameCount - 1 : 0;
    }
    this.frame = next;
    // Ровный шаг: следующий кадр от момента, когда должен был смениться этот; после долгой паузы не догоняем
    this.lastFrameAt = now - this.lastFrameAt > frameDuration * 3 ? now : this.lastFrameAt + frameDuration;
  }

  tick() {
    const now = performance.now();
    this.stepFrames(now);
    const { x, y, width, height } = this.box;
    const { frameWidth, frameHeight, scale, mirrored, unitSize } = this.options;
    const face = mirrored ? -1 : 1;
    const feetX = x + width / 2;
    const feetY = y + height * FEET_Y;
    // Глубина: кто ниже на экране, тот ближе и рисуется поверх
    this.root.zIndex = feetY;

    // Тень под ногами: размер растёт с юнитом (как --unit-size)
    const size = (scale / 0.65) * unitSize;
    placeEllipse(this.shadow, feetX + width * 0.03 * face, feetY, 50 * size, 12 * size);
    this.shadow.alpha = this.paused ? 0.5 : 1;

    // Кадр атласа: клетка листа по центру рамки со сдвигом юнита, кадр — там же, где лежал в клетке
    const sheet = this.current;
    const texture = sheet?.frames[this.frame] ?? null;
    this.sprite.visible = this.glow.visible = !!texture;
    if (sheet && texture) {
      const f = sheet.atlas.frames[this.frame];
      if (f) {
        const cell = frameWidth * scale * sheet.zoom;
        const cellHeight = (cell * frameHeight) / frameWidth;
        const { shiftX, shiftY } = getSpriteShift(sheet.url);
        const left = -cell / 2 + frameWidth * scale * shiftX;
        const top = y + height / 2 - cellHeight / 2 + frameWidth * scale * shiftY - feetY;
        const k = cell / sheet.atlas.cell;
        this.sprite.texture = texture;
        this.sprite.position.set(left + f[4] * k, top + f[5] * k);
        this.sprite.width = f[2] * k;
        this.sprite.height = f[3] * k;
        for (const copy of [this.glow]) {
          copy.texture = texture;
          copy.position.copyFrom(this.sprite.position);
          copy.width = this.sprite.width;
          copy.height = this.sprite.height;
        }
      }
    }

    // Тело: опора в ногах (там же, где transform-origin 50% 80% у прежнего юнита)
    let dx = 0;
    let s = 1;
    this.motions = this.motions.filter(m => now - m.start < m.duration);
    for (const m of this.motions) {
      const t = (now - m.start) / m.duration;
      const [mx, ms] = keyframes(m.kind === 'crit' ? CRIT : m.kind === 'swell' ? SWELL : SHAKE, t);
      dx += mx;
      s *= ms;
    }
    // Рывок к врагу: 0.2 с туда, держится, 0.2 с обратно (как transition у .fx-lunge)
    const lungeLeft = this.lungeUntil - now;
    if (lungeLeft > 0) dx += 18 * Math.min(easeOut(Math.min((650 - lungeLeft) / 200, 1)), easeOut(Math.min(lungeLeft / 200, 1)));
    this.body.position.set(feetX + dx * face, feetY);
    this.body.scale.set(s * face, s);

    this.applyLook(now);
    this.applyGlow(now, feetX, feetY, size);
  }

  // Вид тела — одна матрица цвета: окраска статуса и вспышка удара заливают тело цветом (как source-atop
  // у canvas-юнита: цвет заменяет, а не прибавляет свет), затем серость мёртвого и вид статусов.
  // Фильтр включён, только пока есть что менять
  private applyLook(now: number) {
    let matrix: ColorMatrix = IDENTITY;
    let active = false;
    if (this.tint) {
      const pulse = this.tint.pulse ? 0.75 + 0.25 * Math.sin((now / 1000) * Math.PI * 2 * this.tint.pulse) : 1;
      matrix = overlay(tintOf(this.tint.color), this.tint.alpha * pulse);
      active = true;
    }
    const progress = this.flashState ? (now - this.flashState.start) / this.flashState.duration : 1;
    if (this.flashState && progress >= 1) this.flashState = null;
    if (this.flashState) {
      matrix = multiply(overlay(this.flashState.color, this.flashState.alpha * (1 - progress)), matrix);
      active = true;
    }
    if (this.paused) {
      matrix = multiply(multiply(brightness(0.6), saturate(0)), matrix);
      active = true;
    } else if (this.statusClasses.includes('status-frozen')) {
      matrix = multiply(multiply(brightness(1.15), saturate(0.4)), matrix);
      active = true;
    } else if (this.statusClasses.includes('status-stunned')) {
      matrix = multiply(multiply(brightness(0.9), saturate(0.6)), matrix);
      active = true;
    }
    if (now < this.healUntil) {
      matrix = multiply(brightness(1.15), matrix);
      active = true;
    }
    if (active) {
      this.look.matrix = matrix;
      if (!this.sprite.filters) this.sprite.filters = [this.look];
    } else if (this.sprite.filters) {
      this.sprite.filters = null;
    }
  }

  // Свечение силуэта: ходящий и кастер — цветом хода, цель — красным, лечение — зелёным, заморозка — льдом.
  // Кольцо под ногами: у ходящего ровное, у цели пульсирует
  private applyGlow(now: number, feetX: number, feetY: number, size: number) {
    let color: number | null = null;
    let strength = 0;
    if (this.focus === 'target') {
      color = 0xff1414;
      strength = 1;
    } else if (this.focus === 'attacker' || this.focus === 'caster') {
      color = this.focusColor;
      strength = 0.8;
    } else if (now < this.healUntil) {
      color = 0x4ade80;
      strength = 0.9;
    } else if (this.statusClasses.includes('status-frozen') && !this.paused) {
      color = 0xbae6fd;
      strength = 0.5;
    }
    const glowing = color !== null && this.sprite.visible && pixiFxSettings.unitGlow;
    this.glow.visible = glowing;
    if (glowing && color !== null) {
      this.glowColor.matrix = solid(color);
      this.glow.alpha = strength;
    }

    const ringed = this.focus !== null;
    this.ring.visible = ringed;
    if (ringed) {
      const pulse = this.focus === 'target' ? 0.5 + 0.5 * Math.sin(now / 64) : 1;
      this.ring.tint = this.focus === 'target' ? 0xff3232 : this.focusColor;
      this.ring.position.set(feetX, feetY);
      this.ring.scale.set((52 * size) / TEXTURE_FIGURE.ring, (14 * size) / TEXTURE_FIGURE.ring);
      this.ring.alpha = 0.7 + 0.3 * pulse;
    }
  }

  destroy() {
    this.disposed = true;
    for (const type of Object.keys(this.sheets) as SpriteAnimation[]) {
      for (const sheet of this.sheets[type]) {
        sheet.frames.forEach(frame => frame?.destroy(false));
        if (sheet.image) {
          releaseSource(sheet.image);
          releaseSheet(sheet.atlas.url, sheet.width);
        }
      }
    }
    this.root.destroy({ children: true });
  }
}
