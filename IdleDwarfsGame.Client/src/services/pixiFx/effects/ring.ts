import { Container, Graphics, Sprite } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { easeOut } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';

// Кольцо на земле (как RingStrategy): расходящаяся волна ('pulse') или вращающийся магический круг ('rune').
// args: color, radius, squash (сплющивание эллипса), width, mode

// Ореол штриха: тот же путь шире и полупрозрачнее (как strokeGlowing у canvas)
const GLOW = 10;

// Магический круг строится один раз: два круга и восемь засечек-рун, у каждого штриха — ореол
const buildRune = (radius: number, width: number, color: number) => {
  const g = new Graphics();
  const strokes: [number, (g: Graphics) => void][] = [
    [width, gr => gr.circle(0, 0, radius)],
    [width * 0.5, gr => gr.circle(0, 0, radius * 0.75)],
    [width * 0.8, gr => {
      for (let i = 0; i < 8; i++) {
        const a = (i * Math.PI) / 4;
        gr.moveTo(Math.cos(a) * radius * 0.78, Math.sin(a) * radius * 0.78);
        gr.lineTo(Math.cos(a + 0.2) * radius * 0.97, Math.sin(a + 0.2) * radius * 0.97);
      }
    }],
  ];
  for (const [w, build] of strokes) {
    build(g);
    g.stroke({ width: w + GLOW, color, alpha: 0.25, cap: 'round' });
    build(g);
    g.stroke({ width: w, color, cap: 'round' });
  }
  return g;
};

export const createRing: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const color = tintOf(args.color, '#ffd166');
  const radius = Number(args.radius ?? emitter.padding ?? 50);
  const squash = Number(args.squash ?? 0.35);
  const width = Number(args.width ?? 4);
  const rune = args.mode === 'rune';

  // Внешний контейнер сплющивает в эллипс на земле, внутренний вращается уже в сплющенном пространстве
  const root = new Container();
  root.blendMode = 'add';
  const ground = new Container();
  ground.scale.y = squash;
  root.addChild(ground);

  const fill = new Sprite(fxTexture('glow'));
  fill.anchor.set(0.5);
  fill.tint = color;
  ground.addChild(fill);

  const spin = new Container();
  ground.addChild(spin);
  let line: Graphics | Sprite;
  if (rune) {
    line = buildRune(radius, width, color);
  } else {
    const sprite = new Sprite(fxTexture('ring'));
    sprite.anchor.set(0.5);
    sprite.tint = color;
    line = sprite;
  }
  spin.addChild(line);

  return {
    root,
    update(e) {
      const p = progressOf(e);
      const opacity = e.opacity ?? 1;
      ground.position.set(e.x, e.y);

      if (rune) {
        const appear = easeOut(Math.min(p / 0.2, 1));
        const fade = p > 0.7 ? 1 - (p - 0.7) / 0.3 : 1;
        const scale = 0.6 + 0.4 * appear;
        spin.rotation = performance.now() / 600;
        spin.scale.set(scale);
        spin.alpha = opacity * fade;
        fill.alpha = opacity * fade * 0.25;
        fill.scale.set((radius * scale) / TEXTURE_FIGURE.glow);
      } else {
        const r = radius * (0.3 + 0.9 * easeOut(p));
        line.scale.set(r / TEXTURE_FIGURE.ring);
        spin.alpha = opacity * (1 - p);
        fill.alpha = opacity * (1 - p) * 0.3;
        fill.scale.set(r / TEXTURE_FIGURE.glow);
      }
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
