<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Life Drain: из сердца уходят капли жизни
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Blood">
    <!-- Сердце -->
    <path transform="translate(-2 26) scale(0.76)" d="M50 90 C30 76 8 60 8 36 C8 20 20 10 33 10 C41 10 47 15 50 21 C53 15 59 10 67 10 C80 10 92 20 92 36 C92 60 70 76 50 90 Z" />
    <!-- Капли -->
    <path d="M74 24 C77.168 30.6 81.92 35 81.92 38.08 C81.92 43.624 78.356 46 74 46 C69.644 46 66.08 43.624 66.08 38.08 C66.08 35 70.832 30.6 74 24 Z" />
    <path d="M88 3 C90.304 7.8 93.76 11 93.76 13.24 C93.76 17.272 91.168 19 88 19 C84.832 19 82.24 17.272 82.24 13.24 C82.24 11 85.696 7.8 88 3 Z" />
    <template #cut>
      <path transform="translate(-2 26) scale(0.76)" d="M50 77.7 C35.6 67.6 19.8 56.1 19.8 38.8 C19.8 27.3 28.4 20.1 37.8 20.1 C43.5 20.1 47.8 23.7 50 28 C52.2 23.7 56.5 20.1 62.2 20.1 C71.6 20.1 80.2 27.3 80.2 38.8 C80.2 56.1 64.4 67.6 50 77.7 Z" stroke-width="4.5" />
    </template>
  </SplitIcon>
</template>
