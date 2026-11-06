<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { HeroElement } from '@/battle/types';
import { arenaPreview } from '@/battle/arenaBackgrounds';
import { ELEMENT_COLORS } from '@/components/inventory/lootStyles';
import GemButton from '@/components/common/GemButton.vue';
import type { RewardState } from '@/composables/UseStageRewards';
import { IconChest, IconCrown, IconHourglass } from '@/components/icons/ui';
import { IconPower } from '@/components/icons/stats';

// Пройденный уровень кампании или этаж башни — низкая строка без врагов и прогноза:
// номер, название на фоне арены и награда, если её ещё не забрали. Под названием — что-то своё (слот: награда этажа)
interface Props {
    number: number;
    title: string;
    background: string;
    element: HeroElement;
    boss?: boolean;
    rewardState: RewardState;
    // Плашка над названием (этаж прошлой недели)
    tag?: string | null;
    // Забираем эту награду / заняты другой
    claiming?: boolean;
    disabled?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    boss: false,
    tag: null,
    claiming: false,
    disabled: false,
});

const emit = defineEmits<{ (e: 'claim'): void }>();

const { t } = useI18n();

const accent = computed(() => ELEMENT_COLORS[props.element] ?? ELEMENT_COLORS.Basic);
const status = computed(() => ({
    pending: t('rewards.waiting'),
    claimed: t('rewards.received'),
    none: t('campaign.cleared'),
})[props.rewardState]);
</script>

<template>
    <!-- Светящаяся карточка (glowCard.css): награда ждёт — золото, иначе погашена -->
    <div
        class="cleared-card glow-card"
        :class="[`cleared-card-${props.rewardState}`, props.rewardState === 'pending' ? 'glow-card-claimable' : 'glow-card-done']"
        :style="{ '--accent': accent }"
    >
        <div class="cleared-arena" aria-hidden="true">
            <div class="cleared-arena-image" :style="{ backgroundImage: `url(${arenaPreview(props.background)})` }"></div>
        </div>

        <div class="cleared-number">
            <span :class="{ 'cleared-number-boss': props.boss }">{{ props.number }}</span>
            <span class="cleared-seal">✓</span>
        </div>

        <div class="cleared-body">
            <div class="cleared-heading">
                <span v-if="props.tag" class="cleared-tag">{{ props.tag }}</span>
                <span v-if="props.boss" class="cleared-boss"><IconCrown /></span>
                <span class="cleared-title">{{ props.title }}</span>
                <span class="cleared-status">{{ status }}</span>
            </div>
            <slot></slot>
        </div>

        <div class="cleared-action">
            <GemButton
                v-if="props.rewardState === 'pending'"
                :color="'yellow'"
                class="cleared-claim"
                :disabled="props.disabled"
                :aria-label="t('rewards.claimOne')"
                v-tooltip="props.claiming ? t('rewards.claiming') : t('rewards.claimOne')"
                @click="emit('claim')"
            >
                <!-- Только значок. Размер задаёт невидимая надпись кнопки боя — кнопки в списке выходят одинаковыми на любом языке -->
                <span class="cleared-claim-face">
                    <span class="cleared-claim-sizer" aria-hidden="true"><IconPower /> {{ t('campaign.fight') }}</span>
                    <!-- Пока забираем — песочные часы вместо сундука -->
                    <IconHourglass v-if="props.claiming" class="cleared-claim-icon" />
                    <IconChest v-else class="cleared-claim-icon" />
                </span>
            </GemButton>
            <span v-else class="cleared-done" :class="{ 'cleared-done-fresh': props.rewardState === 'claimed' }">✓</span>
        </div>
    </div>
</template>

<style scoped src="@/components/common/glowCard.css"></style>

<style scoped>
/* Слои внутри изолированной карточки: арена (-2) → ореол (::after, -1) → содержимое → блик по рамке (::before) */
.cleared-card {
    --accent: #cbd5e1;
    --glow-color: var(--accent);
    width: 100%;
    min-height: 76px;
    display: flex;
    align-items: stretch;
    border-radius: 12px;
}

.cleared-arena {
    position: absolute;
    inset: 0;
    z-index: -2;
    overflow: hidden;
    /* Внутри рамки в 2px */
    border-radius: 10px;
}

.cleared-card-pending:hover .cleared-arena-image {
    transform: scale(1.08);
}

.cleared-arena-image {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center 60%;
    filter: grayscale(0.6) brightness(0.6);
    transition: transform 0.4s ease;
}

.cleared-card-pending .cleared-arena-image {
    filter: saturate(0.8) brightness(0.8);
}

.cleared-arena::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to right, rgba(8, 11, 18, 0.75), rgba(8, 11, 18, 0.35) 45%, rgba(8, 11, 18, 0.75));
}

.cleared-number {
    position: relative;
    flex-shrink: 0;
    width: 84px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 10px 0 0 10px;
    border-right: 2px solid color-mix(in srgb, var(--accent) 45%, transparent);
    background: linear-gradient(to bottom, rgba(14, 19, 30, 0.78), rgba(8, 11, 18, 0.9));
    font-size: 1.9em;
    font-weight: 900;
    font-family: RunesFont, sans-serif;
    color: #cbd5e1;
    text-shadow: 1px 1px 2px #000;
}

.cleared-number-boss {
    color: #ffd166;
    text-shadow: 0 0 10px #ff7b00, 1px 1px 2px #000;
}

/* Печать «пройдено» в углу номера */
.cleared-seal {
    position: absolute;
    right: 8px;
    bottom: 6px;
    width: 18px;
    height: 18px;
    border-radius: 50%;
    background: var(--green-gem-color);
    box-shadow: 0 0 0 2px #333, 0 0 6px rgba(0, 200, 83, 0.7);
    color: #fff;
    font-family: Arial, sans-serif;
    font-size: 11px;
    line-height: 18px;
    text-align: center;
    text-shadow: none;
}

.cleared-body {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 4px;
    padding: 6px 16px;
}

.cleared-heading {
    display: flex;
    align-items: center;
    gap: 8px;
    min-width: 0;
}

.cleared-title {
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
    font-weight: 800;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    text-shadow: 0 1px 3px #000, 0 0 6px #000;
}

.cleared-tag {
    flex-shrink: 0;
    padding: 0 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.55);
    border: 1px solid #94a3b8;
    color: #cbd5e1;
    font-size: 11px;
    font-weight: 700;
}

.cleared-boss {
    flex-shrink: 0;
    filter: drop-shadow(0 0 4px #ff7b00);
}

.cleared-status {
    flex-shrink: 0;
    margin-left: auto;
    color: #94a3b8;
    font-size: 12px;
    font-weight: 700;
    text-shadow: 0 1px 2px #000;
}

.cleared-card-pending .cleared-status {
    color: #ffd166;
}

.cleared-action {
    flex-shrink: 0;
    width: 132px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 0 10px 10px 0;
    border-left: 2px solid color-mix(in srgb, var(--accent) 45%, transparent);
    background: linear-gradient(to bottom, rgba(14, 19, 30, 0.78), rgba(8, 11, 18, 0.9));
}

/* Как у кнопки боя (stageModal.css) */
.cleared-claim {
    max-width: 118px;
}

/* Надпись-распорка и значок лежат в одной клетке: распорка задаёт размер, значок — по центру */
.cleared-claim-face {
    display: inline-grid;
    place-items: center;
}

.cleared-claim-sizer,
.cleared-claim-icon {
    grid-area: 1 / 1;
}

.cleared-claim-sizer {
    visibility: hidden;
    white-space: nowrap;
}

/* Значок крупнее буквы, но через transform — высоту кнопки он не меняет */
.cleared-claim-icon {
    transform: scale(1.3);
}

.cleared-done {
    color: #64748b;
    font-size: 26px;
    font-weight: 900;
    text-shadow: 0 1px 2px #000;
}

/* Только что забрали — галочка зелёная */
.cleared-done-fresh {
    color: var(--green-gem-color);
    text-shadow: 0 0 10px rgba(0, 200, 83, 0.6), 0 1px 2px #000;
}

@media (prefers-reduced-motion: reduce) {
    .cleared-card-pending:hover .cleared-arena-image {
        transform: none;
    }
}
</style>
