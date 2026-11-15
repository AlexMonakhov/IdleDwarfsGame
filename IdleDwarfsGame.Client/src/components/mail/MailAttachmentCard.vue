<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import HeroCard from '@/components/inventory/HeroCard.vue';
import ItemTile from '@/components/equipment/ItemTile.vue';
import GemTile from '@/components/inventory/GemTile.vue';
import AbilityTile from '@/components/abilities/AbilityTile.vue';
import { IconCheck } from '@/components/icons/ui';
import CurrencyTile from './CurrencyTile.vue';
import type { MailAttachmentInfo } from '@/models/types/Mail';

// Вложение письма — карточкой своего вида (дворф, предмет, камень, способность, валюта).
// claimed — уже забрано: карточка серая, поверх — зелёная галочка
defineProps<{ attachment: MailAttachmentInfo; claimed?: boolean }>();

const { locale } = useI18n();
</script>

<template>
    <div class="mail-attachment" :class="{ 'mail-attachment-claimed': claimed }">
        <div class="mail-attachment-card">
            <HeroCard v-if="attachment.hero" :hero="attachment.hero" selectable />
            <ItemTile v-else-if="attachment.equipment" :item="attachment.equipment" />
            <GemTile v-else-if="attachment.gem" :item="attachment.gem" />
            <AbilityTile v-else-if="attachment.ability" :ability="attachment.ability" />
            <CurrencyTile v-else-if="attachment.currency" :currency="attachment.currency" :amount="attachment.amount" />
        </div>
        <!-- Несколько одинаковых дворфов, предметов или способностей (у камней и валют число на самой карточке) -->
        <span v-if="attachment.amount > 1 && (attachment.hero || attachment.equipment || attachment.ability)" class="mail-attachment-amount">
            ×{{ attachment.amount.toLocaleString(locale) }}
        </span>
        <span v-if="claimed" class="mail-attachment-check"><IconCheck size="44px" /></span>
    </div>
</template>

<style scoped>
.mail-attachment {
    position: relative;
    display: inline-flex;
}

.mail-attachment-card {
    display: inline-flex;
    transition: filter 0.3s;
}

.mail-attachment-claimed .mail-attachment-card {
    filter: grayscale(1) brightness(0.6);
    pointer-events: none;
}

.mail-attachment-amount {
    position: absolute;
    top: -6px;
    right: -6px;
    z-index: 3;
    padding: 2px 7px;
    border-radius: 999px;
    background: #1b2533;
    border: 1px solid rgba(255, 255, 255, 0.4);
    color: #ffd166;
    font: 700 13px 'Arial', sans-serif;
}

.mail-attachment-check {
    position: absolute;
    inset: 0;
    z-index: 4;
    display: grid;
    place-items: center;
    color: #4ade80;
    --split-icon-primary: #4ade80;
    --split-icon-secondary: #bbf7d0;
    filter: drop-shadow(0 0 6px rgba(0, 0, 0, 0.9)) drop-shadow(0 0 10px rgba(74, 222, 128, 0.6));
    pointer-events: none;
}
</style>
