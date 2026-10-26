<script lang="ts" setup>
import { computed, type CSSProperties } from 'vue';
import type { FrameEffect } from '@/models/types/Account';

// Аура вокруг круглого элемента на CSS-анимациях: свечение под содержимым слота и частицы поверх.
// Подходит для любого круга размера size — аватара, портрета героя, кнопки:
//   <CssAuraEffect effect="snow" :size="64"><UnitAvatar ... /></CssAuraEffect>
// Канвас-версия с тем же API — CanvasAuraEffect
interface Props {
    effect?: FrameEffect;
    size?: number;
}

const props = withDefaults(defineProps<Props>(), {
    effect: 'none',
    size: 56,
});

interface Particle {
    glyph?: string;
    style: CSSProperties;
}

// Псевдослучайное, но одинаковое при каждом рендере — частицы не прыгают при перерисовке
const rand = (i: number, salt: number) => {
    const x = Math.sin(i * 12.9898 + salt * 78.233) * 43758.5453;
    return x - Math.floor(x);
};

// Отрицательная задержка — анимация уже идёт с первого кадра, а не стартует у всех частиц разом
const timing = (duration: number, i: number) => ({
    animationDuration: `${duration.toFixed(2)}s`,
    animationDelay: `${(-rand(i, 9) * duration).toFixed(2)}s`,
});

const SNOW_GLYPHS = ['❄︎', '❅', '❆', '•'];

const snow = (): Particle[] => Array.from({ length: 14 }, (_, i) => ({
    glyph: SNOW_GLYPHS[i % SNOW_GLYPHS.length],
    style: {
        left: `${(rand(i, 1) * 100).toFixed(1)}%`,
        '--flake': (0.09 + rand(i, 2) * 0.09).toFixed(3),
        '--sway': `${((rand(i, 3) - 0.5) * 30).toFixed(1)}%`,
        ...timing(3 + rand(i, 4) * 3, i),
    },
}));

const embers = (): Particle[] => Array.from({ length: 12 }, (_, i) => ({
    style: {
        left: `${(15 + rand(i, 1) * 70).toFixed(1)}%`,
        bottom: `${(rand(i, 2) * 25).toFixed(1)}%`,
        '--ember': (0.035 + rand(i, 3) * 0.04).toFixed(3),
        '--drift': `${((rand(i, 5) - 0.5) * 60).toFixed(1)}%`,
        ...timing(1.6 + rand(i, 4) * 1.6, i),
    },
}));

// Звёздочки по кругу чуть снаружи элемента
const runes = (): Particle[] => Array.from({ length: 8 }, (_, i) => {
    const angle = (i / 8) * Math.PI * 2 + rand(i, 1) * 0.4;
    const radius = 40 + rand(i, 2) * 6;
    return {
        glyph: '✦',
        style: {
            left: `${(50 + Math.cos(angle) * radius).toFixed(1)}%`,
            top: `${(50 + Math.sin(angle) * radius).toFixed(1)}%`,
            '--star': (0.1 + rand(i, 3) * 0.08).toFixed(3),
            ...timing(1.8 + rand(i, 4) * 1.4, i),
        },
    };
});

const PARTICLES: Record<FrameEffect, () => Particle[]> = {
    none: () => [],
    snow,
    embers,
    runes,
};

const particles = computed(() => PARTICLES[props.effect]());
</script>

<template>
    <div class="aura" :class="`aura-${effect}`" :style="{ '--size': `${size}px` }">
        <div v-if="effect !== 'none'" class="aura-halo" aria-hidden="true"></div>
        <div class="aura-content">
            <slot />
        </div>
        <div v-if="particles.length" class="aura-particles" aria-hidden="true">
            <span v-for="(particle, i) in particles" :key="i" class="aura-particle" :style="particle.style">{{ particle.glyph }}</span>
        </div>
    </div>
</template>

<style scoped>
.aura {
    position: relative;
    width: var(--size);
    height: var(--size);
}

.aura-content {
    position: relative;
    z-index: 1;
    width: 100%;
    height: 100%;
}

.aura-halo {
    position: absolute;
    inset: -14%;
    z-index: 0;
    border-radius: 50%;
    pointer-events: none;
}

.aura-particles {
    position: absolute;
    inset: -18%;
    z-index: 2;
    pointer-events: none;
}

.aura-particle {
    position: absolute;
    line-height: 1;
    animation-iteration-count: infinite;
    will-change: transform, opacity;
}

/* ===== Снег: ледяное свечение и снежинки, падающие внутри круга чуть больше элемента ===== */
.aura-snow .aura-halo {
    background: radial-gradient(closest-side, transparent 72%, rgba(186, 230, 253, 0.45) 84%, transparent 100%);
    animation: aura-pulse 4s ease-in-out infinite;
}

.aura-snow .aura-particles {
    overflow: hidden;
    border-radius: 50%;
}

.aura-snow .aura-particle {
    top: -12%;
    font-size: calc(var(--size) * var(--flake));
    color: #f0f9ff;
    text-shadow: 0 0 3px #7dd3fc, 0 0 6px #38bdf8;
    animation-name: aura-snow-fall;
    animation-timing-function: linear;
}

@keyframes aura-snow-fall {
    0% { transform: translate(0, 0) rotate(0deg); opacity: 0; }
    15% { opacity: 1; }
    50% { transform: translate(var(--sway), calc(var(--size) * 0.7)) rotate(90deg); }
    85% { opacity: 1; }
    100% { transform: translate(0, calc(var(--size) * 1.45)) rotate(180deg); opacity: 0; }
}

@keyframes aura-pulse {
    0%, 100% { opacity: 0.6; }
    50% { opacity: 1; }
}

/* ===== Угли: пульсирующий ореол и искры, поднимающиеся снизу ===== */
.aura-embers .aura-halo {
    background: radial-gradient(closest-side, transparent 70%, rgba(255, 120, 0, 0.45) 84%, transparent 100%);
    animation: aura-pulse 2.4s ease-in-out infinite;
}

.aura-embers .aura-particle {
    width: calc(var(--size) * var(--ember));
    height: calc(var(--size) * var(--ember));
    border-radius: 50%;
    background: radial-gradient(circle, #fff4c2 0%, #ffb347 45%, #ff5a00 100%);
    box-shadow: 0 0 4px 1px #ff7b00;
    animation-name: aura-ember-rise;
    animation-timing-function: ease-out;
}

@keyframes aura-ember-rise {
    0% { transform: translate(0, 0) scale(1); opacity: 0; }
    10% { opacity: 1; }
    100% { transform: translate(var(--drift), calc(var(--size) * -1)) scale(0.3); opacity: 0; }
}

/* ===== Руны: вращающееся золотое кольцо и мерцающие звёзды вокруг ===== */
.aura-runes .aura-halo {
    background: conic-gradient(from 0deg, transparent 0deg, #ffd166 40deg, transparent 90deg, transparent 180deg, #a5f3fc 220deg, transparent 270deg);
    -webkit-mask: radial-gradient(closest-side, transparent 76%, #000 82%, #000 92%, transparent 100%);
    mask: radial-gradient(closest-side, transparent 76%, #000 82%, #000 92%, transparent 100%);
    animation: aura-spin 5s linear infinite;
}

.aura-runes .aura-particle {
    /* Центр звезды — в точке на круге; transform занят анимацией, поэтому сдвигаем отступами */
    margin: -0.5em 0 0 -0.5em;
    width: 1em;
    text-align: center;
    font-size: calc(var(--size) * var(--star));
    color: #ffe08a;
    text-shadow: 0 0 4px #ffb300, 0 0 8px #ffd166;
    animation-name: aura-twinkle;
    animation-timing-function: ease-in-out;
}

@keyframes aura-spin {
    to { transform: rotate(360deg); }
}

@keyframes aura-twinkle {
    0%, 100% { transform: scale(0) rotate(0deg); opacity: 0; }
    50% { transform: scale(1) rotate(45deg); opacity: 1; }
}

@media (prefers-reduced-motion: reduce) {
    .aura-halo,
    .aura-particle {
        animation: none !important;
    }
}
</style>
