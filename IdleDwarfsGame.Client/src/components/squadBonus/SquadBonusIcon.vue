<script lang="ts" setup>
import { computed, useId } from 'vue';
import { BONUS_ELEMENTS, BonusElement, SquadBonus } from '@/battle/squadBonuses';
import { ELEMENT_GLYPHS } from './elementGlyphs';
import { PRISM_COLOR, bonusElementColor } from './squadBonusText';

// Иконка бонуса стихий отряда — своя у каждого из 29 бонусов, собирается из силуэтов стихий:
//   моно  — щит цвета стихии, в нём её знак; под знаком ромбики уровня (I–III), если showTier;
//   пара  — щит разрезан по диагонали на цвета двух стихий, знаки обеих — в своих половинах;
//   призма — витраж из семи клиньев всех стихий с белым кристаллом в центре.
// Размер по умолчанию — под шрифт, как у остальных иконок
const props = withDefaults(defineProps<{
    bonus: SquadBonus;
    size?: string;
    showTier?: boolean;
    // Бонус пока не собран (подсказка): иконка приглушена
    dim?: boolean;
}>(), {
    size: '1.6em',
    showTier: true,
    dim: false,
});

const uid = useId();
const ids = {
    shield: `sb-shield-${uid}`,
    first: `sb-first-${uid}`,
    second: `sb-second-${uid}`,
    glow: `sb-glow-${uid}`,
};

// Щит-шестигранник в квадрате 0..64
const SHIELD = 'M32 3 L57 15.5 L57 46 L32 61 L7 46 L7 15.5 Z';

const colors = computed(() => props.bonus.elements.map(bonusElementColor));
const mainColor = computed(() => colors.value[0] ?? PRISM_COLOR);
const secondColor = computed(() => colors.value[1] ?? mainColor.value);

// Знак стихии: квадрат 100 → size px с левым верхним углом в (x, y)
const glyphTransform = (x: number, y: number, size: number) => `translate(${x} ${y}) scale(${size / 100})`;

interface Glyph {
    element: BonusElement;
    color: string;
    transform: string;
}

const glyphs = computed<Glyph[]>(() => {
    const [a, b] = props.bonus.elements;
    if (props.bonus.shape === 'Mono' && a) {
        // С ромбиками уровня знак чуть выше
        const top = props.showTier ? 11 : 14;
        return [{ element: a, color: colors.value[0], transform: glyphTransform(17, top, 30) }];
    }
    if (props.bonus.shape === 'Pair' && a && b) {
        return [
            { element: a, color: colors.value[0], transform: glyphTransform(10, 12, 24) },
            { element: b, color: colors.value[1], transform: glyphTransform(30, 28, 24) },
        ];
    }
    return [];
});

// Призма: семь клиньев вокруг центра щита
const PRISM_CENTER = { x: 32, y: 32 };
const PRISM_RADIUS = 21;
const prismWedges = computed(() => BONUS_ELEMENTS.map((element, i) => {
    const angle = (index: number) => (index / BONUS_ELEMENTS.length) * Math.PI * 2 - Math.PI / 2;
    const point = (index: number) => {
        const a = angle(index);
        return `${(PRISM_CENTER.x + Math.cos(a) * PRISM_RADIUS).toFixed(2)} ${(PRISM_CENTER.y + Math.sin(a) * PRISM_RADIUS).toFixed(2)}`;
    };
    return { element, color: bonusElementColor(element), d: `M${PRISM_CENTER.x} ${PRISM_CENTER.y} L${point(i)} L${point(i + 1)} Z` };
}));

const tierPips = computed(() => {
    if (props.bonus.shape !== 'Mono' || !props.showTier) return [];
    const count = props.bonus.tier;
    const gap = 7;
    const start = 32 - ((count - 1) * gap) / 2;
    return Array.from({ length: count }, (_, i) => start + i * gap);
});
</script>

<template>
    <svg
        class="squad-bonus-icon"
        :class="{ 'squad-bonus-icon-dim': dim }"
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 64 64"
        :width="size"
        :height="size"
        role="img"
    >
        <defs>
            <clipPath :id="ids.shield"><path :d="SHIELD" /></clipPath>
            <!-- Пара: верхний левый треугольник — первая стихия, нижний правый — вторая -->
            <clipPath :id="ids.first"><polygon points="0,0 64,0 0,64" /></clipPath>
            <clipPath :id="ids.second"><polygon points="64,0 64,64 0,64" /></clipPath>
            <radialGradient :id="ids.glow" cx="50%" cy="40%" r="60%">
                <stop offset="0%" stop-color="#fff" stop-opacity="0.22" />
                <stop offset="100%" stop-color="#fff" stop-opacity="0" />
            </radialGradient>
        </defs>

        <!-- Тёмное поле щита, подкрашенное цветом стихии (у пары — двумя половинами) -->
        <path :d="SHIELD" fill="#101820" />
        <g :clip-path="`url(#${ids.shield})`">
            <template v-if="bonus.shape === 'Pair'">
                <rect :clip-path="`url(#${ids.first})`" x="0" y="0" width="64" height="64" :fill="mainColor" fill-opacity="0.3" />
                <rect :clip-path="`url(#${ids.second})`" x="0" y="0" width="64" height="64" :fill="secondColor" fill-opacity="0.3" />
                <line x1="64" y1="0" x2="0" y2="64" stroke="#0b1016" stroke-width="2" />
            </template>
            <rect v-else-if="bonus.shape === 'Mono'" x="0" y="0" width="64" height="64" :fill="mainColor" fill-opacity="0.28" />
            <template v-else>
                <path v-for="wedge in prismWedges" :key="wedge.element" :d="wedge.d" :fill="wedge.color" fill-opacity="0.85" stroke="#101820" stroke-width="1.4" stroke-linejoin="round" />
            </template>
            <rect x="0" y="0" width="64" height="64" :fill="`url(#${ids.glow})`" />
        </g>

        <!-- Обод: у пары каждая половина своего цвета -->
        <template v-if="bonus.shape === 'Pair'">
            <path :clip-path="`url(#${ids.first})`" :d="SHIELD" fill="none" :stroke="mainColor" stroke-width="3" stroke-linejoin="round" />
            <path :clip-path="`url(#${ids.second})`" :d="SHIELD" fill="none" :stroke="secondColor" stroke-width="3" stroke-linejoin="round" />
        </template>
        <path v-else :d="SHIELD" fill="none" :stroke="mainColor" stroke-width="3" stroke-linejoin="round" />

        <!-- Знаки стихий с тёмной обводкой, чтобы читались на своём цвете -->
        <g v-for="glyph in glyphs" :key="glyph.element" :transform="glyph.transform">
            <template v-for="(part, index) in ELEMENT_GLYPHS[glyph.element]" :key="index">
                <path
                    v-for="angle in part.rotate ?? [0]"
                    :key="angle"
                    :d="part.d"
                    :transform="angle ? `rotate(${angle} 50 50)` : undefined"
                    :fill="glyph.color"
                    fill-rule="evenodd"
                    stroke="rgba(0, 0, 0, 0.6)"
                    stroke-width="6"
                    stroke-linejoin="round"
                    paint-order="stroke"
                />
            </template>
        </g>

        <!-- Призма: белый кристалл в центре витража -->
        <path
            v-if="bonus.shape === 'Prism'"
            d="M32 21 L40 32 L32 43 L24 32 Z"
            fill="#ffffff"
            stroke="rgba(0, 0, 0, 0.55)"
            stroke-width="2"
            stroke-linejoin="round"
            paint-order="stroke"
        />

        <!-- Уровень моно-бонуса -->
        <path
            v-for="x in tierPips"
            :key="x"
            :d="`M${x} 46.5 L${x + 3} 50 L${x} 53.5 L${x - 3} 50 Z`"
            fill="#ffd166"
            stroke="rgba(0, 0, 0, 0.6)"
            stroke-width="1.2"
            paint-order="stroke"
        />
    </svg>
</template>

<style scoped>
.squad-bonus-icon {
    flex-shrink: 0;
    vertical-align: -0.35em;
    filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.6));
}

.squad-bonus-icon-dim {
    opacity: 0.55;
    filter: grayscale(0.4) drop-shadow(0 1px 2px rgba(0, 0, 0, 0.6));
}
</style>
