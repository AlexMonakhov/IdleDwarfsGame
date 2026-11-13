<script lang="ts" setup>
import { computed } from 'vue';
import { PortraitUnit } from '@/battle/types';
import { powerParts } from '@/battle/power';
import { STATUS_ICONS } from '@/components/icons/status';
import { ELEMENT_COLORS } from '@/components/inventory/lootStyles';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import StatIcon from '@/components/icons/stats/StatIcon.vue';
import SpellName from '@/components/icons/spells/SpellName.vue';
import AuraName from '@/components/icons/status/AuraName.vue';
import { PROFESSION_ICONS } from '@/components/icons/professions';
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

const stats = computed(() => {
    const rows = [
        { key: 'hp', value: props.unit.maxHp },
        { key: 'attack', value: props.unit.attack },
        { key: 'speed', value: props.unit.speed },
    ];
    // Старый сервер не присылал энергию за удар — тогда строка без неё
    if (props.unit.energyPerAttack != null) rows.push({ key: 'energyPerAttack', value: props.unit.energyPerAttack });
    return rows;
});

// "статы 520 · умения 140 · пассивки 60" — откуда взялась мощь
const powerDetails = computed(() => powerParts(props.unit.power)
    .map(({ part, value }) => t(`power.parts.${part}`, { value }))
    .join(' · '));
</script>

<template>
    <TooltipComponent :accent="ELEMENT_COLORS[unit.element]">
        <UnitAvatar :unit="unit" :size="size" :show-level="showLevel" />

        <template #title>{{ unit.name }}<template v-if="unit.isBoss"> · {{ t('unit.boss') }}</template></template>
        <template #description>
            <!-- У дворфа — специализация, у монстров (гоблин, голем…) — их вид -->
            <div class="portrait-kind">
                <span v-if="unit.profession" class="portrait-stat">
                    <component :is="PROFESSION_ICONS[unit.profession]" v-if="PROFESSION_ICONS[unit.profession]" size="14px" />
                    {{ t(`professions.${unit.profession}`) }}
                </span>
                <span v-else>{{ unit.unitType }}</span>
            </div>
            <div class="portrait-stats">
                <span v-for="stat in stats" :key="stat.key" class="portrait-stat" :title="t(`heroUpgrade.statNames.${stat.key}`)">
                    <StatIcon :stat="stat.key" size="14px" />{{ stat.value }}
                </span>
            </div>
        </template>
        <template #stats>
            <div class="portrait-power"><StatIcon stat="power" size="14px" /> {{ t('power.value', { power: unit.power.total }) }}</div>
            <div class="portrait-power-parts">{{ powerDetails }}</div>
            <!-- «Подпись: значения» — две колонки: перенесённые значения остаются в своей колонке, а не уходят под подпись -->
            <div v-if="unit.spells.length || unit.immunities.length || unit.aura" class="portrait-facts">
                <template v-if="unit.spells.length">
                    <span class="portrait-fact-label">{{ t('unit.spells') }}</span>
                    <span class="portrait-fact-value">
                        <SpellName v-for="spell in unit.spells" :key="spell" :spell="spell" />
                    </span>
                </template>
                <template v-if="unit.immunities.length">
                    <span class="portrait-fact-label">{{ t('unit.immune') }}</span>
                    <span class="portrait-fact-value">
                        <span v-for="name in unit.immunities" :key="name" :title="statusLabel(name)">
                            <component v-if="STATUS_ICONS[name]" :is="STATUS_ICONS[name]" size="14px" />
                            <template v-else>{{ statusLabel(name) }}</template>
                        </span>
                    </span>
                </template>
                <template v-if="unit.aura">
                    <span class="portrait-fact-label">{{ t('unit.aura') }}</span>
                    <span class="portrait-fact-value"><AuraName :aura="unit.aura" size="14px" /></span>
                </template>
            </div>
        </template>
    </TooltipComponent>
</template>

<style scoped>
/* Вид или специализация — своей строкой, под ней статы */
.portrait-kind {
    margin-bottom: 2px;
}

/* Статы — всегда одной строкой: подсказка расширяется под неё (min-width в TooltipPanel) */
.portrait-stats {
    display: flex;
    align-items: center;
    column-gap: 10px;
    white-space: nowrap;
}

.portrait-stat {
    display: inline-flex;
    align-items: center;
    column-gap: 3px;
    font-weight: 700;
}

.portrait-power {
    font-weight: 700;
    color: #ffd166;
}

.portrait-power-parts {
    margin-bottom: 4px;
    font-size: 0.85em;
}

/* Подписи — левая колонка по самой длинной, значения — правая */
.portrait-facts {
    display: grid;
    grid-template-columns: auto 1fr;
    gap: 3px 6px;
    align-items: start;
}

.portrait-fact-label {
    white-space: nowrap;
}

.portrait-fact-value {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 2px 8px;
    min-width: 0;
}
</style>
