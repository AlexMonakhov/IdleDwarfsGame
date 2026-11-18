<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Кнопка «Ещё»: три самоцвета в ряд — раскрывает остальные разделы
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M20 36 L32 50 L20 64 L8 50 Z" />
    <path d="M50 36 L62 50 L50 64 L38 50 Z" />
    <path d="M80 36 L92 50 L80 64 L68 50 Z" />
  </SplitIcon>
</template>
