<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import type { DelveNodeType } from '@/models/types/Social';
import { NODE_ICONS, nodeStyle } from './delveMap';

// Узел карты спуска — деревянная табличка на цепях, как вывеска в шахте: иконка и название.
// Доступная покачивается и обведена жёлтым, у босса — золотая окантовка, через этаж — маленькая без названия.
// Без type — неизвестный узел: тёмная доска со знаком «?»
withDefaults(defineProps<{
    type?: DelveNodeType;
    small?: boolean;
    reachable?: boolean;
    far?: boolean;
}>(), { type: undefined, small: false, reachable: false, far: false });

const { t } = useI18n();
</script>

<template>
    <span
        class="node"
        :data-sound-hover="reachable ? 'ui_hover_town' : undefined"
        :class="[type ? `node-type-${type.toLowerCase()}` : 'node-unknown', { 'node-small': small, 'node-reachable': reachable, 'node-far': far }]"
        :style="type ? nodeStyle(type) : undefined"
    >
        <span class="node-board">
            <component :is="NODE_ICONS[type]" v-if="type" size="20px" />
            <span v-else class="node-question">?</span>
            <span v-if="!small" class="node-name">{{ t(`social.delve.nodes.${type ?? 'Unknown'}`) }}</span>
        </span>
    </span>
</template>

<style scoped>
.node {
    --node-color: #94a3b8;
    position: relative;
    display: inline-flex;
    flex-direction: column;
    align-items: center;
    color: #fff;
    font-family: 'Arial', sans-serif;
    white-space: nowrap;
    /* Место под цепи над доской */
    padding-top: 10px;
    transition: transform 0.15s ease, opacity 0.15s ease, filter 0.15s ease;
}

.node :deep(svg) {
    filter: drop-shadow(2px 2px 1px #000);
}

.node-name {
    font-size: 12px;
    font-weight: bold;
    text-shadow: 1px 1px 0 #000;
}

.node-far {
    opacity: 0.4;
    filter: grayscale(0.5);
}

.node-small {
    transform: scale(0.8);
}

/* Две цепи, на которых висит доска */
.node::before,
.node::after {
    content: '';
    position: absolute;
    top: 0;
    width: 2px;
    height: 12px;
    background: repeating-linear-gradient(to bottom, #9ca3af 0 3px, #4b5563 3px 4px);
}

.node::before { left: 22%; }
.node::after { right: 22%; }

/* Доска: волокна дерева полосами, по углам гвозди */
.node-board {
    position: relative;
    box-sizing: border-box;
    height: 30px;
    display: flex;
    align-items: center;
    gap: 5px;
    padding: 0 10px;
    border-radius: 4px;
    background:
        radial-gradient(circle at 6px 6px, #d1d5db 1.5px, transparent 2px),
        radial-gradient(circle at calc(100% - 6px) 6px, #d1d5db 1.5px, transparent 2px),
        repeating-linear-gradient(to bottom, rgba(0, 0, 0, 0.12) 0 2px, transparent 2px 6px),
        linear-gradient(to bottom, #8b5a2b, #5c3a1a);
    box-shadow:
        inset 0 0 0 2px #3b2410,
        inset 0 -3px 0 rgba(0, 0, 0, 0.3),
        0 0 0 1px color-mix(in srgb, var(--node-color) 70%, transparent),
        0 4px 6px rgba(0, 0, 0, 0.7);
}

.node-small .node-board {
    width: 32px;
    padding: 0;
    justify-content: center;
}

.node-type-boss .node-board {
    box-shadow: inset 0 0 0 2px #facc15, inset 0 -3px 0 rgba(0, 0, 0, 0.3), 0 0 12px rgba(250, 204, 21, 0.6), 0 4px 6px rgba(0, 0, 0, 0.7);
}

/* Неизвестный узел: доска темнее, знак вопроса выжжен на ней */
.node-unknown {
    opacity: 0.75;
}

.node-unknown .node-board {
    background:
        radial-gradient(circle at 6px 6px, #6b7280 1.5px, transparent 2px),
        radial-gradient(circle at calc(100% - 6px) 6px, #6b7280 1.5px, transparent 2px),
        repeating-linear-gradient(to bottom, rgba(0, 0, 0, 0.15) 0 2px, transparent 2px 6px),
        linear-gradient(to bottom, #5c3d22, #3a2412);
}

.node-question {
    width: 20px;
    color: #e7d3b5;
    font-size: 18px;
    font-weight: 900;
    line-height: 1;
    text-align: center;
    text-shadow: 2px 2px 1px #000;
}

.node-reachable {
    animation: node-swing 2.2s ease-in-out infinite;
    transform-origin: 50% 0;
}

.node-reachable .node-board {
    outline: 2px solid #fde047;
    outline-offset: 1px;
}

@keyframes node-swing {
    0%, 100% { transform: rotate(-3deg); }
    50% { transform: rotate(3deg); }
}
</style>
