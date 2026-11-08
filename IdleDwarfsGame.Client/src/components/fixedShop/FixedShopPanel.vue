<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref, type Component } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import FixedOfferCard from './FixedOfferCard.vue';
import FixedShopsApiService from '@/services/apiServices/FixedShopsApiService';
import { useResources } from '@/composables/UseResources';
import { itemName } from '@/components/equipment/equipmentText';
import { formatCountdown } from '@/components/events/eventShared';
import { chestLabel } from '@/i18n';
import type { FixedOfferInfo, FixedShopBuyResult, FixedShopKind, FixedShopOverview } from '@/models/types/FixedShop';

// Лавка с постоянным ассортиментом: кошелёк её валюты, таймер до сброса лимитов и товары.
// Обзор лавки ивента несёт и сам ивент — окно ивента берёт его через событие loaded
const props = defineProps<{
    shop: FixedShopKind;
    currencyIcon: Component;
    currencyLabel: string;
}>();

const emit = defineEmits<{ (e: 'loaded', overview: FixedShopOverview): void }>();

const { t, te, locale } = useI18n();
const { set: setResources } = useResources();

const overview = ref<FixedShopOverview | null>(null);
const loading = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const format = (value: number) => value.toLocaleString(locale.value);

const errorText = (error: unknown, fallback: string) => {
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`fixedShop.errors.${data.code}`)) return t(`fixedShop.errors.${data.code}`);
    return data?.message ?? fallback;
};

/* ===== Время ===== */
// Таймер — по часам сервера: offset — насколько они расходятся с часами браузера
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;

const msLeft = computed(() => (overview.value ? new Date(overview.value.resetAt).getTime() - (now.value + clockOffset) : 0));

/* ===== Загрузка и покупка ===== */
const apply = (next: FixedShopOverview) => {
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
    now.value = Date.now();
    overview.value = next;
    setResources(next.resources);
    emit('loaded', next);
};

const load = async () => {
    loading.value = true;
    try {
        apply(await FixedShopsApiService.get(props.shop));
    } catch (error) {
        message.value = { text: errorText(error, t('fixedShop.loadError')), error: true };
    } finally {
        loading.value = false;
    }
};

const boughtName = (result: FixedShopBuyResult, offer: FixedOfferInfo) => {
    if (result.hero) return result.hero.name;
    if (result.item) return itemName(result.item);
    if (offer.chest) return chestLabel(offer.chest);
    if (result.rainbowCoins) return t('fixedShop.goods.rainbowCoins', { amount: format(result.rainbowCoins) });
    return t('fixedShop.goods.gold', { amount: format(result.reward?.gold ?? 0) });
};

const buy = async (offer: FixedOfferInfo) => {
    if (busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        const result = await FixedShopsApiService.buy(props.shop, offer.key);
        apply(result.shop);
        message.value = { text: t('fixedShop.bought', { name: boughtName(result, offer) }), error: false };
    } catch (error) {
        message.value = { text: errorText(error, t('fixedShop.actionError')), error: true };
        await load();
    } finally {
        busy.value = false;
    }
};

onMounted(() => {
    load();
    ticker = window.setInterval(() => {
        now.value = Date.now();
        // Наступила новая неделя — лимиты сброшены, у лавки ивента — уже другой ивент
        if (overview.value && !loading.value && !busy.value && msLeft.value <= 0) load();
    }, 1000);
});

onBeforeUnmount(() => window.clearInterval(ticker));

defineExpose({ reload: load });
</script>

<template>
    <div class="fixed-shop">
        <div class="fixed-shop-toolbar">
            <span class="fixed-shop-wallet" v-tooltip="currencyLabel">
                <component :is="currencyIcon" /> {{ format(overview?.currency ?? 0) }}
            </span>
            <span v-if="overview" class="fixed-shop-timer" v-tooltip="t(`fixedShop.resetHint.${shop}`)">
                {{ t(`fixedShop.resetIn.${shop}`, { time: formatCountdown(msLeft, t('fixedShop.day')) }) }}
            </span>
        </div>

        <div v-if="message" class="fixed-shop-message" :class="{ 'fixed-shop-message-error': message.error }">{{ message.text }}</div>

        <div v-if="loading && !overview" class="fixed-shop-hint">{{ t('common.loading') }}</div>

        <div v-else-if="overview" class="fixed-shop-grid">
            <FixedOfferCard
                v-for="offer in overview.offers"
                :key="offer.key"
                :offer="offer"
                :currency="overview.currency"
                :currency-icon="currencyIcon"
                :busy="busy"
                @buy="buy(offer)"
            />
        </div>
    </div>
</template>

<style scoped>
.fixed-shop {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 4px 6px 10px;
    font-family: 'Arial', sans-serif;
}

.fixed-shop-toolbar {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px 16px;
}

/* Плашка валюты — как ресурсы в шапке */
.fixed-shop-wallet {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    height: 34px;
    padding: 0 12px;
    box-sizing: border-box;
    border-radius: 10px;
    background: var(--wood-background);
    box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
    color: #fde68a;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
    font-variant-numeric: tabular-nums;
}

.fixed-shop-timer {
    margin-left: auto;
    padding: 3px 10px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.55);
    color: #fde68a;
    font-size: 13px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.fixed-shop-message {
    color: #bbf7d0;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.fixed-shop-message-error {
    color: #ffb4b4;
}

.fixed-shop-hint {
    color: #cbd5e1;
    font-size: 13px;
}

.fixed-shop-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(170px, 1fr));
    gap: 14px;
}
</style>
