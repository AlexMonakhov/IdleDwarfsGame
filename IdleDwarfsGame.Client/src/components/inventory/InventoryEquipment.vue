<script lang="ts" setup>
import { onActivated, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import EquipmentList from '@/components/equipment/EquipmentList.vue';
import EquipmentApiService from '@/services/apiServices/EquipmentApiService';
import HeroesApiService from '@/services/apiServices/HeroesApiService';
import { EquipmentInfo, EquipmentType } from '@/models/types/Equipment';
import { HeroInfo } from '@/models/types/Heroes';

// Все предметы игрока, только просмотр: надевают их в окне прокачки.
// refreshToken меняется, когда из сундука что-то выпало
const props = defineProps<{ refreshToken?: number }>();

const { t } = useI18n();

const items = ref<EquipmentInfo[]>([]);
const heroes = ref<HeroInfo[]>([]);
const filter = ref<EquipmentType | null>(null);
const loading = ref(true);
const errorMessage = ref<string | null>(null);

const load = async () => {
    errorMessage.value = null;
    try {
        [items.value, heroes.value] = await Promise.all([
            EquipmentApiService.getMyEquipment(),
            HeroesApiService.getMyHeroes(),
        ]);
    } catch {
        errorMessage.value = t('inventory.loadError');
    } finally {
        loading.value = false;
    }
};

// Вкладка живёт в KeepAlive, а предметы могли надеть в окне прокачки — перечитываем при каждом показе
onActivated(load);
watch(() => props.refreshToken, load);
</script>

<template>
    <div class="inventory-equipment">
        <div v-if="loading" class="inventory-hint">{{ t('common.loading') }}</div>
        <div v-else-if="errorMessage" class="inventory-hint inventory-hint-error">{{ errorMessage }}</div>
        <div v-else-if="!items.length" class="inventory-hint">{{ t('inventory.noEquipment') }}</div>
        <template v-else>
            <div class="inventory-hint">{{ t('inventory.equipmentHint') }}</div>
            <EquipmentList v-model:filter="filter" :items="items" :heroes="heroes" />
        </template>
    </div>
</template>

<style scoped>
.inventory-equipment {
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
</style>
