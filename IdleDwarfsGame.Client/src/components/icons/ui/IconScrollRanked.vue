<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// История рейтинговой арены: свиток со звездой
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Основа уменьшена и сдвинута влево-вниз, справа сверху — значок -->
    <path transform="translate(0 16) scale(0.84)" d="M18 6 L82 6 C88 6 91 10 91 15 C91 20 88 24 82 24 L18 24 C12 24 9 20 9 15 C9 10 12 6 18 6 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M21 22 L79 22 L79 80 L21 80 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M18 76 L82 76 C88 76 91 80 91 85 C91 90 88 94 82 94 L18 94 C12 94 9 90 9 85 C9 80 12 76 18 76 Z" />
    <template #cut>
      <path transform="translate(0 16) scale(0.84)" d="M32 38 L68 38 M32 49 L68 49 M32 60 L58 60" />
      <path transform="translate(0 16) scale(0.84)" d="M18 24 L82 24" />
      <path transform="translate(0 16) scale(0.84)" d="M18 76 L82 76" />
      <!-- Место под значок -->
      <path d="M56 20 A24 24 0 1 0 104 20 A24 24 0 1 0 56 20 Z" fill="#000" stroke="none" />
    </template>
    <template #badge>
      <!-- Звезда рейтинга -->
      <path d="M80 1 L84.7 13.5 L98.1 14.1 L87.6 22.5 L91.2 35.4 L80 28 L68.8 35.4 L72.4 22.5 L61.9 14.1 L75.3 13.5 Z" />
    </template>
  </SplitIcon>
</template>
