<script lang="ts" setup>
import GemButton from '@/components/common/GemButton.vue';
import ExpeditionsModal from '@/components/expeditions/ExpeditionsModal.vue';
import FriendsModal from '@/components/friends/FriendsModal.vue';
import CollectionModal from '@/components/collection/CollectionModal.vue';
import HeroUpgradeModal from '@/components/upgrade/HeroUpgradeModal.vue';
import SquadModal from '@/components/inventory/SquadModal.vue';
import AccountButton from '@/components/account/AccountButton.vue';
import AccountModal from '@/components/account/AccountModal.vue';
import { ref, watch, watchEffect } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import backgroundMusic from "@/assets/sounds/Beneath_The_Marble_Dome.mp3";
import { useAuth } from '@/composables/UseAuth';
import { useResources } from '@/composables/UseResources';
import { usePresence } from '@/composables/UsePresence';
import { soundService } from '@/services/soundService/SoundService';
import { useI18n } from 'vue-i18n';

const { t, locale } = useI18n();
const router = useRouter();
const route = useRoute();
const { isAuthenticated, logout } = useAuth();
const { resources, refresh: refreshResources, clear: clearResources } = useResources();
const { incomingFriendRequests, start: startPresence, stop: stopPresence } = usePresence();

const expeditionsOpen = ref(false);
const friendsOpen = ref(false);
const collectionOpen = ref(false);
const upgradeOpen = ref(false);
const squadOpen = ref(false);
const accountOpen = ref(false);

// Ресурсы аккаунта подтягиваем при входе и сбрасываем при выходе; там же открываем и закрываем сессию вкладки
watch(isAuthenticated, authenticated => {
    if (authenticated) {
        refreshResources();
        startPresence();
    } else {
        clearResources();
        stopPresence();
    }
}, { immediate: true });

const onLogout = () => {
    expeditionsOpen.value = false;
    friendsOpen.value = false;
    collectionOpen.value = false;
    upgradeOpen.value = false;
    squadOpen.value = false;
    accountOpen.value = false;
    // Пока токен ещё есть — им закрывается сессия
    stopPresence();
    logout();
    router.push('/login');
};

// Фоновая музыка: громкость и mute — из настроек звука
const audioRef = ref<HTMLAudioElement | null>(null);

watchEffect(() => {
    const audio = audioRef.value;
    if (!audio) return;
    audio.volume = soundService.musicGain;
    // Браузер мог не дать автозапуск до первого действия — включение музыки в настройках и есть такое действие
    if (soundService.musicGain > 0 && audio.paused) audio.play().catch(() => undefined);
});
</script>

<template>
    <audio ref="audioRef" :src="backgroundMusic" autoplay loop :muted="soundService.settings.musicMuted"></audio>
    <div class="interface-view" >
        <div  class="nav-menu">
            <GemButton @click="router.push('/')" :color="'blue'">{{ t('nav.home') }}</GemButton>
            <GemButton @click="router.push('about')" :color="'blue'">{{ t('nav.about') }}</GemButton>
            <GemButton v-if="route.name != 'arena'" @click="router.push('arena')" :color="'blue'">{{ t('nav.arena') }}</GemButton>
            <GemButton @click="router.push('town')" :color="'blue'">{{ t('nav.town') }}</GemButton>
            <GemButton v-if="isAuthenticated" @click="squadOpen = true" :color="'pink'">{{ t('nav.inventory') }}</GemButton>
            <GemButton v-if="isAuthenticated" @click="collectionOpen = true" :color="'pink'">{{ t('nav.collection') }}</GemButton>
            <GemButton v-if="isAuthenticated" @click="upgradeOpen = true" :color="'green'">{{ t('nav.upgrade') }}</GemButton>
            <GemButton v-if="isAuthenticated" @click="expeditionsOpen = true" :color="'yellow'">{{ t('nav.expeditions') }}</GemButton>
            <GemButton v-if="isAuthenticated" @click="friendsOpen = true" :color="'green'" :badge-count="incomingFriendRequests">{{ t('nav.friends') }}</GemButton>
            <template v-if="!isAuthenticated">
                <GemButton v-if="route.name != 'login'" @click="router.push('/login')" :color="'green'">{{ t('nav.login') }}</GemButton>
                <GemButton v-if="route.name != 'register'" @click="router.push('/register')" :color="'yellow'">{{ t('nav.register') }}</GemButton>
            </template>
            <GemButton v-else @click="onLogout" :color="'red'">{{ t('nav.logout') }}</GemButton>
        </div>
        <div class="settings">
            <div v-if="isAuthenticated && resources" class="resources">
                <span class="resource resource-gold" :title="t('resources.gold')">🪙 {{ resources.gold.toLocaleString(locale) }}</span>
                <span class="resource resource-experience" :title="t('resources.experience')">✦ {{ resources.experience.toLocaleString(locale) }}</span>
            </div>
            <AccountButton @click="accountOpen = true" />
        </div>
    </div>
    <AccountModal v-model="accountOpen" />
    <ExpeditionsModal v-if="isAuthenticated" v-model="expeditionsOpen" />
    <FriendsModal v-if="isAuthenticated" v-model="friendsOpen" />
    <CollectionModal v-if="isAuthenticated" v-model="collectionOpen" />
    <HeroUpgradeModal v-if="isAuthenticated" v-model="upgradeOpen" />
    <SquadModal v-if="isAuthenticated" v-model="squadOpen" />
</template>

<style scoped>
.interface-view {
  width: 100%;
  height: 100%;
  position: fixed;
  z-index: 1;
  pointer-events: none;
  display: flex;
}
.nav-menu {
  width: 50%;
  display: flex;
  justify-content: flex-start;
  align-items: flex-start;
  padding: 20px;
  column-gap: 5px;
}
.settings {
  width: 50%;
  display: flex;
  justify-content: flex-end;
  align-items: flex-start;
  padding: 20px;
}
/* Ресурсы аккаунта: табличка в стиле шапки модалки */
.resources {
  display: flex;
  align-items: center;
  column-gap: 14px;
  height: 40px;
  margin-right: 10px;
  padding: 0 14px;
  box-sizing: border-box;
  border-radius: 10px;
  background: var(--wood-background);
  box-shadow: inset 0 0 6px black, 0 0 0 2px rgb(157, 173, 175), 0 0 0 4px #333;
  font-family: 'Arial', sans-serif;
  font-size: 15px;
  font-weight: bold;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.resource-gold {
  color: #ffd166;
}

.resource-experience {
  color: #a5f3fc;
}

.nav-menu > *, .settings > * {
  pointer-events: auto;
}
</style>
