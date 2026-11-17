<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Кнопка «Друзья»: два дворфа плечом к плечу
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Задний дворф справа, передний слева -->
    <path d="M64 18 A13 13 0 1 1 63.9 18 Z" />
    <path d="M58 58 C62 52 66 50 70 50 C84 50 94 60 94 86 L66 86 C66 72 63 63 58 58 Z" />
    <path d="M36 14 A16 16 0 1 1 35.9 14 Z" />
    <path d="M6 90 C6 64 20 52 36 52 C52 52 66 64 66 90 Z" />
    <template #cut>
      <!-- Граница переднего силуэта и борода -->
      <path d="M26 46 Q36 58 46 46" />
      <path d="M60 50 C55 56 52 60 50 64" />
    </template>
  </SplitIcon>
</template>
