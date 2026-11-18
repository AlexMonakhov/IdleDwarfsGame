<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
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

// Квадратная мини-карточка слота дворфа спуска: спелл, предмет, пассивка или аура.
// Уровень — в правом верхнем углу, подробности — в подсказке при наведении. Пустой слот — тусклый квадрат
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
    <div class="delve-slot-cell">
        <ItemTooltip v-if="item" :item="item" :hero="hero">
            <div class="delve-slot" data-sound-hover="ui_hover_card" :style="{ '--slot-accent': accent }">
                <img class="delve-slot-image" :src="getItemImage(item.key)" alt="" draggable="false" />
            </div>
        </ItemTooltip>

        <TooltipComponent v-else-if="ability" :accent="accent">
            <div class="delve-slot" data-sound-hover="ui_hover_card" :style="{ '--slot-accent': accent }">
                <component :is="icon" v-if="icon" size="26px" />
                <span v-else class="delve-slot-fallback">⚙</span>
                <span class="delve-slot-level">{{ ability.level }}</span>
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

        <div v-else class="delve-slot delve-slot-empty" v-tooltip="t(`social.delve.slots.${kind}`)">
            <span>{{ t(`social.delve.slotShort.${kind}`) }}</span>
        </div>
    </div>
</template>

<style scoped>
/* Ячейка фиксированного размера: обёртки подсказок не растягивают квадрат */
.delve-slot-cell {
    width: 52px;
    height: 52px;
}

.delve-slot-cell > :deep(*) {
    display: block;
    width: 100%;
    height: 100%;
}

/* Квадрат: металлическая оправа, внутри тёмное стекло с подсветкой цвета редкости или способности */
.delve-slot {
    position: relative;
    box-sizing: border-box;
    width: 52px;
    height: 52px;
    display: grid;
    place-items: center;
    border-radius: 10px;
    border: 3px solid transparent;
    background:
        radial-gradient(circle at 50% 35%, color-mix(in srgb, var(--slot-accent) 45%, transparent), rgba(10, 14, 24, 0.92) 75%) padding-box,
        var(--metalic-background) border-box;
    box-shadow:
        0 2px 3px 1px rgba(0, 0, 0, 0.7),
        inset 0 0 8px color-mix(in srgb, var(--slot-accent) 35%, transparent);
    cursor: help;
    transition: transform 0.15s ease, box-shadow 0.15s ease;
}

.delve-slot:hover {
    transform: translateY(-2px);
    box-shadow:
        0 2px 10px 2px color-mix(in srgb, var(--slot-accent) 65%, transparent),
        inset 0 0 10px color-mix(in srgb, var(--slot-accent) 50%, transparent);
}

.delve-slot-image {
    width: 32px;
    height: 32px;
    object-fit: contain;
    filter: drop-shadow(4px 5px 1px #000);
}

/* Иконки спеллов, пассивок и аур — та же тень, что у плиток алтаря */
.delve-slot > svg {
    filter: drop-shadow(2px 2px 1px #000);
}

.delve-slot-fallback {
    font-size: 20px;
}

/* Уровень — в правом верхнем углу, поверх оправы */
.delve-slot-level {
    position: absolute;
    top: -7px;
    right: -7px;
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
    font-size: 11px;
    font-weight: bold;
    line-height: 1;
}

.delve-slot-empty {
    --slot-accent: #475569;
    background: rgba(15, 23, 32, 0.55);
    border: 2px dashed rgba(148, 163, 184, 0.3);
    box-shadow: none;
    color: #64748b;
    font-family: 'Arial', sans-serif;
    font-size: 10px;
    text-transform: uppercase;
    cursor: default;
}

.delve-slot-empty:hover {
    transform: none;
    box-shadow: none;
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
