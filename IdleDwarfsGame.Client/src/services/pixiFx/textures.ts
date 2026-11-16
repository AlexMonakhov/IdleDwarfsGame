import { Texture } from 'pixi.js';

// Текстуры частиц: рисуются процедурно один раз (белые — цвет даёт tint спрайта) и живут всё время.
// Позже их заменят картинками из атласа — код эффектов от этого не меняется, только источник текстуры.
// Свечение — мягкие края самой текстуры + аддитивное смешивание, а не shadowBlur на каждом кадре

export type FxTextureName =
  | 'glow'     // мягкий круг: огоньки, вспышки, засветка
  | 'dot'      // искра-точка с ореолом
  | 'plus'     // плюсик (лечение)
  | 'star'     // четырёхлучевая звезда
  | 'shard'    // осколок-треугольник
  | 'smoke'    // клуб дыма
  | 'debris'   // обломок камня
  | 'ring'     // кольцо со свечением (ударная волна, круги на земле)
  | 'disc'     // сплошной круг: шлейф снаряда, языки пламени, искры удара
  | 'lance'    // вытянутый ромб с прозрачным хвостом: ледяной осколок-снаряд
  | 'star5';   // пятиконечная звезда со свечением (оглушение)

const cache = new Map<FxTextureName, Texture>();

// Все текстуры — для прогрева рендера (PixiScene.prewarm)
export const FX_TEXTURE_NAMES: FxTextureName[] = ['glow', 'dot', 'plus', 'star', 'shard', 'smoke', 'debris', 'ring', 'disc', 'lance', 'star5'];

const canvasOf = (size: number, draw: (ctx: CanvasRenderingContext2D, half: number) => void) => {
  const canvas = document.createElement('canvas');
  canvas.width = canvas.height = size;
  const ctx = canvas.getContext('2d');
  if (ctx) draw(ctx, size / 2);
  return canvas;
};

const star = (ctx: CanvasRenderingContext2D, outer: number, inner: number, points: number) => {
  ctx.beginPath();
  for (let i = 0; i < points * 2; i++) {
    const r = i % 2 === 0 ? outer : inner;
    const a = (i * Math.PI) / points - Math.PI / 2;
    if (i === 0) ctx.moveTo(Math.cos(a) * r, Math.sin(a) * r);
    else ctx.lineTo(Math.cos(a) * r, Math.sin(a) * r);
  }
  ctx.closePath();
  ctx.fill();
};

// Размер текстуры — с запасом под размытие края; во сколько раз фигура меньше текстуры, знает эффект (TEXTURE_FIGURE)
const DRAWERS: Record<FxTextureName, () => HTMLCanvasElement> = {
  glow: () => canvasOf(128, (ctx, h) => {
    // Профиль как у прежнего спрайта свечения (getGlowSprite): огоньки и вспышки той же яркости
    const g = ctx.createRadialGradient(h, h, 0, h, h, h);
    g.addColorStop(0, 'rgba(255,255,255,0.9)');
    g.addColorStop(0.25, 'rgba(255,255,255,0.45)');
    g.addColorStop(1, 'rgba(255,255,255,0)');
    ctx.fillStyle = g;
    ctx.fillRect(0, 0, h * 2, h * 2);
  }),
  dot: () => canvasOf(64, (ctx, h) => {
    ctx.translate(h, h);
    ctx.fillStyle = '#fff';
    ctx.shadowColor = '#fff';
    ctx.shadowBlur = 16;
    ctx.beginPath();
    ctx.arc(0, 0, 7, 0, Math.PI * 2);
    ctx.fill();
  }),
  plus: () => canvasOf(64, (ctx, h) => {
    ctx.translate(h, h);
    ctx.fillStyle = '#fff';
    ctx.shadowColor = '#fff';
    ctx.shadowBlur = 16;
    const s = 12;
    const t = s * 0.35;
    ctx.fillRect(-s, -t / 2, s * 2, t);
    ctx.fillRect(-t / 2, -s, t, s * 2);
  }),
  star: () => canvasOf(64, (ctx, h) => {
    ctx.translate(h, h);
    ctx.fillStyle = '#fff';
    ctx.shadowColor = '#fff';
    ctx.shadowBlur = 16;
    star(ctx, 12, 4.2, 4);
  }),
  shard: () => canvasOf(32, (ctx, h) => {
    ctx.translate(h, h);
    ctx.fillStyle = '#fff';
    ctx.beginPath();
    ctx.moveTo(14, 0);
    ctx.lineTo(-8.4, 4.9);
    ctx.lineTo(-8.4, -4.9);
    ctx.closePath();
    ctx.fill();
  }),
  smoke: () => canvasOf(64, (ctx, h) => {
    const g = ctx.createRadialGradient(h, h, 0, h, h, h);
    g.addColorStop(0, 'rgba(255,255,255,1)');
    g.addColorStop(0.6, 'rgba(255,255,255,0.8)');
    g.addColorStop(1, 'rgba(255,255,255,0)');
    ctx.fillStyle = g;
    ctx.fillRect(0, 0, h * 2, h * 2);
  }),
  debris: () => canvasOf(32, (ctx, h) => {
    ctx.translate(h, h);
    const s = 12;
    ctx.fillStyle = '#fff';
    ctx.beginPath();
    ctx.moveTo(s, 0);
    ctx.lineTo(s * 0.2, s * 0.9);
    ctx.lineTo(-s * 0.9, s * 0.3);
    ctx.lineTo(-s * 0.5, -s * 0.8);
    ctx.closePath();
    ctx.fill();
  }),
  ring: () => canvasOf(256, (ctx, h) => {
    // Кольцо радиусом 100 из 128: тонкая яркая линия и мягкий ореол вокруг неё
    ctx.translate(h, h);
    ctx.strokeStyle = '#fff';
    ctx.globalAlpha = 0.25;
    ctx.lineWidth = 18;
    ctx.beginPath();
    ctx.arc(0, 0, 100, 0, Math.PI * 2);
    ctx.stroke();
    ctx.globalAlpha = 1;
    ctx.lineWidth = 6;
    ctx.stroke();
  }),
  disc: () => canvasOf(64, (ctx, h) => {
    ctx.fillStyle = '#fff';
    ctx.beginPath();
    ctx.arc(h, h, 30, 0, Math.PI * 2);
    ctx.fill();
  }),
  lance: () => canvasOf(128, (ctx, h) => {
    // Нос справа: r·1.8 вперёд, r·2.4 назад, r·0.55 в ширину при r = 24
    ctx.translate(h, h);
    const r = 24;
    const body = ctx.createLinearGradient(-r * 2.2, 0, r * 1.6, 0);
    body.addColorStop(0, 'rgba(255,255,255,0)');
    body.addColorStop(0.5, 'rgba(255,255,255,0.85)');
    body.addColorStop(1, 'rgba(255,255,255,1)');
    ctx.fillStyle = body;
    ctx.beginPath();
    ctx.moveTo(r * 1.8, 0);
    ctx.lineTo(0, r * 0.55);
    ctx.lineTo(-r * 2.4, 0);
    ctx.lineTo(0, -r * 0.55);
    ctx.closePath();
    ctx.fill();
  }),
  star5: () => canvasOf(96, (ctx, h) => {
    ctx.translate(h, h);
    ctx.fillStyle = '#fff';
    ctx.shadowColor = '#fff';
    ctx.shadowBlur = 20;
    star(ctx, 14, 6.3, 5);
  }),
};

// Во сколько раз фигура меньше половины текстуры: эффект задаёт размер фигуры, а не текстуры
export const TEXTURE_FIGURE: Record<FxTextureName, number> = {
  glow: 64,   // радиус круга = половина текстуры
  dot: 12,   // как у canvas-искр: размер считается от 12, у точки радиус 0.6 от него
  plus: 12,
  star: 12,
  shard: 14,
  smoke: 32,
  debris: 12,
  ring: 100,
  disc: 30,
  lance: 24,
  star5: 14,
};

export const fxTexture = (name: FxTextureName): Texture => {
  let texture = cache.get(name);
  if (!texture) {
    texture = Texture.from(DRAWERS[name]());
    cache.set(name, texture);
  }
  return texture;
};
