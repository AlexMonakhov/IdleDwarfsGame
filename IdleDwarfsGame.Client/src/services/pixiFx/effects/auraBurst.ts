import { Container } from 'pixi.js';
import { ellipseSprite, placeEllipse, radialTexture } from '../art';
import { clamp01, easeOut, fromFrames, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { Mote, type AuraMote } from './mote';
import { Rim } from './auraSphere';

// Всплеск ауры в момент срабатывания (как AuraBurstStrategy): вспышка внутри сферы, две расходящиеся сферы-волны
// и разлёт частиц. args: color, glow, accent, colors[] (палитра частиц), mote, rx, ry, reach (радиус волны), count

interface Spark {
  mote: Mote;
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  rate: number;
  size: number;
  rotation: number;
  spin: number;
}

export const createAuraBurst: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const colorCss: string = args.color ?? '#ffd166';
  const color = tintOf(colorCss);
  const accentCss: string = args.accent ?? '#ffffff';
  const accent = tintOf(accentCss);
  const glow = tintOf(args.glow ?? colorCss);
  const rx = Number(args.rx ?? 60);
  const ry = Number(args.ry ?? 80);
  const reach = Number(args.reach ?? 240);
  const mote: AuraMote = args.mote ?? 'dot';
  const palette = ((args.colors as string[] | undefined) ?? [accentCss, colorCss]).map(c => tintOf(c));

  const root = new Container();
  root.blendMode = 'add';
  // Вспышка: белое ядро и свечение цвета ауры
  const flashGlow = ellipseSprite(radialTexture([[0, 0.35], [0.5, 0.35], [1, 0]]), glow);
  const flashCore = ellipseSprite(radialTexture([[0, 0.55], [0.5, 0], [1, 0]]), accent);
  const waves = [new Rim(color, accent), new Rim(color, accent)];
  root.addChild(flashGlow, flashCore, ...waves.map(w => w.root));

  const sparks: Spark[] = Array.from({ length: Number(args.count ?? 26) }, () => {
    const angle = Math.random() * Math.PI * 2;
    const v = fromFrames.speed(rand(2.5, 6));
    const m = new Mote(mote, palette[Math.floor(Math.random() * palette.length)], accent);
    root.addChild(m.root);
    return {
      mote: m,
      x: emitter.x + Math.cos(angle) * rx * 0.6,
      y: emitter.y + Math.sin(angle) * ry * 0.6,
      vx: Math.cos(angle) * v,
      vy: Math.sin(angle) * v * 0.75,
      life: 1,
      rate: 1 / fromFrames.lifetime(rand(0.02, 0.035)),
      size: rand(2, 4.5),
      rotation: angle,
      spin: fromFrames.spin(rand(-0.1, 0.1)),
    };
  });

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      root.alpha = opacity;
      const p = progressOf(e);

      const flash = p < 0.35 ? 1 - p / 0.35 : 0;
      flashGlow.alpha = flashCore.alpha = flash;
      placeEllipse(flashGlow, e.x, e.y, rx * 1.1, ry * 1.1);
      placeEllipse(flashCore, e.x, e.y, rx * 1.1, ry * 1.1);

      ([[0, 1], [0.14, 0.6]] as const).forEach(([delay, strength], i) => {
        const local = clamp01((p - delay) / (1 - delay));
        if (local <= 0 || local >= 1) {
          waves[i].set(e.x, e.y, rx, ry, 0);
          return;
        }
        const grow = easeOut(local);
        const wx = rx + (reach - rx) * grow;
        const wy = wx * (ry / rx) * (1 - grow * 0.25);
        waves[i].set(e.x, e.y, wx, wy, Math.pow(1 - local, 1.3) * 0.7 * strength);
      });

      const drag = Math.pow(fromFrames.factor(0.94), dt);
      const shrink = Math.pow(fromFrames.factor(0.99), dt);
      for (let i = sparks.length - 1; i >= 0; i--) {
        const s = sparks[i];
        s.x += s.vx * dt;
        s.y += s.vy * dt;
        s.vx *= drag;
        s.vy *= drag;
        s.size *= shrink;
        s.rotation += s.spin * dt;
        s.life -= s.rate * dt;
        if (s.life <= 0 || s.size < 0.3) {
          s.mote.root.destroy({ children: true });
          sparks.splice(i, 1);
          continue;
        }
        s.mote.set(s.x, s.y, s.size, clamp01(s.life * 1.4), s.rotation);
      }
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
