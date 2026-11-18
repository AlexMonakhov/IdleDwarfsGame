<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import TooltipComponent from '@/components/common/TooltipComponent.vue';
import SquadBonusBadge from './SquadBonusBadge.vue';
import SquadBonusIcon from './SquadBonusIcon.vue';
import { ELEMENT_ICONS } from '@/components/inventory/lootStyles';
import type { HeroElement } from '@/battle/types';
import { SquadBonusHint, resolveSquadBonus, squadBonusHints } from '@/battle/squadBonuses';
import { bonusColor, bonusDescription, bonusElementColor, bonusName, elementLabel } from './squadBonusText';
import { bonusComposition } from './squadBonusComposition';

// Строка под отрядом при сборке: действующий бонус и чего не хватает до следующих —
// «+1 Огонь → Неугасимое пламя III». Меняется сразу, пока игрок двигает героев
const props = defineProps<{
    // Стихии героев по местам отряда; пустые места — null
    elements: (HeroElement | null)[];
}>();

const { t } = useI18n();

const bonus = computed(() => resolveSquadBonus(props.elements));
const hints = computed(() => squadBonusHints(props.elements));

const needText = (hint: SquadBonusHint) => [
    ...hint.need.map(n => t('squadBonuses.hintMore', { count: n.count, element: elementLabel(n.element) })),
    ...(hint.needDistinct ? [t('squadBonuses.hintDistinct', hint.needDistinct, { count: hint.needDistinct })] : []),
].join(', ');
</script>

<template>
    <div class="squad-bonus-hints">
        <span class="squad-bonus-hints-label">{{ t('squadBonuses.title') }}:</span>
        <SquadBonusBadge :bonus="bonus" size="30px" label show-empty />

        <template v-if="hints.length">
            <span class="squad-bonus-hints-divider" aria-hidden="true"></span>
            <span class="squad-bonus-hints-label">{{ t('squadBonuses.hintsTitle') }}:</span>
            <TooltipComponent v-for="hint in hints" :key="hint.bonus.id + hint.bonus.tier" :accent="bonusColor(hint.bonus)">
                <span class="squad-bonus-hint" :style="{ '--hint-color': bonusColor(hint.bonus) }">
                    <span class="squad-bonus-hint-need">
                        <template v-for="n in hint.need" :key="n.element">
                            <span class="squad-bonus-hint-element" :style="{ color: bonusElementColor(n.element) }">
                                +{{ n.count }}
                                <img v-if="ELEMENT_ICONS[n.element]" :src="ELEMENT_ICONS[n.element]" :alt="elementLabel(n.element)" class="squad-bonus-hint-gem" />
                            </span>
                        </template>
                        <span v-if="hint.needDistinct" class="squad-bonus-hint-element">+{{ hint.needDistinct }} ?</span>
                    </span>
                    <span class="squad-bonus-hint-arrow">→</span>
                    <SquadBonusIcon :bonus="hint.bonus" size="22px" dim />
                    <span class="squad-bonus-hint-name">{{ bonusName(hint.bonus) }}</span>
                </span>
                <template #title>{{ needText(hint) }} → {{ bonusName(hint.bonus) }}</template>
                <template #subtitle>{{ t(`squadBonuses.shapes.${hint.bonus.shape}`) }} · {{ bonusComposition(hint.bonus) }}</template>
                <template #description>{{ bonusDescription(hint.bonus) }} {{ t('squadBonuses.hintReplace') }}</template>
            </TooltipComponent>
        </template>
    </div>
</template>

<style scoped>
.squad-bonus-hints {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    column-gap: 8px;
    row-gap: 6px;
    margin-top: 8px;
    padding: 6px 10px;
    border-radius: 10px;
    background: rgba(10, 16, 22, 0.55);
    font-family: 'Arial', sans-serif;
}

.squad-bonus-hints-label {
    color: #cbd5e1;
    font-size: 12px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.squad-bonus-hints-divider {
    width: 1px;
    align-self: stretch;
    margin: 0 4px;
    background: rgba(157, 173, 175, 0.4);
}

/* Подсказка: чего добавить → какой бонус */
.squad-bonus-hint {
    display: inline-flex;
    align-items: center;
    column-gap: 5px;
    padding: 2px 8px 2px 6px;
    border-radius: 8px;
    border: 1px dashed color-mix(in srgb, var(--hint-color) 60%, transparent);
    background: color-mix(in srgb, var(--hint-color) 10%, rgba(0, 0, 0, 0.4));
    cursor: help;
}

.squad-bonus-hint-need {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
}

.squad-bonus-hint-element {
    display: inline-flex;
    align-items: center;
    column-gap: 2px;
    color: #f8fafc;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.85);
}

.squad-bonus-hint-gem {
    width: 16px;
    height: 16px;
}

.squad-bonus-hint-arrow {
    color: #94a3b8;
    font-size: 13px;
}

.squad-bonus-hint-name {
    color: var(--hint-color);
    font-size: 12px;
    font-weight: bold;
    white-space: nowrap;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.85);
}
</style>
