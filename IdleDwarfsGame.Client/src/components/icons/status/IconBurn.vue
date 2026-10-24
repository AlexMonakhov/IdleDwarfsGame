<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

// id внутри svg должны быть уникальны, иначе несколько иконок на странице делят один градиент
const uid = useId();
const bgId = `burn-bg-${uid}`;
const innerId = `flame-inner-${uid}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#ff9900" />
        <stop offset="65%" stop-color="#cc0000" />
        <stop offset="100%" stop-color="#4a0000" />
      </radialGradient>
      <linearGradient :id="innerId" x1="0%" y1="100%" x2="0%" y2="0%">
        <stop offset="0%" stop-color="#ffe600" />
        <stop offset="100%" stop-color="#ffffff" />
      </linearGradient>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#ffcc00" stroke-width="3" />

    <g>
      <path d="M 50 18
               C 55 30, 65 35, 72 48
               C 80 62, 73 78, 58 82
               C 55 83, 45 83, 42 82
               C 27 78, 20 62, 28 48
               C 33 39, 36 38, 38 30
               C 40 25, 48 20, 50 18 Z" fill="#ff5500" />
      <path d="M 50 35
               C 53 43, 60 47, 63 56
               C 67 66, 61 76, 50 78
               C 39 76, 33 66, 37 56
               C 40 50, 47 42, 50 35 Z" :fill="`url(#${innerId})`" />
    </g>
  </svg>
</template>
