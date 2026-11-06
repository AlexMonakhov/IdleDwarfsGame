<script lang="ts" setup>
import { computed, onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import GemTile from './GemTile.vue';
import GemFilters from './GemFilters.vue';
import type { Rarity } from '@/models/types/Inventory';
import InventoryApiService from '@/services/apiServices/InventoryApiService';
import { ItemInfo } from '@/models/types/Inventory';

// refreshToken меняется, когда во вкладке сундуков что-то открыли — тогда перечитываем гемы
const props = defineProps<{ refreshToken?: number }>();

const { t } = useI18n();

const gems = ref<ItemInfo[]>([]);

// Фильтры по камню и ступени
const gemFilter = ref<string | null>(null);
const rarityFilter = ref<Rarity | null>(null);
const stacks = computed(() => gems.value.map(g => ({ gem: g.subType, rarity: g.rarity })));
const visible = computed(() => gems.value
    .filter(g => (!gemFilter.value || g.subType === gemFilter.value) && (!rarityFilter.value || g.rarity === rarityFilter.value)));
const loading = ref(true);
const errorMessage = ref<string | null>(null);

const load = async () => {
    errorMessage.value = null;
    try {
        const inventory = await InventoryApiService.getMyInventory();
        gems.value = inventory.items.filter(item => item.itemType === 'Gem');
    } catch {
        errorMessage.value = t('inventory.loadError');
    } finally {
        loading.value = false;
    }
};

onMounted(load);
watch(() => props.refreshToken, load);
</script>

<template>
    <div class="inventory-gems">
        <div v-if="loading" class="inventory-hint">{{ t('common.loading') }}</div>
        <div v-else-if="errorMessage" class="inventory-hint inventory-hint-error">{{ errorMessage }}</div>
        <div v-else-if="!gems.length" class="inventory-hint">{{ t('inventory.noGems') }}</div>
        <template v-else>
            <GemFilters v-model:gem="gemFilter" v-model:rarity="rarityFilter" :gems="stacks" />
            <div v-if="!visible.length" class="inventory-hint">{{ t('lootFilters.nothing') }}</div>
            <div class="inventory-grid">
                <GemTile v-for="gem in visible" :key="`${gem.subType}-${gem.rarity}-${gem.level}`" :item="gem" />
            </div>
        </template>
    </div>
</template>

<style scoped>
.inventory-gems {
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
</style>
