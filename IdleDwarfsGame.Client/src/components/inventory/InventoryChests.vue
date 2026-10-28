<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { AxiosError } from 'axios';
import GemTile from './GemTile.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import InventoryApiService from '@/services/apiServices/InventoryApiService';
import { ChestInfo, ChestOpenResult, ChestType } from '@/models/types/Inventory';
import { CHEST_ICONS } from './lootStyles';

const emit = defineEmits<{ (e: 'opened'): void }>();

const { t } = useI18n();

const chests = ref<ChestInfo[]>([]);
const loading = ref(true);
const opening = ref(false);
const errorMessage = ref<string | null>(null);
const opened = ref<ChestOpenResult | null>(null);

// Одинаковые сундуки показываем одной карточкой со счётчиком, «Открыть» открывает один из них
const groups = computed(() => {
    const byKind = new Map<string, { type: ChestType; level: number; ids: string[] }>();
    for (const chest of chests.value) {
        const key = `${chest.type}-${chest.level}`;
        const group = byKind.get(key) ?? { type: chest.type, level: chest.level, ids: [] };
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
                    {{ CHEST_ICONS[opened.chest.type] }} {{ t('chests.name', { type: t(`chests.types.${opened.chest.type}`), level: opened.chest.level }) }}
                    — {{ opened.items.length || opened.heroes.length ? t('chests.dropped') : t('chests.nothing') }}
                </div>
                <div class="inventory-grid">
                    <GemTile v-for="item in opened.items" :key="`${item.subType}-${item.rarity}`" :item="item" />
                    <HeroCard v-for="hero in opened.heroes" :key="hero.id" :hero="hero" selectable />
                </div>
            </section>

            <div class="inventory-hint">
                {{ chests.length ? t('inventory.chestsCount', { count: chests.length }) : t('inventory.noChests') }}
            </div>
            <div class="inventory-grid">
                <div v-for="group in groups" :key="group.key" class="chest-card">
                    <span class="chest-card-icon">{{ CHEST_ICONS[group.type] }}</span>
                    <span class="chest-card-name">{{ t(`chests.types.${group.type}`) }}</span>
                    <span class="chest-card-level">{{ t('chests.level', { level: group.level }) }}</span>
                    <span v-if="group.ids.length > 1" class="chest-card-count">×{{ group.ids.length }}</span>
                    <button class="chest-card-open" :disabled="opening" @click="open(group.ids[0])">
                        {{ opening ? t('chests.opening') : t('chests.open') }}
                    </button>
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

.chest-card {
    width: 132px;
    box-sizing: border-box;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 4px;
    padding: 12px 8px 10px;
    border: 1px solid rgba(255, 255, 255, 0.25);
    border-radius: 12px;
    background: linear-gradient(180deg, rgba(14, 19, 34, 0.5), rgba(14, 19, 34, 0.85));
    font-family: 'Arial', sans-serif;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.chest-card-icon {
    font-size: 34px;
    line-height: 1;
    filter: drop-shadow(0 2px 4px #000);
}

.chest-card-name {
    color: #f8fafc;
    font-size: 13px;
    font-weight: 700;
    text-align: center;
}

.chest-card-level {
    color: #cbd5e1;
    font-size: 12px;
}

.chest-card-count {
    color: #ffd166;
    font-size: 13px;
    font-weight: 700;
}

.chest-card-open {
    margin-top: 4px;
    border: 1px solid #ffd166;
    border-radius: 8px;
    padding: 5px 14px;
    background: rgba(14, 19, 34, 0.7);
    color: #ffd166;
    cursor: pointer;
}

.chest-card-open:disabled {
    opacity: 0.5;
    cursor: default;
}
</style>
