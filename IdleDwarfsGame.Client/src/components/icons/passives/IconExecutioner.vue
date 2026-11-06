<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// Executioner: топор палача
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Широкое лезвие и топорище -->
    <path d="M56 8 L56 48 C42 48 28 54 14 64 C7 49 7 24 15 6 C28 10 42 10 56 8 Z" />
    <path d="M55 3 L64 3 L64 98 L55 98 Z" />
    <template #cut>
      <path d="M20 15 C15 27 15 45 19 56" />
      <path d="M53 74 L66 74 M53 82 L66 82" />
    </template>
  </SplitIcon>
</template>
