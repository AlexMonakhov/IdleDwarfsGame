<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Бой: скрещённые секиры
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path transform="rotate(-35 50 54)" d="M47 12 L53 12 L53 96 L47 96 Z" />
    <path transform="rotate(-35 50 54)" d="M48 14 L34 9 C22 15 15 27 15 40 C24 35 36 35 48 42 Z" />
    <path transform="matrix(-1 0 0 1 100 0) rotate(-35 50 54)" d="M47 12 L53 12 L53 96 L47 96 Z" />
    <path transform="matrix(-1 0 0 1 100 0) rotate(-35 50 54)" d="M48 14 L34 9 C22 15 15 27 15 40 C24 35 36 35 48 42 Z" />
    <template #cut>
      <!-- Кромки верхнего топорища на скрещении -->
      <path transform="matrix(-1 0 0 1 100 0) rotate(-35 50 54)" d="M44.5 44 L44.5 66 M55.5 44 L55.5 66" />
    </template>
  </SplitIcon>
</template>
