<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Meteor: падающий камень с огненными полосами
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Fire">
    <!-- Огненные полосы -->
    <path d="M40 40 L82 4 L92 12 L56 52 Z" />
    <path d="M58 58 L94 32 L97 44 L62 70 Z" />
    <path d="M24 40 L50 8 L58 13 L36 44 Z" />
    <!-- Камень -->
    <path d="M12 60 L22 42 L44 37 L59 51 L57 75 L40 91 L17 85 Z" />
    <template #cut>
      <!-- Кратер и трещины -->
      <path d="M30 68 A8 8 0 1 0 46 68 A8 8 0 1 0 30 68 Z" />
      <path d="M20 56 L28 50 L34 52" />
      <path d="M24 80 L30 74" />
    </template>
  </SplitIcon>
</template>
