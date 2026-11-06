<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Soul Harvest: коса
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Dark">
    <!-- Лезвие -->
    <path d="M20 14 C44 -2 80 2 98 30 C76 22 52 24 28 40 Z" />
    <path d="M15.1 10.5 L61.1 98.5 L70.9 93.5 L24.9 5.5 Z" />
    <path d="M37.8 60.1 L57.8 51.1 L54.2 42.9 L34.2 51.9 Z" />
    <template #cut>
      <path d="M40 20 C56 12 74 13 88 22" />
    </template>
  </SplitIcon>
</template>
