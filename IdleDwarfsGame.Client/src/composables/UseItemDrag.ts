import { ref } from 'vue';
import { EquipmentInfo } from '@/models/types/Equipment';

// Предмет, который сейчас тащат: слоты подсвечиваются по нему ещё до того, как его над ними отпустят
const dragged = ref<EquipmentInfo | null>(null);
// Тащат из слота на герое, а не из списка: брошенный на список такой предмет снимается
const fromSlot = ref(false);

export function useItemDrag() {
    const start = (event: DragEvent, item: EquipmentInfo, isFromSlot: boolean) => {
        dragged.value = item;
        fromSlot.value = isFromSlot;
        if (event.dataTransfer) {
            event.dataTransfer.effectAllowed = 'move';
            // Без данных Firefox не начинает перетаскивание
            event.dataTransfer.setData('text/plain', item.id);
        }
    };

    const end = () => {
        dragged.value = null;
        fromSlot.value = false;
    };

    return { dragged, fromSlot, start, end };
}
