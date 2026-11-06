<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Способности: рунный камень
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M30 8 L70 8 C78 8 84 14 84 22 L80 84 C80 90 74 94 68 94 L32 94 C26 94 20 90 20 84 L16 22 C16 14 22 8 30 8 Z" />
    <template #cut>
      <!-- Руна «райдо» -->
      <path d="M38 27 L38 77 M38 27 L61 42 L38 57 M45 54 L63 77" stroke-width="5" />
    </template>
  </SplitIcon>
</template>
