<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Снаряжение: щит с каймой и умбоном
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M50 6 C64 12 76 14 88 14 C88 50 76 76 50 94 C24 76 12 50 12 14 C24 14 36 12 50 6 Z" />
    <template #cut>
      <path d="M50 15 C62 20 72 22 80 22 C79 50 70 70 50 85 C30 70 21 50 20 22 C28 22 38 20 50 15 Z" />
      <path d="M41 46 A9 9 0 1 0 59 46 A9 9 0 1 0 41 46 Z" />
    </template>
  </SplitIcon>
</template>
