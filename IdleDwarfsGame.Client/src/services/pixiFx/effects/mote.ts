import { Container, Sprite } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { FLAKE_RADIUS, GLOW_RADIUS, flakeTexture } from '../art';

// Частица ауры (как drawMote у canvas): мягкий ореол цвета ауры + фигура по стихии цветом акцента
export type AuraMote = 'ember' | 'flake' | 'plus' | 'spark' | 'dot';

export class Mote {
  readonly root = new Container();
  private readonly halo: Sprite;
  private readonly shape: Sprite;

  constructor(private readonly kind: AuraMote, color: number, accent: number) {
    this.halo = new Sprite(fxTexture('glow'));
    this.halo.anchor.set(0.5);
    this.halo.tint = color;
    const texture = kind === 'flake' ? flakeTexture()
      : kind === 'plus' ? fxTexture('plus')
        : kind === 'spark' ? fxTexture('star')
          : fxTexture('disc');
    this.shape = new Sprite(texture);
    this.shape.anchor.set(0.5);
    this.shape.tint = accent;
    this.root.addChild(this.halo, this.shape);
  }

  set(x: number, y: number, size: number, alpha: number, rotation = 0) {
    this.root.position.set(x, y);
    this.root.alpha = alpha;
    this.halo.scale.set((size * 3.2) / GLOW_RADIUS);
    this.shape.rotation = this.kind === 'flake' || this.kind === 'spark' ? rotation : 0;
    switch (this.kind) {
      case 'ember':
        this.shape.scale.set((size * 0.55) / TEXTURE_FIGURE.disc, size / TEXTURE_FIGURE.disc);
        break;
      case 'flake':
        this.shape.scale.set((size * 1.4) / FLAKE_RADIUS);
        break;
      case 'plus':
        this.shape.scale.set(size / TEXTURE_FIGURE.plus);
        break;
      case 'spark':
        this.shape.scale.set((size * 1.3) / TEXTURE_FIGURE.star);
        break;
      default:
        this.shape.scale.set((size * 0.6) / TEXTURE_FIGURE.disc);
    }
  }
}
