<script lang="ts" setup>
import { computed } from 'vue';
import type { BackdropStyle } from './chestRewards';

// Фон окна карты награды (слот backdrop у CardComponent) — только градиенты, без картинок.
// Акцент — цвет редкости слота (--slot-color). Параллакс: слои глубже поверхности сдвигаются против наклона карты
// (--tilt-x/y от -1 до 1 из CardComponent), чем глубже — тем сильнее. Точка схода тоннелей — за иконкой (centerY)
const props = withDefaults(defineProps<{
    type?: BackdropStyle;
    // Где точка схода по высоте окна, %
    centerY?: number;
}>(), { type: 'tunnel', centerY: 36 });

interface DiscOptions {
    count: number;
    // Во сколько раз каждое следующее кольцо меньше предыдущего
    ratio: number;
    // Сдвиг самого глубокого кольца при наклоне, px
    shift: number;
    background: (n: number) => string;
    square?: boolean;
    clipPath?: string;
}

// Внешнее кольцо с запасом накрывает углы окна
const OUTER = 190;

const makeDiscs = ({ count, ratio, shift, background, square, clipPath }: DiscOptions) =>
    Array.from({ length: count }, (_, n) => {
        const offset = (shift * n) / (count - 1);
        return {
            n,
            style: {
                width: `${OUTER * Math.pow(ratio, n)}%`,
                background: background(n),
                borderRadius: square || clipPath ? '0' : '50%',
                clipPath,
                transform: `translate(-50%, -50%) translate(calc(var(--tilt-x, 0) * ${-offset}px), calc(var(--tilt-y, 0) * ${-offset}px))`,
            },
        };
    });

// Цикл секторов «светлый, тёмный, акцент, тёмный»
const checker = (sector: number, from: number) => `repeating-conic-gradient(from ${from}deg,
    var(--bd-light) 0deg ${sector}deg, var(--bd-dark) ${sector}deg ${sector * 2}deg,
    var(--bd-accent) ${sector * 2}deg ${sector * 3}deg, var(--bd-dark) ${sector * 3}deg ${sector * 4}deg)`;

const RING_COLORS = ['var(--bd-light)', 'var(--bd-dark)', 'var(--bd-accent)', 'var(--bd-dark)'];

// Квадратный коридор: потолок темнее, стены средние, пол светлее; кольца чередуют акцент и тёмный
const corridorWall = (n: number) => {
    const base = n % 2 ? 'var(--bd-dark)' : 'var(--bd-accent)';
    const shade = (black: number) => `color-mix(in srgb, ${base} ${100 - black}%, #000)`;
    return `conic-gradient(from -45deg, ${shade(55)} 0deg 90deg, ${shade(30)} 90deg 180deg,
        color-mix(in srgb, ${base} 85%, #fff) 180deg 270deg, ${shade(30)} 270deg 360deg)`;
};

// Шестигранная грань: 6 сторон, свет сверху-слева — верхние светлее, нижние темнее; чётные кольца чуть ярче
const HEX = 'polygon(25% 6.7%, 75% 6.7%, 100% 50%, 75% 93.3%, 25% 93.3%, 0% 50%)';
const hexFacets = (n: number) => {
    const lift = n % 2 ? 0 : 12;
    const tone = (mix: number, to: string) => `color-mix(in srgb, var(--bd-accent) ${mix + (to === '#fff' ? 0 : -lift)}%, ${to})`;
    return `conic-gradient(from -30deg,
        ${tone(55, '#fff')} 0deg 60deg, ${tone(80, '#fff')} 60deg 120deg, ${tone(70, '#000')} 120deg 180deg,
        ${tone(45, '#000')} 180deg 240deg, ${tone(60, '#000')} 240deg 300deg, ${tone(70, '#fff')} 300deg 360deg)`;
};

const discs = computed(() => {
    switch (props.type) {
        case 'tunnel':
            return makeDiscs({ count: 18, ratio: 0.8, shift: 18, background: n => checker(360 / 16, n * 9 + (n % 2) * (360 / 16)) });
        case 'rings':
            return makeDiscs({ count: 24, ratio: 0.85, shift: 18, background: n => RING_COLORS[n % 4] });
        case 'vortex':
            return makeDiscs({
                count: 22,
                ratio: 0.84,
                shift: 18,
                background: n => `repeating-conic-gradient(from ${n * 16}deg,
                    var(--bd-accent) 0deg 15deg, color-mix(in srgb, var(--bd-accent) 30%, var(--bd-dark)) 15deg 30deg)`,
            });
        case 'corridor':
            return makeDiscs({ count: 20, ratio: 0.83, shift: 18, background: corridorWall, square: true });
        case 'hexprism':
            return makeDiscs({ count: 18, ratio: 0.82, shift: 18, background: hexFacets, clipPath: HEX });
        default:
            return [];
    }
});

const hasFog = computed(() => ['tunnel', 'rings', 'vortex', 'corridor', 'hexprism'].includes(props.type));
</script>

<template>
    <span class="bd" :class="`bd-${type}`" :style="{ '--bd-center-y': `${centerY}%` }">
        <!-- Тоннели: стопка колец -->
        <span v-if="discs.length" class="bd-discs">
            <span v-for="disc in discs" :key="disc.n" class="bd-disc" :style="disc.style"></span>
        </span>

        <!-- Классика: порода в прожилках со свечением за иконкой -->
        <span v-if="type === 'classic'" class="bd-layer bd-classic"></span>

        <!-- Сияние: вращающиеся лучи, ядро света за иконкой -->
        <template v-if="type === 'sunburst'">
            <span class="bd-layer bd-sunburst-rays"></span>
            <span class="bd-layer bd-sunburst-core"></span>
        </template>

        <!-- Жилы: дальний тусклый слой трещин и ближний яркий -->
        <template v-if="type === 'veins'">
            <span class="bd-layer bd-veins-far"></span>
            <span class="bd-layer bd-veins-near"></span>
        </template>

        <!-- Грани кристалла: два слоя огранки с разных точек -->
        <template v-if="type === 'facets'">
            <span class="bd-layer bd-facets-back"></span>
            <span class="bd-layer bd-facets-front"></span>
        </template>

        <!-- Звёзды: туманность и три слоя звёзд на разной глубине -->
        <template v-if="type === 'stars'">
            <span class="bd-layer bd-nebula"></span>
            <span class="bd-layer bd-stars bd-stars-far"></span>
            <span class="bd-layer bd-stars bd-stars-mid"></span>
            <span class="bd-layer bd-stars bd-stars-near"></span>
        </template>

        <!-- Сетка: пол и потолок уходят к горизонту за иконкой -->
        <template v-if="type === 'grid'">
            <span class="bd-layer bd-grid-glow"></span>
            <span class="bd-grid-plane bd-grid-floor"></span>
            <span class="bd-grid-plane bd-grid-ceiling"></span>
        </template>

        <!-- Жеода: полость с кристаллами глубже, вокруг — полосы агата и каменная оболочка -->
        <template v-if="type === 'geode'">
            <span class="bd-layer bd-geode-cavity"></span>
            <span class="bd-layer bd-geode-bands"></span>
        </template>

        <!-- Осколки: два слоя полупрозрачных пластин внахлёст и мерцающие блики -->
        <template v-if="type === 'shards'">
            <span class="bd-layer bd-shards-back"></span>
            <span class="bd-layer bd-shards-front"></span>
            <span class="bd-layer bd-shards-glints"></span>
        </template>

        <!-- Кварцевая мозаика: мелкая треугольная огранка и свет из центра -->
        <template v-if="type === 'quartz'">
            <span class="bd-layer bd-quartz"></span>
            <span class="bd-layer bd-quartz-light"></span>
        </template>

        <!-- Дымка к центру и чёрное дно тоннеля -->
        <span v-if="hasFog" class="bd-fog"></span>
    </span>
</template>

<style scoped>
.bd {
    --bd-light: #efe6d2;
    --bd-dark: #0b0a10;
    --bd-accent: var(--slot-color, var(--reward-accent, #1f4fa8));
    position: absolute;
    inset: -12px;
    overflow: hidden;
    background: var(--bd-dark);
}

/* Слой с параллаксом: глубину задаёт --depth (px сдвига при полном наклоне) */
.bd-layer {
    position: absolute;
    inset: -24px;
    transform: translate(calc(var(--tilt-x, 0) * var(--depth, 12px) * -1), calc(var(--tilt-y, 0) * var(--depth, 12px) * -1));
    transition: transform 0.3s cubic-bezier(0.23, 1, 0.32, 1);
}

/* ---------- Тоннели (кольца) ---------- */
.bd-discs {
    position: absolute;
    inset: 0;
}

.bd-disc {
    position: absolute;
    left: 50%;
    top: var(--bd-center-y);
    aspect-ratio: 1;
    transition: transform 0.3s cubic-bezier(0.23, 1, 0.32, 1);
    box-shadow: 0 0 3px rgba(0, 0, 0, 0.45);
}

.bd-corridor .bd-disc {
    box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.35);
}

/* Вихрь медленно закручивается */
.bd-vortex .bd-discs {
    transform-origin: 50% var(--bd-center-y);
    animation: bd-spin 40s linear infinite;
}

.bd-fog {
    position: absolute;
    left: 50%;
    top: var(--bd-center-y);
    width: 150%;
    aspect-ratio: 1;
    border-radius: 50%;
    background: radial-gradient(circle closest-side, #000 0%, #000 6%, rgba(0, 0, 0, 0.7) 14%, rgba(0, 0, 0, 0.2) 34%, transparent 55%);
    transform: translate(-50%, -50%) translate(calc(var(--tilt-x, 0) * -18px), calc(var(--tilt-y, 0) * -18px));
    transition: transform 0.3s cubic-bezier(0.23, 1, 0.32, 1);
}

.bd-hexprism .bd-disc {
    box-shadow: none;
    filter: drop-shadow(0 0 2px rgba(0, 0, 0, 0.6));
}

.bd-corridor .bd-fog {
    border-radius: 0;
    background: radial-gradient(closest-side, #000 0%, #000 8%, rgba(0, 0, 0, 0.6) 16%, transparent 45%);
}

/* ---------- Классика ---------- */
.bd-classic {
    --depth: 18px;
    background:
        radial-gradient(circle at 50% var(--bd-center-y), color-mix(in srgb, var(--bd-accent) 75%, #fff) 0%, var(--bd-accent) 18%, transparent 52%),
        repeating-linear-gradient(35deg, rgba(0, 0, 0, 0.18) 0 2px, transparent 2px 14px),
        repeating-linear-gradient(-55deg, rgba(255, 255, 255, 0.06) 0 1px, transparent 1px 19px),
        radial-gradient(ellipse at 50% 45%, color-mix(in srgb, var(--bd-accent) 45%, #1a1420) 0%, #0d0b12 85%);
}

/* ---------- Сияние ---------- */
.bd-sunburst-rays {
    --depth: 10px;
    inset: -60%;
    background: repeating-conic-gradient(
        at 50% calc(30% + var(--bd-center-y) * 0.4),
        var(--bd-accent) 0deg 7deg,
        color-mix(in srgb, var(--bd-accent) 35%, var(--bd-dark)) 7deg 15deg
    );
    /* Вращение вокруг точки, откуда идут лучи, а не вокруг центра слоя */
    transform-origin: 50% calc(30% + var(--bd-center-y) * 0.4);
    animation: bd-spin-rays 30s linear infinite;
}

.bd-sunburst-core {
    --depth: 18px;
    background:
        radial-gradient(circle at 50% calc(var(--bd-center-y) + 4%), #fff 0%, color-mix(in srgb, var(--bd-accent) 60%, #fff) 10%, transparent 38%),
        radial-gradient(ellipse at 50% 50%, transparent 40%, rgba(0, 0, 0, 0.75) 100%);
}

@keyframes bd-spin-rays {
    to { rotate: 360deg; }
}

/* ---------- Жилы ---------- */
.bd-veins-far {
    --depth: 20px;
    background:
        linear-gradient(118deg, transparent 46%, color-mix(in srgb, var(--bd-accent) 50%, transparent) 49.5%, transparent 53%) 0 0 / 90px 70px,
        linear-gradient(62deg, transparent 47%, color-mix(in srgb, var(--bd-accent) 40%, transparent) 50%, transparent 53%) 20px 10px / 70px 110px,
        radial-gradient(ellipse at 50% var(--bd-center-y), color-mix(in srgb, var(--bd-accent) 30%, #15121b) 0%, #07060a 75%);
    filter: blur(0.6px);
}

.bd-veins-near {
    --depth: 6px;
    background:
        linear-gradient(105deg, transparent 48.5%, var(--bd-accent) 49.6%, #fff 50%, var(--bd-accent) 50.4%, transparent 51.5%) -30px 40px / 160px 120px,
        linear-gradient(28deg, transparent 48.5%, var(--bd-accent) 49.6%, #fff 50%, var(--bd-accent) 50.4%, transparent 51.5%) 40px -20px / 130px 180px,
        linear-gradient(150deg, transparent 49%, var(--bd-accent) 50%, transparent 51%) 10px 80px / 110px 90px;
    filter: drop-shadow(0 0 3px var(--bd-accent));
    animation: bd-veins-pulse 2.6s ease-in-out infinite;
}

@keyframes bd-veins-pulse {
    0%, 100% { opacity: 0.65; }
    50% { opacity: 1; }
}

/* ---------- Грани ---------- */
.bd-facets-back {
    --depth: 16px;
    background: conic-gradient(
        at 50% var(--bd-center-y),
        color-mix(in srgb, var(--bd-accent) 90%, #fff) 0deg 22deg,
        color-mix(in srgb, var(--bd-accent) 45%, #000) 22deg 51deg,
        color-mix(in srgb, var(--bd-accent) 70%, #000) 51deg 88deg,
        color-mix(in srgb, var(--bd-accent) 60%, #fff) 88deg 117deg,
        color-mix(in srgb, var(--bd-accent) 35%, #000) 117deg 160deg,
        var(--bd-accent) 160deg 196deg,
        color-mix(in srgb, var(--bd-accent) 55%, #000) 196deg 231deg,
        color-mix(in srgb, var(--bd-accent) 75%, #fff) 231deg 262deg,
        color-mix(in srgb, var(--bd-accent) 40%, #000) 262deg 300deg,
        color-mix(in srgb, var(--bd-accent) 85%, #000) 300deg 333deg,
        color-mix(in srgb, var(--bd-accent) 50%, #fff) 333deg 360deg
    );
}

.bd-facets-front {
    --depth: 6px;
    background:
        conic-gradient(
            at 30% 85%,
            rgba(255, 255, 255, 0.18) 0deg 40deg, transparent 40deg 95deg, rgba(0, 0, 0, 0.25) 95deg 150deg,
            transparent 150deg 230deg, rgba(255, 255, 255, 0.12) 230deg 290deg, transparent 290deg
        ),
        conic-gradient(
            at 78% 10%,
            transparent 0deg 120deg, rgba(0, 0, 0, 0.3) 120deg 170deg, rgba(255, 255, 255, 0.15) 170deg 215deg, transparent 215deg
        ),
        radial-gradient(ellipse at 50% 50%, transparent 45%, rgba(0, 0, 0, 0.6) 100%);
}

/* ---------- Звёзды ---------- */
.bd-nebula {
    --depth: 22px;
    background:
        radial-gradient(ellipse 60% 40% at 30% 30%, color-mix(in srgb, var(--bd-accent) 55%, transparent), transparent 70%),
        radial-gradient(ellipse 50% 45% at 75% 70%, color-mix(in srgb, var(--bd-accent) 35%, transparent), transparent 70%),
        radial-gradient(circle at 50% var(--bd-center-y), rgba(255, 255, 255, 0.25), transparent 30%),
        #05040a;
}

.bd-stars {
    background-repeat: repeat;
}

.bd-stars-far {
    --depth: 20px;
    background-image:
        radial-gradient(1px 1px at 10px 15px, #fff, transparent),
        radial-gradient(1px 1px at 37px 48px, #fff, transparent),
        radial-gradient(1px 1px at 52px 9px, rgba(255, 255, 255, 0.7), transparent),
        radial-gradient(1px 1px at 22px 58px, rgba(255, 255, 255, 0.6), transparent);
    background-size: 64px 64px;
    opacity: 0.7;
}

.bd-stars-mid {
    --depth: 12px;
    background-image:
        radial-gradient(1.5px 1.5px at 20px 30px, #fff, transparent),
        radial-gradient(1.5px 1.5px at 75px 80px, color-mix(in srgb, var(--bd-accent) 40%, #fff), transparent),
        radial-gradient(1.5px 1.5px at 60px 15px, #fff, transparent);
    background-size: 100px 100px;
}

.bd-stars-near {
    --depth: 4px;
    background-image:
        radial-gradient(2.5px 2.5px at 40px 60px, #fff, transparent),
        radial-gradient(2px 2px at 120px 20px, color-mix(in srgb, var(--bd-accent) 50%, #fff), transparent);
    background-size: 150px 140px;
    animation: bd-twinkle 3s ease-in-out infinite;
}

@keyframes bd-twinkle {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.55; }
}

/* ---------- Сетка ---------- */
.bd-grid-glow {
    --depth: 18px;
    background:
        radial-gradient(ellipse 70% 12% at 50% var(--bd-center-y), color-mix(in srgb, var(--bd-accent) 80%, #fff), transparent 70%),
        linear-gradient(180deg, #07060c 0%, color-mix(in srgb, var(--bd-accent) 25%, #07060c) var(--bd-center-y), #07060c 100%);
}

.bd-grid-plane {
    position: absolute;
    left: -60%;
    right: -60%;
    background:
        repeating-linear-gradient(90deg, var(--bd-accent) 0 2px, transparent 2px 28px),
        repeating-linear-gradient(0deg, var(--bd-accent) 0 2px, transparent 2px 28px);
    filter: drop-shadow(0 0 2px var(--bd-accent));
    -webkit-mask: linear-gradient(var(--fade-dir), transparent 0%, #000 60%);
    mask: linear-gradient(var(--fade-dir), transparent 0%, #000 60%);
    transition: transform 0.3s cubic-bezier(0.23, 1, 0.32, 1);
    animation: bd-grid-run 1.6s linear infinite;
}

.bd-grid-floor {
    --fade-dir: 180deg;
    top: var(--bd-center-y);
    height: 120%;
    transform-origin: 50% 0;
    transform:
        translateX(calc(var(--tilt-x, 0) * -10px))
        perspective(140px) rotateX(62deg);
}

.bd-grid-ceiling {
    --fade-dir: 0deg;
    bottom: calc(100% - var(--bd-center-y));
    height: 120%;
    transform-origin: 50% 100%;
    transform:
        translateX(calc(var(--tilt-x, 0) * -10px))
        perspective(140px) rotateX(-62deg);
    opacity: 0.55;
    animation-direction: reverse;
}

@keyframes bd-grid-run {
    to { background-position: 0 28px, 0 28px; }
}

/* ---------- Жеода ---------- */
.bd-geode-cavity {
    --depth: 20px;
    background:
        radial-gradient(circle at 50% var(--bd-center-y), #fff 0%, color-mix(in srgb, var(--bd-accent) 70%, #fff) 5%, transparent 16%),
        repeating-conic-gradient(at 50% var(--bd-center-y),
            color-mix(in srgb, var(--bd-accent) 55%, #fff) 0deg 3deg,
            color-mix(in srgb, var(--bd-accent) 40%, #000) 3deg 7deg,
            var(--bd-accent) 7deg 10deg,
            color-mix(in srgb, var(--bd-accent) 70%, #000) 10deg 14deg),
        var(--bd-dark);
}

.bd-geode-bands {
    --depth: 5px;
    background:
        radial-gradient(ellipse 120% 90% at 50% var(--bd-center-y), transparent 55%, rgba(0, 0, 0, 0.85) 100%),
        repeating-radial-gradient(ellipse 100% 85% at 50% var(--bd-center-y),
            color-mix(in srgb, var(--bd-accent) 35%, #1a1520) 0 5px,
            color-mix(in srgb, var(--bd-accent) 65%, #fff) 5px 7px,
            color-mix(in srgb, var(--bd-accent) 50%, #000) 7px 12px,
            color-mix(in srgb, var(--bd-accent) 20%, #2b2433) 12px 16px);
    /* Полосы только снаружи полости: в центре — окно в кристаллы */
    -webkit-mask: radial-gradient(ellipse 34% 26% at 50% var(--bd-center-y), transparent 92%, #000 100%);
    mask: radial-gradient(ellipse 34% 26% at 50% var(--bd-center-y), transparent 92%, #000 100%);
    filter: drop-shadow(0 0 6px rgba(0, 0, 0, 0.9));
}

/* ---------- Осколки ---------- */
.bd-shards-back {
    --depth: 18px;
    background:
        linear-gradient(115deg, transparent 0 22%, color-mix(in srgb, var(--bd-accent) 60%, transparent) 22% 46%, transparent 46%),
        linear-gradient(40deg, transparent 0 35%, color-mix(in srgb, var(--bd-accent) 45%, transparent) 35% 60%, transparent 60%),
        linear-gradient(-65deg, transparent 0 50%, color-mix(in srgb, var(--bd-accent) 55%, #000) 50% 72%, transparent 72%),
        linear-gradient(170deg, transparent 0 60%, color-mix(in srgb, var(--bd-accent) 35%, transparent) 60% 85%, transparent 85%),
        radial-gradient(ellipse at 50% var(--bd-center-y), color-mix(in srgb, var(--bd-accent) 40%, #120f18), #050408 85%);
}

.bd-shards-front {
    --depth: 6px;
    background:
        linear-gradient(75deg, transparent 0 58%, color-mix(in srgb, var(--bd-accent) 40%, rgba(255, 255, 255, 0.35)) 58% 59%, color-mix(in srgb, var(--bd-accent) 30%, transparent) 59% 76%, transparent 76%),
        linear-gradient(-30deg, transparent 0 18%, rgba(255, 255, 255, 0.14) 18% 19%, color-mix(in srgb, var(--bd-accent) 35%, transparent) 19% 38%, transparent 38%),
        linear-gradient(140deg, transparent 0 66%, rgba(255, 255, 255, 0.2) 66% 67%, color-mix(in srgb, var(--bd-accent) 40%, transparent) 67% 88%, transparent 88%);
}

.bd-shards-glints {
    --depth: 4px;
    background:
        radial-gradient(3px 3px at 30% 25%, #fff, transparent),
        radial-gradient(2px 2px at 72% 58%, #fff, transparent),
        radial-gradient(2.5px 2.5px at 45% 80%, #fff, transparent),
        radial-gradient(2px 2px at 82% 18%, #fff, transparent);
    filter: drop-shadow(0 0 3px #fff);
    animation: bd-twinkle 2.2s ease-in-out infinite;
}

/* ---------- Кварцевая мозаика ---------- */
.bd-quartz {
    --depth: 16px;
    background:
        linear-gradient(60deg, rgba(255, 255, 255, 0.2) 25%, transparent 25% 50%, rgba(0, 0, 0, 0.28) 50% 75%, transparent 75%) 0 0 / 34px 59px,
        linear-gradient(-60deg, rgba(255, 255, 255, 0.1) 25%, transparent 25% 50%, rgba(0, 0, 0, 0.35) 50% 75%, transparent 75%) 0 0 / 34px 59px,
        linear-gradient(0deg, rgba(255, 255, 255, 0.06) 50%, rgba(0, 0, 0, 0.12) 50%) 0 0 / 34px 59px,
        radial-gradient(ellipse at 50% var(--bd-center-y), color-mix(in srgb, var(--bd-accent) 80%, #fff) 0%, var(--bd-accent) 25%, color-mix(in srgb, var(--bd-accent) 30%, #000) 85%);
}

.bd-quartz-light {
    --depth: 4px;
    background:
        radial-gradient(circle at 50% var(--bd-center-y), rgba(255, 255, 255, 0.45), transparent 28%),
        radial-gradient(ellipse at 50% 50%, transparent 50%, rgba(0, 0, 0, 0.55) 100%);
}

@keyframes bd-spin {
    to { rotate: 360deg; }
}
</style>
