<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Thunderclap: боевой молот, бьющий молниями
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Lightning">
    <!-- Боёк и рукоять -->
    <path d="M22 8 L78 8 L84 14 L84 36 L78 42 L22 42 L16 36 L16 14 Z" />
    <path d="M44 42 L56 42 L56 95 L44 95 Z" />
    <!-- Молнии по бокам -->
    <path d="M24 48 L8 68 L19 68 L11 88 L33 62 L22 62 L31 48 Z" />
    <path d="M76 48 L92 68 L81 68 L89 88 L67 62 L78 62 L69 48 Z" />
    <template #cut>
      <path d="M30 8 L30 42 M70 8 L70 42" />
      <path d="M40 60 L60 60 M40 70 L60 70" />
    </template>
  </SplitIcon>
</template>
