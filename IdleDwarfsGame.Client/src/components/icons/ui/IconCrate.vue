<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Хранилище: ящик
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M8 8 L92 8 L92 92 L8 92 Z" />
    <template #cut>
      <path d="M20 20 L80 20 L80 80 L20 80 Z" />
      <path d="M22 78 L78 22" />
    </template>
  </SplitIcon>
</template>
