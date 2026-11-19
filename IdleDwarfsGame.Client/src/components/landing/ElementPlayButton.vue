<script lang="ts" setup>
import { computed, type CSSProperties } from 'vue';
import { SquadBonus } from '@/battle/squadBonuses';
import { ELEMENT_GEM } from './landingElements';
import { ELEMENT_AURAS, type AuraEffect } from '@/components/effects/elementAura';
import GemButton from '@/components/common/GemButton.vue';

// Гем-кнопка игры (GemButton) в цвете бонуса стихий: камень цвета стихии (у пары — разрезан на две, у призмы —
// радужный), дышащее свечение и частицы стихии вокруг — те же мотивы, что у аур на портретах (CssAuraEffect),
// только под прямоугольник. Смена бонуса перетекает цветом и вспыхивает
const props = withDefaults(defineProps<{
    bonus: SquadBonus;
    size?: 'sm' | 'md' | 'lg';
}>(), {
    size: 'md',
});

const HEIGHT = { sm: '40px', md: '52px', lg: '60px' } as const;

const prism = computed(() => props.bonus.shape === 'Prism');

const style = computed(() => {
    const [first, second] = props.bonus.elements;
    const a = first ? ELEMENT_GEM[first] : '#e91e63';
    const b = second ? ELEMENT_GEM[second] : a;
    return { '--c1': a, '--c2': b, '--h': HEIGHT[props.size] } as CSSProperties;
});

interface Particle {
    effect: AuraEffect;
    glyph?: string;
    style: CSSProperties;
}

const rand = (i: number, salt: number) => {
    const x = Math.sin(i * 12.9898 + salt * 78.233) * 43758.5453;
    return x - Math.floor(x);
};

const timing = (duration: number, i: number) => ({
    animationDuration: `${duration.toFixed(2)}s`,
    animationDelay: `${(-rand(i, 9) * duration).toFixed(2)}s`,
});

// Точка на периметре прямоугольника (t — доля обхода) — для молний и звёзд по краю кнопки
const onEdge = (t: number) => {
    const p = (t % 1) * 4;
    if (p < 1) return { left: `${(p * 100).toFixed(1)}%`, top: '0%' };
    if (p < 2) return { left: '100%', top: `${((p - 1) * 100).toFixed(1)}%` };
    if (p < 3) return { left: `${((3 - p) * 100).toFixed(1)}%`, top: '100%' };
    return { left: '0%', top: `${((4 - p) * 100).toFixed(1)}%` };
};

const SNOW_GLYPHS = ['❄︎', '❅', '❆', '•'];

const make = (effect: AuraEffect, count: number, salt: number): Particle[] => Array.from({ length: count }, (_, k) => {
    const i = k + salt * 31;
    switch (effect) {
        case 'snow':
            return { effect, glyph: SNOW_GLYPHS[k % 4], style: { left: `${(rand(i, 1) * 100).toFixed(1)}%`, '--s': (0.28 + rand(i, 2) * 0.22).toFixed(2), '--sway': `${((rand(i, 3) - 0.5) * 30).toFixed(0)}px`, ...timing(2.4 + rand(i, 4) * 2, i) } };
        case 'embers':
            return { effect, style: { left: `${(4 + rand(i, 1) * 92).toFixed(1)}%`, '--s': (0.08 + rand(i, 3) * 0.08).toFixed(3), '--drift': `${((rand(i, 5) - 0.5) * 40).toFixed(0)}px`, ...timing(1.4 + rand(i, 4) * 1.4, i) } };
        case 'sparks':
            return { effect, style: { ...onEdge(rand(i, 1)), '--s': (0.34 + rand(i, 3) * 0.2).toFixed(2), '--angle': `${Math.round(rand(i, 2) * 360)}deg`, ...timing(1.1 + rand(i, 4) * 1.4, i) } };
        case 'leaves':
            return { effect, style: { top: `${(rand(i, 1) * 80).toFixed(1)}%`, '--s': (0.22 + rand(i, 2) * 0.14).toFixed(2), '--rise': `${((rand(i, 3) - 0.5) * 40).toFixed(0)}px`, '--spin': `${(rand(i, 5) > 0.5 ? 1 : -1) * (240 + Math.round(rand(i, 6) * 240))}deg`, '--hue': String(95 + Math.round(rand(i, 7) * 50)), ...timing(2.6 + rand(i, 4) * 2.2, i) } };
        case 'shadow':
        case 'blood': {
            const mote = k % 3 === 2;
            const colors = effect === 'shadow'
                ? (mote ? ['#f3e8ff', '#c084fc'] : ['#7e22ce', 'rgba(46, 16, 101, 0.6)'])
                : (mote ? ['#ffe4e6', '#f472b6'] : ['#be123c', 'rgba(76, 5, 25, 0.6)']);
            return { effect, style: { left: `${(4 + rand(i, 1) * 92).toFixed(1)}%`, '--s': (mote ? 0.1 + rand(i, 3) * 0.08 : 0.4 + rand(i, 3) * 0.35).toFixed(2), '--core': colors[0], '--edge': colors[1], '--drift': `${((rand(i, 5) - 0.5) * 40).toFixed(0)}px`, ...timing(mote ? 1.8 + rand(i, 4) : 2.4 + rand(i, 4) * 2, i) } };
        }
        case 'prism':
        default:
            return { effect: 'prism', glyph: k % 2 ? '✦' : '✧', style: { ...onEdge(rand(i, 1)), '--s': (0.3 + rand(i, 3) * 0.22).toFixed(2), '--hue': String(Math.round(rand(i, 2) * 360)), ...timing(1.6 + rand(i, 4) * 1.6, i) } };
    }
});

const particles = computed<Particle[]>(() => {
    if (prism.value) return make('prism', 14, 7);
    const elements = props.bonus.elements;
    const per = elements.length > 1 ? 8 : 13;
    return elements.flatMap((element, n) => make(ELEMENT_AURAS[element], per, n + 1));
});
</script>

<template>
    <span class="element-play" :class="[`element-play-${size}`, { 'element-play-prism': prism }]" :style="style">
        <span class="ep-glow" aria-hidden="true"></span>
        <GemButton class="ep-gem" color="green" :height="HEIGHT[size]"><slot /></GemButton>
        <!-- Ключ — бонус: при смене частицы новой стихии появляются заново, а кнопка вспыхивает -->
        <span :key="bonus.id" class="ep-particles" aria-hidden="true">
            <span v-for="(particle, i) in particles" :key="i" class="ep-particle" :class="`ep-${particle.effect}`" :style="particle.style">{{ particle.glyph }}</span>
        </span>
        <span :key="`flash-${bonus.id}`" class="ep-flash" aria-hidden="true"></span>
    </span>
</template>

<style scoped>
/* Цвета камня регистрируем как <color>, чтобы смена стихии плавно перетекала, а не щёлкала */
@property --c1 { syntax: '<color>'; inherits: true; initial-value: #00c853; }
@property --c2 { syntax: '<color>'; inherits: true; initial-value: #00c853; }

.element-play {
    position: relative;
    display: inline-flex;
    transition: --c1 0.7s ease, --c2 0.7s ease;
}

/* Дышащее свечение под кнопкой цветом стихии */
.ep-glow {
    position: absolute;
    inset: -4px;
    border-radius: 12px;
    background: linear-gradient(115deg, var(--c1), var(--c2));
    filter: blur(14px);
    opacity: 0.6;
    pointer-events: none;
    animation: ep-breathe 2.6s ease-in-out infinite;
}

.ep-gem {
    position: relative;
    z-index: 1;
    min-width: calc(var(--h) * 3);
}

/* Камень GemButton перекрашиваем в цвет бонуса: у пары — разрезан по диагонали на две стихии */
.ep-gem :deep(.gem-inner) {
    background: linear-gradient(115deg, var(--c1) 0%, var(--c1) 47%, var(--c2) 53%, var(--c2) 100%);
}

.element-play-prism .ep-gem :deep(.gem-inner) {
    background: linear-gradient(115deg, #ff4d6d, #ffb300, #00c853, #2196f3, #9b30ff, #e91e63);
    background-size: 300% 100%;
    animation: ep-prism-slide 6s linear infinite alternate;
}

.ep-gem :deep(.gem-inner-top) {
    padding: 5px calc(var(--h) * 0.4);
}

.ep-gem :deep(.gem-text) {
    font-size: calc(var(--h) * 0.33);
    text-shadow: var(--text-outline);
}

.ep-particles {
    position: absolute;
    inset: -10px;
    z-index: 2;
    pointer-events: none;
    animation: ep-fade-in 0.6s ease;
}

.ep-particle {
    position: absolute;
    line-height: 1;
    animation-iteration-count: infinite;
    will-change: transform, opacity;
}

/* Вспышка при смене бонуса: волна света от кнопки */
.ep-flash {
    position: absolute;
    inset: 0;
    z-index: 3;
    border-radius: 8px;
    pointer-events: none;
    box-shadow: 0 0 0 0 var(--c1);
    animation: ep-flash 0.9s ease-out forwards;
}

/* ===== Частицы стихий ===== */
/* Снег падает сквозь кнопку */
.ep-snow {
    top: -4px;
    font-size: calc(var(--h) * var(--s));
    color: #f0f9ff;
    text-shadow: 0 0 3px #7dd3fc, 0 0 6px #38bdf8;
    animation-name: ep-fall;
    animation-timing-function: linear;
}

/* Угли поднимаются от нижнего края */
.ep-embers {
    bottom: 6px;
    width: calc(var(--h) * var(--s));
    height: calc(var(--h) * var(--s));
    border-radius: 50%;
    background: radial-gradient(circle, #fff4c2 0%, #ffb347 45%, #ff5a00 100%);
    box-shadow: 0 0 4px 1px #ff7b00;
    animation-name: ep-rise;
    animation-timing-function: ease-out;
}

/* Молнии вспыхивают по краю */
.ep-sparks {
    width: calc(var(--h) * var(--s) * 0.6);
    height: calc(var(--h) * var(--s));
    margin: calc(var(--h) * var(--s) * -0.5) 0 0 calc(var(--h) * var(--s) * -0.3);
    background: linear-gradient(to bottom, #ffffff, #fff3a3 35%, #ffb300);
    clip-path: polygon(45% 0, 85% 0, 58% 40%, 92% 40%, 25% 100%, 42% 56%, 10% 56%);
    filter: drop-shadow(0 0 3px #ffd84a);
    rotate: var(--angle);
    animation-name: ep-spark;
    animation-timing-function: steps(1, end);
}

/* Листья пролетают вдоль кнопки */
.ep-leaves {
    left: -6%;
    width: calc(var(--h) * var(--s));
    height: calc(var(--h) * var(--s) * 0.6);
    border-radius: 0 100% 0 100%;
    background: linear-gradient(135deg, hsl(var(--hue), 85%, 70%), hsl(var(--hue), 75%, 35%));
    box-shadow: 0 0 4px hsla(var(--hue), 80%, 50%, 0.6);
    animation-name: ep-drift;
    animation-timing-function: ease-in-out;
}

/* Клубы тьмы и крови поднимаются и расползаются */
.ep-shadow,
.ep-blood {
    bottom: 0;
    width: calc(var(--h) * var(--s));
    height: calc(var(--h) * var(--s));
    border-radius: 50%;
    background: radial-gradient(circle, var(--core) 0%, var(--edge) 55%, transparent 72%);
    animation-name: ep-wisp;
    animation-timing-function: ease-out;
}

/* Радужные звёзды мерцают по краю */
.ep-prism {
    width: 1em;
    margin: -0.5em 0 0 -0.5em;
    text-align: center;
    font-size: calc(var(--h) * var(--s));
    color: hsl(var(--hue), 100%, 82%);
    text-shadow: 0 0 4px hsl(var(--hue), 100%, 60%), 0 0 8px #fff;
    animation-name: ep-twinkle;
    animation-timing-function: ease-in-out;
}

@keyframes ep-breathe {
    0%, 100% { opacity: 0.45; transform: scale(0.97); }
    50% { opacity: 0.85; transform: scale(1.03); }
}

@keyframes ep-prism-slide { to { background-position: 100% 0; } }

@keyframes ep-fade-in { from { opacity: 0; } }

@keyframes ep-flash {
    0% { box-shadow: 0 0 0 0 var(--c1); background: rgba(255, 255, 255, 0.55); }
    100% { box-shadow: 0 0 0 calc(var(--h) * 0.5) transparent; background: transparent; }
}

@keyframes ep-fall {
    0% { transform: translate(0, 0) rotate(0deg); opacity: 0; }
    15% { opacity: 1; }
    50% { transform: translate(var(--sway), calc(var(--h) * 0.6)) rotate(90deg); }
    85% { opacity: 1; }
    100% { transform: translate(0, calc(var(--h) * 1.2)) rotate(180deg); opacity: 0; }
}

@keyframes ep-rise {
    0% { transform: translate(0, 0) scale(1); opacity: 0; }
    10% { opacity: 1; }
    100% { transform: translate(var(--drift), calc(var(--h) * -1.3)) scale(0.3); opacity: 0; }
}

@keyframes ep-spark {
    0% { opacity: 0; transform: scale(0.6); }
    6% { opacity: 1; transform: scale(1); }
    11% { opacity: 0.2; transform: scale(0.9); }
    16% { opacity: 1; transform: scale(1.1); }
    24%, 100% { opacity: 0; transform: scale(0.6); }
}

@keyframes ep-drift {
    0% { transform: translate(0, 0) rotate(0deg); opacity: 0; }
    10% { opacity: 1; }
    50% { transform: translate(calc(var(--h) * 2), var(--rise)) rotate(calc(var(--spin) * 0.5)); }
    90% { opacity: 1; }
    100% { transform: translate(calc(var(--h) * 4.5), 0) rotate(var(--spin)); opacity: 0; }
}

@keyframes ep-wisp {
    0% { transform: translate(0, 0) scale(0.5); opacity: 0; }
    20% { opacity: 0.85; }
    100% { transform: translate(var(--drift), calc(var(--h) * -1.1)) scale(1.4); opacity: 0; }
}

@keyframes ep-twinkle {
    0%, 100% { transform: scale(0) rotate(0deg); opacity: 0; }
    50% { transform: scale(1) rotate(45deg); opacity: 1; }
}

@media (prefers-reduced-motion: reduce) {
    .ep-glow,
    .ep-particle,
    .ep-gem :deep(.gem-inner) {
        animation: none !important;
    }
}
</style>
