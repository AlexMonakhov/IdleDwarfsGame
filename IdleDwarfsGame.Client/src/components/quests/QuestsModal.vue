<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import QuestBoardPanel from './QuestBoardPanel.vue';
import FixedShopPanel from '@/components/fixedShop/FixedShopPanel.vue';
import type { ModalTab, TabIcon } from '@/models/types/Modal';
import { IconElderSeal, IconScroll } from '@/components/icons/ui';

// Ежедневные задания и лавка старейшин, где тратятся печати за них
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type QuestsTab = 'quests' | 'elder';

const ICONS: Record<QuestsTab, TabIcon> = {
    quests: IconScroll,
    elder: IconElderSeal,
};

const tab = ref<QuestsTab>('quests');
const tabs = computed<ModalTab[]>(() => (Object.keys(ICONS) as QuestsTab[]).map(key => ({
    key,
    label: t(`quests.tabs.${key}`),
    icon: ICONS[key],
})));
const activeTab = computed({
    get: () => tab.value,
    set: value => { tab.value = value as QuestsTab; },
});

const header = computed(() => (tab.value === 'elder' ? t('fixedShop.elder.header') : t('quests.header')));
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="header"
        width="min(1100px, calc(calc(94 * var(--vw)) - 140px))"
        height="min(780px, calc(90 * var(--vh)))"
    >
        <div class="quests-modal">
            <QuestBoardPanel v-if="tab === 'quests'" />
            <template v-else>
                <p class="quests-elder-tagline">{{ t('fixedShop.elder.tagline') }}</p>
                <FixedShopPanel shop="Elder" :currency-icon="IconElderSeal" :currency-label="t('resources.elderSeals')" />
            </template>
        </div>
    </ModalComponent>
</template>

<style scoped>
.quests-modal {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
}

.quests-elder-tagline {
    margin: 0 6px 10px;
    color: #fed7aa;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    font-style: italic;
    text-shadow: 1px 1px 0 #000;
}
</style>
