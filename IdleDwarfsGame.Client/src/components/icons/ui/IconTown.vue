<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Кнопка «Город»: крепостная стена с центральной башней
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Боковые башенки с зубцами, стена и высокая башня со шпилем -->
    <path d="M6 92 L6 34 L12 34 L12 40 L18 40 L18 34 L24 34 L24 40 L30 40 L30 34 L36 34 L36 52 L40 52 L40 26 L50 8 L60 26 L60 52 L64 52 L64 34 L70 34 L70 40 L76 40 L76 34 L82 34 L82 40 L88 40 L88 34 L94 34 L94 92 Z" />
    <template #cut>
      <!-- Ворота и бойница башни -->
      <path d="M40 92 L40 74 Q50 62 60 74 L60 92" />
      <path d="M50 32 L50 42" />
    </template>
  </SplitIcon>
</template>
