<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import SquadBonusFigure from './SquadBonusFigure.vue';
import { SquadBonus } from '@/battle/squadBonuses';
import { bonusColor, bonusDescription, bonusName } from './squadBonusText';
import { bonusComposition } from './squadBonusComposition';

// Иконка бонуса отряда с подсказкой: имя, состав и что бонус делает. Без бонуса — ничего или приглушённая «пустая» плашка (showEmpty).
// animated и pulse — живой значок арены (SquadBonusFigure)
const props = withDefaults(defineProps<{
    bonus: SquadBonus | null;
    size?: string;
    // Имя бонуса рядом с иконкой
    label?: boolean;
    showEmpty?: boolean;
    animated?: boolean;
    pulse?: number;
}>(), {
    size: '40px',
    label: false,
    showEmpty: false,
    animated: false,
    pulse: 0,
});

const { t } = useI18n();

const color = computed(() => (props.bonus ? bonusColor(props.bonus) : '#64748b'));
</script>

<template>
    <TooltipComponent v-if="bonus" class="squad-bonus-badge" :accent="color">
        <span class="squad-bonus-badge-body">
            <SquadBonusFigure :bonus="bonus" :size="size" :animated="animated" :pulse="pulse" />
            <span v-if="label" class="squad-bonus-badge-name" :style="{ color }">{{ bonusName(bonus) }}</span>
        </span>
        <template #title>{{ bonusName(bonus) }}</template>
        <template #subtitle>{{ t(`squadBonuses.shapes.${bonus.shape}`) }} · {{ bonusComposition(bonus) }}</template>
        <template #description>{{ bonusDescription(bonus) }}</template>
    </TooltipComponent>
    <TooltipComponent v-else-if="showEmpty" class="squad-bonus-badge squad-bonus-badge-empty">
        <span class="squad-bonus-badge-body">
            <span class="squad-bonus-badge-placeholder" :style="{ width: size, height: size }">?</span>
            <span v-if="label" class="squad-bonus-badge-name">{{ t('squadBonuses.none') }}</span>
        </span>
        <template #title>{{ t('squadBonuses.none') }}</template>
        <template #description>{{ t('squadBonuses.noneHint') }}</template>
    </TooltipComponent>
</template>

<style scoped>
.squad-bonus-badge-body {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    cursor: help;
}

.squad-bonus-badge-name {
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    font-weight: bold;
    white-space: nowrap;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.85);
}

.squad-bonus-badge-empty .squad-bonus-badge-name {
    color: #94a3b8;
}

.squad-bonus-badge-placeholder {
    box-sizing: border-box;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    clip-path: polygon(50% 4%, 89% 24%, 89% 72%, 50% 96%, 11% 72%, 11% 24%);
    background: rgba(100, 116, 139, 0.45);
    color: #cbd5e1;
    font-family: 'Arial', sans-serif;
    font-weight: bold;
}
</style>
