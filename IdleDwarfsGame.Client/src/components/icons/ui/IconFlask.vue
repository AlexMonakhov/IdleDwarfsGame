<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Извлечение способностей: алхимическая колба с зельем
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Горлышко с венчиком и круглое дно -->
    <path d="M37 6 L63 6 L63 14 L58 14 L58 42 L42 42 L42 14 L37 14 Z" />
    <path d="M21 66 A29 29 0 1 0 79 66 A29 29 0 1 0 21 66 Z" />
    <template #cut>
      <!-- Поверхность зелья и пузырьки -->
      <path d="M23 62 C33 55 41 69 50 62 C59 55 67 69 77 62" />
      <path d="M43 77 L43.5 77" stroke-width="5" />
      <path d="M57 83 L57.5 83" stroke-width="4" />
      <path d="M55 72 L55.5 72" stroke-width="3" />
      <path d="M40 20 L60 20" />
    </template>
  </SplitIcon>
</template>
