<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { HeroElement } from '@/battle/types';
import type { HeroInfo } from '@/models/types/Heroes';
import FilterRow from '@/components/common/FilterRow.vue';
import type { FilterOption } from '@/components/common/filterOptions';
import { ELEMENT_COLORS, ELEMENT_ICONS } from './lootStyles';

// Фильтры списка героев: стихия и ранг (hero.level). Показываются только те, что есть в списке, с числом героев
// с учётом второго фильтра. Состояние — снаружи (useHeroFilters): v-model:element и v-model:rank, null — «все»
const props = defineProps<{
    heroes: readonly HeroInfo[];
    element: HeroElement | null;
    rank: number | null;
    // inline — оба ряда в одну строку, прижаты вправо (в шапке рядом с заголовком)
    inline?: boolean;
}>();
const emit = defineEmits<{
    (e: 'update:element', value: HeroElement | null): void;
    (e: 'update:rank', value: number | null): void;
}>();

const { t } = useI18n();

const ELEMENT_ORDER: HeroElement[] = ['Basic', 'Fire', 'Frost', 'Lightning', 'Nature', 'Dark', 'Radiance', 'Blood'];

const elements = computed<FilterOption<HeroElement>[]>(() => ELEMENT_ORDER
    .filter(element => props.heroes.some(h => h.element === element))
    .map(element => ({
        value: element,
        image: ELEMENT_ICONS[element],
        label: ELEMENT_ICONS[element] ? undefined : t('heroFilters.basic'),
        color: ELEMENT_COLORS[element],
        tooltip: t(`elements.${element}`),
        count: props.heroes.filter(h => h.element === element && (props.rank === null || h.level === props.rank)).length,
    })));

const ranks = computed<FilterOption<number>[]>(() => [...new Set(props.heroes.map(h => h.level))]
    .sort((a, b) => a - b)
    .map(rank => ({
        value: rank,
        label: '★'.repeat(rank),
        tooltip: t('heroFilters.rankHint', { rank }),
        count: props.heroes.filter(h => h.level === rank && (props.element === null || h.element === props.element)).length,
    })));
</script>

<template>
    <div v-if="heroes.length" class="hero-filters" :class="{ 'hero-filters-inline': inline }">
        <FilterRow :options="elements" :model-value="element" :all-label="t('heroFilters.allElements')" @update:model-value="emit('update:element', $event as HeroElement | null)" />
        <FilterRow :options="ranks" :model-value="rank" :all-label="t('heroFilters.allRanks')" @update:model-value="emit('update:rank', $event as number | null)" />
    </div>
</template>

<style scoped>
.hero-filters {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
    margin-bottom: 6px;
}

.hero-filters-inline {
    flex-direction: row;
    flex-wrap: wrap;
    justify-content: flex-end;
    gap: 6px 14px;
    margin-bottom: 0;
}

.hero-filters-inline :deep(.filter-row) {
    justify-content: flex-end;
}
</style>
