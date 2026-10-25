import { EffectStrategy, EmitterConfig, Particle } from './interfaces/canvas-interfaces';

// Боевые эффекты арены. Время жизни one-shot эффекта задаётся travelDuration эмиттера,
// у постоянных аур (follow) оно не используется.

const FIRE_PALETTE = ['#fffbe0', '#ffe27a', '#ffb347', '#ff7a1a', '#e2401b', '#8a1f0f'];

const rand = (min: number, max: number) => min + Math.random() * (max - min);
const clamp01 = (value: number) => Math.min(Math.max(value, 0), 1);
const easeOut = (t: number) => 1 - (1 - t) * (1 - t);

// Прогресс one-shot эффекта 0..1
const progressOf = (emitter: EmitterConfig) => {
  const started = emitter.travelStartedAt ?? performance.now();
  return clamp01((performance.now() - started) / Math.max(emitter.travelDuration ?? 1, 1));
};

// Цвет частицы по оставшейся жизни: свежая — первый цвет палитры, угасающая — последний
const paletteColor = (palette: string[], life: number) =>
  palette[Math.min(palette.length - 1, Math.floor((1 - clamp01(life)) * palette.length))];

const stepParticles = (particles: Particle[], drag = 1, gravity = 0, shrink = 1) => {
  for (const p of particles) {
    p.x += p.vx;
    p.y += p.vy;
    p.vx *= drag;
    p.vy = p.vy * drag + gravity;
    p.size *= shrink;
    p.life -= p.maxLife;
    if (p.spin) p.rotation = (p.rotation ?? 0) + p.spin;
  }
  return particles.filter(p => p.life > 0 && p.size > 0.3);
};

const drawStar = (ctx: CanvasRenderingContext2D, x: number, y: number, outer: number, inner: number, points: number, rotation = 0) => {
  ctx.beginPath();
  for (let i = 0; i < points * 2; i++) {
    const r = i % 2 === 0 ? outer : inner;
    const a = rotation + (i * Math.PI) / points - Math.PI / 2;
    const px = x + Math.cos(a) * r;
    const py = y + Math.sin(a) * r;
    if (i === 0) ctx.moveTo(px, py);
    else ctx.lineTo(px, py);
  }
  ctx.closePath();
  ctx.fill();
};

// Базовый класс для эффектов, хранящих частицы по id эмиттера
abstract class ParticleStore<T = Particle[]> implements EffectStrategy {
  protected store = new Map<string, T>();
  abstract update(emitter: EmitterConfig): void;
  abstract draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig): void;

  disposeEmitter(id: string) {
    this.store.delete(id);
  }
}

/**
 * Снаряд от startX/startY к endX/endY: светящееся ядро и шлейф.
 * args: core, glow, trail[], size, arc (высота дуги), shape: 'orb' | 'shard', trailRate, rise
 */
export class ProjectileStrategy extends ParticleStore<{ trail: Particle[]; angle: number; t: number }> {
  update(emitter: EmitterConfig) {
    const state = this.store.get(emitter.id) ?? { trail: [], angle: 0, t: 0 };
    const args = emitter.args ?? {};
    const sx = emitter.startX ?? emitter.x;
    const sy = emitter.startY ?? emitter.y;
    const ex = emitter.endX ?? emitter.x;
    const ey = emitter.endY ?? emitter.y;
    const size = Number(args.size ?? 12);
    const arc = Number(args.arc ?? 0);

    // Траектория с дугой и небольшим ускорением к цели
    const at = (raw: number) => {
      const t = raw * (0.6 + 0.4 * raw);
      return {
        x: sx + (ex - sx) * t,
        y: sy + (ey - sy) * t - Math.sin(Math.PI * t) * arc,
      };
    };

    const raw = progressOf(emitter);
    const { x, y } = at(raw);
    const ahead = at(Math.min(raw + 0.02, 1));
    if (ahead.x !== x || ahead.y !== y) state.angle = Math.atan2(ahead.y - y, ahead.x - x);
    emitter.x = x;
    emitter.y = y;
    state.t = raw;

    if (raw < 1) {
      const palette: string[] = args.trail ?? FIRE_PALETTE;
      const rate = Number(args.trailRate ?? 3);
      const back = state.angle + Math.PI;
      for (let i = 0; i < rate; i++) {
        const spread = rand(-0.5, 0.5);
        const speed = rand(0.6, 2);
        state.trail.push({
          x: x + rand(-size * 0.4, size * 0.4),
          y: y + rand(-size * 0.4, size * 0.4),
          vx: Math.cos(back + spread) * speed,
          vy: Math.sin(back + spread) * speed,
          life: 1,
          maxLife: rand(0.035, 0.07),
          size: rand(size * 0.35, size * 0.8),
          color: palette[0],
        });
      }
    }

    state.trail = stepParticles(state.trail, 0.95, -Number(args.rise ?? 0.05), 0.96);
    this.store.set(emitter.id, state);
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const state = this.store.get(emitter.id);
    if (!state) return;
    const args = emitter.args ?? {};
    const palette: string[] = args.trail ?? FIRE_PALETTE;
    const opacity = emitter.opacity ?? 1;
    const size = Number(args.size ?? 12);
    const glow = args.glow ?? '#ff7a1a';
    const core = args.core ?? '#fff3b0';

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    for (const p of state.trail) {
      ctx.globalAlpha = opacity * p.life * 0.85;
      ctx.fillStyle = paletteColor(palette, p.life);
      ctx.beginPath();
      ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
      ctx.fill();
    }

    if (state.t < 1) {
      const flicker = 1 + Math.sin(performance.now() / 35) * 0.12;
      const r = size * flicker;

      // Ореол
      ctx.globalAlpha = opacity;
      const halo = ctx.createRadialGradient(emitter.x, emitter.y, 0, emitter.x, emitter.y, r * 3);
      halo.addColorStop(0, glow);
      halo.addColorStop(1, 'rgba(0, 0, 0, 0)');
      ctx.fillStyle = halo;
      ctx.beginPath();
      ctx.arc(emitter.x, emitter.y, r * 3, 0, Math.PI * 2);
      ctx.fill();

      ctx.translate(emitter.x, emitter.y);
      ctx.shadowColor = glow;
      ctx.shadowBlur = r * 1.5;

      if (args.shape === 'shard') {
        ctx.rotate(state.angle);
        const body = ctx.createLinearGradient(-r * 2.2, 0, r * 1.6, 0);
        body.addColorStop(0, 'rgba(255, 255, 255, 0)');
        body.addColorStop(0.5, glow);
        body.addColorStop(1, core);
        ctx.fillStyle = body;
        ctx.beginPath();
        ctx.moveTo(r * 1.8, 0);
        ctx.lineTo(0, r * 0.55);
        ctx.lineTo(-r * 2.4, 0);
        ctx.lineTo(0, -r * 0.55);
        ctx.closePath();
        ctx.fill();
      } else {
        const ball = ctx.createRadialGradient(0, 0, 0, 0, 0, r);
        ball.addColorStop(0, '#ffffff');
        ball.addColorStop(0.35, core);
        ball.addColorStop(1, glow);
        ctx.fillStyle = ball;
        ctx.beginPath();
        ctx.arc(0, 0, r, 0, Math.PI * 2);
        ctx.fill();
      }
    }

    ctx.restore();
  }
}

/**
 * Взрыв: вспышка, ударная волна, разлёт частиц и дым.
 * args: colors[], count, radius, speed, ring (цвет или false), flash, smoke, shards, gravity
 */
export class ExplosionStrategy extends ParticleStore<{ sparks: Particle[]; smoke: Particle[] }> {
  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    let state = this.store.get(emitter.id);

    if (!state) {
      const palette: string[] = args.colors ?? FIRE_PALETTE;
      const count = Number(args.count ?? 36);
      const speed = Number(args.speed ?? 5);
      const shards = !!args.shards;

      const sparks = Array.from({ length: count }, () => {
        const angle = Math.random() * Math.PI * 2;
        const v = rand(0.25, 1) * speed;
        return {
          x: emitter.x,
          y: emitter.y,
          vx: Math.cos(angle) * v,
          vy: Math.sin(angle) * v,
          life: 1,
          maxLife: rand(0.02, 0.045),
          size: shards ? rand(4, 9) : rand(2.5, 7),
          color: palette[0],
          rotation: angle,
          spin: shards ? rand(-0.2, 0.2) : 0,
        };
      });

      const smoke = args.smoke
        ? Array.from({ length: Math.round(count / 3) }, () => ({
          x: emitter.x + rand(-12, 12),
          y: emitter.y + rand(-12, 12),
          vx: rand(-0.6, 0.6),
          vy: rand(-1.2, -0.3),
          life: 1,
          maxLife: rand(0.012, 0.022),
          size: rand(10, 18),
          color: args.smokeColor ?? 'rgba(40, 32, 30, 1)',
        }))
        : [];

      state = { sparks, smoke };
    }

    state.sparks = stepParticles(state.sparks, 0.92, Number(args.gravity ?? -0.04), 0.985);
    state.smoke = stepParticles(state.smoke, 0.97, -0.01, 1.012);
    this.store.set(emitter.id, state);
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const state = this.store.get(emitter.id);
    if (!state) return;
    const args = emitter.args ?? {};
    const palette: string[] = args.colors ?? FIRE_PALETTE;
    const opacity = emitter.opacity ?? 1;
    const radius = Number(args.radius ?? emitter.padding ?? 60);
    const p = progressOf(emitter);

    ctx.save();

    // Дым рисуем обычным смешиванием, чтобы он затемнял фон
    for (const s of state.smoke) {
      ctx.globalAlpha = opacity * s.life * 0.35;
      ctx.fillStyle = s.color;
      ctx.beginPath();
      ctx.arc(s.x, s.y, s.size, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.globalCompositeOperation = 'lighter';

    // Вспышка в первой четверти
    if (p < 0.3) {
      const k = 1 - p / 0.3;
      const flashRadius = radius * (0.5 + p * 1.5);
      const flash = ctx.createRadialGradient(emitter.x, emitter.y, 0, emitter.x, emitter.y, flashRadius);
      flash.addColorStop(0, '#ffffff');
      flash.addColorStop(0.3, args.flash ?? palette[1] ?? palette[0]);
      flash.addColorStop(1, 'rgba(0, 0, 0, 0)');
      ctx.globalAlpha = opacity * k;
      ctx.fillStyle = flash;
      ctx.beginPath();
      ctx.arc(emitter.x, emitter.y, flashRadius, 0, Math.PI * 2);
      ctx.fill();
    }

    // Ударная волна
    if (args.ring !== false) {
      const ringP = easeOut(Math.min(p / 0.6, 1));
      ctx.globalAlpha = opacity * (1 - ringP);
      ctx.strokeStyle = args.ring ?? palette[2] ?? palette[0];
      ctx.lineWidth = 7 * (1 - ringP) + 1;
      ctx.beginPath();
      ctx.arc(emitter.x, emitter.y, radius * (0.2 + ringP), 0, Math.PI * 2);
      ctx.stroke();
    }

    for (const s of state.sparks) {
      ctx.globalAlpha = opacity * clamp01(s.life * 1.3);
      ctx.fillStyle = paletteColor(palette, s.life);
      if (args.shards) {
        ctx.save();
        ctx.translate(s.x, s.y);
        ctx.rotate(s.rotation ?? 0);
        ctx.beginPath();
        ctx.moveTo(s.size, 0);
        ctx.lineTo(-s.size * 0.6, s.size * 0.35);
        ctx.lineTo(-s.size * 0.6, -s.size * 0.35);
        ctx.closePath();
        ctx.fill();
        ctx.restore();
      } else {
        ctx.beginPath();
        ctx.arc(s.x, s.y, s.size, 0, Math.PI * 2);
        ctx.fill();
      }
    }

    ctx.restore();
  }
}

/**
 * Росчерк удара оружием по цели.
 * args: color, direction (1 — удар слева направо, -1 — справа налево), radius, width, count (2 — крест для крита)
 */
export class SlashStrategy implements EffectStrategy {
  update() {
    // Рисование целиком зависит от времени
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const color = args.color ?? '#ffd166';
    const direction = Number(args.direction ?? 1);
    const radius = Number(args.radius ?? 55);
    const width = Number(args.width ?? 14);
    const count = Number(args.count ?? 1);
    const p = progressOf(emitter);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    ctx.lineCap = 'round';
    ctx.translate(emitter.x, emitter.y);
    ctx.scale(direction, 1);

    for (let n = 0; n < count; n++) {
      const local = clamp01((p - n * 0.15) / (1 - n * 0.15));
      if (local <= 0) continue;
      const sweep = easeOut(Math.min(local / 0.4, 1));
      const fade = local < 0.4 ? 1 : 1 - (local - 0.4) / 0.6;

      ctx.save();
      ctx.rotate(n === 0 ? -0.45 : 0.55);
      const start = -Math.PI * 0.95;
      const end = -Math.PI * 0.05;
      const current = start + (end - start) * sweep;
      const segments = 18;

      for (const [lineScale, style, blur] of [[1, color, 16], [0.45, '#ffffff', 4]] as const) {
        ctx.strokeStyle = style;
        ctx.shadowColor = color;
        ctx.shadowBlur = blur;
        ctx.globalAlpha = (emitter.opacity ?? 1) * fade;
        for (let i = 0; i < segments; i++) {
          const a0 = start + ((current - start) * i) / segments;
          const a1 = start + ((current - start) * (i + 1)) / segments;
          // Сужение к концам росчерка
          ctx.lineWidth = Math.max(width * lineScale * Math.sin((Math.PI * (i + 0.5)) / segments), 0.5);
          ctx.beginPath();
          ctx.arc(0, radius * 0.35, radius, a0, a1);
          ctx.stroke();
        }
      }
      ctx.restore();
    }

    ctx.restore();
  }
}

/**
 * Пламя на юните (аура горения или короткая вспышка огня при тике).
 * args: width, height, rate, size, colors[], burst (спавн только в первой половине жизни), glow
 */
export class FlamesStrategy extends ParticleStore {
  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const particles = this.store.get(emitter.id) ?? [];
    const width = Number(args.width ?? 50);
    const height = Number(args.height ?? 70);
    const size = Number(args.size ?? 8);
    const palette: string[] = args.colors ?? FIRE_PALETTE;

    const spawning = !args.burst || progressOf(emitter) < 0.5;
    if (spawning) {
      const rate = Number(args.rate ?? 2);
      const count = Math.floor(rate) + (Math.random() < rate % 1 ? 1 : 0);
      for (let i = 0; i < count; i++) {
        // Ближе к центру языки пламени гуще
        const offset = (Math.random() + Math.random() - 1) * width * 0.5;
        particles.push({
          x: emitter.x + offset,
          y: emitter.y + rand(-height * 0.15, height * 0.25),
          vx: rand(-0.25, 0.25) - offset * 0.004,
          vy: -rand(0.7, 1.6) * (height / 70),
          life: 1,
          maxLife: rand(0.025, 0.045),
          size: rand(size * 0.6, size * 1.2),
          color: palette[0],
        });
      }
    }

    this.store.set(emitter.id, stepParticles(particles, 0.99, -0.02, 0.975));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const particles = this.store.get(emitter.id);
    if (!particles) return;
    const args = emitter.args ?? {};
    const palette: string[] = args.colors ?? FIRE_PALETTE;
    const opacity = emitter.opacity ?? 1;
    const width = Number(args.width ?? 50);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    // Тёплое мерцающее свечение вокруг
    if (args.glow !== false) {
      const flicker = 0.75 + Math.sin(performance.now() / 90) * 0.15 + Math.random() * 0.1;
      const glow = ctx.createRadialGradient(emitter.x, emitter.y, 0, emitter.x, emitter.y, width * 1.3);
      glow.addColorStop(0, args.glow ?? 'rgba(255, 120, 30, 0.35)');
      glow.addColorStop(1, 'rgba(0, 0, 0, 0)');
      ctx.globalAlpha = opacity * flicker;
      ctx.fillStyle = glow;
      ctx.beginPath();
      ctx.arc(emitter.x, emitter.y, width * 1.3, 0, Math.PI * 2);
      ctx.fill();
    }

    for (const p of particles) {
      ctx.globalAlpha = opacity * clamp01(p.life * 1.4) * 0.8;
      ctx.fillStyle = paletteColor(palette, p.life);
      ctx.beginPath();
      // Капля, вытянутая вверх — похоже на язык пламени
      ctx.ellipse(p.x, p.y, p.size * 0.7, p.size * 1.25, 0, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.restore();
  }
}

// Снежинка со свечением рендерится один раз в offscreen-канвас и дальше рисуется через drawImage:
// shadowBlur на каждом штрихе каждый кадр слишком дорогой и при нескольких замёрзших юнитах роняет FPS
const FLAKE_RADIUS = 10; // в 2 раза больше максимального размера частицы, чтобы не мылилось
const FLAKE_BLUR = 12;
const flakeSprites = new Map<string, HTMLCanvasElement>();

const getFlakeSprite = (color: string) => {
  let sprite = flakeSprites.get(color);
  if (sprite) return sprite;

  const half = FLAKE_RADIUS + FLAKE_BLUR;
  sprite = document.createElement('canvas');
  sprite.width = sprite.height = half * 2;
  const ctx = sprite.getContext('2d')!;
  ctx.translate(half, half);
  ctx.lineCap = 'round';
  ctx.lineWidth = 2.6;
  ctx.strokeStyle = color;
  ctx.shadowColor = '#7dd3fc';
  ctx.shadowBlur = FLAKE_BLUR;
  for (let i = 0; i < 3; i++) {
    ctx.rotate(Math.PI / 3);
    ctx.beginPath();
    ctx.moveTo(-FLAKE_RADIUS, 0);
    ctx.lineTo(FLAKE_RADIUS, 0);
    ctx.stroke();
  }

  flakeSprites.set(color, sprite);
  return sprite;
};

/**
 * Холодная аура: падающие снежинки, блики и морозная дымка.
 * args: width, height, rate, color
 */
export class FrostAuraStrategy extends ParticleStore {
  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const particles = this.store.get(emitter.id) ?? [];
    const width = Number(args.width ?? 70);
    const height = Number(args.height ?? 110);

    if (Math.random() < Number(args.rate ?? 0.35)) {
      particles.push({
        x: emitter.x + rand(-width / 2, width / 2),
        y: emitter.y - height / 2 + rand(0, height * 0.4),
        vx: rand(-0.2, 0.2),
        vy: rand(0.25, 0.6),
        life: 1,
        maxLife: rand(0.008, 0.014),
        size: rand(2.5, 5),
        color: args.color ?? '#d6f3ff',
        rotation: Math.random() * Math.PI,
        spin: rand(-0.03, 0.03),
      });
    }

    for (const p of particles) {
      p.vx += Math.sin(performance.now() / 400 + p.size) * 0.01;
    }
    this.store.set(emitter.id, stepParticles(particles));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const particles = this.store.get(emitter.id);
    if (!particles) return;
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const width = Number(args.width ?? 70);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    const mist = ctx.createRadialGradient(emitter.x, emitter.y, 0, emitter.x, emitter.y, width);
    mist.addColorStop(0, 'rgba(125, 211, 252, 0.18)');
    mist.addColorStop(1, 'rgba(0, 0, 0, 0)');
    ctx.globalAlpha = opacity;
    ctx.fillStyle = mist;
    ctx.beginPath();
    ctx.arc(emitter.x, emitter.y, width, 0, Math.PI * 2);
    ctx.fill();

    for (const p of particles) {
      const sprite = getFlakeSprite(p.color);
      const half = (sprite.width / 2) * (p.size / FLAKE_RADIUS);
      // Плавное появление и исчезновение
      ctx.globalAlpha = opacity * Math.sin(Math.PI * clamp01(p.life));
      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.rotate(p.rotation ?? 0);
      ctx.drawImage(sprite, -half, -half, half * 2, half * 2);
      ctx.restore();
    }

    ctx.restore();
  }
}

/**
 * Звёздочки, кружащие над головой оглушённого.
 * args: count, radiusX, radiusY, color, size, speed
 */
export class StunStarsStrategy implements EffectStrategy {
  update() {
    // Положение звёзд считается от времени
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const count = Number(args.count ?? 3);
    const rx = Number(args.radiusX ?? 28);
    const ry = Number(args.radiusY ?? 9);
    const size = Number(args.size ?? 7);
    const color = args.color ?? '#ffe066';
    const time = performance.now() * Number(args.speed ?? 0.004);
    const opacity = emitter.opacity ?? 1;

    const stars = Array.from({ length: count }, (_, i) => {
      const a = time + (i * Math.PI * 2) / count;
      return { a, depth: (Math.sin(a) + 1) / 2 };
    }).sort((l, r) => l.depth - r.depth);

    ctx.save();
    ctx.globalAlpha = opacity * 0.35;
    ctx.strokeStyle = color;
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.ellipse(emitter.x, emitter.y, rx, ry, 0, 0, Math.PI * 2);
    ctx.stroke();

    ctx.fillStyle = color;
    ctx.shadowColor = color;
    ctx.shadowBlur = 10;
    for (const star of stars) {
      const scale = 0.65 + star.depth * 0.55;
      ctx.globalAlpha = opacity * (0.55 + star.depth * 0.45);
      drawStar(
        ctx,
        emitter.x + Math.cos(star.a) * rx,
        emitter.y + Math.sin(star.a) * ry,
        size * scale,
        size * scale * 0.45,
        5,
        star.a * 1.5,
      );
    }
    ctx.restore();
  }
}

/**
 * Поднимающиеся искры/плюсики (лечение, реген, бафы).
 * args: color, width, height, rate, burst (кол-во частиц сразу), shape: 'plus' | 'dot' | 'star', rise, duration-bound: once
 */
export class SparklesStrategy extends ParticleStore<{ particles: Particle[]; burstDone: boolean }> {
  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const state = this.store.get(emitter.id) ?? { particles: [], burstDone: false };
    const width = Number(args.width ?? 60);
    const height = Number(args.height ?? 80);
    const rise = Number(args.rise ?? 1);
    const color = args.color ?? '#4ade80';

    const spawn = () => state.particles.push({
      x: emitter.x + rand(-width / 2, width / 2),
      y: emitter.y + rand(-height / 2, height / 2),
      vx: rand(-0.2, 0.2),
      vy: -rand(0.5, 1.3) * rise,
      life: 1,
      maxLife: rand(0.014, 0.028),
      size: rand(3, 6),
      color,
    });

    if (!state.burstDone) {
      for (let i = 0; i < Number(args.burst ?? 0); i++) spawn();
      state.burstDone = true;
    }

    const once = args.once === true;
    if (!once || progressOf(emitter) < 0.4) {
      if (Math.random() < Number(args.rate ?? 0.5)) spawn();
    }

    state.particles = stepParticles(state.particles, 0.99);
    this.store.set(emitter.id, state);
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const state = this.store.get(emitter.id);
    if (!state) return;
    const args = emitter.args ?? {};
    const shape = args.shape ?? 'plus';
    const opacity = emitter.opacity ?? 1;

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    for (const p of state.particles) {
      ctx.globalAlpha = opacity * Math.sin(Math.PI * clamp01(p.life));
      ctx.fillStyle = p.color;
      ctx.shadowColor = p.color;
      ctx.shadowBlur = 8;
      if (shape === 'plus') {
        const t = p.size * 0.35;
        ctx.fillRect(p.x - p.size, p.y - t / 2, p.size * 2, t);
        ctx.fillRect(p.x - t / 2, p.y - p.size, t, p.size * 2);
      } else if (shape === 'star') {
        drawStar(ctx, p.x, p.y, p.size, p.size * 0.35, 4);
      } else {
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.size * 0.6, 0, Math.PI * 2);
        ctx.fill();
      }
    }
    ctx.restore();
  }
}

// ===== Ауры героев =====

const rgbCache = new Map<string, string>();

// '#rrggbb' + альфа -> 'rgba(...)' для стопов градиента
const rgba = (hex: string, alpha: number) => {
  let rgb = rgbCache.get(hex);
  if (!rgb) {
    const n = parseInt(hex.slice(1, 7), 16);
    rgb = `${(n >> 16) & 255}, ${(n >> 8) & 255}, ${n & 255}`;
    rgbCache.set(hex, rgb);
  }
  return `rgba(${rgb}, ${clamp01(alpha)})`;
};

// Мягкий ореол частицы. Как и снежинка, рендерится один раз: градиент на каждую частицу каждый кадр дорогой.
const GLOW_RADIUS = 32;
const glowSprites = new Map<string, HTMLCanvasElement>();

const getGlowSprite = (color: string) => {
  let sprite = glowSprites.get(color);
  if (sprite) return sprite;

  sprite = document.createElement('canvas');
  sprite.width = sprite.height = GLOW_RADIUS * 2;
  const ctx = sprite.getContext('2d')!;
  const gradient = ctx.createRadialGradient(GLOW_RADIUS, GLOW_RADIUS, 0, GLOW_RADIUS, GLOW_RADIUS, GLOW_RADIUS);
  gradient.addColorStop(0, rgba(color, 0.9));
  gradient.addColorStop(0.25, rgba(color, 0.45));
  gradient.addColorStop(1, rgba(color, 0));
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, GLOW_RADIUS * 2, GLOW_RADIUS * 2);

  glowSprites.set(color, sprite);
  return sprite;
};

export type AuraMote = 'ember' | 'flake' | 'plus' | 'spark' | 'dot';

// Частица ауры: мягкий ореол + форма по стихии
const drawMote = (ctx: CanvasRenderingContext2D, shape: AuraMote, x: number, y: number, size: number, color: string, accent: string, rotation = 0) => {
  const halo = size * 3.2;
  ctx.drawImage(getGlowSprite(color), x - halo, y - halo, halo * 2, halo * 2);

  ctx.fillStyle = accent;
  switch (shape) {
    case 'ember':
      ctx.beginPath();
      ctx.ellipse(x, y, size * 0.55, size, 0, 0, Math.PI * 2);
      ctx.fill();
      break;
    case 'flake': {
      const sprite = getFlakeSprite(accent);
      const half = (sprite.width / 2) * (size / FLAKE_RADIUS) * 1.4;
      ctx.save();
      ctx.translate(x, y);
      ctx.rotate(rotation);
      ctx.drawImage(sprite, -half, -half, half * 2, half * 2);
      ctx.restore();
      break;
    }
    case 'plus': {
      const t = size * 0.45;
      ctx.fillRect(x - size, y - t / 2, size * 2, t);
      ctx.fillRect(x - t / 2, y - size, t, size * 2);
      break;
    }
    case 'spark':
      drawStar(ctx, x, y, size * 1.3, size * 0.3, 4, rotation);
      break;
    default:
      ctx.beginPath();
      ctx.arc(x, y, size * 0.6, 0, Math.PI * 2);
      ctx.fill();
  }
};

// Заливка эллипса радиальным градиентом: рисуем единичный круг в масштабированной системе координат.
// stops — [смещение 0..1, цвет]
const fillEllipseGradient = (
  ctx: CanvasRenderingContext2D,
  x: number,
  y: number,
  rx: number,
  ry: number,
  stops: [number, string][],
) => {
  if (rx <= 0 || ry <= 0) return;
  ctx.save();
  ctx.translate(x, y);
  ctx.scale(rx, ry);
  const gradient = ctx.createRadialGradient(0, 0, 0, 0, 0, 1);
  for (const [offset, color] of stops) gradient.addColorStop(offset, color);
  ctx.fillStyle = gradient;
  ctx.beginPath();
  ctx.arc(0, 0, 1, 0, Math.PI * 2);
  ctx.fill();
  ctx.restore();
};

// Светящийся обод сферы (эффект Френеля): центр прозрачный, к краю ярче — юнит внутри хорошо видно
const sphereRim = (color: string, accent: string, alpha: number): [number, string][] => [
  [0, rgba(color, 0)],
  [0.62, rgba(color, 0)],
  [0.86, rgba(color, alpha * 0.45)],
  [0.96, rgba(accent, alpha)],
  [1, rgba(color, 0)],
];

// Орбиты частиц вокруг сферы (доли rx/ry). Нижняя половина эллипса — ближе к зрителю, верхняя — за юнитом.
const AURA_ORBITS = [
  { dy: 0.22, rx: 1.12, ry: 0.2, tilt: -0.12, speed: 0.0011, motes: 3 },
  { dy: -0.28, rx: 0.92, ry: 0.16, tilt: 0.2, speed: -0.0016, motes: 2 },
];

/**
 * Аура героя: полупрозрачная пульсирующая сфера вокруг юнита.
 * Рисуется двумя эмиттерами, чтобы юнит оказался «внутри»:
 *   part 'back' (фоновый слой) — круг на земле с рябью, дымка внутри сферы, дальние половины орбит;
 *   part 'front' (передний слой) — обод сферы, блик, волны пульсации, ближние частицы, дрейфующие искры.
 * Анимация считается от времени и seed, поэтому обе половины синхронны.
 * args: color, glow, accent, mote, drift (<0 вверх, >0 вниз), rate, period (сек), rx, ry, groundOffset, seed, part
 */
export class AuraSphereStrategy extends ParticleStore {
  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    if (args.part !== 'front') return;

    const particles = this.store.get(emitter.id) ?? [];
    const rx = Number(args.rx ?? 60);
    const ry = Number(args.ry ?? 80);
    const drift = Number(args.drift ?? -1);

    if (Math.random() < Number(args.rate ?? 0.25)) {
      // Поднимающиеся частицы рождаются в нижней части сферы, падающие — в верхней
      const a = Math.random() * Math.PI * 2;
      const r = Math.sqrt(Math.random()) * 0.8;
      particles.push({
        x: emitter.x + Math.cos(a) * rx * r,
        y: emitter.y + Math.sin(a) * ry * r * 0.5 - drift * ry * 0.4,
        vx: rand(-0.15, 0.15),
        vy: drift * rand(0.35, 0.8),
        life: 1,
        maxLife: rand(0.008, 0.014),
        size: rand(1.8, 3.6),
        color: args.color ?? '#ffd166',
        rotation: Math.random() * Math.PI,
        spin: rand(-0.03, 0.03),
      });
    }

    for (const p of particles) {
      p.vx += Math.sin(performance.now() / 500 + p.size * 3) * 0.008;
    }
    this.store.set(emitter.id, stepParticles(particles, 0.995));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    if (opacity <= 0) return;

    const color: string = args.color ?? '#ffd166';
    const glow: string = args.glow ?? color;
    const accent: string = args.accent ?? '#ffffff';
    const mote: AuraMote = args.mote ?? 'dot';
    const rx = Number(args.rx ?? 60);
    const ry = Number(args.ry ?? 80);
    const period = Number(args.period ?? 2) * 1000;
    const time = performance.now() + Number(args.seed ?? 0);
    // Фаза пульсации 0..1: в начале периода от сферы отходит волна, сама сфера «дышит»
    const phase = (time % period) / period;
    const breath = 1 + Math.sin(phase * Math.PI * 2) * 0.035;
    const front = args.part === 'front';

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    ctx.globalAlpha = opacity;

    if (front) {
      this.drawPulse(ctx, emitter.x, emitter.y, rx, ry, color, accent, phase);
      fillEllipseGradient(ctx, emitter.x, emitter.y, rx * breath, ry * breath, sphereRim(color, accent, 0.42));
      this.drawHighlight(ctx, emitter.x, emitter.y, rx * breath, ry * breath, accent);
    } else {
      this.drawGround(ctx, emitter.x, emitter.y + Number(args.groundOffset ?? ry * 0.75), rx, color, glow, phase, time);
      // Дымка внутри сферы — за юнитом, поэтому не мешает его разглядеть
      fillEllipseGradient(ctx, emitter.x, emitter.y, rx * breath, ry * breath, [
        [0, rgba(glow, 0.1)],
        [0.6, rgba(glow, 0.14)],
        [0.92, rgba(color, 0.28)],
        [1, rgba(color, 0)],
      ]);
    }

    this.drawOrbits(ctx, emitter.x, emitter.y, rx * breath, ry * breath, color, accent, mote, time, front);

    if (front) {
      for (const p of this.store.get(emitter.id) ?? []) {
        ctx.globalAlpha = opacity * Math.sin(Math.PI * clamp01(p.life)) * 0.9;
        drawMote(ctx, mote, p.x, p.y, p.size, p.color, accent, p.rotation);
      }
    }

    ctx.restore();
  }

  // Волна-сфера, отходящая от ауры в начале каждого периода
  private drawPulse(ctx: CanvasRenderingContext2D, x: number, y: number, rx: number, ry: number, color: string, accent: string, phase: number) {
    const wave = clamp01(phase / 0.7);
    if (wave >= 1) return;
    const scale = 1 + easeOut(wave) * 0.45;
    const alpha = Math.pow(1 - wave, 1.6) * 0.55;
    fillEllipseGradient(ctx, x, y, rx * scale, ry * scale, sphereRim(color, accent, alpha));
  }

  // Глянцевый блик слева сверху — именно он делает из свечения «сферу»
  private drawHighlight(ctx: CanvasRenderingContext2D, x: number, y: number, rx: number, ry: number, accent: string) {
    ctx.save();
    ctx.translate(x - rx * 0.42, y - ry * 0.55);
    ctx.rotate(-0.65);
    ctx.scale(rx * 0.34, ry * 0.11);
    const gradient = ctx.createRadialGradient(0, 0, 0, 0, 0, 1);
    gradient.addColorStop(0, rgba(accent, 0.38));
    gradient.addColorStop(1, rgba(accent, 0));
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.arc(0, 0, 1, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // Тонкий отблеск на противоположном краю
    ctx.strokeStyle = rgba(accent, 0.22);
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    ctx.ellipse(x, y, rx * 0.93, ry * 0.93, 0, 0.15, 0.95);
    ctx.stroke();
  }

  // Светящийся круг на земле, вращающиеся руны и рябь от каждой пульсации
  private drawGround(ctx: CanvasRenderingContext2D, x: number, y: number, rx: number, color: string, glow: string, phase: number, time: number) {
    const squash = 0.3;
    const flicker = 0.85 + Math.sin(time / 260) * 0.08;
    fillEllipseGradient(ctx, x, y, rx * 1.35, rx * 1.35 * squash, [
      [0, rgba(glow, 0.5 * flicker)],
      [0.55, rgba(color, 0.22 * flicker)],
      [1, rgba(color, 0)],
    ]);

    ctx.save();
    ctx.translate(x, y);
    ctx.scale(1, squash);
    ctx.strokeStyle = color;
    ctx.lineCap = 'round';

    // Рябь: кольцо расходится по земле вместе с волной сферы
    const ripple = easeOut(clamp01(phase / 0.8));
    ctx.save();
    ctx.globalAlpha *= (1 - ripple) * 0.75;
    ctx.lineWidth = 4 * (1 - ripple) + 1;
    ctx.beginPath();
    ctx.arc(0, 0, rx * (0.55 + ripple * 1.05), 0, Math.PI * 2);
    ctx.stroke();
    ctx.restore();

    // Внешнее кольцо с засечками и пунктирное внутреннее, вращаются в разные стороны
    ctx.save();
    ctx.globalAlpha *= 0.55;
    ctx.rotate(time / 2600);
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(0, 0, rx * 1.02, 0, Math.PI * 2);
    ctx.stroke();
    ctx.lineWidth = 2.5;
    for (let i = 0; i < 12; i++) {
      const a = (i * Math.PI) / 6;
      ctx.beginPath();
      ctx.moveTo(Math.cos(a) * rx * 1.02, Math.sin(a) * rx * 1.02);
      ctx.lineTo(Math.cos(a) * rx * 1.12, Math.sin(a) * rx * 1.12);
      ctx.stroke();
    }
    ctx.rotate(-time / 1300);
    ctx.setLineDash([rx * 0.18, rx * 0.12]);
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    ctx.arc(0, 0, rx * 0.8, 0, Math.PI * 2);
    ctx.stroke();
    ctx.restore();

    ctx.restore();
  }

  // Наклонные орбиты с бегущими по ним частицами; каждая половина рисует свою часть орбиты
  private drawOrbits(
    ctx: CanvasRenderingContext2D,
    x: number,
    y: number,
    rx: number,
    ry: number,
    color: string,
    accent: string,
    mote: AuraMote,
    time: number,
    front: boolean,
  ) {
    const baseAlpha = ctx.globalAlpha;

    for (const orbit of AURA_ORBITS) {
      const ox = x;
      const oy = y + orbit.dy * ry;
      const orx = orbit.rx * rx;
      const ory = orbit.ry * ry;

      // Ближняя половина эллипса — углы 0..π (нижняя дуга), дальняя — π..2π
      ctx.globalAlpha = baseAlpha * (front ? 0.45 : 0.25);
      ctx.strokeStyle = color;
      ctx.lineWidth = front ? 1.6 : 1.2;
      ctx.beginPath();
      ctx.ellipse(ox, oy, orx, ory, orbit.tilt, front ? 0 : Math.PI, front ? Math.PI : Math.PI * 2);
      ctx.stroke();

      for (let i = 0; i < orbit.motes; i++) {
        const a = time * orbit.speed + (i * Math.PI * 2) / orbit.motes;
        const depth = Math.sin(a); // >0 — перед юнитом
        if (front !== depth > 0) continue;

        const px = Math.cos(a) * orx;
        const py = depth * ory;
        const cos = Math.cos(orbit.tilt);
        const sin = Math.sin(orbit.tilt);
        const scale = 0.7 + (depth + 1) * 0.35;
        ctx.globalAlpha = baseAlpha * (0.45 + (depth + 1) * 0.275);
        drawMote(ctx, mote, ox + px * cos - py * sin, oy + px * sin + py * cos, 3.2 * scale, color, accent, a);
      }
    }

    ctx.globalAlpha = baseAlpha;
  }
}

/**
 * Всплеск ауры в момент срабатывания: от носителя расходятся две сферы-волны и разлетаются частицы.
 * args: color, glow, accent, colors[] (палитра частиц), mote, rx, ry, reach (радиус волны)
 */
export class AuraBurstStrategy extends ParticleStore {
  update(emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    let particles = this.store.get(emitter.id);

    if (!particles) {
      const palette: string[] = args.colors ?? [args.accent ?? '#ffffff', args.color ?? '#ffd166'];
      const rx = Number(args.rx ?? 60);
      const ry = Number(args.ry ?? 80);
      particles = Array.from({ length: Number(args.count ?? 26) }, () => {
        const angle = Math.random() * Math.PI * 2;
        const v = rand(2.5, 6);
        return {
          x: emitter.x + Math.cos(angle) * rx * 0.6,
          y: emitter.y + Math.sin(angle) * ry * 0.6,
          vx: Math.cos(angle) * v,
          vy: Math.sin(angle) * v * 0.75,
          life: 1,
          maxLife: rand(0.02, 0.035),
          size: rand(2, 4.5),
          color: palette[Math.floor(Math.random() * palette.length)],
          rotation: angle,
          spin: rand(-0.1, 0.1),
        };
      });
    }

    this.store.set(emitter.id, stepParticles(particles, 0.94, 0, 0.99));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const opacity = emitter.opacity ?? 1;
    const color: string = args.color ?? '#ffd166';
    const glow: string = args.glow ?? color;
    const accent: string = args.accent ?? '#ffffff';
    const rx = Number(args.rx ?? 60);
    const ry = Number(args.ry ?? 80);
    const reach = Number(args.reach ?? 240);
    const p = progressOf(emitter);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';

    // Вспышка внутри сферы
    if (p < 0.35) {
      ctx.globalAlpha = opacity * (1 - p / 0.35);
      fillEllipseGradient(ctx, emitter.x, emitter.y, rx * 1.1, ry * 1.1, [
        [0, rgba(accent, 0.55)],
        [0.5, rgba(glow, 0.35)],
        [1, rgba(color, 0)],
      ]);
    }

    // Две волны: основная и догоняющая
    for (const [delay, strength] of [[0, 1], [0.14, 0.6]] as const) {
      const local = clamp01((p - delay) / (1 - delay));
      if (local <= 0 || local >= 1) continue;
      const grow = easeOut(local);
      const wx = rx + (reach - rx) * grow;
      const wy = wx * (ry / rx) * (1 - grow * 0.25);
      ctx.globalAlpha = opacity;
      fillEllipseGradient(ctx, emitter.x, emitter.y, wx, wy, sphereRim(color, accent, Math.pow(1 - local, 1.3) * 0.7 * strength));
    }

    for (const s of this.store.get(emitter.id) ?? []) {
      ctx.globalAlpha = opacity * clamp01(s.life * 1.4);
      drawMote(ctx, args.mote ?? 'dot', s.x, s.y, s.size, s.color, accent, s.rotation);
    }

    ctx.restore();
  }
}

/**
 * Кольцо на земле: волна ('pulse') или вращающийся магический круг ('rune').
 * args: color, radius, squash (сплющивание эллипса), width, mode
 */
export class RingStrategy implements EffectStrategy {
  update() {
    // Анимация целиком от времени
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const args = emitter.args ?? {};
    const color = args.color ?? '#ffd166';
    const radius = Number(args.radius ?? emitter.padding ?? 50);
    const squash = Number(args.squash ?? 0.35);
    const width = Number(args.width ?? 4);
    const opacity = emitter.opacity ?? 1;
    const p = progressOf(emitter);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    ctx.translate(emitter.x, emitter.y);
    ctx.scale(1, squash);
    ctx.strokeStyle = color;
    ctx.shadowColor = color;
    ctx.shadowBlur = 12;

    if (args.mode === 'rune') {
      const appear = easeOut(Math.min(p / 0.2, 1));
      const fade = p > 0.7 ? 1 - (p - 0.7) / 0.3 : 1;
      const r = radius * (0.6 + 0.4 * appear);
      ctx.globalAlpha = opacity * fade;
      ctx.rotate(performance.now() / 600);

      ctx.lineWidth = width;
      ctx.beginPath();
      ctx.arc(0, 0, r, 0, Math.PI * 2);
      ctx.stroke();

      ctx.lineWidth = width * 0.5;
      ctx.beginPath();
      ctx.arc(0, 0, r * 0.75, 0, Math.PI * 2);
      ctx.stroke();

      // Руны-засечки между кругами
      ctx.lineWidth = width * 0.8;
      for (let i = 0; i < 8; i++) {
        const a = (i * Math.PI) / 4;
        ctx.beginPath();
        ctx.moveTo(Math.cos(a) * r * 0.78, Math.sin(a) * r * 0.78);
        ctx.lineTo(Math.cos(a + 0.2) * r * 0.97, Math.sin(a + 0.2) * r * 0.97);
        ctx.stroke();
      }

      const fill = ctx.createRadialGradient(0, 0, 0, 0, 0, r);
      fill.addColorStop(0, color);
      fill.addColorStop(1, 'rgba(0, 0, 0, 0)');
      ctx.globalAlpha = opacity * fade * 0.25;
      ctx.fillStyle = fill;
      ctx.beginPath();
      ctx.arc(0, 0, r, 0, Math.PI * 2);
      ctx.fill();
    } else {
      const grow = easeOut(p);
      const r = radius * (0.3 + 0.9 * grow);
      ctx.globalAlpha = opacity * (1 - p);
      ctx.lineWidth = width * (1 - p) + 1;
      ctx.beginPath();
      ctx.arc(0, 0, r, 0, Math.PI * 2);
      ctx.stroke();

      const fill = ctx.createRadialGradient(0, 0, 0, 0, 0, r);
      fill.addColorStop(0, color);
      fill.addColorStop(1, 'rgba(0, 0, 0, 0)');
      ctx.globalAlpha = opacity * (1 - p) * 0.3;
      ctx.fillStyle = fill;
      ctx.beginPath();
      ctx.arc(0, 0, r, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.restore();
  }
}
