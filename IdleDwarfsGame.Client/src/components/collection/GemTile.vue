<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { ItemInfo } from '@/models/types/Inventory';
import { GEM_COLORS, RARITY_COLORS } from './lootStyles';

// Стопка гемов: камень, редкость и сколько штук
const props = defineProps<{ item: ItemInfo }>();

const { t, te } = useI18n();

const name = computed(() => te(`gems.${props.item.subType}`) ? t(`gems.${props.item.subType}`) : props.item.subType);
</script>

<template>
    <div class="gem-tile" :style="{ '--gem-color': GEM_COLORS[item.subType] ?? '#cbd5e1', '--rarity-color': RARITY_COLORS[item.rarity] }">
        <span class="gem-tile-stone">◆</span>
        <span class="gem-tile-name">{{ name }}</span>
        <span class="gem-tile-rarity">{{ t(`rarity.${item.rarity}`) }}</span>
        <span class="gem-tile-quantity">×{{ item.quantity }}</span>
    </div>
</template>

<style scoped>
.gem-tile {
    width: 104px;
    box-sizing: border-box;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 2px;
    padding: 10px 6px 8px;
    border: 1px solid var(--rarity-color);
    border-radius: 10px;
    background: linear-gradient(180deg, rgba(14, 19, 34, 0.55), rgba(14, 19, 34, 0.85));
    box-shadow: inset 0 0 12px color-mix(in srgb, var(--rarity-color) 25%, transparent);
    font-family: 'Arial', sans-serif;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.gem-tile-stone {
    font-size: 30px;
    line-height: 1;
    color: var(--gem-color);
    filter: drop-shadow(0 0 6px var(--gem-color));
}

.gem-tile-name {
    color: #f8fafc;
    font-size: 13px;
    font-weight: 700;
}

.gem-tile-rarity {
    color: var(--rarity-color);
    font-size: 11px;
}

.gem-tile-quantity {
    color: #ffd166;
    font-size: 14px;
    font-weight: 700;
}
</style>
