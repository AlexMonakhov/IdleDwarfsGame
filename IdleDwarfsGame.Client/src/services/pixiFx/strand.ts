import { MeshRope, Point, Texture, type RopeGeometry } from 'pixi.js';
import type { BoltPath } from './geometry';

// Светящаяся нить вдоль ломаной (молнии, дуги, ленты, блики) на MeshRope: число точек задаётся один раз,
// дальше в кадре меняются только их координаты — без триангуляции штриха, как у Graphics.
// Текстура — мягкая по ширине полоса (края прозрачные): ширина нити — ширина штриха, ореол — та же нить шире и тусклее

let texture: Texture | null = null;
const strandTexture = () => {
  if (texture) return texture;
  const canvas = document.createElement('canvas');
  canvas.width = 4;
  canvas.height = 32;
  const ctx = canvas.getContext('2d');
  if (ctx) {
    const g = ctx.createLinearGradient(0, 0, 0, 32);
    g.addColorStop(0, 'rgba(255,255,255,0)');
    g.addColorStop(0.3, 'rgba(255,255,255,1)');
    g.addColorStop(0.7, 'rgba(255,255,255,1)');
    g.addColorStop(1, 'rgba(255,255,255,0)');
    ctx.fillStyle = g;
    ctx.fillRect(0, 0, 4, 32);
  }
  texture = Texture.from(canvas);
  return texture;
};

export class Strand {
  readonly rope: MeshRope;
  private readonly points: Point[];

  constructor(count: number, width: number, color: number, alpha = 1) {
    this.points = Array.from({ length: Math.max(2, count) }, () => new Point());
    // Мягкие края съедают часть ширины текстуры: штрих шириной width — нить чуть шире
    this.rope = new MeshRope({ texture: strandTexture(), points: this.points, width: width * 1.4, textureScale: 0 });
    // Свой обработчик кадра у MeshRope снимаем: он каждый кадр возвращает нити ширину текстуры (width игнорируется —
    // все нити выходили толщиной 32px и сливались в белую полосу) и заново грузит вершины на видеокарту.
    // Вершины пересчитываются только по commit
    this.rope.onRender = null;
    this.rope.tint = color;
    this.rope.alpha = alpha;
    this.rope.blendMode = 'add';
  }

  // Новые координаты (сразу на видеокарту); лишние точки (путь короче) прижимаются к последней
  set(path: BoltPath) {
    const last = path[path.length - 1];
    this.points.forEach((point, i) => {
      const p = path[i] ?? last;
      point.set(p.x, p.y);
    });
    this.commit();
  }

  // Точка по одной — потом commit
  setXY(i: number, x: number, y: number) {
    this.points[i]?.set(x, y);
  }

  commit() {
    // Геометрия MeshRope — RopeGeometry (в типах Mesh она объявлена общей MeshGeometry)
    (this.rope.geometry as unknown as RopeGeometry).updateVertices();
  }

  get size() {
    return this.points.length;
  }
}
