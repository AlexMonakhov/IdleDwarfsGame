<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// История боёв: свиток
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Верхний валик, лист, нижний валик -->
    <path d="M18 6 L82 6 C88 6 91 10 91 15 C91 20 88 24 82 24 L18 24 C12 24 9 20 9 15 C9 10 12 6 18 6 Z" />
    <path d="M21 22 L79 22 L79 80 L21 80 Z" />
    <path d="M18 76 L82 76 C88 76 91 80 91 85 C91 90 88 94 82 94 L18 94 C12 94 9 90 9 85 C9 80 12 76 18 76 Z" />
    <template #cut>
      <!-- Строки -->
      <path d="M32 38 L68 38 M32 49 L68 49 M32 60 L58 60" />
      <path d="M18 24 L82 24" />
      <path d="M18 76 L82 76" />
    </template>
  </SplitIcon>
</template>
