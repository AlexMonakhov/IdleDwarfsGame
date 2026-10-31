<script lang="ts" setup>
import { nextTick, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { HeroInfo } from '@/models/types/Heroes';
import { EquipmentInfo } from '@/models/types/Equipment';
import { ELEMENT_COLORS, RARITY_COLORS } from '@/components/inventory/lootStyles';
import { effectActive, effectText, itemName, itemStatLines } from './equipmentText';

// Подробности о предмете у курсора. Через Teleport: списки со скроллом обрезали бы его.
// anchor — прямоугольник плитки: подсказка встаёт сбоку от неё и не вылезает за экран
const props = defineProps<{
    item: EquipmentInfo;
    anchor: DOMRect;
    // Выбранный герой: эффекты, которым у него нечего усиливать, приглушены
    hero?: HeroInfo | null;
    ownerName?: string | null;
}>();

const { t } = useI18n();

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
            class="item-tooltip"
            :class="`item-tooltip-${item.rarity}`"
            :style="{ left: `${position.left}px`, top: `${position.top}px`, '--rarity-color': RARITY_COLORS[item.rarity] }"
        >
            <div class="item-tooltip-name">{{ itemName(item) }}</div>
            <div class="item-tooltip-kind">{{ t(`rarity.${item.rarity}`) }} · {{ t(`equipment.types.${item.type}`) }}</div>

            <div v-if="item.elements.length" class="item-tooltip-elements">
                {{ t('equipment.onlyFor') }}
                <span v-for="element in item.elements" :key="element" :style="{ color: ELEMENT_COLORS[element] }">{{ t(`elements.${element}`) }}</span>
            </div>

            <div v-for="stat in itemStatLines(item.stats)" :key="stat.key" class="item-tooltip-stat">
                <span>{{ t(`equipment.stats.${stat.key}`) }}</span>
                <span class="item-tooltip-value">+{{ stat.value }}</span>
            </div>

            <div
                v-for="(effect, index) in item.effects"
                :key="index"
                class="item-tooltip-effect"
                :class="{ 'item-tooltip-effect-idle': hero && !effectActive(effect, hero) }"
            >{{ effectText(effect) }}</div>

            <div v-if="ownerName" class="item-tooltip-owner">{{ t('equipment.wornBy', { name: ownerName }) }}</div>
        </div>
    </Teleport>
</template>

<style scoped>
.item-tooltip {
    position: fixed;
    /* Над любым окном из стека модалок */
    z-index: 100000;
    box-sizing: border-box;
    width: 250px;
    padding: 10px 12px;
    display: flex;
    flex-direction: column;
    row-gap: 4px;
    border: 1px solid var(--rarity-color);
    border-radius: 10px;
    background: rgba(10, 14, 24, 0.97);
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.7);
    color: #e2e8f0;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    pointer-events: none;
}

.item-tooltip-Artifact {
    background: linear-gradient(180deg, rgba(90, 14, 14, 0.98), rgba(30, 6, 8, 0.98));
}

.item-tooltip-name {
    color: var(--rarity-color);
    font-size: 15px;
    font-weight: bold;
}

.item-tooltip-kind {
    color: #94a3b8;
    font-size: 12px;
}

.item-tooltip-elements {
    display: flex;
    flex-wrap: wrap;
    column-gap: 6px;
    font-size: 12px;
    font-weight: bold;
}

.item-tooltip-stat {
    display: flex;
    justify-content: space-between;
}

.item-tooltip-value {
    color: #86efac;
    font-weight: bold;
}

.item-tooltip-effect {
    color: #fde68a;
}

.item-tooltip-effect-idle {
    color: #64748b;
}

.item-tooltip-owner {
    margin-top: 2px;
    color: #a5f3fc;
    font-size: 12px;
}
</style>
