<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import TowerRewardLine from '@/components/tower/TowerRewardLine.vue';
import type { RewardNotice } from '@/composables/UseStageRewards';

// Футер окна кампании и башни: слева — что только что получено (или ошибка, или строка состояния),
// справа — «Забрать всё». Кнопка горит, пока есть незабранные награды, и видна при любой прокрутке списка
const props = defineProps<{
    remaining: number;
    busy: boolean;
    notice: RewardNotice | null;
    error: boolean;
}>();

const emit = defineEmits<{
    (e: 'claimAll'): void;
    (e: 'dismiss'): void;
}>();

const { t } = useI18n();

const noticeTitle = computed(() => props.notice?.skippedTo
    ? t('tower.skipped', { floor: props.notice.skippedTo })
    : t('rewards.claimedTitle'));
</script>

<template>
    <div class="rewards-footer">
        <div class="rewards-footer-info">
            <div v-if="props.notice" class="rewards-notice">
                <span class="rewards-notice-title">{{ noticeTitle }}</span>
                <TowerRewardLine :reward="props.notice.reward" />
                <span v-if="props.notice.reward.chests.length" class="rewards-notice-hint" v-tooltip="t('rewards.chestsHint')">ⓘ</span>
                <button class="rewards-notice-close" v-tooltip="t('common.close')" @click="emit('dismiss')">✕</button>
            </div>
            <span v-else-if="props.error" class="rewards-footer-error">{{ t('rewards.claimError') }}</span>
            <span v-else class="rewards-footer-status">
                {{ props.remaining ? t('rewards.pendingCount', { count: props.remaining }) : t('rewards.allClaimed') }}
            </span>
        </div>

        <GemButton
            :color="'yellow'"
            class="rewards-claim-all"
            :class="{ 'rewards-claim-all-ready': props.remaining > 0 && !props.busy }"
            :disabled="props.busy || !props.remaining"
            :badge-count="props.remaining"
            v-tooltip="props.remaining ? undefined : t('rewards.nothingToClaim')"
            @click="emit('claimAll')"
        >🎁 {{ props.busy ? t('rewards.claiming') : t('rewards.claimAll') }}</GemButton>
    </div>
</template>

<style scoped>
.rewards-footer {
    flex: 1;
    min-width: 0;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
}

.rewards-footer-info {
    flex: 1;
    min-width: 0;
}

.rewards-notice {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 4px 10px;
    padding: 4px 12px;
    border: 1px solid rgba(255, 209, 102, 0.6);
    border-radius: 10px;
    background: rgba(14, 19, 34, 0.75);
    animation: rewards-notice-in 0.35s cubic-bezier(0.34, 1.56, 0.64, 1);
}

@keyframes rewards-notice-in {
    from { opacity: 0; transform: translateY(6px); }
}

.rewards-notice-title {
    color: #ffd166;
    font-weight: 700;
    text-shadow: 0 1px 3px #000;
}

.rewards-notice-hint {
    color: #cbd5e1;
    cursor: help;
}

.rewards-notice-close {
    margin-left: auto;
    border: none;
    background: none;
    color: #cbd5e1;
    cursor: pointer;
}

.rewards-footer-status {
    color: #cbd5e1;
    font-size: 14px;
    text-shadow: 0 1px 3px #000;
}

.rewards-footer-error {
    color: #fca5a5;
    font-size: 14px;
}

.rewards-claim-all {
    flex-shrink: 0;
}

/* Есть что забрать — кнопка пульсирует золотом */
.rewards-claim-all-ready {
    animation: rewards-claim-glow 1.6s ease-in-out infinite;
}

@keyframes rewards-claim-glow {
    0%, 100% { box-shadow: 0 8px 15px rgba(0, 0, 0, 0.4), 0 0 6px 1px rgba(255, 190, 70, 0.4); }
    50% { box-shadow: 0 8px 15px rgba(0, 0, 0, 0.4), 0 0 20px 6px rgba(255, 190, 70, 0.9); }
}

@media (prefers-reduced-motion: reduce) {
    .rewards-claim-all-ready,
    .rewards-notice {
        animation: none;
    }
}
</style>
