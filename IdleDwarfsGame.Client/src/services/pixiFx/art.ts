import { Sprite, Texture } from 'pixi.js';
import type { BLEND_MODES } from 'pixi.js';
import { fxTexture, TEXTURE_FIGURE } from './textures';

// Заготовки эффектов, которые рисуются на 2D-канвасе один раз и дальше живут текстурами Pixi:
// радиальные градиенты по профилю прозрачности, снежинка, слои печати порчи, следы на земле.
// В кадре такие текстуры только двигаются, вращаются, масштабируются и меняют прозрачность

const textureCache = new Map<string, Texture>();

const cached = (key: string, draw: () => HTMLCanvasElement) => {
  let texture = textureCache.get(key);
  if (!texture) {
    texture = Texture.from(draw());
    textureCache.set(key, texture);
  }
  return texture;
};

const canvasOf = (size: number) => {
  const canvas = document.createElement('canvas');
  canvas.width = canvas.height = size;
  return canvas;
};

// ===== Радиальные градиенты =====

// Радиус текстуры градиента: спрайт масштабируется на rx / RADIAL_RADIUS по x и ry / RADIAL_RADIUS по y
export const RADIAL_RADIUS = 64;

// Белый круг с прозрачностью по профилю [смещение 0..1, альфа]: цвет даёт tint, эллипс — масштаб спрайта
export const radialTexture = (stops: [number, number][]) => cached(`radial|${stops.join(';')}`, () => {
  const canvas = canvasOf(RADIAL_RADIUS * 2);
  const ctx = canvas.getContext('2d');
  if (ctx) {
    const g = ctx.createRadialGradient(RADIAL_RADIUS, RADIAL_RADIUS, 0, RADIAL_RADIUS, RADIAL_RADIUS, RADIAL_RADIUS);
    for (const [offset, alpha] of stops) g.addColorStop(offset, `rgba(255,255,255,${alpha})`);
    ctx.fillStyle = g;
    ctx.fillRect(0, 0, RADIAL_RADIUS * 2, RADIAL_RADIUS * 2);
  }
  return canvas;
});

// Спрайт-эллипс по центру точки; blend — смешивание (по умолчанию add, как прежний 'lighter')
export const ellipseSprite = (texture: Texture, tint = 0xffffff, blend: BLEND_MODES = 'add') => {
  const sprite = new Sprite(texture);
  sprite.anchor.set(0.5);
  sprite.tint = tint;
  sprite.blendMode = blend;
  return sprite;
};

// Поставить эллипс-спрайт радиальной текстуры (или glow) в точку с полуосями rx, ry
export const placeEllipse = (sprite: Sprite, x: number, y: number, rx: number, ry: number, radius = RADIAL_RADIUS) => {
  sprite.position.set(x, y);
  sprite.scale.set(Math.max(rx, 0) / radius, Math.max(ry, 0) / radius);
};

// Мягкое свечение (как getGlowSprite) радиусом r
export const glowSprite = (tint: number, blend: BLEND_MODES = 'add') => ellipseSprite(fxTexture('glow'), tint, blend);
export const GLOW_RADIUS = TEXTURE_FIGURE.glow;

// ===== Снежинка =====

export const FLAKE_RADIUS = 10;
const FLAKE_BLUR = 12;

// Снежинка с морозным свечением; белая — цвет даёт tint
export const flakeTexture = () => cached('flake', () => {
  const half = FLAKE_RADIUS + FLAKE_BLUR;
  const canvas = canvasOf(half * 2);
  const ctx = canvas.getContext('2d');
  if (ctx) {
    ctx.translate(half, half);
    ctx.lineCap = 'round';
    ctx.lineWidth = 2.6;
    ctx.strokeStyle = '#ffffff';
    ctx.shadowColor = '#ffffff';
    ctx.shadowBlur = FLAKE_BLUR;
    for (let i = 0; i < 3; i++) {
      ctx.rotate(Math.PI / 3);
      ctx.beginPath();
      ctx.moveTo(-FLAKE_RADIUS, 0);
      ctx.lineTo(FLAKE_RADIUS, 0);
      ctx.stroke();
    }
  }
  return canvas;
});
// Половина текстуры снежинки
export const FLAKE_HALF = FLAKE_RADIUS + FLAKE_BLUR;

// ===== Печать порчи =====

const SIGIL_RES = 2;
const SIGIL_BLUR = 14;

// Слой печати со свечением, нарисованный один раз; extent — радиус без свечения, paint рисует с центром в 0, 0.
// Возвращает текстуру и её половину в координатах сцены
const sigilLayer = (key: string, color: string, extent: number, paint: (ctx: CanvasRenderingContext2D) => void) => {
  const half = Math.ceil((extent + SIGIL_BLUR * 1.5) * SIGIL_RES);
  const texture = cached(`sigil|${key}`, () => {
    const canvas = canvasOf(half * 2);
    const ctx = canvas.getContext('2d');
    if (ctx) {
      ctx.translate(half, half);
      ctx.scale(SIGIL_RES, SIGIL_RES);
      ctx.globalCompositeOperation = 'lighter';
      ctx.shadowColor = color;
      ctx.shadowBlur = SIGIL_BLUR * SIGIL_RES;
      paint(ctx);
    }
    return canvas;
  });
  return { texture, half: half / SIGIL_RES };
};

// Два кольца и глаз в центре
export const sigilBase = (color: string, accent: string, r: number) => sigilLayer(`base|${color}|${accent}|${r}`, color, r, ctx => {
  ctx.strokeStyle = color;
  ctx.lineWidth = 2.5;
  ctx.beginPath();
  ctx.arc(0, 0, r, 0, Math.PI * 2);
  ctx.stroke();
  ctx.lineWidth = 1.2;
  ctx.beginPath();
  ctx.arc(0, 0, r * 0.84, 0, Math.PI * 2);
  ctx.stroke();
  ctx.strokeStyle = accent;
  ctx.fillStyle = accent;
  ctx.lineWidth = 1.8;
  ctx.beginPath();
  ctx.moveTo(-r * 0.4, 0);
  ctx.quadraticCurveTo(0, -r * 0.32, r * 0.4, 0);
  ctx.quadraticCurveTo(0, r * 0.32, -r * 0.4, 0);
  ctx.stroke();
  ctx.beginPath();
  ctx.ellipse(0, 0, r * 0.06, r * 0.15, 0, 0, Math.PI * 2);
  ctx.fill();
});

// Гексаграмма и засечки по кругу
export const sigilHexagram = (color: string, r: number) => sigilLayer(`hex|${color}|${r}`, color, r, ctx => {
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.8;
  for (const offset of [0, Math.PI]) {
    ctx.beginPath();
    for (let i = 0; i < 3; i++) {
      const a = offset + (i * Math.PI * 2) / 3 - Math.PI / 2;
      if (i === 0) ctx.moveTo(Math.cos(a) * r * 0.8, Math.sin(a) * r * 0.8);
      else ctx.lineTo(Math.cos(a) * r * 0.8, Math.sin(a) * r * 0.8);
    }
    ctx.closePath();
    ctx.stroke();
  }
  for (let i = 0; i < 12; i++) {
    const a = (i * Math.PI) / 6;
    ctx.beginPath();
    ctx.moveTo(Math.cos(a) * r * 0.86, Math.sin(a) * r * 0.86);
    ctx.lineTo(Math.cos(a) * r * 0.98, Math.sin(a) * r * 0.98);
    ctx.stroke();
  }
});

// Внешнее пунктирное кольцо с ромбиками рун
export const sigilRunes = (color: string, accent: string, r: number) => sigilLayer(`runes|${color}|${accent}|${r}`, color, r * 1.4, ctx => {
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.4;
  ctx.setLineDash([r * 0.22, r * 0.1]);
  ctx.beginPath();
  ctx.arc(0, 0, r * 1.28, 0, Math.PI * 2);
  ctx.stroke();
  ctx.setLineDash([]);
  ctx.fillStyle = accent;
  for (let i = 0; i < 8; i++) {
    const a = (i * Math.PI) / 4;
    ctx.save();
    ctx.translate(Math.cos(a) * r * 1.28, Math.sin(a) * r * 1.28);
    ctx.rotate(a);
    ctx.beginPath();
    ctx.moveTo(r * 0.12, 0);
    ctx.lineTo(0, r * 0.07);
    ctx.lineTo(-r * 0.12, 0);
    ctx.lineTo(0, -r * 0.07);
    ctx.closePath();
    ctx.fill();
    ctx.restore();
  }
});

// ===== Следы на земле =====

export type GroundMarkKind = 'burn' | 'frost' | 'stain' | 'glow';

export const MARK_RADIUS = 80;
// Запас под размытие и шипы инея, которые выходят за пятно
export const MARK_SIZE = MARK_RADIUS * 3;

type MarkRay = [number, number, number];

const rand = (min: number, max: number) => min + Math.random() * (max - min);

const rgbaOf = (hex: string, alpha: number) => {
  const n = parseInt(hex.slice(1, 7), 16);
  return `rgba(${(n >> 16) & 255}, ${(n >> 8) & 255}, ${n & 255}, ${alpha})`;
};

const traceBlotch = (ctx: CanvasRenderingContext2D, radii: number[], scale = 1) => {
  const c = MARK_SIZE / 2;
  ctx.beginPath();
  radii.forEach((r, i) => {
    const a = (i / radii.length) * Math.PI * 2;
    const x = c + Math.cos(a) * r * scale;
    const y = c + Math.sin(a) * r * scale;
    if (i === 0) ctx.moveTo(x, y);
    else ctx.lineTo(x, y);
  });
  ctx.closePath();
};

const traceRays = (ctx: CanvasRenderingContext2D, rays: MarkRay[], from: number) => {
  const c = MARK_SIZE / 2;
  ctx.beginPath();
  for (const [a, length, bend] of rays) {
    const mid = (from + length) / 2;
    ctx.moveTo(c + Math.cos(a) * from, c + Math.sin(a) * from);
    ctx.lineTo(c + Math.cos(a + bend) * mid, c + Math.sin(a + bend) * mid);
    ctx.lineTo(c + Math.cos(a) * length, c + Math.sin(a) * length);
  }
};

const softGlow = (ctx: CanvasRenderingContext2D, color: string, x: number, y: number, r: number) => {
  const g = ctx.createRadialGradient(x, y, 0, x, y, r);
  g.addColorStop(0, rgbaOf(color, 0.9));
  g.addColorStop(0.25, rgbaOf(color, 0.45));
  g.addColorStop(1, rgbaOf(color, 0));
  ctx.fillStyle = g;
  ctx.fillRect(x - r, y - r, r * 2, r * 2);
};

// След по виду и цвету: основа (держится почти всю жизнь) и раскалённая часть (гаснет первой, add).
// additive — основа тоже светится (пятно света), а не затемняет землю
export const groundMarkTextures = (kind: GroundMarkKind, color: string) => {
  const key = `mark|${kind}|${color}`;
  const baseKey = `${key}|base`;
  if (!textureCache.has(baseKey)) {
    const base = canvasOf(MARK_SIZE);
    const glow = canvasOf(MARK_SIZE);
    const b = base.getContext('2d');
    const g = glow.getContext('2d');
    if (b && g) {
      b.lineCap = g.lineCap = 'round';
      b.lineJoin = g.lineJoin = 'round';
      const c = MARK_SIZE / 2;
      const R = MARK_RADIUS;
      const radii = Array.from({ length: 18 }, () => R * rand(0.72, 1));
      const rayCount = kind === 'frost' ? 11 : 7;
      const rays = Array.from({ length: rayCount }, (_, i): MarkRay => [
        (i / rayCount) * Math.PI * 2 + rand(-0.3, 0.3),
        R * (kind === 'frost' ? rand(0.7, 1.3) : rand(0.55, 0.95)),
        kind === 'glow' ? 0 : rand(-0.3, 0.3),
      ]);

      switch (kind) {
        case 'burn':
          b.shadowColor = 'rgba(12, 6, 2, 0.9)';
          b.shadowBlur = 18;
          b.fillStyle = 'rgba(28, 16, 8, 0.72)';
          traceBlotch(b, radii);
          b.fill();
          b.shadowBlur = 0;
          b.strokeStyle = 'rgba(8, 4, 2, 0.85)';
          b.lineWidth = 4;
          traceRays(b, rays, R * 0.15);
          b.stroke();
          g.shadowColor = color;
          g.shadowBlur = 12;
          g.strokeStyle = color;
          g.lineWidth = 3;
          traceRays(g, rays, R * 0.15);
          g.stroke();
          g.globalAlpha = 0.6;
          g.lineWidth = 4;
          traceBlotch(g, radii, 0.9);
          g.stroke();
          break;
        case 'frost':
          b.shadowColor = color;
          b.shadowBlur = 16;
          b.fillStyle = rgbaOf(color, 0.3);
          traceBlotch(b, radii);
          b.fill();
          b.shadowBlur = 6;
          b.strokeStyle = 'rgba(255, 255, 255, 0.75)';
          b.lineWidth = 3;
          traceRays(b, rays, R * 0.1);
          b.stroke();
          g.shadowColor = '#ffffff';
          g.shadowBlur = 14;
          g.strokeStyle = '#ffffff';
          g.lineWidth = 4;
          traceRays(g, rays, R * 0.1);
          g.stroke();
          break;
        case 'stain':
          b.shadowColor = rgbaOf(color, 0.8);
          b.shadowBlur = 14;
          b.fillStyle = rgbaOf(color, 0.42);
          traceBlotch(b, radii);
          b.fill();
          b.fillStyle = rgbaOf(color, 0.35);
          traceBlotch(b, radii, 0.55);
          b.fill();
          for (const [a, length] of rays) {
            b.beginPath();
            b.arc(c + Math.cos(a) * length * 1.25, c + Math.sin(a) * length * 1.25, rand(4, 9), 0, Math.PI * 2);
            b.fill();
          }
          g.shadowColor = color;
          g.shadowBlur = 14;
          g.strokeStyle = color;
          g.lineWidth = 4;
          traceBlotch(g, radii, 0.95);
          g.stroke();
          break;
        default:
          softGlow(b, color, c, c, R);
          b.shadowColor = color;
          b.shadowBlur = 12;
          b.strokeStyle = rgbaOf(color, 0.55);
          b.lineWidth = 5;
          traceRays(b, rays, R * 0.3);
          b.stroke();
          softGlow(g, '#ffffff', c, c, R * 0.6);
      }
    }
    textureCache.set(baseKey, Texture.from(base));
    textureCache.set(`${key}|glow`, Texture.from(glow));
  }
  return {
    base: textureCache.get(baseKey) as Texture,
    glow: textureCache.get(`${key}|glow`) as Texture,
    additive: kind === 'glow',
  };
};
