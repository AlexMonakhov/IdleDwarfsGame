<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Скорость: сапог с крылом
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Сапог носком вправо: голенище с отворотом, подъём, закруглённый носок -->
    <path d="M37 9 L67 9 L65 20 L64 50 C72 55 85 58 91 66 C95 71 95 79 90 82 L37 82 C34 72 36 60 38 50 L37 20 Z" />
    <!-- Подошва -->
    <path d="M34 86 L91 86 C93 86 94 88 94 89 C94 92 92 93 89 93 L34 93 Z" />
    <!-- Крыло у голенища -->
    <path d="M39 22 C29 12 16 7 2 9 C10 17 21 24 37 33 Z" />
    <path d="M37.5 34 C27 27 14 25 4 28 C12 35 23 39 37 43 Z" />
    <path d="M37 45 C27 42 17 43 8 48 C16 52 26 53 36.5 53 Z" />
    <template #cut>
      <!-- Отворот голенища и носок -->
      <path d="M34 21 L68 21" />
      <path d="M78 62 C83 67 84 75 81 82" />
    </template>
  </SplitIcon>
</template>
