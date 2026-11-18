<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { AxiosError } from 'axios';
import ChestFilters from './ChestFilters.vue';
import ChestOpening from '@/components/chestOpening/ChestOpening.vue';
import { cardsFromResult, type ChestRewardCard } from '@/components/chestOpening/chestRewards';
import { useResources } from '@/composables/UseResources';
import InventoryApiService from '@/services/apiServices/InventoryApiService';
import { ChestInfo, ChestOpenResult, ChestType } from '@/models/types/Inventory';
import { CHEST_ICONS, ELEMENT_COLORS, RARITY_COLORS } from './lootStyles';
import { HeroElement } from '@/battle/types';
import GlassButton from '@/components/common/GlassButton.vue';
import GemButton from '@/components/common/GemButton.vue';
import { RARITY_ORDER } from '@/models/types/Forge';
import type { Rarity } from '@/models/types/Inventory';
import { emitTutorialEvent } from '@/tutorial/events';
import LoadingCrystal from '@/components/loading/LoadingCrystal.vue';

// Уровень сундука — как редкость: 1 — обычный … 5 — легендарный (цвет стекла и свечение)
const chestRarity = (level: number): Rarity => RARITY_ORDER[Math.min(Math.max(level, 1), 5) - 1];
const chestColor = (group: { level: number; element: HeroElement }) =>
    group.element !== 'Basic' ? ELEMENT_COLORS[group.element] : RARITY_COLORS[chestRarity(group.level)];

const emit = defineEmits<{ (e: 'opened'): void }>();

const { t } = useI18n();

const chests = ref<ChestInfo[]>([]);
const loading = ref(true);
const opening = ref(false);
const errorMessage = ref<string | null>(null);
// Открытый сундук: окно с пятью картами (ChestOpening) поверх инвентаря
const opened = ref<{ result: ChestOpenResult; cards: ChestRewardCard[] } | null>(null);
const { refresh: refreshResources } = useResources();

// Одинаковые сундуки показываем одной карточкой со счётчиком, «Открыть» открывает один из них
// Фильтры по виду и уровню
const typeFilter = ref<ChestType | null>(null);
const levelFilter = ref<number | null>(null);

const groups = computed(() => {
    const byKind = new Map<string, { type: ChestType; level: number; element: HeroElement; ids: string[] }>();
    const shown = chests.value.filter(c => (!typeFilter.value || c.type === typeFilter.value) && (!levelFilter.value || c.level === levelFilter.value));
    for (const chest of shown) {
        const key = `${chest.type}-${chest.level}-${chest.element}`;
        const group = byKind.get(key) ?? { type: chest.type, level: chest.level, element: chest.element, ids: [] };
        group.ids.push(chest.id);
        byKind.set(key, group);
    }
    return [...byKind.entries()].map(([key, group]) => ({ key, ...group }));
});

const errorText = (error: unknown, fallback: string) =>
    (error as AxiosError<{ message?: string }>).response?.data?.message ?? fallback;

const load = async () => {
    try {
        chests.value = (await InventoryApiService.getMyInventory()).chests;
    } catch {
        errorMessage.value = t('inventory.loadError');
    } finally {
        loading.value = false;
    }
};

const open = async (chestId: string) => {
    if (opening.value) return;
    opening.value = true;
    errorMessage.value = null;
    try {
        const result = await InventoryApiService.openChest(chestId);
        opened.value = { result, cards: cardsFromResult(result, t) };
        emit('opened');
        emitTutorialEvent('chest:opened');
        // Бонусные золото, опыт и монеты уже начислены — обновляем счётчики
        refreshResources();
    } catch (error) {
        errorMessage.value = errorText(error, t('chests.openError'));
    } finally {
        opening.value = false;
    }
    await load();
};

onMounted(load);
</script>

<template>
    <div v-tutorial="'warehouse.chests'" class="inventory-chests">
        <LoadingCrystal v-if="loading" />
        <template v-else>
            <div v-if="errorMessage" class="inventory-hint inventory-hint-error">{{ errorMessage }}</div>

            <ChestOpening
                v-if="opened"
                :key="opened.result.chest.id"
                :rewards="opened.cards"
                :chest-type="opened.result.chest.type"
                :chest-level="opened.result.chest.level"
                @close="opened = null; emitTutorialEvent('chest:closed')"
            />

            <div class="inventory-hint">
                {{ chests.length ? t('inventory.chestsCount', { count: chests.length }) : t('inventory.noChests') }}
            </div>
            <ChestFilters v-model:type="typeFilter" v-model:level="levelFilter" :chests="chests" />
            <div v-if="chests.length && !groups.length" class="inventory-hint">{{ t('lootFilters.nothing') }}</div>
            <div class="inventory-grid">
                <!-- Карточка как у предмета (lootTile.css): стекло цвета стихии сундука (бесконечности) или его уровня -->
                <div
                    v-for="(group, groupIndex) in groups"
                    :key="group.key"
                    class="loot-tile chest-card"
                    data-sound-hover="ui_hover_card"
                    :class="`loot-tile-${chestRarity(group.level)}`"
                    :style="{ '--rarity-color': chestColor(group) }"
                >
                    <GlassButton tag="div" class="loot-tile-glass">
                        <span class="loot-tile-picture"><component :is="CHEST_ICONS[group.type]" size="52px" /></span>
                        <span class="loot-tile-name">{{ t(`chests.types.${group.type}`) }}</span>
                        <span v-if="group.element !== 'Basic'" class="chest-card-element" :style="{ color: ELEMENT_COLORS[group.element] }">
                            {{ t(`elementNames.${group.element}`) }}
                        </span>
                        <span class="loot-tile-rarity">{{ t('chests.level', { level: group.level }) }}</span>
                        <GemButton v-tutorial="groupIndex === 0 ? 'warehouse.chest.open' : null" class="chest-card-open" :color="'yellow'" data-sound="ui_click_primary" no-filter :disabled="opening" @click="open(group.ids[0])">
                            {{ opening ? t('chests.opening') : t('chests.open') }}
                        </GemButton>
                    </GlassButton>
                    <span v-if="group.ids.length > 1" class="loot-tile-count">×{{ group.ids.length }}</span>
                </div>
            </div>
        </template>
    </div>
</template>

<style scoped>
.inventory-chests {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
}

.inventory-hint {
    margin-bottom: 10px;
    text-align: center;
    color: #cbd5e1;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.inventory-hint-error {
    color: #ffb4b4;
}

.inventory-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 10px;
    padding: 4px;
}

/* Стихия сундука бесконечности — под названием, её цветом */
.chest-card-element {
    font-size: 12px;
    font-weight: 700;
}

/* Кнопка внутри стекла, внизу; поверх блика, чтобы ловить клики */
.chest-card .chest-card-open {
    position: relative;
    z-index: 2;
    align-self: center;
    margin-top: auto;
}

.chest-card .loot-tile-rarity {
    margin-bottom: 6px;
}

.chest-card-open :deep(.gem-text) {
    font-size: 13px;
}
</style>

<style scoped src="./lootTile.css"></style>
