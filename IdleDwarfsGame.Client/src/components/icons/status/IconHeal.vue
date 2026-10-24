<script lang="ts" setup>
import { useId } from 'vue';

withDefaults(defineProps<{ size?: string }>(), { size: '16px' });

const uid = useId();
const bgId = `heal-bg-${uid}`;
const glowId = `heal-glow-${uid}`;
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <radialGradient :id="bgId" cx="50%" cy="50%" r="50%">
        <stop offset="0%" stop-color="#00ff88" />
        <stop offset="65%" stop-color="#008044" />
        <stop offset="100%" stop-color="#002b17" />
      </radialGradient>
      <filter :id="glowId" x="-20%" y="-20%" width="140%" height="140%">
        <feGaussianBlur stdDeviation="2" result="blur" />
        <feComposite in="SourceGraphic" in2="blur" operator="over" />
      </filter>
    </defs>

    <circle cx="50" cy="50" r="45" :fill="`url(#${bgId})`" stroke="#b3ffd9" stroke-width="3" />

    <!-- Крест -->
    <g fill="#ffffff" :filter="`url(#${glowId})`">
      <rect x="41" y="23" width="18" height="54" rx="4" />
      <rect x="23" y="41" width="54" height="18" rx="4" />
    </g>

    <!-- Искорки -->
    <circle cx="28" cy="28" r="3" fill="#e6fffa" />
    <circle cx="72" cy="30" r="2" fill="#e6fffa" />
    <circle cx="74" cy="70" r="3.5" fill="#e6fffa" />
    <circle cx="26" cy="68" r="2" fill="#e6fffa" />
  </svg>
</template>
