<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import ItemTile from '@/components/equipment/ItemTile.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import GemButton from '@/components/common/GemButton.vue';
import GemIcon from '@/components/icons/GemIcon.vue';
import { IconBloodCoin, IconCoin, IconHonorBadge, IconLock } from '@/components/icons/ui';
import { RARITY_COLORS } from '@/components/inventory/lootStyles';
import type { ShopKind, ShopOfferInfo, ShopWallet } from '@/models/types/Shop';

// Товар на полке: карточка как в инвентаре (предмет) или как у героя (дворф), под ней цена, «Купить» и замок
const props = defineProps<{
    offer: ShopOfferInfo;
    shop: ShopKind;
    wallet: ShopWallet;
    busy: boolean;
}>();

const emit = defineEmits<{
    (e: 'buy'): void;
    (e: 'toggle-lock'): void;
}>();

const { t, locale } = useI18n();

const format = (value: number) => value.toLocaleString(locale.value);

const currencyIcon = computed(() => (props.shop === 'Pvp' ? IconBloodCoin : IconHonorBadge));
const currencyOwned = computed(() => (props.shop === 'Pvp' ? props.wallet.bloodCoins : props.wallet.honorBadges));

const gemsOwned = computed(() => props.wallet.gems
    .filter(g => g.gem === props.offer.price.gem && g.rarity === props.offer.price.gemRarity)
    .reduce((sum, g) => sum + g.quantity, 0));

// Чего хватает — каждая часть цены красится отдельно, кнопка доступна, только если хватает всего
const enoughGold = computed(() => props.wallet.gold >= props.offer.price.gold);
const enoughCurrency = computed(() => currencyOwned.value >= props.offer.price.currency);
const enoughGems = computed(() => !props.offer.price.gem || gemsOwned.value >= props.offer.price.gems);
const affordable = computed(() => enoughGold.value && enoughCurrency.value && enoughGems.value);

const gemHint = computed(() => props.offer.price.gem
    ? t('shop.gemPrice', {
        rarity: t(`rarity.${props.offer.price.gemRarity}`),
        gem: t(`gems.${props.offer.price.gem}`),
        owned: format(gemsOwned.value),
    })
    : undefined);
</script>

<template>
    <div
        class="shop-offer"
        :class="{ 'shop-offer-sold': offer.sold, 'shop-offer-locked': offer.locked }"
        :style="{ '--rarity-color': RARITY_COLORS[offer.rarity] }"
    >
        <div class="shop-offer-card">
            <ItemTile v-if="offer.item" :item="offer.item" />
            <HeroCard v-else-if="offer.hero" :hero="offer.hero" />
            <span v-if="offer.sold" class="shop-offer-stamp">{{ t('shop.sold') }}</span>
        </div>

        <div class="shop-offer-price">
            <span v-if="offer.price.gold" class="shop-price-part" :class="{ 'shop-price-short': !enoughGold }">
                <IconCoin /> {{ format(offer.price.gold) }}
            </span>
            <span v-if="offer.price.currency" class="shop-price-part" :class="{ 'shop-price-short': !enoughCurrency }">
                <component :is="currencyIcon" /> {{ format(offer.price.currency) }}
            </span>
            <span
                v-if="offer.price.gem && offer.price.gems"
                class="shop-price-part"
                :class="{ 'shop-price-short': !enoughGems }"
                :style="{ '--gem-rarity': RARITY_COLORS[offer.price.gemRarity] }"
                v-tooltip="gemHint"
            >
                <GemIcon :gem="offer.price.gem" /> <span class="shop-price-gem-count">×{{ format(offer.price.gems) }}</span>
            </span>
        </div>

        <div class="shop-offer-actions">
            <GemButton
                class="shop-offer-buy"
                :color="'green'"
                :disabled="offer.sold || busy || !affordable"
                v-tooltip="!offer.sold && !affordable ? t('shop.notEnough') : undefined"
                @click="emit('buy')"
            >{{ offer.sold ? t('shop.sold') : t('shop.buy') }}</GemButton>
            <!-- Закреплённый товар не трогают обновления полки; закрепить можно один предмет и одного дворфа -->
            <button
                type="button"
                class="shop-offer-lock"
                :class="{ 'shop-offer-lock-on': offer.locked }"
                :disabled="offer.sold || busy"
                :aria-pressed="offer.locked"
                v-tooltip="offer.locked ? t('shop.unlockHint') : t('shop.lockHint')"
                @click="emit('toggle-lock')"
            >
                <IconLock size="18px" />
            </button>
        </div>
    </div>
</template>

<style scoped>
.shop-offer {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 8px;
    padding: 10px 8px 10px;
    border-radius: 14px;
    background: rgba(0, 0, 0, 0.28);
    box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.6), 0 0 0 1px color-mix(in srgb, var(--rarity-color) 35%, transparent);
    font-family: 'Arial', sans-serif;
    transition: box-shadow 0.2s ease;
}

/* Закреплённый товар — золотая кайма */
.shop-offer-locked {
    box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.6), 0 0 0 2px #ffd166, 0 0 12px 2px rgba(255, 209, 102, 0.45);
}

.shop-offer-card {
    position: relative;
    display: flex;
    justify-content: center;
    min-height: 150px;
    align-items: center;
}

.shop-offer-sold .shop-offer-card > :not(.shop-offer-stamp) {
    filter: grayscale(0.85) brightness(0.6);
}

/* Штамп «Продано» поперёк карточки */
.shop-offer-stamp {
    position: absolute;
    top: 50%;
    left: 50%;
    z-index: 5;
    transform: translate(-50%, -50%) rotate(-14deg);
    padding: 3px 12px;
    border: 3px solid #f87171;
    border-radius: 8px;
    background: rgba(20, 6, 8, 0.75);
    color: #fca5a5;
    font-size: 15px;
    font-weight: 900;
    letter-spacing: 2px;
    text-transform: uppercase;
    text-shadow: 1px 1px 0 #000;
    pointer-events: none;
}

.shop-offer-price {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 2px 10px;
    min-height: 20px;
    color: #ffd166;
    font-size: 14px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.85);
    font-variant-numeric: tabular-nums;
}

.shop-price-part {
    display: inline-flex;
    align-items: center;
    column-gap: 4px;
    white-space: nowrap;
}

/* Камни: число — в цвете ступени камня (обычные, необычные, редкие) */
.shop-price-gem-count {
    color: color-mix(in srgb, var(--gem-rarity) 70%, white);
}

/* Не хватает — красным */
.shop-price-short,
.shop-price-short .shop-price-gem-count {
    color: #f87171;
}

.shop-offer-actions {
    display: flex;
    align-items: center;
    column-gap: 6px;
}

.shop-offer-buy :deep(.gem-text) {
    font-size: 13px;
}

.shop-offer-lock {
    width: 34px;
    height: 34px;
    display: inline-flex;
    justify-content: center;
    align-items: center;
    padding: 0;
    border: 1px solid rgba(148, 163, 184, 0.6);
    border-radius: 50%;
    background: rgba(15, 23, 42, 0.7);
    --split-icon-primary: #94a3b8;
    --split-icon-secondary: #64748b;
    cursor: pointer;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.shop-offer-lock:hover:not(:disabled) {
    border-color: #ffd166;
}

.shop-offer-lock-on {
    border-color: #ffd166;
    box-shadow: 0 0 8px 1px rgba(255, 209, 102, 0.6);
    --split-icon-primary: #ffe7a3;
    --split-icon-secondary: #f2b33d;
}

.shop-offer-lock:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}
</style>
