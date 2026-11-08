<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import { IconCheck, IconCoin, IconMinecart, IconNugget, IconPickaxe, IconTrophy } from '@/components/icons/ui';
import { PROFESSION_ICONS } from '@/components/icons/professions';
import EventsApiService from '@/services/apiServices/EventsApiService';
import { useHeroes } from '@/composables/UseHeroes';
import { useResources } from '@/composables/UseResources';
import { formatCountdown } from './eventShared';
import { isBusy, type HeroInfo } from '@/models/types/Heroes';
import type { WorldEventInfo } from '@/models/types/FixedShop';
import type { CommunityMinesOverview, DepositInfo, DepositKind } from '@/models/types/WorldEvents';

// Ивент «Общие шахты»: карта месторождений на весь сервер. Игрок ставит своих дворфов (до maxDwarvesPerPlayer
// на месторождение) и улучшает скорость за золото — для всех. Выработанное месторождение закрывается,
// каждый участник с долей не меньше minShare забирает самородки
const emit = defineEmits<{ (e: 'event', event: WorldEventInfo): void }>();

const { t, te, locale } = useI18n();
const { heroes, refresh: refreshHeroes } = useHeroes();
const { set: setResources } = useResources();

const overview = ref<CommunityMinesOverview | null>(null);
const loading = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);
const selectedId = ref<string | null>(null);
const picked = ref<string[]>([]);

const format = (value: number) => Math.round(value).toLocaleString(locale.value);
const percent = (value: number) => `${(value * 100).toLocaleString(locale.value, { maximumFractionDigits: 1 })}%`;

const errorText = (error: unknown, fallback: string) => {
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`communityMines.errors.${data.code}`)) return t(`communityMines.errors.${data.code}`);
    return data?.message ?? fallback;
};

/* ===== Время ===== */
const now = ref(Date.now());
let clockOffset = 0;
let loadedAt = Date.now();
let ticker = 0;
let poller = 0;

const until = (iso: string | null) => (iso ? new Date(iso).getTime() - (now.value + clockOffset) : 0);

// Руда досчитывается на глазах по скорости месторождения — до следующего ответа сервера
const liveOre = (deposit: DepositInfo) => {
    if (deposit.closedAt || deposit.exhaustedAt) return deposit.oreMined;
    const hours = Math.max(0, now.value - loadedAt) / 3_600_000;
    return Math.min(deposit.oreTotal, deposit.oreMined + deposit.rate * hours);
};

/* ===== Загрузка и действия ===== */
const apply = (next: CommunityMinesOverview) => {
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
    loadedAt = Date.now();
    now.value = loadedAt;
    overview.value = next;
    setResources(next.resources);
    emit('event', next.event);
    // Выбранное месторождение пропало или ещё не выбрано — берём первое открытое
    if (!next.deposits.some(d => d.id === selectedId.value))
        selectedId.value = next.deposits.find(d => !d.closedAt)?.id ?? next.deposits[0]?.id ?? null;
};

const load = async () => {
    loading.value = true;
    try {
        apply(await EventsApiService.getCommunityMines());
    } catch (error) {
        message.value = { text: errorText(error, t('communityMines.loadError')), error: true };
    } finally {
        loading.value = false;
    }
};

const run = async (action: () => Promise<void>, heroesChanged = false) => {
    if (busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        await action();
        // Дворфы стали заняты или свободны — список героев (и окна отряда, шахт, экспедиций) должен это знать
        if (heroesChanged) await refreshHeroes();
    } catch (error) {
        message.value = { text: errorText(error, t('communityMines.actionError')), error: true };
        await load();
    } finally {
        busy.value = false;
    }
};

const send = (deposit: DepositInfo) => run(async () => {
    apply(await EventsApiService.sendDwarves(deposit.id, picked.value));
    message.value = { text: t('communityMines.sent', { count: picked.value.length }), error: false };
    picked.value = [];
}, true);

const recall = (deposit: DepositInfo, heroId: string) => run(async () => {
    apply(await EventsApiService.recallDwarf(deposit.id, heroId));
}, true);

const upgrade = (deposit: DepositInfo) => run(async () => {
    apply(await EventsApiService.upgradeDeposit(deposit.id));
    message.value = { text: t('communityMines.upgraded'), error: false };
});

const claim = () => run(async () => {
    const result = await EventsApiService.claimCommunityMines();
    apply(result.overview);
    message.value = { text: t('communityMines.claimed', { amount: format(result.currency) }), error: false };
});

onMounted(() => {
    load();
    refreshHeroes();
    ticker = window.setInterval(() => { now.value = Date.now(); }, 1000);
    // Месторождения копает весь сервер — состав и доли меняются без нас
    poller = window.setInterval(() => { if (!busy.value) load(); }, 30_000);
});

onBeforeUnmount(() => {
    window.clearInterval(ticker);
    window.clearInterval(poller);
});

watch(selectedId, () => { picked.value = []; });

/* ===== Карта ===== */
// Места месторождений на карте (в процентах); номер месторождения выбирает место по кругу
const SPOTS: [number, number][] = [[18, 30], [44, 22], [72, 30], [26, 64], [56, 58], [82, 66], [40, 84], [12, 82]];
const spotOf = (deposit: DepositInfo) => SPOTS[deposit.index % SPOTS.length];

const KIND_COLORS: Record<DepositKind, string> = {
    Copper: '#f97316',
    Iron: '#94a3b8',
    Silver: '#e2e8f0',
    Gold: '#fbbf24',
    Mithril: '#67e8f9',
};

// На карте — открытые и закрытые за последние сутки: старые не загромождают её
const visible = computed(() => (overview.value?.deposits ?? [])
    .filter(d => !d.closedAt || until(d.closedAt) > -86_400_000)
    // Место на карте одно на номер по кругу: свежее месторождение рисуется поверх старого
    .sort((a, b) => a.index - b.index));

const selected = computed(() => overview.value?.deposits.find(d => d.id === selectedId.value) ?? null);

/* ===== Выбор дворфов ===== */
const dwarfRate = (hero: HeroInfo) => {
    const rate = 10 * (1 + 0.5 * (hero.level - 1)) * (1 + 0.02 * ((hero.upgrade?.level ?? 1) - 1));
    return hero.profession === 'Miner' ? rate * 1.5 : rate;
};

const freeDwarves = computed(() => {
    const deposit = selected.value;
    if (!deposit) return [];
    return (heroes.value ?? [])
        .filter(h => h.unitType === 'Dwarf' && h.placeInSquad == null && !isBusy(h))
        .filter(h => deposit.maxRank == null || h.level <= deposit.maxRank)
        .sort((a, b) => dwarfRate(b) - dwarfRate(a));
});

const slotsLeft = computed(() => (selected.value && overview.value
    ? overview.value.maxDwarvesPerPlayer - selected.value.myDwarves.length
    : 0));

const togglePick = (heroId: string) => {
    const index = picked.value.indexOf(heroId);
    if (index >= 0) picked.value.splice(index, 1);
    else if (picked.value.length < slotsLeft.value) picked.value.push(heroId);
};

const pendingTotal = computed(() => overview.value?.pending.reduce((sum, p) => sum + p.currency, 0) ?? 0);
const canAct = computed(() => !!overview.value?.open && !!selected.value && !selected.value.closedAt && !selected.value.exhaustedAt);
</script>

<template>
    <div class="community-mines">
        <div v-if="message" class="cm-message" :class="{ 'cm-message-error': message.error }">{{ message.text }}</div>
        <div v-if="loading && !overview" class="cm-hint">{{ t('common.loading') }}</div>

        <template v-else-if="overview">
            <p v-if="!overview.open && !overview.deposits.length" class="cm-hint">{{ t('communityMines.notThisWeek') }}</p>

            <div class="cm-layout">
                <!-- ===== Карта ===== -->
                <div class="cm-map-wrap">
                    <div class="cm-map">
                        <button
                            v-for="deposit in visible"
                            :key="deposit.id"
                            type="button"
                            class="cm-spot"
                            :class="{
                                'cm-spot-selected': deposit.id === selectedId,
                                'cm-spot-closed': deposit.closedAt,
                                'cm-spot-mine': deposit.myDwarves.length,
                            }"
                            :style="{
                                left: `${spotOf(deposit)[0]}%`,
                                top: `${spotOf(deposit)[1]}%`,
                                '--kind-color': KIND_COLORS[deposit.kind],
                                '--spot-size': `${40 + deposit.size * 5}px`,
                            }"
                            @click="selectedId = deposit.id"
                        >
                            <span class="cm-spot-ore"><IconCheck v-if="deposit.closedAt" /><IconPickaxe v-else /></span>
                            <span class="cm-spot-ring" :style="{ '--done': `${(liveOre(deposit) / deposit.oreTotal) * 100}%` }"></span>
                            <span class="cm-spot-label">{{ t(`communityMines.kinds.${deposit.kind}`) }}</span>
                            <span v-if="deposit.dwarves && !deposit.closedAt" class="cm-spot-dwarves">{{ deposit.dwarves }}</span>
                        </button>
                    </div>
                    <span v-if="overview.nextSpawnAt" class="cm-next">
                        {{ until(overview.nextSpawnAt) > 0
                            ? t('communityMines.nextSpawn', { time: formatCountdown(until(overview.nextSpawnAt), t('fixedShop.day')) })
                            : t('communityMines.mapFull', { max: overview.maxOpen }) }}
                    </span>
                </div>

                <!-- ===== Месторождение ===== -->
                <div class="cm-side">
                    <section v-if="selected" class="cm-box cm-deposit" :style="{ '--kind-color': KIND_COLORS[selected.kind] }">
                        <div class="cm-deposit-head">
                            <h4 class="cm-deposit-name">{{ t(`communityMines.kinds.${selected.kind}`) }}</h4>
                            <span class="cm-chip">{{ t('communityMines.size', { size: selected.size }) }}</span>
                            <span v-if="selected.maxRank" class="cm-chip" v-tooltip="t('communityMines.maxRankHint')">
                                {{ t('communityMines.maxRank', { rank: selected.maxRank }) }}
                            </span>
                        </div>

                        <div class="cm-progress">
                            <div class="cm-progress-fill" :style="{ width: `${(liveOre(selected) / selected.oreTotal) * 100}%` }"></div>
                            <span class="cm-progress-text">{{ format(liveOre(selected)) }} / {{ format(selected.oreTotal) }}</span>
                        </div>

                        <div class="cm-facts">
                            <span v-if="selected.closedAt">{{ selected.exhaustedAt ? t('communityMines.exhausted') : t('communityMines.closedByEnd') }}</span>
                            <template v-else>
                                <span>{{ t('communityMines.rate', { rate: format(selected.rate) }) }}</span>
                                <span v-if="selected.etaAt">{{ t('communityMines.eta', { time: formatCountdown(until(selected.etaAt), t('fixedShop.day')) }) }}</span>
                                <span v-else>{{ t('communityMines.idle') }}</span>
                            </template>
                            <span>{{ t('communityMines.crew', { dwarves: selected.dwarves, players: selected.participants }) }}</span>
                        </div>

                        <!-- Доля и награда игрока -->
                        <div class="cm-mine-share">
                            <span>{{ t('communityMines.myShare', { share: percent(selected.myShare), ore: format(selected.myContribution) }) }}</span>
                            <span v-tooltip="t('communityMines.rewardHint', { min: percent(overview.minShare) })">
                                <IconNugget /> {{ selected.myReward ? format(selected.myReward) : t('communityMines.noReward', { min: percent(overview.minShare) }) }}
                            </span>
                        </div>

                        <!-- Улучшение для всех -->
                        <div v-if="!selected.closedAt" class="cm-upgrade">
                            <span>
                                {{ t('communityMines.speed', { level: selected.speedLevel, max: selected.maxSpeedLevel, factor: percent(selected.speedFactor) }) }}
                            </span>
                            <span
                                class="cm-upgrades"
                                :class="{ 'cm-upgrades-full': selected.myUpgrades >= selected.maxUpgradesPerPlayer }"
                                v-tooltip="t('communityMines.upgradeLimitHint', { max: selected.maxUpgradesPerPlayer })"
                            >{{ t('communityMines.myUpgrades', { count: selected.myUpgrades, max: selected.maxUpgradesPerPlayer }) }}</span>
                            <GemButton
                                v-if="selected.upgradeGold != null"
                                :color="'blue'"
                                :disabled="!canAct || busy || selected.myUpgrades >= selected.maxUpgradesPerPlayer"
                                v-tooltip="t('communityMines.upgradeHint', { factor: percent(selected.nextSpeedFactor), ore: format(selected.upgradePoints) })"
                                @click="upgrade(selected)"
                            >{{ t('communityMines.upgrade') }} <IconCoin /> {{ format(selected.upgradeGold) }}</GemButton>
                        </div>

                        <!-- Мои дворфы -->
                        <div class="cm-dwarves">
                            <h5 class="cm-subtitle">{{ t('communityMines.myDwarves', { count: selected.myDwarves.length, max: overview.maxDwarvesPerPlayer }) }}</h5>
                            <ul v-if="selected.myDwarves.length" class="cm-dwarf-list">
                                <li v-for="dwarf in selected.myDwarves" :key="dwarf.heroId">
                                    <component :is="PROFESSION_ICONS[dwarf.profession]" v-if="dwarf.profession && PROFESSION_ICONS[dwarf.profession]" />
                                    <span class="cm-dwarf-name">{{ dwarf.name }} · {{ t('communityMines.rank', { rank: dwarf.rank }) }}</span>
                                    <span class="cm-dwarf-rate">{{ t('communityMines.dwarfRate', { rate: format(dwarf.rate) }) }}</span>
                                    <button v-if="!selected.closedAt" type="button" class="cm-recall" :disabled="busy" @click="recall(selected, dwarf.heroId)">
                                        {{ t('communityMines.recall') }}
                                    </button>
                                </li>
                            </ul>

                            <template v-if="canAct && slotsLeft > 0">
                                <h5 class="cm-subtitle">{{ t('communityMines.pick', { count: slotsLeft }) }}</h5>
                                <div v-if="freeDwarves.length" class="cm-pick-list">
                                    <button
                                        v-for="hero in freeDwarves"
                                        :key="hero.id"
                                        type="button"
                                        class="cm-pick"
                                        :class="{ 'cm-pick-on': picked.includes(hero.id) }"
                                        :disabled="!picked.includes(hero.id) && picked.length >= slotsLeft"
                                        @click="togglePick(hero.id)"
                                    >
                                        <span class="cm-dwarf-name">{{ hero.name }} · {{ t('communityMines.rank', { rank: hero.level }) }}</span>
                                        <span class="cm-dwarf-rate">{{ t('communityMines.dwarfRate', { rate: format(dwarfRate(hero)) }) }}</span>
                                    </button>
                                </div>
                                <span v-else class="cm-muted">{{ t('communityMines.noFreeDwarves') }}</span>
                                <GemButton :color="'green'" :disabled="!picked.length || busy" @click="send(selected)">
                                    <IconMinecart /> {{ t('communityMines.send', { count: picked.length }) }}
                                </GemButton>
                            </template>
                        </div>

                        <div v-if="selected.top.length" class="cm-top">
                            <h5 class="cm-subtitle">{{ t('communityMines.top') }}</h5>
                            <ol class="cm-list cm-ranking">
                                <li v-for="entry in selected.top" :key="entry.playerId">
                                    <span>{{ entry.name }}</span>
                                    <span class="cm-value">{{ percent(entry.share) }}</span>
                                </li>
                            </ol>
                        </div>
                    </section>
                    <section v-else class="cm-box"><span class="cm-muted">{{ t('communityMines.noDeposits') }}</span></section>

                    <section class="cm-box">
                        <h4 class="cm-title">{{ t('communityMines.pendingTitle') }}</h4>
                        <template v-if="overview.pending.length">
                            <ul class="cm-list">
                                <li v-for="reward in overview.pending" :key="reward.depositId">
                                    <span>{{ t(`communityMines.kinds.${reward.kind}`) }} · {{ percent(reward.share) }}</span>
                                    <span class="cm-value"><IconNugget /> {{ format(reward.currency) }}</span>
                                </li>
                            </ul>
                            <GemButton :color="'yellow'" :disabled="busy" @click="claim">{{ t('communityMines.claim', { amount: format(pendingTotal) }) }}</GemButton>
                        </template>
                        <span v-else class="cm-muted">{{ t('communityMines.noPending') }}</span>
                    </section>

                    <section class="cm-box">
                        <h4 class="cm-title"><IconTrophy /> {{ t('communityMines.eventTop') }}</h4>
                        <ol v-if="overview.leaderboard.length" class="cm-list cm-ranking">
                            <li v-for="entry in overview.leaderboard" :key="entry.playerId">
                                <span>{{ entry.name }}</span>
                                <span class="cm-value">{{ format(entry.contribution) }}</span>
                            </li>
                        </ol>
                        <span v-else class="cm-muted">{{ t('communityMines.noContribution') }}</span>
                        <span v-if="overview.myRank" class="cm-mine">{{ t('communityMines.myRank', { rank: overview.myRank, ore: format(overview.myContribution) }) }}</span>
                    </section>
                </div>
            </div>
        </template>
    </div>
</template>

<style scoped>
.community-mines {
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 0 6px 10px;
    font-family: 'Arial', sans-serif;
    color: #f8fafc;
}

.cm-message {
    color: #bbf7d0;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.cm-message-error {
    color: #ffb4b4;
}

.cm-hint,
.cm-muted {
    color: #cbd5e1;
    font-size: 13px;
}

.cm-layout {
    display: grid;
    grid-template-columns: minmax(300px, 1.3fr) minmax(280px, 1fr);
    gap: 14px;
    align-items: start;
}

/* ===== Карта: пещера с жилами ===== */
.cm-map-wrap {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
}

.cm-map {
    position: relative;
    aspect-ratio: 4 / 3;
    border-radius: 16px;
    overflow: hidden;
    background:
        radial-gradient(ellipse 30% 22% at 30% 40%, rgba(251, 191, 36, 0.10), transparent 70%),
        radial-gradient(ellipse 35% 25% at 70% 65%, rgba(103, 232, 249, 0.08), transparent 70%),
        radial-gradient(ellipse 90% 80% at 50% 50%, #3b2f24 0%, #241c15 60%, #120d09 100%);
    box-shadow: inset 0 0 40px rgba(0, 0, 0, 0.95), 0 0 0 2px rgba(217, 119, 6, 0.35);
}

/* Прожилки породы */
.cm-map::before {
    content: '';
    position: absolute;
    inset: 0;
    background:
        repeating-linear-gradient(115deg, transparent 0 38px, rgba(255, 255, 255, 0.025) 38px 40px),
        repeating-linear-gradient(35deg, transparent 0 56px, rgba(0, 0, 0, 0.18) 56px 60px);
    pointer-events: none;
}

.cm-spot {
    position: absolute;
    transform: translate(-50%, -50%);
    width: var(--spot-size);
    height: var(--spot-size);
    padding: 0;
    border: none;
    border-radius: 50%;
    background: radial-gradient(circle at 35% 35%, color-mix(in srgb, var(--kind-color) 70%, #fff), var(--kind-color) 45%, color-mix(in srgb, var(--kind-color) 40%, #000));
    box-shadow: 0 0 14px color-mix(in srgb, var(--kind-color) 60%, transparent), 0 3px 6px rgba(0, 0, 0, 0.8);
    cursor: pointer;
    transition: transform 0.15s ease;
}

.cm-spot:hover {
    transform: translate(-50%, -50%) scale(1.08);
}

.cm-spot-selected {
    box-shadow: 0 0 0 3px #fde68a, 0 0 20px color-mix(in srgb, var(--kind-color) 80%, transparent);
}

.cm-spot-closed {
    filter: grayscale(0.85) brightness(0.6);
}

.cm-spot-ore {
    position: absolute;
    inset: 0;
    display: grid;
    place-items: center;
    color: #1c1917;
    font-size: 20px;
    --split-icon-primary: #1c1917;
    --split-icon-secondary: #44403c;
}

/* Кольцо выработки вокруг жилы */
.cm-spot-ring {
    position: absolute;
    inset: -6px;
    border-radius: 50%;
    background: conic-gradient(#4ade80 var(--done), rgba(0, 0, 0, 0.45) 0);
    -webkit-mask: radial-gradient(circle, transparent 62%, #000 63%);
    mask: radial-gradient(circle, transparent 62%, #000 63%);
}

.cm-spot-label {
    position: absolute;
    top: calc(100% + 8px);
    left: 50%;
    transform: translateX(-50%);
    white-space: nowrap;
    font-size: 12px;
    font-weight: bold;
    text-shadow: 1px 1px 0 #000, 0 0 4px #000;
}

.cm-spot-dwarves {
    position: absolute;
    top: -8px;
    right: -8px;
    min-width: 20px;
    height: 20px;
    padding: 0 4px;
    border-radius: 10px;
    background: #1e293b;
    box-shadow: 0 0 0 2px #fde68a;
    font-size: 11px;
    font-weight: bold;
    line-height: 20px;
}

.cm-spot-mine .cm-spot-dwarves {
    background: #166534;
}

.cm-next {
    align-self: center;
    padding: 3px 10px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.55);
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

/* ===== Карточки справа ===== */
.cm-side {
    display: flex;
    flex-direction: column;
    row-gap: 12px;
}

.cm-box {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    padding: 10px 14px;
    border-radius: 12px;
    background: rgba(0, 0, 0, 0.35);
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6);
}

.cm-deposit {
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.6), 0 0 0 1px color-mix(in srgb, var(--kind-color) 55%, transparent);
}

.cm-deposit-head {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 6px 8px;
}

.cm-deposit-name {
    margin: 0 auto 0 0;
    color: var(--kind-color);
    font-size: 18px;
    text-shadow: 1px 1px 0 #000;
}

.cm-chip {
    padding: 2px 8px;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.1);
    font-size: 11px;
    font-weight: bold;
}

.cm-progress {
    position: relative;
    height: 18px;
    border-radius: 9px;
    background: rgba(0, 0, 0, 0.6);
    overflow: hidden;
}

.cm-progress-fill {
    height: 100%;
    background: linear-gradient(to right, color-mix(in srgb, var(--kind-color) 60%, #000), var(--kind-color));
}

.cm-progress-text {
    position: absolute;
    inset: 0;
    display: grid;
    place-items: center;
    font-size: 11px;
    font-weight: bold;
    text-shadow: 1px 1px 0 #000;
    font-variant-numeric: tabular-nums;
}

.cm-facts,
.cm-mine-share,
.cm-upgrade {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 4px 14px;
    font-size: 13px;
}

.cm-mine-share {
    font-weight: bold;
    color: #fde68a;
}

.cm-mine-share > span {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
}

.cm-upgrade {
    justify-content: space-between;
}

.cm-upgrades {
    color: #cbd5e1;
    font-size: 12px;
}

.cm-upgrades-full {
    color: #fca5a5;
}

.cm-upgrade :deep(.gem-text) {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    font-size: 13px;
}

.cm-subtitle {
    margin: 4px 0 0;
    color: #cbd5e1;
    font-size: 12px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.cm-dwarves {
    display: flex;
    flex-direction: column;
    row-gap: 6px;
}

.cm-dwarf-list,
.cm-list {
    display: flex;
    flex-direction: column;
    row-gap: 4px;
    margin: 0;
    padding: 0;
    list-style: none;
    font-size: 13px;
}

.cm-dwarf-list li,
.cm-list li {
    display: flex;
    align-items: center;
    column-gap: 8px;
}

.cm-list li {
    justify-content: space-between;
}

.cm-dwarf-name {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.cm-dwarf-rate {
    color: #a7f3d0;
    font-size: 12px;
    font-variant-numeric: tabular-nums;
}

.cm-recall {
    padding: 2px 8px;
    border: none;
    border-radius: 6px;
    background: rgba(248, 113, 113, 0.25);
    color: #fecaca;
    font-size: 11px;
    font-weight: bold;
    cursor: pointer;
}

.cm-recall:disabled {
    opacity: 0.5;
    cursor: default;
}

.cm-pick-list {
    display: flex;
    flex-direction: column;
    row-gap: 4px;
    max-height: 180px;
    overflow-y: auto;
}

.cm-pick {
    display: flex;
    align-items: center;
    column-gap: 8px;
    padding: 5px 8px;
    border: none;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.06);
    color: inherit;
    font: inherit;
    font-size: 13px;
    text-align: left;
    cursor: pointer;
}

.cm-pick:hover:not(:disabled) {
    background: rgba(255, 255, 255, 0.12);
}

.cm-pick-on {
    background: rgba(74, 222, 128, 0.25);
    box-shadow: 0 0 0 1px #4ade80;
}

.cm-pick:disabled {
    opacity: 0.45;
    cursor: default;
}

.cm-title {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    margin: 0;
    color: #fbbf24;
    font-size: 13px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.cm-ranking {
    counter-reset: rank;
}

.cm-ranking li::before {
    counter-increment: rank;
    content: counter(rank) '.';
    min-width: 22px;
    color: #94a3b8;
}

.cm-ranking li > span:first-child {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.cm-value {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.cm-mine {
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
}

@media (max-width: 860px) {
    .cm-layout {
        grid-template-columns: 1fr;
    }
}
</style>
