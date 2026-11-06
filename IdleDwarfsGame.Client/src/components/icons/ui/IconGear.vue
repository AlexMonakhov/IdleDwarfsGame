<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Настройки: шестерёнка
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M42.3 16.9 L44.3 4.4 L55.7 4.4 L57.7 16.9 L67.9 21.1 L78.3 13.7 L86.3 21.7 L78.9 32.1 L83.1 42.3 L95.6 44.3 L95.6 55.7 L83.1 57.7 L78.9 67.9 L86.3 78.3 L78.3 86.3 L67.9 78.9 L57.7 83.1 L55.7 95.6 L44.3 95.6 L42.3 83.1 L32.1 78.9 L21.7 86.3 L13.7 78.3 L21.1 67.9 L16.9 57.7 L4.4 55.7 L4.4 44.3 L16.9 42.3 L21.1 32.1 L13.7 21.7 L21.7 13.7 L32.1 21.1 Z" />
    <path d="M15 50 A35 35 0 1 0 85 50 A35 35 0 1 0 15 50 Z" />
    <template #cut>
      <path d="M37 50 A13 13 0 1 0 63 50 A13 13 0 1 0 37 50 Z" fill="#000" stroke="none" />
      <path d="M26 50 A24 24 0 1 0 74 50 A24 24 0 1 0 26 50 Z" />
    </template>
  </SplitIcon>
</template>
