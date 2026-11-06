<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Амулет: слот шеи
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M11.7 5 L19.7 23 L24.3 21 L16.3 3 Z" />
    <path d="M20.1 23.6 L34.1 39.6 L37.9 36.4 L23.9 20.4 Z" />
    <path d="M34.5 40 L42.5 46 L45.5 42 L37.5 36 Z" />
    <path d="M19.5 22 A2.5 2.5 0 1 0 24.5 22 A2.5 2.5 0 1 0 19.5 22 Z" />
    <path d="M33.5 38 A2.5 2.5 0 1 0 38.5 38 A2.5 2.5 0 1 0 33.5 38 Z" />
    <path d="M83.7 3 L75.7 21 L80.3 23 L88.3 5 Z" />
    <path d="M76.1 20.4 L62.1 36.4 L65.9 39.6 L79.9 23.6 Z" />
    <path d="M62.5 36 L54.5 42 L57.5 46 L65.5 40 Z" />
    <path d="M75.5 22 A2.5 2.5 0 1 0 80.5 22 A2.5 2.5 0 1 0 75.5 22 Z" />
    <path d="M61.5 38 A2.5 2.5 0 1 0 66.5 38 A2.5 2.5 0 1 0 61.5 38 Z" />
    <!-- Медальон с камнем -->
    <path d="M26 68 A24 24 0 1 0 74 68 A24 24 0 1 0 26 68 Z" />
    <template #cut>
      <path d="M34 68 A16 16 0 1 0 66 68 A16 16 0 1 0 34 68 Z" />
      <path d="M50 58 L58 68 L50 78 L42 68 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
