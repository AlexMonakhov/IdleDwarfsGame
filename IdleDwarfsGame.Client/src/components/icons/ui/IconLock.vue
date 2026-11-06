<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Закрыто: навесной замок
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Дужка и корпус -->
    <path d="M27 48 L27 32 C27 17 37 7 50 7 C63 7 73 17 73 32 L73 48 L63 48 L63 32 C63 23 58 17 50 17 C42 17 37 23 37 32 L37 48 Z" />
    <path d="M20 44 L80 44 C84 44 86 46 86 50 L86 88 C86 92 84 94 80 94 L20 94 C16 94 14 92 14 88 L14 50 C14 46 16 44 20 44 Z" />
    <template #cut>
      <!-- Скважина -->
      <path d="M43 63 A7 7 0 1 0 57 63 A7 7 0 1 0 43 63 Z M46 66 L54 66 L55.5 81 L44.5 81 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
