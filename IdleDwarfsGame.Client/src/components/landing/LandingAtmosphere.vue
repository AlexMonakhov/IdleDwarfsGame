<script lang="ts" setup>
import { computed, type CSSProperties } from 'vue';
import { SquadBonus } from '@/battle/squadBonuses';
import { ELEMENT_AURAS, type AuraEffect } from '@/components/effects/elementAura';
import { bonusTints } from './landingElements';

// Атмосфера лендинга на весь экран — в цвет текущего бонуса стихий: подсветка фона и частицы стихии
// (угли поднимаются, снег падает, молнии вспыхивают, листья летят, клубы тьмы и крови, радужные звёзды).
// Смена бонуса — плавная смена слоя. Мотивы те же, что у аур на портретах (CssAuraEffect)
const props = defineProps<{
    bonus: SquadBonus;
    // Узкий экран: частиц меньше
    compact?: boolean;
}>();

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

const SNOW_GLYPHS = ['❄︎', '❅', '❆', '•', '•'];

const make = (effect: AuraEffect, count: number, salt: number): Particle[] => Array.from({ length: count }, (_, k) => {
    const i = k + salt * 37;
    const left = `${(rand(i, 1) * 100).toFixed(1)}%`;
    switch (effect) {
        case 'embers':
            return { effect, style: { left, '--s': `${(3 + rand(i, 3) * 5).toFixed(1)}px`, '--drift': `${((rand(i, 5) - 0.5) * 18).toFixed(1)}vw`, ...timing(7 + rand(i, 4) * 7, i) } };
        case 'snow':
            return { effect, glyph: SNOW_GLYPHS[k % SNOW_GLYPHS.length], style: { left, '--s': `${(10 + rand(i, 2) * 14).toFixed(0)}px`, '--sway': `${((rand(i, 3) - 0.5) * 10).toFixed(1)}vw`, ...timing(11 + rand(i, 4) * 9, i) } };
        case 'sparks':
            return { effect, style: { left, top: `${(rand(i, 2) * 100).toFixed(1)}%`, '--s': `${(26 + rand(i, 3) * 30).toFixed(0)}px`, '--angle': `${Math.round((rand(i, 6) - 0.5) * 70)}deg`, ...timing(2.5 + rand(i, 4) * 4, i) } };
        case 'leaves':
            return { effect, style: { top: `${(rand(i, 2) * 90).toFixed(1)}%`, '--s': `${(12 + rand(i, 3) * 10).toFixed(0)}px`, '--rise': `${((rand(i, 5) - 0.3) * 30).toFixed(1)}vh`, '--spin': `${(rand(i, 6) > 0.5 ? 1 : -1) * (360 + Math.round(rand(i, 7) * 360))}deg`, '--hue': String(90 + Math.round(rand(i, 8) * 55)), ...timing(12 + rand(i, 4) * 10, i) } };
        case 'shadow':
        case 'blood': {
            const mote = k % 3 === 2;
            const colors = effect === 'shadow'
                ? (mote ? ['#f3e8ff', '#c084fc'] : ['rgba(126, 34, 206, 0.55)', 'rgba(46, 16, 101, 0.3)'])
                : (mote ? ['#ffe4e6', '#f472b6'] : ['rgba(190, 18, 60, 0.5)', 'rgba(76, 5, 25, 0.3)']);
            return { effect, style: { left, '--s': mote ? `${(5 + rand(i, 3) * 5).toFixed(0)}px` : `${(120 + rand(i, 3) * 160).toFixed(0)}px`, '--core': colors[0], '--edge': colors[1], '--drift': `${((rand(i, 5) - 0.5) * 16).toFixed(1)}vw`, ...timing(mote ? 8 + rand(i, 4) * 5 : 14 + rand(i, 4) * 10, i) } };
        }
        case 'prism':
        default:
            return { effect: 'prism', glyph: k % 2 ? '✦' : '✧', style: { left, top: `${(rand(i, 2) * 100).toFixed(1)}%`, '--s': `${(10 + rand(i, 3) * 16).toFixed(0)}px`, '--hue': String(Math.round(rand(i, 6) * 360)), ...timing(3 + rand(i, 4) * 4, i) } };
    }
});

const particles = computed<Particle[]>(() => {
    const total = props.compact ? 18 : 34;
    if (props.bonus.shape === 'Prism') return make('prism', total, 7);
    const elements = props.bonus.elements;
    return elements.flatMap((element, n) => make(ELEMENT_AURAS[element], Math.round(total / elements.length), n + 1));
});

// Подсветка: пятна оттенков стихий по углам и снизу
const tintStyle = computed<CSSProperties>(() => {
    const [a, b = a, c = b] = bonusTints(props.bonus);
    return {
        background: [
            `radial-gradient(ellipse 55% 45% at 15% 25%, color-mix(in srgb, ${a} 22%, transparent), transparent 70%)`,
            `radial-gradient(ellipse 50% 45% at 85% 65%, color-mix(in srgb, ${b} 20%, transparent), transparent 70%)`,
            `radial-gradient(ellipse 80% 35% at 50% 105%, color-mix(in srgb, ${c} 24%, transparent), transparent 75%)`,
        ].join(', '),
    };
});
</script>

<template>
    <div class="atmosphere" aria-hidden="true">
        <Transition name="atmosphere-fade">
            <div :key="bonus.id" class="atmosphere-layer">
                <div class="atmosphere-tint" :style="tintStyle"></div>
                <span v-for="(particle, i) in particles" :key="i" class="atmo-particle" :class="`atmo-${particle.effect}`" :style="particle.style">{{ particle.glyph }}</span>
            </div>
        </Transition>
    </div>
</template>

<style scoped>
.atmosphere {
    position: absolute;
    inset: 0;
    overflow: hidden;
    pointer-events: none;
}

.atmosphere-layer,
.atmosphere-tint {
    position: absolute;
    inset: 0;
}

.atmosphere-fade-enter-active,
.atmosphere-fade-leave-active {
    transition: opacity 1.1s ease;
}

.atmosphere-fade-enter-from,
.atmosphere-fade-leave-to {
    opacity: 0;
}

.atmo-particle {
    position: absolute;
    line-height: 1;
    animation-iteration-count: infinite;
    will-change: transform, opacity;
}

/* Угли поднимаются снизу через весь экран */
.atmo-embers {
    bottom: -10px;
    width: var(--s);
    height: var(--s);
    border-radius: 50%;
    background: radial-gradient(circle, #fff4c2 0%, #ffb347 45%, #ff5a00 100%);
    box-shadow: 0 0 8px 2px rgba(255, 123, 0, 0.8);
    animation-name: atmo-rise;
    animation-timing-function: ease-out;
}

/* Снег падает сверху */
.atmo-snow {
    top: -30px;
    font-size: var(--s);
    color: #f0f9ff;
    text-shadow: 0 0 4px #7dd3fc, 0 0 10px #38bdf8;
    animation-name: atmo-fall;
    animation-timing-function: linear;
}

/* Молнии вспыхивают в случайных местах — коротко и рвано */
.atmo-sparks {
    width: calc(var(--s) * 0.6);
    height: var(--s);
    background: linear-gradient(to bottom, #ffffff, #fff3a3 35%, #ffb300);
    clip-path: polygon(45% 0, 85% 0, 58% 40%, 92% 40%, 25% 100%, 42% 56%, 10% 56%);
    filter: drop-shadow(0 0 6px #ffd84a);
    rotate: var(--angle);
    animation-name: atmo-spark;
    animation-timing-function: steps(1, end);
}

/* Листья пролетают слева направо, кружась */
.atmo-leaves {
    left: -40px;
    width: var(--s);
    height: calc(var(--s) * 0.6);
    border-radius: 0 100% 0 100%;
    background: linear-gradient(135deg, hsl(var(--hue), 85%, 70%), hsl(var(--hue), 75%, 35%));
    box-shadow: 0 0 6px hsla(var(--hue), 80%, 50%, 0.6);
    animation-name: atmo-drift;
    animation-timing-function: ease-in-out;
}

/* Клубы тьмы и крови поднимаются и расползаются; мелкие — искры */
.atmo-shadow,
.atmo-blood {
    bottom: -20%;
    width: var(--s);
    height: var(--s);
    border-radius: 50%;
    background: radial-gradient(circle, var(--core) 0%, var(--edge) 50%, transparent 72%);
    animation-name: atmo-wisp;
    animation-timing-function: ease-out;
}

/* Радужные звёзды мерцают по всему экрану */
.atmo-prism {
    font-size: var(--s);
    color: hsl(var(--hue), 100%, 82%);
    text-shadow: 0 0 6px hsl(var(--hue), 100%, 60%), 0 0 12px #fff;
    animation-name: atmo-twinkle;
    animation-timing-function: ease-in-out;
}

@keyframes atmo-rise {
    0% { transform: translate(0, 0) scale(1); opacity: 0; }
    10% { opacity: 1; }
    80% { opacity: 0.8; }
    100% { transform: translate(var(--drift), -105vh) scale(0.4); opacity: 0; }
}

@keyframes atmo-fall {
    0% { transform: translate(0, 0) rotate(0deg); opacity: 0; }
    8% { opacity: 0.9; }
    50% { transform: translate(var(--sway), 55vh) rotate(180deg); }
    92% { opacity: 0.9; }
    100% { transform: translate(0, 110vh) rotate(360deg); opacity: 0; }
}

@keyframes atmo-spark {
    0% { opacity: 0; transform: scale(0.6); }
    3% { opacity: 1; transform: scale(1); }
    6% { opacity: 0.2; }
    9% { opacity: 1; transform: scale(1.1); }
    14%, 100% { opacity: 0; transform: scale(0.6); }
}

@keyframes atmo-drift {
    0% { transform: translate(0, 0) rotate(0deg); opacity: 0; }
    8% { opacity: 0.9; }
    50% { transform: translate(55vw, var(--rise)) rotate(calc(var(--spin) * 0.5)); }
    92% { opacity: 0.9; }
    100% { transform: translate(110vw, 0) rotate(var(--spin)); opacity: 0; }
}

@keyframes atmo-wisp {
    0% { transform: translate(0, 0) scale(0.6); opacity: 0; }
    20% { opacity: 0.8; }
    100% { transform: translate(var(--drift), -120vh) scale(1.4); opacity: 0; }
}

@keyframes atmo-twinkle {
    0%, 100% { transform: scale(0) rotate(0deg); opacity: 0; }
    50% { transform: scale(1) rotate(45deg); opacity: 1; }
}

@media (prefers-reduced-motion: reduce) {
    .atmo-particle {
        display: none;
    }
}
</style>
