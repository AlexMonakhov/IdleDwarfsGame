<script lang="ts" setup>
import { soundService } from '@/services/soundService/SoundService';
import { computed, onMounted, ref } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import LoadingCrystal from '@/components/loading/LoadingCrystal.vue';
import RewardChips from './RewardChips.vue';
import { grantedText } from './rewardText';
import AchievementsApiService from '@/services/apiServices/AchievementsApiService';
import { useRewards } from '@/composables/UseRewards';
import { useResources } from '@/composables/UseResources';
import { IconCheck, IconTrophy } from '@/components/icons/ui';
import type { AchievementBoard, AchievementClaimResult, AchievementInfo } from '@/models/types/Achievements';

// Достижения: постоянные цели со ступенями. Прогресс копится от действий за всё время и от состояния аккаунта,
// достигнутые ступени забираются кнопкой (сразу все у достижения или «Забрать всё»). Сервер: Services/Achievements.cs
const { t, te, locale } = useI18n();
const { board, setBoard } = useRewards();
const { set: setResources } = useResources();

const loading = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const format = (value: number) => value.toLocaleString(locale.value);

const errorText = (error: unknown, fallback: string) => {
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`achievements.errors.${data.code}`)) return t(`achievements.errors.${data.code}`);
    return data?.message ?? fallback;
};

const apply = (next: AchievementBoard) => {
    setBoard(next);
    setResources(next.resources);
};

const load = async () => {
    loading.value = true;
    try {
        apply(await AchievementsApiService.get());
    } catch (error) {
        message.value = { text: errorText(error, t('achievements.loadError')), error: true };
    } finally {
        loading.value = false;
    }
};

const run = async (action: () => Promise<AchievementClaimResult>) => {
    if (busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        const result = await action();
        soundService.play('achievement_claim');
        apply(result.board);
        message.value = { text: t('achievements.claimed', { reward: grantedText(result.granted) }), error: false };
    } catch (error) {
        message.value = { text: errorText(error, t('achievements.actionError')), error: true };
        await load();
    } finally {
        busy.value = false;
    }
};

const claim = (achievement: AchievementInfo) => run(() => AchievementsApiService.claim(achievement.key));
const claimAll = () => run(() => AchievementsApiService.claimAll());

onMounted(load);

/* ===== Вид ===== */
const claimable = (a: AchievementInfo) => a.reachedTiers > a.claimedTiers;
const finished = (a: AchievementInfo) => a.claimedTiers >= a.tiers.length;

// Готовые к получению — сверху, полностью пройденные — вниз; внутри групп — порядок сервера
const sorted = computed(() => {
    const list = board.value?.achievements ?? [];
    const rank = (a: AchievementInfo) => (claimable(a) ? 0 : finished(a) ? 2 : 1);
    return list.map((a, index) => ({ a, index })).sort((x, y) => rank(x.a) - rank(y.a) || x.index - y.index).map(x => x.a);
});

// Ступень, к которой идёт прогресс: первая не забранная (последняя — если всё забрано)
const currentTier = (a: AchievementInfo) => a.tiers[Math.min(a.claimedTiers, a.tiers.length - 1)];

// Полоска — от предыдущей цели до текущей, чтобы на больших целях было видно движение
const fill = (a: AchievementInfo) => {
    if (finished(a) || claimable(a)) return 100;
    const target = currentTier(a).target;
    const from = a.claimedTiers > 0 ? a.tiers[a.claimedTiers - 1].target : 0;
    return Math.max(0, Math.min(100, ((a.progress - from) / (target - from)) * 100));
};

const name = (a: AchievementInfo) => (te(`achievements.list.${a.key}.name`) ? t(`achievements.list.${a.key}.name`) : a.key);
const text = (a: AchievementInfo) => (te(`achievements.list.${a.key}.text`)
    ? t(`achievements.list.${a.key}.text`, { target: format(currentTier(a).target) })
    : '');
</script>

<template>
    <div class="achievements">
        <div v-if="message" class="ach-message" :class="{ 'ach-message-error': message.error }">{{ message.text }}</div>

        <LoadingCrystal v-if="loading && !board" />

        <template v-else-if="board">
            <section class="ach-head">
                <span class="ach-total"><IconTrophy /> {{ t('achievements.total', { done: board.claimedTiers, total: board.totalTiers }) }}</span>
                <span class="ach-hint">{{ t('achievements.hint') }}</span>
                <GemButton class="ach-claim-all" color="green" :disabled="!board.claimable || busy" :badge-count="board.claimable" @click="claimAll">
                    {{ t('achievements.claimAll') }}
                </GemButton>
            </section>

            <ul class="ach-list">
                <li
                    v-for="a in sorted"
                    :key="a.key"
                    class="ach"
                    :class="{ 'ach-ready': claimable(a), 'ach-finished': finished(a) }"
                >
                    <div class="ach-body">
                        <div class="ach-title-row">
                            <span class="ach-title">{{ name(a) }}</span>
                            <span class="ach-tiers" v-tooltip="t('achievements.tiers', { done: a.claimedTiers, total: a.tiers.length })">
                                <span
                                    v-for="(tier, index) in a.tiers"
                                    :key="index"
                                    class="ach-pip"
                                    :class="{ 'ach-pip-claimed': tier.claimed, 'ach-pip-reached': tier.reached && !tier.claimed, 'ach-pip-frame': !!tier.frame }"
                                />
                            </span>
                        </div>
                        <span class="ach-text">{{ finished(a) ? t('achievements.finished') : text(a) }}</span>
                        <div class="ach-progress">
                            <div class="ach-progress-fill" :style="{ width: `${fill(a)}%` }" />
                            <span class="ach-progress-text">{{ format(a.progress) }} / {{ format(currentTier(a).target) }}</span>
                        </div>
                    </div>

                    <RewardChips
                        v-if="!finished(a)"
                        class="ach-reward"
                        :reward="currentTier(a).reward"
                        :rainbow-coins="currentTier(a).rainbowCoins"
                        :elder-seals="currentTier(a).elderSeals"
                        :frame="currentTier(a).frame"
                    />

                    <div class="ach-actions">
                        <span v-if="finished(a)" class="ach-done"><IconCheck /> {{ t('quests.done') }}</span>
                        <GemButton v-else color="green" :disabled="!claimable(a) || busy" @click="claim(a)">{{ t('quests.claim') }}</GemButton>
                    </div>
                </li>
            </ul>
        </template>
    </div>
</template>

<style scoped>
.achievements {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 12px;
    padding: 4px 6px 10px;
    font-family: 'Arial', sans-serif;
    color: #f8fafc;
}

.ach-message {
    color: #bbf7d0;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.ach-message-error {
    color: #ffb4b4;
}

.ach-head {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px 16px;
    padding: 12px 16px;
    border-radius: 14px;
    background: rgba(0, 0, 0, 0.3);
    box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.6);
}

.ach-total {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    color: #fde68a;
    font-size: 16px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.ach-hint {
    flex: 1;
    min-width: 160px;
    color: #cbd5e1;
    font-size: 12px;
}

.ach-list {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    margin: 0;
    padding: 0;
    list-style: none;
}

.ach {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto auto;
    align-items: center;
    column-gap: 14px;
    padding: 10px 14px;
    border-radius: 12px;
    background: rgba(0, 0, 0, 0.3);
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6), 0 0 0 1px #64748b;
}

.ach-ready {
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6), 0 0 0 2px #fde68a, 0 0 12px rgba(253, 224, 71, 0.35);
}

.ach-finished {
    opacity: 0.6;
}

.ach-body {
    display: flex;
    flex-direction: column;
    row-gap: 5px;
    min-width: 0;
}

.ach-title-row {
    display: flex;
    align-items: center;
    column-gap: 10px;
}

.ach-title {
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.ach-tiers {
    display: inline-flex;
    column-gap: 4px;
}

.ach-pip {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background: #1e293b;
    box-shadow: 0 0 0 1px #64748b;
}

.ach-pip-frame {
    border-radius: 2px;
    transform: rotate(45deg);
}

.ach-pip-reached {
    background: #f59e0b;
    box-shadow: 0 0 0 1px #fde68a, 0 0 6px rgba(253, 224, 71, 0.8);
}

.ach-pip-claimed {
    background: #4ade80;
    box-shadow: 0 0 0 1px #166534;
}

.ach-text {
    color: #cbd5e1;
    font-size: 12px;
}

.ach-progress {
    position: relative;
    height: 14px;
    border-radius: 7px;
    background: rgba(255, 255, 255, 0.08);
    overflow: hidden;
}

.ach-progress-fill {
    height: 100%;
    background: linear-gradient(to right, #f59e0b, #fde047);
    opacity: 0.8;
    transition: width 0.4s ease;
}

.ach-progress-text {
    position: absolute;
    inset: 0;
    display: grid;
    place-items: center;
    font-size: 11px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.9);
    font-variant-numeric: tabular-nums;
}

.ach-reward {
    justify-content: flex-end;
    max-width: 260px;
}

.ach-actions {
    display: flex;
    align-items: center;
}

.ach-done {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    color: #4ade80;
    font-size: 14px;
    font-weight: bold;
}

@media (max-width: 760px) {
    .ach {
        grid-template-columns: minmax(0, 1fr) auto;
        row-gap: 8px;
    }

    .ach-reward {
        grid-column: 1;
        justify-content: flex-start;
    }
}
</style>
