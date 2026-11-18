<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import TimerBadge from '@/components/common/TimerBadge.vue';
import LoadingCrystal from '@/components/loading/LoadingCrystal.vue';
import RewardChips from './RewardChips.vue';
import { grantedText } from './rewardText';
import AchievementsApiService from '@/services/apiServices/AchievementsApiService';
import { useRewards } from '@/composables/UseRewards';
import { useResources } from '@/composables/UseResources';
import { formatCountdown } from '@/components/events/eventShared';
import { IconCheck, IconFlame } from '@/components/icons/ui';
import type { LoginCalendarInfo, LoginDayInfo } from '@/models/types/Achievements';

// Календарь входов: 28 дней, каждый день подряд — следующая награда, к концу недели и цикла всё лучше.
// Пропуск обнуляет цикл (сервер: Services/LoginCalendar.cs)
const { t, te } = useI18n();
const { calendar, setCalendar, refreshAchievements } = useRewards();
const { set: setResources } = useResources();

const loading = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const errorText = (error: unknown, fallback: string) => {
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`achievements.errors.${data.code}`)) return t(`achievements.errors.${data.code}`);
    return data?.message ?? fallback;
};

/* ===== Время ===== */
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;
const msLeft = computed(() => (calendar.value ? new Date(calendar.value.resetAt).getTime() - (now.value + clockOffset) : 0));

const apply = (next: LoginCalendarInfo) => {
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
    now.value = Date.now();
    setCalendar(next);
    setResources(next.resources);
};

const load = async () => {
    loading.value = true;
    try {
        apply(await AchievementsApiService.getCalendar());
    } catch (error) {
        message.value = { text: errorText(error, t('achievements.loadError')), error: true };
    } finally {
        loading.value = false;
    }
};

const claim = async () => {
    if (busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        const result = await AchievementsApiService.claimCalendar();
        apply(result.calendar);
        message.value = { text: t('achievements.claimed', { reward: grantedText(result.granted) }), error: false };
        // Дни входа и серия — тоже достижения
        refreshAchievements();
    } catch (error) {
        message.value = { text: errorText(error, t('achievements.actionError')), error: true };
        await load();
    } finally {
        busy.value = false;
    }
};

onMounted(() => {
    load();
    ticker = window.setInterval(() => {
        now.value = Date.now();
        // Наступили новые сутки — открылась следующая награда
        if (calendar.value && !loading.value && !busy.value && msLeft.value <= 0) load();
    }, 1000);
});

onBeforeUnmount(() => window.clearInterval(ticker));

/* ===== Вид ===== */
const weeks = computed(() => {
    const days = calendar.value?.days ?? [];
    const rows: LoginDayInfo[][] = [];
    for (let i = 0; i < days.length; i += 7) rows.push(days.slice(i, i + 7));
    return rows;
});

const canClaim = computed(() => !!calendar.value && !calendar.value.claimedToday);
</script>

<template>
    <div class="login-calendar">
        <div v-if="message" class="login-message" :class="{ 'login-message-error': message.error }">{{ message.text }}</div>

        <LoadingCrystal v-if="loading && !calendar" />

        <template v-else-if="calendar">
            <section class="login-head">
                <div class="login-head-text">
                    <span class="login-title">{{ t('loginCalendar.title', { cycle: calendar.cycle }) }}</span>
                    <span class="login-hint">
                        {{ calendar.missedDaysAllowed > 0
                            ? t('loginCalendar.hintGrace', { days: calendar.missedDaysAllowed })
                            : t('loginCalendar.hint') }}
                    </span>
                </div>
                <span class="login-streak" v-tooltip="t('loginCalendar.bestStreak', { best: calendar.bestStreak, total: calendar.totalDays })">
                    <IconFlame /> {{ t('loginCalendar.streak', { days: calendar.streak }) }}
                </span>
                <TimerBadge
                    v-if="calendar.claimedToday"
                    class="login-timer"
                    v-tooltip="t('loginCalendar.resetHint')"
                    :text="t('loginCalendar.nextIn', { time: formatCountdown(msLeft, t('fixedShop.day')) })"
                />
                <GemButton v-else class="login-claim" color="green" :disabled="busy" @click="claim">
                    {{ t('loginCalendar.claim', { day: calendar.day }) }}
                </GemButton>
            </section>

            <p v-if="calendar.broken && !calendar.claimedToday" class="login-broken">{{ t('loginCalendar.broken') }}</p>

            <div class="login-grid">
                <template v-for="(week, w) in weeks" :key="w">
                    <button
                        v-for="day in week"
                        :key="day.day"
                        type="button"
                        class="login-day"
                        :class="{
                            'login-day-claimed': day.claimed,
                            'login-day-today': day.today && !day.claimed,
                            'login-day-big': day.big,
                        }"
                        :disabled="!(day.today && canClaim) || busy"
                        @click="claim"
                    >
                        <span class="login-day-number">{{ t('loginCalendar.day', { day: day.day }) }}</span>
                        <RewardChips class="login-day-reward" compact :reward="day.reward" :rainbow-coins="day.rainbowCoins" :elder-seals="day.elderSeals" />
                        <span v-if="day.claimed" class="login-day-check"><IconCheck /></span>
                    </button>
                </template>
            </div>
        </template>
    </div>
</template>

<style scoped>
.login-calendar {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 12px;
    padding: 4px 6px 10px;
    font-family: 'Arial', sans-serif;
    color: #f8fafc;
}

.login-message {
    color: #bbf7d0;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.login-message-error {
    color: #ffb4b4;
}

.login-head {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px 18px;
    padding: 12px 16px;
    border-radius: 14px;
    background: rgba(0, 0, 0, 0.3);
    box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.6);
}

.login-head-text {
    display: flex;
    flex-direction: column;
    row-gap: 4px;
    flex: 1;
    min-width: 200px;
}

.login-title {
    color: #fde68a;
    font-size: 16px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.login-hint {
    color: #cbd5e1;
    font-size: 12px;
}

.login-streak {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    color: #fdba74;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
    --split-icon-primary: #fb923c;
    --split-icon-secondary: #9a3412;
}

.login-broken {
    margin: 0;
    color: #fca5a5;
    font-size: 13px;
    font-style: italic;
}

.login-grid {
    display: grid;
    grid-template-columns: repeat(7, minmax(0, 1fr));
    gap: 8px;
}

.login-day {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: space-between;
    row-gap: 6px;
    min-height: 84px;
    padding: 8px 4px;
    border: none;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.3);
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6), 0 0 0 1px #64748b;
    color: inherit;
    font: inherit;
    cursor: default;
}

.login-day-number {
    color: #cbd5e1;
    font-size: 11px;
    font-weight: bold;
    text-transform: uppercase;
}

.login-day-big {
    background: linear-gradient(to bottom, rgba(245, 158, 11, 0.25), rgba(0, 0, 0, 0.35));
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6), 0 0 0 2px #f59e0b;
}

.login-day-big .login-day-number {
    color: #fde68a;
}

.login-day-today {
    cursor: pointer;
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6), 0 0 0 2px #fde68a, 0 0 16px rgba(253, 224, 71, 0.6);
    animation: login-pulse 1.4s ease-in-out infinite;
}

.login-day-today:disabled {
    cursor: default;
}

.login-day-claimed .login-day-reward {
    opacity: 0.4;
}

.login-day-check {
    position: absolute;
    inset: 0;
    display: grid;
    place-items: center;
    font-size: 30px;
    --split-icon-primary: #4ade80;
    --split-icon-secondary: #166534;
}

@keyframes login-pulse {
    50% { transform: scale(1.04); }
}

@media (max-width: 760px) {
    .login-grid {
        gap: 4px;
    }

    .login-day {
        min-height: 64px;
        padding: 4px 2px;
    }
}
</style>
