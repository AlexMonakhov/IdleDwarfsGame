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
