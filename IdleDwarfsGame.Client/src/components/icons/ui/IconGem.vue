<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Камни: огранённый самоцвет
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M28 14 L72 14 L92 38 L50 92 L8 38 Z" />
    <template #cut>
      <path d="M8 38 L92 38" />
      <path d="M28 14 L38 38 L50 14 L62 38 L72 14" />
      <path d="M38 38 L50 88 L62 38" />
    </template>
  </SplitIcon>
</template>
