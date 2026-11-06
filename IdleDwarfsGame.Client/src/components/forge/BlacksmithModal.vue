<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import ForgeItemsTab from './ForgeItemsTab.vue';
import ForgeGemsTab from './ForgeGemsTab.vue';
import ForgeApiService from '@/services/apiServices/ForgeApiService';
import type { ForgeOverview } from '@/models/types/Forge';
import type { ModalTab, TabIcon } from '@/models/types/Modal';
import { IconAnvil, IconGem } from '@/components/icons/ui';

// Кузница — по образцу алтаря: «Предметы» — переплавка пяти предметов одной редкости в предмет редкостью выше,
// «Камни» — сплав камней в камень ступенью выше
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type ForgeTab = 'items' | 'gems';

const ICONS: Record<ForgeTab, TabIcon> = {
    items: IconAnvil,
    gems: IconGem,
};

const tab = ref<ForgeTab>('items');
const tabs = computed<ModalTab[]>(() => (Object.keys(ICONS) as ForgeTab[]).map(key => ({
    key,
    label: t(`forge.tabs.${key}`),
    icon: ICONS[key],
})));
const activeTab = computed({
    get: () => tab.value,
    set: value => { tab.value = value as ForgeTab; },
});

const forge = ref<ForgeOverview | null>(null);
const loading = ref(false);
const loadError = ref(false);

const load = async () => {
    loading.value = true;
    try {
        forge.value = await ForgeApiService.get();
        loadError.value = false;
    } catch {
        loadError.value = true;
    } finally {
        loading.value = false;
    }
};

// Предметы могли надеть или получить в других окнах — перечитываем при каждом открытии
watch(open, isOpen => {
    if (isOpen) load();
}, { immediate: true });
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="t('forge.header')"
        width="min(1280px, calc(calc(94 * var(--vw)) - 140px))"
        height="min(780px, calc(90 * var(--vh)))"
    >
        <div class="blacksmith">
            <div v-if="loading && !forge" class="blacksmith-hint">{{ t('common.loading') }}</div>
            <div v-else-if="loadError && !forge" class="blacksmith-hint blacksmith-hint-error">{{ t('forge.loadError') }}</div>
            <template v-else-if="forge">
                <ForgeItemsTab v-if="tab === 'items'" :forge="forge" @changed="forge = $event" @stale="load" />
                <ForgeGemsTab v-else :forge="forge" @changed="forge = $event" @stale="load" />
            </template>
        </div>
    </ModalComponent>
</template>

<style scoped>
.blacksmith {
    flex: 1;
    min-width: 0;
    min-height: 0;
    display: flex;
}

.blacksmith-hint {
    margin: 6px;
    color: #cbd5e1;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
}

.blacksmith-hint-error {
    color: #ffb4b4;
}
</style>
