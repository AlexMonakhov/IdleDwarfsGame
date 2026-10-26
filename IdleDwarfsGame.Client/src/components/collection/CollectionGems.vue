<script lang="ts" setup>
import { onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import GemTile from './GemTile.vue';
import InventoryApiService from '@/services/apiServices/InventoryApiService';
import { ItemInfo } from '@/models/types/Inventory';

// refreshToken меняется, когда во вкладке сундуков что-то открыли — тогда перечитываем гемы
const props = defineProps<{ refreshToken?: number }>();

const { t } = useI18n();

const gems = ref<ItemInfo[]>([]);
const loading = ref(true);
const errorMessage = ref<string | null>(null);

const load = async () => {
    errorMessage.value = null;
    try {
        const inventory = await InventoryApiService.getMyInventory();
        gems.value = inventory.items.filter(item => item.itemType === 'Gem');
    } catch {
        errorMessage.value = t('collection.loadError');
    } finally {
        loading.value = false;
    }
};

onMounted(load);
watch(() => props.refreshToken, load);
</script>

<template>
    <div class="collection-gems">
        <div v-if="loading" class="collection-hint">{{ t('common.loading') }}</div>
        <div v-else-if="errorMessage" class="collection-hint collection-hint-error">{{ errorMessage }}</div>
        <div v-else-if="!gems.length" class="collection-hint">{{ t('collection.noGems') }}</div>
        <div v-else class="collection-grid">
            <GemTile v-for="gem in gems" :key="`${gem.subType}-${gem.rarity}-${gem.level}`" :item="gem" />
        </div>
    </div>
</template>

<style scoped>
.collection-gems {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
}

.collection-hint {
    margin-bottom: 10px;
    text-align: center;
    color: #cbd5e1;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.collection-hint-error {
    color: #ffb4b4;
}

.collection-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 10px;
    padding: 4px;
}
</style>
