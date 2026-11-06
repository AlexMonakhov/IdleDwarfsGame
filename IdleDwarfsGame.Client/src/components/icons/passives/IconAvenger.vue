<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// Avenger: меч, воткнутый в могильный холм
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Холм -->
    <path d="M6 96 C18 74 82 74 94 96 Z" />
    <!-- Навершие, рукоять, гарда, клинок остриём вниз -->
    <path d="M43 9 A7 7 0 1 0 57 9 A7 7 0 1 0 43 9 Z" />
    <path d="M45 14 L55 14 L55 33 L45 33 Z" />
    <path d="M26 33 L74 33 L74 41 L26 41 Z" />
    <path d="M42 41 L58 41 L58 80 L50 90 L42 80 Z" />
    <template #cut>
      <path d="M50 47 L50 74" />
      <path d="M30 80 C40 77 60 77 70 80" />
    </template>
  </SplitIcon>
</template>
