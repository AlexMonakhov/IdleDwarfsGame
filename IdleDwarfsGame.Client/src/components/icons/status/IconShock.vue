<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const bgId = `shock-bg-${useId()}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#9be7ff" />
        <stop offset="65%" stop-color="#1d4ed8" />
        <stop offset="100%" stop-color="#0b1a4a" />
      </radialGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#e0fbff" stroke-width="3" />

    <!-- Молния -->
    <polygon points="56,14 30,54 47,54 40,86 70,42 53,42 62,14" fill="#ffffff" stroke="#fff3a3" stroke-width="2" stroke-linejoin="round" />
  </svg>
</template>
