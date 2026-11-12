<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import SocialApiService from '@/services/apiServices/SocialApiService';
import { useResources } from '@/composables/UseResources';
import { usePvp } from '@/composables/UsePvp';
import { useAuth } from '@/composables/UseAuth';
import type { ArenaBetMatchInfo, ArenaBetOverview } from '@/models/types/Social';
import { formatLeft, formatNumber, openExternalFight, socialErrorText } from '@/components/social/socialShared';
import { IconBloodCoin, IconTrophy } from '@/components/icons/ui';
import StatIcon from '@/components/icons/stats/StatIcon.vue';

// «Бой дня»: ставки кровавых монет на бой двух сильнейших отрядов (сервер: ArenaBetRules)
const emit = defineEmits<{
    (e: 'navigate'): void;
    (e: 'message', text: string, isError: boolean): void;
}>();

const { t } = useI18n();
const router = useRouter();
const { set: setResources } = useResources();
const { pvpOpen, pvpTab } = usePvp();

const overview = ref<ArenaBetOverview | null>(null);
const busy = ref(false);
const loadError = ref(false);
const amount = ref(20);
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;

const apply = (next: ArenaBetOverview) => {
    overview.value = next;
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
    setResources(next.resources);
};

const load = async () => {
    try {
        apply(await SocialApiService.getBets());
        loadError.value = false;
    } catch (error) {
        loadError.value = true;
        emit('message', socialErrorText(error, 'bets'), true);
    }
};

onMounted(() => {
    load();
    ticker = window.setInterval(() => {
        now.value = Date.now();
        // Окно закончилось — бой проведён, перечитываем
        const closes = overview.value?.current?.closesAt;
        if (closes && new Date(closes).getTime() <= now.value + clockOffset && !busy.value) load();
    }, 1000);
});
onBeforeUnmount(() => window.clearInterval(ticker));

const current = computed(() => overview.value?.current ?? null);
const previous = computed(() => overview.value?.previous ?? null);
const coins = computed(() => overview.value?.resources.bloodCoins ?? 0);
const myUserId = computed(() => useAuth().user.value?.id ?? null);
const isOwnFight = (match: ArenaBetMatchInfo) => !!myUserId.value && (match.a.userId === myUserId.value || match.b.userId === myUserId.value);
const closesIn = computed(() => (current.value ? formatLeft(new Date(current.value.closesAt).getTime() - now.value - clockOffset) : ''));
const sideName = (match: ArenaBetMatchInfo, side: 1 | 2) => (side === 1 ? match.a.name : match.b.name);

const run = async (action: () => Promise<ArenaBetOverview>, done: (next: ArenaBetOverview) => string) => {
    if (busy.value) return;
    busy.value = true;
    try {
        const next = await action();
        apply(next);
        emit('message', done(next), false);
    } catch (error) {
        emit('message', socialErrorText(error, 'bets'), true);
    } finally {
        busy.value = false;
    }
};

const bet = (side: 1 | 2) => run(() => SocialApiService.placeBet(side, amount.value), () => t('social.bets.placed'));
const claim = () => {
    const claimable = overview.value?.claimable ?? 0;
    return run(() => SocialApiService.claimBets(), () => t('social.bets.claimed', { coins: claimable }));
};

const watch = (match: ArenaBetMatchInfo) => {
    emit('navigate');
    openExternalFight(router, {
        title: t('social.bets.replayTitle', { a: match.a.name, b: match.b.name }),
        load: async () => {
            const replay = await SocialApiService.replayBet(match.windowStart);
            return { combat: replay.combat, background: replay.background };
        },
        backLabel: t('social.bets.back'),
        back: () => {
            pvpTab.value = 'bets';
            pvpOpen.value = true;
        },
    });
};
</script>

<template>
    <div class="bets">
        <p class="bets-hint">{{ t('social.bets.hint', { hours: 6, cut: Math.round((overview?.houseCut ?? 0.1) * 100) }) }}</p>

        <div v-if="!overview" class="bets-hint">{{ loadError ? t('social.loadError') : t('common.loading') }}</div>
        <template v-else>
            <div class="bets-bar">
                <span class="bets-coins"><IconBloodCoin /> {{ formatNumber(coins) }}</span>
                <GemButton v-if="overview.claimable > 0" :color="'yellow'" :disabled="busy" @click="claim">
                    <IconBloodCoin /> {{ t('social.bets.claim', { coins: formatNumber(overview.claimable) }) }}
                </GemButton>
            </div>

            <!-- Текущий бой: две стороны и ставка -->
            <section v-if="current" class="bets-match">
                <div class="bets-match-head">
                    <h4>{{ t('social.bets.title') }}</h4>
                    <span v-if="current.open" class="bets-muted">{{ t('social.bets.closesIn', { time: closesIn }) }}</span>
                </div>
                <div class="bets-sides">
                    <div v-for="side in ([1, 2] as const)" :key="side" class="bets-side" :class="{ 'bets-side-mine': current.myBet?.side === side }">
                        <template v-for="info in [side === 1 ? current.a : current.b]" :key="info.userId">
                            <span class="bets-name">{{ info.name }}</span>
                            <span class="bets-muted"><StatIcon stat="power" /> {{ t('social.bets.power', { power: formatNumber(info.power) }) }}</span>
                            <div class="bets-units">
                                <UnitPortrait v-for="unit in info.units" :key="unit.entityId" :unit="unit" :size="40" />
                            </div>
                            <span class="bets-muted">
                                {{ t('social.bets.pool', { pool: formatNumber(info.pool), bettors: info.bettors, multiplier: info.multiplier ? info.multiplier.toFixed(2) : '—' }) }}
                            </span>
                            <GemButton
                                v-if="current.open && !current.myBet && !isOwnFight(current)"
                                :color="side === 1 ? 'red' : 'blue'"
                                :disabled="busy || coins < amount"
                                @click="bet(side)"
                            >{{ t('social.bets.betOn', { name: info.name }) }}</GemButton>
                        </template>
                    </div>
                </div>
                <div v-if="current.open && !current.myBet && !isOwnFight(current)" class="bets-amount">
                    <span>{{ t('social.bets.amount') }}:</span>
                    <input v-model.number="amount" type="range" :min="overview.minBet" :max="overview.maxBet" step="5" />
                    <span class="bets-coins"><IconBloodCoin /> {{ amount }}</span>
                </div>
                <p v-if="current.myBet" class="bets-mybet">
                    {{ t('social.bets.myBet', { amount: current.myBet.amount, name: sideName(current, current.myBet.side) }) }}
                </p>
                <p v-else-if="isOwnFight(current)" class="bets-muted">{{ t('social.bets.ownFight') }}</p>
            </section>
            <div v-else class="bets-hint">{{ t('social.bets.noMatch') }}</div>

            <!-- Прошлый бой: итог, выплата, повтор -->
            <section v-if="previous" class="bets-match bets-previous">
                <div class="bets-match-head">
                    <h4>{{ t('social.bets.previous') }}</h4>
                    <span class="bets-muted">{{ previous.a.name }} vs {{ previous.b.name }}</span>
                </div>
                <p v-if="previous.winner" class="bets-winner">
                    <IconTrophy /> {{ t('social.bets.winner', { name: sideName(previous, previous.winner), rounds: previous.rounds }) }}
                </p>
                <p v-if="previous.myBet" class="bets-muted">
                    {{ t('social.bets.myBet', { amount: previous.myBet.amount, name: sideName(previous, previous.myBet.side) }) }}
                    <template v-if="previous.myBet.payout !== null"> · {{ t('social.bets.payout', { payout: previous.myBet.payout }) }}</template>
                </p>
                <GemButton :color="'green'" @click="watch(previous)">{{ t('social.bets.watch') }}</GemButton>
            </section>
        </template>
    </div>
</template>

<style scoped>
.bets {
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 6px;
    font-family: 'Arial', sans-serif;
    text-align: left;
    color: #f8fafc;
    font-size: 13px;
}

.bets-hint,
.bets-muted {
    margin: 0;
    color: #cbd5e1;
    font-size: 12px;
}

.bets-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
}

.bets-coins {
    font-weight: bold;
    color: #fca5a5;
}

.bets-match {
    padding: 10px 12px;
    border-radius: 12px;
    border: 2px solid rgba(239, 68, 68, 0.5);
    background: linear-gradient(135deg, rgba(60, 14, 14, 0.85), rgba(15, 23, 32, 0.85));
}

.bets-previous {
    border-color: rgba(157, 173, 175, 0.5);
    background: rgba(15, 23, 32, 0.7);
}

.bets-match-head {
    display: flex;
    align-items: baseline;
    justify-content: space-between;
    gap: 10px;
    margin-bottom: 8px;
}

.bets-match-head h4 {
    margin: 0;
    font-size: 15px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.bets-sides {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
}

@media (max-width: 640px) {
    .bets-sides {
        grid-template-columns: 1fr;
    }
}

.bets-side {
    display: flex;
    flex-direction: column;
    row-gap: 6px;
    padding: 8px;
    border-radius: 10px;
    border: 2px solid transparent;
    background: rgba(0, 0, 0, 0.3);
}

.bets-side-mine {
    border-color: #fbbf24;
}

.bets-name {
    font-size: 15px;
    font-weight: bold;
}

.bets-units {
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
}

.bets-amount {
    display: flex;
    align-items: center;
    gap: 10px;
    margin-top: 8px;
}

.bets-amount input {
    flex: 1;
}

.bets-mybet {
    margin: 8px 0 0;
    color: #fde68a;
    font-weight: bold;
}

.bets-winner {
    margin: 0 0 6px;
    color: #fde68a;
    font-weight: bold;
}
</style>
