<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Dawnlight: солнце, встающее над горизонтом
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Radiance">
    <!-- Солнце и лучи -->
    <path d="M23 72 A27 27 0 0 1 77 72 Z" />
    <path d="M17.6 65.7 L3.4 56.9 L20.1 58.1 Z" />
    <path d="M24.4 51.2 L16 36.8 L30.1 45.6 Z" />
    <path d="M37.1 41.6 L36.5 24.9 L44.8 39.4 Z" />
    <path d="M55.2 39.4 L63.5 24.9 L62.9 41.6 Z" />
    <path d="M69.9 45.6 L84 36.8 L75.6 51.2 Z" />
    <path d="M79.9 58.1 L96.6 56.9 L82.4 65.7 Z" />
    <!-- Горизонт -->
    <path d="M4 78 L96 78 L96 85 L4 85 Z" />
    <path d="M18 89 L82 89 L82 95 L18 95 Z" />
  </SplitIcon>
</template>
