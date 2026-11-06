<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Атака: двусторонний боевой топор
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Топорище с остриём сверху и навершием снизу -->
    <path d="M50 3 L55.5 13 L54.5 84 L45.5 84 L44.5 13 Z" />
    <path d="M43.5 86 L56.5 86 L56.5 91 L50 97 L43.5 91 Z" />
    <!-- Лезвия -->
    <path d="M48 26 C37 24 29 17 22 8 C10 18 5 28 5 37 C5 46 10 55 22 65 C29 56 37 49 48 47 Z" />
    <path d="M52 26 C63 24 71 17 78 8 C90 18 95 28 95 37 C95 46 90 55 78 65 C71 56 63 49 52 47 Z" />
    <template #cut>
      <!-- Заточка вдоль кромок -->
      <path d="M21 18 C15 25 12.5 31 12.5 37 C12.5 43 15 49 21 56" />
      <path d="M79 18 C85 25 87.5 31 87.5 37 C87.5 43 85 49 79 56" />
      <!-- Обмотка рукояти -->
      <path d="M42 63 L58 63" />
      <path d="M42 71 L58 71" />
    </template>
  </SplitIcon>
</template>
