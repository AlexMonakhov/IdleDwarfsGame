<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch, type Component } from 'vue';
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
    storedNow,
} from '@/models/types/Mining';
import { vDragScroll } from '@/directives/dragScroll';
import MineResourceIcon from './MineResourceIcon.vue';
import { IconCrate, IconHourglass, IconMinecart, IconPickaxe } from '@/components/icons/ui';

// Окно шахты: слева жила (клик — добыча) и дворф со складом, справа улучшения
const props = defineProps<{
    modelValue: boolean;
    mine: MineInfo;
    wallet: MiningWallet;
    dwarfUnlockLevel: number;
    // Время сервера (мс) — по нему досчитываем склад
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

const dwarf = computed(() => heroes.value.find(h => h.id === props.mine.heroId) ?? null);

// В шахту идут свободные герои из инвентаря: не из отряда, не в экспедиции и не в другой шахте
const candidates = computed(() => heroes.value
    .filter(h => h.placeInSquad === null && !isBusy(h))
    .sort((a, b) => b.level - a.level || a.name.localeCompare(b.name)));

/* ===== Добыча кликами ===== */
const MAX_BATCH = 50;
const FLUSH_MS = 1000;
const FLOAT_MS = 900;

let pendingClicks = 0;
let flushTimer = 0;

interface FloatNumber { id: number; amount: number; x: number; y: number }
const floats = ref<FloatNumber[]>([]);
let floatId = 0;
// Номер удара: новый ключ перезапускает анимацию жилы
const hit = ref(0);

const dig = (event: MouseEvent) => {
    pendingClicks++;
    hit.value++;

    const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    const float = { id: ++floatId, amount: props.mine.perClick, x: event.clientX - rect.left, y: event.clientY - rect.top };
    floats.value = [...floats.value, float];
    window.setTimeout(() => { floats.value = floats.value.filter(f => f.id !== float.id); }, FLOAT_MS);
};

// Клики уходят на сервер пачкой раз в секунду; лишние сверх его лимита он просто не засчитает
const flush = () => {
    if (!pendingClicks) return;
    const clicks = Math.min(pendingClicks, MAX_BATCH);
    pendingClicks = 0;
    const mineId = props.mine.id;
    enqueue(() => MiningApiService.dig(mineId, clicks))
        .then(result => emit('changed', result))
        .catch(error => {
            message.value = { text: errorText(error), error: true };
            emit('stale');
        });
};

/* ===== Склад дворфа ===== */
const stored = computed(() => storedNow(props.mine, props.serverNow));
const collectable = computed(() => Math.floor(stored.value));
const isFull = computed(() => !!props.mine.heroId && stored.value >= props.mine.capacity);
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

const remove = async () => {
    await act('dwarf', () => MiningApiService.removeDwarf(props.mine.id), () => t('mining.dwarf.removed'));
    loadHeroes();
};

const collect = () => act(
    'collect',
    () => MiningApiService.collect(props.mine.id),
    result => t('mining.dwarf.collected', { amount: format(result.collected) }),
);

/* ===== Улучшения ===== */
const upgradeText = (info: MineUpgradeInfo) => {
    if (info.upgrade === 'Level') return t('mining.upgrades.effects.Level');
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
        height="min(760px, 90vh)"
    >
        <div class="mine" :style="{ '--mine-color': color }">
            <!-- ===== Жила и дворф ===== -->
            <section class="mine-left">
                <div class="mine-heading">
                    <span class="mine-level">{{ t('mining.levelOf', { level: mine.level, max: mine.maxLevel }) }}</span>
                    <span class="mine-per-click">{{ t('mining.dig.perClick', { amount: mine.perClick }) }}</span>
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
                    >+{{ float.amount }}</span>
                </button>
                <div class="mine-hint">{{ t('mining.dig.hint') }}</div>

                <div class="mine-dwarf">
                    <h4 class="mine-subtitle">{{ t('mining.dwarf.title') }}</h4>

                    <div v-if="dwarfLocked" class="mine-hint">{{ t('mining.dwarf.locked', { level: dwarfUnlockLevel }) }}</div>
                    <template v-else>
                        <div class="mine-hint">{{ t('mining.dwarf.hint') }}</div>

                        <div v-if="mine.heroId || collectable > 0" class="mine-storage">
                            <div class="mine-storage-text">
                                <span>{{ t('mining.dwarf.storage', { stored: format(collectable), capacity: format(mine.capacity) }) }}</span>
                                <span v-if="mine.heroId" class="mine-storage-rate">{{ t('mining.dwarf.rate', { rate: format(mine.ratePerHour) }) }}</span>
                            </div>
                            <div class="mine-bar" :class="{ 'mine-bar-full': isFull }">
                                <div class="mine-bar-fill" :style="{ width: `${Math.min(100, (stored / mine.capacity) * 100)}%` }"></div>
                            </div>
                            <GemButton :color="'yellow'" :disabled="!!busy || collectable < 1" @click="collect">
                                {{ isFull ? t('mining.dwarf.full') + ' ' : '' }}{{ t('mining.dwarf.collect') }}
                            </GemButton>
                        </div>

                        <div v-if="mine.heroId" class="mine-dwarf-current">
                            <HeroCard v-if="dwarf" :hero="dwarf" />
                            <GemButton :color="'red'" :disabled="!!busy" @click="remove">{{ t('mining.dwarf.remove') }}</GemButton>
                        </div>
                        <template v-else>
                            <div class="mine-hint">{{ t('mining.dwarf.choose') }}</div>
                            <div v-drag-scroll class="mine-candidates">
                                <HeroCard
                                    v-for="hero in candidates"
                                    :key="hero.id"
                                    :hero="hero"
                                    selectable
                                    :disabled="!!busy"
                                    @click="assign(hero)"
                                />
                                <div v-if="!candidates.length" class="mine-hint">{{ t('mining.dwarf.noHeroes') }}</div>
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

.mine-dwarf-current {
    display: flex;
    align-items: center;
    column-gap: 14px;
    /* Сверху место под звёзды ранга карточки */
    padding-top: 10px;
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
