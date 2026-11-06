<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Blood Price: ритуальный кинжал и капли крови
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Blood">
    <!-- Навершие, рукоять, гарда, клинок -->
    <path d="M43 8 A7 7 0 1 0 57 8 A7 7 0 1 0 43 8 Z" />
    <path d="M44 13 L56 13 L56 30 L44 30 Z" />
    <path d="M22 28 C34 34 66 34 78 28 L73 40 C64 43 36 43 27 40 Z" />
    <path d="M40 42 L60 42 L59 74 L50 96 L41 74 Z" />
    <!-- Капли -->
    <path d="M83 52 C87.32 61 93.8 67 93.8 71.2 C93.8 78.76 88.94 82 83 82 C77.06 82 72.2 78.76 72.2 71.2 C72.2 67 78.68 61 83 52 Z" />
    <path d="M16 62 C19.168 68.6 23.92 73 23.92 76.08 C23.92 81.624 20.356 84 16 84 C11.644 84 8.08 81.624 8.08 76.08 C8.08 73 12.832 68.6 16 62 Z" />
    <template #cut>
      <path d="M50 48 L50 78" />
      <path d="M44 19 L56 19 M44 25 L56 25" />
    </template>
  </SplitIcon>
</template>
