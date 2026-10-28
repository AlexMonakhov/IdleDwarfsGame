import { EffectStrategy, EmitterConfig, Particle } from './interfaces/canvas-interfaces';
import { BoltPath, jaggedPath } from './canvas-effects';
import {
  ParticleStore,
  clamp01,
  drawStar,
  easeOut,
  fillEllipseGradient,
  getGlowSprite,
  paletteColor,
  progressOf,
  rand,
  rgba,
  stepParticles,
} from './combat-effects';

// Эффекты заклинаний стихий: у каждого свой способ доставки — вал по земле, туча над врагами,
// темница из-под ног, печать над головой и т.д. Время жизни задаёт travelDuration эмиттера.

const FIRE = ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'];
const ICE = ['#ffffff', '#e0f6ff', '#bae6fd', '#7dd3fc', '#38bdf8'];
const PRISM = ['#ffffff', '#ff7ad9', '#a78bfa', '#6bd5ff', '#6bff95', '#ffd166'];

// Плавное появление за fadeIn и угасание за fadeOut (доли жизни)
const envelope = (p: number, fadeIn: number, fadeOut: number) => Math.min(clamp01(p / fadeIn), clamp01((1 - p) / fadeOut));

const endpoints = (emitter: EmitterConfig) => ({
  sx: emitter.startX ?? emitter.x,
  sy: emitter.startY ?? emitter.y,
  ex: emitter.endX ?? emitter.x,
  ey: emitter.endY ?? emitter.y,
});

/**
 * Огненный вал: стена пламени катится по земле от startX к endX, за ней остаётся тлеющая полоса.
 * args: colors[], height, width (толщина вала), rate, travel (доля жизни на проход, дальше догорает)
 */
export class FireWallStrategy extends ParticleStore {
  private front(emitter: EmitterConfig) {
    const { sx, sy, ex, ey } = endpoints(emitter);
    const t = clamp01(progressOf(emitter) / Number(emitter.args?.travel ?? 0.75));
    return { x: sx + (ex - sx) * t, y: sy + (ey - sy) * t, t, sx, dir: Math.sign(ex - sx) || 1 };
  }

  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const particles = this.store.get(emitter.id) ?? [];
    const height = Number(args.height ?? 110);
    const width = Number(args.width ?? 46);
    const palette: string[] = args.colors ?? FIRE;
    const front = this.front(emitter);

    if (front.t < 1) {
      for (let i = 0; i < Number(args.rate ?? 9); i++) {
        // У самого фронта языки гуще и выше, позади — догорают пониже
        const back = Math.random() ** 2 * width * 2.2;
        const tall = 1 - (back / (width * 2.2)) * 0.6;
        particles.push({
          x: front.x - front.dir * back + rand(-width * 0.25, width * 0.25),
          y: front.y + rand(-6, 6),
          vx: front.dir * rand(0.4, 1.4),
          vy: -rand(1.4, 3.2) * (height / 110) * tall,
          life: 1,
          maxLife: rand(0.022, 0.04),
          size: rand(7, 13) * tall,
          color: palette[0],
        });
      }
    }

    this.store.set(emitter.id, stepParticles(particles, 0.985, -0.03, 0.975));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const particles = this.store.get(emitter.id);
    if (!particles) return;
    const args = emitter.args ?? {};
    const palette: string[] = args.colors ?? FIRE;
    const opacity = emitter.opacity ?? 1;
    const height = Number(args.height ?? 110);
    const width = Number(args.width ?? 46);
    const travel = Number(args.travel ?? 0.75);
    const p = progressOf(emitter);
    const front = this.front(emitter);
    const fade = p > travel ? 1 - (p - travel) / (1 - travel) : 1;

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    // Опалённая полоса земли: от кастера к фронту разгорается
    if (Math.abs(front.x - front.sx) > 1) {
      const scorch = ctx.createLinearGradient(front.sx, 0, front.x, 0);
      scorch.addColorStop(0, rgba('#ff4d00', 0));
      scorch.addColorStop(1, rgba('#ff7a1a', 0.45));
      ctx.globalAlpha = opacity * fade;
      ctx.fillStyle = scorch;
      ctx.beginPath();
      ctx.ellipse((front.sx + front.x) / 2, front.y, Math.abs(front.x - front.sx) / 2, 14, 0, 0, Math.PI * 2);
      ctx.fill();
    }

    // Жар перед валом
    ctx.globalAlpha = opacity * fade;
    fillEllipseGradient(ctx, front.x, front.y - height * 0.3, width * 1.6, height * 0.7, [
      [0, rgba('#ffb347', 0.4)],
      [1, rgba('#ff4d00', 0)],
    ]);

    for (const f of particles) {
      ctx.globalAlpha = opacity * clamp01(f.life * 1.4) * 0.85;
      ctx.fillStyle = paletteColor(palette, f.life);
      ctx.beginPath();
      ctx.ellipse(f.x, f.y, f.size * 0.7, f.size * 1.3, 0, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.restore();
  }
}

/**
 * Буран над группой целей: клубящаяся туча и косой снег под ней.
 * args: spread (ширина группы, добавляет BattleFx), margin, lift (на сколько выше эмиттера туча), fall (высота снегопада),
 *       wind, direction (1 — ветер вправо, -1 — влево), density (множитель снегопада)
 */
export class StormStrategy extends ParticleStore<{ puffs: Particle[]; snow: Particle[] }> {
  private layout(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    return {
      width: Number(args.spread ?? 0) + Number(args.margin ?? 160),
      cloudY: emitter.y - Number(args.lift ?? 70),
    };
  }

  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const { width, cloudY } = this.layout(emitter);
    let state = this.store.get(emitter.id);

    if (!state) {
      const count = Math.max(6, Math.round(width / 28));
      const puffs = Array.from({ length: count }, (_, i) => ({
        x: emitter.x - width / 2 + (width * (i + 0.5)) / count + rand(-10, 10),
        y: cloudY + rand(-14, 10),
        vx: rand(-0.15, 0.15),
        vy: 0,
        life: 1,
        maxLife: 0,
        size: rand(26, 40),
        color: '',
        rotation: Math.random() * Math.PI * 2,
      }));
      state = { puffs, snow: [] };
    }

    const p = progressOf(emitter);
    // direction — в какую сторону бьёт кастер: ветер дует от него
    const wind = Number(args.wind ?? 2.2) * Number(args.direction ?? 1);
    const fall = Number(args.fall ?? 260);
    if (p > 0.12 && p < 0.85) {
      const rate = (width / 50) * Number(args.density ?? 1);
      for (let i = 0; i < rate; i++) {
        const vy = rand(5, 8);
        state.snow.push({
          x: emitter.x + rand(-width / 2, width / 2) - wind * 20,
          y: cloudY + rand(0, 15),
          vx: wind * rand(0.8, 1.2),
          vy,
          life: 1,
          maxLife: vy / fall,
          size: rand(1.2, 2.6),
          color: Math.random() < 0.3 ? '#bae6fd' : '#ffffff',
        });
      }
    }

    for (const puff of state.puffs) puff.x += puff.vx;
    state.snow = stepParticles(state.snow);
    this.store.set(emitter.id, state);
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const state = this.store.get(emitter.id);
    if (!state) return;
    const opacity = emitter.opacity ?? 1;
    const { width, cloudY } = this.layout(emitter);
    const p = progressOf(emitter);
    const env = envelope(p, 0.15, 0.2);
    const swell = 0.6 + 0.4 * easeOut(clamp01(p / 0.2));
    const time = performance.now();

    ctx.save();

    // Туча — обычным смешиванием, чтобы затемняла фон
    ctx.globalAlpha = opacity * env * 0.9;
    for (const puff of state.puffs) {
      fillEllipseGradient(ctx, puff.x + Math.sin(time / 700 + (puff.rotation ?? 0)) * 4, puff.y, puff.size * 1.3 * swell, puff.size * 0.7 * swell, [
        [0, 'rgba(71, 85, 105, 0.85)'],
        [0.6, 'rgba(51, 65, 85, 0.6)'],
        [1, 'rgba(30, 41, 59, 0)'],
      ]);
    }

    ctx.globalCompositeOperation = 'lighter';
    // Холодный отсвет на брюхе тучи
    fillEllipseGradient(ctx, emitter.x, cloudY + 16, width * 0.5, 24, [
      [0, rgba('#7dd3fc', 0.35)],
      [1, rgba('#7dd3fc', 0)],
    ]);

    // Снег косыми штрихами
    ctx.lineCap = 'round';
    for (const s of state.snow) {
      ctx.globalAlpha = opacity * env * Math.sin(Math.PI * clamp01(s.life)) * 0.9;
      ctx.strokeStyle = s.color;
      ctx.lineWidth = s.size;
      ctx.beginPath();
      ctx.moveTo(s.x, s.y);
      ctx.lineTo(s.x - s.vx * 2.5, s.y - s.vy * 2.5);
      ctx.stroke();
    }

    ctx.restore();
  }
}

/**
 * Вспышка на весь экран (раскат грома): два коротких всполоха, свет сверху.
 * args: color, alpha
 */
export class ScreenFlashStrategy implements EffectStrategy {
  update() {
    // Целиком от времени
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const p = progressOf(emitter);
    const bump = (center: number, spread: number) => Math.exp(-(((p - center) / spread) ** 2));
    const k = Math.max(bump(0.08, 0.07), bump(0.32, 0.06) * 0.6);
    if (k < 0.01) return;

    const color = emitter.args?.color ?? '#fde68a';
    const { width, height } = ctx.canvas;
    ctx.save();
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.globalCompositeOperation = 'lighter';
    ctx.globalAlpha = (emitter.opacity ?? 1) * k * Number(emitter.args?.alpha ?? 0.35);
    const light = ctx.createLinearGradient(0, 0, 0, height);
    light.addColorStop(0, rgba(color, 1));
    light.addColorStop(1, rgba(color, 0.15));
    ctx.fillStyle = light;
    ctx.fillRect(0, 0, width, height);
    ctx.restore();
  }
}

interface IceSpike {
  dx: number;
  dy: number;
  height: number;
  width: number;
  lean: number;
  delay: number;
  depth: number;
}

// Кольцо шипов вокруг точки; thick — множитель ширины, lag — на сколько (доля жизни) позже они начинают расти
const iceRing = (count: number, radius: number, height: number, thick: number, lag = 0): IceSpike[] =>
  Array.from({ length: count }, (_, i) => {
    const a = (i / count) * Math.PI * 2 + rand(-0.2, 0.2);
    // depth > 0 — шип перед юнитом: пониже, чтобы не закрывать его целиком
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

// Доля жизни, к которой шипы выросли и темница сомкнулась
const PRISON_CLOSED = 0.26;

/**
 * Ледяная темница: из-под ног цели вырастает частокол ледяных шипов, держится и раскалывается.
 * args: radius, height, count, shatter (доля жизни, когда лёд раскалывается),
 *       thick (множитель ширины шипов), outer (шипов во внешнем кольце пониже),
 *       burst (сколько ледяной крошки брызжет во все стороны, когда темница смыкается)
 */
export class IcePrisonStrategy extends ParticleStore<{ spikes: IceSpike[]; shards: Particle[]; closed: boolean; shattered: boolean }> {
  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const radius = Number(args.radius ?? 40);
    const height = Number(args.height ?? 120);
    const thick = Number(args.thick ?? 1);
    let state = this.store.get(emitter.id);

    if (!state) {
      const spikes = [
        ...iceRing(Number(args.count ?? 8), radius, height, thick),
        ...iceRing(Number(args.outer ?? 0), radius * 1.7, height * 0.5, thick * 0.8, 0.05),
      ].sort((l, r) => l.dy - r.dy);
      state = { spikes, shards: [], closed: false, shattered: false };
    }

    const p = progressOf(emitter);
    if (!state.closed && p >= PRISON_CLOSED) {
      state.closed = true;
      for (let i = 0; i < Number(args.burst ?? 0); i++) {
        const a = Math.random() * Math.PI * 2;
        const v = rand(2.5, 6);
        state.shards.push({
          x: emitter.x + Math.cos(a) * radius * 0.8,
          y: emitter.y - rand(0, height * 0.8),
          vx: Math.cos(a) * v,
          vy: Math.sin(a) * v * 0.6 - rand(1, 3),
          life: 1,
          maxLife: rand(0.02, 0.035),
          size: rand(2.5, 6),
          color: ICE[Math.floor(Math.random() * ICE.length)],
          rotation: a,
          spin: rand(-0.3, 0.3),
        });
      }
    }

    if (!state.shattered && p >= Number(args.shatter ?? 0.75)) {
      state.shattered = true;
      for (const s of state.spikes) {
        for (let k = 0; k < 4; k++) {
          const along = rand(0.1, 0.9);
          state.shards.push({
            x: emitter.x + s.dx + s.lean * s.height * along,
            y: emitter.y + s.dy - s.height * along,
            vx: (Math.sign(s.dx) || 1) * rand(1, 3.5),
            vy: -rand(1, 4),
            life: 1,
            maxLife: rand(0.025, 0.045),
            size: rand(4, 8),
            color: ICE[Math.floor(Math.random() * ICE.length)],
            rotation: Math.random() * Math.PI * 2,
            spin: rand(-0.3, 0.3),
          });
        }
      }
    }

    state.shards = stepParticles(state.shards, 0.97, 0.22);
    this.store.set(emitter.id, state);
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const state = this.store.get(emitter.id);
    if (!state) return;
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const radius = Number(args.radius ?? 40);
    const shatter = Number(args.shatter ?? 0.75);
    const p = progressOf(emitter);
    const time = performance.now();
    const { x, y } = emitter;

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    ctx.globalAlpha = opacity * envelope(p, 0.1, 0.25);
    fillEllipseGradient(ctx, x, y, radius * 1.6, radius * 0.45, [
      [0, rgba('#e0f6ff', 0.5)],
      [0.6, rgba('#7dd3fc', 0.25)],
      [1, rgba('#38bdf8', 0)],
    ]);

    if (p < shatter) {
      // Перед тем как расколоться, лёд вспыхивает
      const crack = clamp01((p - shatter + 0.08) / 0.08);
      for (const s of state.spikes) {
        const grow = easeOut(clamp01((p - s.delay) / 0.22));
        if (grow <= 0) continue;
        const h = s.height * grow;
        const bx = x + s.dx;
        const by = y + s.dy;
        const tipX = bx + s.lean * h;
        const tipY = by - h;

        // Тело шипа — полупрозрачный лёд, юнит виден сквозь него
        ctx.globalCompositeOperation = 'source-over';
        ctx.globalAlpha = opacity * (s.depth > 0 ? 0.7 : 0.5);
        const body = ctx.createLinearGradient(bx, by, tipX, tipY);
        body.addColorStop(0, 'rgba(14, 116, 144, 0.6)');
        body.addColorStop(0.6, 'rgba(125, 211, 252, 0.75)');
        body.addColorStop(1, 'rgba(240, 249, 255, 0.95)');
        ctx.fillStyle = body;
        ctx.beginPath();
        ctx.moveTo(bx - s.width / 2, by);
        ctx.lineTo(tipX, tipY);
        ctx.lineTo(bx + s.width / 2, by);
        ctx.lineTo(bx, by + s.width * 0.2);
        ctx.closePath();
        ctx.fill();

        // Блик по грани
        ctx.globalCompositeOperation = 'lighter';
        ctx.globalAlpha = opacity * (0.45 + Math.sin(time / 120 + s.dx) * 0.15 + crack * 0.5);
        ctx.strokeStyle = '#ffffff';
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.moveTo(bx - s.width * 0.1, by);
        ctx.lineTo(tipX, tipY);
        ctx.stroke();
      }
    }

    ctx.globalCompositeOperation = 'lighter';
    for (const shard of state.shards) {
      ctx.save();
      ctx.translate(shard.x, shard.y);
      ctx.rotate(shard.rotation ?? 0);
      ctx.globalAlpha = opacity * clamp01(shard.life * 1.3);
      ctx.fillStyle = shard.color;
      ctx.beginPath();
      ctx.moveTo(shard.size, 0);
      ctx.lineTo(-shard.size * 0.6, shard.size * 0.35);
      ctx.lineTo(-shard.size * 0.6, -shard.size * 0.35);
      ctx.closePath();
      ctx.fill();
      ctx.restore();
    }

    ctx.restore();
  }
}

interface Vine {
  side: number;
  amp: number;
  height: number;
  turns: number;
  phase: number;
  thick: number;
  delay: number;
}

// Узлы листьев и шипов вдоль лозы (доля длины)
const LEAF_AT = [0.35, 0.6, 0.85];
const THORN_AT = [0.2, 0.48, 0.72];

/**
 * Цепкие корни: из земли под целью вырастают лозы, спиралью обвивают её и увядают.
 * Участки лозы «за спиной» юнита рисуются бледнее — так видно, что она обвивает тело.
 * args: count, height, width, thick (множитель толщины), color, light (прожилка), leaf
 */
export class RootsStrategy extends ParticleStore<{ vines: Vine[]; dirt: Particle[] }> {
  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    let state = this.store.get(emitter.id);

    if (!state) {
      const height = Number(args.height ?? 120);
      const width = Number(args.width ?? 60);
      const vines = Array.from({ length: Number(args.count ?? 5) }, (_, i) => ({
        side: i % 2 ? 1 : -1,
        amp: width * rand(0.35, 0.55),
        height: height * rand(0.7, 1),
        turns: rand(1.2, 1.8),
        phase: rand(0, Math.PI),
        thick: rand(5, 8) * Number(args.thick ?? 1),
        delay: i * 0.04,
      }));
      // Комья земли, выбитые лозами
      const dirt = Array.from({ length: 16 }, () => ({
        x: emitter.x + rand(-width * 0.5, width * 0.5),
        y: emitter.y + rand(-4, 4),
        vx: rand(-1.5, 1.5),
        vy: -rand(2, 4.5),
        life: 1,
        maxLife: rand(0.025, 0.04),
        size: rand(2, 4),
        color: Math.random() < 0.5 ? '#5b3a1e' : '#3f2a14',
      }));
      state = { vines, dirt };
    }

    state.dirt = stepParticles(state.dirt, 0.97, 0.22);
    this.store.set(emitter.id, state);
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const state = this.store.get(emitter.id);
    if (!state) return;
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const color = args.color ?? '#3f6212';
    const light = args.light ?? '#84cc16';
    const leaf = args.leaf ?? '#4ade80';
    const p = progressOf(emitter);
    const wither = clamp01((p - 0.75) / 0.25);
    const { x, y } = emitter;

    ctx.save();
    ctx.globalAlpha = opacity * (1 - wither);
    fillEllipseGradient(ctx, x, y, Number(args.width ?? 60) * 0.9, 12, [
      [0, 'rgba(63, 42, 20, 0.7)'],
      [1, 'rgba(63, 42, 20, 0)'],
    ]);

    ctx.lineCap = 'round';
    const segments = 24;
    for (const vine of state.vines) {
      const grow = easeOut(clamp01((p - vine.delay) / 0.35));
      if (grow <= 0) continue;
      const angle = (s: number) => s * Math.PI * vine.turns + vine.phase;
      const at = (s: number) => ({
        x: x + vine.side * vine.amp * Math.sin(angle(s)) * (1 - 0.35 * s),
        y: y - s * vine.height,
      });

      for (let k = 0; k < segments * grow; k++) {
        const s0 = k / segments;
        const s1 = Math.min((k + 1) / segments, grow);
        const a = at(s0);
        const b = at(s1);
        const behind = Math.cos(angle(s0)) * vine.side < 0;
        ctx.globalAlpha = opacity * (1 - wither) * (behind ? 0.45 : 1);
        ctx.lineWidth = vine.thick * (1 - s0 * 0.8);
        ctx.strokeStyle = color;
        ctx.beginPath();
        ctx.moveTo(a.x, a.y);
        ctx.lineTo(b.x, b.y);
        ctx.stroke();
        // Светлая прожилка сверху
        ctx.lineWidth = Math.max(ctx.lineWidth * 0.3, 0.8);
        ctx.strokeStyle = light;
        ctx.beginPath();
        ctx.moveTo(a.x - 1, a.y);
        ctx.lineTo(b.x - 1, b.y);
        ctx.stroke();
      }

      ctx.globalAlpha = opacity * (1 - wither);
      ctx.fillStyle = color;
      for (const s of THORN_AT) {
        if (s > grow) continue;
        const point = at(s);
        const out = vine.side * (Math.cos(angle(s)) >= 0 ? 1 : -1);
        const size = vine.thick * (1 - s * 0.6);
        ctx.beginPath();
        ctx.moveTo(point.x + out * size * 1.6, point.y - size * 0.6);
        ctx.lineTo(point.x, point.y - size * 0.5);
        ctx.lineTo(point.x, point.y + size * 0.5);
        ctx.closePath();
        ctx.fill();
      }

      ctx.fillStyle = leaf;
      for (const s of LEAF_AT) {
        if (s > grow) continue;
        const point = at(s);
        ctx.save();
        ctx.translate(point.x, point.y);
        ctx.rotate(vine.side * (0.7 + s * 0.4));
        ctx.beginPath();
        ctx.ellipse(vine.side * 5, 0, 6 * (1 - wither * 0.5), 2.6, 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();
      }
    }

    ctx.globalAlpha = opacity;
    for (const d of state.dirt) {
      ctx.globalAlpha = opacity * clamp01(d.life * 1.5);
      ctx.fillStyle = d.color;
      ctx.fillRect(d.x - d.size / 2, d.y - d.size / 2, d.size, d.size);
    }

    ctx.restore();
  }
}

interface DrainOrb {
  t: number;
  speed: number;
  strand: number;
  size: number;
}

/**
 * Похищение жизни: от кастера (start) к цели (end) тянется извивающаяся лента из нескольких прядей,
 * по ней к кастеру текут алые сгустки жизни.
 * args: color, glow, core, amplitude, rate, strands (сколько прядей)
 */
export class DrainStrategy extends ParticleStore<DrainOrb[]> {
  // Точка ленты: t = 0 у кастера, t = 1 у цели. Волна бежит к кастеру; strand — доля сдвига фазы
  // между прядями (0..1 — пряди равномерно по кругу), сгустки берут промежуточные значения
  private at(emitter: EmitterConfig, t: number, strand: number, time: number) {
    const { sx, sy, ex, ey } = endpoints(emitter);
    const dx = ex - sx;
    const dy = ey - sy;
    const length = Math.hypot(dx, dy) || 1;
    const amp = Number(emitter.args?.amplitude ?? 14) * Math.sin(Math.PI * t);
    const wave = Math.sin(t * Math.PI * 3 + time / 90 + strand * Math.PI * 2) * amp;
    return { x: sx + dx * t - (dy / length) * wave, y: sy + dy * t + (dx / length) * wave };
  }

  update(emitter: EmitterConfig) {
    const orbs = this.store.get(emitter.id) ?? [];
    const p = progressOf(emitter);
    if (p > 0.15 && p < 0.85 && Math.random() < Number(emitter.args?.rate ?? 0.7)) {
      orbs.push({ t: 1, speed: rand(0.018, 0.03), strand: Math.random(), size: rand(2.5, 4.5) });
    }
    for (const orb of orbs) orb.t -= orb.speed;
    this.store.set(emitter.id, orbs.filter(o => o.t > 0));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const color = args.color ?? '#e11d48';
    const glow = args.glow ?? '#be185d';
    const core = args.core ?? '#fecdd3';
    const p = progressOf(emitter);
    const reach = easeOut(clamp01(p / 0.2));
    const fade = clamp01((1 - p) / 0.2);
    const time = performance.now();
    const { sx, sy, ex, ey } = endpoints(emitter);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    ctx.lineJoin = 'round';
    ctx.lineCap = 'round';

    // Кровавый водоворот на цели и тёмное поглощение у кастера
    ctx.globalAlpha = opacity * fade * reach;
    fillEllipseGradient(ctx, ex, ey, 34, 40, [
      [0, rgba(color, 0.5)],
      [1, rgba(glow, 0)],
    ]);
    fillEllipseGradient(ctx, sx, sy, 26, 30, [
      [0, rgba(core, 0.35)],
      [1, rgba(color, 0)],
    ]);

    const strands = Number(args.strands ?? 2);
    for (let s = 0; s < strands; s++) {
      const strand = s / strands;
      for (const [width, style, alpha] of [[5, glow, 0.6], [1.8, core, 0.9]] as const) {
        ctx.globalAlpha = opacity * fade * alpha;
        ctx.strokeStyle = style;
        ctx.lineWidth = width;
        ctx.shadowColor = color;
        ctx.shadowBlur = width * 2;
        ctx.beginPath();
        for (let i = 0; i <= 30; i++) {
          const point = this.at(emitter, (i / 30) * reach, strand, time);
          if (i === 0) ctx.moveTo(point.x, point.y);
          else ctx.lineTo(point.x, point.y);
        }
        ctx.stroke();
      }
    }
    ctx.shadowBlur = 0;

    for (const orb of this.store.get(emitter.id) ?? []) {
      const point = this.at(emitter, orb.t, orb.strand, time);
      const halo = orb.size * 3.5;
      ctx.globalAlpha = opacity * fade;
      ctx.drawImage(getGlowSprite(color), point.x - halo, point.y - halo, halo * 2, halo * 2);
      ctx.fillStyle = core;
      ctx.beginPath();
      ctx.arc(point.x, point.y, orb.size * 0.6, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.restore();
  }
}

/**
 * Порча: над головой цели проступает печать с глазом, вращается и обрушивается на неё.
 * args: color, accent, radius, lift (на сколько выше эмиттера висит печать), drop (на сколько падает), slam (доля жизни начала падения),
 *       ornate (второе кольцо рун снаружи)
 */
export class SigilStrategy extends ParticleStore {
  private pose(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const p = progressOf(emitter);
    const lift = Number(args.lift ?? 40);
    const slam = Number(args.slam ?? 0.65);
    const appear = easeOut(clamp01(p / 0.25));
    const fall = clamp01((p - slam) / 0.12);
    return {
      x: emitter.x,
      y: emitter.y - lift + fall * fall * (lift + Number(args.drop ?? 70)),
      scale: (0.4 + 0.6 * appear) * (1 - 0.4 * fall),
      alpha: p > slam + 0.12 ? 0 : appear,
      spin: performance.now() / 700 + (1 - appear) * 2,
      falling: fall > 0,
    };
  }

  update(emitter: EmitterConfig) {
    const drips = this.store.get(emitter.id) ?? [];
    const pose = this.pose(emitter);
    const radius = Number(emitter.args?.radius ?? 30);
    // С печати стекают капли тьмы
    if (pose.alpha > 0.5 && !pose.falling && Math.random() < 0.35) {
      drips.push({
        x: pose.x + rand(-radius * 0.6, radius * 0.6),
        y: pose.y + radius * 0.5,
        vx: 0,
        vy: rand(0.3, 0.8),
        life: 1,
        maxLife: rand(0.02, 0.035),
        size: rand(1.5, 3),
        color: emitter.args?.color ?? '#a855f7',
      });
    }
    this.store.set(emitter.id, stepParticles(drips, 1, 0.04));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const color = args.color ?? '#a855f7';
    const accent = args.accent ?? '#f0abfc';
    const r = Number(args.radius ?? 30);
    const pose = this.pose(emitter);

    ctx.save();
    for (const d of this.store.get(emitter.id) ?? []) {
      ctx.globalAlpha = opacity * clamp01(d.life);
      ctx.fillStyle = d.color;
      ctx.beginPath();
      ctx.ellipse(d.x, d.y, d.size * 0.6, d.size, 0, 0, Math.PI * 2);
      ctx.fill();
    }

    if (pose.alpha > 0) {
      ctx.translate(pose.x, pose.y);
      ctx.scale(pose.scale, pose.scale);

      // Тёмная подложка — печать читается на любом фоне
      ctx.globalAlpha = opacity * pose.alpha;
      const backing = ctx.createRadialGradient(0, 0, 0, 0, 0, r * 1.3);
      backing.addColorStop(0, 'rgba(30, 5, 56, 0.6)');
      backing.addColorStop(1, 'rgba(30, 5, 56, 0)');
      ctx.fillStyle = backing;
      ctx.beginPath();
      ctx.arc(0, 0, r * 1.3, 0, Math.PI * 2);
      ctx.fill();

      ctx.globalCompositeOperation = 'lighter';
      ctx.strokeStyle = color;
      ctx.shadowColor = color;
      ctx.shadowBlur = 14;
      ctx.lineWidth = 2.5;
      ctx.beginPath();
      ctx.arc(0, 0, r, 0, Math.PI * 2);
      ctx.stroke();
      ctx.lineWidth = 1.2;
      ctx.beginPath();
      ctx.arc(0, 0, r * 0.84, 0, Math.PI * 2);
      ctx.stroke();

      // Гексаграмма и засечки вращаются, глаз в центре смотрит прямо
      ctx.save();
      ctx.rotate(pose.spin);
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
      ctx.restore();

      // Внешнее кольцо рун вращается навстречу гексаграмме
      if (args.ornate) {
        ctx.save();
        ctx.rotate(-pose.spin * 1.4);
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
        ctx.restore();
      }

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
    }

    ctx.restore();
  }
}

/**
 * Сияющая вспышка: от кастера расходится кольцо света — светлая оболочка, лучи и волна по земле.
 * Радиус растёт линейно до |endX - startX| за долю жизни travel: BattleFx по этому же закону считает, когда задело цель.
 * args: colors[] (искры на фронте), color, accent, travel, groundOffset, sparks (искр за кадр), echo (вторая волна следом)
 */
export class NovaStrategy extends ParticleStore {
  private radius(emitter: EmitterConfig) {
    const { sx, ex } = endpoints(emitter);
    const travel = Number(emitter.args?.travel ?? 0.7);
    return (Math.abs(ex - sx) || 200) * clamp01(progressOf(emitter) / travel);
  }

  update(emitter: EmitterConfig) {
    const sparks = this.store.get(emitter.id) ?? [];
    const { sx, sy } = endpoints(emitter);
    const palette: string[] = emitter.args?.colors ?? PRISM;
    const r = this.radius(emitter);
    if (progressOf(emitter) < Number(emitter.args?.travel ?? 0.7)) {
      for (let i = 0; i < Number(emitter.args?.sparks ?? 3); i++) {
        const a = Math.random() * Math.PI * 2;
        sparks.push({
          x: sx + Math.cos(a) * r,
          y: sy + Math.sin(a) * r * 0.62,
          vx: Math.cos(a) * 0.4,
          vy: Math.sin(a) * 0.3 - 0.3,
          life: 1,
          maxLife: rand(0.03, 0.05),
          size: rand(2, 4),
          color: palette[Math.floor(Math.random() * palette.length)],
          rotation: Math.random() * Math.PI,
        });
      }
    }
    this.store.set(emitter.id, stepParticles(sparks, 0.98));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const color = args.color ?? '#fde68a';
    const accent = args.accent ?? '#ffffff';
    const travel = Number(args.travel ?? 0.7);
    const p = progressOf(emitter);
    const r = this.radius(emitter);
    const fade = p < travel ? 1 : 1 - (p - travel) / (1 - travel);
    const { sx, sy } = endpoints(emitter);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    // Вспышка на кастере
    if (p < 0.25) {
      const k = 1 - p / 0.25;
      ctx.globalAlpha = opacity * k;
      fillEllipseGradient(ctx, sx, sy, 90, 110, [
        [0, rgba(accent, 0.8)],
        [0.4, rgba(color, 0.4)],
        [1, rgba(color, 0)],
      ]);
    }

    // Лучи из центра
    if (r > 1) {
      ctx.save();
      ctx.translate(sx, sy);
      ctx.rotate(performance.now() / 3000);
      const rays = ctx.createRadialGradient(0, 0, 0, 0, 0, r);
      rays.addColorStop(0, rgba(accent, 0.45));
      rays.addColorStop(1, rgba(color, 0));
      ctx.globalAlpha = opacity * fade;
      ctx.fillStyle = rays;
      for (let i = 0; i < 12; i++) {
        ctx.rotate(Math.PI / 6);
        ctx.beginPath();
        ctx.moveTo(0, 0);
        ctx.lineTo(r, -r * 0.05);
        ctx.lineTo(r, r * 0.05);
        ctx.closePath();
        ctx.fill();
      }
      ctx.restore();
    }

    // Светлая оболочка и волна по земле; с echo за ней идёт вторая, слабее
    const shells: [number, number][] = args.echo ? [[1, 1], [0.72, 0.55]] : [[1, 1]];
    for (const [scale, strength] of shells) {
      const shell = r * scale;
      ctx.globalAlpha = opacity * fade * strength;
      fillEllipseGradient(ctx, sx, sy, shell, shell * 0.62, [
        [0, rgba(color, 0)],
        [0.78, rgba(color, 0.05)],
        [0.94, rgba(accent, 0.55)],
        [1, rgba(color, 0)],
      ]);
      ctx.save();
      ctx.translate(sx, sy + Number(args.groundOffset ?? 60));
      ctx.scale(1, 0.25);
      ctx.strokeStyle = color;
      ctx.shadowColor = color;
      ctx.shadowBlur = 12;
      ctx.lineWidth = 6 * fade + 1;
      ctx.beginPath();
      ctx.arc(0, 0, Math.max(shell, 1), 0, Math.PI * 2);
      ctx.stroke();
      ctx.restore();
    }

    for (const s of this.store.get(emitter.id) ?? []) {
      ctx.globalAlpha = opacity * clamp01(s.life * 1.3);
      const halo = s.size * 3;
      ctx.drawImage(getGlowSprite(s.color), s.x - halo, s.y - halo, halo * 2, halo * 2);
      ctx.fillStyle = '#ffffff';
      drawStar(ctx, s.x, s.y, s.size * 1.2, s.size * 0.3, 4, s.rotation);
    }

    ctx.restore();
  }
}

/**
 * Столб рассветного света падает на цель сверху: луч раскрывается, у ног разливается сияние,
 * сверху опускаются искорки.
 * args: color, core, width, height (высота столба над точкой эмиттера), rate (вероятность искорки за кадр)
 */
export class LightPillarStrategy extends ParticleStore {
  update(emitter: EmitterConfig) {
    const motes = this.store.get(emitter.id) ?? [];
    const args = emitter.args ?? {};
    const width = Number(args.width ?? 70);
    const height = Number(args.height ?? 320);
    if (progressOf(emitter) < 0.7 && Math.random() < Number(args.rate ?? 0.6)) {
      motes.push({
        x: emitter.x + rand(-width * 0.35, width * 0.35),
        y: emitter.y - rand(height * 0.3, height),
        vx: 0,
        vy: rand(0.6, 1.4),
        life: 1,
        maxLife: rand(0.012, 0.02),
        size: rand(1.5, 3),
        color: args.core ?? '#fffbeb',
      });
    }
    this.store.set(emitter.id, stepParticles(motes));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const color = args.color ?? '#ffd166';
    const core = args.core ?? '#fffbeb';
    const height = Number(args.height ?? 320);
    const p = progressOf(emitter);
    const open = easeOut(clamp01(p / 0.2));
    const fade = p > 0.6 ? 1 - (p - 0.6) / 0.4 : 1;
    const width = Number(args.width ?? 70) * (0.25 + 0.75 * open);
    const { x, y } = emitter;

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    // Столб нарезан полосами: к земле ярче, вверху растворяется
    const beam = ctx.createLinearGradient(x - width / 2, 0, x + width / 2, 0);
    beam.addColorStop(0, rgba(color, 0));
    beam.addColorStop(0.5, rgba(core, 0.5));
    beam.addColorStop(1, rgba(color, 0));
    ctx.fillStyle = beam;
    const slices = 8;
    for (let i = 0; i < slices; i++) {
      ctx.globalAlpha = opacity * fade * ((i + 1) / slices);
      ctx.fillRect(x - width / 2, y - height + (height * i) / slices, width, height / slices + 1);
    }

    ctx.globalAlpha = opacity * fade;
    fillEllipseGradient(ctx, x, y, width * 1.1, width * 0.3, [
      [0, rgba(core, 0.8)],
      [0.5, rgba(color, 0.4)],
      [1, rgba(color, 0)],
    ]);

    for (const m of this.store.get(emitter.id) ?? []) {
      ctx.globalAlpha = opacity * Math.sin(Math.PI * clamp01(m.life));
      const halo = m.size * 3;
      ctx.drawImage(getGlowSprite(color), m.x - halo, m.y - halo, halo * 2, halo * 2);
      ctx.fillStyle = m.color;
      ctx.beginPath();
      ctx.arc(m.x, m.y, m.size * 0.6, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.restore();
  }
}

/**
 * Электрические дуги, потрескивающие по телу цели.
 * args: color, count, width, height
 */
export class ArcsStrategy implements EffectStrategy {
  private arcs = new Map<string, { paths: BoltPath[]; frame: number }>();

  update(emitter: EmitterConfig) {
    const state = this.arcs.get(emitter.id);
    if (state && state.frame++ % 3 !== 0) return;

    const args = emitter.args ?? {};
    const rx = Number(args.width ?? 50) / 2;
    const ry = Number(args.height ?? 80) / 2;
    const paths = Array.from({ length: Number(args.count ?? 3) }, () => {
      const a = Math.random() * Math.PI * 2;
      const b = a + rand(0.6, 1.6) * (Math.random() < 0.5 ? -1 : 1);
      return jaggedPath(
        emitter.x + Math.cos(a) * rx, emitter.y + Math.sin(a) * ry,
        emitter.x + Math.cos(b) * rx, emitter.y + Math.sin(b) * ry,
        5, 7,
      );
    });
    this.arcs.set(emitter.id, { paths, frame: (state?.frame ?? 0) + 1 });
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const state = this.arcs.get(emitter.id);
    if (!state) return;
    const color = emitter.args?.color ?? '#9be7ff';
    const flicker = Math.random() > 0.15 ? 1 : 0.3;

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    ctx.globalAlpha = (emitter.opacity ?? 1) * (1 - progressOf(emitter)) * flicker;
    ctx.lineJoin = 'round';
    for (const [width, style] of [[3, color], [1, '#ffffff']] as const) {
      ctx.lineWidth = width;
      ctx.strokeStyle = style;
      ctx.shadowColor = color;
      ctx.shadowBlur = 8;
      for (const path of state.paths) {
        ctx.beginPath();
        path.forEach((point, i) => (i === 0 ? ctx.moveTo(point.x, point.y) : ctx.lineTo(point.x, point.y)));
        ctx.stroke();
      }
    }
    ctx.restore();
  }

  disposeEmitter(id: string) {
    this.arcs.delete(id);
  }
}

/**
 * Метеор: раскалённое ядро с длинным огненным хвостом падает с ускорением от start к end,
 * земля под точкой падения разгорается его светом, с хвоста сыплются искры.
 * args: colors[] (искры), core, glow, size, tail (длина хвоста, px)
 */
export class CometStrategy extends ParticleStore {
  private head(emitter: EmitterConfig) {
    const { sx, sy, ex, ey } = endpoints(emitter);
    const raw = progressOf(emitter);
    // Ускоряется к земле
    const t = raw * raw * 0.6 + raw * 0.4;
    return { x: sx + (ex - sx) * t, y: sy + (ey - sy) * t, raw, angle: Math.atan2(ey - sy, ex - sx), ex, ey };
  }

  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const sparks = this.store.get(emitter.id) ?? [];
    const palette: string[] = args.colors ?? FIRE;
    const size = Number(args.size ?? 13);
    const head = this.head(emitter);

    if (head.raw < 1) {
      for (let i = 0; i < 4; i++) {
        const back = head.angle + Math.PI + rand(-0.35, 0.35);
        const v = rand(1, 3.5);
        sparks.push({
          x: head.x + rand(-size * 0.5, size * 0.5),
          y: head.y + rand(-size * 0.5, size * 0.5),
          vx: Math.cos(back) * v,
          vy: Math.sin(back) * v,
          life: 1,
          maxLife: rand(0.03, 0.06),
          size: rand(size * 0.25, size * 0.6),
          color: palette[0],
        });
      }
    }

    this.store.set(emitter.id, stepParticles(sparks, 0.95, -0.03, 0.96));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const palette: string[] = args.colors ?? FIRE;
    const size = Number(args.size ?? 13);
    const core = args.core ?? '#fffbe0';
    const glow = args.glow ?? '#ff6a00';
    const head = this.head(emitter);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    // Отсвет на земле: чем ближе метеор, тем ярче и шире
    if (head.raw < 1) {
      const spot = 25 + 55 * head.raw;
      ctx.globalAlpha = opacity;
      fillEllipseGradient(ctx, head.ex, head.ey, spot, spot * 0.3, [
        [0, rgba('#ffb347', 0.7 * head.raw)],
        [0.5, rgba('#ff4d00', 0.35 * head.raw)],
        [1, rgba('#ff4d00', 0)],
      ]);
    }

    for (const s of this.store.get(emitter.id) ?? []) {
      ctx.globalAlpha = opacity * clamp01(s.life * 1.4) * 0.85;
      ctx.fillStyle = paletteColor(palette, s.life);
      ctx.beginPath();
      ctx.arc(s.x, s.y, s.size, 0, Math.PI * 2);
      ctx.fill();
    }

    if (head.raw < 1) {
      // Хвост — клин от ядра назад по траектории: снаружи оранжевый, внутри почти белый. Отрастает в начале полёта
      const length = Number(args.tail ?? 150) * Math.min(1, 0.3 + head.raw * 2);
      const flicker = 1 + Math.sin(performance.now() / 30) * 0.08;
      ctx.translate(head.x, head.y);
      ctx.rotate(head.angle);
      ctx.globalAlpha = opacity;

      const layers: [number, number, [number, string][]][] = [
        [size * 1.7, length, [[0, rgba(glow, 0.9)], [0.4, rgba('#e2401b', 0.5)], [1, rgba('#8a1f0f', 0)]]],
        [size * 0.8, length * 0.6, [[0, rgba(core, 1)], [0.5, rgba('#ffb347', 0.6)], [1, rgba('#ff7a1a', 0)]]],
      ];
      for (const [width, tail, stops] of layers) {
        const w = width * flicker;
        const gradient = ctx.createLinearGradient(w, 0, -tail, 0);
        stops.forEach(([offset, color]) => gradient.addColorStop(offset, color));
        ctx.fillStyle = gradient;
        ctx.beginPath();
        ctx.moveTo(0, -w);
        ctx.arc(0, 0, w, -Math.PI / 2, Math.PI / 2);
        ctx.quadraticCurveTo(-tail * 0.3, w * 0.9, -tail, 0);
        ctx.quadraticCurveTo(-tail * 0.3, -w * 0.9, 0, -w);
        ctx.fill();
      }

      // Ядро с ореолом
      const halo = ctx.createRadialGradient(0, 0, 0, 0, 0, size * 3.2);
      halo.addColorStop(0, rgba(glow, 0.7));
      halo.addColorStop(1, rgba(glow, 0));
      ctx.fillStyle = halo;
      ctx.beginPath();
      ctx.arc(0, 0, size * 3.2, 0, Math.PI * 2);
      ctx.fill();

      const ball = ctx.createRadialGradient(0, 0, 0, 0, 0, size);
      ball.addColorStop(0, '#ffffff');
      ball.addColorStop(0.45, core);
      ball.addColorStop(1, glow);
      ctx.fillStyle = ball;
      ctx.beginPath();
      ctx.arc(0, 0, size * flicker, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.restore();
  }
}
