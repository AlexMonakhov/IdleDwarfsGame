<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import GlassButton from '@/components/common/GlassButton.vue';
import type { MailCurrency } from '@/models/types/Mail';
import { CURRENCY_COLORS, CURRENCY_ICONS } from './mailCurrencies';

// Валюта во вложении письма и в каталоге админки — карточка как у камня (lootTile.css): стекло цвета валюты,
// значок, название; amount — сколько (без него — просто вид валюты)
defineProps<{ currency: MailCurrency; amount?: number | null }>();

const { t, locale } = useI18n();
</script>

<template>
    <div class="loot-tile currency-tile" :style="{ '--rarity-color': CURRENCY_COLORS[currency] }">
        <GlassButton tag="div" class="loot-tile-glass">
            <span class="loot-tile-picture"><component :is="CURRENCY_ICONS[currency]" size="52px" /></span>
            <span class="loot-tile-name">{{ t(`mail.currencies.${currency}`) }}</span>
            <span class="loot-tile-rarity">{{ t('mail.currency') }}</span>
        </GlassButton>
        <span v-if="amount" class="loot-tile-count">×{{ amount.toLocaleString(locale) }}</span>
    </div>
</template>

<style scoped src="@/components/inventory/lootTile.css"></style>
