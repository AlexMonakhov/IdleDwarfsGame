<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Frost Bolt: ледяной осколок, летящий остриём вниз
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Frost">
    <!-- Осколок -->
    <path d="M5 95 L42 28 L84 8 L72 58 Z" />
    <!-- Льдинки следом -->
    <path d="M60 6 L74 2 L68 16 Z" />
    <path d="M88 26 L98 20 L94 36 Z" />
    <path d="M82 44 L94 44 L86 54 Z" />
    <template #cut>
      <path d="M10 90 L76 18" />
    </template>
  </SplitIcon>
</template>
