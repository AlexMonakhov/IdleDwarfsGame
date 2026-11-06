<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Blizzard: снежинка
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Frost">
    <path transform="rotate(0 50 50)" d="M53.5 50 L53.5 6 L46.5 6 L46.5 50 Z" />
    <path transform="rotate(0 50 50)" d="M52.1 23.9 L40.1 11.9 L35.9 16.1 L47.9 28.1 Z" />
    <path transform="rotate(0 50 50)" d="M52.1 28.1 L64.1 16.1 L59.9 11.9 L47.9 23.9 Z" />
    <path transform="rotate(60 50 50)" d="M53.5 50 L53.5 6 L46.5 6 L46.5 50 Z" />
    <path transform="rotate(60 50 50)" d="M52.1 23.9 L40.1 11.9 L35.9 16.1 L47.9 28.1 Z" />
    <path transform="rotate(60 50 50)" d="M52.1 28.1 L64.1 16.1 L59.9 11.9 L47.9 23.9 Z" />
    <path transform="rotate(120 50 50)" d="M53.5 50 L53.5 6 L46.5 6 L46.5 50 Z" />
    <path transform="rotate(120 50 50)" d="M52.1 23.9 L40.1 11.9 L35.9 16.1 L47.9 28.1 Z" />
    <path transform="rotate(120 50 50)" d="M52.1 28.1 L64.1 16.1 L59.9 11.9 L47.9 23.9 Z" />
    <path transform="rotate(180 50 50)" d="M53.5 50 L53.5 6 L46.5 6 L46.5 50 Z" />
    <path transform="rotate(180 50 50)" d="M52.1 23.9 L40.1 11.9 L35.9 16.1 L47.9 28.1 Z" />
    <path transform="rotate(180 50 50)" d="M52.1 28.1 L64.1 16.1 L59.9 11.9 L47.9 23.9 Z" />
    <path transform="rotate(240 50 50)" d="M53.5 50 L53.5 6 L46.5 6 L46.5 50 Z" />
    <path transform="rotate(240 50 50)" d="M52.1 23.9 L40.1 11.9 L35.9 16.1 L47.9 28.1 Z" />
    <path transform="rotate(240 50 50)" d="M52.1 28.1 L64.1 16.1 L59.9 11.9 L47.9 23.9 Z" />
    <path transform="rotate(300 50 50)" d="M53.5 50 L53.5 6 L46.5 6 L46.5 50 Z" />
    <path transform="rotate(300 50 50)" d="M52.1 23.9 L40.1 11.9 L35.9 16.1 L47.9 28.1 Z" />
    <path transform="rotate(300 50 50)" d="M52.1 28.1 L64.1 16.1 L59.9 11.9 L47.9 23.9 Z" />
    <!-- Середина -->
    <path d="M39 50 A11 11 0 1 0 61 50 A11 11 0 1 0 39 50 Z" />
    <template #cut>
      <path d="M45.5 50 A4.5 4.5 0 1 0 54.5 50 A4.5 4.5 0 1 0 45.5 50 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
