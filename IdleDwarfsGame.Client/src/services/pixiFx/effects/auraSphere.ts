import { Container, Graphics, Sprite } from 'pixi.js';
import { ellipseSprite, placeEllipse, radialTexture } from '../art';
import { fxTexture, TEXTURE_FIGURE } from '../textures';
import { clamp01, easeOut, rand } from '../particles';
import { tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { Mote, type AuraMote } from './mote';

// Аура героя (как AuraSphereStrategy): полупрозрачная пульсирующая сфера вокруг юнита. Две половины, чтобы юнит
// оказался «внутри»: part 'back' (под юнитами) — круг на земле с рунами и рябью, дымка, дальние половины орбит;
// part 'front' (над юнитами) — обод, блик, волна пульсации, ближние частицы и дрейфующие искры.
// Всё от времени и seed, поэтому половины синхронны.
// args: color, glow, accent, mote, drift (<0 вверх, >0 вниз), rate, period (сек), rx, ry, groundOffset, seed, part

// Обод сферы (эффект Френеля): центр прозрачный, к краю ярче — цветом ауры и тонкая кромка цветом акцента
export const rimTextures = () => ({
  color: radialTexture([[0, 0], [0.62, 0], [0.86, 0.45], [0.96, 0.2], [1, 0]]),
  accent: radialTexture([[0, 0], [0.86, 0], [0.96, 1], [1, 0]]),
});

// Обод из двух спрайтов; set ставит его эллипсом с силой alpha
export class Rim {
  readonly root = new Container();
  private readonly color: Sprite;
  private readonly accent: Sprite;

  constructor(color: number, accent: number) {
    const textures = rimTextures();
    this.color = ellipseSprite(textures.color, color);
    this.accent = ellipseSprite(textures.accent, accent);
    this.root.addChild(this.color, this.accent);
  }

  set(x: number, y: number, rx: number, ry: number, alpha: number) {
    this.root.visible = alpha > 0.001;
    placeEllipse(this.color, x, y, rx, ry);
    placeEllipse(this.accent, x, y, rx, ry);
    this.color.alpha = this.accent.alpha = alpha;
  }
}

const AURA_ORBITS = [
  { dy: 0.22, rx: 1.12, ry: 0.2, tilt: -0.12, speed: 0.0011, motes: 3 },
  { dy: -0.28, rx: 0.92, ry: 0.16, tilt: 0.2, speed: -0.0016, motes: 2 },
];

// Половина наклонного эллипса: front — нижняя дуга (0..π, перед юнитом), иначе верхняя (π..2π)
const halfEllipse = (rx: number, ry: number, tilt: number, front: boolean) => {
  const points: number[] = [];
  const from = front ? 0 : Math.PI;
  const cos = Math.cos(tilt);
  const sin = Math.sin(tilt);
  for (let i = 0; i <= 24; i++) {
    const a = from + (Math.PI * i) / 24;
    const px = Math.cos(a) * rx;
    const py = Math.sin(a) * ry;
    points.push(px * cos - py * sin, px * sin + py * cos);
  }
  return points;
};

interface Drifter {
  mote: Mote;
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  rate: number;
  size: number;
  rotation: number;
}

export const createAuraSphere: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const front = args.part === 'front';
  const colorCss: string = args.color ?? '#ffd166';
  const color = tintOf(colorCss);
  const glow = tintOf(args.glow ?? colorCss);
  const accent = tintOf(args.accent, '#ffffff');
  const mote: AuraMote = args.mote ?? 'dot';
  const rx = Number(args.rx ?? 60);
  const ry = Number(args.ry ?? 80);
  const period = Number(args.period ?? 2) * 1000;
  const seed = Number(args.seed ?? 0);
  const drift = Number(args.drift ?? -1);
  const rate = clamp01(Number(args.rate ?? 0.25));
  const groundOffset = Number(args.groundOffset ?? ry * 0.75);

  const root = new Container();
  root.blendMode = 'add';
  // Сфера целиком (обод, блик, орбиты) — в своей системе с центром в точке эмиттера, «дыхание» — её масштаб
  const sphere = new Container();
  root.addChild(sphere);

  // ----- Задняя половина: земля и дымка -----
  const groundGlow = ellipseSprite(radialTexture([[0, 0.5], [0.55, 0.22], [1, 0]]), glow);
  const ground = new Container();
  const ripple = new Sprite(fxTexture('ring'));
  ripple.anchor.set(0.5);
  ripple.tint = color;
  // Внешнее кольцо с засечками и пунктирное внутреннее; вращаются в разные стороны
  const outerRing = new Graphics().circle(0, 0, rx * 1.02).stroke({ width: 2, color });
  for (let i = 0; i < 12; i++) {
    const a = (i * Math.PI) / 6;
    outerRing.moveTo(Math.cos(a) * rx * 1.02, Math.sin(a) * rx * 1.02).lineTo(Math.cos(a) * rx * 1.12, Math.sin(a) * rx * 1.12);
  }
  outerRing.stroke({ width: 2.5, color, cap: 'round' });
  outerRing.alpha = 0.55;
  const innerRing = new Graphics();
  const dash = rx * 0.18;
  const gap = rx * 0.12;
  const circumference = Math.PI * 2 * rx * 0.8;
  for (let s = 0; s < circumference; s += dash + gap) {
    const a0 = s / (rx * 0.8);
    const a1 = Math.min(s + dash, circumference) / (rx * 0.8);
    innerRing.moveTo(Math.cos(a0) * rx * 0.8, Math.sin(a0) * rx * 0.8).arc(0, 0, rx * 0.8, a0, a1);
  }
  innerRing.stroke({ width: 1.5, color, cap: 'round' });
  innerRing.alpha = 0.55;
  const haze = ellipseSprite(radialTexture([[0, 0.1], [0.6, 0.14], [0.92, 0.28], [1, 0]]), glow);
  if (!front) {
    ground.scale.y = 0.3;
    ground.addChild(ripple, outerRing, innerRing);
    root.addChildAt(groundGlow, 0);
    root.addChildAt(ground, 1);
    sphere.addChild(haze);
  }

  // ----- Передняя половина: волна, обод, блик -----
  const pulse = new Rim(color, accent);
  const rim = new Rim(color, accent);
  const highlight = ellipseSprite(radialTexture([[0, 0.38], [1, 0]]), accent);
  const glint = new Graphics();
  if (front) {
    sphere.addChild(pulse.root, rim.root, highlight);
    // Тонкий отблеск на противоположном краю
    const points: number[] = [];
    for (let i = 0; i <= 12; i++) {
      const a = 0.15 + (0.8 * i) / 12;
      points.push(Math.cos(a) * rx * 0.93, Math.sin(a) * ry * 0.93);
    }
    glint.poly(points, false).stroke({ width: 1.5, color: accent, alpha: 0.22 });
    sphere.addChild(glint);
    highlight.rotation = -0.65;
  }

  // ----- Орбиты: своя половина каждой и бегущие по ним частицы -----
  const orbitMotes: { orbit: typeof AURA_ORBITS[number]; index: number; mote: Mote }[] = [];
  for (const orbit of AURA_ORBITS) {
    const line = new Graphics()
      .poly(halfEllipse(orbit.rx * rx, orbit.ry * ry, orbit.tilt, front), false)
      .stroke({ width: front ? 1.6 : 1.2, color, alpha: front ? 0.45 : 0.25 });
    line.position.set(0, orbit.dy * ry);
    sphere.addChild(line);
    for (let i = 0; i < orbit.motes; i++) {
      const m = new Mote(mote, color, accent);
      sphere.addChild(m.root);
      orbitMotes.push({ orbit, index: i, mote: m });
    }
  }

  // Дрейфующие искры (только передняя половина) живут в координатах сцены: эмиттер может уйти, искра — нет
  const drifters: Drifter[] = [];
  const drifterLayer = new Container();
  root.addChild(drifterLayer);

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      root.visible = opacity > 0;
      root.alpha = opacity;
      const time = performance.now() + seed;
      const phase = (time % period) / period;
      const breath = 1 + Math.sin(phase * Math.PI * 2) * 0.035;
      sphere.position.set(e.x, e.y);
      sphere.scale.set(breath);

      if (front) {
        const wave = clamp01(phase / 0.7);
        const waveScale = (1 + easeOut(wave) * 0.45) / breath;
        pulse.set(0, 0, rx * waveScale, ry * waveScale, wave >= 1 ? 0 : Math.pow(1 - wave, 1.6) * 0.55);
        rim.set(0, 0, rx, ry, 0.42);
        placeEllipse(highlight, -rx * 0.42, -ry * 0.55, rx * 0.34, ry * 0.11);
      } else {
        const gy = e.y + groundOffset;
        const flicker = 0.85 + Math.sin(time / 260) * 0.08;
        placeEllipse(groundGlow, e.x, gy, rx * 1.35, rx * 1.35 * 0.3);
        groundGlow.alpha = flicker;
        ground.position.set(e.x, gy);
        const rippleP = easeOut(clamp01(phase / 0.8));
        ripple.scale.set((rx * (0.55 + rippleP * 1.05)) / TEXTURE_FIGURE.ring);
        ripple.alpha = (1 - rippleP) * 0.75;
        outerRing.rotation = time / 2600;
        innerRing.rotation = time / 2600 - time / 1300;
        placeEllipse(haze, 0, 0, rx, ry);
      }

      for (const { orbit, index, mote: m } of orbitMotes) {
        const a = time * orbit.speed + (index * Math.PI * 2) / orbit.motes;
        const depth = Math.sin(a);
        m.root.visible = front === depth > 0;
        if (!m.root.visible) continue;
        const px = Math.cos(a) * orbit.rx * rx;
        const py = depth * orbit.ry * ry;
        const cos = Math.cos(orbit.tilt);
        const sin = Math.sin(orbit.tilt);
        const scale = 0.7 + (depth + 1) * 0.35;
        m.set(px * cos - py * sin, orbit.dy * ry + px * sin + py * cos, 3.2 * scale, 0.45 + (depth + 1) * 0.275, a);
      }

      if (!front) return;
      if (Math.random() < 1 - Math.pow(1 - rate, dt * 60)) {
        const a = Math.random() * Math.PI * 2;
        const r = Math.sqrt(Math.random()) * 0.8;
        const m = new Mote(mote, color, accent);
        drifterLayer.addChild(m.root);
        drifters.push({
          mote: m,
          x: e.x + Math.cos(a) * rx * r,
          y: e.y + Math.sin(a) * ry * r * 0.5 - drift * ry * 0.4,
          vx: rand(-0.15, 0.15) * 60,
          vy: drift * rand(0.35, 0.8) * 60,
          life: 1,
          rate: rand(0.008, 0.014) * 60,
          size: rand(1.8, 3.6),
          rotation: Math.random() * Math.PI,
        });
      }
      const drag = Math.pow(0.995, dt * 60);
      for (let i = drifters.length - 1; i >= 0; i--) {
        const d = drifters[i];
        d.vx += Math.sin(performance.now() / 500 + d.size * 3) * 0.008 * 3600 * dt;
        d.x += d.vx * dt;
        d.y += d.vy * dt;
        d.vx *= drag;
        d.vy *= drag;
        d.life -= d.rate * dt;
        if (d.life <= 0) {
          d.mote.root.destroy({ children: true });
          drifters.splice(i, 1);
          continue;
        }
        d.mote.set(d.x, d.y, d.size, Math.sin(Math.PI * clamp01(d.life)) * 0.9, d.rotation);
      }
    },
    destroy() {
      root.destroy({ children: true });
    },
  };
};
