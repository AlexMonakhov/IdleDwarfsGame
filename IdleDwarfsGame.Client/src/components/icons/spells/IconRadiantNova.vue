<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Radiant Nova: вспышка восьмиконечной звезды
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Radiance">
    <path d="M50 2 L57.3 32.4 L83.9 16.1 L67.6 42.7 L98 50 L67.6 57.3 L83.9 83.9 L57.3 67.6 L50 98 L42.7 67.6 L16.1 83.9 L32.4 57.3 L2 50 L32.4 42.7 L16.1 16.1 L42.7 32.4 Z" />
    <template #cut>
      <path d="M40 50 A10 10 0 1 0 60 50 A10 10 0 1 0 40 50 Z" />
    </template>
  </SplitIcon>
</template>
