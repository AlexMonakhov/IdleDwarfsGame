<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import type { ModalTab, TabIcon } from '@/models/types/Modal';
import { IconAura, IconDwarf, IconFlame } from '@/components/icons/ui';
import CollectionHeroes from './CollectionHeroes.vue';
import CollectionAuras from './CollectionAuras.vue';
import CollectionEffects from './CollectionEffects.vue';

// Справочник: все герои с их заклинаниями, ауры и эффекты — с превью, как они выглядят в бою
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type CollectionTab = 'heroes' | 'auras' | 'effects';

const ICONS: Record<CollectionTab, TabIcon> = {
    heroes: IconDwarf,
    auras: IconAura,
    effects: IconFlame,
};

const tab = ref<CollectionTab>('heroes');

const tabs = computed<ModalTab[]>(() => (Object.keys(ICONS) as CollectionTab[]).map(key => ({
    key,
    label: t(`collection.tabs.${key}`),
    icon: ICONS[key],
})));

const activeTab = computed({
    get: () => tab.value,
    set: value => { tab.value = value as CollectionTab; },
});
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="t(`collection.tabs.${tab}`)"
        width="min(1280px, calc(calc(94 * var(--vw)) - 140px))"
        height="min(780px, calc(90 * var(--vh)))"
    >
        <!-- Без KeepAlive: скрытая вкладка не должна крутить сцену превью со звуком -->
        <CollectionHeroes v-if="tab === 'heroes'" />
        <CollectionAuras v-else-if="tab === 'auras'" />
        <CollectionEffects v-else />
    </ModalComponent>
</template>
