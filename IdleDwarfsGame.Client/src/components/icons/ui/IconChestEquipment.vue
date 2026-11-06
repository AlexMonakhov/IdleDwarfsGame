<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Сундук снаряжения
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
      <!-- Меч -->
      <path transform="rotate(45 80 20)" d="M80 -2 L85 5 L85 26 L75 26 L75 5 Z" />
      <path transform="rotate(45 80 20)" d="M68 26 L92 26 L92 31 L68 31 Z" />
      <path transform="rotate(45 80 20)" d="M77.5 31 L82.5 31 L82.5 40 L77.5 40 Z" />
    </template>
  </SplitIcon>
</template>
