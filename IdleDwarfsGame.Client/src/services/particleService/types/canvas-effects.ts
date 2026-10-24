import { EffectStrategy, Particle, EmitterConfig } from './interfaces/canvas-interfaces';

export class ParticlesStrategy implements EffectStrategy {
  private emitterParticles = new Map<string, Particle[]>();

  update(emitter: EmitterConfig) {
    const particles = this.emitterParticles.get(emitter.id) || [];
    const speed = Number(emitter.args?.speed ?? 2);
    const color = emitter.args?.color ?? 'rgb(232, 250, 70)';

    if (emitter.startX !== undefined && emitter.startY !== undefined && emitter.endX !== undefined && emitter.endY !== undefined) {
      const now = performance.now();
      const elapsed = emitter.travelStartedAt !== undefined ? now - emitter.travelStartedAt : 0;
      const total = Math.max(emitter.travelDuration ?? 600, 1);
      const t = Math.min(elapsed / total, 1);

      const x = emitter.startX + (emitter.endX - emitter.startX) * t;
      const y = emitter.startY + (emitter.endY - emitter.startY) * t;

      emitter.x = x;
      emitter.y = y;

      if (particles.length < 50) {
        particles.push({
          x,
          y,
          vx: (Math.random() - 0.5) * speed,
          vy: (Math.random() - 0.5) * speed,
          life: 1,
          maxLife: Math.random() * 0.02 + 0.01,
          size: Math.random() * 4 + 2,
          color,
        });
      }
    } else if (particles.length < 50) {
      particles.push({
        x: emitter.x,
        y: emitter.y,
        vx: (Math.random() - 0.5) * speed,
        vy: (Math.random() - 0.5) * speed,
        life: 1,
        maxLife: Math.random() * 0.02 + 0.01,
        size: Math.random() * 3 + 1,
        color,
      });
    }

    particles.forEach((p, i) => {
      p.x += p.vx;
      p.y += p.vy;
      p.life -= p.maxLife;
      if (p.life <= 0) particles.splice(i, 1);
    });

    this.emitterParticles.set(emitter.id, particles);
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const particles = this.emitterParticles.get(emitter.id);
    if (!particles) return;
    ctx.save();

    ctx.globalCompositeOperation = 'lighter';
    particles.forEach(p => {
      ctx.fillStyle = p.color;
      const baseAlpha = emitter.opacity ?? 1;
      ctx.globalAlpha = baseAlpha;
      ctx.beginPath();
      ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
      ctx.fill();
    });
    ctx.restore();
  }

  disposeEmitter(id: string) {
    this.emitterParticles.delete(id);
  }
}

export class GodRaysStrategy implements EffectStrategy {
  private angle = 0;
  private pulse = 0.8;
  private readonly rayCount = 15;

  update() {
    this.angle += 0.005;
    this.pulse = Math.sin(Date.now() / 1000) * 0.2 + 0.8;
  }

  draw(ctx: CanvasRenderingContext2D, emitter: any) {
    const maxRadius = emitter.padding || 40;
    const baseAlpha = (emitter?.opacity ?? 1);
    ctx.globalCompositeOperation = 'lighter';
    ctx.save();
    ctx.translate(emitter.x, emitter.y);
    ctx.rotate(this.angle);

    const gradient = ctx.createRadialGradient(0, 0, 0, 0, 0, maxRadius);
    gradient.addColorStop(0.2, 'rgb(21, 14, 220)');
    gradient.addColorStop(1, 'rgba(248, 0, 124, 0)');

    ctx.fillStyle = gradient;

    for (let i = 0; i < this.rayCount; i++) {
      ctx.beginPath();
      ctx.moveTo(0, 0);
      const startAngle = (i * Math.PI * 2) / this.rayCount;
      const endAngle = startAngle + 0.2;
      ctx.arc(0, 0, maxRadius, startAngle, endAngle);
      ctx.lineTo(0, 0);
      ctx.fill();
      ctx.globalAlpha = baseAlpha * this.pulse;
    }

    ctx.restore();
  }
}

export class GodRaysRadianceStrategy implements EffectStrategy {
  private angle = 0;
  private pulse = 0.8;
  private readonly rayCount = 65;
  private uniqueRayOffsets: number[] = [];

  constructor() {
    for (let i = 0; i < this.rayCount; i++) {
      this.uniqueRayOffsets.push(Math.random() * 100);
    }
  }

  update() {
    this.angle += 0.001;
    this.pulse = Math.sin(Date.now() / 1000) * 0.1 + 0.8;
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const maxRadius = emitter.padding || 40;
    const baseRadius = 50;
    const innerRadius = 30;
    const rayWidth = 0.6;
    const currentTime = Date.now() / 1000;

    const baseAlpha = (emitter?.opacity ?? 1);
    ctx.globalCompositeOperation = 'lighter';
    ctx.save();

    ctx.translate(emitter.x, emitter.y);

    const gradient = ctx.createRadialGradient(0, 0, 0, 0, 0, maxRadius);
    gradient.addColorStop(1, 'rgba(250, 192, 68, 0.9)');

    ctx.fillStyle = gradient;
    ctx.globalAlpha = baseAlpha * this.pulse;
    ctx.rotate(this.angle);
    for (let i = 0; i < this.rayCount; i++) {
      const startAngle = (i * Math.PI * 2.5) / this.rayCount;
      const endAngle = startAngle + rayWidth;
      const midAngle = startAngle + (rayWidth / 2);
      const smoothVariation = Math.sin(currentTime + this.uniqueRayOffsets[i]) * 20 + 20;
      const currentOuterRadius = (baseRadius + smoothVariation) * this.pulse;

      ctx.beginPath();
      ctx.moveTo(
        innerRadius * Math.cos(startAngle),
        innerRadius * Math.sin(startAngle)
      );

      ctx.lineTo(
        innerRadius * Math.cos(endAngle),
        innerRadius * Math.sin(endAngle)
      );

      ctx.lineTo(
        currentOuterRadius * Math.cos(midAngle),
        currentOuterRadius * Math.sin(midAngle)
      );

      ctx.closePath();
      ctx.fill();
    }

    ctx.restore();
  }
}

// Ломаная молния между startX/startY и endX/endY, перестраивается каждые несколько кадров
export class LightningStrategy implements EffectStrategy {
  private bolts = new Map<string, { points: { x: number; y: number }[]; frame: number }>();

  update(emitter: EmitterConfig) {
    const bolt = this.bolts.get(emitter.id);
    if (bolt && bolt.frame++ % 3 !== 0) return;

    const sx = emitter.startX ?? emitter.x;
    const sy = emitter.startY ?? emitter.y;
    const ex = emitter.endX ?? emitter.x;
    const ey = emitter.endY ?? emitter.y;
    const segments = 12;
    const jitter = Number(emitter.args?.jitter ?? 18);

    const points = [{ x: sx, y: sy }];
    for (let i = 1; i < segments; i++) {
      const t = i / segments;
      points.push({
        x: sx + (ex - sx) * t + (Math.random() - 0.5) * jitter * 2,
        y: sy + (ey - sy) * t + (Math.random() - 0.5) * jitter * 2,
      });
    }
    points.push({ x: ex, y: ey });

    this.bolts.set(emitter.id, { points, frame: (bolt?.frame ?? 0) + 1 });
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const bolt = this.bolts.get(emitter.id);
    if (!bolt) return;

    const color = emitter.args?.color ?? '#9be7ff';
    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    ctx.globalAlpha = emitter.opacity ?? 1;
    ctx.lineJoin = 'round';

    // Широкое свечение + тонкое ядро
    for (const [width, style] of [[10, color], [3, '#ffffff']] as const) {
      ctx.lineWidth = width;
      ctx.strokeStyle = style;
      ctx.shadowColor = color;
      ctx.shadowBlur = width * 2;
      ctx.beginPath();
      bolt.points.forEach((p, i) => (i === 0 ? ctx.moveTo(p.x, p.y) : ctx.lineTo(p.x, p.y)));
      ctx.stroke();
    }
    ctx.restore();
  }

  disposeEmitter(id: string) {
    this.bolts.delete(id);
  }
}

// Вспышка попадания: расширяющееся кольцо и разлетающиеся искры
export class ImpactStrategy implements EffectStrategy {
  private sparks = new Map<string, Particle[]>();

  update(emitter: EmitterConfig) {
    let sparks = this.sparks.get(emitter.id);
    if (!sparks) {
      const color = emitter.args?.color ?? '#ffd166';
      sparks = Array.from({ length: 18 }, () => {
        const angle = Math.random() * Math.PI * 2;
        const speed = Math.random() * 4 + 2;
        return {
          x: emitter.x,
          y: emitter.y,
          vx: Math.cos(angle) * speed,
          vy: Math.sin(angle) * speed,
          life: 1,
          maxLife: Math.random() * 0.03 + 0.03,
          size: Math.random() * 3 + 2,
          color,
        };
      });
      this.sparks.set(emitter.id, sparks);
    }

    sparks.forEach(p => {
      p.x += p.vx;
      p.y += p.vy;
      p.vx *= 0.94;
      p.vy *= 0.94;
      p.life -= p.maxLife;
    });
    this.sparks.set(emitter.id, sparks.filter(p => p.life > 0));
  }

  draw(ctx: CanvasRenderingContext2D, emitter: EmitterConfig) {
    const color = emitter.args?.color ?? '#ffd166';
    const age = performance.now() - (emitter.travelStartedAt ?? performance.now());
    const progress = Math.min(age / 400, 1);
    const radius = (emitter.padding ?? 40) * (0.3 + progress);

    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    ctx.globalAlpha = (emitter.opacity ?? 1) * (1 - progress);
    ctx.strokeStyle = color;
    ctx.lineWidth = 6 * (1 - progress) + 1;
    ctx.beginPath();
    ctx.arc(emitter.x, emitter.y, radius, 0, Math.PI * 2);
    ctx.stroke();

    ctx.globalAlpha = emitter.opacity ?? 1;
    this.sparks.get(emitter.id)?.forEach(p => {
      ctx.globalAlpha = (emitter.opacity ?? 1) * Math.max(p.life, 0);
      ctx.fillStyle = p.color;
      ctx.beginPath();
      ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
      ctx.fill();
    });
    ctx.restore();
  }

  disposeEmitter(id: string) {
    this.sparks.delete(id);
  }
}
