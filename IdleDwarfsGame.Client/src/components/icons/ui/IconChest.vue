<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Сундуки и награды: окованный сундук с замком
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Крышка и короб -->
    <path d="M10 40 C10 22 27 13 50 13 C73 13 90 22 90 40 Z" />
    <path d="M10 44 L90 44 L90 88 L10 88 Z" />
    <template #cut>
      <!-- Оковка -->
      <path d="M25 16 L25 86 M75 16 L75 86" />
      <!-- Накладка замка и скважина -->
      <path d="M42 38 L58 38 L58 62 L42 62 Z" />
      <path d="M50 47 L50 53" stroke-width="4" />
    </template>
  </SplitIcon>
</template>
