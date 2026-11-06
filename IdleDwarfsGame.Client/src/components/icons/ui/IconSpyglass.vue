<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Профессия разведчика: подзорная труба
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M14.2 94.2 L34.2 74.2 L25.8 65.8 L5.8 85.8 Z" />
    <path d="M39 73 L62 50 L50 38 L27 61 Z" />
    <path d="M67.5 49.5 L94.5 22.5 L77.5 5.5 L50.5 32.5 Z" />
    <template #cut>
      <path d="M68 22 L78 32" />
      <path d="M42 50 L50 58" />
    </template>
  </SplitIcon>
</template>
