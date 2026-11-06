<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Hex: печать с пентаграммой
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Dark">
    <path d="M5 50 A45 45 0 1 0 95 50 A45 45 0 1 0 5 50 Z" />
    <template #cut>
      <path d="M13 50 A37 37 0 1 0 87 50 A37 37 0 1 0 13 50 Z" />
      <!-- Пентаграмма -->
      <path d="M50 17 L69.4 76.7 L18.6 39.8 L81.4 39.8 L30.6 76.7 Z" />
    </template>
  </SplitIcon>
</template>
