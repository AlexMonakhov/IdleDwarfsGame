import { Color, Container, Graphics } from 'pixi.js';
import { ellipseSprite, placeEllipse, radialTexture } from '../art';
import { ParticleSim, clamp01, easeOut, fromFrames, rand } from '../particles';
import { progressOf, tintOf, type PixiEffect, type PixiEffectFactory } from '../effect';
import { Strand } from '../strand';
import { envelope } from '../geometry';

// Ледяная темница (как IcePrisonStrategy): из-под ног цели вырастает частокол шипов, держится и раскалывается.
// args: radius, height, count, shatter (доля жизни, когда лёд раскалывается), thick, outer (шипов во внешнем кольце),
//       burst (ледяная крошка, когда темница смыкается), colors (палитра светлый → тёмный; по умолчанию лёд), gravity

const ICE = ['#ffffff', '#e0f6ff', '#bae6fd', '#7dd3fc', '#38bdf8'];
const PRISON_CLOSED = 0.26;

interface Spike {
  dx: number;
  dy: number;
  height: number;
  width: number;
  lean: number;
  delay: number;
  depth: number;
}

const iceRing = (count: number, radius: number, height: number, thick: number, lag = 0): Spike[] =>
  Array.from({ length: count }, (_, i) => {
    const a = (i / count) * Math.PI * 2 + rand(-0.2, 0.2);
    const depth = Math.sin(a);
    return {
      dx: Math.cos(a) * radius,
      dy: depth * radius * 0.3,
      height: height * rand(0.65, 1) * (depth > 0 ? 0.75 : 1),
      width: rand(14, 22) * thick,
      lean: -Math.cos(a) * 0.28,
      delay: lag + rand(0, 0.08),
      depth,
    };
  });

// Цвета темницы: свечение у ног, тело шипа (основание → верх) и блик
const prisonLook = (colors?: string[]) => {
  if (!colors?.length) {
    return {
      floor: '#7dd3fc',
      body: [['#0e7490', 0.6], ['#7dd3fc', 0.75], ['#f0f9ff', 0.95]] as [string, number][],
      edge: '#ffffff',
    };
  }
  const at = (share: number) => colors[Math.min(colors.length - 1, Math.round(share * (colors.length - 1)))];
  return {
    floor: at(0.5),
    body: [[at(1), 0.75], [at(0.6), 0.85], [at(0), 0.95]] as [string, number][],
    edge: at(0),
  };
};

export const createIcePrison: PixiEffectFactory = (emitter): PixiEffect => {
  const args = emitter.args ?? {};
  const radius = Number(args.radius ?? 40);
  const height = Number(args.height ?? 120);
  const thick = Number(args.thick ?? 1);
  const shatter = Number(args.shatter ?? 0.75);
  const palette = ((args.colors as string[] | undefined) ?? ICE).map(c => tintOf(c));
  const look = prisonLook(args.colors as string[] | undefined);
  const edge = tintOf(look.edge);
  // Тело шипа: градиент заменяем тремя полосами по высоте (основание, середина, верх)
  const body = look.body.map(([c, a]) => ({ color: new Color(c).toNumber(), alpha: a }));
  const spikes = [
    ...iceRing(Number(args.count ?? 8), radius, height, thick),
    ...iceRing(Number(args.outer ?? 0), radius * 1.7, height * 0.5, thick * 0.8, 0.05),
  ].sort((l, r) => l.dy - r.dy);

  const root = new Container();
  const floor = ellipseSprite(radialTexture([[0, 0.5], [0.6, 0.25], [1, 0]]), tintOf(look.floor));
  const bodies = new Graphics();
  // Блики по граням — нить из двух точек на шип: мерцают прозрачностью, без перестройки геометрии
  const edges = new Container();
  const glints = spikes.map(() => new Strand(2, 1.5, edge));
  const grownGlints = spikes.map(() => false);
  edges.addChild(...glints.map(g => g.rope));
  const shards = new ParticleSim({
    texture: 'shard',
    drag: fromFrames.factor(0.97), gravity: fromFrames.gravity(Number(args.gravity ?? 0.22)),
    alpha: life => clamp01(life * 1.3),
  });
  root.addChild(floor, bodies, edges, shards.container);
  let closed = false;
  let shattered = false;
  let grown = false;

  const pick = () => palette[Math.floor(Math.random() * palette.length)];

  return {
    root,
    update(e, dt) {
      const opacity = e.opacity ?? 1;
      const p = progressOf(e);
      const { x, y } = e;

      if (!closed && p >= PRISON_CLOSED) {
        closed = true;
        for (let i = 0; i < Number(args.burst ?? 0); i++) {
          const a = Math.random() * Math.PI * 2;
          const v = rand(2.5, 6);
          shards.spawn({
            x: x + Math.cos(a) * radius * 0.8, y: y - rand(0, height * 0.8),
            vx: fromFrames.speed(Math.cos(a) * v), vy: fromFrames.speed(Math.sin(a) * v * 0.6 - rand(1, 3)),
            lifetime: fromFrames.lifetime(rand(0.02, 0.035)),
            size: rand(2.5, 6), tint: pick(), rotation: a, spin: fromFrames.spin(rand(-0.3, 0.3)),
          });
        }
      }
      if (!shattered && p >= shatter) {
        shattered = true;
        for (const s of spikes) {
          for (let k = 0; k < 4; k++) {
            const along = rand(0.1, 0.9);
            shards.spawn({
              x: x + s.dx + s.lean * s.height * along, y: y + s.dy - s.height * along,
              vx: fromFrames.speed((Math.sign(s.dx) || 1) * rand(1, 3.5)), vy: fromFrames.speed(-rand(1, 4)),
              lifetime: fromFrames.lifetime(rand(0.025, 0.045)),
              size: rand(4, 8), tint: pick(), rotation: Math.random() * Math.PI * 2, spin: fromFrames.spin(rand(-0.3, 0.3)),
            });
          }
        }
      }
      shards.step(dt);
      shards.container.alpha = opacity;

      floor.alpha = opacity * envelope(p, 0.1, 0.25);
      floor.blendMode = 'add';
      placeEllipse(floor, x, y, radius * 1.6, radius * 0.45);

      // Шипы растут — перестраиваем; выросли — тело стоит, мерцает только блик; раскололись — исчезли
      bodies.visible = edges.visible = p < shatter;
      if (p >= shatter) return;
      const crack = clamp01((p - shatter + 0.08) / 0.08);
      const growing = spikes.some(s => p - s.delay < 0.22);
      if (growing || !grown) {
        bodies.clear();
        for (const s of spikes) {
          const grow = easeOut(clamp01((p - s.delay) / 0.22));
          if (grow <= 0) continue;
          const h = s.height * grow;
          const bx = x + s.dx;
          const by = y + s.dy;
          const alpha = s.depth > 0 ? 0.7 : 0.5;
          // Три полосы по высоте вместо градиента: основание, середина, верх
          for (let band = 0; band < 3; band++) {
            const t0 = band / 3;
            const t1 = (band + 1) / 3;
            const w0 = (s.width / 2) * (1 - t0);
            const w1 = (s.width / 2) * (1 - t1);
            const cx0 = bx + s.lean * h * t0;
            const cx1 = bx + s.lean * h * t1;
            const y0 = by - h * t0;
            const y1 = by - h * t1;
            const points = band === 0
              ? [cx0 - w0, y0, cx1 - w1, y1, cx1 + w1, y1, cx0 + w0, y0, bx, by + s.width * 0.2]
              : [cx0 - w0, y0, cx1 - w1, y1, cx1 + w1, y1, cx0 + w0, y0];
            bodies.poly(points).fill({ color: body[band].color, alpha: body[band].alpha * alpha });
          }
        }
        grown = !growing;
      }
      spikes.forEach((s, i) => {
        const grow = easeOut(clamp01((p - s.delay) / 0.22));
        const glint = glints[i];
        glint.rope.visible = grow > 0;
        if (grow <= 0) return;
        const h = s.height * grow;
        const bx = x + s.dx;
        const by = y + s.dy;
        glint.setXY(0, bx - s.width * 0.1, by);
        glint.setXY(1, bx + s.lean * h, by - h);
        // Шип растёт — точки меняются; вырос — геометрия стоит, мерцает только прозрачность
        if (grow < 1 || !grownGlints[i]) {
          glint.commit();
          grownGlints[i] = grow >= 1;
        }
        glint.rope.alpha = clamp01(0.45 + Math.sin(performance.now() / 120 + s.dx) * 0.15 + crack * 0.5);
      });
      bodies.alpha = edges.alpha = opacity;
    },
    destroy() {
      shards.destroy();
      root.destroy({ children: true });
    },
  };
};
