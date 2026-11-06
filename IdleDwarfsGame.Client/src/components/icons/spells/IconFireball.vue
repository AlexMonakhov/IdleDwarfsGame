<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Fireball: огненный шар с хвостом пламени
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Fire">
    <!-- Хвост -->
    <path d="M18 46 C32 30 48 22 64 17 L58 28 C70 20 83 12 96 5 C88 17 80 27 70 37 L83 35 C75 47 67 57 58 66 Z" />
    <!-- Шар -->
    <path d="M9 63 A29 29 0 1 0 67 63 A29 29 0 1 0 9 63 Z" />
    <template #cut>
      <path d="M23 63 A15 15 0 1 0 53 63 A15 15 0 1 0 23 63 Z" />
      <path d="M30 60 C31 54 35 51 40 51" />
    </template>
  </SplitIcon>
</template>
