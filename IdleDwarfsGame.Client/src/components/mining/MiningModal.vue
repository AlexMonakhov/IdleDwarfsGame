<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import BuildMineModal from '@/components/mining/BuildMineModal.vue';
import MineModal from '@/components/mining/MineModal.vue';
import { mineColor, mineGlyph, mineImage, plotImage } from '@/components/mining/miningArt';
import { GEM_COLORS } from '@/components/inventory/lootStyles';
import MiningApiService from '@/services/apiServices/MiningApiService';
import { useResources } from '@/composables/UseResources';
import { GemType, MineInfo, MiningActionResult, MiningBoard, MiningWallet, storedNow } from '@/models/types/Mining';

// Шахты: места под шахты, по клику на свободное — постройка, на шахту — её окно (поверх этого)
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t, locale } = useI18n();
const { set: setResources } = useResources();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const board = ref<MiningBoard | null>(null);
const loading = ref(false);
const errorMessage = ref<string | null>(null);

/* ===== Время ===== */
// Склад дворфа считаем по часам сервера: offset — насколько они расходятся с часами браузера
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;
const serverNow = computed(() => now.value + clockOffset);

const load = async () => {
    loading.value = true;
    errorMessage.value = null;
    try {
        const next = await MiningApiService.getBoard();
        clockOffset = new Date(next.serverTime).getTime() - Date.now();
        now.value = Date.now();
        board.value = next;
        setResources({ gold: next.wallet.gold, experience: next.wallet.experience });
    } catch {
        errorMessage.value = t('mining.loadError');
    } finally {
        loading.value = false;
    }
};

// Доску перечитываем при каждом открытии, а часы тикают только пока окно открыто
watch(open, isOpen => {
    window.clearInterval(ticker);
    if (!isOpen) return;

    load();
    ticker = window.setInterval(() => { now.value = Date.now(); }, 1000);
}, { immediate: true });

onBeforeUnmount(() => window.clearInterval(ticker));

/* ===== Места ===== */
const plots = computed(() => {
    const count = board.value?.plotCount ?? 0;
    return Array.from({ length: count }, (_, i) => ({
        plot: i + 1,
        mine: board.value?.mines.find(m => m.plot === i + 1) ?? null,
    }));
});

const emptyPlotImage = plotImage();

const format = (value: number) => Math.floor(value).toLocaleString(locale.value);

const storedOf = (mine: MineInfo) => storedNow(mine, serverNow.value);
const isFull = (mine: MineInfo) => !!mine.heroId && storedOf(mine) >= mine.capacity;

// Кристаллы — те, что есть у игрока (золото и опыт и так видны в шапке)
const crystals = computed(() => Object.entries(board.value?.wallet.crystals ?? {})
    .map(([gem, quantity]) => ({ gem: gem as GemType, quantity: quantity ?? 0 }))
    .filter(c => c.quantity > 0));

/* ===== Окна ===== */
// Место и шахта остаются выбранными и после закрытия: окно не размонтируется посреди анимации,
// а окно шахты успевает отправить последние клики
const buildPlot = ref(1);
const buildOpen = ref(false);

const selectedMineId = ref<string | null>(null);
const selectedMine = computed(() => board.value?.mines.find(m => m.id === selectedMineId.value) ?? null);
const mineOpen = ref(false);

const openPlot = (plot: number, mine: MineInfo | null) => {
    if (mine) {
        selectedMineId.value = mine.id;
        mineOpen.value = true;
    } else {
        buildPlot.value = plot;
        buildOpen.value = true;
    }
};

// Результат действия из окон: шахта и ресурсы после него
const applyResult = (result: MiningActionResult) => {
    if (!board.value) return;
    const mines = board.value.mines.filter(m => m.id !== result.mine.id);
    board.value = { ...board.value, mines: [...mines, result.mine].sort((a, b) => a.plot - b.plot) };
    applyWallet(result.wallet);
};

const applyWallet = (wallet: MiningWallet) => {
    if (board.value) board.value = { ...board.value, wallet };
    setResources({ gold: wallet.gold, experience: wallet.experience });
};

// После постройки цена следующей шахты другая — перечитываем места и сразу открываем новую шахту
const onBuilt = async (result: MiningActionResult) => {
    buildOpen.value = false;
    applyResult(result);
    await load();
    selectedMineId.value = result.mine.id;
    mineOpen.value = true;
};
</script>

<template>
    <ModalComponent
        v-model="open"
        :header-message="t('mining.title')"
        width="min(1100px, calc(94 * var(--vw)))"
        height="min(780px, calc(90 * var(--vh)))"
    >
        <div class="mining">
            <div class="mining-hint">{{ t('mining.hint', { level: board?.dwarfUnlockLevel ?? 2 }) }}</div>

            <div v-if="crystals.length" class="mining-wallet">
                <span
                    v-for="crystal in crystals"
                    :key="crystal.gem"
                    class="mining-crystal"
                    :style="{ color: GEM_COLORS[crystal.gem] }"
                    v-tooltip="t(`gems.${crystal.gem}`)"
                >◆ {{ format(crystal.quantity) }}</span>
            </div>

            <div v-if="loading && !board" class="mining-hint">{{ t('common.loading') }}</div>
            <div v-else-if="errorMessage" class="mining-hint mining-hint-error">{{ errorMessage }}</div>

            <div v-else class="mining-plots">
                <button
                    v-for="{ plot, mine } in plots"
                    :key="plot"
                    type="button"
                    class="mining-plot"
                    :class="{ 'mining-plot-empty': !mine, 'mining-plot-full': mine && isFull(mine) }"
                    :style="mine ? { '--mine-color': mineColor(mine.type) } : undefined"
                    @click="openPlot(plot, mine)"
                >
                    <template v-if="mine">
                        <div class="mining-plot-art">
                            <img v-if="mineImage(mine.type)" :src="mineImage(mine.type)!" class="mining-plot-image" alt="" />
                            <span v-else class="mining-plot-glyph">⛏</span>
                            <span class="mining-plot-resource">{{ mineGlyph(mine.type) }}</span>
                            <span class="mining-plot-level">{{ t('mining.level', { level: mine.level }) }}</span>
                        </div>
                        <div class="mining-plot-name">{{ t(`mining.types.${mine.type}`) }}</div>
                        <div v-if="mine.heroId" class="mining-plot-storage">
                            <div class="mining-bar">
                                <div class="mining-bar-fill" :style="{ width: `${Math.min(100, (storedOf(mine) / mine.capacity) * 100)}%` }"></div>
                            </div>
                            <span>{{ isFull(mine) ? t('mining.dwarf.full') : t('mining.dwarf.storage', { stored: format(storedOf(mine)), capacity: format(mine.capacity) }) }}</span>
                        </div>
                        <div v-else class="mining-plot-sub">{{ t('mining.dig.perClick', { amount: mine.perClick }) }}</div>
                    </template>
                    <template v-else>
                        <div class="mining-plot-art">
                            <img v-if="emptyPlotImage" :src="emptyPlotImage" class="mining-plot-image" alt="" />
                            <span v-else class="mining-plot-glyph mining-plot-glyph-empty">＋</span>
                        </div>
                        <div class="mining-plot-name">{{ t('mining.emptyPlot') }}</div>
                        <div class="mining-plot-sub">{{ t('mining.buildHere') }}</div>
                    </template>
                </button>
            </div>
        </div>
    </ModalComponent>

    <BuildMineModal
        v-if="board"
        v-model="buildOpen"
        :plot="buildPlot"
        :cost="board.buildCost"
        :wallet="board.wallet"
        @built="onBuilt"
    />
    <MineModal
        v-if="board && selectedMine"
        v-model="mineOpen"
        :mine="selectedMine"
        :wallet="board.wallet"
        :dwarf-unlock-level="board.dwarfUnlockLevel"
        :server-now="serverNow"
        @changed="applyResult"
        @wallet="applyWallet"
        @stale="load"
    />
</template>

<style scoped>
.mining {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    padding: 6px;
    font-family: 'Arial', sans-serif;
}

.mining-hint {
    margin-bottom: 14px;
    color: #cbd5e1;
    font-size: 14px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.mining-hint-error {
    color: #ffb4b4;
}

.mining-wallet {
    display: inline-flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 14px;
    margin-bottom: 18px;
    padding: 6px 14px;
    border-radius: 10px;
    background: var(--wood-background);
    box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.mining-plots {
    align-self: stretch;
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
    gap: 20px;
}

.mining-plot {
    --mine-color: #cbd5e1;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
    padding: 12px 12px 14px;
    border-radius: 14px;
    border: 2px solid rgba(157, 173, 175, 0.7);
    background: rgba(15, 23, 32, 0.8);
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5);
    color: inherit;
    font: inherit;
    cursor: pointer;
    transition: transform 0.12s ease, border-color 0.15s ease, box-shadow 0.15s ease;
}

.mining-plot:hover {
    transform: translateY(-3px);
    border-color: var(--mine-color);
    box-shadow: 0 6px 14px rgba(0, 0, 0, 0.6), 0 0 12px 2px var(--mine-color);
}

.mining-plot-empty {
    border-style: dashed;
    background: rgba(0, 0, 0, 0.35);
}

.mining-plot-empty:hover {
    border-color: var(--yellow-gem-color);
    box-shadow: 0 6px 14px rgba(0, 0, 0, 0.6), 0 0 12px 2px var(--yellow-gem-color);
}

/* Склад полон — пора забирать */
.mining-plot-full {
    border-color: var(--green-gem-color);
    animation: mining-plot-ready 1.6s ease-in-out infinite;
}

@keyframes mining-plot-ready {
    0%, 100% { box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5), 0 0 6px 1px var(--green-gem-color); }
    50% { box-shadow: 0 4px 10px rgba(0, 0, 0, 0.5), 0 0 18px 4px var(--green-gem-color); }
}

.mining-plot-art {
    position: relative;
    width: 100%;
    height: 140px;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 10px;
    background: radial-gradient(circle at 50% 70%, color-mix(in srgb, var(--mine-color) 25%, transparent) 0%, rgba(0, 0, 0, 0.4) 70%);
    box-shadow: inset 0 0 14px #000;
}

.mining-plot-image {
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
}

.mining-plot-glyph {
    font-size: 64px;
    line-height: 1;
    filter: drop-shadow(0 2px 4px #000);
}

.mining-plot-glyph-empty {
    color: rgba(203, 213, 225, 0.45);
    font-size: 54px;
}

.mining-plot-resource {
    position: absolute;
    bottom: 6px;
    right: 8px;
    color: var(--mine-color);
    font-size: 26px;
    filter: drop-shadow(0 0 6px var(--mine-color));
}

.mining-plot-level {
    position: absolute;
    top: 6px;
    left: 8px;
    padding: 2px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.65);
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
}

.mining-plot-name {
    color: #f8fafc;
    font-size: 15px;
    font-weight: bold;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.mining-plot-sub {
    color: #cbd5e1;
    font-size: 12px;
}

.mining-plot-storage {
    width: 100%;
    display: flex;
    flex-direction: column;
    row-gap: 4px;
    color: #e2e8f0;
    font-size: 12px;
    font-weight: bold;
}

.mining-bar {
    height: 10px;
    border-radius: 5px;
    background: rgba(0, 0, 0, 0.6);
    box-shadow: inset 0 0 3px #000;
    overflow: hidden;
}

.mining-bar-fill {
    height: 100%;
    background: linear-gradient(to bottom, color-mix(in srgb, var(--mine-color) 60%, white), var(--mine-color));
    transition: width 0.9s linear;
}
</style>
