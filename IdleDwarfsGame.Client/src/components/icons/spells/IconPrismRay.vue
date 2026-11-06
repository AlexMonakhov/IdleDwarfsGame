<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Prism Ray: луч входит в призму и распадается на три
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Radiance">
    <path d="M3 67.3 L35 57.3 L33 50.7 L1 60.7 Z" />
    <!-- Призма -->
    <path d="M50 12 L85 76 L15 76 Z" />
    <path d="M67.5 46.6 L99.5 28.6 L96.5 23.4 L64.5 41.4 Z" />
    <path d="M70.6 54.9 L98.6 48.9 L97.4 43.1 L69.4 49.1 Z" />
    <path d="M73.3 62.9 L97.3 68.9 L98.7 63.1 L74.7 57.1 Z" />
    <template #cut>
      <path d="M50 27 L73 69 L27 69 Z" />
    </template>
  </SplitIcon>
</template>
