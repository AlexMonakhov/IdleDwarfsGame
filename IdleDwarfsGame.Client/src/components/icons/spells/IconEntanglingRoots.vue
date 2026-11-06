<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Entangling Roots: корни, вырастающие из земли и закручивающиеся
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Nature">
    <!-- Земля -->
    <path d="M4 95 C18 80 82 80 96 95 Z" />
    <!-- Корни: два завитка по бокам и один посередине -->
    <path d="M32 86 C20 70 16 50 26 36 C32 28 42 30 42 40 C42 46 36 49 33 45 C31 56 35 70 40 86 Z" />
    <path d="M68 86 C80 70 84 50 74 36 C68 28 58 30 58 40 C58 46 64 49 67 45 C69 56 65 70 60 86 Z" />
    <path d="M46 86 C44 60 53 40 46 10 C56 20 59 36 57 50 C56 64 55 76 54 86 Z" />
  </SplitIcon>
</template>
