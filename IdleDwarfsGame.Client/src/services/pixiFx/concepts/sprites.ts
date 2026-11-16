import { Texture } from 'pixi.js';
import type { CustomParticleTexture } from '../particles';

// Рисованные текстуры для концептов: листья, лепестки, снежинки, кристаллы, перья, капли, пузыри, шипы, языки пламени.
// Рисуются один раз на canvas в двойном разрешении (на ретине не мылятся) и живут в кэше.
// Цветные (лист, кристалл, капля, пузырь, шип, пламя) — уже раскрашены, tint им не нужен.
// Белые (лепесток, снежинка, перо) — под tint, как общие текстуры textures.ts.
// Вытянутые фигуры смотрят вправо (+x): частица с align поворачивается носом по полёту.
// Готовые картинки художника встают на место любой из них — меняется только то, откуда берётся Texture

const cache = new Map<string, CustomParticleTexture>();

const make = (key: string, width: number, height: number, figure: number, draw: (ctx: CanvasRenderingContext2D) => void): CustomParticleTexture => {
  let entry = cache.get(key);
  if (entry) return entry;
  const canvas = document.createElement('canvas');
  canvas.width = width;
  canvas.height = height;
  const ctx = canvas.getContext('2d');
  if (ctx) {
    ctx.translate(width / 2, height / 2);
    draw(ctx);
  }
  entry = { texture: Texture.from(canvas), figure };
  cache.set(key, entry);
  return entry;
};

// Половинка фигуры, очерченная двумя кривыми от хвоста (-r, 0) к носу (r, 0)
const almond = (ctx: CanvasRenderingContext2D, r: number, width: number, bulge = 0) => {
  ctx.beginPath();
  ctx.moveTo(-r, 0);
  ctx.bezierCurveTo(-r * 0.5 + bulge, -width * 1.3, r * 0.4, -width * 1.1, r, 0);
  ctx.bezierCurveTo(r * 0.4, width * 1.1, -r * 0.5 + bulge, width * 1.3, -r, 0);
  ctx.closePath();
};

// Лист: сочный зелёный, светлая середина, жилки, тёмный край
export const leafTexture = (hue: 'green' | 'gold' = 'green') => make(`leaf|${hue}`, 128, 128, 56, ctx => {
  const r = 54;
  const colors = hue === 'green' ? ['#ecfccb', '#84cc16', '#15803d'] : ['#fef9c3', '#facc15', '#c2410c'];
  // Черешок
  ctx.strokeStyle = hue === 'green' ? '#3f6212' : '#9a3412';
  ctx.lineWidth = 3;
  ctx.beginPath();
  ctx.moveTo(-r - 6, 2);
  ctx.lineTo(-r + 6, 0);
  ctx.stroke();
  almond(ctx, r, 20, -6);
  const g = ctx.createLinearGradient(0, -24, 0, 24);
  g.addColorStop(0, colors[1]);
  g.addColorStop(0.45, colors[0]);
  g.addColorStop(0.55, colors[1]);
  g.addColorStop(1, colors[2]);
  ctx.fillStyle = g;
  ctx.fill();
  ctx.lineWidth = 2;
  ctx.strokeStyle = hue === 'green' ? '#14532d' : '#7c2d12';
  ctx.stroke();
  // Жилки
  ctx.strokeStyle = 'rgba(255,255,240,0.85)';
  ctx.lineWidth = 2;
  ctx.beginPath();
  ctx.moveTo(-r + 4, 0);
  ctx.quadraticCurveTo(0, -2, r - 4, 0);
  ctx.stroke();
  ctx.lineWidth = 1.2;
  for (let i = 0; i < 4; i++) {
    const x = -r * 0.55 + i * 22;
    for (const side of [-1, 1]) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.quadraticCurveTo(x + 8, side * 8, x + 16, side * (16 - i * 2.5));
      ctx.stroke();
    }
  }
});

// Лепесток: белый, плотный у основания и прозрачнее к краю — цвет даёт tint
export const petalTexture = () => make('petal', 128, 128, 56, ctx => {
  const r = 54;
  ctx.beginPath();
  ctx.moveTo(-r, 0);
  ctx.bezierCurveTo(-r * 0.2, -36, r * 0.7, -30, r, 0);
  ctx.bezierCurveTo(r * 0.7, 30, -r * 0.2, 36, -r, 0);
  ctx.closePath();
  const g = ctx.createRadialGradient(-r, 0, 0, -r, 0, r * 2);
  g.addColorStop(0, 'rgba(255,255,255,1)');
  g.addColorStop(0.6, 'rgba(255,255,255,0.75)');
  g.addColorStop(1, 'rgba(255,255,255,0.35)');
  ctx.fillStyle = g;
  ctx.fill();
  ctx.strokeStyle = 'rgba(255,255,255,1)';
  ctx.lineWidth = 2.5;
  ctx.stroke();
  ctx.strokeStyle = 'rgba(255,255,255,0.6)';
  ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(-r + 6, 0);
  ctx.lineTo(r * 0.6, 0);
  ctx.stroke();
});

// Снежинка: шесть лучей с веточками и мягким свечением — белая, под tint
export const snowflakeTexture = () => make('snowflake', 128, 128, 50, ctx => {
  const r = 46;
  ctx.lineCap = 'round';
  ctx.strokeStyle = '#ffffff';
  ctx.shadowColor = '#ffffff';
  for (const [width, blur] of [[7, 14], [3.5, 4]] as const) {
    ctx.lineWidth = width;
    ctx.shadowBlur = blur;
    for (let i = 0; i < 6; i++) {
      ctx.save();
      ctx.rotate((i * Math.PI) / 3);
      ctx.beginPath();
      ctx.moveTo(0, 0);
      ctx.lineTo(r, 0);
      for (const [at, len] of [[0.38, 0.3], [0.66, 0.22]] as const) {
        ctx.moveTo(r * at, 0);
        ctx.lineTo(r * (at + len * 0.7), -r * len);
        ctx.moveTo(r * at, 0);
        ctx.lineTo(r * (at + len * 0.7), r * len);
      }
      ctx.stroke();
      ctx.restore();
    }
  }
  ctx.shadowBlur = 0;
  ctx.fillStyle = '#ffffff';
  ctx.beginPath();
  ctx.arc(0, 0, 6, 0, Math.PI * 2);
  ctx.fill();
});

// Осколок льда: гранёный, светлая и тёмная грани, белое ребро
export const iceShardTexture = () => make('iceShard', 128, 128, 56, ctx => {
  const r = 54;
  const top: [number, number][] = [[-r, 0], [-r * 0.2, -16], [r, 0]];
  const bottom: [number, number][] = [[-r, 0], [-r * 0.1, 15], [r, 0]];
  const poly = (points: [number, number][], fill: string) => {
    ctx.beginPath();
    points.forEach(([x, y], i) => (i ? ctx.lineTo(x, y) : ctx.moveTo(x, y)));
    ctx.closePath();
    ctx.fillStyle = fill;
    ctx.fill();
  };
  poly(top, '#e0f6ff');
  poly(bottom, '#38bdf8');
  ctx.strokeStyle = '#ffffff';
  ctx.lineWidth = 2;
  ctx.beginPath();
  ctx.moveTo(-r, 0);
  ctx.lineTo(r, 0);
  ctx.stroke();
  ctx.strokeStyle = 'rgba(14,116,144,0.9)';
  ctx.lineWidth = 1.5;
  ctx.beginPath();
  [...top, ...bottom.slice(1).reverse()].forEach(([x, y], i) => (i ? ctx.lineTo(x, y) : ctx.moveTo(x, y)));
  ctx.closePath();
  ctx.stroke();
});

// Кристалл льда для гробницы: высокий гранёный шип, основание внизу по центру (anchor 0.5, 1).
// figure — высота шипа в пикселях текстуры
export const CRYSTAL_W = 96;
export const CRYSTAL_H = 256;
export const crystalTexture = (variant: 0 | 1 | 2) => make(`crystal|${variant}`, CRYSTAL_W, CRYSTAL_H, CRYSTAL_H - 8, ctx => {
  ctx.translate(0, CRYSTAL_H / 2);
  const w = [40, 34, 44][variant];
  const h = CRYSTAL_H - 8;
  const tipX = [4, -6, 0][variant];
  const shoulder = [0.72, 0.78, 0.68][variant];
  const left: [number, number][] = [[-w, 0], [-w * 0.8, -h * shoulder], [tipX, -h], [tipX * 0.3, 0]];
  const right: [number, number][] = [[tipX * 0.3, 0], [tipX, -h], [w * 0.85, -h * (shoulder - 0.06)], [w, 0]];
  const face = (points: [number, number][], stops: [number, string][]) => {
    ctx.beginPath();
    points.forEach(([x, y], i) => (i ? ctx.lineTo(x, y) : ctx.moveTo(x, y)));
    ctx.closePath();
    const g = ctx.createLinearGradient(0, 0, 0, -h);
    stops.forEach(([at, c]) => g.addColorStop(at, c));
    ctx.fillStyle = g;
    ctx.fill();
  };
  face(left, [[0, 'rgba(125,211,252,0.95)'], [0.6, 'rgba(224,246,255,0.92)'], [1, 'rgba(255,255,255,1)']]);
  face(right, [[0, 'rgba(3,105,161,0.95)'], [0.6, 'rgba(14,165,233,0.92)'], [1, 'rgba(186,230,253,1)']]);
  // Блик-трещина внутри и светлое ребро
  ctx.strokeStyle = 'rgba(255,255,255,0.75)';
  ctx.lineWidth = 2;
  ctx.beginPath();
  ctx.moveTo(-w * 0.45, -h * 0.15);
  ctx.lineTo(-w * 0.3, -h * 0.45);
  ctx.lineTo(-w * 0.5, -h * 0.6);
  ctx.stroke();
  ctx.strokeStyle = '#ffffff';
  ctx.lineWidth = 3;
  ctx.beginPath();
  ctx.moveTo(tipX * 0.3, 0);
  ctx.lineTo(tipX, -h);
  ctx.stroke();
  ctx.strokeStyle = 'rgba(8,47,73,0.8)';
  ctx.lineWidth = 2;
  ctx.beginPath();
  [...left, ...right.slice(2)].forEach(([x, y], i) => (i ? ctx.lineTo(x, y) : ctx.moveTo(x, y)));
  ctx.closePath();
  ctx.stroke();
});

// Перо: стержень и опахало, плотное у стержня и мягкое к краю — белое, под tint
export const featherTexture = () => make('feather', 128, 128, 56, ctx => {
  const r = 54;
  ctx.beginPath();
  ctx.moveTo(-r * 0.7, 0);
  ctx.bezierCurveTo(-r * 0.2, -26, r * 0.6, -20, r, -2);
  ctx.bezierCurveTo(r * 0.6, 14, -r * 0.2, 22, -r * 0.7, 0);
  ctx.closePath();
  const g = ctx.createLinearGradient(0, -24, 0, 22);
  g.addColorStop(0, 'rgba(255,255,255,0.35)');
  g.addColorStop(0.5, 'rgba(255,255,255,1)');
  g.addColorStop(1, 'rgba(255,255,255,0.4)');
  ctx.fillStyle = g;
  ctx.fill();
  // Бородки
  ctx.strokeStyle = 'rgba(255,255,255,0.55)';
  ctx.lineWidth = 1;
  for (let i = 0; i < 9; i++) {
    const x = -r * 0.5 + i * 11;
    ctx.beginPath();
    ctx.moveTo(x, -1);
    ctx.lineTo(x + 9, -16 + Math.abs(i - 4) * 1.5);
    ctx.moveTo(x, 1);
    ctx.lineTo(x + 9, 14 - Math.abs(i - 4) * 1.5);
    ctx.stroke();
  }
  ctx.strokeStyle = '#ffffff';
  ctx.lineWidth = 2.5;
  ctx.beginPath();
  ctx.moveTo(-r, 1);
  ctx.quadraticCurveTo(0, 0, r, -2);
  ctx.stroke();
});

// Капля: хвост слева, круглая голова справа, глянцевый блик
export const dropTexture = (kind: 'blood' | 'venom') => make(`drop|${kind}`, 96, 96, 40, ctx => {
  const c = kind === 'blood' ? ['#fecaca', '#ef4444', '#7f1d1d'] : ['#f7fee7', '#a3e635', '#365314'];
  ctx.beginPath();
  ctx.moveTo(-40, 0);
  ctx.quadraticCurveTo(0, -18, 20, -18);
  ctx.arc(20, 0, 18, -Math.PI / 2, Math.PI / 2);
  ctx.quadraticCurveTo(0, 18, -40, 0);
  ctx.closePath();
  const g = ctx.createRadialGradient(24, -6, 2, 14, 0, 34);
  g.addColorStop(0, c[0]);
  g.addColorStop(0.35, c[1]);
  g.addColorStop(1, c[2]);
  ctx.fillStyle = g;
  ctx.fill();
  ctx.fillStyle = 'rgba(255,255,255,0.85)';
  ctx.beginPath();
  ctx.ellipse(26, -8, 6, 3.5, -0.4, 0, Math.PI * 2);
  ctx.fill();
});

// Пузырь яда: прозрачная середина, яркий ободок, блик
export const bubbleTexture = () => make('bubble', 96, 96, 40, ctx => {
  const g = ctx.createRadialGradient(0, 0, 0, 0, 0, 40);
  g.addColorStop(0, 'rgba(190,242,100,0.15)');
  g.addColorStop(0.75, 'rgba(163,230,53,0.35)');
  g.addColorStop(0.92, 'rgba(217,249,157,1)');
  g.addColorStop(1, 'rgba(101,163,13,0)');
  ctx.fillStyle = g;
  ctx.beginPath();
  ctx.arc(0, 0, 40, 0, Math.PI * 2);
  ctx.fill();
  ctx.fillStyle = 'rgba(255,255,255,0.9)';
  ctx.beginPath();
  ctx.ellipse(-14, -16, 9, 5, -0.6, 0, Math.PI * 2);
  ctx.fill();
});

// Шип лозы: изогнутый, основание внизу по центру (anchor 0.5, 1); figure — высота
export const THORN_W = 64;
export const THORN_H = 160;
export const thornTexture = () => make('thorn', THORN_W, THORN_H, THORN_H - 6, ctx => {
  ctx.translate(0, THORN_H / 2);
  const h = THORN_H - 6;
  ctx.beginPath();
  ctx.moveTo(-18, 0);
  ctx.quadraticCurveTo(-10, -h * 0.55, 12, -h);
  ctx.quadraticCurveTo(4, -h * 0.5, 18, 0);
  ctx.closePath();
  const g = ctx.createLinearGradient(-18, 0, 18, 0);
  g.addColorStop(0, '#d9f99d');
  g.addColorStop(0.35, '#84cc16');
  g.addColorStop(1, '#3f6212');
  ctx.fillStyle = g;
  ctx.fill();
  ctx.strokeStyle = '#1a2e05';
  ctx.lineWidth = 2;
  ctx.stroke();
  ctx.strokeStyle = 'rgba(247,254,231,0.9)';
  ctx.lineWidth = 2;
  ctx.beginPath();
  ctx.moveTo(-12, -4);
  ctx.quadraticCurveTo(-6, -h * 0.55, 10, -h + 6);
  ctx.stroke();
});

// Язык пламени: раскалённая голова справа, хвост тает влево (под сложение)
export const flameTexture = () => make('flame', 128, 128, 56, ctx => {
  const r = 54;
  ctx.beginPath();
  ctx.moveTo(-r, 0);
  ctx.bezierCurveTo(-r * 0.3, -14, r * 0.1, -30, r * 0.55, -28);
  ctx.arc(r * 0.55, 0, 28, -Math.PI / 2, Math.PI / 2);
  ctx.bezierCurveTo(r * 0.1, 30, -r * 0.3, 14, -r, 0);
  ctx.closePath();
  const g = ctx.createRadialGradient(r * 0.55, 0, 0, r * 0.3, 0, r * 1.3);
  g.addColorStop(0, 'rgba(255,255,240,1)');
  g.addColorStop(0.2, 'rgba(255,226,122,1)');
  g.addColorStop(0.5, 'rgba(255,122,26,0.85)');
  g.addColorStop(1, 'rgba(160,30,10,0)');
  ctx.fillStyle = g;
  ctx.fill();
});

// ----- Реалистичный лёд (гробница): полупрозрачный, без обводок, с гранями, бликами и инеем -----

// Детерминированный генератор: у одной текстуры каждый раз одни и те же грани и крапинки
const seeded = (seed: number) => () => {
  seed = (seed * 16807) % 2147483647;
  return (seed - 1) / 2147483646;
};

const tracePoly = (ctx: CanvasRenderingContext2D, points: [number, number][]) => {
  ctx.beginPath();
  points.forEach(([x, y], i) => (i ? ctx.lineTo(x, y) : ctx.moveTo(x, y)));
  ctx.closePath();
};


// Осколок прозрачного льда для разлёта: острый, просвечивает, светлое ребро. Смотрит вправо (+x)
export const iceChipTexture = () => make('iceChip', 96, 96, 40, ctx => {
  const r = 40;
  const points: [number, number][] = [[-r * 0.8, -6], [-r * 0.1, -14], [r, 0], [-r * 0.2, 12], [-r * 0.9, 5]];
  tracePoly(ctx, points);
  const g = ctx.createLinearGradient(0, -14, 0, 12);
  g.addColorStop(0, 'rgba(255,255,255,0.85)');
  g.addColorStop(0.5, 'rgba(186,230,253,0.6)');
  g.addColorStop(1, 'rgba(56,189,248,0.45)');
  ctx.fillStyle = g;
  ctx.fill();
  ctx.strokeStyle = 'rgba(255,255,255,0.9)';
  ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(-r * 0.8, -6);
  ctx.lineTo(-r * 0.1, -14);
  ctx.lineTo(r, 0);
  ctx.stroke();
});

// Ледяной клык: гранёный кристалл — три видимые грани разного тона, рёбра, остриё из фасеток, сколы по кромке,
// светлая сердцевина и преломления внутри, иней у основания. Прозрачнее iceSpike. Основание внизу по центру
export const ICE_FANG_W = 96;
export const ICE_FANG_H = 256;
export const iceFangTexture = (variant: 0 | 1 | 2) => make(`iceFang|${variant}`, ICE_FANG_W, ICE_FANG_H, ICE_FANG_H - 6, ctx => {
  ctx.translate(0, ICE_FANG_H / 2);
  const rnd = seeded(613 + variant * 29);
  const h = ICE_FANG_H - 6;
  const w = [34, 28, 40][variant];
  const tipX = [6, -8, 3][variant];
  const shoulder = [0.62, 0.68, 0.56][variant];
  // Кромка со сколами: между опорными точками — мелкие зазубрины
  const chip = (a: [number, number], b: [number, number], n: number, depth: number) => {
    const out: [number, number][] = [];
    for (let i = 1; i < n; i++) {
      const t = i / n;
      const nx = -(b[1] - a[1]);
      const ny = b[0] - a[0];
      const len = Math.hypot(nx, ny) || 1;
      const d = (rnd() - 0.6) * depth;
      out.push([a[0] + (b[0] - a[0]) * t + (nx / len) * d, a[1] + (b[1] - a[1]) * t + (ny / len) * d]);
    }
    return out;
  };
  const baseL: [number, number] = [-w, 0];
  const shL: [number, number] = [-w * 0.86, -h * shoulder];
  const facetL: [number, number] = [-w * 0.42, -h * (shoulder + 0.22)];
  const tip: [number, number] = [tipX, -h];
  const facetR: [number, number] = [w * 0.38, -h * (shoulder + 0.2)];
  const shR: [number, number] = [w * 0.82, -h * (shoulder - 0.03)];
  const baseR: [number, number] = [w, 0];
  const outline: [number, number][] = [baseL, ...chip(baseL, shL, 6, 5), shL, facetL, tip, facetR, shR, ...chip(shR, baseR, 6, 5), baseR];
  // Рёбра: делят тело на левую, среднюю и правую грани
  const ridgeL: [number, number][] = [[-w * 0.32, 0], [-w * 0.3, -h * shoulder], tip];
  const ridgeR: [number, number][] = [[w * 0.3, 0], [w * 0.28, -h * (shoulder - 0.02)], tip];
  ctx.save();
  tracePoly(ctx, outline);
  ctx.clip();
  const face = (points: [number, number][], stops: [number, string][]) => {
    tracePoly(ctx, points);
    const g = ctx.createLinearGradient(0, 0, 0, -h);
    stops.forEach(([at, c]) => g.addColorStop(at, c));
    ctx.fillStyle = g;
    ctx.fill();
  };
  face([baseL, shL, facetL, tip, ridgeL[1], ridgeL[0]], [[0, 'rgba(224,242,254,0.34)'], [0.7, 'rgba(240,249,255,0.42)'], [1, 'rgba(255,255,255,0.6)']]);
  face([ridgeL[0], ridgeL[1], tip, ridgeR[1], ridgeR[0]], [[0, 'rgba(186,230,253,0.16)'], [0.6, 'rgba(186,230,253,0.24)'], [1, 'rgba(255,255,255,0.5)']]);
  face([ridgeR[0], ridgeR[1], tip, facetR, shR, baseR], [[0, 'rgba(56,189,248,0.26)'], [0.7, 'rgba(125,211,252,0.3)'], [1, 'rgba(224,242,254,0.5)']]);
  // Фасетки острия
  tracePoly(ctx, [facetL, tip, ridgeL[1]]);
  ctx.fillStyle = 'rgba(255,255,255,0.22)';
  ctx.fill();
  tracePoly(ctx, [facetR, tip, ridgeR[1]]);
  ctx.fillStyle = 'rgba(14,165,233,0.16)';
  ctx.fill();
  // Сердцевина: мягкая светлая полоса по средней грани
  const core = ctx.createLinearGradient(-w * 0.2, 0, w * 0.2, 0);
  core.addColorStop(0, 'rgba(255,255,255,0)');
  core.addColorStop(0.5, 'rgba(255,255,255,0.22)');
  core.addColorStop(1, 'rgba(255,255,255,0)');
  ctx.fillStyle = core;
  ctx.fillRect(-w * 0.2, -h, w * 0.4, h);
  // Преломления: косые светлые полосы
  for (let i = 0; i < 4; i++) {
    const y0 = -h * (0.12 + rnd() * 0.6);
    ctx.strokeStyle = `rgba(255,255,255,${0.12 + rnd() * 0.16})`;
    ctx.lineWidth = 2 + rnd() * 3;
    ctx.beginPath();
    ctx.moveTo(-w, y0 + w * 0.4);
    ctx.lineTo(w, y0 - w * 0.5);
    ctx.stroke();
  }
  // Иней у основания
  const frost = ctx.createLinearGradient(0, 0, 0, -h * 0.28);
  frost.addColorStop(0, 'rgba(255,255,255,0.6)');
  frost.addColorStop(1, 'rgba(255,255,255,0)');
  ctx.fillStyle = frost;
  ctx.fillRect(-w, -h * 0.28, w * 2, h * 0.28);
  for (let i = 0; i < 60; i++) {
    ctx.fillStyle = `rgba(255,255,255,${0.15 + rnd() * 0.5})`;
    ctx.fillRect(-w + rnd() * w * 2, -rnd() * rnd() * h * 0.5, 1.5, 1.5);
  }
  ctx.restore();
  // Рёбра и кромка: светлые линии разной яркости
  ctx.lineJoin = 'round';
  ctx.lineCap = 'round';
  const line = (pts: [number, number][], css: string, width: number) => {
    ctx.strokeStyle = css;
    ctx.lineWidth = width;
    ctx.beginPath();
    pts.forEach(([x, y], i) => (i ? ctx.lineTo(x, y) : ctx.moveTo(x, y)));
    ctx.stroke();
  };
  line(outline.slice(0, outline.indexOf(tip) + 1), 'rgba(255,255,255,0.75)', 1.4);
  line(outline.slice(outline.indexOf(tip)), 'rgba(186,230,253,0.4)', 1);
  line(ridgeL, 'rgba(255,255,255,0.5)', 1.2);
  line(ridgeR, 'rgba(255,255,255,0.3)', 1);
  // Искры-блики
  for (const [sx, sy, r] of [[-w * 0.5, -h * 0.45, 5], [w * 0.15, -h * 0.78, 4]] as [number, number, number][]) {
    ctx.fillStyle = 'rgba(255,255,255,0.9)';
    ctx.beginPath();
    ctx.moveTo(sx, sy - r);
    ctx.lineTo(sx + r * 0.25, sy);
    ctx.lineTo(sx, sy + r);
    ctx.lineTo(sx - r * 0.25, sy);
    ctx.closePath();
    ctx.moveTo(sx - r, sy);
    ctx.lineTo(sx, sy + r * 0.25);
    ctx.lineTo(sx + r, sy);
    ctx.lineTo(sx, sy - r * 0.25);
    ctx.closePath();
    ctx.fill();
  }
});
