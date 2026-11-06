<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch, type Component } from 'vue';
import HeroFilters from '@/components/inventory/HeroFilters.vue';
import { useHeroFilters } from '@/composables/UseHeroFilters';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import GemButton from '@/components/common/GemButton.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import MineCostView from './MineCostView.vue';
import { mineColor, oreImage } from './miningArt';
import MiningApiService from '@/services/apiServices/MiningApiService';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { HeroInfo, isBusy } from '@/models/types/Heroes';
import {
    MineInfo,
    MineUpgrade,
    MineUpgradeInfo,
    MiningActionResult,
    MiningError,
    MiningWallet,
    canAfford,
    digEnergyNow,
    formatPerClick,
    storedNow,
} from '@/models/types/Mining';
import { vDragScroll } from '@/directives/dragScroll';
import MineResourceIcon from './MineResourceIcon.vue';
import { IconCrate, IconHourglass, IconLock, IconMinecart, IconPickaxe } from '@/components/icons/ui';

// Окно шахты: слева жила (клик — добыча) и дворф со складом, справа улучшения
const props = defineProps<{
    modelValue: boolean;
    mine: MineInfo;
    wallet: MiningWallet;
    dwarfUnlockLevel: number;
    // Сила кирки (MiningBoard): запас кликов на все шахты и сколько восстанавливается в час
    digEnergyMax: number;
    digEnergyPerHour: number;
    // Время сервера (мс) — по нему досчитываем склад и силу кирки
    serverNow: number;
}>();
const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'changed', result: MiningActionResult): void;
    (e: 'wallet', wallet: MiningWallet): void;
    // Сервер отказал — данные устарели, месту шахт стоит их перечитать
    (e: 'stale'): void;
}>();

const { t, te, locale } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const color = computed(() => mineColor(props.mine.type));
const ore = computed(() => oreImage(props.mine.type));
const name = computed(() => t(`mining.types.${props.mine.type}`));

const format = (value: number) =>
    value.toLocaleString(locale.value, { maximumFractionDigits: Number.isInteger(value) ? 0 : 1 });

const message = ref<{ text: string; error: boolean } | null>(null);
const busy = ref<string | null>(null);

const errorText = (error: unknown) => {
    const code = (error as AxiosError<{ code?: MiningError }>).response?.data?.code;
    return code && te(`mining.errors.${code}`) ? t(`mining.errors.${code}`) : t('mining.actionError');
};

// Запросы к шахте идут по одному: иначе пачка кликов и улучшение поспорили бы за версию шахты
let queue: Promise<unknown> = Promise.resolve();
function enqueue<T>(request: () => Promise<T>): Promise<T> {
    const next = queue.then(request);
    queue = next.catch(() => undefined);
    return next;
}

/* ===== Герои ===== */
const heroes = ref<HeroInfo[]>([]);

const loadHeroes = async () => {
    try {
        heroes.value = await HeroesApiService.getMyHeroes();
    } catch {
        // Без списка героев просто не будет выбора дворфа
    }
};

// Места шахты: открытые — с дворфом или пустые, закрытые — откроются на nextSlotLevel и дальше
interface DwarfPlace {
    key: string;
    heroId: string | null;
    hero: HeroInfo | null;
    open: boolean;
}

const places = computed<DwarfPlace[]>(() => Array.from({ length: props.mine.maxDwarves }, (_, index) => {
    const heroId = props.mine.heroIds[index] ?? null;
    return {
        key: heroId ?? `place-${index}`,
        heroId,
        hero: heroId ? heroes.value.find(h => h.id === heroId) ?? null : null,
        open: index < props.mine.dwarfSlots,
    };
}));

const hasFreePlace = computed(() => props.mine.heroIds.length < props.mine.dwarfSlots);

// В шахту идут свободные герои из инвентаря: не из отряда, не в экспедиции и не в другой шахте
// Фильтры по стихии и рангу — поверх свободных героев
const freeHeroes = computed(() => heroes.value.filter(h => h.placeInSquad === null && !isBusy(h)));
const { element: elementFilter, rank: rankFilter, filtered: filteredCandidates } = useHeroFilters(freeHeroes);

const candidates = computed(() => [...filteredCandidates.value]
    .sort((a, b) => b.level - a.level || a.name.localeCompare(b.name)));

/* ===== Добыча кликами ===== */
const MAX_BATCH = 50;
const FLUSH_MS = 1000;
const FLOAT_MS = 900;

// Клики, ещё не отправленные на сервер, и отправленные, но без ответа: пока ответа нет, сила кирки и доля жилы
// уже показываются с их учётом — иначе полоска прыгала бы назад между пачками
const pendingClicks = ref(0);
const sentClicks = ref(0);
let flushTimer = 0;

interface FloatNumber { id: number; amount: number; x: number; y: number }
const floats = ref<FloatNumber[]>([]);
let floatId = 0;
// Номер удара: новый ключ перезапускает анимацию жилы
const hit = ref(0);

// Сила кирки сейчас, с учётом ещё не подтверждённых кликов
const energy = computed(() => Math.max(0,
    digEnergyNow(props.wallet, props.digEnergyMax, props.digEnergyPerHour, props.serverNow) - pendingClicks.value - sentClicks.value));
const energyLeft = computed(() => Math.floor(energy.value));
const tired = computed(() => energyLeft.value < 1);
// Через сколько минут кирка снова сможет ударить
const tiredMinutes = computed(() => Math.max(1, Math.ceil(((1 - energy.value) / props.digEnergyPerHour) * 60)));

// Доля следующей штуки, набранная кликами (у кристаллов клик даёт меньше единицы)
const veinProgress = computed(() => {
    const total = props.mine.digProgress + (pendingClicks.value + sentClicks.value) * props.mine.perClick;
    return total - Math.floor(total);
});
const fractionalClick = computed(() => props.mine.perClick < 1);

const formatClick = (amount: number) => formatPerClick(amount, locale.value);

const dig = (event: MouseEvent) => {
    // Кирка устала — клик не засчитается, и анимации удара нет
    if (tired.value) {
        message.value = { text: t('mining.dig.tired', { minutes: tiredMinutes.value }), error: true };
        return;
    }
    pendingClicks.value++;
    hit.value++;

    const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    const float = { id: ++floatId, amount: props.mine.perClick, x: event.clientX - rect.left, y: event.clientY - rect.top };
    floats.value = [...floats.value, float];
    window.setTimeout(() => { floats.value = floats.value.filter(f => f.id !== float.id); }, FLOAT_MS);
};

// Клики уходят на сервер пачкой раз в секунду; лишние сверх его лимита он просто не засчитает
const flush = () => {
    if (!pendingClicks.value) return;
    const clicks = Math.min(pendingClicks.value, MAX_BATCH);
    pendingClicks.value = 0;
    sentClicks.value += clicks;
    const mineId = props.mine.id;
    enqueue(() => MiningApiService.dig(mineId, clicks))
        .then(result => emit('changed', result))
        .catch(error => {
            message.value = { text: errorText(error), error: true };
            emit('stale');
        })
        // Ответ пришёл — в шахте и кошельке уже учтены эти клики
        .finally(() => { sentClicks.value = Math.max(0, sentClicks.value - clicks); });
};

/* ===== Склад дворфа ===== */
const stored = computed(() => storedNow(props.mine, props.serverNow));
const collectable = computed(() => Math.floor(stored.value));
const isFull = computed(() => props.mine.heroIds.length > 0 && stored.value >= props.mine.capacity);
const dwarfLocked = computed(() => props.mine.level < props.dwarfUnlockLevel);

/* ===== Действия ===== */
async function act<T extends MiningActionResult>(key: string, request: () => Promise<T>, success: (result: T) => string) {
    if (busy.value) return;
    flush();
    busy.value = key;
    message.value = null;
    try {
        const result = await enqueue(request);
        emit('changed', result);
        message.value = { text: success(result), error: false };
    } catch (error) {
        message.value = { text: errorText(error), error: true };
        emit('stale');
    } finally {
        busy.value = null;
    }
}

const upgrade = (info: MineUpgradeInfo) => act(
    info.upgrade,
    () => MiningApiService.upgrade(props.mine.id, info.upgrade),
    result => t('mining.upgrades.upgraded', {
        name: t(`mining.upgrades.names.${info.upgrade}`),
        level: result.mine.upgrades.find(u => u.upgrade === info.upgrade)?.level ?? info.level + 1,
    }),
);

const assign = async (hero: HeroInfo) => {
    await act('dwarf', () => MiningApiService.assignDwarf(props.mine.id, hero.id), () => t('mining.dwarf.assigned', { name: hero.name }));
    loadHeroes();
};

const remove = async (heroId: string) => {
    await act('dwarf', () => MiningApiService.removeDwarf(props.mine.id, heroId), () => t('mining.dwarf.removed'));
    loadHeroes();
};

const collect = () => act(
    'collect',
    () => MiningApiService.collect(props.mine.id),
    result => t('mining.dwarf.collected', { amount: format(result.collected) }),
);

/* ===== Улучшения ===== */
const upgradeText = (info: MineUpgradeInfo) => {
    if (info.upgrade === 'Level') {
        // Уровень шахты открывает места для дворфов — пишем, сколько открыто и когда следующее
        const slots = t('mining.dwarf.slots', { count: props.mine.dwarfSlots, max: props.mine.maxDwarves });
        const next = props.mine.nextSlotLevel !== null ? ' ' + t('mining.dwarf.nextSlot', { level: props.mine.nextSlotLevel }) : '';
        return `${t('mining.upgrades.effects.Level')} ${slots}${next}`;
    }
    const value = format(info.value);
    const next = info.next !== null ? ` → ${format(info.next)}` : '';
    return t(`mining.upgrades.effects.${info.upgrade}`, { value: `${value}${next}` });
};

const upgradeBlocked = (info: MineUpgradeInfo) => {
    if (info.cost) return null;
    // Уровень шахты упёрся в максимум — и остальным расти некуда
    return info.upgrade === 'Level' || props.mine.level >= props.mine.maxLevel
        ? t('mining.upgrades.maxed')
        : t('mining.upgrades.needLevel');
};

const UPGRADE_ICONS: Record<MineUpgrade, Component> = { Level: IconPickaxe, Speed: IconHourglass, Yield: IconMinecart, Storage: IconCrate };

/* ===== Открытие и закрытие ===== */
watch(open, isOpen => {
    window.clearInterval(flushTimer);
    if (!isOpen) {
        flush();
        return;
    }
    message.value = null;
    floats.value = [];
    loadHeroes();
    flushTimer = window.setInterval(flush, FLUSH_MS);
}, { immediate: true });

onBeforeUnmount(() => {
    window.clearInterval(flushTimer);
    flush();
});
</script>

<template>
    <ModalComponent
        v-model="open"
        :header-message="name"
        width="min(1100px, calc(94vw - 40px))"
        height="min(880px, 94vh)"
    >
        <div class="mine" :style="{ '--mine-color': color }">
            <!-- ===== Жила и дворф ===== -->
            <section v-drag-scroll="{ fade: false }" class="mine-left">
                <div class="mine-heading">
                    <span class="mine-level">{{ t('mining.levelOf', { level: mine.level, max: mine.maxLevel }) }}</span>
                    <span class="mine-per-click">{{ t('mining.dig.perClick', { amount: formatClick(mine.perClick) }) }}</span>
                </div>

                <button type="button" class="mine-ore" @click="dig">
                    <span :key="hit" class="mine-ore-art" :class="{ 'mine-ore-hit': hit }">
                        <img v-if="ore" :src="ore" class="mine-ore-image" alt="" draggable="false" />
                        <span v-else class="mine-ore-glyph"><MineResourceIcon :type="mine.type" /></span>
                    </span>
                    <span
                        v-for="float in floats"
                        :key="float.id"
                        class="mine-float"
                        :style="{ left: `${float.x}px`, top: `${float.y}px` }"
                    >+{{ formatClick(float.amount) }}</span>
                </button>
                <div class="mine-hint">{{ t('mining.dig.hint') }}</div>

                <!-- Сила кирки — общий на все шахты запас кликов; доля жилы — сколько набрано до следующей штуки -->
                <div class="mine-meters">
                    <div class="mine-meter" :class="{ 'mine-meter-tired': tired }" v-tooltip="t('mining.dig.energyHint', { max: digEnergyMax, perHour: digEnergyPerHour })">
                        <span class="mine-meter-label"><IconPickaxe /> {{ t('mining.dig.energy', { energy: energyLeft, max: digEnergyMax }) }}</span>
                        <div class="mine-meter-bar"><div class="mine-meter-fill mine-meter-fill-energy" :style="{ width: `${(energy / digEnergyMax) * 100}%` }"></div></div>
                    </div>
                    <div v-if="fractionalClick" class="mine-meter" v-tooltip="t('mining.dig.veinHint')">
                        <span class="mine-meter-label">{{ t('mining.dig.vein', { percent: Math.floor(veinProgress * 100) }) }}</span>
                        <div class="mine-meter-bar"><div class="mine-meter-fill" :style="{ width: `${veinProgress * 100}%` }"></div></div>
                    </div>
                </div>

                <div class="mine-dwarf">
                    <h4 class="mine-subtitle">{{ t('mining.dwarf.title') }}</h4>

                    <div v-if="dwarfLocked" class="mine-hint">{{ t('mining.dwarf.locked', { level: dwarfUnlockLevel }) }}</div>
                    <template v-else>
                        <div class="mine-hint">{{ t('mining.dwarf.hint') }}</div>

                        <div v-if="mine.heroIds.length || collectable > 0" class="mine-storage">
                            <div class="mine-storage-text">
                                <span>{{ t('mining.dwarf.storage', { stored: format(collectable), capacity: format(mine.capacity) }) }}</span>
                                <span v-if="mine.heroIds.length" class="mine-storage-rate">{{ t('mining.dwarf.rate', { rate: format(mine.ratePerHour) }) }}</span>
                            </div>
                            <div class="mine-bar" :class="{ 'mine-bar-full': isFull }">
                                <div class="mine-bar-fill" :style="{ width: `${Math.min(100, (stored / mine.capacity) * 100)}%` }"></div>
                            </div>
                            <GemButton :color="'yellow'" :disabled="!!busy || collectable < 1" @click="collect">
                                {{ isFull ? t('mining.dwarf.full') + ' ' : '' }}{{ t('mining.dwarf.collect') }}
                            </GemButton>
                        </div>

                        <!-- Места шахты: дворф с кнопкой «Отозвать» под карточкой, свободное место или закрытое до уровня -->
                        <div class="mine-places">
                            <div v-for="place in places" :key="place.key" class="mine-place">
                                <template v-if="place.heroId">
                                    <HeroCard v-if="place.hero" :hero="place.hero" />
                                    <div v-else class="mine-place-empty">…</div>
                                    <GemButton class="mine-place-recall" :color="'red'" :disabled="!!busy" @click="remove(place.heroId)">
                                        {{ t('mining.dwarf.remove') }}
                                    </GemButton>
                                </template>
                                <div v-else-if="place.open" class="mine-place-empty">{{ t('mining.dwarf.free') }}</div>
                                <div v-else class="mine-place-empty mine-place-locked" v-tooltip="t('mining.dwarf.placeLocked')">
                                    <IconLock size="22px" />
                                </div>
                            </div>
                        </div>

                        <template v-if="hasFreePlace">
                            <div class="mine-hint">{{ t('mining.dwarf.choose') }}</div>
                            <HeroFilters v-model:element="elementFilter" v-model:rank="rankFilter" :heroes="freeHeroes" />
                            <div v-drag-scroll class="mine-candidates">
                                <HeroCard
                                    v-for="hero in candidates"
                                    :key="hero.id"
                                    :hero="hero"
                                    selectable
                                    :disabled="!!busy"
                                    @click="assign(hero)"
                                />
                                <div v-if="!candidates.length" class="mine-hint">{{ freeHeroes.length ? t('heroFilters.nothing') : t('mining.dwarf.noHeroes') }}</div>
                            </div>
                        </template>
                    </template>
                </div>
            </section>

            <!-- ===== Улучшения ===== -->
            <section v-drag-scroll="{ fade: false }" class="mine-right">
                <h4 class="mine-subtitle">{{ t('mining.upgrades.title') }}</h4>
                <div v-for="info in mine.upgrades" :key="info.upgrade" class="mine-upgrade">
                    <span class="mine-upgrade-icon"><component :is="UPGRADE_ICONS[info.upgrade]" /></span>
                    <div class="mine-upgrade-body">
                        <div class="mine-upgrade-title">
                            <span>{{ t(`mining.upgrades.names.${info.upgrade}`) }}</span>
                            <span class="mine-upgrade-level">{{ info.level }} / {{ info.maxLevel }}</span>
                        </div>
                        <div class="mine-upgrade-effect">{{ upgradeText(info) }}</div>
                        <div class="mine-upgrade-action">
                            <MineCostView v-if="info.cost" :cost="info.cost" :wallet="wallet" />
                            <span v-else class="mine-upgrade-blocked">{{ upgradeBlocked(info) }}</span>
                            <GemButton
                                v-if="info.cost"
                                :color="'green'"
                                :disabled="!!busy || !canAfford(wallet, info.cost)"
                                @click="upgrade(info)"
                            >{{ t('mining.upgrades.upgrade') }}</GemButton>
                        </div>
                    </div>
                </div>
            </section>
        </div>

        <template #modalFooter>
            <div class="mine-footer">
                <span class="mine-message" :class="{ 'mine-message-error': message?.error }">{{ message?.text ?? '' }}</span>
            </div>
        </template>
    </ModalComponent>
</template>

<style scoped>
.mine {
    --mine-color: #cbd5e1;
    flex: 1;
    min-width: 0;
    display: grid;
    grid-template-columns: minmax(0, 1fr) minmax(300px, 1fr);
    gap: 14px;
    font-family: 'Arial', sans-serif;
}

.mine-left,
.mine-right {
    min-height: 0;
    display: flex;
    flex-direction: column;
    row-gap: 8px;
    padding: 12px 14px;
    overflow-y: auto;
    border-radius: 12px;
    background: rgba(15, 23, 32, 0.8);
    border: 2px solid rgba(157, 173, 175, 0.7);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
}

.mine-heading {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
}

.mine-level {
    color: #fde68a;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.mine-per-click {
    color: var(--mine-color);
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.mine-subtitle {
    margin: 4px 0 0;
    color: #f8fafc;
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

/* Полоски под жилой: сила кирки и доля до следующей штуки */
.mine-meters {
    display: flex;
    flex-direction: column;
    row-gap: 6px;
    margin-bottom: 8px;
}

.mine-meter {
    display: flex;
    align-items: center;
    column-gap: 10px;
    font-family: 'Arial', sans-serif;
}

.mine-meter-label {
    flex: 0 0 auto;
    min-width: 150px;
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    color: #e2e8f0;
    font-size: 12px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.mine-meter-bar {
    flex: 1;
    height: 10px;
    overflow: hidden;
    border-radius: 5px;
    background: rgba(0, 0, 0, 0.55);
    box-shadow: inset 0 0 3px #000, 0 0 0 1px rgba(157, 173, 175, 0.45);
}

.mine-meter-fill {
    height: 100%;
    background: linear-gradient(to bottom, color-mix(in srgb, var(--mine-color) 70%, white), var(--mine-color));
    transition: width 0.25s ease;
}

.mine-meter-fill-energy {
    background: linear-gradient(to bottom, #fde68a, #f59e0b);
}

/* Кирка устала — полоска и подпись краснеют */
.mine-meter-tired .mine-meter-label {
    color: #fca5a5;
}

.mine-meter-tired .mine-meter-fill-energy {
    background: linear-gradient(to bottom, #fca5a5, #b91c1c);
}

.mine-hint {
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

/* ===== Жила ===== */
.mine-ore {
    position: relative;
    flex-shrink: 0;
    height: 220px;
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 0;
    overflow: hidden;
    border-radius: 12px;
    border: 2px solid #333;
    background: radial-gradient(circle at 50% 60%, color-mix(in srgb, var(--mine-color) 30%, transparent) 0%, rgba(0, 0, 0, 0.6) 65%), #1b1210;
    box-shadow: 0 0 0 3px rgb(157, 173, 175), inset 0 0 40px rgba(0, 0, 0, 0.85);
    cursor: pointer;
    user-select: none;
}

.mine-ore:hover .mine-ore-art {
    filter: brightness(1.15);
}

.mine-ore-art {
    display: flex;
    transition: filter 0.15s ease;
}

.mine-ore-hit {
    animation: mine-ore-hit 0.18s ease-out;
}

@keyframes mine-ore-hit {
    0% { transform: scale(1); }
    40% { transform: scale(0.9) rotate(-3deg); }
    100% { transform: scale(1); }
}

.mine-ore-image {
    max-height: 190px;
    max-width: 100%;
    object-fit: contain;
    pointer-events: none;
}

.mine-ore-glyph {
    color: var(--mine-color);
    font-size: 120px;
    line-height: 1;
    filter: drop-shadow(0 0 16px var(--mine-color)) drop-shadow(0 4px 4px #000);
}

.mine-float {
    position: absolute;
    z-index: 1;
    color: var(--mine-color);
    font-size: 22px;
    font-weight: 900;
    white-space: nowrap;
    text-shadow: 0 0 6px rgba(0, 0, 0, 0.9), 0 2px 2px #000;
    pointer-events: none;
    transform: translate(-50%, -50%);
    animation: mine-float 0.9s ease-out forwards;
}

@keyframes mine-float {
    0% { opacity: 0; transform: translate(-50%, -30%) scale(0.7); }
    20% { opacity: 1; transform: translate(-50%, -70%) scale(1.15); }
    100% { opacity: 0; transform: translate(-50%, -260%) scale(1); }
}

/* ===== Дворф и склад ===== */
.mine-dwarf {
    display: flex;
    flex-direction: column;
    row-gap: 8px;
}

.mine-storage {
    display: flex;
    flex-direction: column;
    row-gap: 6px;
}

.mine-storage-text {
    display: flex;
    justify-content: space-between;
    color: #e2e8f0;
    font-size: 13px;
    font-weight: bold;
}

.mine-storage-rate {
    color: var(--mine-color);
}

.mine-bar {
    height: 12px;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.6);
    box-shadow: inset 0 0 3px #000;
    overflow: hidden;
}

.mine-bar-fill {
    height: 100%;
    background: linear-gradient(to bottom, color-mix(in srgb, var(--mine-color) 60%, white), var(--mine-color));
    transition: width 0.9s linear;
}

.mine-bar-full {
    box-shadow: inset 0 0 3px #000, 0 0 10px 1px var(--green-gem-color);
}

/* Места дворфов в ряд (на узком окне — в два); сверху место под звёзды ранга карточки */
.mine-places {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 10px;
    padding-top: 10px;
}

/* Карточка дворфа, под ней — «Отозвать» */
.mine-place {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 8px;
}

.mine-place-recall :deep(.gem-text) {
    font-size: 13px;
}

/* Пустое место — пунктирная рамка размером с карточку дворфа */
.mine-place-empty {
    box-sizing: border-box;
    width: 136px;
    min-height: 196px;
    display: flex;
    justify-content: center;
    align-items: center;
    border: 2px dashed rgba(157, 173, 175, 0.5);
    border-radius: 16px;
    background: rgba(0, 0, 0, 0.3);
    color: rgba(203, 213, 225, 0.6);
    font-size: 13px;
    text-transform: uppercase;
}

/* Закрытое место — откроется с уровнем шахты */
.mine-place-locked {
    border-style: solid;
    border-color: rgba(157, 173, 175, 0.25);
    background: rgba(0, 0, 0, 0.5);
    cursor: help;
}

.mine-candidates {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 10px;
    padding: 10px 4px 4px;
}

/* ===== Улучшения ===== */
.mine-upgrade {
    display: flex;
    column-gap: 12px;
    padding: 10px 0;
    border-bottom: 1px solid rgba(157, 173, 175, 0.2);
}

.mine-upgrade-icon {
    flex-shrink: 0;
    width: 42px;
    height: 42px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.45);
    box-shadow: inset 0 0 6px #000, 0 0 0 1px rgba(157, 173, 175, 0.5);
    font-size: 22px;
}

.mine-upgrade-body {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 4px;
}

.mine-upgrade-title {
    display: flex;
    justify-content: space-between;
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.mine-upgrade-level {
    color: #fde68a;
    font-variant-numeric: tabular-nums;
}

.mine-upgrade-effect {
    color: #cbd5e1;
    font-size: 13px;
}

.mine-upgrade-action {
    display: flex;
    justify-content: space-between;
    align-items: center;
    column-gap: 10px;
    font-size: 14px;
}

.mine-upgrade-blocked {
    color: rgba(203, 213, 225, 0.6);
    font-size: 13px;
    font-weight: bold;
}

/* ===== Футер ===== */
.mine-footer {
    flex: 1;
    display: flex;
    align-items: center;
}

.mine-message {
    flex: 1;
    text-align: left;
    color: #fff;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.mine-message-error {
    color: #ffb4b4;
}
</style>
