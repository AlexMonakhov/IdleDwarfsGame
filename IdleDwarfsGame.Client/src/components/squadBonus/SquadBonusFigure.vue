<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import SquadBonusIcon from './SquadBonusIcon.vue';
import { BONUS_ELEMENTS, SquadBonus } from '@/battle/squadBonuses';
import { bonusColor, bonusElementColor } from './squadBonusText';

// Иконка бонуса отряда, которая умеет жить: animated — дышащее свечение, вращающееся кольцо и искры на орбите
// в цветах стихий; pulse — счётчик срабатываний: каждое новое значение пускает от значка волну и вспышку.
// Арена (SquadBonusBadge, BattleState.bonusPulses) и коллекция (наведение — анимация, клик — импульс)
const props = withDefaults(defineProps<{
    bonus: SquadBonus;
    size?: string;
    showTier?: boolean;
    animated?: boolean;
    pulse?: number;
}>(), {
    size: '40px',
    showTier: true,
    animated: false,
    pulse: 0,
});

const color = computed(() => bonusColor(props.bonus));
const secondColor = computed(() => (props.bonus.elements[1] ? bonusElementColor(props.bonus.elements[1]) : color.value));

// Кольцо: у призмы — все семь стихий по кругу, у остальных — две дуги своих цветов
const ringGradient = computed(() => {
    if (props.bonus.shape === 'Prism') {
        const colors = BONUS_ELEMENTS.map(bonusElementColor);
        return `conic-gradient(${[...colors, colors[0]].join(', ')})`;
    }
    return `conic-gradient(${color.value} 0deg, transparent 80deg, transparent 100deg, ${secondColor.value} 180deg, transparent 260deg, transparent 280deg, ${color.value} 360deg)`;
});

const style = computed(() => ({
    '--badge-size': props.size,
    '--badge-color': color.value,
    '--badge-second': secondColor.value,
    '--badge-ring': ringGradient.value,
}));

// Волны импульса: у каждой свой ключ, гаснет сама. Значок при этом вспыхивает (flash перезапускает анимацию)
const WAVE_MS = 1300;
const waves = ref<number[]>([]);
const flash = ref(0);
const timers = new Set<number>();

watch(() => props.pulse, (value, old) => {
    if (!value || value === old) return;
    waves.value = [...waves.value, value];
    flash.value = value;
    const timer = window.setTimeout(() => {
        waves.value = waves.value.filter(id => id !== value);
        timers.delete(timer);
    }, WAVE_MS);
    timers.add(timer);
});

onBeforeUnmount(() => timers.forEach(timer => window.clearTimeout(timer)));
</script>

<template>
    <span class="squad-bonus-figure" :style="style">
        <Transition name="badge-aura">
            <span v-if="animated" class="badge-aura" aria-hidden="true">
                <span class="badge-halo"></span>
                <span class="badge-ring"></span>
                <span class="badge-orbit">
                    <i class="badge-spark"></i>
                    <i class="badge-spark badge-spark-second"></i>
                    <i class="badge-spark"></i>
                </span>
            </span>
        </Transition>
        <span :key="flash" class="badge-icon" :class="{ 'badge-icon-flash': flash > 0 }">
            <SquadBonusIcon :bonus="bonus" :size="size" :show-tier="showTier" />
        </span>
        <span v-for="wave in waves" :key="wave" class="badge-wave" aria-hidden="true"></span>
    </span>
</template>

<style scoped>
.squad-bonus-figure {
    position: relative;
    display: inline-flex;
    flex-shrink: 0;
}

.badge-icon {
    position: relative;
    z-index: 2;
    display: inline-flex;
}

.badge-aura {
    position: absolute;
    inset: calc(var(--badge-size) * -0.32);
    z-index: 1;
    pointer-events: none;
}

/* Дышащее свечение под щитом */
.badge-halo {
    position: absolute;
    inset: 12%;
    border-radius: 50%;
    background: radial-gradient(circle, color-mix(in srgb, var(--badge-color) 55%, transparent) 0%,
        color-mix(in srgb, var(--badge-second) 25%, transparent) 45%, transparent 70%);
    animation: badge-breathe 2.8s ease-in-out infinite;
}

/* Кольцо из дуг цветов стихий: тонкий обод, медленно вращается */
.badge-ring {
    position: absolute;
    inset: 4%;
    border-radius: 50%;
    background: var(--badge-ring);
    -webkit-mask: radial-gradient(circle, transparent 62%, #000 64%, #000 68%, transparent 70%);
    mask: radial-gradient(circle, transparent 62%, #000 64%, #000 68%, transparent 70%);
    opacity: 0.85;
    animation: badge-spin 9s linear infinite;
}

/* Искры на орбите — вращаются навстречу кольцу */
.badge-orbit {
    position: absolute;
    inset: 0;
    animation: badge-spin 5.5s linear infinite reverse;
}

.badge-spark {
    position: absolute;
    top: 50%;
    left: 50%;
    width: 6px;
    height: 6px;
    margin: -3px;
    border-radius: 50%;
    background: #fff;
    box-shadow: 0 0 6px 2px var(--badge-color);
    transform: rotate(var(--spark-angle, 0deg)) translateY(calc(var(--badge-size) * -0.72));
    animation: badge-twinkle 1.6s ease-in-out infinite;
}

.badge-spark:nth-child(1) { --spark-angle: 0deg; }
.badge-spark:nth-child(2) { --spark-angle: 120deg; animation-delay: -0.5s; }
.badge-spark:nth-child(3) { --spark-angle: 240deg; animation-delay: -1.1s; }

.badge-spark-second {
    box-shadow: 0 0 6px 2px var(--badge-second);
}

/* Импульс: волна-кольцо расходится от значка, сам значок вспыхивает */
.badge-wave {
    position: absolute;
    top: 50%;
    left: 50%;
    z-index: 0;
    width: var(--badge-size);
    height: var(--badge-size);
    margin: calc(var(--badge-size) / -2);
    border-radius: 50%;
    pointer-events: none;
    border: 3px solid var(--badge-color);
    box-shadow: 0 0 14px 4px color-mix(in srgb, var(--badge-color) 60%, transparent),
        inset 0 0 12px 2px color-mix(in srgb, var(--badge-second) 50%, transparent);
    animation: badge-wave 1.3s cubic-bezier(0.2, 0.7, 0.3, 1) forwards;
}

/* Вторая волна чуть позже, цветом второй стихии */
.badge-wave::after {
    content: '';
    position: absolute;
    inset: -3px;
    border-radius: 50%;
    border: 2px solid var(--badge-second);
    animation: badge-wave-inner 1.3s cubic-bezier(0.2, 0.7, 0.3, 1) 0.15s backwards;
}

.badge-icon-flash {
    animation: badge-flash 0.6s ease-out;
}

@keyframes badge-breathe {
    0%, 100% { transform: scale(0.85); opacity: 0.6; }
    50% { transform: scale(1.05); opacity: 1; }
}

@keyframes badge-spin {
    to { transform: rotate(360deg); }
}

@keyframes badge-twinkle {
    0%, 100% { opacity: 0.35; scale: 0.7; }
    50% { opacity: 1; scale: 1.15; }
}

@keyframes badge-wave {
    0% { transform: scale(0.7); opacity: 1; }
    100% { transform: scale(3.4); opacity: 0; }
}

@keyframes badge-wave-inner {
    0% { transform: scale(0.8); opacity: 0.9; }
    100% { transform: scale(0.6); opacity: 0; }
}

@keyframes badge-flash {
    0% { transform: scale(1); filter: brightness(1); }
    30% { transform: scale(1.22); filter: brightness(1.9) drop-shadow(0 0 10px var(--badge-color)); }
    100% { transform: scale(1); filter: brightness(1); }
}

@media (prefers-reduced-motion: reduce) {
    .badge-halo,
    .badge-ring,
    .badge-orbit,
    .badge-spark {
        animation: none;
    }
}

.badge-aura-enter-active,
.badge-aura-leave-active {
    transition: opacity 0.25s ease;
}

.badge-aura-enter-from,
.badge-aura-leave-to {
    opacity: 0;
}
</style>
