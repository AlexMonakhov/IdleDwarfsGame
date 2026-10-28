<script lang="ts" setup>
import { computed, type CSSProperties } from 'vue';
import type { AuraEffect } from './elementAura';

// Аура вокруг круглого элемента на CSS-анимациях: свечение под содержимым слота и частицы поверх.
// Подходит для любого круга размера size — аватара, портрета героя, кнопки:
//   <CssAuraEffect effect="snow" :size="64"><UnitAvatar ... /></CssAuraEffect>
// Канвас-версия с тем же API — CanvasAuraEffect (только эффекты рамок аватара, без стихийных)
interface Props {
    effect?: AuraEffect;
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

// Точка на круге вокруг элемента (в % от слоя частиц)
const onRing = (angle: number, radius: number) => ({
    left: `${(50 + Math.cos(angle) * radius).toFixed(1)}%`,
    top: `${(50 + Math.sin(angle) * radius).toFixed(1)}%`,
});

// Молнии-зигзаги вспыхивают в случайных точках кольца
const sparks = (): Particle[] => Array.from({ length: 9 }, (_, i) => {
    const angle = rand(i, 1) * Math.PI * 2;
    return {
        style: {
            ...onRing(angle, 38 + rand(i, 2) * 8),
            '--bolt': (0.13 + rand(i, 3) * 0.08).toFixed(3),
            // Молния смотрит от центра наружу
            '--angle': `${((angle * 180) / Math.PI + 90).toFixed(0)}deg`,
            ...timing(1.1 + rand(i, 4) * 1.4, i),
        },
    };
});

// Листья кружатся и опадают внутри круга, как снег, но крупнее и с вращением
const leaves = (): Particle[] => Array.from({ length: 9 }, (_, i) => ({
    style: {
        left: `${(8 + rand(i, 1) * 84).toFixed(1)}%`,
        '--leaf': (0.08 + rand(i, 2) * 0.06).toFixed(3),
        '--sway': `${((rand(i, 3) - 0.5) * 50).toFixed(1)}%`,
        '--spin': `${(rand(i, 5) > 0.5 ? 1 : -1) * (240 + Math.round(rand(i, 6) * 240))}deg`,
        '--hue': (95 + Math.round(rand(i, 7) * 50)).toString(),
        ...timing(3.4 + rand(i, 4) * 2.6, i),
    },
}));

// Клубы тьмы поднимаются снизу и расползаются; каждая третья частица — яркая фиолетовая искра
const shadow = (): Particle[] => Array.from({ length: 12 }, (_, i) => {
    const mote = i % 3 === 2;
    return {
        style: {
            left: `${(12 + rand(i, 1) * 76).toFixed(1)}%`,
            bottom: `${(rand(i, 2) * 30).toFixed(1)}%`,
            '--wisp': (mote ? 0.03 + rand(i, 3) * 0.03 : 0.16 + rand(i, 3) * 0.14).toFixed(3),
            '--wisp-core': mote ? '#f3e8ff' : '#7e22ce',
            '--wisp-edge': mote ? '#c084fc' : 'rgba(46, 16, 101, 0.6)',
            '--drift': `${((rand(i, 5) - 0.5) * 50).toFixed(1)}%`,
            ...timing(mote ? 1.8 + rand(i, 4) : 2.6 + rand(i, 4) * 2, i),
        },
    };
});

// Радужные звёздочки мерцают по кругу, каждая своим цветом спектра
const prism = (): Particle[] => Array.from({ length: 10 }, (_, i) => {
    const angle = (i / 10) * Math.PI * 2 + rand(i, 1) * 0.5;
    return {
        glyph: i % 2 ? '✦' : '✧',
        style: {
            ...onRing(angle, 38 + rand(i, 2) * 10),
            '--star': (0.1 + rand(i, 3) * 0.09).toFixed(3),
            '--hue': Math.round((i / 10) * 360).toString(),
            ...timing(1.6 + rand(i, 4) * 1.6, i),
        },
    };
});

const PARTICLES: Record<AuraEffect, () => Particle[]> = {
    none: () => [],
    snow,
    embers,
    runes,
    sparks,
    leaves,
    shadow,
    prism,
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

/* ===== Искры (топаз): потрескивающее жёлтое кольцо и молнии-зигзаги, вспыхивающие по кругу ===== */
.aura-sparks .aura-halo {
    background: radial-gradient(closest-side, transparent 70%, rgba(255, 224, 102, 0.55) 83%, rgba(255, 179, 0, 0.2) 92%, transparent 100%);
    animation: aura-crackle 1.3s steps(1, end) infinite;
}

.aura-sparks .aura-particle {
    margin: calc(var(--size) * var(--bolt) * -0.5) 0 0 calc(var(--size) * var(--bolt) * -0.3);
    width: calc(var(--size) * var(--bolt) * 0.6);
    height: calc(var(--size) * var(--bolt));
    background: linear-gradient(to bottom, #ffffff, #fff3a3 35%, #ffb300);
    clip-path: polygon(45% 0, 85% 0, 58% 40%, 92% 40%, 25% 100%, 42% 56%, 10% 56%);
    rotate: var(--angle);
    animation-name: aura-spark-flash;
    animation-timing-function: steps(1, end);
}

/* Вспышка короткая и рваная: молния мигает пару раз и надолго гаснет */
@keyframes aura-spark-flash {
    0% { opacity: 0; transform: scale(0.6); }
    6% { opacity: 1; transform: scale(1); }
    11% { opacity: 0.2; transform: scale(0.9); }
    16% { opacity: 1; transform: scale(1.1); }
    24%, 100% { opacity: 0; transform: scale(0.6); }
}

@keyframes aura-crackle {
    0% { opacity: 0.55; }
    18% { opacity: 1; }
    24% { opacity: 0.4; }
    47% { opacity: 0.9; }
    55% { opacity: 0.6; }
    78% { opacity: 1; }
    86% { opacity: 0.5; }
}

/* ===== Листья (изумруд): мягкое зелёное свечение и листья, кружащиеся внутри круга ===== */
.aura-leaves .aura-halo {
    background: radial-gradient(closest-side, transparent 72%, rgba(74, 222, 128, 0.45) 84%, transparent 100%);
    animation: aura-pulse 3.6s ease-in-out infinite;
}

.aura-leaves .aura-particles {
    overflow: hidden;
    border-radius: 50%;
}

.aura-leaves .aura-particle {
    top: -10%;
    width: calc(var(--size) * var(--leaf));
    height: calc(var(--size) * var(--leaf) * 0.6);
    /* Лист — капля с острыми концами по диагонали */
    border-radius: 0 100% 0 100%;
    background: linear-gradient(135deg, hsl(var(--hue), 85%, 70%), hsl(var(--hue), 75%, 35%));
    box-shadow: inset 0 0 0 1px hsla(var(--hue), 90%, 85%, 0.35), 0 0 4px hsla(var(--hue), 80%, 50%, 0.6);
    animation-name: aura-leaf-fall;
    animation-timing-function: ease-in-out;
}

@keyframes aura-leaf-fall {
    0% { transform: translate(0, 0) rotate(0deg); opacity: 0; }
    12% { opacity: 1; }
    35% { transform: translate(var(--sway), calc(var(--size) * 0.45)) rotate(calc(var(--spin) * 0.35)); }
    65% { transform: translate(calc(var(--sway) * -0.6), calc(var(--size) * 0.9)) rotate(calc(var(--spin) * 0.7)); }
    88% { opacity: 1; }
    100% { transform: translate(0, calc(var(--size) * 1.35)) rotate(var(--spin)); opacity: 0; }
}

/* ===== Тьма (аметист): пульсирующий фиолетовый омут и клубы тени, поднимающиеся вокруг ===== */
.aura-shadow .aura-halo {
    background:
        conic-gradient(from 0deg, transparent 0deg, rgba(168, 85, 247, 0.7) 60deg, transparent 140deg, transparent 200deg, rgba(126, 34, 206, 0.7) 260deg, transparent 330deg),
        radial-gradient(closest-side, transparent 66%, rgba(59, 7, 100, 0.55) 80%, transparent 100%);
    -webkit-mask: radial-gradient(closest-side, transparent 64%, #000 76%, #000 92%, transparent 100%);
    mask: radial-gradient(closest-side, transparent 64%, #000 76%, #000 92%, transparent 100%);
    animation: aura-spin 7s linear infinite reverse, aura-pulse 3s ease-in-out infinite;
}

.aura-shadow .aura-particle {
    width: calc(var(--size) * var(--wisp));
    height: calc(var(--size) * var(--wisp));
    border-radius: 50%;
    background: radial-gradient(circle, var(--wisp-core) 0%, var(--wisp-edge) 55%, transparent 72%);
    animation-name: aura-wisp-rise;
    animation-timing-function: ease-out;
}

@keyframes aura-wisp-rise {
    0% { transform: translate(0, 0) scale(0.5); opacity: 0; }
    20% { opacity: 0.9; }
    100% { transform: translate(var(--drift), calc(var(--size) * -0.9)) scale(1.5); opacity: 0; }
}

/* ===== Сияние (бриллиант): радужное кольцо с переливом и разноцветные звёздочки ===== */
.aura-prism .aura-halo {
    background: conic-gradient(from 0deg, #ff6b6b, #ffd166, #6bff95, #6bd5ff, #a78bfa, #ff7ad9, #ff6b6b);
    -webkit-mask: radial-gradient(closest-side, transparent 74%, #000 81%, #000 90%, transparent 100%);
    mask: radial-gradient(closest-side, transparent 74%, #000 81%, #000 90%, transparent 100%);
    animation: aura-spin 4s linear infinite, aura-prism-shift 6s linear infinite;
}

.aura-prism .aura-particle {
    margin: -0.5em 0 0 -0.5em;
    width: 1em;
    text-align: center;
    font-size: calc(var(--size) * var(--star));
    color: hsl(var(--hue), 100%, 82%);
    text-shadow: 0 0 4px hsl(var(--hue), 100%, 60%), 0 0 8px #ffffff;
    animation-name: aura-twinkle, aura-prism-shift;
    animation-timing-function: ease-in-out, linear;
}

/* Перелив: оттенок плывёт по кругу спектра */
@keyframes aura-prism-shift {
    to { filter: hue-rotate(360deg); }
}

@media (prefers-reduced-motion: reduce) {
    .aura-halo,
    .aura-particle {
        animation: none !important;
    }
}
</style>
