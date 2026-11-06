<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Ауры: сияющее кольцо
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M30 50 A20 20 0 1 0 70 50 A20 20 0 1 0 30 50 Z" />
    <path d="M46.6 28.3 L50 3 L53.4 28.3 Z" />
    <path d="M62.9 32.2 L83.2 16.8 L67.8 37.1 Z" />
    <path d="M71.7 46.6 L97 50 L71.7 53.4 Z" />
    <path d="M67.8 62.9 L83.2 83.2 L62.9 67.8 Z" />
    <path d="M53.4 71.7 L50 97 L46.6 71.7 Z" />
    <path d="M37.1 67.8 L16.8 83.2 L32.2 62.9 Z" />
    <path d="M28.3 53.4 L3 50 L28.3 46.6 Z" />
    <path d="M32.2 37.1 L16.8 16.8 L37.1 32.2 Z" />
    <path d="M55.9 28.8 L63.8 16.7 L60.8 30.9 Z" />
    <path d="M69.1 39.2 L83.3 36.2 L71.2 44.1 Z" />
    <path d="M71.2 55.9 L83.3 63.8 L69.1 60.8 Z" />
    <path d="M60.8 69.1 L63.8 83.3 L55.9 71.2 Z" />
    <path d="M44.1 71.2 L36.2 83.3 L39.2 69.1 Z" />
    <path d="M30.9 60.8 L16.7 63.8 L28.8 55.9 Z" />
    <path d="M28.8 44.1 L16.7 36.2 L30.9 39.2 Z" />
    <path d="M39.2 30.9 L36.2 16.7 L44.1 28.8 Z" />
    <template #cut>
      <path d="M40 50 A10 10 0 1 0 60 50 A10 10 0 1 0 40 50 Z" />
    </template>
  </SplitIcon>
</template>
