<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import SquadRow from '@/components/inventory/SquadRow.vue';
import { IconExpand } from '@/components/icons/ui';
import type { PortraitUnit } from '@/battle/types';
import { HeroInfo } from '@/models/types/Heroes';

// Блок отряда в окнах (гильдия, арена): заголовок, справа действия, под ним сам отряд.
// expanded — слоты с карточками героев, как в окне отряда; свёрнутый — ряд портретов, передняя линия справа.
// Развернуть можно, только если переданы герои по слотам: у закреплённого снимка арены есть лишь бойцы (units)
const props = withDefaults(defineProps<{
    title: string;
    // heroes[i] — герой на месте i + 1, null — место пустое
    heroes?: (HeroInfo | null)[] | null;
    // Бойцы для свёрнутого вида, уже в порядке показа (например, с сервера арены); без них — портреты из heroes
    units?: PortraitUnit[] | null;
    portraitSize?: number;
    // v-model:expanded
    expanded?: boolean;
}>(), {
    heroes: null,
    units: null,
    portraitSize: 44,
    expanded: false,
});

const emit = defineEmits<{
    (e: 'update:expanded', value: boolean): void;
    (e: 'slot-click', index: number): void;
}>();

const expanded = computed({
    get: () => props.expanded,
    set: value => emit('update:expanded', value),
});

const { t } = useI18n();

const expandable = computed(() => !!props.heroes);
const showExpanded = computed(() => expanded.value && expandable.value);

// Как на арене: передняя линия (место 1) ближе к центру, то есть справа
const portraits = computed<PortraitUnit[]>(() => {
    if (props.units) return props.units;
    return (props.heroes ?? [])
        .filter((hero): hero is HeroInfo => !!hero)
        .sort((a, b) => (b.placeInSquad ?? 0) - (a.placeInSquad ?? 0));
});
</script>

<template>
    <section class="squad-summary" :class="{ 'squad-summary-expanded': showExpanded }">
        <div class="squad-summary-heading">
            <span class="squad-summary-title">{{ title }}</span>
            <!-- Мощь, ошибки, рейтинг — сразу за заголовком -->
            <slot name="info" />
            <!-- Действия справа; кнопка «развернуть» — рядом с ними (обычно рядом с «Изменить отряд») -->
            <div v-if="$slots.actions || expandable" class="squad-summary-actions">
                <slot name="actions" />
                <GemButton
                    v-if="expandable"
                    :color="'blue'"
                    class="squad-summary-toggle"
                    :aria-label="expanded ? t('squad.collapse') : t('squad.expand')"
                    :aria-expanded="expanded"
                    v-tooltip="expanded ? t('squad.collapse') : t('squad.expand')"
                    @click="expanded = !expanded"
                >
                    <IconExpand class="squad-summary-toggle-icon" :class="{ 'squad-summary-toggle-icon-open': expanded }" />
                </GemButton>
            </div>
        </div>

        <!-- Тот же ряд слотов, что в окне отряда; клик по слоту отдаётся родителю -->
        <SquadRow v-if="showExpanded" :heroes="heroes ?? []" readonly @slot-click="emit('slot-click', $event)" />
        <div v-else-if="portraits.length" class="squad-summary-team">
            <UnitPortrait v-for="(unit, index) in portraits" :key="index" :unit="unit" :size="portraitSize" />
        </div>

        <slot />
    </section>
</template>

<style scoped>
.squad-summary {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
}

/* Под номерами слотов, которые выступают над рядом */
.squad-summary-expanded {
    row-gap: 18px;
}

.squad-summary-heading {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    column-gap: 14px;
    row-gap: 6px;
}

.squad-summary-toggle {
    flex-shrink: 0;
}

.squad-summary-toggle-icon {
    transition: transform 0.2s ease;
}

.squad-summary-toggle-icon-open {
    transform: rotate(180deg);
}

.squad-summary-title {
    color: #f8fafc;
    font-size: 14px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.squad-summary-actions {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    column-gap: 8px;
    margin-left: auto;
}

.squad-summary-team {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}
</style>
