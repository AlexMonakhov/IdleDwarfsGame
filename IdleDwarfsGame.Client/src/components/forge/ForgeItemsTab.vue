<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import ItemTile from '@/components/equipment/ItemTile.vue';
import EquipmentIcon from '@/components/equipment/EquipmentIcon.vue';
import ItemFilters from '@/components/equipment/ItemFilters.vue';
import ForgeApiService from '@/services/apiServices/ForgeApiService';
import { RARITY_COLORS } from '@/components/inventory/lootStyles';
import { vDragScroll } from '@/directives/dragScroll';
import type { EquipmentInfo, EquipmentType } from '@/models/types/Equipment';
import type { Rarity } from '@/models/types/Inventory';
import { canForgeFrom, ForgeOverview, nextRarity } from '@/models/types/Forge';
import { forgeErrorText } from './forgeText';

// Переплавка: itemFusionSize снятых предметов одной редкости — в случайный предмет редкостью выше.
// Вид — как у случайного из переплавленных (как стихия при слиянии дворфов на алтаре). Артефакты не куются
const props = defineProps<{ forge: ForgeOverview }>();
const emit = defineEmits<{
    (e: 'changed', forge: ForgeOverview): void;
    // Сервер отказал — данные устарели, окну стоит перечитать их
    (e: 'stale'): void;
}>();

const { t } = useI18n();

const size = computed(() => props.forge.itemFusionSize);
const pickedIds = ref<string[]>([]);
const confirming = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);
const forged = ref<EquipmentInfo | null>(null);
const rarityFilter = ref<Rarity | null>(null);
const typeFilter = ref<EquipmentType | null>(null);

const picked = computed(() => pickedIds.value
    .map(id => props.forge.items.find(i => i.id === id))
    .filter((i): i is EquipmentInfo => !!i));
const rarity = computed(() => picked.value[0]?.rarity ?? null);
const full = computed(() => picked.value.length === size.value);

// Предметы, ушедшие из кузницы (надели в другом окне), из выбора выпадают
watch(() => props.forge.items, () => {
    pickedIds.value = pickedIds.value.filter(id => props.forge.items.some(i => i.id === id));
});

watch(pickedIds, () => { confirming.value = false; }, { deep: true });

// Фильтры по виду и редкости
const visible = computed(() => props.forge.items
    .filter(i => (!rarityFilter.value || i.rarity === rarityFilter.value) && (!typeFilter.value || i.type === typeFilter.value)));

const tooHigh = (item: EquipmentInfo) => !canForgeFrom(item.rarity, props.forge.maxRarity);

const unavailable = (item: EquipmentInfo) =>
    tooHigh(item) || (rarity.value !== null && item.rarity !== rarity.value && !pickedIds.value.includes(item.id));

const reason = (item: EquipmentInfo) => {
    if (tooHigh(item)) return t('forge.items.maxRarity');
    if (rarity.value !== null && item.rarity !== rarity.value) return t('forge.errors.DifferentRarities');
    return null;
};

const toggle = (item: EquipmentInfo) => {
    message.value = null;
    if (pickedIds.value.includes(item.id)) {
        pickedIds.value = pickedIds.value.filter(id => id !== item.id);
        return;
    }
    if (unavailable(item) || full.value) return;
    pickedIds.value = [...pickedIds.value, item.id];
};

// Доля каждого вида среди выбранных — его шанс
const chances = computed(() => {
    const counts = new Map<EquipmentType, number>();
    picked.value.forEach(i => counts.set(i.type, (counts.get(i.type) ?? 0) + 1));
    return [...counts.entries()]
        .map(([type, count]) => ({ type, percent: Math.round((count / picked.value.length) * 100) }))
        .sort((a, b) => b.percent - a.percent);
});

const resultRarity = computed(() => (rarity.value ? nextRarity(rarity.value) : null));

const fuse = async () => {
    if (!full.value || busy.value) return;
    if (!confirming.value) {
        confirming.value = true;
        return;
    }

    busy.value = true;
    try {
        const result = await ForgeApiService.fuseItems(pickedIds.value);
        forged.value = result.item;
        message.value = { text: t('forge.items.done'), error: false };
        pickedIds.value = [];
        emit('changed', result.forge);
    } catch (error) {
        message.value = { text: forgeErrorText(error), error: true };
        emit('stale');
    } finally {
        busy.value = false;
        confirming.value = false;
    }
};

const buttonText = computed(() => {
    if (busy.value) return t('forge.items.forging');
    if (confirming.value) return t('forge.items.confirm', { count: size.value });
    return t('forge.items.forge');
});
</script>

<template>
    <div class="forge">
        <aside class="forge-panel">
            <div class="forge-note">{{ t('forge.items.hint', { count: size }) }}</div>

            <h4 class="forge-subtitle">{{ t('forge.items.slots', { count: picked.length, total: size }) }}</h4>
            <div class="forge-slots">
                <button
                    v-for="index in size"
                    :key="index"
                    class="forge-slot"
                    :class="{ 'forge-slot-filled': picked[index - 1] }"
                    :style="picked[index - 1] ? { '--rarity-color': RARITY_COLORS[picked[index - 1].rarity] } : undefined"
                    :disabled="!picked[index - 1]"
                    @click="picked[index - 1] && toggle(picked[index - 1])"
                >
                    <ItemTile v-if="picked[index - 1]" :item="picked[index - 1]" compact />
                    <span v-else class="forge-slot-empty">+</span>
                </button>
            </div>

            <template v-if="resultRarity">
                <div class="forge-note">
                    {{ t('forge.items.result') }}
                    <span class="forge-rarity" :style="{ color: RARITY_COLORS[resultRarity] }">{{ t(`rarity.${resultRarity}`) }}</span>
                </div>
                <h4 class="forge-subtitle">{{ t('forge.items.chances') }}</h4>
                <div class="forge-chances">
                    <div v-for="chance in chances" :key="chance.type" class="forge-chance">
                        <EquipmentIcon :type="chance.type" />
                        <span class="forge-chance-name">{{ t(`equipment.types.${chance.type}`) }}</span>
                        <span class="forge-chance-value">{{ chance.percent }}%</span>
                    </div>
                </div>
            </template>

            <div class="forge-note forge-note-warn">{{ t('forge.items.lost', { rarity: t(`rarity.${forge.maxRarity}`) }) }}</div>

            <div class="forge-actions">
                <button class="forge-filter" :disabled="!picked.length || busy" @click="pickedIds = []">{{ t('forge.clear') }}</button>
                <GemButton
                    class="forge-action"
                    :color="confirming ? 'red' : 'blue'"
                    :disabled="!full || busy"
                    @click="fuse"
                >{{ buttonText }}</GemButton>
            </div>

            <div v-if="message" class="forge-message" :class="{ 'forge-message-error': message.error }">{{ message.text }}</div>

            <!-- Последний выкованный предмет -->
            <div v-if="forged" class="forge-result">
                <h4 class="forge-subtitle">{{ t('forge.items.forged') }}</h4>
                <ItemTile :item="forged" />
            </div>
        </aside>

        <section v-drag-scroll class="forge-list">
            <ItemFilters v-model:type="typeFilter" v-model:rarity="rarityFilter" :items="forge.items" />
            <div class="forge-grid">
                <button
                    v-for="item in visible"
                    :key="item.id"
                    type="button"
                    class="forge-item"
                    :class="{ 'forge-item-picked': pickedIds.includes(item.id), 'forge-item-off': unavailable(item) }"
                    v-tooltip="reason(item)"
                    @click="toggle(item)"
                >
                    <ItemTile :item="item" />
                </button>
            </div>
            <div v-if="!forge.items.length" class="forge-note">{{ t('forge.items.empty') }}</div>
        </section>
    </div>
</template>

<style scoped src="@/components/abilities/forge.css"></style>
<style scoped src="./forgeShared.css"></style>

<style scoped>
.forge-slot {
    width: 54px;
    height: 54px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    border: 2px dashed rgba(157, 173, 175, 0.6);
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.45);
    cursor: default;
}

.forge-slot-filled {
    border: 2px solid var(--rarity-color, #ffd166);
    box-shadow: 0 0 10px color-mix(in srgb, var(--rarity-color, #ffd166) 50%, transparent);
    cursor: pointer;
}

.forge-slot-empty {
    color: rgba(203, 213, 225, 0.5);
    font-size: 22px;
    font-weight: bold;
}

/* Предмет в списке — кнопка вокруг плитки: выбран — золотая кайма, не подходит — приглушён */
.forge-item {
    display: flex;
    padding: 0;
    border: none;
    border-radius: 16px;
    background: none;
    cursor: pointer;
    transition: transform 0.12s ease, box-shadow 0.15s ease, opacity 0.15s ease;
}

.forge-item:hover:not(.forge-item-off) {
    transform: translateY(-2px);
}

.forge-item-picked {
    box-shadow: 0 0 0 3px #ffd166, 0 0 14px 3px rgba(255, 209, 102, 0.5);
}

.forge-item-off {
    opacity: 0.4;
    filter: grayscale(0.6);
    cursor: not-allowed;
}

.forge-result {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 6px;
}
</style>
