<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { HeroInfo } from '@/models/types/Heroes';
import { EquipmentInfo, EquipmentType } from '@/models/types/Equipment';
import type { Rarity } from '@/models/types/Inventory';
import ItemFilters from './ItemFilters.vue';
import ItemTile from './ItemTile.vue';

// Предметы игрока с фильтрами по виду и редкости. Без выбранного героя — только просмотр.
// Вид — снаружи (v-model:filter): его выбирает и клик по слоту героя; редкость — здесь
const props = defineProps<{
    items: EquipmentInfo[];
    // Все герои игрока: по ним подписываем, на ком надет предмет
    heroes: HeroInfo[];
    hero?: HeroInfo | null;
    draggable?: boolean;
    filter: EquipmentType | null;
}>();

const emit = defineEmits<{
    (e: 'pick', item: EquipmentInfo): void;
    (e: 'update:filter', value: EquipmentType | null): void;
}>();

const { t } = useI18n();

const rarity = ref<Rarity | null>(null);

const visible = computed(() => props.items
    .filter(item => (!props.filter || item.type === props.filter) && (!rarity.value || item.rarity === rarity.value)));

const ownerName = (item: EquipmentInfo) =>
    item.heroId ? props.heroes.find(hero => hero.id === item.heroId)?.name ?? null : null;
</script>

<template>
    <div class="equipment-list">
        <ItemFilters v-model:rarity="rarity" :type="filter" :items="items" @update:type="emit('update:filter', $event)" />

        <div v-if="!visible.length" class="equipment-empty">{{ t('equipment.empty') }}</div>
        <div v-else class="equipment-grid">
            <ItemTile
                v-for="item in visible"
                :key="item.id"
                :item="item"
                :hero="hero"
                :owner-name="ownerName(item)"
                :draggable="draggable"
                @click="emit('pick', item)"
            />
        </div>
    </div>
</template>

<style scoped>
.equipment-list {
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    font-family: 'Arial', sans-serif;
}

.equipment-empty {
    text-align: center;
    color: #cbd5e1;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.equipment-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: stretch;
    gap: 10px;
    padding: 4px;
}
</style>
