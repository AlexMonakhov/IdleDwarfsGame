<script lang="ts" setup>
import { EQUIPMENT_ICONS } from '@/components/inventory/lootStyles';
import { EquipmentType } from '@/models/types/Equipment';

// Значок вида предмета под размер текущего шрифта. Цвета заданы явно: у шлема (IconHelm) свои серые по умолчанию
defineProps<{ type: EquipmentType }>();
</script>

<template>
    <component
        :is="EQUIPMENT_ICONS[type]"
        class="equipment-icon"
        size="1.2em"
        primary-color="rgb(217 215 178)"
        secondary-color="rgb(64 255 251)"
    />
</template>

<style scoped>
.equipment-icon {
    vertical-align: -0.2em;
}
</style>
