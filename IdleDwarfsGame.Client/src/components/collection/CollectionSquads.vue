<script lang="ts" setup>
import { reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import SquadBonusIcon from '@/components/squadBonus/SquadBonusIcon.vue';
import SquadBonusFigure from '@/components/squadBonus/SquadBonusFigure.vue';
import { soundService } from '@/services/soundService/SoundService';
import { ALL_MONO, ALL_PAIRS, PRISM, SquadBonus, monoBonus } from '@/battle/squadBonuses';
import { bonusColor, bonusDescription, bonusElementColor, bonusName, elementLabel } from '@/components/squadBonus/squadBonusText';
import { bonusComposition } from '@/components/squadBonus/squadBonusComposition';
import { vDragScroll } from '@/directives/dragScroll';

// Справочник бонусов стихий отряда: как отряд получает бонус и что делает каждый — моно, пары и призма.
// Числа моно-бонусов — сразу для всех трёх уровней. Наведение на карточку оживляет значок, как на арене,
// клик — импульс, как при срабатывании бонуса в бою
const { t } = useI18n();

const hovered = ref<string | null>(null);
const pulses = reactive<Record<string, number>>({});
const pulse = (bonus: SquadBonus) => {
    pulses[bonus.id] = (pulses[bonus.id] ?? 0) + 1;
    soundService.play('battle_aura');
};

// Лесенка составов сверху вниз — по силе бонуса
const RULES: { key: string; bonus: SquadBonus | null }[] = [
    { key: 'mono3', bonus: monoBonus('Fire', 3) },
    { key: 'mono2', bonus: monoBonus('Fire', 2) },
    { key: 'pair', bonus: ALL_PAIRS[0] },
    { key: 'mono1', bonus: monoBonus('Fire', 1) },
    { key: 'prism', bonus: PRISM },
    { key: 'none', bonus: null },
];

const SECTIONS = [
    { key: 'mono', title: 'squadBonuses.monoTitle', hint: 'squadBonuses.monoHint', bonuses: ALL_MONO },
    { key: 'pairs', title: 'squadBonuses.pairsTitle', hint: 'squadBonuses.pairsHint', bonuses: ALL_PAIRS },
    { key: 'prism', title: 'squadBonuses.prismTitle', hint: 'squadBonuses.prismHint', bonuses: [PRISM] },
];

// У моно в подписи — составы всех уровней: «3 / 4 / 5 × Огонь»
const compositionOf = (bonus: SquadBonus) => (bonus.shape === 'Mono'
    ? t('squadBonuses.composition', { count: '3 / 4 / 5', element: elementLabel(bonus.elements[0]) })
    : bonusComposition(bonus));
</script>

<template>
    <section v-drag-scroll class="squads">
        <div class="squads-intro">{{ t('collection.squadsHint') }}</div>

        <div class="squads-rules">
            <h4 class="squads-title">{{ t('squadBonuses.rulesTitle') }}</h4>
            <p class="squads-hint">{{ t('squadBonuses.rulesHint') }}</p>
            <div class="squads-rules-grid">
                <template v-for="rule in RULES" :key="rule.key">
                    <span class="squads-rule-icon">
                        <SquadBonusIcon v-if="rule.bonus" :bonus="rule.bonus" size="26px" />
                    </span>
                    <span class="squads-rule-composition">{{ t(`squadBonuses.rules.${rule.key}`) }}</span>
                    <span class="squads-rule-result" :class="{ 'squads-rule-none': !rule.bonus }">{{ t(`squadBonuses.rules.${rule.key}Result`) }}</span>
                </template>
            </div>
        </div>

        <div v-for="section in SECTIONS" :key="section.key" class="squads-section">
            <h4 class="squads-title">{{ t(section.title) }}</h4>
            <p class="squads-hint">{{ t(section.hint) }}</p>
            <div class="squads-grid">
                <article
                    v-for="bonus in section.bonuses"
                    :key="bonus.id"
                    class="squads-card"
                    :class="{ 'squads-card-active': hovered === bonus.id }"
                    :style="{ '--bonus-color': bonusColor(bonus), '--bonus-second': bonus.elements[1] ? bonusElementColor(bonus.elements[1]) : bonusColor(bonus) }"
                    @mouseenter="hovered = bonus.id"
                    @mouseleave="hovered = hovered === bonus.id ? null : hovered"
                    @click="pulse(bonus)"
                >
                    <SquadBonusFigure
                        class="squads-card-figure"
                        :bonus="section.key === 'mono' ? monoBonus(bonus.elements[0], 3) : bonus"
                        :show-tier="false"
                        size="52px"
                        :animated="hovered === bonus.id"
                        :pulse="pulses[bonus.id] ?? 0"
                    />
                    <div class="squads-card-text">
                        <div class="squads-card-name">{{ bonusName(bonus, false) }}</div>
                        <div class="squads-card-composition">{{ compositionOf(bonus) }}</div>
                        <p class="squads-card-description selectable">{{ bonusDescription(bonus, true) }}</p>
                    </div>
                </article>
            </div>
        </div>
    </section>
</template>

<style scoped>
.squads {
    flex: 1;
    min-width: 0;
    min-height: 0;
    display: flex;
    flex-direction: column;
    row-gap: 14px;
    overflow-y: auto;
    padding: 4px 8px 12px;
    font-family: 'Arial', sans-serif;
}

.squads-intro {
    text-align: center;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.squads-rules,
.squads-section {
    padding: 12px 16px;
    border-radius: 12px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.7);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.squads-title {
    margin: 0;
    color: #ffd166;
    font-size: 16px;
    letter-spacing: 1px;
    text-transform: uppercase;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.9);
}

.squads-hint {
    margin: 4px 0 10px;
    color: #cbd5e1;
    font-size: 13px;
    line-height: 1.4;
}

/* Лесенка: значок, состав, что даёт */
.squads-rules-grid {
    display: grid;
    grid-template-columns: auto auto 1fr;
    align-items: center;
    column-gap: 12px;
    row-gap: 6px;
}

.squads-rule-icon {
    display: inline-flex;
    width: 26px;
    justify-content: center;
}

.squads-rule-composition {
    color: #f8fafc;
    font-size: 14px;
    font-weight: bold;
    white-space: nowrap;
}

.squads-rule-result {
    color: #e2e8f0;
    font-size: 13px;
}

.squads-rule-none {
    color: #94a3b8;
}

.squads-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
    gap: 10px;
}

/* Карточка бонуса: полоса слева — цвета его стихий */
.squads-card {
    position: relative;
    cursor: pointer;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
    display: flex;
    align-items: flex-start;
    column-gap: 12px;
    padding: 10px 12px;
    border-radius: 12px;
    border: 2px solid rgba(157, 173, 175, 0.35);
    border-left: 5px solid var(--bonus-color);
    background:
        linear-gradient(135deg, color-mix(in srgb, var(--bonus-color) 16%, transparent) 0%, transparent 45%,
            transparent 60%, color-mix(in srgb, var(--bonus-second) 12%, transparent) 100%),
        rgba(10, 16, 22, 0.75);
}

.squads-card-active {
    border-color: color-mix(in srgb, var(--bonus-color) 70%, transparent);
    border-left-color: var(--bonus-color);
    box-shadow: 0 0 14px color-mix(in srgb, var(--bonus-color) 35%, transparent);
}

/* Свечение и волны значка выходят за карточку — карточка их не обрезает, текст лежит поверх волн */
.squads-card-figure {
    margin: 4px;
}

.squads-card-text {
    position: relative;
    z-index: 3;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 2px;
}

.squads-card-name {
    color: var(--bonus-color);
    font-size: 15px;
    font-weight: bold;
    text-shadow: 0 0 6px color-mix(in srgb, var(--bonus-color) 45%, transparent), 1px 1px 0 rgba(0, 0, 0, 0.9);
}

.squads-card-composition {
    color: #cbd5e1;
    font-size: 12px;
}

.squads-card-description {
    margin: 4px 0 0;
    color: #e2e8f0;
    font-size: 13px;
    line-height: 1.4;
}
</style>
