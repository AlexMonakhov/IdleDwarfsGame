<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Соперники рейтинговой арены: скрещённые секиры со звездой
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Основа уменьшена и сдвинута влево-вниз, справа сверху — значок -->
    <path transform="translate(0 16) scale(0.84) rotate(-35 50 54)" d="M47 12 L53 12 L53 96 L47 96 Z" />
    <path transform="translate(0 16) scale(0.84) rotate(-35 50 54)" d="M48 14 L34 9 C22 15 15 27 15 40 C24 35 36 35 48 42 Z" />
    <path transform="translate(0 16) scale(0.84) matrix(-1 0 0 1 100 0) rotate(-35 50 54)" d="M47 12 L53 12 L53 96 L47 96 Z" />
    <path transform="translate(0 16) scale(0.84) matrix(-1 0 0 1 100 0) rotate(-35 50 54)" d="M48 14 L34 9 C22 15 15 27 15 40 C24 35 36 35 48 42 Z" />
    <template #cut>
      <path transform="translate(0 16) scale(0.84) matrix(-1 0 0 1 100 0) rotate(-35 50 54)" d="M44.5 44 L44.5 66 M55.5 44 L55.5 66" />
      <!-- Место под значок -->
      <path d="M56 20 A24 24 0 1 0 104 20 A24 24 0 1 0 56 20 Z" fill="#000" stroke="none" />
    </template>
    <template #badge>
      <!-- Звезда рейтинга -->
      <path d="M80 1 L84.7 13.5 L98.1 14.1 L87.6 22.5 L91.2 35.4 L80 28 L68.8 35.4 L72.4 22.5 L61.9 14.1 L75.3 13.5 Z" />
    </template>
  </SplitIcon>
</template>
