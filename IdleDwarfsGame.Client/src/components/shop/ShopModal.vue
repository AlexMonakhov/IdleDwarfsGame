<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import ModalComponent from '@/components/common/ModalComponent.vue';
import ShopPanel from './ShopPanel.vue';
import type { ModalTab, TabIcon } from '@/models/types/Modal';
import { IconBloodCoin, IconDwarf, IconShield } from '@/components/icons/ui';

// Рынок в городе. «Предметы» и «Герои» — одна полка рынка (золото и камни), иконки вкладок — как на алтаре.
// «Подпольный рынок» — полка за кровавые монеты с арены
const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ (e: 'update:modelValue', value: boolean): void }>();

const { t } = useI18n();

const open = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

type ShopTab = 'items' | 'heroes' | 'underground';

const ICONS: Record<ShopTab, TabIcon> = {
    items: IconShield,
    heroes: IconDwarf,
    underground: IconBloodCoin,
};

const tab = ref<ShopTab>('items');
const tabs = computed<ModalTab[]>(() => (Object.keys(ICONS) as ShopTab[]).map(key => ({
    key,
    label: t(`shop.tabs.${key}`),
    icon: ICONS[key],
})));
const activeTab = computed({
    get: () => tab.value,
    set: value => { tab.value = value as ShopTab; },
});

const header = computed(() => (tab.value === 'underground' ? t('shop.underground.header') : t('shop.header')));
</script>

<template>
    <ModalComponent
        v-model="open"
        v-model:active-tab="activeTab"
        :tabs="tabs"
        :header-message="header"
        width="min(1280px, calc(calc(94 * var(--vw)) - 140px))"
        height="min(820px, calc(90 * var(--vh)))"
    >
        <div class="shop-modal">
            <!-- Одна полка рынка на обе вкладки: переключение не перечитывает её -->
            <ShopPanel v-if="tab !== 'underground'" shop="Market" :sections="[tab === 'items' ? 'Item' : 'Hero']" />
            <template v-else>
                <p class="shop-underground-tagline">{{ t('shop.underground.tagline') }}</p>
                <ShopPanel shop="Pvp" :sections="['Item', 'Hero']" theme="underground" />
            </template>
        </div>
    </ModalComponent>
</template>

<style scoped>
.shop-modal {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
}

.shop-underground-tagline {
    margin: 0 6px 10px;
    color: #fca5a5;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    font-style: italic;
    text-shadow: 0 0 6px rgba(220, 38, 38, 0.5), 1px 1px 0 #000;
}
</style>
