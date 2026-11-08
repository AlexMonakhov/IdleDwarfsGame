<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import QuestsApiService from '@/services/apiServices/QuestsApiService';
import { useQuests } from '@/composables/UseQuests';
import { useResources } from '@/composables/UseResources';
import { CHEST_ICONS } from '@/components/inventory/lootStyles';
import { EVENT_CURRENCY_ICONS, formatCountdown } from '@/components/events/eventShared';
import { chestLabel } from '@/i18n';
import { IconCheck, IconCoin, IconElderSeal, IconExperience, IconRainbowCoin, IconRefresh, IconStar } from '@/components/icons/ui';
import type { QuestBoard, QuestClaimResult, QuestInfo, QuestMilestoneInfo } from '@/models/types/Quests';

// Доска ежедневных заданий: шкала очков с порогами наград и пять заданий. Прогресс копится от любых действий
// в игре; выполненное задание забирается кнопкой — его очки идут в шкалу, пороги шкалы забираются отдельно
const { t, te, locale } = useI18n();
const { board, set: setBoard, refresh } = useQuests();
const { set: setResources } = useResources();

const loading = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const format = (value: number) => value.toLocaleString(locale.value);

const errorText = (error: unknown, fallback: string) => {
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`quests.errors.${data.code}`)) return t(`quests.errors.${data.code}`);
    return data?.message ?? fallback;
};

/* ===== Время ===== */
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;

const msLeft = computed(() => (board.value ? new Date(board.value.resetAt).getTime() - (now.value + clockOffset) : 0));

/* ===== Загрузка и действия ===== */
const apply = (next: QuestBoard) => {
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
    now.value = Date.now();
    setBoard(next);
    setResources(next.resources);
};

const load = async () => {
    loading.value = true;
    try {
        apply(await QuestsApiService.get());
    } catch (error) {
        message.value = { text: errorText(error, t('quests.loadError')), error: true };
    } finally {
        loading.value = false;
    }
};

const run = async (action: () => Promise<void>) => {
    if (busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        await action();
    } catch (error) {
        message.value = { text: errorText(error, t('quests.actionError')), error: true };
        await load();
    } finally {
        busy.value = false;
    }
};

// Что получено — одной строкой для сообщения
const rewardText = (result: QuestClaimResult) => {
    const parts: string[] = [];
    if (result.elderSeals) parts.push(t('quests.reward.seals', { amount: format(result.elderSeals) }));
    if (result.eventCurrency) parts.push(t('quests.reward.event', { amount: format(result.eventCurrency) }));
    if (result.reward.gold) parts.push(t('quests.reward.gold', { amount: format(result.reward.gold) }));
    if (result.reward.experience) parts.push(t('quests.reward.experience', { amount: format(result.reward.experience) }));
    parts.push(...result.reward.chests.map(chest => chestLabel(chest)));
    return parts.join(', ');
};

const claim = (quest: QuestInfo) => run(async () => {
    const result = await QuestsApiService.claim(quest.id);
    apply(result.board);
    message.value = { text: t('quests.claimed', { reward: rewardText(result) }), error: false };
});

const claimMilestone = (milestone: QuestMilestoneInfo) => run(async () => {
    const result = await QuestsApiService.claimMilestone(milestone.index);
    apply(result.board);
    message.value = { text: t('quests.claimed', { reward: rewardText(result) }), error: false };
});

const reroll = (quest: QuestInfo) => run(async () => {
    apply(await QuestsApiService.reroll(quest.id));
    message.value = { text: t('quests.rerolled'), error: false };
});

onMounted(() => {
    load();
    ticker = window.setInterval(() => {
        now.value = Date.now();
        // Наступили новые сутки — на сервере уже новая доска
        if (board.value && !loading.value && !busy.value && msLeft.value <= 0) load();
    }, 1000);
});

onBeforeUnmount(() => {
    window.clearInterval(ticker);
    // Значок на кнопке — по свежей доске
    refresh();
});

/* ===== Вид ===== */
const fill = computed(() => (board.value ? Math.min(100, (board.value.points / board.value.maxPoints) * 100) : 0));
const milestoneLeft = (milestone: QuestMilestoneInfo) => (board.value ? (milestone.points / board.value.maxPoints) * 100 : 0);

const eventIcon = computed(() => (board.value?.event ? EVENT_CURRENCY_ICONS[board.value.event.kind] : null));

const canReroll = computed(() => !!board.value
    && (board.value.freeReroll || (board.value.resources.rainbowCoins ?? 0) >= board.value.rerollCost));

const questText = (quest: QuestInfo) => (te(`quests.templates.${quest.key}`)
    ? t(`quests.templates.${quest.key}`, { target: format(quest.target) })
    : quest.key);
</script>

<template>
    <div class="quest-board">
        <div v-if="message" class="quest-message" :class="{ 'quest-message-error': message.error }">{{ message.text }}</div>

        <div v-if="loading && !board" class="quest-hint">{{ t('common.loading') }}</div>

        <template v-else-if="board">
            <!-- ===== Шкала очков ===== -->
            <section class="quest-bar-section">
                <div class="quest-bar-head">
                    <span class="quest-points"><IconStar /> {{ t('quests.points', { points: board.points, max: board.maxPoints }) }}</span>
                    <span v-if="board.event" class="quest-event-note">
                        <component :is="eventIcon" /> {{ t('quests.eventNote', { event: t(`events.kinds.${board.event.kind}.name`) }) }}
                    </span>
                    <span class="quest-timer" v-tooltip="t('quests.resetHint')">
                        {{ t('quests.resetIn', { time: formatCountdown(msLeft, t('fixedShop.day')) }) }}
                    </span>
                </div>

                <div class="quest-bar">
                    <div class="quest-bar-fill" :style="{ width: `${fill}%` }" />
                    <button
                        v-for="milestone in board.milestones"
                        :key="milestone.index"
                        type="button"
                        class="quest-milestone"
                        :class="{
                            'quest-milestone-ready': milestone.reached && !milestone.claimed,
                            'quest-milestone-claimed': milestone.claimed,
                        }"
                        :style="{ left: `${milestoneLeft(milestone)}%` }"
                        :disabled="!milestone.reached || milestone.claimed || busy"
                        @click="claimMilestone(milestone)"
                    >
                        <span class="quest-milestone-points">{{ milestone.points }}</span>
                        <span class="quest-milestone-reward">
                            <IconCheck v-if="milestone.claimed" />
                            <template v-else>
                                <span v-for="(chest, index) in milestone.reward.chests" :key="`c${index}`" v-tooltip="chestLabel(chest)">
                                    <component :is="CHEST_ICONS[chest.type]" />
                                </span>
                                <span v-if="milestone.elderSeals" v-tooltip="t('resources.elderSeals')"><IconElderSeal /> {{ milestone.elderSeals }}</span>
                                <span v-if="milestone.eventCurrency && eventIcon"><component :is="eventIcon" /> {{ milestone.eventCurrency }}</span>
                                <span v-if="milestone.reward.gold" v-tooltip="t('resources.gold')"><IconCoin /> {{ format(milestone.reward.gold) }}</span>
                            </template>
                        </span>
                    </button>
                </div>
            </section>

            <!-- ===== Задания ===== -->
            <ul class="quest-list">
                <li
                    v-for="quest in board.quests"
                    :key="quest.id"
                    class="quest"
                    :class="[`quest-${quest.difficulty.toLowerCase()}`, { 'quest-done': quest.completed, 'quest-claimed': quest.claimed }]"
                >
                    <span class="quest-difficulty" v-tooltip="t(`quests.difficulty.${quest.difficulty}`)">
                        <IconStar v-for="n in quest.difficulty === 'Easy' ? 1 : quest.difficulty === 'Medium' ? 2 : 3" :key="n" />
                    </span>

                    <div class="quest-body">
                        <span class="quest-title">{{ questText(quest) }}</span>
                        <div class="quest-progress">
                            <div class="quest-progress-fill" :style="{ width: `${(quest.progress / quest.target) * 100}%` }" />
                            <span class="quest-progress-text">{{ format(quest.progress) }} / {{ format(quest.target) }}</span>
                        </div>
                    </div>

                    <div class="quest-reward">
                        <span v-tooltip="t('quests.pointsHint')"><IconStar /> {{ quest.points }}</span>
                        <span v-tooltip="t('resources.elderSeals')"><IconElderSeal /> {{ quest.elderSeals }}</span>
                        <span v-if="quest.eventCurrency && eventIcon"><component :is="eventIcon" /> {{ quest.eventCurrency }}</span>
                        <span v-if="quest.gold" v-tooltip="t('resources.gold')"><IconCoin /> {{ format(quest.gold) }}</span>
                        <span v-if="quest.experience" v-tooltip="t('resources.experience')"><IconExperience /> {{ format(quest.experience) }}</span>
                    </div>

                    <div class="quest-actions">
                        <span v-if="quest.claimed" class="quest-claimed-mark"><IconCheck /> {{ t('quests.done') }}</span>
                        <GemButton
                            v-else
                            :color="'green'"
                            :disabled="!quest.completed || busy"
                            @click="claim(quest)"
                        >{{ t('quests.claim') }}</GemButton>
                        <button
                            v-if="!quest.claimed && !quest.completed"
                            type="button"
                            class="quest-reroll"
                            :disabled="!canReroll || busy"
                            v-tooltip="board.freeReroll ? t('quests.rerollFree') : t('quests.rerollPaid', { cost: board.rerollCost })"
                            @click="reroll(quest)"
                        >
                            <IconRefresh />
                            <span v-if="!board.freeReroll" class="quest-reroll-cost"><IconRainbowCoin /> {{ board.rerollCost }}</span>
                        </button>
                    </div>
                </li>
            </ul>
        </template>
    </div>
</template>

<style scoped>
.quest-board {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 14px;
    padding: 4px 6px 10px;
    font-family: 'Arial', sans-serif;
    color: #f8fafc;
}

.quest-message {
    color: #bbf7d0;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.quest-message-error {
    color: #ffb4b4;
}

.quest-hint {
    color: #cbd5e1;
    font-size: 13px;
}

/* ===== Шкала ===== */
.quest-bar-section {
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 12px 16px 46px;
    border-radius: 14px;
    background: rgba(0, 0, 0, 0.3);
    box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.6);
}

.quest-bar-head {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px 16px;
}

.quest-points {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    color: #fde68a;
    font-size: 16px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.quest-event-note {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    color: #fcd34d;
    font-size: 13px;
    font-style: italic;
}

.quest-timer {
    margin-left: auto;
    padding: 3px 10px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.55);
    color: #fde68a;
    font-size: 13px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.quest-bar {
    position: relative;
    height: 14px;
    margin: 14px 18px 0 0;
    border-radius: 7px;
    background: rgba(255, 255, 255, 0.08);
    box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.8), 0 0 0 1px rgba(255, 255, 255, 0.12);
}

.quest-bar-fill {
    height: 100%;
    border-radius: inherit;
    background: linear-gradient(to right, #f59e0b, #fde047);
    box-shadow: 0 0 10px rgba(253, 224, 71, 0.5);
    transition: width 0.4s ease;
}

/* Порог: кружок с числом очков на шкале, под ним — награда */
.quest-milestone {
    position: absolute;
    top: 50%;
    transform: translate(-50%, -50%);
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 0;
    border: none;
    background: none;
    color: inherit;
    font: inherit;
    cursor: default;
}

.quest-milestone-points {
    display: grid;
    place-items: center;
    width: 30px;
    height: 30px;
    border-radius: 50%;
    background: #1e293b;
    box-shadow: 0 0 0 2px #64748b;
    font-size: 12px;
    font-weight: bold;
}

.quest-milestone-reward {
    position: absolute;
    top: 34px;
    display: flex;
    align-items: center;
    column-gap: 6px;
    white-space: nowrap;
    font-size: 12px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.quest-milestone-ready {
    cursor: pointer;
}

.quest-milestone-ready .quest-milestone-points {
    background: #f59e0b;
    box-shadow: 0 0 0 2px #fde68a, 0 0 14px rgba(253, 224, 71, 0.8);
    animation: quest-pulse 1.2s ease-in-out infinite;
}

.quest-milestone-claimed .quest-milestone-points {
    background: #166534;
    box-shadow: 0 0 0 2px #4ade80;
}

.quest-milestone-claimed .quest-milestone-reward {
    color: #4ade80;
}

@keyframes quest-pulse {
    50% { transform: scale(1.12); }
}

/* ===== Задания ===== */
.quest-list {
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    margin: 0;
    padding: 0;
    list-style: none;
}

.quest {
    display: grid;
    grid-template-columns: 54px minmax(0, 1fr) auto auto;
    align-items: center;
    column-gap: 14px;
    padding: 10px 14px;
    border-radius: 12px;
    background: rgba(0, 0, 0, 0.3);
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6), 0 0 0 1px var(--quest-color, #64748b);
}

.quest-easy { --quest-color: #4ade80; }
.quest-medium { --quest-color: #60a5fa; }
.quest-hard { --quest-color: #c084fc; }

.quest-done:not(.quest-claimed) {
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6), 0 0 0 2px #fde68a, 0 0 12px rgba(253, 224, 71, 0.35);
}

.quest-claimed {
    opacity: 0.6;
}

.quest-difficulty {
    display: flex;
    color: var(--quest-color);
    --split-icon-primary: var(--quest-color);
    --split-icon-secondary: color-mix(in srgb, var(--quest-color) 60%, #000);
}

.quest-body {
    display: flex;
    flex-direction: column;
    row-gap: 6px;
    min-width: 0;
}

.quest-title {
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.quest-progress {
    position: relative;
    height: 16px;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.08);
    overflow: hidden;
}

.quest-progress-fill {
    height: 100%;
    background: var(--quest-color);
    opacity: 0.75;
    transition: width 0.4s ease;
}

.quest-progress-text {
    position: absolute;
    inset: 0;
    display: grid;
    place-items: center;
    font-size: 11px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.9);
    font-variant-numeric: tabular-nums;
}

.quest-reward {
    display: flex;
    flex-wrap: wrap;
    justify-content: flex-end;
    gap: 4px 10px;
    max-width: 260px;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.quest-reward > span {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
}

.quest-actions {
    display: flex;
    align-items: center;
    column-gap: 8px;
}

.quest-claimed-mark {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    color: #4ade80;
    font-size: 14px;
    font-weight: bold;
}

.quest-reroll {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    height: 34px;
    padding: 0 8px;
    border: none;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.1);
    color: #e2e8f0;
    font-size: 12px;
    font-weight: bold;
    cursor: pointer;
}

.quest-reroll:hover:not(:disabled) {
    background: rgba(255, 255, 255, 0.2);
}

.quest-reroll:disabled {
    opacity: 0.4;
    cursor: default;
}

.quest-reroll-cost {
    display: inline-flex;
    align-items: center;
    column-gap: 2px;
}

@media (max-width: 760px) {
    .quest {
        grid-template-columns: 40px minmax(0, 1fr);
        row-gap: 8px;
    }

    .quest-reward,
    .quest-actions {
        grid-column: 2;
        justify-content: flex-start;
    }
}
</style>
