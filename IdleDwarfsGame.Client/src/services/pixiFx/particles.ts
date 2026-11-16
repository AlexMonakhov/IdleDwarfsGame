import { Container, type Sprite, type Texture } from 'pixi.js';
import { giveSprite, takeSprite } from './spritePool';
import type { BLEND_MODES } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE, type FxTextureName } from './textures';

// Частицы Pixi-эффектов. Единицы — как у Unity ParticleSystem (чтобы пресеты потом переносились):
// время — секунды, скорость — px/с, гравитация — px/с² (вниз — плюс), сопротивление и усадка — множитель за секунду.
// Старые canvas-эффекты задавали всё «за кадр при 60 Гц» — переводит fromFrames

export const fromFrames = {
  speed: (perFrame: number) => perFrame * 60,
  spin: (perFrame: number) => perFrame * 60,
  gravity: (perFrame: number) => perFrame * 3600,
  // Множитель за кадр (drag 0.92, shrink 0.985) → за секунду
  factor: (perFrame: number) => Math.pow(perFrame, 60),
  // maxLife — доля жизни, что уходит за кадр → секунды жизни
  lifetime: (maxLife: number) => 1 / (maxLife * 60),
};

export const rand = (min: number, max: number) => min + Math.random() * (max - min);
export const clamp01 = (value: number) => Math.min(Math.max(value, 0), 1);
export const easeOut = (t: number) => 1 - (1 - t) * (1 - t);

// Своя текстура частицы (не из общего набора textures.ts): figure — радиус фигуры в пикселях текстуры
export interface CustomParticleTexture {
  texture: Texture;
  figure: number;
}

export interface ParticleBehavior {
  texture: FxTextureName | CustomParticleTexture;
  blend?: BLEND_MODES;
  // Доля скорости, что остаётся за секунду (1 — без сопротивления)
  drag?: number;
  gravity?: number;
  // Множитель размера за секунду (1 — не меняется)
  shrink?: number;
  // Частица меньше этого радиуса исчезает
  minSize?: number;
  // Прозрачность по оставшейся жизни (1 → 0)
  alpha?: (life: number) => number;
  // Цвет по оставшейся жизни; без него — цвет, с которым частица родилась
  tint?: (life: number) => number;
  // Во сколько раз спрайт больше «тела» частицы (ореол огонька)
  halo?: number;
  // Растяжение по осям (языки пламени — капля, вытянутая вверх)
  stretch?: [number, number];
}

export interface ParticleInit {
  x: number;
  y: number;
  vx: number;
  vy: number;
  lifetime: number;
  // Радиус фигуры, px
  size: number;
  tint?: number;
  rotation?: number;
  // рад/с
  spin?: number;
}

interface LiveParticle {
  sprite: Sprite;
  vx: number;
  vy: number;
  life: number;
  lifeRate: number;
  size: number;
  spin: number;
}

// Набор частиц одного вида (одна текстура → один батч на видеокарте)
export class ParticleSim {
  readonly container = new Container();
  private readonly particles: LiveParticle[] = [];
  private readonly figure: number;
  private readonly texture: Texture;

  constructor(private readonly behavior: ParticleBehavior) {
    this.container.blendMode = behavior.blend ?? 'add';
    const custom = typeof behavior.texture === 'object' ? behavior.texture : null;
    this.texture = custom ? custom.texture : fxTexture(behavior.texture as FxTextureName);
    this.figure = custom ? custom.figure : TEXTURE_FIGURE[behavior.texture as FxTextureName];
  }

  get count() {
    return this.particles.length;
  }

  spawn(init: ParticleInit) {
    const sprite = takeSprite(this.texture);
    sprite.position.set(init.x, init.y);
    sprite.rotation = init.rotation ?? 0;
    sprite.tint = init.tint ?? 0xffffff;
    this.container.addChild(sprite);
    const particle: LiveParticle = {
      sprite,
      vx: init.vx,
      vy: init.vy,
      life: 1,
      lifeRate: 1 / Math.max(init.lifetime, 0.001),
      size: init.size,
      spin: init.spin ?? 0,
    };
    this.particles.push(particle);
    this.apply(particle);
  }

  step(dt: number) {
    const b = this.behavior;
    const drag = Math.pow(b.drag ?? 1, dt);
    const shrink = Math.pow(b.shrink ?? 1, dt);
    const gravity = (b.gravity ?? 0) * dt;
    const minSize = b.minSize ?? 0.3;

    for (let i = this.particles.length - 1; i >= 0; i--) {
      const p = this.particles[i];
      const s = p.sprite;
      s.x += p.vx * dt;
      s.y += p.vy * dt;
      p.vx *= drag;
      p.vy = p.vy * drag + gravity;
      p.size *= shrink;
      p.life -= p.lifeRate * dt;
      if (p.spin) s.rotation += p.spin * dt;

      if (p.life <= 0 || p.size < minSize) {
        giveSprite(s);
        this.particles.splice(i, 1);
        continue;
      }
      this.apply(p);
    }
  }

  private apply(p: LiveParticle) {
    const b = this.behavior;
    const scale = (p.size * (b.halo ?? 1)) / this.figure;
    if (b.stretch) p.sprite.scale.set(scale * b.stretch[0], scale * b.stretch[1]);
    else p.sprite.scale.set(scale);
    p.sprite.alpha = b.alpha ? b.alpha(p.life) : p.life;
    if (b.tint) p.sprite.tint = b.tint(p.life);
  }

  // Спрайты живых частиц — обратно в пул, сам контейнер удаляется
  destroy() {
    for (const p of this.particles) giveSprite(p.sprite);
    this.particles.length = 0;
    this.container.destroy();
  }
}

// Цвет по оставшейся жизни: свежая частица — первый цвет палитры, угасающая — последний (как paletteColor у canvas)
// Сколько частиц родить за кадр при «rate за кадр при 60 Гц» (дробная часть — с шансом), как perFrameCount у прежних canvas-эффектов
export const countFor = (ratePerFrame: number, dt: number) => {
  const scaled = ratePerFrame * dt * 60;
  return Math.floor(scaled) + (Math.random() < scaled % 1 ? 1 : 0);
};

export const paletteTint = (palette: number[]) => (life: number) =>
  palette[Math.min(palette.length - 1, Math.floor((1 - clamp01(life)) * palette.length))];
