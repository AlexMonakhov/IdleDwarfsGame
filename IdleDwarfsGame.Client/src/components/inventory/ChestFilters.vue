<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import FilterRow from '@/components/common/FilterRow.vue';
import type { FilterOption } from '@/components/common/filterOptions';
import type { ChestType, Rarity } from '@/models/types/Inventory';
import { RARITY_ORDER } from '@/models/types/Forge';
import { CHEST_ICONS, RARITY_COLORS } from './lootStyles';

// Фильтры сундуков: вид (героев, гемов, снаряжения, бесконечности) и уровень. Показываются только те, что есть,
// с числом сундуков с учётом второго фильтра
const props = defineProps<{
    chests: readonly { type: ChestType; level: number }[];
    type: ChestType | null;
    level: number | null;
}>();
const emit = defineEmits<{
    (e: 'update:type', value: ChestType | null): void;
    (e: 'update:level', value: number | null): void;
}>();

const { t } = useI18n();

const TYPE_ORDER: ChestType[] = ['Hero', 'Gem', 'Equipment', 'Infinity'];

// Уровень сундука красим как редкость: 1 — обычный … 5 — легендарный
const levelColor = (level: number) => RARITY_COLORS[RARITY_ORDER[Math.min(Math.max(level, 1), 5) - 1] as Rarity];

const types = computed<FilterOption<ChestType>[]>(() => TYPE_ORDER
    .filter(type => props.chests.some(c => c.type === type))
    .map(type => ({
        value: type,
        icon: CHEST_ICONS[type],
        label: t(`chests.types.${type}`),
        count: props.chests.filter(c => c.type === type && (props.level === null || c.level === props.level)).length,
    })));

const levels = computed<FilterOption<number>[]>(() => [...new Set(props.chests.map(c => c.level))]
    .sort((a, b) => a - b)
    .map(level => ({
        value: level,
        label: t('chests.level', { level }),
        color: levelColor(level),
        count: props.chests.filter(c => c.level === level && (props.type === null || c.type === props.type)).length,
    })));
</script>

<template>
    <div v-if="chests.length" class="chest-filters">
        <FilterRow :options="types" :model-value="type" :all-label="t('lootFilters.allChests')" @update:model-value="emit('update:type', $event as ChestType | null)" />
        <FilterRow :options="levels" :model-value="level" :all-label="t('lootFilters.allLevels')" @update:model-value="emit('update:level', $event as number | null)" />
    </div>
</template>

<style scoped>
.chest-filters {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
    margin-bottom: 10px;
}
</style>
