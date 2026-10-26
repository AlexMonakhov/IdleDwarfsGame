<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';import type { ModalTab } from '@/models/types/Modal';
import CollectionHeroes from './CollectionHeroes.vue';
import CollectionGems from './CollectionGems.vue';
import CollectionChests from './CollectionChests.vue';
import CollectionPlaceholder from './CollectionPlaceholder.vue';

const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type CollectionTab = 'heroes' | 'gems' | 'chests' | 'artifacts';

const ICONS: Record<CollectionTab, string> = {
    heroes: '⚔',
    gems: '💎',
    chests: '🎁',
    artifacts: '🏺',
};

const tab = ref<CollectionTab>('heroes');

// Вкладки живут в KeepAlive и сами не перезагружаются: после открытия сундука просим героев и гемы перечитаться
const lootVersion = ref(0);

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
        width="min(860px, calc(94vw - 140px))"
        height="min(640px, 84vh)"
    >
        <!-- KeepAlive — чтобы при переключении вкладок не перезагружать героев -->
        <KeepAlive>
            <CollectionHeroes v-if="tab === 'heroes'" :refresh-token="lootVersion" />
            <CollectionGems v-else-if="tab === 'gems'" :refresh-token="lootVersion" />
            <CollectionChests v-else-if="tab === 'chests'" @opened="lootVersion++" />
            <CollectionPlaceholder
                v-else
                :key="'artifacts'"
                :icon="ICONS.artifacts"
                :title="t('collection.tabs.artifacts')"
                :hint="t('collection.artifactsHint')"
            />
        </KeepAlive>
    </ModalComponent>
</template>
