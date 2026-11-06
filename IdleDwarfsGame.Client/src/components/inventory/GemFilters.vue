<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import FilterRow from '@/components/common/FilterRow.vue';
import type { FilterOption } from '@/components/common/filterOptions';
import type { Rarity } from '@/models/types/Inventory';
import { RARITY_ORDER } from '@/models/types/Forge';
import GemIcon from '@/components/icons/GemIcon.vue';
import { GEM_COLORS, RARITY_COLORS } from './lootStyles';

// Фильтры камней: вид камня и ступень (редкость). Показываются только те, что есть, с числом стопок
// с учётом второго фильтра. gems — стопки: камень (subType) и ступень
const props = defineProps<{
    gems: readonly { gem: string; rarity: Rarity }[];
    gem: string | null;
    rarity: Rarity | null;
}>();
const emit = defineEmits<{
    (e: 'update:gem', value: string | null): void;
    (e: 'update:rarity', value: Rarity | null): void;
}>();

const { t, te } = useI18n();

const GEM_ORDER = ['Ruby', 'Sapphire', 'Topaz', 'Emerald', 'Amethyst', 'Diamond', 'Garnet'];

const kinds = computed<FilterOption<string>[]>(() => [...new Set(props.gems.map(g => g.gem))]
    .sort((a, b) => GEM_ORDER.indexOf(a) - GEM_ORDER.indexOf(b))
    .map(gem => ({
        value: gem,
        icon: GemIcon,
        iconProps: { gem },
        color: GEM_COLORS[gem],
        tooltip: te(`gems.${gem}`) ? t(`gems.${gem}`) : gem,
        count: props.gems.filter(g => g.gem === gem && (props.rarity === null || g.rarity === props.rarity)).length,
    })));

const rarities = computed<FilterOption<Rarity>[]>(() => RARITY_ORDER
    .filter(rarity => props.gems.some(g => g.rarity === rarity))
    .map(rarity => ({
        value: rarity,
        label: t(`rarity.${rarity}`),
        color: RARITY_COLORS[rarity],
        count: props.gems.filter(g => g.rarity === rarity && (props.gem === null || g.gem === props.gem)).length,
    })));
</script>

<template>
    <div v-if="gems.length" class="gem-filters">
        <FilterRow :options="kinds" :model-value="gem" :all-label="t('lootFilters.allGems')" @update:model-value="emit('update:gem', $event as string | null)" />
        <FilterRow :options="rarities" :model-value="rarity" :all-label="t('lootFilters.allRarities')" @update:model-value="emit('update:rarity', $event as Rarity | null)" />
    </div>
</template>

<style scoped>
.gem-filters {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
    margin-bottom: 6px;
}
</style>
