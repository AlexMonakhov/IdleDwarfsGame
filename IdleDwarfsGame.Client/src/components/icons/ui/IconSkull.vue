<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Боссы и гибель: рогатый череп
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Рога -->
    <path d="M20 38 C9 33 4 20 8 4 C14 17 21 22 30 24 Z" />
    <path d="M80 38 C91 33 96 20 92 4 C86 17 79 22 70 24 Z" />
    <!-- Череп с челюстью -->
    <path d="M50 12 C72 12 86 28 86 48 C86 60 80 68 72 72 L72 90 L28 90 L28 72 C20 68 14 60 14 48 C14 28 28 12 50 12 Z" />
    <template #cut>
      <!-- Глазницы, нос и зубы -->
      <path d="M27 50 A9 8 0 1 0 45 50 A9 8 0 1 0 27 50 Z" fill="#000" stroke="none" />
      <path d="M55 50 A9 8 0 1 0 73 50 A9 8 0 1 0 55 50 Z" fill="#000" stroke="none" />
      <path d="M50 61 L45 70 L55 70 Z" fill="#000" stroke="none" />
      <path d="M28 76 L72 76" />
      <path d="M40 76 L40 90 M50 76 L50 90 M60 76 L60 90" />
    </template>
  </SplitIcon>
</template>
