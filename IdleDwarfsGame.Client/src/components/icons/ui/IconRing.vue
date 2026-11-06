<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Кольцо: слот кольца
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Камень и ободок -->
    <path d="M38 8 L62 8 L72 20 L50 38 L28 20 Z" />
    <path d="M19 64 A31 31 0 1 0 81 64 A31 31 0 1 0 19 64 Z" />
    <template #cut>
      <path d="M29 64 A21 21 0 1 0 71 64 A21 21 0 1 0 29 64 Z" fill="#000" stroke="none" />
      <path d="M28 20 L72 20" />
      <path d="M28 38 L72 38" />
    </template>
  </SplitIcon>
</template>
