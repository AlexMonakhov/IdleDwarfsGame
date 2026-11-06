<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// DoubleStrike: два клинка — второй удар
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Первый клинок -->
    <path transform="translate(-12 -12) rotate(45 50 50)" d="M50 4 L56 14 L56 62 L44 62 L44 14 Z" />
    <path transform="translate(-12 -12) rotate(45 50 50)" d="M36 62 L64 62 L64 68 L36 68 Z" />
    <path transform="translate(-12 -12) rotate(45 50 50)" d="M46.5 68 L53.5 68 L53.5 82 L46.5 82 Z" />
    <path transform="translate(-12 -12) rotate(45 50 50)" d="M50 80 L55 85 L50 90 L45 85 Z" />
    <!-- Второй клинок -->
    <path transform="translate(12 12) rotate(45 50 50)" d="M50 4 L56 14 L56 62 L44 62 L44 14 Z" />
    <path transform="translate(12 12) rotate(45 50 50)" d="M36 62 L64 62 L64 68 L36 68 Z" />
    <path transform="translate(12 12) rotate(45 50 50)" d="M46.5 68 L53.5 68 L53.5 82 L46.5 82 Z" />
    <path transform="translate(12 12) rotate(45 50 50)" d="M50 80 L55 85 L50 90 L45 85 Z" />
    <template #cut>
      <path transform="translate(-12 -12) rotate(45 50 50)" d="M50 16 L50 52" />
      <path transform="translate(12 12) rotate(45 50 50)" d="M50 16 L50 52" />
    </template>
  </SplitIcon>
</template>
