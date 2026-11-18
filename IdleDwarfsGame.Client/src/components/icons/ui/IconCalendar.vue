<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Календарь входов: листок календаря с кольцами и отмеченным днём
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M8 16 L92 16 L92 92 L8 92 Z" />
    <!-- Кольца сверху -->
    <path d="M24 4 L34 4 L34 24 L24 24 Z" />
    <path d="M66 4 L76 4 L76 24 L66 24 Z" />
    <template #cut>
      <!-- Шапка листка и сетка дней -->
      <path d="M10 34 L90 34" />
      <path d="M22 50 L32 50 M45 50 L55 50 M68 50 L78 50 M22 66 L32 66 M45 66 L55 66 M22 82 L32 82 M45 82 L55 82 M68 82 L78 82" />
      <!-- Отмеченный день -->
      <path d="M64 66 L70 72 L82 60" />
    </template>
  </SplitIcon>
</template>
