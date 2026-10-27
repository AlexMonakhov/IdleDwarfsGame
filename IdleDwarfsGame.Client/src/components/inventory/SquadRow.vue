<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import HeroCard from '@/components/inventory/HeroCard.vue';
import { HeroInfo } from '@/models/types/Heroes';

// Ряд из SQUAD_SIZE слотов отряда. В окне отряда — с перетаскиванием (события наружу, состав меняет родитель);
// с readonly — только показ, клик по слоту отдаётся родителю (например, чтобы открыть окно отряда)
const props = defineProps<{
    // heroes[i] — герой на месте i + 1, null — место пустое
    heroes: (HeroInfo | null)[];
    readonly?: boolean;
    hoverSlot?: number | null;
    draggingId?: string | null;
}>();

const emit = defineEmits<{
    (e: 'slot-dragover', index: number): void;
    (e: 'slot-dragleave'): void;
    (e: 'slot-drop', index: number): void;
    (e: 'hero-dragstart', event: DragEvent, heroId: string, index: number): void;
    (e: 'hero-dragend'): void;
    (e: 'slot-click', index: number): void;
}>();

const { t } = useI18n();

const onDragOver = (index: number) => {
    if (!props.readonly) emit('slot-dragover', index);
};

const onDrop = (index: number) => {
    if (!props.readonly) emit('slot-drop', index);
};
</script>

<template>
    <!-- Все слоты всегда в одну линию — порядок слотов это порядок строя -->
    <div class="squad-row">
        <div
            v-for="(hero, index) in heroes"
            :key="index"
            class="squad-slot"
            :class="{
                'squad-slot-hover': hoverSlot === index,
                'squad-slot-empty': !hero,
                'squad-slot-readonly': readonly,
            }"
            @dragover.prevent="onDragOver(index)"
            @dragleave="!readonly && emit('slot-dragleave')"
            @drop.prevent="onDrop(index)"
            @click="readonly && emit('slot-click', index)"
        >
            <span class="squad-slot-number">{{ index + 1 }}</span>
            <HeroCard
                v-if="hero"
                :hero="hero"
                :selectable="readonly"
                :dragging="draggingId === hero.id"
                @dragstart="emit('hero-dragstart', $event, hero.id, index)"
                @dragend="emit('hero-dragend')"
                @click="!readonly && emit('slot-click', index)"
            />
            <span v-else class="squad-slot-placeholder">{{ t('inventory.emptySlot') }}</span>
        </div>
    </div>
</template>

<style scoped>
.squad-row {
    display: flex;
    justify-content: center;
    flex-wrap: nowrap;
    gap: 10px;
}

.squad-slot {
    position: relative;
    box-sizing: border-box;
    width: 148px;
    min-height: 196px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 12px;
    background: rgba(0, 0, 0, 0.35);
    border: 2px solid rgba(157, 173, 175, 0.5);
    box-shadow: inset 0 0 10px #000;
    transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.squad-slot-empty {
    border-style: dashed;
}

.squad-slot-hover {
    border-color: var(--yellow-gem-color);
    box-shadow: inset 0 0 10px #000, 0 0 12px 2px var(--yellow-gem-color);
}

/* Только показ: весь слот кликабельный */
.squad-slot-readonly {
    cursor: pointer;
}

.squad-slot-readonly:hover {
    border-color: var(--yellow-gem-color);
}

.squad-slot-number {
    position: absolute;
    top: -10px;
    left: -10px;
    z-index: 1;
    width: 22px;
    height: 22px;
    border-radius: 50%;
    background: var(--wood-background);
    border: 1px solid rgb(157, 173, 175);
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 12px;
    font-weight: bold;
    line-height: 22px;
    text-align: center;
}

.squad-slot-placeholder {
    color: rgba(203, 213, 225, 0.6);
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    text-transform: uppercase;
}
</style>
