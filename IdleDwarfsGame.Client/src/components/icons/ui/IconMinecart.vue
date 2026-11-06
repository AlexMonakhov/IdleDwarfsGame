<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Выход добычи: вагонетка с рудой
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Руда горкой, кузов, колёса -->
    <path d="M16 40 C16 28 28 23 35 29 C40 18 56 17 61 27 C68 21 82 26 84 40 Z" />
    <path d="M8 44 L92 44 L82 78 L18 78 Z" />
    <path d="M23 86 A8 8 0 1 0 39 86 A8 8 0 1 0 23 86 Z" />
    <path d="M61 86 A8 8 0 1 0 77 86 A8 8 0 1 0 61 86 Z" />
    <template #cut>
      <path d="M20 58 L80 58" />
      <path d="M36 30 L40 38 M60 28 L56 36" />
    </template>
  </SplitIcon>
</template>
