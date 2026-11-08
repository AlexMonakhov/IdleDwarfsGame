<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Печать старейшин — валюта лавки старейшин (ежедневные задания): сургучная печать с руной на двух лентах
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :primary-color="props.primaryColor ?? '#fecaca'" :secondary-color="props.secondaryColor ?? '#c2410c'">
    <!-- Ленты -->
    <path d="M30 66 L42 66 L36 98 L28 90 L20 96 Z" />
    <path d="M70 66 L58 66 L64 98 L72 90 L80 96 Z" />
    <!-- Сургуч: круг с наплывами по краю -->
    <path d="M50 6 L58 11 L67 9 L72 17 L81 19 L82 28 L89 34 L86 43 L90 51 L84 58 L84 67 L75 70 L70 78 L61 76 L53 82 L45 77 L36 79 L31 71 L22 68 L22 59 L15 52 L19 44 L15 35 L22 29 L23 20 L32 17 L37 9 L46 11 Z" />
    <template #cut>
      <!-- Оттиск: кольцо и руна «одал» -->
      <path d="M28 44 A24 24 0 1 0 76 44 A24 24 0 1 0 28 44 Z" />
      <path d="M52 28 L62 40 L52 52 L42 40 Z M46 47 L38 60 M58 47 L66 60" />
    </template>
  </SplitIcon>
</template>
