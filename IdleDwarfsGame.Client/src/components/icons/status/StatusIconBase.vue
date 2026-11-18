<script lang="ts" setup>
import { useId } from 'vue';

// Общая основа иконок статусов: монета с радиальным градиентом цвета эффекта (свет сверху слева),
// светлый обод, тёмная фаска внутри и стеклянный блик. Рисунок статуса — в слоте, в квадрате 0..100:
// по умолчанию светлая заливка с тёмной обводкой под ней (paint-order) — так знак читается и на 16px над юнитом.
// Детали рисунка свою заливку и stroke="none" задают сами
const props = withDefaults(defineProps<{
    size?: string;
    // Цвета монеты: свет в центре, основной, тёмный край, обод
    light: string;
    mid: string;
    dark: string;
    rim: string;
    // Цвет обводки знака — тёмный оттенок цвета статуса
    outline?: string;
}>(), {
    size: '16px',
    outline: 'rgba(15, 8, 4, 0.6)',
});

// id внутри svg должны быть уникальны, иначе несколько иконок на странице делят один градиент
const uid = useId();
const bgId = `status-bg-${uid}`;
const glossId = `status-gloss-${uid}`;
</script>

<template>
    <svg class="status-icon-svg" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="props.size" :height="props.size">
        <defs>
            <radialGradient :id="bgId" cx="38%" cy="32%" r="78%">
                <stop offset="0%" :stop-color="light" />
                <stop offset="55%" :stop-color="mid" />
                <stop offset="100%" :stop-color="dark" />
            </radialGradient>
            <linearGradient :id="glossId" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stop-color="#fff" stop-opacity="0.5" />
                <stop offset="100%" stop-color="#fff" stop-opacity="0" />
            </linearGradient>
        </defs>

        <!-- Тёмный край, монета с ободом, фаска и блик -->
        <circle cx="50" cy="50" r="48" :fill="dark" />
        <circle cx="50" cy="50" r="44.5" :fill="`url(#${bgId})`" :stroke="rim" stroke-width="3.5" />
        <circle cx="50" cy="50" r="39" fill="none" stroke="rgba(0, 0, 0, 0.3)" stroke-width="2" />
        <ellipse cx="50" cy="29" rx="27" ry="13" :fill="`url(#${glossId})`" />

        <!-- Знак статуса -->
        <g fill="#ffffff" :stroke="outline" stroke-width="5" stroke-linejoin="round" stroke-linecap="round" paint-order="stroke">
            <slot />
        </g>
    </svg>
</template>

<style scoped>
.status-icon-svg {
    flex-shrink: 0;
}
</style>
