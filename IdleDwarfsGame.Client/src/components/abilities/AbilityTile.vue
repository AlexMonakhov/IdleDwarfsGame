<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import GlassButton from '@/components/common/GlassButton.vue';
import { STATUS_ICONS } from '@/components/icons/status';
import { ELEMENT_ICONS } from '@/components/inventory/lootStyles';
import { AbilityInfo } from '@/models/types/Abilities';
import { elementColor } from '@/battle/sprites';
import { abilityName, abilityText } from './abilityText';

// Способность — извлечённая или на дворфе: значок, название, уровень точками и что она делает
const props = defineProps<{
    ability: AbilityInfo;
    selected?: boolean;
    // Подходит в пару выбранной: слегка подсвечена
    highlighted?: boolean;
    disabled?: boolean;
}>();

const { t } = useI18n();

// Заклинание — цвета своей стихии, пассивка — золото, аура — бирюза
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

const elementIcon = computed(() => (props.ability.element ? ELEMENT_ICONS[props.ability.element] : undefined));
const statusIcon = computed(() => (props.ability.kind === 'Aura' ? STATUS_ICONS[props.ability.key] : undefined));
</script>

<template>
    <div
        class="ability-tile"
        :class="{
            'ability-tile-selected': selected,
            'ability-tile-highlighted': highlighted && !selected,
            'ability-tile-disabled': disabled,
        }"
        :style="{ '--ability-accent': accent }"
    >
        <GlassButton tag="div" class="ability-tile-glass" :active="selected">
            <span class="ability-tile-kind">{{ t(`abilities.kinds.${ability.kind}`) }}</span>
            <span class="ability-tile-icon">
                <img v-if="elementIcon" :src="elementIcon" alt="" draggable="false" />
                <component :is="statusIcon" v-else-if="statusIcon" size="30px" />
                <template v-else>⚙</template>
            </span>
            <span class="ability-tile-name">{{ abilityName(ability) }}</span>
            <span class="ability-tile-level" v-tooltip="t('abilities.level', { level: ability.level, max: ability.maxLevel })">
                <span
                    v-for="step in ability.maxLevel"
                    :key="step"
                    class="ability-tile-pip"
                    :class="{ 'ability-tile-pip-done': step <= ability.level }"
                ></span>
            </span>
            <span class="ability-tile-text">{{ abilityText(ability) }}</span>
        </GlassButton>
    </div>
</template>

<style scoped>
/* Оправа как у плитки предмета, стекло — цвета способности */
.ability-tile {
    --glass-top: color-mix(in srgb, var(--ability-accent) 30%, transparent);
    --glass-mid: color-mix(in srgb, var(--ability-accent) 18%, transparent);
    --glass-bottom: color-mix(in srgb, var(--ability-accent) 10%, rgba(10, 14, 24, 0.6));
    position: relative;
    box-sizing: border-box;
    width: 136px;
    display: flex;
    flex-direction: column;
    padding: 8px;
    border-radius: 14px;
    background: var(--metalic-background);
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 4px 3px 2px #181717,
        0 3px 12px 2px rgba(0, 0, 0, 0.6);
    font-family: 'Arial', sans-serif;
    text-align: center;
    text-shadow: var(--text-outline);
    user-select: none;
    cursor: pointer;
    transition: transform 0.2s cubic-bezier(0.34, 1.4, 0.64, 1), box-shadow 0.3s ease, opacity 0.15s ease;
}

.ability-tile:hover {
    transform: translateY(-3px);
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 4px 3px 2px #181717,
        0 3px 18px 4px color-mix(in srgb, var(--ability-accent) 55%, transparent);
}

.ability-tile-selected,
.ability-tile-selected:hover {
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 0 0 3px #ffd166,
        0 0 18px 4px rgba(255, 209, 102, 0.65);
}

.ability-tile-highlighted {
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 0 0 2px #86efac,
        0 0 14px 3px rgba(134, 239, 172, 0.45);
}

.ability-tile-disabled {
    cursor: not-allowed;
    opacity: 0.45;
    filter: grayscale(0.6);
}

.ability-tile-disabled:hover {
    transform: none;
}

/* Два класса — чтобы перебить размеры и скругление самого GlassButton */
.ability-tile .ability-tile-glass {
    --glass-glow: color-mix(in srgb, var(--ability-accent) 40%, transparent);
    flex: 1;
    width: 100%;
    min-width: 0;
    justify-content: flex-start;
    row-gap: 3px;
    padding: 8px 6px;
    border-radius: 8px;
}

.ability-tile:hover .ability-tile-glass {
    --glass-glow: color-mix(in srgb, var(--ability-accent) 75%, transparent);
}

.ability-tile:hover .ability-tile-glass::after {
    transform: translateX(120%);
}

.ability-tile-glass > * {
    position: relative;
    z-index: 1;
}

.ability-tile .ability-tile-glass::after {
    z-index: 2;
}

.ability-tile-kind {
    color: var(--ability-accent);
    font-size: 10px;
    font-weight: bold;
    letter-spacing: 1px;
    text-transform: uppercase;
}

.ability-tile-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    color: var(--ability-accent);
    font-size: 28px;
    filter: drop-shadow(0 0 5px var(--ability-accent));
}

.ability-tile-icon img {
    width: 34px;
    height: 34px;
    object-fit: contain;
}

.ability-tile-name {
    color: #f8fafc;
    font-size: 12px;
    font-weight: 700;
    line-height: 1.15;
}

.ability-tile-level {
    display: flex;
    justify-content: center;
    column-gap: 3px;
}

.ability-tile-pip {
    width: 14px;
    height: 6px;
    border-radius: 2px;
    background: rgba(0, 0, 0, 0.55);
    box-shadow: inset 0 0 2px #000;
}

.ability-tile-pip-done {
    background: linear-gradient(to bottom, #fde68a, #f59e0b);
    box-shadow: 0 0 4px rgba(245, 158, 11, 0.6);
}

.ability-tile-text {
    color: #cbd5e1;
    font-size: 10px;
    line-height: 1.2;
}
</style>
