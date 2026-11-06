<script lang="ts" setup>
import { computed, useId } from 'vue';
import banner from '@/assets/result-banner.webp';

// Итог боя на арене: картинка с лентой, надпись — SVG-текстом по дуге ленты (золото с тёмной обводкой).
// tone: victory — как есть; defeat — картинка приглушена, надпись красная
const props = withDefaults(defineProps<{
    title: string;
    tone?: 'victory' | 'defeat';
}>(), {
    tone: 'victory',
});

// Координаты — в пикселях исходной картинки (1670×941), viewBox SVG такой же, поэтому текст масштабируется вместе с ней.
// Середина ленты: у краёв (430, 457), в центре 420 — лента выгнута дугой вверх
const ARC_LEFT = 430;
const ARC_RIGHT = 1240;
const ARC_EDGE_Y = 457;
const ARC_MID_Y = 420;
// Сколько надписи помещается на ленте с запасом по краям, наибольший кегль и разрядка (как letter-spacing в стилях)
const TEXT_WIDTH = 700;
const MAX_FONT = 118;
const LETTER_SPACING = 0.04;
const FONT = '"Arial Black", Arial, sans-serif';

const text = computed(() => props.title.toLocaleUpperCase());

// Ширина надписи при наибольшем кегле — замером на канвасе: узкие пробел и «!» не раздувают оценку
let measureCtx: CanvasRenderingContext2D | null = null;
const measure = (value: string) => {
    if (!measureCtx) measureCtx = document.createElement('canvas').getContext('2d');
    if (!measureCtx) return value.length * MAX_FONT * 0.89;
    measureCtx.font = `900 ${MAX_FONT}px ${FONT}`;
    return measureCtx.measureText(value).width + value.length * LETTER_SPACING * MAX_FONT;
};

// Кегль — по ширине надписи: «Победа» крупно, длинные надписи мельче, но всегда в пределах ленты
const fontSize = computed(() => Math.min(MAX_FONT, (MAX_FONT * TEXT_WIDTH) / Math.max(1, measure(text.value))));

// Линия букв (baseline) ниже середины ленты на половину высоты прописных — так надпись стоит ровно посередине
const arcPath = computed(() => {
    const baseline = fontSize.value * 0.36;
    const control = 2 * ARC_MID_Y - ARC_EDGE_Y;
    return `M ${ARC_LEFT} ${ARC_EDGE_Y + baseline} Q ${(ARC_LEFT + ARC_RIGHT) / 2} ${control + baseline} ${ARC_RIGHT} ${ARC_EDGE_Y + baseline}`;
});

// id внутри svg должны быть уникальны: на странице может оказаться несколько баннеров
const uid = useId();
const arcId = `result-arc-${uid}`;
const fillId = `result-fill-${uid}`;
const glowId = `result-glow-${uid}`;
</script>

<template>
    <div class="result-banner" :class="`result-banner-${tone}`" role="img" :aria-label="title">
        <img :src="banner" class="result-banner-image" alt="" draggable="false" />
        <svg class="result-banner-text" viewBox="0 0 1670 941" aria-hidden="true">
            <defs>
                <path :id="arcId" :d="arcPath" />
                <linearGradient :id="fillId" x1="0" y1="0" x2="0" y2="1">
                    <template v-if="tone === 'defeat'">
                        <stop offset="0%" stop-color="#ffe4e4" />
                        <stop offset="45%" stop-color="#ff8f8f" />
                        <stop offset="56%" stop-color="#c81e1e" />
                        <stop offset="100%" stop-color="#ff9d9d" />
                    </template>
                    <template v-else>
                        <stop offset="0%" stop-color="#fffbe0" />
                        <stop offset="45%" stop-color="#ffd766" />
                        <stop offset="56%" stop-color="#e8961a" />
                        <stop offset="100%" stop-color="#ffe9a3" />
                    </template>
                </linearGradient>
                <!-- Мягкая тень под буквами -->
                <filter :id="glowId" x="-20%" y="-50%" width="140%" height="200%">
                    <feGaussianBlur stdDeviation="6" />
                </filter>
            </defs>

            <!-- Тень: та же надпись, сдвинута вниз и размыта -->
            <text class="result-banner-shadow" :font-size="fontSize" :filter="`url(#${glowId})`" transform="translate(0 8)">
                <textPath :href="`#${arcId}`" startOffset="50%" text-anchor="middle">{{ text }}</textPath>
            </text>
            <!-- Сама надпись: сначала толстая тёмная обводка, поверх — градиент -->
            <text class="result-banner-letters" :font-size="fontSize" :fill="`url(#${fillId})`">
                <textPath :href="`#${arcId}`" startOffset="50%" text-anchor="middle">{{ text }}</textPath>
            </text>
        </svg>
    </div>
</template>

<style scoped>
.result-banner {
    position: relative;
    width: min(660px, calc(88 * var(--vw)));
    aspect-ratio: 1670 / 941;
    animation: result-banner-in 0.55s cubic-bezier(0.2, 1.4, 0.4, 1) both;
    pointer-events: none;
}

.result-banner-image,
.result-banner-text {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
}

.result-banner-image {
    filter: drop-shadow(0 10px 18px rgba(0, 0, 0, 0.6));
    user-select: none;
}

/* Поражение: картинка тускнеет, лента теряет цвет */
.result-banner-defeat .result-banner-image {
    filter: grayscale(0.65) brightness(0.72) drop-shadow(0 10px 18px rgba(0, 0, 0, 0.6));
}

/* Шрифт и разрядка — те же, что в FONT и LETTER_SPACING: по ним считается кегль */
.result-banner-text text {
    font-family: 'Arial Black', 'Arial', sans-serif;
    font-weight: 900;
    letter-spacing: 0.04em;
}

.result-banner-shadow {
    fill: rgba(0, 0, 0, 0.6);
}

.result-banner-letters {
    stroke: #2a0d3f;
    stroke-width: 14px;
    stroke-linejoin: round;
    paint-order: stroke fill;
    animation: result-banner-glow 2.4s ease-in-out 0.55s infinite;
}

.result-banner-defeat .result-banner-letters {
    stroke: #2b0a0a;
}

/* Появление: баннер «выпрыгивает» с лёгким перелётом */
@keyframes result-banner-in {
    from { opacity: 0; transform: scale(0.6) translateY(20px); }
    to { opacity: 1; transform: none; }
}

/* Надпись мягко подсвечивается */
@keyframes result-banner-glow {
    0%, 100% { filter: drop-shadow(0 0 0 rgba(255, 214, 102, 0)); }
    50% { filter: drop-shadow(0 0 10px rgba(255, 214, 102, 0.75)); }
}

.result-banner-defeat .result-banner-letters {
    animation: none;
}

@media (prefers-reduced-motion: reduce) {
    .result-banner,
    .result-banner-letters {
        animation: none;
    }
}
</style>
