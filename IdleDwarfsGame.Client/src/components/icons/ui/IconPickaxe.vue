<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Шахта: кирка
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Клюв и рукоять -->
    <path d="M5 38 C27 10 73 10 95 38 L90 42 C70 25 30 25 10 42 Z" />
    <path d="M45 14 L55 14 L57 95 L43 95 Z" />
    <template #cut>
      <path d="M41 30 L59 30" />
    </template>
  </SplitIcon>
</template>
