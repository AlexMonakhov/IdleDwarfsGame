<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { AxiosError } from 'axios';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import ShopOfferCard from './ShopOfferCard.vue';
import ShopApiService from '@/services/apiServices/ShopApiService';
import { useResources } from '@/composables/UseResources';
import { itemName } from '@/components/equipment/equipmentText';
import { IconBloodCoin, IconCoin, IconHonorBadge, IconRainbowCoin, IconRefresh } from '@/components/icons/ui';
import type { ShopKind, ShopOfferInfo, ShopOfferKind, ShopOverview } from '@/models/types/Shop';

// Полка одного магазина: кошелёк, таймер до ежедневного обновления, ручное обновление и товары.
// sections — какие разделы показать: на рынке вкладки «Предметы» и «Герои» делят одну полку,
// подпольный рынок и лавка гильдии показывают оба раздела сразу
const props = withDefaults(defineProps<{
    shop: ShopKind;
    sections: ShopOfferKind[];
    // underground — оформление подпольного рынка
    theme?: 'default' | 'underground';
}>(), {
    theme: 'default',
});

const { t, te, locale } = useI18n();
const { set: setResources } = useResources();

const overview = ref<ShopOverview | null>(null);
const loading = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const format = (value: number) => value.toLocaleString(locale.value);

const errorText = (error: unknown, fallback: string) => {
    const data = (error as AxiosError<{ code?: string; message?: string }>).response?.data;
    if (data?.code && te(`shop.errors.${data.code}`)) return t(`shop.errors.${data.code}`);
    return data?.message ?? fallback;
};

/* ===== Время ===== */
// Таймер — по часам сервера: offset — насколько они расходятся с часами браузера
const now = ref(Date.now());
let clockOffset = 0;
let ticker = 0;

const msLeft = computed(() => (overview.value ? new Date(overview.value.refreshAt).getTime() - (now.value + clockOffset) : 0));

const formatTime = (ms: number) => {
    const total = Math.max(0, Math.ceil(ms / 1000));
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${Math.floor(total / 3600)}:${pad(Math.floor((total % 3600) / 60))}:${pad(total % 60)}`;
};

/* ===== Загрузка и действия ===== */
const apply = (next: ShopOverview) => {
    clockOffset = new Date(next.serverTime).getTime() - Date.now();
    now.value = Date.now();
    overview.value = next;
    setResources(next.resources);
};

const load = async () => {
    loading.value = true;
    try {
        apply(await ShopApiService.get(props.shop));
    } catch (error) {
        message.value = { text: errorText(error, t('shop.loadError')), error: true };
    } finally {
        loading.value = false;
    }
};

const run = async (action: () => Promise<void>) => {
    if (busy.value) return;
    busy.value = true;
    message.value = null;
    try {
        await action();
    } catch (error) {
        message.value = { text: errorText(error, t('shop.actionError')), error: true };
        // Полку могли изменить в другой вкладке — показываем актуальную
        await load();
    } finally {
        busy.value = false;
    }
};

const reroll = () => run(async () => {
    apply(await ShopApiService.reroll(props.shop));
    message.value = { text: t('shop.rerolled'), error: false };
});

const toggleLock = (offer: ShopOfferInfo) => run(async () => {
    apply(await ShopApiService.setLocked(offer.id, !offer.locked));
});

const buy = (offer: ShopOfferInfo) => run(async () => {
    const result = await ShopApiService.buy(offer.id);
    apply(result.shop);
    const name = result.hero?.name ?? (result.item ? itemName(result.item) : '');
    message.value = { text: t('shop.bought', { name }), error: false };
});

onMounted(() => {
    load();
    ticker = window.setInterval(() => {
        now.value = Date.now();
        // Наступили новые сутки — полка на сервере уже свежая
        if (overview.value && !loading.value && !busy.value && msLeft.value <= 0) load();
    }, 1000);
});

onBeforeUnmount(() => window.clearInterval(ticker));

// Тот же компонент может переключиться на другой магазин — перечитываем
watch(() => props.shop, () => {
    overview.value = null;
    message.value = null;
    load();
});

/* ===== Вид ===== */
const currency = computed(() => {
    if (props.shop === 'Pvp') return { icon: IconBloodCoin, value: overview.value?.wallet.bloodCoins ?? 0, label: t('shop.currency.bloodCoins') };
    if (props.shop === 'Guild') return { icon: IconHonorBadge, value: overview.value?.wallet.honorBadges ?? 0, label: t('shop.currency.honorBadges') };
    return { icon: IconCoin, value: overview.value?.wallet.gold ?? 0, label: t('resources.gold') };
});

const offersOf = (kind: ShopOfferKind) => (kind === 'Item' ? overview.value?.items : overview.value?.heroes) ?? [];

const canReroll = computed(() => !!overview.value && (overview.value.freeReroll || overview.value.wallet.rainbowCoins >= overview.value.rerollCost));
</script>

<template>
    <div class="shop-panel" :class="`shop-panel-${theme}`">
        <!-- ===== Шапка: кошелёк, таймер, обновление ===== -->
        <div class="shop-toolbar">
            <div class="shop-wallet">
                <span class="shop-wallet-chip" v-tooltip="currency.label">
                    <component :is="currency.icon" /> {{ format(currency.value) }}
                </span>
                <span class="shop-wallet-chip shop-wallet-rainbow" v-tooltip="t('shop.currency.rainbowCoins')">
                    <IconRainbowCoin /> {{ format(overview?.wallet.rainbowCoins ?? 0) }}
                </span>
            </div>

            <span v-if="overview" class="shop-timer" v-tooltip="t('shop.refreshHint')">
                {{ t('shop.refreshIn', { time: formatTime(msLeft) }) }}
            </span>

            <GemButton
                class="shop-reroll"
                :color="theme === 'underground' ? 'red' : 'blue'"
                :disabled="!canReroll || busy || loading"
                v-tooltip="t('shop.rerollHint', { cost: overview?.rerollCost ?? 0 })"
                @click="reroll"
            >
                <IconRefresh /> {{ t('shop.reroll') }}
                <span class="shop-reroll-cost">
                    <template v-if="overview?.freeReroll">{{ t('shop.free') }}</template>
                    <template v-else><IconRainbowCoin /> {{ overview?.rerollCost }}</template>
                </span>
            </GemButton>
        </div>

        <div v-if="message" class="shop-message" :class="{ 'shop-message-error': message.error }">{{ message.text }}</div>

        <div v-if="loading && !overview" class="shop-hint">{{ t('common.loading') }}</div>

        <!-- ===== Разделы ===== -->
        <template v-else-if="overview">
            <section v-for="kind in sections" :key="kind" class="shop-section">
                <h4 v-if="sections.length > 1" class="shop-section-title">{{ t(`shop.sections.${kind}`) }}</h4>
                <div class="shop-grid">
                    <ShopOfferCard
                        v-for="offer in offersOf(kind)"
                        :key="offer.id"
                        :offer="offer"
                        :shop="shop"
                        :wallet="overview.wallet"
                        :busy="busy"
                        @buy="buy(offer)"
                        @toggle-lock="toggleLock(offer)"
                    />
                </div>
            </section>
        </template>
    </div>
</template>

<style scoped>
.shop-panel {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    row-gap: 10px;
    padding: 4px 6px 10px;
    font-family: 'Arial', sans-serif;
}

.shop-toolbar {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px 16px;
}

.shop-wallet {
    display: flex;
    column-gap: 10px;
}

/* Плашки валют — как ресурсы в шапке */
.shop-wallet-chip {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    height: 34px;
    padding: 0 12px;
    box-sizing: border-box;
    border-radius: 10px;
    background: var(--wood-background);
    box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
    color: #ffd166;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
    font-variant-numeric: tabular-nums;
}

.shop-wallet-rainbow {
    color: #f5d0fe;
}

.shop-timer {
    margin-left: auto;
    padding: 3px 10px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.55);
    color: #fde68a;
    font-size: 13px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.shop-reroll :deep(.gem-text) {
    display: inline-flex;
    align-items: center;
    column-gap: 6px;
    font-size: 14px;
    white-space: nowrap;
}

.shop-reroll-cost {
    display: inline-flex;
    align-items: center;
    column-gap: 3px;
    padding: 0 6px;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.3);
    font-size: 12px;
}

.shop-message {
    color: #bbf7d0;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.shop-message-error {
    color: #ffb4b4;
}

.shop-hint {
    color: #cbd5e1;
    font-size: 13px;
}

.shop-section-title {
    margin: 4px 0 8px;
    color: #f8fafc;
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

/* Четыре товара в ряд на широком окне, на узком — сколько влезет */
.shop-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(170px, 1fr));
    gap: 14px;
}

/* ===== Подпольный рынок: тёмный подвал в кровавых тонах ===== */
.shop-panel-underground {
    border-radius: 14px;
    padding: 12px 14px 14px;
    background:
        radial-gradient(ellipse 70% 40% at 50% 0%, rgba(160, 16, 32, 0.35), transparent 70%),
        radial-gradient(ellipse 60% 50% at 50% 110%, rgba(90, 0, 12, 0.5), transparent 70%),
        repeating-linear-gradient(135deg, rgba(255, 255, 255, 0.015) 0 6px, transparent 6px 14px),
        rgba(12, 6, 8, 0.82);
    box-shadow: inset 0 0 24px rgba(0, 0, 0, 0.9), 0 0 0 1px rgba(190, 30, 45, 0.45);
}

.shop-panel-underground .shop-wallet-chip {
    background: linear-gradient(to bottom, #3a1418, #1e0a0d);
    box-shadow: inset 0 0 6px black, 0 0 0 2px #7f1d1d, 0 0 0 4px #1a0507;
    color: #fca5a5;
}

.shop-panel-underground .shop-wallet-rainbow {
    color: #f5d0fe;
}

.shop-panel-underground .shop-section-title {
    color: #fca5a5;
    text-shadow: 0 0 8px rgba(220, 38, 38, 0.7), 1px 1px 0 #000;
}

.shop-panel-underground .shop-timer {
    background: rgba(40, 4, 8, 0.75);
    color: #fca5a5;
}

.shop-panel-underground :deep(.shop-offer) {
    background: rgba(30, 4, 8, 0.55);
}
</style>
