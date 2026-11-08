<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import { arenaPreview } from '@/battle/arenaBackgrounds';
import { IconPower } from '@/components/icons/stats';
import { IconFang, IconSkull, IconTrophy } from '@/components/icons/ui';
import EventsApiService from '@/services/apiServices/EventsApiService';
import { formatCountdown } from './eventShared';
import type { WorldEventInfo } from '@/models/types/FixedShop';
import type { WorldBossOverview } from '@/models/types/WorldEvents';

// Ивент «Мировые боссы»: один босс на весь сервер. Удар — бой отрядом на арене (как босс гильдии),
// за каждый удар — клыки, за убитого босса — награда всем, кто бил, по урону. Убили — сразу следующий, сильнее
const emit = defineEmits<{
    (e: 'navigate'): void;
    (e: 'event', event: WorldEventInfo): void;
}>();

const { t, te, locale } = useI18n();
const router = useRouter();

const overview = ref<WorldBossOverview | null>(null);
const loading = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const format = (value: number) => Math.round(value).toLocaleString(locale.value);

const errorText = (error: unknown, fallback: string) => {
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`worldBoss.errors.${data.code}`)) return t(`worldBoss.errors.${data.code}`);
    return data?.message ?? fallback;
};

/* ===== Время ===== */
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;
let poller = 0;

const cooldownLeft = computed(() => {
    const at = overview.value?.nextAttackAt;
    return at ? new Date(at).getTime() - (now.value + clockOffset) : 0;
});

/* ===== Загрузка и действия ===== */
const apply = (next: WorldBossOverview) => {
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
    now.value = Date.now();
    overview.value = next;
    emit('event', next.event);
};

const load = async () => {
    loading.value = true;
    try {
        apply(await EventsApiService.getWorldBoss());
    } catch (error) {
        message.value = { text: errorText(error, t('worldBoss.loadError')), error: true };
    } finally {
        loading.value = false;
    }
};

const claim = async () => {
    if (busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        const result = await EventsApiService.claimWorldBoss();
        apply(result.overview);
        message.value = { text: t('worldBoss.claimed', { amount: format(result.currency) }), error: false };
    } catch (error) {
        message.value = { text: errorText(error, t('worldBoss.actionError')), error: true };
        await load();
    } finally {
        busy.value = false;
    }
};

// Бой показывает арена; attack — разовая метка, повторный удар с открытой арены тоже запускает бой
const attack = () => {
    emit('navigate');
    router.push({ name: 'arena', query: { worldBoss: '1', attack: Date.now() } });
};

onMounted(() => {
    load();
    ticker = window.setInterval(() => { now.value = Date.now(); }, 1000);
    // Босса бьёт весь сервер — HP и таблица меняются без нас
    poller = window.setInterval(() => { if (!busy.value) load(); }, 20_000);
});

onBeforeUnmount(() => {
    window.clearInterval(ticker);
    window.clearInterval(poller);
});

/* ===== Вид ===== */
const boss = computed(() => overview.value?.boss ?? null);
const hpPercent = computed(() => (boss.value && boss.value.maxHp > 0 ? (boss.value.currentHp / boss.value.maxHp) * 100 : 0));
const pendingTotal = computed(() => overview.value?.pending.reduce((sum, p) => sum + p.currency, 0) ?? 0);
const canAttack = computed(() => !!overview.value?.open && !!boss.value && !boss.value.defeatedAt && cooldownLeft.value <= 0);
</script>

<template>
    <div class="world-boss">
        <div v-if="message" class="wb-message" :class="{ 'wb-message-error': message.error }">{{ message.text }}</div>
        <div v-if="loading && !overview" class="wb-hint">{{ t('common.loading') }}</div>

        <template v-else-if="overview">
            <p v-if="!overview.open && !boss" class="wb-hint">{{ t('worldBoss.notThisWeek') }}</p>

            <div class="wb-layout">
                <!-- ===== Босс ===== -->
                <article v-if="boss" class="wb-card" :class="{ 'wb-card-defeated': boss.defeatedAt }">
                    <div class="wb-card-arena" aria-hidden="true" :style="{ backgroundImage: `url(${arenaPreview(boss.background)})` }"></div>
                    <div class="wb-card-body">
                        <span class="wb-stage">{{ t('worldBoss.stage', { stage: boss.stage }) }}</span>
                        <UnitPortrait :unit="boss.unit" :size="96" :show-level="false" />
                        <span class="wb-name">{{ boss.unit.name }}</span>
                        <span class="wb-level">{{ t('worldBoss.level', { level: boss.level }) }}<template v-if="boss.arena"> · {{ boss.arena.name }}</template></span>

                        <div class="wb-hp" v-tooltip="`${format(boss.currentHp)} / ${format(boss.maxHp)}`">
                            <div class="wb-hp-fill" :style="{ width: `${hpPercent}%` }"></div>
                            <span class="wb-hp-text">{{ format(boss.currentHp) }} / {{ format(boss.maxHp) }}</span>
                        </div>

                        <div class="wb-stats">
                            <span>{{ t('worldBoss.myDamage', { damage: format(boss.myDamage) }) }}</span>
                            <span v-tooltip="t('worldBoss.killRewardHint', { base: overview.killBase, pool: overview.sharePool, bonus: overview.lastHitBonus })">
                                <IconFang /> {{ t('worldBoss.killReward', { amount: format(boss.myKillReward || overview.killBase) }) }}
                            </span>
                            <span><IconFang /> {{ t('worldBoss.perAttack', { amount: overview.attackCurrency }) }}</span>
                        </div>

                        <div class="wb-actions">
                            <span v-if="boss.defeatedAt" class="wb-status">
                                <IconSkull /> {{ boss.killedBy ? t('worldBoss.killedBy', { name: boss.killedBy }) : t('worldBoss.defeated') }}
                            </span>
                            <span v-else-if="cooldownLeft > 0" class="wb-status">
                                {{ t('worldBoss.nextAttack', { time: formatCountdown(cooldownLeft, t('fixedShop.day')) }) }}
                            </span>
                            <GemButton
                                v-if="overview.open && !boss.defeatedAt"
                                :color="'red'"
                                :disabled="!canAttack || busy"
                                v-tooltip="t('worldBoss.attackHint', { minutes: overview.attackCooldownMinutes })"
                                @click="attack"
                            ><IconPower /> {{ t('worldBoss.attack') }}</GemButton>
                        </div>
                    </div>
                </article>

                <!-- ===== Награды и таблицы ===== -->
                <div class="wb-side">
                    <section class="wb-box">
                        <h4 class="wb-title">{{ t('worldBoss.pendingTitle') }}</h4>
                        <template v-if="overview.pending.length">
                            <ul class="wb-list">
                                <li v-for="reward in overview.pending" :key="reward.bossId">
                                    <span>{{ t('worldBoss.stage', { stage: reward.stage }) }}<template v-if="reward.lastHit"> · {{ t('worldBoss.lastHit') }}</template></span>
                                    <span class="wb-value"><IconFang /> {{ format(reward.currency) }}</span>
                                </li>
                            </ul>
                            <GemButton :color="'yellow'" :disabled="busy" @click="claim">
                                {{ t('worldBoss.claim', { amount: format(pendingTotal) }) }}
                            </GemButton>
                        </template>
                        <span v-else class="wb-muted">{{ t('worldBoss.noPending') }}</span>
                    </section>

                    <section v-if="boss" class="wb-box">
                        <h4 class="wb-title">{{ t('worldBoss.bossTop') }}</h4>
                        <ol v-if="boss.top.length" class="wb-list wb-ranking">
                            <li v-for="entry in boss.top" :key="entry.playerId">
                                <span>{{ entry.name }}</span>
                                <span class="wb-value">{{ format(entry.damage) }}</span>
                            </li>
                        </ol>
                        <span v-else class="wb-muted">{{ t('worldBoss.noDamage') }}</span>
                    </section>

                    <section class="wb-box">
                        <h4 class="wb-title"><IconTrophy /> {{ t('worldBoss.eventTop') }}</h4>
                        <ol v-if="overview.leaderboard.length" class="wb-list wb-ranking">
                            <li v-for="entry in overview.leaderboard" :key="entry.playerId">
                                <span>{{ entry.name }}</span>
                                <span class="wb-value">{{ format(entry.damage) }}</span>
                            </li>
                        </ol>
                        <span v-else class="wb-muted">{{ t('worldBoss.noDamage') }}</span>
                        <span v-if="overview.myRank" class="wb-mine">{{ t('worldBoss.myRank', { rank: overview.myRank, damage: format(overview.myEventDamage) }) }}</span>
                    </section>
                </div>
            </div>
        </template>
    </div>
</template>

<style scoped>
.world-boss {
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 0 6px 10px;
    font-family: 'Arial', sans-serif;
    color: #f8fafc;
}

.wb-message {
    color: #bbf7d0;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.wb-message-error {
    color: #ffb4b4;
}

.wb-hint,
.wb-muted {
    color: #cbd5e1;
    font-size: 13px;
}

.wb-layout {
    display: grid;
    grid-template-columns: minmax(280px, 1.2fr) minmax(240px, 1fr);
    gap: 14px;
    align-items: start;
}

.wb-card {
    position: relative;
    overflow: hidden;
    border-radius: 14px;
    box-shadow: inset 0 0 14px rgba(0, 0, 0, 0.8), 0 0 0 2px rgba(252, 165, 165, 0.45), 0 0 18px rgba(185, 28, 28, 0.35);
}

.wb-card-arena {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center;
    filter: saturate(0.6) brightness(0.45);
}

.wb-card-defeated .wb-card-arena {
    filter: grayscale(1) brightness(0.35);
}

.wb-card-body {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 8px;
    padding: 16px 18px 18px;
}

.wb-stage {
    padding: 2px 10px;
    border-radius: 8px;
    background: rgba(127, 29, 29, 0.85);
    color: #fecaca;
    font-size: 12px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.wb-name {
    font-size: 20px;
    font-weight: bold;
    text-shadow: 0 0 10px rgba(185, 28, 28, 0.7), 1px 1px 0 #000;
}

.wb-level {
    color: #e2e8f0;
    font-size: 13px;
}

.wb-hp {
    position: relative;
    width: 100%;
    height: 22px;
    border-radius: 11px;
    background: rgba(0, 0, 0, 0.6);
    box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.9), 0 0 0 1px rgba(255, 255, 255, 0.15);
    overflow: hidden;
}

.wb-hp-fill {
    height: 100%;
    background: linear-gradient(to bottom, #f87171, #b91c1c);
    transition: width 0.5s ease;
}

.wb-hp-text {
    position: absolute;
    inset: 0;
    display: grid;
    place-items: center;
    font-size: 12px;
    font-weight: bold;
    text-shadow: 1px 1px 0 #000;
    font-variant-numeric: tabular-nums;
}

.wb-stats {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 4px 14px;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 #000;
}

.wb-stats > span {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
}

.wb-actions {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    justify-content: center;
    gap: 8px 12px;
    margin-top: 4px;
}

.wb-status {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    padding: 4px 10px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.6);
    color: #fde68a;
    font-size: 13px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.wb-side {
    display: flex;
    flex-direction: column;
    row-gap: 12px;
}

.wb-box {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    padding: 10px 14px;
    border-radius: 12px;
    background: rgba(0, 0, 0, 0.35);
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6);
}

.wb-title {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    margin: 0;
    color: #fca5a5;
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.wb-list {
    display: flex;
    flex-direction: column;
    row-gap: 4px;
    margin: 0;
    padding: 0;
    list-style: none;
    font-size: 13px;
}

.wb-list li {
    display: flex;
    justify-content: space-between;
    column-gap: 10px;
}

.wb-ranking {
    counter-reset: rank;
}

.wb-ranking li::before {
    counter-increment: rank;
    content: counter(rank) '.';
    min-width: 22px;
    color: #94a3b8;
}

.wb-ranking li > span:first-child {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.wb-value {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.wb-mine {
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
}

@media (max-width: 760px) {
    .wb-layout {
        grid-template-columns: 1fr;
    }
}
</style>
