<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Разведка: лупа
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M7 41 A34 34 0 1 0 75 41 A34 34 0 1 0 7 41 Z" />
    <!-- Ручка -->
    <path d="M60 70 L70 60 L94 84 C97 87 97 91 94 94 C91 97 87 97 84 94 Z" />
    <template #cut>
      <path d="M18 41 A23 23 0 1 0 64 41 A23 23 0 1 0 18 41 Z" fill="#000" stroke="none" />
      <!-- Блик на стекле -->
      <path d="M28 38 C29 31 33 27 39 26" />
      <path d="M66 66 L72 72" />
    </template>
  </SplitIcon>
</template>
