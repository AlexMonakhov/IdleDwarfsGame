<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// Vampirism: клыки и капля крови
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Верхняя челюсть с клыками -->
    <path d="M6 18 C28 34 72 34 94 18 L94 28 C82 39 72 43 65 44 L59 72 L54 45 L46 45 L41 72 L35 44 C28 43 18 39 6 28 Z" />
    <!-- Капля -->
    <path d="M59 76 C62.168 82.6 66.92 87 66.92 90.08 C66.92 95.624 63.356 98 59 98 C54.644 98 51.08 95.624 51.08 90.08 C51.08 87 55.832 82.6 59 76 Z" />
  </SplitIcon>
</template>
