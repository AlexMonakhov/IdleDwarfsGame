<script lang="ts" setup>
import { computed, type Component } from 'vue';
import { useI18n } from 'vue-i18n';
import ItemTile from '@/components/equipment/ItemTile.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import GemButton from '@/components/common/GemButton.vue';
import { IconCoin, IconRainbowCoin } from '@/components/icons/ui';
import { CHEST_ICONS, RARITY_COLORS } from '@/components/inventory/lootStyles';
import { chestLabel } from '@/i18n';
import type { FixedOfferInfo } from '@/models/types/FixedShop';

// Товар лавки с постоянным ассортиментом: карточка (герой, предмет или значок сундука/золота/монет),
// сколько ещё можно купить за период, цена и «Купить»
const props = defineProps<{
    offer: FixedOfferInfo;
    currency: number;
    currencyIcon: Component;
    busy: boolean;
}>();

const emit = defineEmits<{ (e: 'buy'): void }>();

const { t, locale } = useI18n();

const format = (value: number) => value.toLocaleString(locale.value);

const left = computed(() => Math.max(0, props.offer.limit - props.offer.bought));
const soldOut = computed(() => left.value === 0);
const affordable = computed(() => props.currency >= props.offer.price);

// Подпись под значком у товаров без своей карточки
const caption = computed(() => {
    const offer = props.offer;
    if (offer.chest) return chestLabel(offer.chest);
    if (offer.kind === 'Gold') return t('fixedShop.goods.gold', { amount: format(offer.gold) });
    if (offer.kind === 'RainbowCoins') return t('fixedShop.goods.rainbowCoins', { amount: format(offer.rainbowCoins) });
    return '';
});
</script>

<template>
    <div
        class="fixed-offer"
        :class="{ 'fixed-offer-sold-out': soldOut, 'fixed-offer-main': offer.kind === 'Hero' || offer.kind === 'Item' }"
        :style="{ '--rarity-color': RARITY_COLORS[offer.rarity] }"
    >
        <div class="fixed-offer-card">
            <HeroCard v-if="offer.hero" :hero="offer.hero" />
            <ItemTile v-else-if="offer.item" :item="offer.item" />
            <div v-else class="fixed-offer-goods">
                <component :is="CHEST_ICONS[offer.chest.type]" v-if="offer.chest" size="64px" />
                <IconCoin v-else-if="offer.kind === 'Gold'" size="64px" />
                <IconRainbowCoin v-else size="64px" />
                <span class="fixed-offer-caption">{{ caption }}</span>
            </div>
            <span v-if="soldOut" class="fixed-offer-stamp">{{ t('fixedShop.soldOut') }}</span>
        </div>

        <span class="fixed-offer-limit" v-tooltip="t('fixedShop.limitHint')">
            {{ t('fixedShop.left', { left, limit: offer.limit }) }}
        </span>

        <span class="fixed-offer-price" :class="{ 'fixed-offer-short': !affordable && !soldOut }">
            <component :is="currencyIcon" /> {{ format(offer.price) }}
        </span>

        <GemButton
            class="fixed-offer-buy"
            :color="'green'"
            :disabled="soldOut || busy || !affordable"
            v-tooltip="!soldOut && !affordable ? t('fixedShop.notEnough') : undefined"
            @click="emit('buy')"
        >{{ soldOut ? t('fixedShop.soldOut') : t('fixedShop.buy') }}</GemButton>
    </div>
</template>

<style scoped>
.fixed-offer {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 8px;
    padding: 10px 8px;
    border-radius: 14px;
    background: rgba(0, 0, 0, 0.28);
    box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.6), 0 0 0 1px color-mix(in srgb, var(--rarity-color) 35%, transparent);
    font-family: 'Arial', sans-serif;
}

/* Главные награды ивента — герой и предметы — светятся цветом редкости */
.fixed-offer-main {
    box-shadow: inset 0 0 10px rgba(0, 0, 0, 0.6), 0 0 0 2px color-mix(in srgb, var(--rarity-color) 70%, transparent),
        0 0 16px color-mix(in srgb, var(--rarity-color) 40%, transparent);
}

.fixed-offer-sold-out {
    opacity: 0.55;
}

.fixed-offer-card {
    position: relative;
    display: flex;
    justify-content: center;
    min-height: 120px;
}

.fixed-offer-goods {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    row-gap: 8px;
    min-width: 120px;
    padding: 10px;
    filter: drop-shadow(2px 3px 2px rgba(0, 0, 0, 0.8));
}

.fixed-offer-caption {
    max-width: 150px;
    color: #f8fafc;
    font-size: 13px;
    font-weight: bold;
    text-align: center;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.fixed-offer-stamp {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%) rotate(-12deg);
    padding: 4px 10px;
    border: 2px solid #fca5a5;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.7);
    color: #fca5a5;
    font-size: 14px;
    font-weight: bold;
    text-transform: uppercase;
    white-space: nowrap;
}

.fixed-offer-limit {
    color: #cbd5e1;
    font-size: 12px;
}

.fixed-offer-price {
    display: inline-flex;
    align-items: center;
    column-gap: 5px;
    color: #fde68a;
    font-size: 15px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
    font-variant-numeric: tabular-nums;
}

.fixed-offer-short {
    color: #fca5a5;
}

.fixed-offer-buy :deep(.gem-text) {
    font-size: 14px;
}
</style>
