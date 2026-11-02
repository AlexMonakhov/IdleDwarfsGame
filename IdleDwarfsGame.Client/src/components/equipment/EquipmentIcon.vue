<script lang="ts" setup>
import IconHelm from '@/components/icons/IconHelm.vue';
import { EQUIPMENT_ICONS } from '@/components/inventory/lootStyles';
import { EquipmentType } from '@/models/types/Equipment';

// Значок вида предмета под размер текущего шрифта. Шлем — проба своей графики, остальные пока эмодзи
defineProps<{ type: EquipmentType }>();
</script>

<template>
    <IconHelm
        v-if="type === 'Helmet'"
        class="equipment-icon"
        size="1.2em"
        primary-color="rgb(217 215 178)"
        secondary-color="rgb(64 255 251)"
    />
    <template v-else>{{ EQUIPMENT_ICONS[type] }}</template>
</template>

<style scoped>
.equipment-icon {
    vertical-align: -0.2em;
}
</style>
