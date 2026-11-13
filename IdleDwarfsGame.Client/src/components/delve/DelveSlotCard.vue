<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import GlassButton from '@/components/common/GlassButton.vue';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import ItemTooltip from '@/components/equipment/ItemTooltip.vue';
import { STATUS_ICONS } from '@/components/icons/status';
import { spellIcon } from '@/components/icons/spells';
import { PASSIVE_ICONS } from '@/components/icons/passives';
import { RARITY_COLORS, getItemImage } from '@/components/inventory/lootStyles';
import { abilityName, abilityText } from '@/components/abilities/abilityText';
import { elementColor } from '@/battle/sprites';
import type { AbilityInfo } from '@/models/types/Abilities';
import type { EquipmentInfo } from '@/models/types/Equipment';
import type { HeroInfo } from '@/models/types/Heroes';
import { spellDescription, spellPower } from './delveText';

// Маленькая карточка слота дворфа спуска (спелл, предмет, пассивка, аура) с подробной подсказкой при наведении.
// Пустой слот — тусклая плашка с названием слота
type SlotKind = 'spell' | 'item' | 'passive' | 'aura';

const props = defineProps<{
    kind: SlotKind;
    hero: HeroInfo;
}>();

const { t } = useI18n();

const ABILITY_KIND: Record<Exclude<SlotKind, 'item'>, AbilityInfo['kind']> = { spell: 'Spell', passive: 'Passive', aura: 'Aura' };

const item = computed<EquipmentInfo | null>(() => (props.kind === 'item' ? props.hero.equipment[0] ?? null : null));
const ability = computed<AbilityInfo | null>(() =>
    props.kind === 'item' ? null : props.hero.abilities.find(a => a.kind === ABILITY_KIND[props.kind as Exclude<SlotKind, 'item'>]) ?? null);
const spellDetail = computed(() =>
    ability.value?.kind === 'Spell' ? props.hero.spellDetails?.find(d => d.spell === `${ability.value?.spellName} ${ability.value?.level}`) ?? null : null);

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
    if (item.value) return RARITY_COLORS[item.value.rarity];
    if (!ability.value) return '#475569';
    if (ability.value.kind === 'Passive') return '#ffd166';
    if (ability.value.kind === 'Aura') return '#5eead4';
    return ELEMENT_ACCENTS[elementColor(ability.value.element)] ?? '#9ca3af';
});

const icon = computed(() => {
    const a = ability.value;
    if (!a) return undefined;
    if (a.kind === 'Spell') return spellIcon(a.spellName);
    if (a.kind === 'Aura') return STATUS_ICONS[a.key];
    return PASSIVE_ICONS[a.key];
});
</script>

<template>
    <ItemTooltip v-if="item" :item="item" :hero="hero">
        <div class="delve-slot" :style="{ '--slot-accent': accent }">
            <GlassButton tag="div" class="delve-slot-glass">
                <img class="delve-slot-image" :src="getItemImage(item.key)" alt="" draggable="false" />
            </GlassButton>
        </div>
    </ItemTooltip>

    <TooltipComponent v-else-if="ability" :accent="accent">
        <div class="delve-slot" :style="{ '--slot-accent': accent }">
            <GlassButton tag="div" class="delve-slot-glass">
                <component :is="icon" v-if="icon" size="24px" />
                <span v-else class="delve-slot-fallback">⚙</span>
                <span class="delve-slot-level">{{ ability.level }}</span>
            </GlassButton>
        </div>
        <template #title>{{ abilityName(ability) }} · {{ t('abilities.level', { level: ability.level, max: ability.maxLevel }) }}</template>
        <template #subtitle>{{ t(`abilities.kinds.${ability.kind}`) }}</template>
        <template #description>
            <div class="delve-slot-tooltip">
                <span v-if="ability.kind === 'Spell' && spellDescription(ability.spellName)">{{ spellDescription(ability.spellName) }}</span>
                <span v-if="ability.kind === 'Spell' && spellDetail" class="delve-slot-power">{{ spellPower(spellDetail) }}</span>
                <span v-if="abilityText(ability)">{{ abilityText(ability) }}</span>
            </div>
        </template>
    </TooltipComponent>

    <div v-else class="delve-slot delve-slot-empty" :style="{ '--slot-accent': accent }" v-tooltip="t(`social.delve.slots.${kind}`)">
        <span>{{ t(`social.delve.slotShort.${kind}`) }}</span>
    </div>
</template>

<style scoped>
/* Мини-копия плитки предмета: металлическая оправа, стекло цвета редкости или способности */
.delve-slot {
    --glass-top: color-mix(in srgb, var(--slot-accent) 35%, transparent);
    --glass-mid: color-mix(in srgb, var(--slot-accent) 20%, transparent);
    --glass-bottom: color-mix(in srgb, var(--slot-accent) 12%, rgba(10, 14, 24, 0.6));
    box-sizing: border-box;
    width: 48px;
    height: 48px;
    padding: 4px;
    display: flex;
    border-radius: 10px;
    background: var(--metalic-background);
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 2px 2px 1px #181717;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
}

.delve-slot:hover {
    transform: translateY(-2px);
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 2px 10px 2px color-mix(in srgb, var(--slot-accent) 60%, transparent);
}

.delve-slot .delve-slot-glass {
    --glass-glow: color-mix(in srgb, var(--slot-accent) 55%, transparent);
    position: relative;
    flex: 1;
    width: 100%;
    min-width: 0;
    padding: 0;
    display: grid;
    place-items: center;
    border-radius: 7px;
}

.delve-slot-image {
    width: 30px;
    height: 30px;
    object-fit: contain;
    filter: drop-shadow(0 1px 1px rgba(0, 0, 0, 0.8));
}

.delve-slot-fallback {
    font-size: 18px;
}

.delve-slot-level {
    position: absolute;
    right: 2px;
    bottom: 0;
    color: #ffd166;
    font-family: 'Arial', sans-serif;
    font-size: 11px;
    font-weight: bold;
    text-shadow: var(--text-outline);
}

.delve-slot-empty {
    display: grid;
    place-items: center;
    background: rgba(15, 23, 32, 0.55);
    box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.25);
    color: #64748b;
    font-family: 'Arial', sans-serif;
    font-size: 10px;
    text-transform: uppercase;
}

.delve-slot-empty:hover {
    transform: none;
    box-shadow: inset 0 0 0 1px rgba(148, 163, 184, 0.4);
}

.delve-slot-tooltip {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.delve-slot-power {
    color: #fde68a;
}
</style>
