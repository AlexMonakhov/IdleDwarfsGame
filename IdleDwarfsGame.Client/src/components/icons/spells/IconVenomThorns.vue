<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Venom Thorns: колючая лоза и капля яда
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Nature">
    <!-- Лоза -->
    <path d="M41 96 C37 76 57 66 53 46 C49 30 37 22 43 4 L57 4 C51 22 63 30 67 46 C71 66 51 76 55 96 Z" />
    <!-- Шипы -->
    <path d="M62 62 L90 52 L64 48 Z" />
    <path d="M48 84 L20 82 L45 72 Z" />
    <path d="M50 38 L24 28 L50 26 Z" />
    <path d="M58 20 L82 8 L58 8 Z" />
    <!-- Капля яда -->
    <path d="M20 44 C24.896 54.2 32.24 61 32.24 65.76 C32.24 74.328 26.732 78 20 78 C13.267999999999999 78 7.76 74.328 7.76 65.76 C7.76 61 15.104 54.2 20 44 Z" />
  </SplitIcon>
</template>
