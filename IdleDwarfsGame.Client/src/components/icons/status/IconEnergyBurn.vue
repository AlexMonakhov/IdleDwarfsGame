<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const bgId = `energy-burn-bg-${useId()}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#a855f7" />
        <stop offset="65%" stop-color="#4c1d95" />
        <stop offset="100%" stop-color="#1e0538" />
      </radialGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#e9d5ff" stroke-width="3" />

    <!-- Молния энергии, расколотая тьмой -->
    <path d="M56 14 L28 56 H46 L40 86 L72 40 H54 Z" fill="#fde68a" stroke="#f59e0b" stroke-width="2" stroke-linejoin="round" />
    <path d="M30 34 L50 48 L44 52 L70 70" fill="none" stroke="#1e0538" stroke-width="6" stroke-linecap="round" stroke-linejoin="round" />
  </svg>
</template>
