<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const bgId = `mark-bg-${useId()}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#f0abfc" />
        <stop offset="60%" stop-color="#86198f" />
        <stop offset="100%" stop-color="#2e0532" />
      </radialGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#f5d0fe" stroke-width="3" />

    <!-- Прицел метки: кольцо, перекрестие и сердцевина -->
    <circle cx="50" cy="50" r="24" fill="none" stroke="#fdf4ff" stroke-width="5" />
    <path d="M50 14 V32 M50 68 V86 M14 50 H32 M68 50 H86" stroke="#fdf4ff" stroke-width="5" stroke-linecap="round" />
    <circle cx="50" cy="50" r="7" fill="#f0abfc" stroke="#fdf4ff" stroke-width="2" />
  </svg>
</template>
