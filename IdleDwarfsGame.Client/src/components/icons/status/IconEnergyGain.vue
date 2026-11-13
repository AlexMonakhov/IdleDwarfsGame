<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const bgId = `energy-gain-bg-${useId()}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#7dd3fc" />
        <stop offset="65%" stop-color="#0369a1" />
        <stop offset="100%" stop-color="#082f49" />
      </radialGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#e0f2fe" stroke-width="3" />

    <!-- Молния энергии и плюс: энергия вернулась -->
    <path d="M50 14 L24 56 H42 L36 86 L66 42 H48 Z" fill="#fde68a" stroke="#f59e0b" stroke-width="2" stroke-linejoin="round" />
    <path d="M74 22 V42 M64 32 H84" stroke="#ecfeff" stroke-width="6" stroke-linecap="round" />
  </svg>
</template>
