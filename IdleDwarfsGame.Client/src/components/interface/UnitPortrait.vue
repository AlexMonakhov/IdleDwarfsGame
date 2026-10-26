<script lang="ts" setup>
import { computed } from 'vue';
import { PortraitUnit } from '@/battle/types';
import { powerParts } from '@/battle/power';
import { STATUS_ICONS } from '@/components/icons/status';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import UnitAvatar from './UnitAvatar.vue';
import { useI18n } from 'vue-i18n';
import { statusLabel } from '@/i18n';

const { t } = useI18n();

interface Props {
    unit: PortraitUnit;
    size?: number;
    // Цифра ранга на портрете; карточка героя показывает ранг звёздами и прячет её
    showLevel?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    size: 48,
    showLevel: true
});

// "статы 520 · умения 140 · пассивки 60" — откуда взялась мощь
const powerDetails = computed(() => powerParts(props.unit.power)
    .map(({ part, value }) => t(`power.parts.${part}`, { value }))
    .join(' · '));
</script>

<template>
    <TooltipComponent>
        <UnitAvatar :unit="unit" :size="size" :show-level="showLevel" />

        <template #title>{{ unit.name }}<template v-if="unit.isBoss"> · {{ t('unit.boss') }}</template></template>
        <template #description>
            {{ t('unit.stats', { type: unit.unitType, hp: unit.maxHp, attack: unit.attack, speed: unit.speed }) }}
        </template>
        <template #stats>
            <div class="portrait-power">⚔ {{ t('power.value', { power: unit.power.total }) }}</div>
            <div class="portrait-power-parts">{{ powerDetails }}</div>
            <div v-if="unit.spells.length">{{ t('unit.spells', { list: unit.spells.join(', ') }) }}</div>
            <div v-if="unit.immunities.length" class="portrait-immunities">
                {{ t('unit.immune') }}
                <span v-for="name in unit.immunities" :key="name" :title="statusLabel(name)">
                    <component v-if="STATUS_ICONS[name]" :is="STATUS_ICONS[name]" size="14px" />
                    <template v-else>{{ statusLabel(name) }}</template>
                </span>
            </div>
            <div v-if="unit.aura">{{ t('unit.aura', { aura: unit.aura }) }}</div>
        </template>
    </TooltipComponent>
</template>

<style scoped>
.portrait-power {
    font-weight: 700;
    color: #ffd166;
}

.portrait-power-parts {
    margin-bottom: 4px;
    font-size: 0.85em;
    opacity: 0.8;
}

.portrait-immunities {
    display: flex;
    align-items: center;
    gap: 4px;
}
</style>
