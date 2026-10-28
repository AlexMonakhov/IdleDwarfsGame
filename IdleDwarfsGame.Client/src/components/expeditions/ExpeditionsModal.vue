<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import ExpeditionScene from '@/components/expeditions/ExpeditionScene.vue';
import ExpeditionsApiService from '@/services/apiServices/ExpeditionsApiService';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { useResources } from '@/composables/UseResources';
import { HeroInfo } from '@/models/types/Heroes';
import { estimateReward, ExpeditionBoard, ExpeditionInfo, ExpeditionStatus } from '@/models/types/Expeditions';
import caveBackground from '@/assets/red_crystal_arena.png';
import crystalsBackground from '@/assets/arena_blue_crystals.png';

const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t, te } = useI18n();
const { set: setResources } = useResources();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const board = ref<ExpeditionBoard | null>(null);
const heroes = ref<HeroInfo[]>([]);
const loading = ref(false);
const busy = ref(false);
const errorMessage = ref<string | null>(null);
const notice = ref<string | null>(null);

// Открытое задание; null — список заданий
const selectedId = ref<string | null>(null);
// Герои в ячейках выбранного задания, null — пустая ячейка
const party = ref<(string | null)[]>([]);

/* ===== Время ===== */
// Таймеры считаем по часам сервера: offset — насколько они расходятся с часами браузера
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;
const serverNow = computed(() => now.value + clockOffset);

const msUntil = (iso: string | null) => (iso ? new Date(iso).getTime() - serverNow.value : 0);

const formatTime = (ms: number) => {
    const total = Math.max(0, Math.ceil(ms / 1000));
    const h = Math.floor(total / 3600);
    const m = Math.floor((total % 3600) / 60);
    const s = total % 60;
    const pad = (n: number) => String(n).padStart(2, '0');
    return h > 0 ? `${h}:${pad(m)}:${pad(s)}` : `${m}:${pad(s)}`;
};

// Задание могло закончиться, пока окно открыто — не ждём перезагрузки доски
const statusOf = (expedition: ExpeditionInfo): ExpeditionStatus =>
    expedition.status === 'InProgress' && msUntil(expedition.endsAt) <= 0 ? 'Ready' : expedition.status;

/* ===== Загрузка ===== */
const errorText = (error: unknown, fallback: string) =>
    (error as AxiosError<{ message?: string }>).response?.data?.message ?? fallback;

const load = async () => {
    loading.value = true;
    errorMessage.value = null;
    try {
        const [nextBoard, nextHeroes] = await Promise.all([ExpeditionsApiService.getBoard(), HeroesApiService.getMyHeroes()]);
        clockOffset = new Date(nextBoard.serverTime).getTime() - Date.now();
        now.value = Date.now();
        board.value = nextBoard;
        heroes.value = nextHeroes;
        // Задание пропало с доски (сменилась доска) — возвращаемся к списку
        if (selectedId.value && !nextBoard.expeditions.some(e => e.id === selectedId.value)) {
            selectedId.value = null;
        }
    } catch {
        errorMessage.value = t('expeditions.loadError');
    } finally {
        loading.value = false;
    }
};

watch(open, isOpen => {
    window.clearInterval(ticker);
    if (!isOpen) return;

    selectedId.value = null;
    notice.value = null;
    load();
    ticker = window.setInterval(() => {
        now.value = Date.now();
        // Доска сменилась — подтягиваем новые задания
        if (board.value && !loading.value && msUntil(board.value.refreshAt) <= 0) load();
    }, 1000);
}, { immediate: true });

onBeforeUnmount(() => window.clearInterval(ticker));

/* ===== Список ===== */
const expeditions = computed(() => board.value?.expeditions ?? []);
const maxHeroes = computed(() => board.value?.maxHeroes ?? 4);

const templateText = (key: string, field: 'name' | 'description') =>
    te(`expeditions.templates.${key}.${field}`) ? t(`expeditions.templates.${key}.${field}`) : key;

const BACKGROUNDS = [caveBackground, crystalsBackground];
const backgroundOf = (expedition: ExpeditionInfo) => BACKGROUNDS[(expedition.slot - 1) % BACKGROUNDS.length];

const openExpedition = (expedition: ExpeditionInfo) => {
    if (expedition.status === 'Claimed') return;
    selectedId.value = expedition.id;
    party.value = Array(maxHeroes.value).fill(null);
    errorMessage.value = null;
    notice.value = null;
};

const back = () => {
    selectedId.value = null;
    errorMessage.value = null;
};

/* ===== Выбранное задание ===== */
const selected = computed(() => expeditions.value.find(e => e.id === selectedId.value) ?? null);
const selectedStatus = computed(() => (selected.value ? statusOf(selected.value) : null));

const heroById = computed(() => new Map(heroes.value.map(h => [h.id, h])));

// В задание идут только герои из инвентаря: отряд остаётся дома. Ушедшие в экспедицию видны, но недоступны
const reserve = computed(() => heroes.value
    .filter(h => h.placeInSquad === null)
    .sort((a, b) => Number(a.onExpedition) - Number(b.onExpedition) || b.power.total - a.power.total || a.name.localeCompare(b.name)));

const partyHeroes = computed(() => party.value.map(id => (id ? heroById.value.get(id) ?? null : null)));
const partyIds = computed(() => party.value.filter((id): id is string => id !== null));
const partyPower = computed(() => partyHeroes.value.reduce((sum, hero) => sum + (hero?.power.total ?? 0), 0));
const estimate = computed(() => (selected.value ? estimateReward(selected.value.rates, partyPower.value) : null));

const isInParty = (heroId: string) => party.value.includes(heroId);

const toggleHero = (hero: HeroInfo) => {
    if (hero.onExpedition || busy.value) return;
    const next = [...party.value];
    const index = next.indexOf(hero.id);
    if (index >= 0) {
        next[index] = null;
    } else {
        const free = next.indexOf(null);
        if (free < 0) return;
        next[free] = hero.id;
    }
    party.value = next;
};

const clearSlot = (index: number) => {
    if (busy.value) return;
    const next = [...party.value];
    next[index] = null;
    party.value = next;
};

// Герои на запущенном задании — для сцены
const sceneHeroes = computed(() => (selected.value?.heroIds ?? [])
    .map(id => heroById.value.get(id))
    .filter((hero): hero is HeroInfo => !!hero));

/* ===== Действия ===== */
const start = async () => {
    if (!selected.value || !partyIds.value.length || busy.value) return;
    busy.value = true;
    errorMessage.value = null;
    try {
        await ExpeditionsApiService.start(selected.value.id, partyIds.value);
    } catch (error) {
        errorMessage.value = errorText(error, t('expeditions.startError'));
    } finally {
        busy.value = false;
    }
    // И после успеха, и после ошибки (задание уже взято, доска сменилась) показываем актуальное состояние
    await load();
};

const claim = async () => {
    if (!selected.value || busy.value) return;
    busy.value = true;
    errorMessage.value = null;
    try {
        const result = await ExpeditionsApiService.claim(selected.value.id);
        setResources(result.resources);
        notice.value = t('expeditions.claimed', { gold: result.reward.gold, experience: result.reward.experience });
        selectedId.value = null;
    } catch (error) {
        errorMessage.value = errorText(error, t('expeditions.claimError'));
    } finally {
        busy.value = false;
    }
    await load();
};

const footerText = computed(() => {
    if (errorMessage.value) return errorMessage.value;
    if (notice.value) return notice.value;
    if (selectedStatus.value === 'Available' && !partyIds.value.length) return t('expeditions.needHero');
    if (!selected.value && board.value) return t('expeditions.refreshIn', { time: formatTime(msUntil(board.value.refreshAt)) });
    return '';
});
</script>

<template>
    <ModalComponent
        v-model="open"
        :header-message="t('expeditions.header')"
        width="min(1280px, 94vw)"
        height="min(780px, 90vh)"
    >
        <div class="expeditions">
            <div v-if="loading && !board" class="expeditions-hint">{{ t('common.loading') }}</div>

            <!-- ===== Список заданий ===== -->
            <div v-else-if="!selected" class="expedition-list">
                <button
                    v-for="expedition in expeditions"
                    :key="expedition.id"
                    type="button"
                    class="expedition-card"
                    :class="`expedition-card-${statusOf(expedition)}`"
                    :disabled="expedition.status === 'Claimed'"
                    @click="openExpedition(expedition)"
                >
                    <div class="expedition-card-image" :style="{ backgroundImage: `url(${backgroundOf(expedition)})` }">
                        <span class="expedition-duration">⏳ {{ t('expeditions.duration', { hours: expedition.durationHours }) }}</span>
                    </div>
                    <div class="expedition-card-body">
                        <div class="expedition-card-title">{{ templateText(expedition.templateKey, 'name') }}</div>
                        <div class="expedition-card-description">{{ templateText(expedition.templateKey, 'description') }}</div>
                        <div class="expedition-card-footer">
                            <span class="expedition-status">{{ t(`expeditions.status.${statusOf(expedition)}`) }}</span>
                            <span v-if="statusOf(expedition) === 'InProgress'" class="expedition-timer">
                                {{ t('expeditions.timeLeft', { time: formatTime(msUntil(expedition.endsAt)) }) }}
                            </span>
                            <span v-else-if="expedition.status !== 'Available'" class="expedition-reward">
                                {{ t('expeditions.reward', { gold: expedition.reward.gold, experience: expedition.reward.experience }) }}
                            </span>
                        </div>
                    </div>
                </button>
            </div>

            <!-- ===== Подбор группы ===== -->
            <div v-else-if="selectedStatus === 'Available'" class="expedition-details">
                <div class="expedition-heading">
                    <h3 class="expedition-title">{{ templateText(selected.templateKey, 'name') }}</h3>
                    <span class="expedition-duration">⏳ {{ t('expeditions.duration', { hours: selected.durationHours }) }}</span>
                </div>
                <div class="expeditions-hint">{{ templateText(selected.templateKey, 'description') }}</div>

                <section class="expedition-section">
                    <div class="expedition-heading">
                        <h4 class="expedition-subtitle">{{ t('expeditions.party') }}</h4>
                        <span class="expedition-power">
                            ⚔ {{ t('expeditions.power', { power: partyPower }) }}
                            <template v-if="estimate"> · {{ t('expeditions.reward', estimate) }}</template>
                        </span>
                    </div>
                    <div class="expeditions-hint">{{ t('expeditions.rewardHint') }}</div>
                    <div class="party-row">
                        <div
                            v-for="(hero, index) in partyHeroes"
                            :key="index"
                            class="party-slot"
                            :class="{ 'party-slot-empty': !hero }"
                        >
                            <span class="party-slot-number">{{ index + 1 }}</span>
                            <HeroCard v-if="hero" :hero="hero" selectable @click="clearSlot(index)" />
                            <span v-else class="party-slot-placeholder">{{ t('squad.emptySlot') }}</span>
                        </div>
                    </div>
                </section>

                <section class="expedition-section">
                    <h4 class="expedition-subtitle">{{ t('expeditions.heroes') }}</h4>
                    <div class="expeditions-hint">{{ t('expeditions.heroesHint') }}</div>
                    <div class="reserve-grid">
                        <HeroCard
                            v-for="hero in reserve"
                            :key="hero.id"
                            :hero="hero"
                            selectable
                            :disabled="hero.onExpedition"
                            :selected="isInParty(hero.id)"
                            :class="{ 'hero-picked': isInParty(hero.id) }"
                            @click="toggleHero(hero)"
                        />
                        <div v-if="!reserve.length" class="expeditions-hint">{{ t('expeditions.noHeroes') }}</div>
                    </div>
                </section>
            </div>

            <!-- ===== Задание идёт / выполнено ===== -->
            <div v-else class="expedition-details">
                <div class="expedition-heading">
                    <h3 class="expedition-title">{{ templateText(selected.templateKey, 'name') }}</h3>
                    <span class="expedition-power">
                        ⚔ {{ t('expeditions.power', { power: selected.power }) }}
                        · {{ t('expeditions.reward', { gold: selected.reward.gold, experience: selected.reward.experience }) }}
                    </span>
                </div>
                <div class="expeditions-hint">{{ templateText(selected.templateKey, 'description') }}</div>

                <ExpeditionScene
                    :key="selected.id"
                    :heroes="sceneHeroes"
                    :background="backgroundOf(selected)"
                    :finished="selectedStatus === 'Ready'"
                >
                    <div class="scene-overlay">
                        <span class="scene-status">{{ t(`expeditions.status.${selectedStatus}`) }}</span>
                        <span v-if="selectedStatus === 'InProgress'" class="scene-timer">{{ formatTime(msUntil(selected.endsAt)) }}</span>
                    </div>
                </ExpeditionScene>
            </div>
        </div>

        <template #modalFooter>
            <div class="expeditions-footer">
                <span class="expeditions-status" :class="{ 'expeditions-status-error': errorMessage }">{{ footerText }}</span>
                <GemButton v-if="selected" :color="'blue'" :disabled="busy" @click="back">{{ t('common.back') }}</GemButton>
                <GemButton
                    v-if="selectedStatus === 'Available'"
                    :color="'green'"
                    :disabled="!partyIds.length || busy"
                    @click="start"
                >{{ busy ? t('expeditions.starting') : t('expeditions.start') }}</GemButton>
                <GemButton
                    v-if="selectedStatus === 'InProgress' || selectedStatus === 'Ready'"
                    :color="'yellow'"
                    :disabled="selectedStatus !== 'Ready' || busy"
                    @click="claim"
                >{{ busy ? t('expeditions.claiming') : t('expeditions.claim') }}</GemButton>
            </div>
        </template>
    </ModalComponent>
</template>

<style scoped>
.expeditions {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    font-family: 'Arial', sans-serif;
}

.expeditions-hint {
    margin-bottom: 10px;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

/* ===== Список ===== */
.expedition-list {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
    gap: 16px;
    padding: 6px;
}

.expedition-card {
    display: flex;
    flex-direction: column;
    padding: 0;
    overflow: hidden;
    border-radius: 12px;
    border: 2px solid rgba(157, 173, 175, 0.7);
    background: rgba(15, 23, 32, 0.8);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
    color: inherit;
    font: inherit;
    text-align: left;
    cursor: pointer;
    transition: transform 0.12s ease, border-color 0.15s ease, box-shadow 0.15s ease;
}

.expedition-card:hover:not(:disabled) {
    transform: translateY(-3px);
    border-color: var(--yellow-gem-color);
    box-shadow: 0 6px 14px rgba(0, 0, 0, 0.6), 0 0 12px 2px var(--yellow-gem-color);
}

.expedition-card:disabled {
    cursor: default;
    filter: grayscale(0.7) brightness(0.75);
}

.expedition-card-InProgress {
    border-color: var(--blue-gem-color);
}

.expedition-card-Ready {
    border-color: var(--green-gem-color);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5), 0 0 14px 2px var(--green-gem-color);
}

.expedition-card-image {
    position: relative;
    height: 110px;
    background-size: cover;
    background-position: center 60%;
}

.expedition-card-image .expedition-duration {
    position: absolute;
    top: 8px;
    right: 8px;
}

.expedition-card-body {
    flex: 1;
    display: flex;
    flex-direction: column;
    row-gap: 6px;
    padding: 10px 12px 12px;
}

.expedition-card-title,
.expedition-title {
    margin: 0;
    color: #f8fafc;
    font-size: 16px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.expedition-card-description {
    flex: 1;
    color: #cbd5e1;
    font-size: 13px;
    line-height: 1.35;
}

.expedition-card-footer {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    column-gap: 8px;
    font-size: 12px;
    font-weight: bold;
}

.expedition-status {
    color: #e2e8f0;
    text-transform: uppercase;
}

.expedition-card-Ready .expedition-status {
    color: var(--green-gem-color);
}

.expedition-timer,
.expedition-reward {
    color: #ffd166;
}

.expedition-duration {
    padding: 2px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.65);
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
}

/* ===== Задание ===== */
.expedition-details {
    display: flex;
    flex-direction: column;
    padding: 6px;
}

.expedition-heading {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    column-gap: 12px;
    margin-bottom: 4px;
}

.expedition-subtitle {
    margin: 0 0 2px;
    color: #f8fafc;
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.expedition-power {
    color: #ffd166;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.expedition-section {
    margin-bottom: 14px;
}

.party-row {
    display: flex;
    justify-content: center;
    flex-wrap: wrap;
    gap: 12px;
}

.party-slot {
    position: relative;
    box-sizing: border-box;
    width: 148px;
    min-height: 196px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 12px;
    background: rgba(0, 0, 0, 0.35);
    border: 2px solid rgba(157, 173, 175, 0.5);
    box-shadow: inset 0 0 10px #000;
}

.party-slot-empty {
    border-style: dashed;
}

.party-slot-number {
    position: absolute;
    top: -10px;
    left: -10px;
    z-index: 1;
    width: 22px;
    height: 22px;
    border-radius: 50%;
    background: var(--wood-background);
    border: 1px solid rgb(157, 173, 175);
    color: #fff;
    font-size: 12px;
    font-weight: bold;
    line-height: 22px;
    text-align: center;
}

.party-slot-placeholder {
    color: rgba(203, 213, 225, 0.6);
    font-size: 13px;
    text-transform: uppercase;
}

.reserve-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 10px;
    padding: 4px;
}

/* Герой уже в ячейке: в списке он выбран (кайма — у самой карточки) и приглушён */
.hero-picked {
    opacity: 0.6;
}

/* ===== Сцена ===== */
.scene-overlay {
    position: absolute;
    top: 12px;
    left: 50%;
    z-index: 3;
    transform: translateX(-50%);
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 2px;
    padding: 6px 18px;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.55);
    box-shadow: 0 0 0 2px rgba(157, 173, 175, 0.6);
}

.scene-status {
    color: #e2e8f0;
    font-size: 12px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.scene-timer {
    color: #ffd166;
    font-size: 22px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
    text-shadow: 0 1px 3px #000;
}

/* ===== Футер ===== */
.expeditions-footer {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: flex-end;
    column-gap: 10px;
}

.expeditions-status {
    flex: 1;
    text-align: left;
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.expeditions-status-error {
    color: #ffb4b4;
}
</style>
