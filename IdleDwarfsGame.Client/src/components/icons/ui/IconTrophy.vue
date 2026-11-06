<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Сезоны и рекорды: кубок
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Чаша с ручками -->
    <path d="M24 8 L76 8 L76 30 C76 48 64 58 50 58 C36 58 24 48 24 30 Z" />
    <path d="M25 15 L14 15 C8 15 5 21 5 28 C5 40 15 47 27 47 L27 40 C18 40 12 35 12 28 C12 24 13 22 16 22 L25 22 Z" />
    <path d="M75 15 L86 15 C92 15 95 21 95 28 C95 40 85 47 73 47 L73 40 C82 40 88 35 88 28 C88 24 87 22 84 22 L75 22 Z" />
    <!-- Ножка и подставка -->
    <path d="M44 56 L56 56 L56 74 L44 74 Z" />
    <path d="M30 76 L70 76 L74 92 L26 92 Z" />
    <template #cut>
      <path d="M50 21 L52.6 28.4 L60.5 28.6 L54.3 33.4 L56.5 40.9 L50 36.5 L43.5 40.9 L45.7 33.4 L39.5 28.6 L47.4 28.4 Z" />
      <path d="M30 84 L70 84" />
    </template>
  </SplitIcon>
</template>
