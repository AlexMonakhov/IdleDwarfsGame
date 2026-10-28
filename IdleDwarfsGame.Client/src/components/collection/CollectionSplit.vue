<script lang="ts" setup>
import PreviewStage from './PreviewStage.vue';
import { CollectionPreview } from './previewScenes';
import { vDragScroll } from '@/directives/dragScroll';

// Раскладка вкладки «Коллекции», как в окне прокачки: справа список, по клику слева выезжает панель
// со сценой превью и описанием выбранного
defineProps<{
    hint: string;
    preview: CollectionPreview | null;
}>();
</script>

<template>
    <div class="collection-split" :class="{ 'collection-split-open': preview }">
        <Transition name="collection-panel">
            <div v-if="preview" class="collection-panel">
                <div class="collection-panel-stage">
                    <PreviewStage
                        :key="preview.key"
                        :scene="preview.scene"
                        :element="preview.element"
                        :aura-pulse="preview.auraPulse"
                    />
                </div>
                <div class="collection-panel-info">
                    <div class="collection-panel-header">
                        <h4 class="collection-panel-title" :style="{ color: preview.color }">{{ preview.title }}</h4>
                        <slot name="switcher" />
                    </div>
                    <p v-if="preview.description" class="collection-panel-description">{{ preview.description }}</p>
                    <div v-if="$slots.tags" class="collection-panel-tags">
                        <slot name="tags" />
                    </div>
                </div>
            </div>
        </Transition>

        <section v-drag-scroll class="collection-list">
            <div class="collection-hint">{{ hint }}</div>
            <div class="collection-grid">
                <slot />
            </div>
        </section>
    </div>
</template>

<style scoped>
.collection-split {
    flex: 1;
    min-width: 0;
    display: flex;
    column-gap: 14px;
    font-family: 'Arial', sans-serif;
}

.collection-panel {
    flex: 0 1 560px;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 12px;
}

.collection-panel-stage {
    flex: 1;
    min-height: 320px;
}

.collection-panel-info {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    padding: 12px 16px;
    border-radius: 12px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.7);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.collection-panel-header {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
}

.collection-panel-title {
    margin: 0;
    color: #f8fafc;
    font-size: 18px;
    letter-spacing: 1px;
    text-transform: uppercase;
    text-shadow: 0 0 8px currentColor, 1px 1px 0 rgba(0, 0, 0, 0.9);
}

.collection-panel-description {
    margin: 0;
    color: #e2e8f0;
    font-size: 13px;
    line-height: 1.4;
}

.collection-panel-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.collection-list {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    overflow-y: auto;
}

/* С открытой панелью список не уже одной колонки карточек */
.collection-split-open .collection-list {
    min-width: 290px;
}

.collection-hint {
    margin-bottom: 10px;
    text-align: center;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.collection-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-content: flex-start;
    gap: 10px;
    padding: 4px;
}

/* Панель выезжает слева */
.collection-panel-enter-active {
    transition: transform 0.3s cubic-bezier(0.34, 1.3, 0.64, 1), opacity 0.25s ease;
}

.collection-panel-leave-active {
    display: none;
}

.collection-panel-enter-from {
    opacity: 0;
    transform: translateX(-40px);
}
</style>

<!-- Общие элементы вкладок: их разметка приходит через слоты, поэтому стили не scoped -->
<style>
/* Плитка списка: значок, название, пояснение */
.collection-tile {
    box-sizing: border-box;
    width: 210px;
    display: flex;
    align-items: center;
    column-gap: 10px;
    padding: 10px 12px;
    border-radius: 12px;
    border: 2px solid rgba(157, 173, 175, 0.45);
    background: rgba(15, 23, 32, 0.78);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.45);
    color: #f8fafc;
    font-family: 'Arial', sans-serif;
    text-align: left;
    cursor: pointer;
    transition: transform 0.2s cubic-bezier(0.34, 1.4, 0.64, 1), border-color 0.2s ease, box-shadow 0.2s ease;
}

.collection-tile:hover {
    transform: translateY(-2px);
    border-color: var(--tile-accent, #ffd166);
    box-shadow: 0 4px 14px color-mix(in srgb, var(--tile-accent, #ffd166) 45%, transparent);
}

.collection-tile-selected,
.collection-tile-selected:hover {
    border-color: #ffd166;
    box-shadow: 0 0 0 1px #ffd166, 0 0 16px 3px rgba(255, 209, 102, 0.55);
}

.collection-tile-text {
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 2px;
}

.collection-tile-name {
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.collection-tile-note {
    color: #cbd5e1;
    font-size: 12px;
}

/* Метка в описании: тип урона, статус, лечение */
.collection-chip {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    padding: 2px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.6);
    color: var(--chip-color, #e2e8f0);
    font-family: 'Arial', sans-serif;
    font-size: 12px;
    font-weight: bold;
}

.collection-chip-label {
    color: #cbd5e1;
    font-weight: normal;
}
</style>
