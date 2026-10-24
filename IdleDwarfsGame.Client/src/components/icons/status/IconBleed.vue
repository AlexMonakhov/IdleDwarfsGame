<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const bgId = `bleed-bg-${useId()}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#ff1a40" />
        <stop offset="65%" stop-color="#800014" />
        <stop offset="100%" stop-color="#2a0007" />
      </radialGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#ff8093" stroke-width="3" />

    <!-- Главная капля -->
    <path d="M 50 20
             C 50 20, 70 50, 70 63
             C 70 74, 61 82, 50 82
             C 39 82, 30 74, 30 63
             C 30 50, 50 20, 50 20 Z" fill="#e60026" />
    <!-- Блик -->
    <path d="M 42 50
             C 40 55, 38 62, 40 68
             C 37 66, 36 60, 38 52 Z" fill="#ffffff" opacity="0.7" />
    <!-- Малая капля -->
    <path d="M 68 28
             C 68 28, 78 43, 78 49
             C 78 54, 73 58, 68 58
             C 63 58, 58 54, 58 49
             C 58 43, 68 28, 68 28 Z" fill="#ff3352" transform="rotate(15 68 43)" />
  </svg>
</template>
