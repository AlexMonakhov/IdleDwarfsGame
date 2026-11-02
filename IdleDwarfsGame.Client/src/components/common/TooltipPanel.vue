<script lang="ts" setup>
import { nextTick, onMounted, ref } from 'vue';

// Единый вид подсказки: его используют TooltipComponent и директива v-tooltip.
// Через Teleport: списки со скроллом обрезали бы её.
// anchor — прямоугольник того, на что навели: подсказка встаёт сбоку от него и не вылезает за экран
const props = withDefaults(defineProps<{
    anchor: DOMRect;
    // Цвет фона: стихия героя, редкость предмета. Без него — нейтральный
    accent?: string;
}>(), {
    accent: '#64748b',
});

const GAP = 10;
const MARGIN = 8;

const el = ref<HTMLElement | null>(null);
const position = ref({ left: -9999, top: -9999 });

onMounted(async () => {
    await nextTick();
    if (!el.value) return;
    const { width, height } = el.value.getBoundingClientRect();
    const fitsRight = props.anchor.right + GAP + width + MARGIN <= window.innerWidth;
    const left = fitsRight ? props.anchor.right + GAP : props.anchor.left - GAP - width;
    const top = Math.min(props.anchor.top, window.innerHeight - height - MARGIN);
    position.value = { left: Math.max(MARGIN, left), top: Math.max(MARGIN, top) };
});
</script>

<template>
    <Teleport to="body">
        <div
            ref="el"
            class="tooltip-panel"
            :style="{ left: `${position.left}px`, top: `${position.top}px`, '--tooltip-accent': accent }"
        >
            <div v-if="$slots.title" class="tooltip-panel-title"><slot name="title" /></div>
            <div v-if="$slots.subtitle" class="tooltip-panel-subtitle"><slot name="subtitle" /></div>
            <div v-if="$slots.description" class="tooltip-panel-description"><slot name="description" /></div>
            <div v-if="$slots.stats" class="tooltip-panel-stats"><slot name="stats" /></div>
        </div>
    </Teleport>
</template>

<style scoped>
.tooltip-panel {
    position: fixed;
    /* Над любым окном из стека модалок */
    z-index: 100000;
    box-sizing: border-box;
    width: max-content;
    max-width: 250px;
    padding: 10px 12px;
    display: flex;
    flex-direction: column;
    row-gap: 4px;
    border: 1px solid color-mix(in srgb, var(--tooltip-accent) 70%, white);
    border-radius: 10px;
    /* Толща кристалла, как у карточки дворфа: цвет утоплен в тёмно-синее стекло */
    background: linear-gradient(165deg,
        color-mix(in srgb, var(--tooltip-accent) 55%, #3b5272) 0%,
        color-mix(in srgb, var(--tooltip-accent) 45%, #26344a) 45%,
        color-mix(in srgb, var(--tooltip-accent) 30%, #141d2b) 100%);
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.7);
    color: #f8fafc;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    line-height: 1.3;
    /* Фон бывает любого цвета: читаемость держит обводка, а не подбор цвета под фон */
    text-shadow: var(--text-outline);
    pointer-events: none;
    animation: tooltip-panel-in 0.15s ease;
}

.tooltip-panel-title {
    font-size: 15px;
    font-weight: bold;
}

.tooltip-panel-subtitle {
    font-size: 12px;
}

.tooltip-panel-stats {
    margin-top: 2px;
    padding-top: 6px;
    border-top: 1px solid rgba(255, 255, 255, 0.35);
    font-size: 12px;
}

@keyframes tooltip-panel-in {
    from { opacity: 0; }
}
</style>
