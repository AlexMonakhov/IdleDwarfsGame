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
import { HeroInfo, isBusy } from '@/models/types/Heroes';
import { estimateReward, ExpeditionBoard, ExpeditionInfo, ExpeditionStatus } from '@/models/types/Expeditions';
import caveBackground from '@/assets/red_crystal_arena.png';
import crystalsBackground from '@/assets/arenas/arena_blue.png';
import StatIcon from '@/components/icons/stats/StatIcon.vue';
import { IconCoin, IconExperience, IconHourglass } from '@/components/icons/ui';

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

// Фон задания и цвет его карточки (рамка, блик, ореол) — как у карточек боссов гильдии
const BACKGROUNDS = [
    { image: caveBackground, color: 'red' },
    { image: crystalsBackground, color: 'blue' },
];
const sceneOf = (expedition: ExpeditionInfo) => BACKGROUNDS[(expedition.slot - 1) % BACKGROUNDS.length];
const backgroundOf = (expedition: ExpeditionInfo) => sceneOf(expedition).image;

// Свечение карточки: доступное — горит цветом фона, в пути — приглушено, выполнено — золотом, награда забрана — гаснет
const CARD_STATE: Record<ExpeditionStatus, string> = {
    Available: 'exp-card-ready',
    InProgress: 'exp-card-waiting',
    Ready: 'exp-card-claimable',
    Claimed: 'exp-card-done',
};

// Сколько задания пройдено, 0…100
const progressOf = (expedition: ExpeditionInfo) => {
    if (statusOf(expedition) !== 'InProgress') return 100;
    const start = expedition.startedAt ? new Date(expedition.startedAt).getTime() : 0;
    const end = expedition.endsAt ? new Date(expedition.endsAt).getTime() : 0;
    if (end <= start) return 100;
    return Math.min(100, Math.max(0, ((serverNow.value - start) / (end - start)) * 100));
};

// У доступного задания награда ещё не зафиксирована — показываем, сколько дадут даже без мощи
const cardReward = (expedition: ExpeditionInfo) =>
    (expedition.status === 'Available' ? estimateReward(expedition.rates, 0) : expedition.reward);

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
    .sort((a, b) => Number(isBusy(a)) - Number(isBusy(b)) || b.power.total - a.power.total || a.name.localeCompare(b.name)));

const partyHeroes = computed(() => party.value.map(id => (id ? heroById.value.get(id) ?? null : null)));
const partyIds = computed(() => party.value.filter((id): id is string => id !== null));
const partyPower = computed(() => partyHeroes.value.reduce((sum, hero) => sum + (hero?.power.total ?? 0), 0));
const estimate = computed(() => (selected.value ? estimateReward(selected.value.rates, partyPower.value) : null));

const isInParty = (heroId: string) => party.value.includes(heroId);

const toggleHero = (hero: HeroInfo) => {
    if (isBusy(hero) || busy.value) return;
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

// Забрать можно и из открытого задания, и прямо с карточки в списке
const claim = async (expedition: ExpeditionInfo | null = selected.value) => {
    if (!expedition || busy.value) return;
    busy.value = true;
    errorMessage.value = null;
    try {
        const result = await ExpeditionsApiService.claim(expedition.id);
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
        width="min(1280px, calc(94 * var(--vw)))"
        height="min(780px, calc(90 * var(--vh)))"
    >
        <div class="expeditions">
            <div v-if="loading && !board" class="expeditions-hint">{{ t('common.loading') }}</div>

            <!-- ===== Список заданий ===== -->
            <div v-else-if="!selected" class="expedition-list">
                <!-- Карточка — как у боссов гильдии, только без портрета. Клик по карточке открывает задание -->
                <article
                    v-for="expedition in expeditions"
                    :key="expedition.id"
                    class="exp-card"
                    :class="[`exp-card-${sceneOf(expedition).color}`, CARD_STATE[statusOf(expedition)]]"
                    :role="expedition.status === 'Claimed' ? undefined : 'button'"
                    :tabindex="expedition.status === 'Claimed' ? undefined : 0"
                    @click="openExpedition(expedition)"
                    @keydown.enter="openExpedition(expedition)"
                >
                    <!-- Фон — место, куда идут герои -->
                    <div class="exp-card-arena" aria-hidden="true">
                        <div class="exp-card-arena-image" :style="{ backgroundImage: `url(${backgroundOf(expedition)})` }"></div>
                    </div>

                    <div class="exp-card-top">
                        <span class="exp-tier"><IconHourglass /> {{ t('expeditions.duration', { hours: expedition.durationHours }) }}</span>
                        <span class="exp-name">{{ templateText(expedition.templateKey, 'name') }}</span>
                        <span class="exp-status">{{ t(`expeditions.status.${statusOf(expedition)}`) }}</span>
                    </div>

                    <div class="exp-description">{{ templateText(expedition.templateKey, 'description') }}</div>

                    <!-- Ход задания — как полоска HP босса -->
                    <div v-if="statusOf(expedition) === 'InProgress' || statusOf(expedition) === 'Ready'" class="exp-progress">
                        <div class="exp-progress-fill" :style="{ width: `${progressOf(expedition)}%` }">
                            <span class="exp-progress-shine"></span>
                        </div>
                        <span class="exp-progress-text">
                            <template v-if="statusOf(expedition) === 'InProgress'">{{ t('expeditions.timeLeft', { time: formatTime(msUntil(expedition.endsAt)) }) }}</template>
                            <template v-else>{{ t('expeditions.status.Ready') }}</template>
                        </span>
                    </div>

                    <div class="exp-reward">
                        <span class="exp-label">{{ expedition.status === 'Available' ? t('expeditions.rewardFrom') : t('expeditions.rewardLabel') }}</span>
                        <span><IconCoin /> +{{ cardReward(expedition).gold }} · <IconExperience /> +{{ cardReward(expedition).experience }}</span>
                        <span v-if="expedition.power" class="exp-power"><StatIcon stat="power" /> {{ t('expeditions.power', { power: expedition.power }) }}</span>
                    </div>

                    <div class="exp-actions">
                        <GemButton v-if="statusOf(expedition) === 'Available'" :color="'green'" :disabled="busy">{{ t('expeditions.pickParty') }}</GemButton>
                        <GemButton v-else-if="statusOf(expedition) === 'InProgress'" :color="'blue'" :disabled="busy">{{ t('expeditions.watch') }}</GemButton>
                        <GemButton
                            v-else-if="statusOf(expedition) === 'Ready'"
                            :color="'yellow'"
                            :disabled="busy"
                            @click.stop="claim(expedition)"
                        >{{ busy ? t('expeditions.claiming') : t('expeditions.claim') }}</GemButton>
                        <span v-else class="exp-done">{{ t('expeditions.status.Claimed') }}</span>
                    </div>
                </article>
            </div>

            <!-- ===== Подбор группы ===== -->
            <div v-else-if="selectedStatus === 'Available'" class="expedition-details">
                <div class="expedition-heading">
                    <h3 class="expedition-title">{{ templateText(selected.templateKey, 'name') }}</h3>
                    <span class="expedition-duration"><IconHourglass /> {{ t('expeditions.duration', { hours: selected.durationHours }) }}</span>
                </div>
                <div class="expeditions-hint">{{ templateText(selected.templateKey, 'description') }}</div>

                <section class="expedition-section">
                    <div class="expedition-heading">
                        <h4 class="expedition-subtitle">{{ t('expeditions.party') }}</h4>
                        <span class="expedition-power">
                            <StatIcon stat="power" /> {{ t('expeditions.power', { power: partyPower }) }}
                            <template v-if="estimate"> · {{ t('expeditions.reward', { ...estimate }) }}</template>
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
                            :disabled="isBusy(hero)"
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
                        <StatIcon stat="power" /> {{ t('expeditions.power', { power: selected.power }) }}
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
                    @click="claim()"
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
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: 22px;
    /* Место под ореол карточек: тело модалки обрезает всё, что вылезает за край */
    padding: 14px;
}

/* ===== Карточка задания — по образцу карточки босса гильдии (GuildBosses.vue), без портрета.
   Свечение в два слоя: ::before — блик, бегущий по рамке, ::after — пульсирующий ореол вокруг карточки ===== */
.exp-card {
    --arena-color: #9ca3af;
    --spin-duration: 4s;
    --pulse-duration: 2.4s;
    --glow-strength: 1;
    position: relative;
    isolation: isolate;
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 12px;
    border-radius: 12px;
    border: 2px solid color-mix(in srgb, var(--arena-color) 70%, transparent);
    background: rgb(15, 23, 32);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
    cursor: pointer;
    outline: none;
    transition: transform 0.15s ease, box-shadow 0.15s ease, border-color 0.15s ease;
}

.exp-card:hover,
.exp-card:focus-visible {
    --glow-strength: 1.6;
    --spin-duration: 1.6s;
    transform: translateY(-4px);
    border-color: var(--arena-color);
    box-shadow: 0 8px 18px rgba(0, 0, 0, 0.65);
}

.exp-card-red { --arena-color: var(--red-gem-color); }
.exp-card-blue { --arena-color: var(--blue-gem-color); }

/* Фон под содержимым (внутри изолированной карточки -1 — уровень ореола ::after, поэтому -2) */
.exp-card-arena {
    position: absolute;
    inset: 0;
    z-index: -2;
    overflow: hidden;
    border-radius: 10px;
}

.exp-card-arena-image {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center 60%;
    transition: transform 0.4s ease, filter 0.3s ease;
}

/* Сверху светлее — видно место задания, книзу темнее — под текстом; плюс подсветка цветом карточки */
.exp-card-arena::after {
    content: '';
    position: absolute;
    inset: 0;
    background:
        radial-gradient(ellipse 80% 45% at 50% 0%, color-mix(in srgb, var(--arena-color) 30%, transparent), transparent),
        linear-gradient(to bottom, rgba(15, 23, 32, 0.35) 0%, rgba(15, 23, 32, 0.75) 45%, rgba(15, 23, 32, 0.92) 100%);
}

.exp-card:hover .exp-card-arena-image {
    transform: scale(1.08);
}

/* Угол блика анимируется как число — для этого его нужно объявить */
@property --exp-angle {
    syntax: '<angle>';
    initial-value: 0deg;
    inherits: false;
}

.exp-card::before {
    content: '';
    position: absolute;
    inset: -2px;
    z-index: 1;
    padding: 2px;
    border-radius: inherit;
    pointer-events: none;
    background: conic-gradient(
        from var(--exp-angle),
        transparent 0deg,
        transparent 250deg,
        var(--arena-color) 300deg,
        #fff 330deg,
        var(--arena-color) 345deg,
        transparent 360deg
    );
    /* Остаётся только кольцо толщиной в рамку */
    -webkit-mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
    -webkit-mask-composite: xor;
    mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
    mask-composite: exclude;
    opacity: var(--glow-strength);
    animation: exp-border-spin var(--spin-duration) linear infinite;
}

.exp-card::after {
    content: '';
    position: absolute;
    inset: 0;
    z-index: -1;
    border-radius: inherit;
    pointer-events: none;
    box-shadow:
        0 0 14px 2px var(--arena-color),
        0 0 28px 6px color-mix(in srgb, var(--arena-color) 45%, transparent);
    opacity: calc(0.55 * var(--glow-strength));
    animation: exp-glow-pulse var(--pulse-duration) ease-in-out infinite;
}

/* Можно отправлять — горит ярче и быстрее */
.exp-card-ready {
    --spin-duration: 3s;
    --pulse-duration: 1.8s;
}

/* Герои в пути — свечение приглушено и медленнее */
.exp-card-waiting {
    --spin-duration: 8s;
    --pulse-duration: 4s;
    --glow-strength: 0.45;
}

/* Выполнено, награда ждёт — карточка светится золотом */
.exp-card-claimable {
    --arena-color: #ffd166;
    --spin-duration: 2.2s;
    --pulse-duration: 1.4s;
}

/* Награда забрана — карточка гаснет, серая и не кликается */
.exp-card-done {
    border-color: rgba(157, 173, 175, 0.6);
    cursor: default;
}

.exp-card-done:hover {
    transform: none;
    border-color: rgba(157, 173, 175, 0.6);
}

.exp-card-done::before,
.exp-card-done::after {
    display: none;
}

.exp-card-done .exp-card-arena-image {
    filter: grayscale(0.85) brightness(0.7);
}

.exp-card-done:hover .exp-card-arena-image {
    transform: none;
}

@keyframes exp-border-spin {
    to { --exp-angle: 360deg; }
}

@keyframes exp-glow-pulse {
    0%, 100% { opacity: calc(0.35 * var(--glow-strength)); }
    50% { opacity: calc(0.85 * var(--glow-strength)); }
}

.exp-card-top {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 4px;
}

/* Длительность — плашкой цвета карточки, как ранг босса */
.exp-tier {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    padding: 2px 10px;
    border-radius: 8px;
    background: var(--arena-color);
    color: #fff;
    font-size: 12px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.exp-card-claimable .exp-tier {
    color: #3b2a00;
    text-shadow: none;
}

.exp-name {
    margin-top: 6px;
    color: #f8fafc;
    font-size: 16px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-align: center;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.exp-status {
    color: #cbd5e1;
    font-size: 12px;
    font-weight: bold;
    text-transform: uppercase;
}

.exp-card-claimable .exp-status {
    color: #ffd166;
}

.exp-description {
    flex: 1;
    color: #e2e8f0;
    font-size: 13px;
    line-height: 1.35;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

/* Полоска хода задания — как HP босса, только голубая */
.exp-progress {
    position: relative;
    height: 20px;
    overflow: hidden;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.6);
    box-shadow: inset 0 0 4px #000, 0 0 0 2px rgba(157, 173, 175, 0.6);
}

.exp-progress-fill {
    position: relative;
    overflow: hidden;
    height: 100%;
    background: linear-gradient(to bottom, #7dd3fc, #0369a1);
    box-shadow: 0 0 8px 1px rgba(125, 211, 252, 0.7);
    transition: width 0.9s linear;
}

.exp-card-claimable .exp-progress-fill {
    background: linear-gradient(to bottom, #ffe08a, #d89020);
    box-shadow: 0 0 8px 1px rgba(255, 209, 102, 0.7);
}

.exp-progress-shine {
    position: absolute;
    inset: 0;
    background: linear-gradient(110deg, transparent 30%, rgba(255, 255, 255, 0.45) 50%, transparent 70%);
    transform: translateX(-100%);
    animation: exp-progress-shine 2.6s ease-in-out infinite;
}

@keyframes exp-progress-shine {
    0% { transform: translateX(-100%); }
    60%, 100% { transform: translateX(100%); }
}

.exp-progress-text {
    position: absolute;
    inset: 0;
    color: #fff;
    font-size: 12px;
    font-weight: bold;
    line-height: 20px;
    text-align: center;
    font-variant-numeric: tabular-nums;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.9);
}

.exp-reward {
    display: flex;
    flex-direction: column;
    row-gap: 2px;
    color: #ffd166;
    font-size: 13px;
    font-weight: bold;
}

.exp-label {
    color: #e2e8f0;
    font-size: 12px;
    text-transform: uppercase;
}

.exp-power {
    color: #cbd5e1;
    font-size: 12px;
}

.exp-actions {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    row-gap: 6px;
    min-height: 46px;
}

.exp-done {
    color: var(--green-gem-color);
    font-size: 13px;
    font-weight: bold;
    text-transform: uppercase;
}

@media (prefers-reduced-motion: reduce) {
    .exp-card::before,
    .exp-card::after,
    .exp-progress-shine {
        animation: none !important;
    }

    .exp-card:hover,
    .exp-card:hover .exp-card-arena-image {
        transform: none;
    }
}

.expedition-title {
    margin: 0;
    color: #f8fafc;
    font-size: 16px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.expedition-duration {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
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
