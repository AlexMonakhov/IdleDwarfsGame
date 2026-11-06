<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Кровавая монета — валюта подпольного рынка (бои на арене): тёмно-красная монета с каплей крови
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :primary-color="props.primaryColor ?? '#ff9a9a'" :secondary-color="props.secondaryColor ?? '#c0262d'">
    <path d="M6 50 A44 44 0 1 0 94 50 A44 44 0 1 0 6 50 Z" />
    <template #cut>
      <!-- Гурт и капля -->
      <path d="M18 50 A32 32 0 1 0 82 50 A32 32 0 1 0 18 50 Z" />
      <path d="M50 27 C50 27 35 45 35 55 A15 15 0 0 0 65 55 C65 45 50 27 50 27 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
