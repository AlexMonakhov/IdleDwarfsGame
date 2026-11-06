<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Энергия (максимум): гранёный кристалл
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M50 3 L77 30 L69 86 L50 97 L31 86 L23 30 Z" />
    <template #cut>
      <!-- Грани -->
      <path d="M23 30 L39 38 L61 38 L77 30" />
      <path d="M39 38 L50 5" />
      <path d="M61 38 L50 5" />
      <path d="M39 38 L46 88" />
      <path d="M61 38 L54 88" />
    </template>
  </SplitIcon>
</template>
