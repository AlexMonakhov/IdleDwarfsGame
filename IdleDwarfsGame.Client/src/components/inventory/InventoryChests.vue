<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { AxiosError } from 'axios';
import GemTile from './GemTile.vue';
import ChestFilters from './ChestFilters.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import ItemTile from '@/components/equipment/ItemTile.vue';
import InventoryApiService from '@/services/apiServices/InventoryApiService';
import { ChestInfo, ChestOpenResult, ChestType } from '@/models/types/Inventory';
import { CHEST_ICONS, ELEMENT_COLORS, RARITY_COLORS } from './lootStyles';
import { HeroElement } from '@/battle/types';
import { chestLabel } from '@/i18n';
import GlassButton from '@/components/common/GlassButton.vue';
import GemButton from '@/components/common/GemButton.vue';
import { RARITY_ORDER } from '@/models/types/Forge';
import type { Rarity } from '@/models/types/Inventory';

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
const opened = ref<ChestOpenResult | null>(null);

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
        opened.value = await InventoryApiService.openChest(chestId);
        emit('opened');
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
    <div class="inventory-chests">
        <div v-if="loading" class="inventory-hint">{{ t('common.loading') }}</div>
        <template v-else>
            <div v-if="errorMessage" class="inventory-hint inventory-hint-error">{{ errorMessage }}</div>

            <section v-if="opened" class="chest-result">
                <div class="chest-result-title">
                    <component :is="CHEST_ICONS[opened.chest.type]" /> {{ chestLabel(opened.chest) }}
                    — {{ opened.items.length || opened.heroes.length || opened.equipment?.length ? t('chests.dropped') : t('chests.nothing') }}
                </div>
                <div class="inventory-grid">
                    <GemTile v-for="item in opened.items" :key="`${item.subType}-${item.rarity}`" :item="item" />
                    <HeroCard v-for="hero in opened.heroes" :key="hero.id" :hero="hero" selectable />
                    <ItemTile v-for="item in opened.equipment" :key="item.id" :item="item" />
                </div>
            </section>

            <div class="inventory-hint">
                {{ chests.length ? t('inventory.chestsCount', { count: chests.length }) : t('inventory.noChests') }}
            </div>
            <ChestFilters v-model:type="typeFilter" v-model:level="levelFilter" :chests="chests" />
            <div v-if="chests.length && !groups.length" class="inventory-hint">{{ t('lootFilters.nothing') }}</div>
            <div class="inventory-grid">
                <!-- Карточка как у предмета (lootTile.css): стекло цвета стихии сундука (бесконечности) или его уровня -->
                <div
                    v-for="group in groups"
                    :key="group.key"
                    class="loot-tile chest-card"
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
                    </GlassButton>
                    <span v-if="group.ids.length > 1" class="loot-tile-count">×{{ group.ids.length }}</span>
                    <GemButton class="chest-card-open" :color="'yellow'" :disabled="opening" @click="open(group.ids[0])">
                        {{ opening ? t('chests.opening') : t('chests.open') }}
                    </GemButton>
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

.chest-result {
    margin-bottom: 14px;
    padding: 10px;
    border: 1px solid rgba(255, 209, 102, 0.55);
    border-radius: 12px;
    background: rgba(14, 19, 34, 0.55);
}

.chest-result-title {
    margin: 0 4px 8px;
    text-align: center;
    color: #ffd166;
    font-family: 'Arial', sans-serif;
    font-size: 14px;
    font-weight: 700;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

/* Стихия сундука бесконечности — под названием, её цветом */
.chest-card-element {
    font-size: 12px;
    font-weight: 700;
}

/* Кнопка под стеклом, на оправе */
.chest-card .chest-card-open {
    align-self: center;
    margin-top: 8px;
}

.chest-card-open :deep(.gem-text) {
    font-size: 13px;
}
</style>

<style scoped src="./lootTile.css"></style>
