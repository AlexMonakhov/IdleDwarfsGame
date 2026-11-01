<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { HeroInfo } from '@/models/types/Heroes';
import { EQUIPMENT_TYPES, EquipmentInfo, EquipmentType } from '@/models/types/Equipment';
import { EQUIPMENT_ICONS } from '@/components/inventory/lootStyles';
import ItemTile from './ItemTile.vue';

// Предметы игрока с фильтром по виду. Без выбранного героя — только просмотр
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

const visible = computed(() => props.items.filter(item => !props.filter || item.type === props.filter));

const ownerName = (item: EquipmentInfo) =>
    item.heroId ? props.heroes.find(hero => hero.id === item.heroId)?.name ?? null : null;
</script>

<template>
    <div class="equipment-list">
        <div class="equipment-filters">
            <button
                class="equipment-filter"
                :class="{ 'equipment-filter-active': !filter }"
                @click="emit('update:filter', null)"
            >{{ t('equipment.all') }}</button>
            <button
                v-for="type in EQUIPMENT_TYPES"
                :key="type"
                class="equipment-filter"
                :class="{ 'equipment-filter-active': filter === type }"
                :title="t(`equipment.types.${type}`)"
                @click="emit('update:filter', type)"
            >{{ EQUIPMENT_ICONS[type] }} {{ t(`equipment.types.${type}`) }}</button>
        </div>

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

.equipment-filters {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 6px;
}

.equipment-filter {
    padding: 4px 10px;
    border: 1px solid rgba(157, 173, 175, 0.5);
    border-radius: 8px;
    background: rgba(14, 19, 34, 0.7);
    color: #cbd5e1;
    font-size: 12px;
    cursor: pointer;
}

.equipment-filter:hover {
    color: #f8fafc;
    border-color: rgba(255, 209, 102, 0.7);
}

.equipment-filter-active {
    color: #ffd166;
    border-color: #ffd166;
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
