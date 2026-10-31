<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useItemDrag } from '@/composables/UseItemDrag';
import { HeroInfo } from '@/models/types/Heroes';
import { EquipmentInfo, EquipmentSlot, EquipmentType, SLOT_TYPE, canWear } from '@/models/types/Equipment';
import { EQUIPMENT_ICONS } from '@/components/inventory/lootStyles';
import ItemTile from './ItemTile.vue';

// Слоты снаряжения поверх сцены с героем. Предмет кладут перетаскиванием;
// клик по занятому слоту снимает предмет, по пустому — открывает подходящие предметы
const props = defineProps<{ hero: HeroInfo }>();

const emit = defineEmits<{
    (e: 'equip', item: EquipmentInfo, slot: EquipmentSlot): void;
    (e: 'unequip', item: EquipmentInfo): void;
    (e: 'pick-type', type: EquipmentType): void;
}>();

const { t } = useI18n();
const { dragged, end } = useItemDrag();

const COLUMNS: EquipmentSlot[][] = [
    ['Helmet', 'Neck', 'Weapon'],
    ['Ring1', 'Ring2'],
];

const hoverSlot = ref<EquipmentSlot | null>(null);

const worn = computed(() => {
    const bySlot = new Map<EquipmentSlot, EquipmentInfo>();
    (props.hero.equipment ?? []).forEach(item => {
        if (item.slot) bySlot.set(item.slot, item);
    });
    return bySlot;
});

const fits = (item: EquipmentInfo, slot: EquipmentSlot) =>
    item.type === SLOT_TYPE[slot] && canWear(item, props.hero.element);

const accepts = (slot: EquipmentSlot) => !!dragged.value && fits(dragged.value, slot);

const onDragOver = (event: DragEvent, slot: EquipmentSlot) => {
    if (!accepts(slot)) return;
    event.preventDefault();
    hoverSlot.value = slot;
};

const onDrop = (slot: EquipmentSlot) => {
    const item = dragged.value;
    hoverSlot.value = null;
    // Плитка может исчезнуть вместе с переездом предмета, и тогда dragend не придёт
    end();
    if (item && fits(item, slot)) emit('equip', item, slot);
};

const onClick = (slot: EquipmentSlot) => {
    const item = worn.value.get(slot);
    if (item) emit('unequip', item);
    else emit('pick-type', SLOT_TYPE[slot]);
};
</script>

<template>
    <div v-for="(column, index) in COLUMNS" :key="index" class="gear-column" :class="`gear-column-${index}`">
        <div
            v-for="slot in column"
            :key="slot"
            class="gear-slot"
            :class="{
                'gear-slot-accepts': accepts(slot),
                'gear-slot-rejects': dragged && !accepts(slot),
                'gear-slot-hover': hoverSlot === slot,
            }"
            :title="worn.get(slot) ? t('equipment.clickToRemove') : t(`equipment.slots.${slot}`)"
            @dragover="onDragOver($event, slot)"
            @dragleave="hoverSlot = null"
            @drop.prevent="onDrop(slot)"
            @click="onClick(slot)"
        >
            <ItemTile v-if="worn.get(slot)" :item="worn.get(slot)!" compact draggable />
            <span v-else class="gear-slot-empty">{{ EQUIPMENT_ICONS[SLOT_TYPE[slot]] }}</span>
        </div>
    </div>
</template>

<style scoped>
.gear-column {
    position: absolute;
    /* Сверху — имя и теги героя */
    top: 92px;
    z-index: 3;
    display: flex;
    flex-direction: column;
    row-gap: 8px;
}

.gear-column-0 {
    left: 10px;
}

.gear-column-1 {
    right: 10px;
}

.gear-slot {
    width: 54px;
    height: 54px;
    box-sizing: border-box;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid rgba(157, 173, 175, 0.55);
    border-radius: 10px;
    background: rgba(10, 14, 24, 0.72);
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.8);
    cursor: pointer;
    transition: border-color 0.15s ease, box-shadow 0.15s ease, opacity 0.15s ease;
}

.gear-slot:hover {
    border-color: #ffd166;
}

.gear-slot-empty {
    font-size: 24px;
    opacity: 0.35;
    filter: grayscale(1);
}

/* Тащат подходящий предмет */
.gear-slot-accepts {
    border-color: #86efac;
    box-shadow: inset 0 0 8px rgba(0, 0, 0, 0.8), 0 0 10px rgba(134, 239, 172, 0.7);
}

.gear-slot-hover {
    background: rgba(34, 80, 50, 0.85);
}

.gear-slot-rejects {
    opacity: 0.5;
}
</style>
