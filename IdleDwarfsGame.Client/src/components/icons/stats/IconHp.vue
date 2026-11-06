<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Здоровье: сердце с внутренним контуром
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M50 90 C30 76 8 60 8 36 C8 20 20 10 33 10 C41 10 47 15 50 21 C53 15 59 10 67 10 C80 10 92 20 92 36 C92 60 70 76 50 90 Z" />
    <template #cut>
      <path d="M50 77.7 C35.6 67.6 19.8 56.1 19.8 38.8 C19.8 27.3 28.4 20.1 37.8 20.1 C43.5 20.1 47.8 23.7 50 28 C52.2 23.7 56.5 20.1 62.2 20.1 C71.6 20.1 80.2 27.3 80.2 38.8 C80.2 56.1 64.4 67.6 50 77.7 Z" />
    </template>
  </SplitIcon>
</template>
