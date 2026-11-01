<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useItemDrag } from '@/composables/UseItemDrag';
import { HeroInfo } from '@/models/types/Heroes';
import { EquipmentInfo, canWear } from '@/models/types/Equipment';
import { EQUIPMENT_ICONS, RARITY_COLORS } from '@/components/inventory/lootStyles';
import ItemTooltip from './ItemTooltip.vue';
import { itemName } from './equipmentText';

const props = defineProps<{
    item: EquipmentInfo;
    // Выбранный герой: не подходящий ему по стихии предмет приглушён
    hero?: HeroInfo | null;
    ownerName?: string | null;
    // Только значок — для слота на герое
    compact?: boolean;
    draggable?: boolean;
}>();

const { t } = useI18n();
const { start, end } = useItemDrag();

const anchor = ref<DOMRect | null>(null);

const locked = computed(() => !!props.hero && !canWear(props.item, props.hero.element));

const show = (event: MouseEvent) => {
    anchor.value = (event.currentTarget as HTMLElement).getBoundingClientRect();
};

const hide = () => {
    anchor.value = null;
};

const onDragStart = (event: DragEvent) => {
    hide();
    start(event, props.item, !!props.compact);
};
</script>

<template>
    <div
        class="item-tile"
        :class="[`item-tile-${item.rarity}`, {
            'item-tile-compact': compact,
            'item-tile-locked': locked,
            'item-tile-worn': !compact && item.heroId,
        }]"
        :style="{ '--rarity-color': RARITY_COLORS[item.rarity] }"
        :draggable="draggable"
        @dragstart="onDragStart"
        @dragend="end"
        @mouseenter="show"
        @mouseleave="hide"
    >
        <span class="item-tile-icon">{{ EQUIPMENT_ICONS[item.type] }}</span>
        <template v-if="!compact">
            <span class="item-tile-name">{{ itemName(item) }}</span>
            <span class="item-tile-rarity">{{ t(`rarity.${item.rarity}`) }}</span>
            <span v-if="ownerName" class="item-tile-owner">{{ ownerName }}</span>
        </template>
        <ItemTooltip v-if="anchor" :item="item" :anchor="anchor" :hero="hero" :owner-name="ownerName" />
    </div>
</template>

<style scoped>
.item-tile {
    position: relative;
    width: 112px;
    min-height: 104px;
    box-sizing: border-box;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 2px;
    padding: 8px 6px;
    border: 1px solid var(--rarity-color);
    border-radius: 10px;
    background: linear-gradient(180deg, rgba(14, 19, 34, 0.55), rgba(14, 19, 34, 0.85));
    box-shadow: inset 0 0 12px color-mix(in srgb, var(--rarity-color) 25%, transparent);
    font-family: 'Arial', sans-serif;
    text-align: center;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
    user-select: none;
    transition: transform 0.15s ease, filter 0.15s ease;
}

.item-tile:hover {
    transform: translateY(-2px);
    filter: brightness(1.15);
}

/* Артефакты — на красном фоне */
.item-tile-Artifact {
    background: linear-gradient(180deg, #b91c1c, #450a0a);
    box-shadow: inset 0 0 14px rgba(255, 120, 90, 0.55), 0 0 10px rgba(239, 68, 68, 0.6);
}

.item-tile-Artifact .item-tile-rarity {
    color: #fecaca;
}

.item-tile-Legendary {
    box-shadow: inset 0 0 14px color-mix(in srgb, var(--rarity-color) 40%, transparent), 0 0 8px color-mix(in srgb, var(--rarity-color) 45%, transparent);
}

/* Не подходит выбранному герою по стихии */
.item-tile-locked {
    opacity: 0.45;
    filter: grayscale(0.6);
}

.item-tile-worn::after {
    content: '✔';
    position: absolute;
    top: 4px;
    right: 6px;
    color: #86efac;
    font-size: 12px;
}

.item-tile-icon {
    font-size: 28px;
    line-height: 1.1;
    filter: drop-shadow(0 0 5px var(--rarity-color));
}

.item-tile-name {
    color: #f8fafc;
    font-size: 12px;
    font-weight: 700;
    line-height: 1.15;
}

.item-tile-rarity {
    color: var(--rarity-color);
    font-size: 11px;
}

.item-tile-owner {
    max-width: 100%;
    overflow: hidden;
    color: #a5f3fc;
    font-size: 11px;
    text-overflow: ellipsis;
    white-space: nowrap;
}

/* В слоте значок занимает весь слот */
.item-tile-compact {
    width: 100%;
    height: 100%;
    min-height: 0;
    justify-content: center;
    padding: 0;
}
</style>
