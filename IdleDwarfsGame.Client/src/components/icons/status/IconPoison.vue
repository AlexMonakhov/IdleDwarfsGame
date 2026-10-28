<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const bgId = `poison-bg-${useId()}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#bef264" />
        <stop offset="65%" stop-color="#4d7c0f" />
        <stop offset="100%" stop-color="#1a2e05" />
      </radialGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#d9f99d" stroke-width="3" />

    <!-- Капля яда -->
    <path d="M50 18 C 62 36, 70 46, 70 58 A 20 20 0 0 1 30 58 C 30 46, 38 36, 50 18 Z" fill="#ecfccb" />
    <!-- Пузырьки -->
    <circle cx="44" cy="58" r="5" fill="#65a30d" />
    <circle cx="56" cy="64" r="3.5" fill="#65a30d" />
    <circle cx="54" cy="50" r="2.5" fill="#65a30d" />
  </svg>
</template>
