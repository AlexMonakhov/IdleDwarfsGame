<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Chain Lightning: молния скачет между тремя целями
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Lightning">
    <!-- Цели -->
    <path d="M3 17 A14 14 0 1 0 31 17 A14 14 0 1 0 3 17 Z" />
    <path d="M69 50 A14 14 0 1 0 97 50 A14 14 0 1 0 69 50 Z" />
    <path d="M6 83 A14 14 0 1 0 34 83 A14 14 0 1 0 6 83 Z" />
    <path d="M29.4 29.3 L51.4 23.3 L48.6 12.7 L26.6 18.7 Z" />
    <path d="M44.8 16.3 L38.8 34.3 L49.2 37.7 L55.2 19.7 Z" />
    <path d="M42.8 41.4 L70.8 47.4 L73.2 36.6 L45.2 30.6 Z" />
    <path d="M44.5 18 A5.5 5.5 0 1 0 55.5 18 A5.5 5.5 0 1 0 44.5 18 Z" />
    <path d="M38.5 36 A5.5 5.5 0 1 0 49.5 36 A5.5 5.5 0 1 0 38.5 36 Z" />
    <path d="M72 52.5 L52 52.5 L52 63.5 L72 63.5 Z" />
    <path d="M46.9 60.2 L52.9 74.2 L63.1 69.8 L57.1 55.8 Z" />
    <path d="M56.8 66.6 L30.8 72.6 L33.2 83.4 L59.2 77.4 Z" />
    <path d="M46.5 58 A5.5 5.5 0 1 0 57.5 58 A5.5 5.5 0 1 0 46.5 58 Z" />
    <path d="M52.5 72 A5.5 5.5 0 1 0 63.5 72 A5.5 5.5 0 1 0 52.5 72 Z" />
    <template #cut>
      <path d="M11.5 17 A5.5 5.5 0 1 0 22.5 17 A5.5 5.5 0 1 0 11.5 17 Z" fill="#000" stroke="none" />
      <path d="M77.5 50 A5.5 5.5 0 1 0 88.5 50 A5.5 5.5 0 1 0 77.5 50 Z" fill="#000" stroke="none" />
      <path d="M14.5 83 A5.5 5.5 0 1 0 25.5 83 A5.5 5.5 0 1 0 14.5 83 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
