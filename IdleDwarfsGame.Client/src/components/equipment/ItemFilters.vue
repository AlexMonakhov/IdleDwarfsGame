<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import FilterRow from '@/components/common/FilterRow.vue';
import type { FilterOption } from '@/components/common/filterOptions';
import { EQUIPMENT_TYPES, type EquipmentInfo, type EquipmentType } from '@/models/types/Equipment';
import type { Rarity } from '@/models/types/Inventory';
import { RARITY_ORDER } from '@/models/types/Forge';
import { RARITY_COLORS } from '@/components/inventory/lootStyles';
import EquipmentIcon from './EquipmentIcon.vue';

// Фильтры предметов: вид (оружие, шлем, амулет, кольцо) и редкость. Число — сколько предметов с учётом второго фильтра.
// Вид показываем всегда (его выбирает и клик по слоту героя), редкости — только те, что есть
const props = defineProps<{
    items: readonly EquipmentInfo[];
    type: EquipmentType | null;
    rarity: Rarity | null;
}>();
const emit = defineEmits<{
    (e: 'update:type', value: EquipmentType | null): void;
    (e: 'update:rarity', value: Rarity | null): void;
}>();

const { t } = useI18n();

const types = computed<FilterOption<EquipmentType>[]>(() => EQUIPMENT_TYPES.map(type => ({
    value: type,
    icon: EquipmentIcon,
    iconProps: { type },
    label: t(`equipment.types.${type}`),
    count: props.items.filter(i => i.type === type && (props.rarity === null || i.rarity === props.rarity)).length,
})));

const rarities = computed<FilterOption<Rarity>[]>(() => RARITY_ORDER
    .filter(rarity => props.items.some(i => i.rarity === rarity))
    .map(rarity => ({
        value: rarity,
        label: t(`rarity.${rarity}`),
        color: RARITY_COLORS[rarity],
        count: props.items.filter(i => i.rarity === rarity && (props.type === null || i.type === props.type)).length,
    })));
</script>

<template>
    <div class="item-filters">
        <FilterRow :options="types" :model-value="type" :all-label="t('equipment.all')" @update:model-value="emit('update:type', $event as EquipmentType | null)" />
        <FilterRow
            v-if="rarities.length"
            :options="rarities"
            :model-value="rarity"
            :all-label="t('lootFilters.allRarities')"
            @update:model-value="emit('update:rarity', $event as Rarity | null)"
        />
    </div>
</template>

<style scoped>
.item-filters {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
}
</style>
