<script lang="ts" setup>
import { onBeforeUnmount, onMounted, ref } from 'vue';

// Частицы итога боя на весь экран арены, под баннером (ResultBanner):
// победа — конфетти залпами вылетает из-за баннера и опадает, вокруг баннера мерцают звёзды; поражение — дождь и снег.
// Свой маленький канвас: эффекты боя (CanvasLayer) к этому моменту уже отыграли
const props = defineProps<{
    mode: 'victory' | 'defeat';
    // Баннер: из-за него вылетают конфетти, вокруг него — звёзды. Положение берётся каждый раз заново —
    // баннер сдвигается, когда под ним выезжает табличка с итогами
    anchor?: HTMLElement | null;
}>();

const canvas = ref<HTMLCanvasElement | null>(null);

// Если баннера нет — считаем, что он посередине сверху (доли ширины и высоты канваса)
const FALLBACK = { x: 0.32, y: 0.2, w: 0.36, h: 0.32 };

// Залпы конфетти: большой сразу, два догоняющих, дальше — небольшой «хлопок» раз в несколько секунд
const BURSTS = [
    { at: 0, count: 200 },
    { at: 0.9, count: 100 },
    { at: 1.8, count: 70 },
];
const POP_EVERY = 4;
const POP_COUNT = 50;
// Полёт: вверх-в-стороны из-за баннера, потом сопротивление воздуха тормозит и бумажки опадают
const LAUNCH_SPEED = [560, 1080];
const GRAVITY = 620;
const DRAG = 2.4;
const STAR_RATE = 14;
const RAIN_COUNT = 200;
const SNOW_COUNT = 120;

const CONFETTI_COLORS = ['#ffd166', '#ffb300', '#c084fc', '#9b30ff', '#f472b6', '#7dd3fc', '#4ade80', '#ffffff'];

interface Confetti { x: number; y: number; vx: number; vy: number; w: number; h: number; angle: number; spin: number; tilt: number; tiltSpeed: number; color: string }
interface Star { x: number; y: number; vx: number; vy: number; size: number; age: number; life: number; color: string }
interface Drop { x: number; y: number; speed: number; length: number; alpha: number }
interface Flake { x: number; y: number; speed: number; radius: number; phase: number; sway: number; alpha: number }

const rand = (min: number, max: number) => min + Math.random() * (max - min);
const pick = <T,>(list: T[]) => list[Math.floor(Math.random() * list.length)];

let frame = 0;
let width = 0;
let height = 0;
let lastTime = 0;
let elapsed = 0;
let nextBurst = 0;
let nextPop = 0;
let spawnStars = 0;
const confetti: Confetti[] = [];
const stars: Star[] = [];
const drops: Drop[] = [];
const flakes: Flake[] = [];

// Прямоугольник баннера в координатах канваса. Интерфейс масштабирован (--ui-scale), поэтому экранные пиксели
// переводим в пиксели канваса через отношение его ширины в вёрстке к ширине на экране
const bannerRect = () => {
    const el = canvas.value;
    if (!props.anchor || !el) return { x: FALLBACK.x * width, y: FALLBACK.y * height, w: FALLBACK.w * width, h: FALLBACK.h * height };
    const box = el.getBoundingClientRect();
    const rect = props.anchor.getBoundingClientRect();
    const scale = box.width ? el.clientWidth / box.width : 1;
    return { x: (rect.left - box.left) * scale, y: (rect.top - box.top) * scale, w: rect.width * scale, h: rect.height * scale };
};

// Бумажка стартует за лентой баннера и вылетает вверх-в-стороны; с краёв — сильнее вбок
const newConfetti = (): Confetti => {
    const banner = bannerRect();
    const across = rand(-1, 1);
    const angle = -Math.PI / 2 + across * 1.15 + rand(-0.15, 0.15);
    const speed = rand(LAUNCH_SPEED[0], LAUNCH_SPEED[1]);
    return {
        x: banner.x + banner.w * (0.5 + across * 0.28),
        y: banner.y + banner.h * rand(0.38, 0.6),
        vx: Math.cos(angle) * speed,
        vy: Math.sin(angle) * speed,
        w: rand(12, 22),
        h: rand(7, 12),
        angle: rand(0, Math.PI * 2),
        spin: rand(-8, 8),
        tilt: rand(0, Math.PI * 2),
        tiltSpeed: rand(4, 10),
        color: pick(CONFETTI_COLORS),
    };
};

const burst = (count: number) => {
    for (let i = 0; i < count; i++) confetti.push(newConfetti());
};

// Звезда рождается у края баннера и медленно разлетается от его центра
const newStar = (): Star => {
    const banner = bannerRect();
    const angle = rand(0, Math.PI * 2);
    const cx = banner.x + banner.w / 2;
    const cy = banner.y + banner.h * 0.45;
    const x = cx + Math.cos(angle) * banner.w * rand(0.3, 0.5);
    const y = cy + Math.sin(angle) * banner.h * rand(0.3, 0.55);
    const speed = rand(10, 35);
    return { x, y, vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed, size: rand(10, 24), age: 0, life: rand(0.9, 1.8), color: pick(['#fff7d1', '#ffd166', '#e9d5ff']) };
};

const newDrop = (anywhere: boolean): Drop => ({
    x: rand(-width * 0.2, width),
    y: anywhere ? rand(0, height) : rand(-60, -10),
    speed: rand(750, 1100),
    length: rand(26, 44),
    alpha: rand(0.25, 0.5),
});

const newFlake = (anywhere: boolean): Flake => ({
    x: rand(0, width),
    y: anywhere ? rand(0, height) : -10,
    speed: rand(30, 70),
    radius: rand(2.5, 6),
    phase: rand(0, Math.PI * 2),
    sway: rand(10, 30),
    alpha: rand(0.5, 0.95),
});

const resize = () => {
    const el = canvas.value;
    if (!el) return;
    const dpr = window.devicePixelRatio || 1;
    width = el.clientWidth;
    height = el.clientHeight;
    el.width = Math.round(width * dpr);
    el.height = Math.round(height * dpr);
    el.getContext('2d')?.setTransform(dpr, 0, 0, dpr, 0, 0);
};

const drawStar = (ctx: CanvasRenderingContext2D, star: Star) => {
    // Мерцание: звезда вырастает и гаснет за свою жизнь
    const t = star.age / star.life;
    const scale = Math.sin(t * Math.PI);
    const r = star.size * scale;
    ctx.save();
    ctx.translate(star.x, star.y);
    ctx.globalAlpha = scale;
    ctx.fillStyle = star.color;
    ctx.shadowColor = '#ffd166';
    ctx.shadowBlur = 12;
    // Четырёхлучевая искра: длинные лучи по осям, узкие между ними
    ctx.beginPath();
    for (let i = 0; i < 8; i++) {
        const angle = (i * Math.PI) / 4;
        const radius = i % 2 === 0 ? r : r * 0.28;
        ctx.lineTo(Math.cos(angle) * radius, Math.sin(angle) * radius);
    }
    ctx.closePath();
    ctx.fill();
    ctx.restore();
};

const tick = (time: number) => {
    const ctx = canvas.value?.getContext('2d');
    if (!ctx) return;
    // Шаг ограничен: после свёрнутой вкладки частицы не телепортируются
    const dt = Math.min(0.05, lastTime ? (time - lastTime) / 1000 : 0);
    lastTime = time;
    ctx.clearRect(0, 0, width, height);

    if (props.mode === 'victory') {
        elapsed += dt;
        while (nextBurst < BURSTS.length && elapsed >= BURSTS[nextBurst].at) burst(BURSTS[nextBurst++].count);
        if (nextBurst >= BURSTS.length && elapsed >= nextPop) {
            if (nextPop) burst(POP_COUNT);
            nextPop = elapsed + POP_EVERY;
        }
        spawnStars += STAR_RATE * dt;
        while (spawnStars >= 1) { stars.push(newStar()); spawnStars -= 1; }

        for (let i = confetti.length - 1; i >= 0; i--) {
            const c = confetti[i];
            // Тяжесть тянет вниз, сопротивление воздуха гасит скорость: быстрый вылет, потом медленное опадание
            c.vy += GRAVITY * dt;
            c.vx *= 1 - DRAG * dt;
            c.vy *= 1 - DRAG * dt;
            c.x += (c.vx + Math.sin(c.tilt) * 30) * dt;
            c.y += c.vy * dt;
            c.angle += c.spin * dt;
            c.tilt += c.tiltSpeed * dt;
            if (c.y > height + 20) { confetti.splice(i, 1); continue; }
            ctx.save();
            ctx.translate(c.x, c.y);
            ctx.rotate(c.angle);
            // Сжатие по высоте — бумажка переворачивается в полёте
            ctx.scale(1, Math.cos(c.tilt));
            ctx.fillStyle = c.color;
            ctx.fillRect(-c.w / 2, -c.h / 2, c.w, c.h);
            ctx.restore();
        }

        for (let i = stars.length - 1; i >= 0; i--) {
            const s = stars[i];
            s.age += dt;
            if (s.age >= s.life) { stars.splice(i, 1); continue; }
            s.x += s.vx * dt;
            s.y += s.vy * dt;
            drawStar(ctx, s);
        }
    } else {
        // Дождь — косые штрихи
        ctx.lineCap = 'round';
        ctx.lineWidth = 2.2;
        for (const d of drops) {
            d.y += d.speed * dt;
            d.x += d.speed * 0.18 * dt;
            if (d.y > height + 30) Object.assign(d, newDrop(false));
            ctx.strokeStyle = `rgba(170, 190, 215, ${d.alpha})`;
            ctx.beginPath();
            ctx.moveTo(d.x, d.y);
            ctx.lineTo(d.x - d.length * 0.18, d.y - d.length);
            ctx.stroke();
        }
        // Снег — медленные покачивающиеся хлопья поверх дождя
        ctx.fillStyle = '#f1f5f9';
        for (const f of flakes) {
            f.y += f.speed * dt;
            f.phase += dt;
            if (f.y > height + 10) Object.assign(f, newFlake(false));
            ctx.globalAlpha = f.alpha;
            ctx.beginPath();
            ctx.arc(f.x + Math.sin(f.phase) * f.sway, f.y, f.radius, 0, Math.PI * 2);
            ctx.fill();
        }
        ctx.globalAlpha = 1;
    }

    frame = requestAnimationFrame(tick);
};

onMounted(() => {
    // Без анимаций по настройке системы частиц нет
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)').matches) return;
    resize();
    window.addEventListener('resize', resize);
    // Конфетти вылетают залпами из tick: к первому кадру баннер уже стоит на месте
    if (props.mode === 'defeat') {
        for (let i = 0; i < RAIN_COUNT; i++) drops.push(newDrop(true));
        for (let i = 0; i < SNOW_COUNT; i++) flakes.push(newFlake(true));
    }
    frame = requestAnimationFrame(tick);
});

onBeforeUnmount(() => {
    cancelAnimationFrame(frame);
    window.removeEventListener('resize', resize);
});
</script>

<template>
    <canvas ref="canvas" class="result-particles" aria-hidden="true"></canvas>
</template>

<style scoped>
.result-particles {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
}
</style>
