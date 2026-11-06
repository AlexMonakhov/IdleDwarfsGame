<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// LastStand: треснувшее, но целое сердце
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <path d="M50 90 C30 76 8 60 8 36 C8 20 20 10 33 10 C41 10 47 15 50 21 C53 15 59 10 67 10 C80 10 92 20 92 36 C92 60 70 76 50 90 Z" />
    <template #cut>
      <!-- Трещина -->
      <path d="M50 21 L43 38 L57 50 L45 64 L50 80" stroke-width="5" />
    </template>
  </SplitIcon>
</template>
