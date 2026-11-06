<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Скорость добычи: песочные часы
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M16 5 L84 5 L84 14 L16 14 Z" />
    <path d="M16 86 L84 86 L84 95 L16 95 Z" />
    <path d="M25 16 L75 16 C75 34 58 42 56 50 C58 58 75 66 75 84 L25 84 C25 66 42 58 44 50 C42 42 25 34 25 16 Z" />
    <template #cut>
      <!-- Пустое стекло: песок остался внизу верхней колбы и насыпался горкой в нижней -->
      <path d="M32 21 L68 21 C67 26 64 29 61 32 L39 32 C36 29 33 26 32 21 Z" fill="#000" stroke="none" />
      <path d="M48 56 C44 60 37 66 33 72 L67 72 C63 66 56 60 52 56 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
