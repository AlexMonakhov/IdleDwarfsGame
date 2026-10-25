<script lang="ts" setup>
import GemButton from '@/components/common/GemButton.vue';
import IconButton from '@/components/common/IconButton.vue';
import IconHelm from '@/components/icons/IconHelm.vue';
import ExpeditionsModal from '@/components/expeditions/ExpeditionsModal.vue';
import { onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import backgroundMusic from "@/assets/sounds/Beneath_The_Marble_Dome.mp3";
import { useAuth } from '@/composables/UseAuth';
import { useResources } from '@/composables/UseResources';
import { useI18n } from 'vue-i18n';
import { toggleLocale } from '@/i18n';

const { t, locale } = useI18n();
const router = useRouter();
const route = useRoute();
const { isAuthenticated, logout } = useAuth();
const { resources, refresh: refreshResources, clear: clearResources } = useResources();

const expeditionsOpen = ref(false);

// Ресурсы аккаунта подтягиваем при входе и сбрасываем при выходе
watch(isAuthenticated, authenticated => {
    if (authenticated) refreshResources();
    else clearResources();
}, { immediate: true });

const onLogout = () => {
    expeditionsOpen.value = false;
    logout();
    router.push('/login');
};
const mute  = ref<boolean>(false);
const audioRef = ref<HTMLAudioElement | null>(null);

const volumeChange = (delta: number) => {
    // 2. Access the element using .value
    const audio = audioRef.value;
    
    // 3. Add a null check to ensure the element is mounted
    if (audio) {
        // Use Math.round to avoid floating point math errors (e.g., 0.30000000000000004)
        const newVolume = Math.min(1, Math.max(0, audio.volume + delta));
        audio.volume = parseFloat(newVolume.toFixed(2));
    }
}

onMounted(() => {
    if (audioRef.value) {
        audioRef.value.volume = 0.01; // Set initial volume to 50%
    }
});
</script>

<template>
    <audio ref="audioRef" :src="backgroundMusic" autoplay loop :muted="mute"></audio>
    <div class="interface-view" >
        <div  class="nav-menu">
            <GemButton @click="router.push('/')" :color="'blue'">{{ t('nav.home') }}</GemButton>
            <GemButton @click="router.push('about')" :color="'blue'">{{ t('nav.about') }}</GemButton>
            <GemButton v-if="route.name != 'arena'" @click="router.push('arena')" :color="'blue'">{{ t('nav.arena') }}</GemButton>
            <GemButton @click="router.push('town')" :color="'blue'">{{ t('nav.town') }}</GemButton>
            <GemButton v-if="isAuthenticated && route.name != 'inventory'" @click="router.push('/inventory')" :color="'pink'">{{ t('nav.inventory') }}</GemButton>
            <GemButton v-if="isAuthenticated" @click="expeditionsOpen = true" :color="'yellow'">{{ t('nav.expeditions') }}</GemButton>
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
            <GemButton :title="t('nav.language')" :color="'yellow'" round @click="toggleLocale">{{ locale.toUpperCase() }}</GemButton>
            <i class="button" @click="volumeChange(0.1)">+</i>
            <i class="button" @click="volumeChange(-0.1)">-</i>
            <IconButton @click="mute = !mute"><IconHelm size="60px" ></IconHelm></IconButton>
        </div>
    </div>
    <ExpeditionsModal v-if="isAuthenticated" v-model="expeditionsOpen" />
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
.button {
  height: 40px;
  width: 40px;
  background: red;
  text-align: center;
  margin: 5px;
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
