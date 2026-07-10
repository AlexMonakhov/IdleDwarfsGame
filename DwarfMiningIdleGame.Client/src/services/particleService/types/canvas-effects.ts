import { EffectStrategy, Particle } from './interfaces/canvas-interfaces';

export class ParticlesStrategy implements EffectStrategy {
  // Частицы теперь должны храниться либо в эмиттере, либо в Map внутри стратегии по ID эмиттера
  private emitterParticles = new Map<string, Particle[]>();

  update(emitter: any) {
    const particles = this.emitterParticles.get(emitter.id) || [];
    
    if (particles.length < 50) {
      particles.push({
        x: emitter.x, // Используем X кнопки
        y: emitter.y, // Используем Y кнопки
        vx: (Math.random() - 0.5) * 2,
        vy: (Math.random() - 0.5) * 2,
        life: 1,
        maxLife: Math.random() * 0.02 + 0.01,
        size: Math.random() * 3 + 1,
        color: `rgb(232, 250, 70)`
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

  draw(ctx: CanvasRenderingContext2D, emitter: any) {
    const particles = this.emitterParticles.get(emitter.id);
    if (!particles) return;
    ctx.save(); // Сохраняем состояние
    
    ctx.globalCompositeOperation = 'lighter';
    particles.forEach(p => {
      ctx.fillStyle = p.color;
      ctx.globalAlpha = 1;
      ctx.beginPath();
      ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
      ctx.fill();
    });
    ctx.restore(); // Восстанавливаем состояние, чтобы свечения не влияли на другие эффекты
  }
}

export class GodRaysStrategy implements EffectStrategy {
  private angle = 0;
  private pulse = 0.8;
  private readonly rayCount = 15;

  update() {
    this.angle += 0.005;
    this.pulse = Math.sin(Date.now() / 1000) * 0.2 + 0.8; // Пульсация от 0.6 до 1.0
  }

  draw(ctx: CanvasRenderingContext2D, emitter: any) {
    const maxRadius = emitter.padding || 40;
    ctx.globalCompositeOperation = 'lighter';
    ctx.save();
    // Перемещаемся к координатам конкретной кнопки
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
      ctx.globalAlpha = 1 * this.pulse; 
    }
    
    ctx.restore();
  }
}

export class GodRaysRadianceStrategy implements EffectStrategy {
  private angle = 0;
  private pulse = 0.8;
  private readonly rayCount = 65; // Увеличено до 25 лучей
  // Массив для хранения уникальных смещений для плавного "дыхания" каждого луча
  private uniqueRayOffsets: number[] = [];

  constructor() {
    // В конструкторе инициализируем уникальные смещения для каждого луча
    for (let i = 0; i < this.rayCount; i++) {
      this.uniqueRayOffsets.push(Math.random() * 100); // Случайное смещение для каждого луча
    }
  }

  update() {
    this.angle += 0.001;
    this.pulse = Math.sin(Date.now() / 1000) * 0.1 + 0.8; // Глобальная пульсация от 0.6 до 1.0
  }

  draw(ctx: CanvasRenderingContext2D, emitter: any) {
    const maxRadius = emitter.padding || 40;
    
    // Новые параметры для формы луча
    const baseRadius = 50;   // Базовая длина луча (острие)
    const innerRadius = 30;   // Отступ от центра (создает пустоту внутри)
    const rayWidth = 0.6;     // Ширина основания луча в радианах

    // Время для плавного изменения (в секундах)
    const currentTime = Date.now() / 1000;

    ctx.globalCompositeOperation = 'lighter';
    ctx.save();
    
    // Перемещаемся к координатам кнопки
    ctx.translate(emitter.x, emitter.y);

    const gradient = ctx.createRadialGradient(0, 0, 0, 0, 0, maxRadius);
    gradient.addColorStop(1, 'rgba(250, 192, 68, 0.9)');

    ctx.fillStyle = gradient;
    ctx.globalAlpha = this.pulse; 
    ctx.rotate(this.angle);
    for (let i = 0; i < this.rayCount; i++) {
      // Распределяем лучи по всему кругу (2 * PI)
      const startAngle = (i * Math.PI * 2.5) / this.rayCount;
      const endAngle = startAngle + rayWidth;

      // Угол для острия (середина между startAngle и endAngle)
      const midAngle = startAngle + (rayWidth / 2);
       // Добавляем небольшое вращение для каждого луча
      // --- Плавное изменение уникальной длины для *этого* луча ---
      // Используем синус с уникальным смещением для каждого луча
      // Это создает плавную, плавную вариацию (+/- 25px)
      const smoothVariation = Math.sin(currentTime + this.uniqueRayOffsets[i]) * 20 + 20;
      
      // Вычисляем итоговый радиус для острия с учетом глобальной пульсации
      // (базовая длина + плавная вариация) * глобальная пульсация
      const currentOuterRadius = (baseRadius + smoothVariation) * this.pulse;

      ctx.beginPath();
      // 1. Левая точка внутреннего основания
      ctx.moveTo(
        innerRadius * Math.cos(startAngle),
        innerRadius * Math.sin(startAngle)
      );
      
      // 2. Правая точка внутреннего основания
      ctx.lineTo(
        innerRadius * Math.cos(endAngle),
        innerRadius * Math.sin(endAngle)
      );
      
      // 3. Внешняя точка (острие луча), используем currentOuterRadius
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