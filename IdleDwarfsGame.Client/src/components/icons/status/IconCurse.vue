<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const bgId = `curse-bg-${useId()}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#c084fc" />
        <stop offset="65%" stop-color="#581c87" />
        <stop offset="100%" stop-color="#1e0538" />
      </radialGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#e9d5ff" stroke-width="3" />

    <!-- Глаз порчи -->
    <path d="M18 50 Q 50 22, 82 50 Q 50 78, 18 50 Z" fill="#f3e8ff" />
    <circle cx="50" cy="50" r="12" fill="#7e22ce" />
    <ellipse cx="50" cy="50" rx="3.5" ry="10" fill="#1e0538" />
  </svg>
</template>
