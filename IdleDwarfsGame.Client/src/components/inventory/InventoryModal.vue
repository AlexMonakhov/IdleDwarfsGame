<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';import type { ModalTab } from '@/models/types/Modal';
import InventoryHeroes from './InventoryHeroes.vue';
import InventoryGems from './InventoryGems.vue';
import InventoryChests from './InventoryChests.vue';
import InventoryEquipment from './InventoryEquipment.vue';

const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type InventoryTab = 'heroes' | 'equipment' | 'gems' | 'chests';

const ICONS: Record<InventoryTab, string> = {
    heroes: '⚔',
    equipment: '🛡',
    gems: '💎',
    chests: '🎁',
};

const tab = ref<InventoryTab>('heroes');

// Вкладки живут в KeepAlive и сами не перезагружаются: после открытия сундука просим героев и гемы перечитаться
const lootVersion = ref(0);

const tabs = computed<ModalTab[]>(() => (Object.keys(ICONS) as InventoryTab[]).map(key => ({
    key,
    label: t(`inventory.tabs.${key}`),
    icon: ICONS[key],
})));

const activeTab = computed({
    get: () => tab.value,
    set: value => { tab.value = value as InventoryTab; },
});
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="t(`inventory.tabs.${tab}`)"
        width="min(860px, calc(94vw - 140px))"
        height="min(640px, 84vh)"
    >
        <!-- KeepAlive — чтобы при переключении вкладок не перезагружать героев -->
        <KeepAlive>
            <InventoryHeroes v-if="tab === 'heroes'" :refresh-token="lootVersion" />
            <InventoryEquipment v-else-if="tab === 'equipment'" :refresh-token="lootVersion" />
            <InventoryGems v-else-if="tab === 'gems'" :refresh-token="lootVersion" />
            <InventoryChests v-else @opened="lootVersion++" />
        </KeepAlive>
    </ModalComponent>
</template>
