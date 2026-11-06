<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Сундук героев
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Основа уменьшена и сдвинута влево-вниз, справа сверху — значок -->
    <path transform="translate(0 16) scale(0.84)" d="M10 40 C10 22 27 13 50 13 C73 13 90 22 90 40 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M10 44 L90 44 L90 88 L10 88 Z" />
    <template #cut>
      <path transform="translate(0 16) scale(0.84)" d="M25 16 L25 86 M75 16 L75 86" />
      <path transform="translate(0 16) scale(0.84)" d="M42 38 L58 38 L58 62 L42 62 Z" />
      <path transform="translate(0 16) scale(0.84)" d="M50 47 L50 53" stroke-width="4" />
      <!-- Место под значок -->
      <path d="M56 20 A24 24 0 1 0 104 20 A24 24 0 1 0 56 20 Z" fill="#000" stroke="none" />
    </template>
    <template #badge>
      <!-- Рогатый шлем -->
      <path d="M66 27 C66 14 72 8 80 8 C88 8 94 14 94 27 Z" />
      <path d="M62 27 L98 27 L98 33 L62 33 Z" />
      <path d="M68 17 C62 15 59 10 60 2 C63 7 66 10 71 11 Z" />
      <path d="M92 17 C98 15 101 10 100 2 C97 7 94 10 89 11 Z" />
    </template>
  </SplitIcon>
</template>
