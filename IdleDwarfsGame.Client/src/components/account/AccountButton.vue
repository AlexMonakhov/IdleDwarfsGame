<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import AvatarFrame from './AvatarFrame.vue';
import { useAccount } from '@/composables/UseAccount';

const { t } = useI18n();
const { user, level, selectedFrame } = useAccount();
</script>

<template>
    <button type="button" class="account-button" :title="user ? user.username : t('account.settingsTitle')">
        <AvatarFrame :effect="selectedFrame" :size="124">
            <span v-if="user" class="account-level" :title="t('account.level', { level })">{{ level }}</span>
            <span v-else class="account-level account-level-guest">⚙</span>
        </AvatarFrame>
    </button>
</template>

<style scoped>
.account-button {
    padding: 0;
    border: none;
    background: none;
    border-radius: 50%;
    cursor: pointer;
    transition: transform 0.1s ease, filter 0.2s ease;
}

.account-button:hover {
    filter: brightness(1.15);
}

.account-button:active {
    transform: scale(0.95);
}

/* Значок уровня — табличка на нижнем краю рамки */
.account-level {
    position: absolute;
    left: 50%;
    bottom: -10px;
    z-index: 3;
    transform: translateX(-50%);
    min-width: 30px;
    height: 24px;
    padding: 0 7px;
    box-sizing: border-box;
    border-radius: 12px;
    background: var(--wood-background);
    box-shadow: inset 0 0 4px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 3px #333, 0 2px 4px rgba(0, 0, 0, 0.6);
    color: #ffd166;
    font: bold 14px/24px 'Arial', sans-serif;
    text-align: center;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.account-level-guest {
    color: #cbd5e1;
}
</style>
