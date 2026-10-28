<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import PvpApiService from '@/services/apiServices/PvpApiService';
import { PvpBattleInfo, PvpOpponentInfo, PvpOverview, PvpSeasonInfo } from '@/models/types/Pvp';
import type { ModalTab } from '@/models/types/Modal';
import { forecastVerdict } from '@/battle/power';
import { arenaBackground, arenaBackgroundFilter } from '@/battle/arenaBackgrounds';
import { useSquadModal } from '@/composables/UseSquadModal';
import { formatTime } from '@/components/guild/guildShared';
import { formatSeasonLeft, pvpErrorText, signed } from './pvpShared';

// PvP: 5 соперников по мощи отряда (±10%, а если игроков мало — шире), бой на арене нарастающей ярости,
// очки в дневном, недельном и месячном сезонах. Побеждённый соперник пропадает из подбора до 00:00 UTC
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t, locale } = useI18n();
const router = useRouter();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

/* ===== Вкладки ===== */
type PvpTab = 'fight' | 'seasons' | 'history';

const ICONS: Record<PvpTab, string> = {
    fight: '⚔',
    seasons: '🏆',
    history: '📜',
};

const tab = ref<PvpTab>('fight');
const tabs = computed<ModalTab[]>(() => (Object.keys(ICONS) as PvpTab[]).map(key => ({
    key,
    label: t(`pvp.tabs.${key}`),
    icon: ICONS[key],
})));

const activeTab = computed({
    get: () => tab.value,
    set: value => {
        tab.value = value as PvpTab;
        errorMessage.value = null;
    },
});

/* ===== Загрузка ===== */
const overview = ref<PvpOverview | null>(null);
const loading = ref(false);
const errorMessage = ref<string | null>(null);

// Таймеры считаем по часам сервера: offset — насколько они расходятся с часами браузера
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;
const serverNow = computed(() => now.value + clockOffset);
const msUntil = (iso: string) => new Date(iso).getTime() - serverNow.value;

const load = async () => {
    loading.value = true;
    try {
        const next = await PvpApiService.getOverview();
        clockOffset = new Date(next.serverTime).getTime() - Date.now();
        now.value = Date.now();
        overview.value = next;
        errorMessage.value = null;
    } catch (error) {
        errorMessage.value = pvpErrorText(error, t('pvp.loadError'));
    } finally {
        loading.value = false;
    }
};

watch(open, isOpen => {
    window.clearInterval(ticker);
    if (!isOpen) return;

    errorMessage.value = null;
    load();
    ticker = window.setInterval(() => {
        now.value = Date.now();
        // Наступили новые сутки — побеждённые вернулись, дневной сезон начался заново
        if (overview.value && !loading.value && msUntil(overview.value.dayResetAt) <= 0) load();
    }, 1000);
}, { immediate: true });

onBeforeUnmount(() => window.clearInterval(ticker));

// Окно отряда открывается поверх PvP; когда его закрыли — мощь и соперники другие
const { squadOpen } = useSquadModal();
watch(squadOpen, (isOpen, wasOpen) => {
    if (!isOpen && wasOpen && open.value) load();
});

const header = computed(() => (tab.value === 'fight' ? t('pvp.header') : t(`pvp.tabs.${tab.value}`)));

/* ===== Соперники ===== */
const format = (value: number) => value.toLocaleString(locale.value);
const noSquad = computed(() => !!overview.value && overview.value.squad.length === 0);

const chance = (opponent: PvpOpponentInfo) => Math.round(opponent.forecast.winChance * 100);
const verdict = (opponent: PvpOpponentInfo) => forecastVerdict(opponent.forecast.winChance);

// Насколько соперник сильнее/слабее: зелёный — слабее, красный — сильнее
const diffClass = (opponent: PvpOpponentInfo) =>
    opponent.powerDiffPercent > 2 ? 'pvp-diff-stronger' : opponent.powerDiffPercent < -2 ? 'pvp-diff-weaker' : 'pvp-diff-even';
const diffText = (opponent: PvpOpponentInfo) =>
    `${opponent.powerDiffPercent > 0 ? '+' : opponent.powerDiffPercent < 0 ? '−' : '±'}${Math.abs(opponent.powerDiffPercent).toFixed(1)}%`;

// Как на арене: передняя линия (позиция 1) у обеих команд ближе к центру
const squadFrontRight = computed(() => [...(overview.value?.squad ?? [])].sort((a, b) => b.position - a.position));

const fight = (opponent: PvpOpponentInfo) => {
    if (loading.value || noSquad.value) return;
    open.value = false;
    // attack — разовая метка: повторный бой, не уходя с арены, тоже запускает бой
    router.push({ name: 'arena', query: { pvp: opponent.userId, attack: Date.now() } });
};

/* ===== Сезоны ===== */
const seasons = computed(() => overview.value?.seasons ?? []);
const seasonLeft = (season: PvpSeasonInfo) => formatSeasonLeft(msUntil(season.endsAt), formatTime);

/* ===== История ===== */
const history = computed(() => overview.value?.history ?? []);
const battleTime = (battle: PvpBattleInfo) =>
    new Date(battle.foughtAt).toLocaleString(locale.value, { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });

const footerText = computed(() => {
    if (errorMessage.value) return errorMessage.value;
    if (!overview.value) return '';
    return t('pvp.resetIn', { time: formatTime(msUntil(overview.value.dayResetAt)) });
});
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="header"
        width="min(900px, calc(94vw - 140px))"
        height="min(940px, 92vh)"
    >
        <div v-if="loading && !overview" class="pvp-hint pvp-page">{{ t('pvp.searching') }}</div>

        <div v-else-if="overview" class="pvp-page">
            <!-- ===== Соперники ===== -->
            <template v-if="tab === 'fight'">
                <!-- Арена: на ней идут все бои PvP -->
                <section class="pvp-arena">
                    <div
                        class="pvp-arena-image"
                        :style="{ backgroundImage: `url(${arenaBackground(overview.arena.background)})`, filter: arenaBackgroundFilter(overview.arena.background) }"
                        aria-hidden="true"
                    ></div>
                    <div class="pvp-arena-body">
                        <span class="pvp-arena-name">🔥 {{ t('pvp.arenaName') }}</span>
                        <span class="pvp-arena-rule">
                            {{ t('pvp.arenaRule', { rounds: overview.arena.intervalRounds, bonus: overview.arena.damageBonusPercent }) }}
                        </span>
                    </div>
                </section>

                <section class="pvp-squad">
                    <div class="pvp-squad-heading">
                        <span class="pvp-subtitle">{{ t('pvp.mySquad') }}</span>
                        <span v-if="noSquad" class="pvp-error">{{ t('pvp.noSquad') }}</span>
                        <span v-else class="pvp-power">⚔ {{ t('power.squad', { power: format(overview.myPower) }) }}</span>
                        <GemButton :color="'pink'" :disabled="loading" @click="squadOpen = true">{{ t('pvp.editSquad') }}</GemButton>
                    </div>
                    <div class="pvp-team">
                        <UnitPortrait v-for="unit in squadFrontRight" :key="unit.entityId" :unit="unit" :size="44" />
                    </div>
                </section>

                <div class="pvp-hint">
                    {{ t('pvp.matchHint', { count: overview.opponentCount, spread: overview.powerSpreadPercent }) }}
                    <template v-if="overview.beatenToday"> · {{ t('pvp.beatenToday', { count: overview.beatenToday }) }}</template>
                </div>

                <div v-if="!noSquad && !overview.opponents.length" class="pvp-empty">{{ t('pvp.noOpponents') }}</div>

                <ul class="pvp-list">
                    <li v-for="opponent in overview.opponents" :key="opponent.userId" class="pvp-opponent">
                        <div class="pvp-opponent-info">
                            <span class="pvp-opponent-name">{{ opponent.username }}</span>
                            <span class="pvp-power">⚔ {{ format(opponent.power) }}</span>
                            <span class="pvp-diff" :class="diffClass(opponent)" :title="t('pvp.diffHint')">{{ diffText(opponent) }}</span>
                        </div>

                        <div class="pvp-team pvp-team-enemy">
                            <UnitPortrait v-for="unit in opponent.squad" :key="unit.entityId" :unit="unit" :size="40" />
                        </div>

                        <div class="pvp-opponent-forecast">
                            <span class="pvp-verdict" :class="`forecast-${verdict(opponent)}`" :title="t('power.chance', { chance: chance(opponent) })">
                                {{ t(`power.verdict.${verdict(opponent)}`) }} · {{ chance(opponent) }}%
                            </span>
                            <span class="pvp-stakes">
                                <span class="pvp-win">{{ signed(opponent.winPoints) }}</span>
                                /
                                <span class="pvp-loss">{{ signed(-opponent.lossPoints) }}</span>
                            </span>
                        </div>

                        <GemButton :color="'red'" :disabled="loading || noSquad" @click="fight(opponent)">⚔ {{ t('pvp.fight') }}</GemButton>
                    </li>
                </ul>
            </template>

            <!-- ===== Сезоны ===== -->
            <template v-else-if="tab === 'seasons'">
                <div class="pvp-hint">{{ t('pvp.seasonsHint') }}</div>
                <div class="pvp-seasons">
                    <article v-for="season in seasons" :key="season.type" class="pvp-season">
                        <div class="pvp-season-heading">
                            <span class="pvp-subtitle">{{ t(`pvp.seasons.${season.type}`) }}</span>
                            <span class="pvp-season-left">⏳ {{ seasonLeft(season) }}</span>
                        </div>
                        <div class="pvp-season-me">
                            <span class="pvp-season-points">{{ format(season.points) }}</span>
                            <span class="pvp-season-label">{{ t('pvp.points') }}</span>
                            <span class="pvp-season-stats">
                                {{ season.rank ? t('pvp.rank', { rank: season.rank }) : t('pvp.unranked') }}
                                · {{ t('pvp.winsLosses', { wins: season.wins, losses: season.losses }) }}
                            </span>
                        </div>
                        <ol v-if="season.top.length" class="pvp-top">
                            <li v-for="entry in season.top" :key="entry.userId" :class="{ 'pvp-top-me': entry.isMe }">
                                <span class="pvp-top-rank">{{ entry.rank }}</span>
                                <span class="pvp-top-name">{{ entry.username }}</span>
                                <span class="pvp-top-points">{{ format(entry.points) }}</span>
                            </li>
                        </ol>
                        <span v-else class="pvp-muted">{{ t('pvp.noScores') }}</span>
                    </article>
                </div>
            </template>

            <!-- ===== История ===== -->
            <template v-else>
                <div v-if="!history.length" class="pvp-empty">{{ t('pvp.noHistory') }}</div>
                <ul class="pvp-list">
                    <li v-for="battle in history" :key="battle.id" class="pvp-battle" :class="battle.won ? 'pvp-battle-won' : 'pvp-battle-lost'">
                        <span class="pvp-battle-icon" :title="battle.isAttack ? t('pvp.attack') : t('pvp.defence')">{{ battle.isAttack ? '⚔' : '🛡' }}</span>
                        <div class="pvp-battle-main">
                            <span class="pvp-opponent-name">
                                {{ battle.isAttack ? t('pvp.attackedPlayer', { name: battle.opponentName }) : t('pvp.attackedBy', { name: battle.opponentName }) }}
                            </span>
                            <span class="pvp-muted">{{ battleTime(battle) }} · ⚔ {{ format(battle.myPower) }} vs {{ format(battle.opponentPower) }}</span>
                        </div>
                        <span class="pvp-battle-result">{{ battle.won ? t('pvp.won') : t('pvp.lost') }}</span>
                        <span v-if="battle.isAttack" class="pvp-battle-points" :class="battle.points >= 0 ? 'pvp-win' : 'pvp-loss'">{{ signed(battle.points) }}</span>
                    </li>
                </ul>
            </template>
        </div>

        <template #modalFooter>
            <div class="pvp-footer">
                <span class="pvp-footer-status" :class="{ 'pvp-error': errorMessage }">{{ footerText }}</span>
                <GemButton :color="'blue'" :disabled="loading" @click="load">{{ loading ? t('pvp.searching') : t('pvp.refresh') }}</GemButton>
            </div>
        </template>
    </ModalComponent>
</template>

<style scoped>
.pvp-page {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 6px;
    font-family: 'Arial', sans-serif;
    text-align: left;
}

.pvp-hint,
.pvp-empty {
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.pvp-empty {
    padding: 18px;
    text-align: center;
}

.pvp-subtitle {
    color: #f8fafc;
    font-size: 14px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.pvp-power {
    color: #ffd166;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.pvp-error {
    color: #ffb4b4;
    font-size: 13px;
    font-weight: bold;
}

.pvp-muted {
    color: #94a3b8;
    font-size: 12px;
}

/* Арена: фон арены за описанием её правила */
.pvp-arena {
    position: relative;
    isolation: isolate;
    overflow: hidden;
    min-height: 74px;
    display: flex;
    align-items: center;
    border-radius: 12px;
    border: 2px solid rgba(255, 140, 66, 0.7);
    box-shadow: 0 0 14px 2px rgba(255, 120, 50, 0.35), 0 4px 10px rgba(0, 0, 0, 0.5);
}

.pvp-arena-image {
    position: absolute;
    inset: 0;
    z-index: -1;
    background-size: cover;
    background-position: center 60%;
}

.pvp-arena-image::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to right, rgba(15, 23, 32, 0.9) 0%, rgba(15, 23, 32, 0.55) 70%, rgba(15, 23, 32, 0.3) 100%);
}

.pvp-arena-body {
    display: flex;
    flex-direction: column;
    row-gap: 4px;
    padding: 10px 14px;
}

.pvp-arena-name {
    color: #ffb27a;
    font-size: 16px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.9);
}

.pvp-arena-rule {
    color: #f1f5f9;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.9);
}

.pvp-squad {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
}

.pvp-squad-heading {
    display: flex;
    align-items: center;
    column-gap: 14px;
}

.pvp-squad-heading > .gem-button {
    flex-shrink: 0;
    margin-left: auto;
}

.pvp-team {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.pvp-list {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    margin: 0;
    padding: 0;
    list-style: none;
}

/* Строка соперника: имя и мощь, его отряд, прогноз и ставка, кнопка боя */
.pvp-opponent,
.pvp-battle {
    display: flex;
    align-items: center;
    column-gap: 14px;
    min-height: 60px;
    padding: 8px 12px;
    box-sizing: border-box;
    border-radius: 10px;
    border: 2px solid rgba(157, 173, 175, 0.5);
    background: rgba(15, 23, 32, 0.75);
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.45);
    transition: transform 0.15s ease, border-color 0.15s ease;
}

.pvp-opponent:hover {
    transform: translateY(-2px);
    border-color: rgba(255, 140, 66, 0.8);
}

.pvp-opponent-info {
    width: 150px;
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    row-gap: 2px;
}

.pvp-opponent-name {
    overflow: hidden;
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    text-overflow: ellipsis;
    white-space: nowrap;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.pvp-diff {
    align-self: flex-start;
    padding: 0 6px;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.5);
    font-size: 11px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.pvp-diff-stronger { color: #f87171; }
.pvp-diff-weaker { color: #4ade80; }
.pvp-diff-even { color: #e2e8f0; }

.pvp-team-enemy {
    flex: 1;
    min-width: 0;
}

.pvp-opponent-forecast {
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 4px;
    font-size: 12px;
    font-weight: bold;
}

.pvp-verdict {
    padding: 1px 8px;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.45);
    border: 1px solid currentColor;
    white-space: nowrap;
}

.forecast-easy { color: #4ade80; }
.forecast-likely { color: #bef264; }
.forecast-risky { color: #fbbf24; }
.forecast-deadly { color: #f87171; }

.pvp-stakes {
    color: #cbd5e1;
    font-variant-numeric: tabular-nums;
}

.pvp-win { color: #4ade80; }
.pvp-loss { color: #f87171; }

/* ===== Сезоны ===== */
.pvp-seasons {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(230px, 1fr));
    gap: 14px;
}

.pvp-season {
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 12px;
    border-radius: 12px;
    border: 2px solid rgba(255, 209, 102, 0.55);
    background: rgba(15, 23, 32, 0.8);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.pvp-season-heading {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
}

.pvp-season-left {
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.pvp-season-me {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.pvp-season-points {
    color: #ffd166;
    font-size: 32px;
    font-weight: 900;
    text-shadow: 0 2px 6px #000;
}

.pvp-season-label {
    color: #e2e8f0;
    font-size: 11px;
    text-transform: uppercase;
}

.pvp-season-stats {
    margin-top: 4px;
    color: #cbd5e1;
    font-size: 12px;
}

.pvp-top {
    margin: 0;
    padding: 0;
    list-style: none;
    display: flex;
    flex-direction: column;
    row-gap: 2px;
    font-size: 13px;
}

.pvp-top li {
    display: flex;
    column-gap: 8px;
    padding: 2px 6px;
    border-radius: 6px;
    color: #cbd5e1;
}

.pvp-top li.pvp-top-me {
    background: rgba(255, 209, 102, 0.18);
    color: #fff;
    font-weight: bold;
}

.pvp-top-rank {
    width: 20px;
    color: #ffd166;
    text-align: right;
}

.pvp-top-name {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.pvp-top-points {
    color: #ffd166;
    font-variant-numeric: tabular-nums;
}

/* ===== История ===== */
.pvp-battle-won { border-color: rgba(74, 222, 128, 0.5); }
.pvp-battle-lost { border-color: rgba(248, 113, 113, 0.5); }

.pvp-battle-icon {
    font-size: 22px;
}

.pvp-battle-main {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 2px;
}

.pvp-battle-result {
    color: #f8fafc;
    font-size: 13px;
    font-weight: bold;
    text-transform: uppercase;
}

.pvp-battle-won .pvp-battle-result { color: #4ade80; }
.pvp-battle-lost .pvp-battle-result { color: #f87171; }

.pvp-battle-points {
    width: 44px;
    text-align: right;
    font-size: 15px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.pvp-footer {
    flex: 1;
    display: flex;
    align-items: center;
    column-gap: 12px;
    min-height: 24px;
}

.pvp-footer-status {
    flex: 1;
    text-align: left;
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

@media (prefers-reduced-motion: reduce) {
    .pvp-opponent:hover {
        transform: none;
    }
}
</style>
