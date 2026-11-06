<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Healing Rune: рунный круг с крестом исцеления
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Nature">
    <path d="M6 50 A44 44 0 1 0 94 50 A44 44 0 1 0 6 50 Z" />
    <template #cut>
      <path d="M15 50 A35 35 0 1 0 85 50 A35 35 0 1 0 15 50 Z" />
      <!-- Крест -->
      <path d="M43 22 L57 22 L57 43 L78 43 L78 57 L57 57 L57 78 L43 78 L43 57 L22 57 L22 43 L43 43 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
