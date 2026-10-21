<script lang="ts" setup>
import GemButton from '@/components/common/GemButton.vue';
import IconButton from '@/components/common/IconButton.vue';
import IconHelm from '@/components/icons/IconHelm.vue';
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import backgroundMusic from "@/assets/sounds/Beneath_The_Marble_Dome.mp3";

const router = useRouter();
const route = useRoute();
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
            <GemButton @click="router.push('/')" :color="'blue'">Home</GemButton>
            <GemButton @click="router.push('about')" :color="'blue'">About</GemButton>
            <GemButton v-if="route.name != 'arena'" @click="router.push('arena')" :color="'blue'">Arena</GemButton>
            <GemButton @click="router.push('town')" :color="'blue'">Town</GemButton>
        </div>
        <div class="settings">
            <i class="button" @click="volumeChange(0.1)">+</i>
            <i class="button" @click="volumeChange(-0.1)">-</i>
            <IconButton @click="mute = !mute"><IconHelm size="60px" ></IconHelm></IconButton>
        </div>
    </div>
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

.nav-menu > *, .settings > * {
  pointer-events: auto;
}
</style>
