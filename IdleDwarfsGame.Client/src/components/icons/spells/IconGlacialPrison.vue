<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Glacial Prison: ледяная глыба-клетка
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Frost">
    <!-- Верхняя, левая и правая грани -->
    <path d="M50 6 L89 26 L50 46 L11 26 Z" />
    <path d="M11 31 L48 51 L48 95 L11 75 Z" />
    <path d="M52 51 L89 31 L89 75 L52 95 Z" />
    <template #cut>
      <!-- Прутья клетки -->
      <path d="M23 41 L23 81 M36 48 L36 88 M64 48 L64 88 M77 41 L77 81" />
      <path d="M38 18 L50 24 L62 18" />
    </template>
  </SplitIcon>
</template>
