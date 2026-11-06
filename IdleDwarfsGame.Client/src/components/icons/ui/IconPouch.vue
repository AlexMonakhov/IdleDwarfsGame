<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Лавка: кошель с монетами
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Завязанный верх, шнурок и мешок -->
    <path d="M30 6 L40 15 L50 6 L60 15 L70 6 L67 26 L33 26 Z" />
    <path d="M35 29 L65 29 L65 36 L35 36 Z" />
    <path d="M32 38 C15 50 11 72 19 85 C27 96 73 96 81 85 C89 72 85 50 68 38 Z" />
    <template #cut>
      <!-- Монета на мешке -->
      <path d="M37 66 A13 13 0 1 0 63 66 A13 13 0 1 0 37 66 Z" />
      <path d="M50 59 L50 73" />
    </template>
  </SplitIcon>
</template>
