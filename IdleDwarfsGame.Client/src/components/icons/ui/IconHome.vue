<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Кнопка «Главная»: дом с двускатной крышей и дверью
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M50 8 L94 46 L82 46 L82 92 L18 92 L18 46 L6 46 Z" />
    <template #cut>
      <!-- Дверь и окно-ромб под коньком -->
      <path d="M41 92 L41 66 Q50 58 59 66 L59 92" />
      <path d="M50 30 L57 37 L50 44 L43 37 Z" />
    </template>
  </SplitIcon>
</template>
