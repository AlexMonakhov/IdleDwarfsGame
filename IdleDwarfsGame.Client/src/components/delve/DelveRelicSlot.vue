<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import { RELIC_ICONS, relicAccent, relicColors } from './delveRelics';

// Полученная реликвия — квадратная мини-карточка, как слоты способностей у дворфов отряда.
// Число копий — в правом верхнем углу, название и эффект — в подсказке. Редкая — в золотой оправе
const props = defineProps<{
    relicKey: string;
    count: number;
    rare?: boolean;
}>();

const { t } = useI18n();
const accent = computed(() => relicAccent(props.relicKey));
const icon = computed(() => RELIC_ICONS[props.relicKey]);
const colors = computed(() => relicColors(props.relicKey));
</script>

<template>
    <div class="relic-slot-cell">
        <TooltipComponent :accent="accent">
            <div class="relic-slot" :class="{ 'relic-slot-rare': rare }" :style="{ '--relic-accent': accent }">
                <component :is="icon" v-if="icon" size="30px" :primary-color="colors.light" :secondary-color="colors.dark" />
                <span v-else>💠</span>
                <span v-if="count > 1" class="relic-slot-count">×{{ count }}</span>
            </div>
            <template #title>{{ t(`social.delve.relicNames.${relicKey}`) }}<template v-if="count > 1"> ×{{ count }}</template></template>
            <template #subtitle>{{ rare ? t('social.delve.relicRare') : t('social.delve.relicKind') }}</template>
            <template #description>{{ t(`social.delve.relicHints.${relicKey}`) }}</template>
        </TooltipComponent>
    </div>
</template>

<style scoped>
/* Как .delve-slot в DelveSlotCard, только чуть меньше: их бывает много */
.relic-slot-cell {
    width: 44px;
    height: 44px;
}

.relic-slot-cell > :deep(*) {
    display: block;
    width: 100%;
    height: 100%;
}

.relic-slot {
    position: relative;
    box-sizing: border-box;
    width: 44px;
    height: 44px;
    display: grid;
    place-items: center;
    border-radius: 9px;
    border: 3px solid transparent;
    /* Тёмное дно и яркое пятно цвета реликвии под значком — светлая половина значка читается на нём */
    background:
        radial-gradient(circle at 50% 45%, color-mix(in srgb, var(--relic-accent) 70%, transparent) 0%, color-mix(in srgb, var(--relic-accent) 25%, #0a0e18) 55%, #070a12 100%) padding-box,
        var(--metalic-background) border-box;
    box-shadow:
        0 2px 3px 1px rgba(0, 0, 0, 0.7),
        0 0 0 1px color-mix(in srgb, var(--relic-accent) 80%, transparent),
        inset 0 0 6px rgba(0, 0, 0, 0.8);
    cursor: help;
    transition: transform 0.15s ease;
}

/* Редкая: золотая оправа и тёплое свечение */
.relic-slot-rare {
    background:
        radial-gradient(circle at 50% 45%, color-mix(in srgb, var(--relic-accent) 70%, transparent) 0%, color-mix(in srgb, var(--relic-accent) 25%, #0a0e18) 55%, #070a12 100%) padding-box,
        linear-gradient(145deg, #fef08a, #ca8a04 55%, #fde047) border-box;
    box-shadow:
        0 2px 3px 1px rgba(0, 0, 0, 0.7),
        0 0 8px 1px rgba(250, 204, 21, 0.55),
        inset 0 0 6px rgba(0, 0, 0, 0.8);
}

.relic-slot:hover {
    transform: translateY(-2px);
}

.relic-slot > svg {
    filter: drop-shadow(2px 2px 1px #000);
}

.relic-slot-count {
    position: absolute;
    top: -7px;
    right: -8px;
    min-width: 18px;
    height: 18px;
    padding: 0 3px;
    box-sizing: border-box;
    display: grid;
    place-items: center;
    border-radius: 9px;
    background: #1e1b10;
    border: 1px solid #ffd166;
    color: #ffd166;
    font-family: 'Arial', sans-serif;
    font-size: 10px;
    font-weight: bold;
    line-height: 1;
}
</style>
