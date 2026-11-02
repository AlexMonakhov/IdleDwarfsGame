<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import { HeroInfo } from '@/models/types/Heroes';
import { EquipmentInfo } from '@/models/types/Equipment';
import { ELEMENT_ICONS, RARITY_COLORS } from '@/components/inventory/lootStyles';
import { effectActive, effectText, itemName, itemStatLines } from './equipmentText';

// Подробности о предмете поверх общей подсказки. Дефолтный слот — то, на что наводят
const props = defineProps<{
    item: EquipmentInfo;
    // Выбранный герой: эффекты, которым у него нечего усиливать, приглушены
    hero?: HeroInfo | null;
    ownerName?: string | null;
}>();

const { t } = useI18n();

const accent = computed(() => RARITY_COLORS[props.item.rarity]);
</script>

<template>
    <TooltipComponent :accent="accent">
        <slot />

        <template #title>{{ itemName(item) }}</template>
        <template #subtitle>{{ t(`rarity.${item.rarity}`) }} · {{ t(`equipment.types.${item.type}`) }}</template>
        <template #description>
            <div class="item-tooltip">
                <div v-if="item.elements.length" class="item-tooltip-elements">
                    {{ t('equipment.onlyFor') }}
                    <span v-for="element in item.elements" :key="element" class="item-tooltip-element">
                        <img v-if="ELEMENT_ICONS[element]" :src="ELEMENT_ICONS[element]" alt="" />
                        {{ t(`elements.${element}`) }}
                    </span>
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
            </div>
        </template>
        <template v-if="ownerName" #stats>{{ t('equipment.wornBy', { name: ownerName }) }}</template>
    </TooltipComponent>
</template>

<style scoped>
.item-tooltip {
    width: 224px;
    display: flex;
    flex-direction: column;
    row-gap: 4px;
}

.item-tooltip-elements {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    column-gap: 6px;
    font-size: 12px;
    font-weight: bold;
}

.item-tooltip-element {
    display: inline-flex;
    align-items: center;
    column-gap: 3px;
}

.item-tooltip-element img {
    width: 16px;
    height: 16px;
}

.item-tooltip-stat {
    display: flex;
    justify-content: space-between;
}

.item-tooltip-value {
    color: #bbf7d0;
    font-weight: bold;
}

.item-tooltip-effect {
    color: #fef3c7;
}

/* Яркий текст с обводкой приглушаем прозрачностью, а не цветом: цвет потерялся бы на цветном фоне */
.item-tooltip-effect-idle {
    color: #f8fafc;
    opacity: 0.6;
}
</style>
