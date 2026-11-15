<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import { elementColor } from '@/battle/sprites';
import { spellDescription, spellPower } from '@/components/delve/delveText';
import type { AbilityInfo } from '@/models/types/Abilities';
import type { SpellDetail } from '@/models/types/Heroes';
import { abilityName, abilityText } from './abilityText';

// Подсказка способности при наведении — как у слота дворфа спуска: название и уровень, вид, описание заклинания,
// его сила и что способность делает. Сила заклинания: spellDetail — точные числа у дворфа, на котором оно надето;
// без дворфа (извлечённое, награда спуска) — урон и лечение в % атаки
const props = defineProps<{
    ability: AbilityInfo;
    spellDetail?: SpellDetail | null;
}>();

const { t } = useI18n();

const ELEMENT_ACCENTS: Record<string, string> = {
    red: 'var(--red-gem-color)',
    blue: 'var(--blue-gem-color)',
    yellow: 'var(--yellow-gem-color)',
    green: 'var(--green-gem-color)',
    purple: 'var(--purple-gem-color)',
    violet: 'var(--pink-gem-color)',
    white: 'var(--white-gem-color)',
};

const accent = computed(() => {
    if (props.ability.kind === 'Passive') return '#ffd166';
    if (props.ability.kind === 'Aura') return '#5eead4';
    return ELEMENT_ACCENTS[elementColor(props.ability.element)] ?? '#9ca3af';
});

const description = computed(() => (props.ability.kind === 'Spell' ? spellDescription(props.ability.spellName) : ''));
const percentText = computed(() => {
    const a = props.ability;
    if (a.kind !== 'Spell') return '';
    const parts: string[] = [];
    if (a.spellDamagePercent > 0) {
        parts.push(a.spellTargets > 1
            ? t('abilities.spellDamageTargets', { percent: a.spellDamagePercent, targets: a.spellTargets })
            : t('abilities.spellDamage', { percent: a.spellDamagePercent }));
    }
    if (a.spellHealingPercent > 0) parts.push(t('abilities.spellHealing', { percent: a.spellHealingPercent }));
    if (a.spellEnergyCost > 0) parts.push(t('heroUpgrade.spellCost', { cost: a.spellEnergyCost }));
    return parts.join(' · ');
});

const powerText = computed(() => (props.spellDetail ? spellPower(props.spellDetail) : percentText.value));
</script>

<template>
    <TooltipComponent class="ability-tooltip-trigger" :accent="accent">
        <slot />
        <template #title>{{ abilityName(ability) }} · {{ t('abilities.level', { level: ability.level, max: ability.maxLevel }) }}</template>
        <template #subtitle>{{ t(`abilities.kinds.${ability.kind}`) }}</template>
        <template #description>
            <div class="ability-tooltip">
                <span v-if="description">{{ description }}</span>
                <span v-if="powerText" class="ability-tooltip-power">{{ powerText }}</span>
                <span v-if="abilityText(ability)">{{ abilityText(ability) }}</span>
            </div>
        </template>
    </TooltipComponent>
</template>

<style scoped>
/* Обёртка подсказки не мешает раскладке: плитка внутри тянется как раньше */
.dwarf-tooltip-wrapper.ability-tooltip-trigger {
    display: flex;
    cursor: inherit;
}

.ability-tooltip {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.ability-tooltip-power {
    color: #fde68a;
}
</style>
