<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import TowerRewardLine from '@/components/tower/TowerRewardLine.vue';
import type { RewardNotice } from '@/composables/UseStageRewards';
import { IconBanner, IconChest } from '@/components/icons/ui';

// Шапка окон кампании и башни (футера у них нет): слева — прогресс (слот), справа — «Сменить отряд» и «Забрать всё».
// Под шапкой — что только что получено или ошибка сбора; ниже — то, что окно положит в слот по умолчанию
const props = defineProps<{
    remaining: number;
    busy: boolean;
    notice: RewardNotice | null;
    error: boolean;
}>();

const emit = defineEmits<{
    (e: 'claimAll'): void;
    (e: 'dismiss'): void;
    (e: 'squad'): void;
}>();

const { t } = useI18n();

const noticeTitle = computed(() => props.notice?.skippedTo
    ? t('tower.skipped', { floor: props.notice.skippedTo })
    : t('rewards.claimedTitle'));
</script>

<template>
    <div class="stage-header">
        <div class="stage-header-row">
            <div class="stage-header-info">
                <slot name="info" />
            </div>
            <div class="stage-header-actions">
                <GemButton :color="'pink'" v-tooltip="t('tower.squadHint')" @click="emit('squad')">
                    <IconBanner /> {{ t('tower.squad') }}
                </GemButton>
                <GemButton
                    :color="'yellow'"
                    class="stage-claim-all"
                    :class="{ 'stage-claim-all-ready': props.remaining > 0 && !props.busy }"
                    :disabled="props.busy || !props.remaining"
                    :badge-count="props.remaining"
                    v-tooltip="props.remaining ? t('rewards.pendingCount', { count: props.remaining }) : t('rewards.nothingToClaim')"
                    @click="emit('claimAll')"
                ><IconChest /> {{ props.busy ? t('rewards.claiming') : t('rewards.claimAll') }}</GemButton>
            </div>
        </div>

        <div v-if="props.notice" class="stage-notice">
            <span class="stage-notice-title">{{ noticeTitle }}</span>
            <TowerRewardLine :reward="props.notice.reward" />
            <span v-if="props.notice.reward.chests.length" class="stage-notice-hint" v-tooltip="t('rewards.chestsHint')">ⓘ</span>
            <button class="stage-notice-close" v-tooltip="t('common.close')" @click="emit('dismiss')">✕</button>
        </div>
        <div v-else-if="props.error" class="stage-header-error">{{ t('rewards.claimError') }}</div>

        <slot />
    </div>
</template>

<style scoped>
.stage-header {
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    gap: 8px;
}

/* Как заголовки отряда в гильдии и на арене: сведения слева, кнопки прижаты вправо */
.stage-header-row {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px 14px;
}

.stage-header-info {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px 14px;
}

.stage-header-actions {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    column-gap: 8px;
    margin-left: auto;
}

/* Есть что забрать — кнопка пульсирует золотом */
.stage-claim-all-ready {
    animation: stage-claim-glow 1.6s ease-in-out infinite;
}

@keyframes stage-claim-glow {
    0%, 100% { box-shadow: 0 8px 15px rgba(0, 0, 0, 0.4), 0 0 6px 1px rgba(255, 190, 70, 0.4); }
    50% { box-shadow: 0 8px 15px rgba(0, 0, 0, 0.4), 0 0 20px 6px rgba(255, 190, 70, 0.9); }
}

.stage-notice {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 4px 10px;
    padding: 4px 12px;
    border: 1px solid rgba(255, 209, 102, 0.6);
    border-radius: 10px;
    background: rgba(14, 19, 34, 0.75);
    animation: stage-notice-in 0.35s cubic-bezier(0.34, 1.56, 0.64, 1);
}

@keyframes stage-notice-in {
    from { opacity: 0; transform: translateY(-6px); }
}

.stage-notice-title {
    color: #ffd166;
    font-weight: 700;
    text-shadow: 0 1px 3px #000;
}

.stage-notice-hint {
    color: #cbd5e1;
    cursor: help;
}

.stage-notice-close {
    margin-left: auto;
    border: none;
    background: none;
    color: #cbd5e1;
    cursor: pointer;
}

.stage-header-error {
    color: #fca5a5;
    font-size: 14px;
    font-weight: bold;
}

@media (prefers-reduced-motion: reduce) {
    .stage-claim-all-ready,
    .stage-notice {
        animation: none;
    }
}
</style>
