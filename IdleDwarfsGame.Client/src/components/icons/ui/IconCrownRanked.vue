<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Сезоны рейтинговой арены: корона со звездой
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Основа уменьшена и сдвинута влево-вниз, справа сверху — значок -->
    <path transform="translate(0 16) scale(0.84)" d="M10 30 L31 54 L50 20 L69 54 L90 30 L84 76 L16 76 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M4 25 A6 6 0 1 0 16 25 A6 6 0 1 0 4 25 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M44 14 A6 6 0 1 0 56 14 A6 6 0 1 0 44 14 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M84 25 A6 6 0 1 0 96 25 A6 6 0 1 0 84 25 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M16 80 L84 80 L84 92 L16 92 Z" />
    <template #cut>
      <path transform="translate(0 16) scale(0.84)" d="M30 86 L30.5 86 M50 86 L50.5 86 M70 86 L70.5 86" stroke-width="5" />
      <path transform="translate(0 16) scale(0.84)" d="M20 66 L80 66" />
      <!-- Место под значок -->
      <path d="M56 20 A24 24 0 1 0 104 20 A24 24 0 1 0 56 20 Z" fill="#000" stroke="none" />
    </template>
    <template #badge>
      <!-- Звезда рейтинга -->
      <path d="M80 1 L84.7 13.5 L98.1 14.1 L87.6 22.5 L91.2 35.4 L80 28 L68.8 35.4 L72.4 22.5 L61.9 14.1 L75.3 13.5 Z" />
    </template>
  </SplitIcon>
</template>
