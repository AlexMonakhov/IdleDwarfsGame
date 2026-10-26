import type { FrameEffect } from '@/models/types/Account';

// Канвас-версии эффектов рамки аватара. Рисуется в двух слоях: back — свечение под рамкой, front — частицы поверх.
// Координаты нормированы на размер аватара: центр (0, 0), край рамки — радиус 0.5

// Во сколько раз канвас больше аватара — частицам нужно место вокруг рамки
export const FRAME_CANVAS_SCALE = 1.6;

interface FrameRenderer {
    update(dt: number): void;
    // t — секунды с начала, px — сколько пикселей экрана в единице (для размытия теней)
    drawBack(ctx: CanvasRenderingContext2D, t: number, px: number): void;
    drawFront(ctx: CanvasRenderingContext2D, t: number, px: number): void;
}

const rand = (min: number, max: number) => min + Math.random() * (max - min);
const smoothstep = (edge0: number, edge1: number, x: number) => {
    const k = Math.min(1, Math.max(0, (x - edge0) / (edge1 - edge0)));
    return k * k * (3 - 2 * k);
};

// Мягкое кольцо свечения вокруг рамки
const drawHalo = (ctx: CanvasRenderingContext2D, color: string, alpha: number, inner = 0.48, outer = 0.74) => {
    const gradient = ctx.createRadialGradient(0, 0, inner, 0, 0, outer);
    gradient.addColorStop(0, `rgba(${color}, 0)`);
    gradient.addColorStop(0.45, `rgba(${color}, ${alpha})`);
    gradient.addColorStop(1, `rgba(${color}, 0)`);
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.arc(0, 0, outer, 0, Math.PI * 2);
    ctx.fill();
};

/* ===== Снег ===== */
interface Flake {
    x: number;
    y: number;
    r: number;
    speed: number;
    sway: number;
    phase: number;
    rot: number;
    spin: number;
    // Крупные — шестилучевые снежинки, мелкие — просто точки
    crystal: boolean;
}

// Снег падает внутри круга этого радиуса и тает у его края
const SNOW_CLIP = 0.72;

class SnowRenderer implements FrameRenderer {
    private flakes: Flake[] = Array.from({ length: 18 }, () => this.spawn(true));

    private spawn(anywhere: boolean): Flake {
        const crystal = Math.random() < 0.55;
        const r = crystal ? rand(0.035, 0.07) : rand(0.008, 0.016);
        return {
            x: rand(-SNOW_CLIP, SNOW_CLIP),
            y: anywhere ? rand(-SNOW_CLIP, SNOW_CLIP) : -SNOW_CLIP - r,
            r,
            // Крупные падают медленнее — так снег выглядит объёмнее
            speed: crystal ? rand(0.18, 0.3) : rand(0.25, 0.4),
            sway: rand(0.02, 0.06),
            phase: rand(0, Math.PI * 2),
            rot: rand(0, Math.PI),
            spin: rand(-1, 1),
            crystal,
        };
    }

    update(dt: number) {
        this.flakes.forEach((flake, i) => {
            flake.y += flake.speed * dt;
            flake.phase += dt * 1.6;
            flake.rot += flake.spin * dt;
            if (flake.y > SNOW_CLIP + flake.r) this.flakes[i] = this.spawn(false);
        });
    }

    drawBack(ctx: CanvasRenderingContext2D, t: number) {
        drawHalo(ctx, '186, 230, 253', 0.38 + 0.14 * Math.sin(t * 1.5));
    }

    drawFront(ctx: CanvasRenderingContext2D, t: number, px: number) {
        ctx.save();
        ctx.beginPath();
        ctx.arc(0, 0, SNOW_CLIP, 0, Math.PI * 2);
        ctx.clip();

        ctx.strokeStyle = '#f0f9ff';
        ctx.fillStyle = '#f0f9ff';
        ctx.shadowColor = '#38bdf8';
        ctx.shadowBlur = 0.03 * px;
        ctx.lineCap = 'round';

        for (const flake of this.flakes) {
            const x = flake.x + Math.sin(flake.phase) * flake.sway;
            const fade = 1 - smoothstep(SNOW_CLIP * 0.75, SNOW_CLIP, Math.hypot(x, flake.y));
            if (fade <= 0) continue;
            ctx.globalAlpha = fade;

            if (!flake.crystal) {
                ctx.beginPath();
                ctx.arc(x, flake.y, flake.r, 0, Math.PI * 2);
                ctx.fill();
                continue;
            }

            ctx.save();
            ctx.translate(x, flake.y);
            ctx.rotate(flake.rot);
            ctx.lineWidth = flake.r * 0.16;
            ctx.beginPath();
            for (let arm = 0; arm < 6; arm++) {
                ctx.rotate(Math.PI / 3);
                ctx.moveTo(0, 0);
                ctx.lineTo(0, -flake.r);
                // Веточки на луче
                ctx.moveTo(0, -flake.r * 0.55);
                ctx.lineTo(flake.r * 0.25, -flake.r * 0.8);
                ctx.moveTo(0, -flake.r * 0.55);
                ctx.lineTo(-flake.r * 0.25, -flake.r * 0.8);
            }
            ctx.stroke();
            ctx.restore();
        }
        ctx.restore();
    }
}

/* ===== Угли ===== */
interface Ember {
    x: number;
    y: number;
    vx: number;
    vy: number;
    life: number;
    maxLife: number;
    r: number;
    flicker: number;
}

// Цвет остывающей искры: от почти белого к красному
const EMBER_COLORS: [number, number, number][] = [[255, 244, 194], [255, 179, 71], [255, 90, 0], [180, 30, 0]];

const emberColor = (k: number, alpha: number) => {
    const scaled = Math.min(0.999, k) * (EMBER_COLORS.length - 1);
    const i = Math.floor(scaled);
    const f = scaled - i;
    const [r, g, b] = EMBER_COLORS[i].map((c, j) => Math.round(c + (EMBER_COLORS[i + 1][j] - c) * f));
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
};

class EmbersRenderer implements FrameRenderer {
    private embers: Ember[] = Array.from({ length: 20 }, () => this.spawn(true));

    private spawn(midLife: boolean): Ember {
        // Искры вылетают из нижней половины кольца
        const angle = rand(Math.PI * 0.1, Math.PI * 0.9);
        const radius = rand(0.42, 0.54);
        const maxLife = rand(1.2, 2.4);
        return {
            x: Math.cos(angle) * radius,
            y: Math.sin(angle) * radius,
            vx: rand(-0.08, 0.08),
            vy: -rand(0.25, 0.45),
            life: midLife ? rand(0, maxLife) : 0,
            maxLife,
            r: rand(0.012, 0.026),
            flicker: rand(0, Math.PI * 2),
        };
    }

    update(dt: number) {
        this.embers.forEach((ember, i) => {
            ember.life += dt;
            ember.x += ember.vx * dt + Math.sin(ember.life * 6 + ember.flicker) * 0.04 * dt;
            ember.y += ember.vy * dt;
            // Горячий воздух разгоняет искру вверх
            ember.vy -= 0.06 * dt;
            if (ember.life >= ember.maxLife) this.embers[i] = this.spawn(false);
        });
    }

    drawBack(ctx: CanvasRenderingContext2D, t: number) {
        drawHalo(ctx, '255, 110, 0', 0.4 + 0.12 * Math.sin(t * 2.6) + 0.06 * Math.sin(t * 7.3));
    }

    drawFront(ctx: CanvasRenderingContext2D) {
        ctx.save();
        ctx.globalCompositeOperation = 'lighter';
        ctx.lineCap = 'round';

        for (const ember of this.embers) {
            const k = ember.life / ember.maxLife;
            const alpha = (k < 0.1 ? k / 0.1 : 1 - k) * (0.75 + 0.25 * Math.sin(ember.life * 18 + ember.flicker));
            if (alpha <= 0) continue;
            const r = ember.r * (1 - 0.6 * k);

            // Короткий хвост по направлению полёта
            ctx.strokeStyle = emberColor(k + 0.15, alpha * 0.5);
            ctx.lineWidth = r;
            ctx.beginPath();
            ctx.moveTo(ember.x - ember.vx * 0.1, ember.y - ember.vy * 0.1);
            ctx.lineTo(ember.x, ember.y);
            ctx.stroke();

            const glow = ctx.createRadialGradient(ember.x, ember.y, 0, ember.x, ember.y, r * 3);
            glow.addColorStop(0, emberColor(k, alpha));
            glow.addColorStop(0.35, emberColor(k + 0.2, alpha * 0.6));
            glow.addColorStop(1, emberColor(k + 0.3, 0));
            ctx.fillStyle = glow;
            ctx.beginPath();
            ctx.arc(ember.x, ember.y, r * 3, 0, Math.PI * 2);
            ctx.fill();
        }
        ctx.restore();
    }
}

/* ===== Руны ===== */
interface Star {
    angle: number;
    radius: number;
    life: number;
    maxLife: number;
    size: number;
}

interface Mote {
    angle: number;
    radius: number;
    speed: number;
    phase: number;
}

// Две «кометы», бегущие по кольцу: золотая и ледяная
const RUNE_COMETS = [{ offset: 0, color: '255, 209, 102' }, { offset: Math.PI, color: '165, 243, 252' }];
const RUNE_RING = 0.58;

class RunesRenderer implements FrameRenderer {
    private stars: Star[] = Array.from({ length: 7 }, () => this.spawnStar(true));
    private motes: Mote[] = Array.from({ length: 10 }, () => ({
        angle: rand(0, Math.PI * 2),
        radius: rand(0.6, 0.7),
        speed: rand(0.3, 0.7),
        phase: rand(0, Math.PI * 2),
    }));

    private spawnStar(midLife: boolean): Star {
        const maxLife = rand(1.2, 2.2);
        return {
            angle: rand(0, Math.PI * 2),
            radius: rand(0.56, 0.68),
            life: midLife ? rand(0, maxLife) : 0,
            maxLife,
            size: rand(0.05, 0.09),
        };
    }

    update(dt: number) {
        this.stars.forEach((star, i) => {
            star.life += dt;
            if (star.life >= star.maxLife) this.stars[i] = this.spawnStar(false);
        });
        this.motes.forEach(mote => {
            mote.angle += mote.speed * dt;
            mote.phase += dt * 3;
        });
    }

    drawBack(ctx: CanvasRenderingContext2D, t: number) {
        drawHalo(ctx, '255, 209, 102', 0.18);

        // Комета — цепочка дуг, тускнеющих к хвосту; полный оборот за 5 секунд
        const head = t * (Math.PI * 2 / 5);
        const SEGMENTS = 24;
        const LENGTH = Math.PI * 0.7;
        ctx.save();
        ctx.lineWidth = 0.05;
        ctx.lineCap = 'round';
        for (const comet of RUNE_COMETS) {
            for (let i = 0; i < SEGMENTS; i++) {
                const from = head + comet.offset - (LENGTH * (i + 1)) / SEGMENTS;
                const to = head + comet.offset - (LENGTH * i) / SEGMENTS;
                ctx.strokeStyle = `rgba(${comet.color}, ${(1 - i / SEGMENTS) ** 2})`;
                ctx.beginPath();
                ctx.arc(0, 0, RUNE_RING, from, to);
                ctx.stroke();
            }
        }
        ctx.restore();
    }

    drawFront(ctx: CanvasRenderingContext2D, t: number, px: number) {
        ctx.save();
        ctx.globalCompositeOperation = 'lighter';
        ctx.shadowColor = '#ffb300';
        ctx.shadowBlur = 0.05 * px;

        for (const mote of this.motes) {
            ctx.fillStyle = `rgba(255, 224, 138, ${0.35 + 0.35 * Math.sin(mote.phase)})`;
            ctx.beginPath();
            ctx.arc(Math.cos(mote.angle) * mote.radius, Math.sin(mote.angle) * mote.radius, 0.01, 0, Math.PI * 2);
            ctx.fill();
        }

        ctx.fillStyle = '#ffe08a';
        for (const star of this.stars) {
            const k = star.life / star.maxLife;
            const scale = Math.sin(Math.PI * k);
            if (scale <= 0) continue;
            const outer = star.size * scale;
            const inner = outer * 0.25;

            ctx.save();
            ctx.translate(Math.cos(star.angle) * star.radius, Math.sin(star.angle) * star.radius);
            ctx.rotate(k * Math.PI / 2);
            ctx.globalAlpha = scale;
            // Четырёхлучевая звезда
            ctx.beginPath();
            for (let i = 0; i < 8; i++) {
                const r = i % 2 === 0 ? outer : inner;
                const a = (i * Math.PI) / 4;
                ctx.lineTo(Math.sin(a) * r, -Math.cos(a) * r);
            }
            ctx.closePath();
            ctx.fill();
            ctx.restore();
        }
        ctx.restore();
    }
}

const RENDERERS: Record<Exclude<FrameEffect, 'none'>, () => FrameRenderer> = {
    snow: () => new SnowRenderer(),
    embers: () => new EmbersRenderer(),
    runes: () => new RunesRenderer(),
};

/* ===== Общий цикл ===== */
// Один requestAnimationFrame на все рамки на экране, а не по циклу на каждый аватар
type Painter = (t: number, dt: number) => void;
const painters = new Set<Painter>();
let frameId = 0;
let lastTime = 0;

const tick = (now: number) => {
    // После неактивной вкладки dt может быть огромным — частицы не должны телепортироваться
    const dt = Math.min(0.05, (now - lastTime) / 1000);
    lastTime = now;
    painters.forEach(paint => paint(now / 1000, dt));
    frameId = painters.size ? requestAnimationFrame(tick) : 0;
};

const subscribe = (paint: Painter) => {
    painters.add(paint);
    if (!frameId) {
        lastTime = performance.now();
        frameId = requestAnimationFrame(tick);
    }
    return () => { painters.delete(paint); };
};

/**
 * Запускает эффект на паре канвасов (под рамкой и над ней) для аватара размера size.
 * Возвращает функцию остановки
 */
export function mountFrameEffect(
    back: HTMLCanvasElement,
    front: HTMLCanvasElement,
    effect: FrameEffect,
    size: number,
): () => void {
    const backCtx = back.getContext('2d');
    const frontCtx = front.getContext('2d');
    if (effect === 'none' || !backCtx || !frontCtx) return () => undefined;

    const dpr = window.devicePixelRatio || 1;
    const area = size * FRAME_CANVAS_SCALE;
    const pixels = Math.round(area * dpr);
    for (const canvas of [back, front]) {
        canvas.width = pixels;
        canvas.height = pixels;
    }

    const renderer = RENDERERS[effect]();
    const px = size * dpr;

    const paint: Painter = (t, dt) => {
        renderer.update(dt);
        for (const [ctx, draw] of [[backCtx, renderer.drawBack], [frontCtx, renderer.drawFront]] as const) {
            ctx.setTransform(1, 0, 0, 1, 0, 0);
            ctx.clearRect(0, 0, pixels, pixels);
            // Единица — размер аватара, начало — центр канваса
            ctx.setTransform(px, 0, 0, px, pixels / 2, pixels / 2);
            draw.call(renderer, ctx, t, px);
        }
    };

    // Без анимации — один застывший кадр
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)').matches) {
        paint(0, 0);
        return () => undefined;
    }
    return subscribe(paint);
}
