<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const bgId = `freeze-bg-${useId()}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#ffffff" />
        <stop offset="60%" stop-color="#7dd3fc" />
        <stop offset="100%" stop-color="#0c4a6e" />
      </radialGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#e0f6ff" stroke-width="3" />

    <!-- Ледяная глыба -->
    <polygon points="50,16 74,32 74,64 50,84 26,64 26,32" fill="#e0f6ff" fill-opacity="0.85" stroke="#ffffff" stroke-width="2" />
    <!-- Грани -->
    <path d="M50 16 L50 50 L74 64 M50 50 L26 64" fill="none" stroke="#38bdf8" stroke-width="2" />
  </svg>
</template>
