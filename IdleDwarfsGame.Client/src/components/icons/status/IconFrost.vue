<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const uid = useId();
const bgId = `freeze-bg-${uid}`;
const glowId = `freeze-glow-${uid}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#00f0ff" />
        <stop offset="60%" stop-color="#0077ff" />
        <stop offset="100%" stop-color="#002b66" />
      </radialGradient>
      <filter :id="glowId" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="2.5" result="blur" />
        <feComposite in="SourceGraphic" in2="blur" operator="over" />
      </filter>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#a5f3fc" stroke-width="3" />

    <!-- Снежинка -->
    <g stroke="#ffffff" stroke-width="4" stroke-linecap="round" :filter="`url(#${glowId})`">
      <line x1="50" y1="20" x2="50" y2="80" />
      <line x1="24" y1="35" x2="76" y2="65" />
      <line x1="24" y1="65" x2="76" y2="35" />
      <path d="M 42 28 L 50 20 L 58 28" fill="none" />
      <path d="M 42 72 L 50 80 L 58 72" fill="none" />
      <path d="M 68 33 L 76 35 L 70 45" fill="none" />
      <path d="M 32 67 L 24 65 L 30 55" fill="none" />
      <path d="M 32 33 L 24 35 L 30 45" fill="none" />
      <path d="M 68 67 L 76 65 L 70 55" fill="none" />
    </g>
  </svg>
</template>
