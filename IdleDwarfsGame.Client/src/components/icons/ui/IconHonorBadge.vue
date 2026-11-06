<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Значок почёта — валюта лавки гильдии (боссы гильдии): медаль на ленте со звездой
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :primary-color="props.primaryColor ?? '#dbeafe'" :secondary-color="props.secondaryColor ?? '#4f8ff7'">
    <!-- Лента -->
    <path d="M28 4 L46 4 L58 36 L40 36 Z" />
    <path d="M72 4 L54 4 L42 36 L60 36 Z" />
    <!-- Медаль -->
    <path d="M20 64 A30 30 0 1 0 80 64 A30 30 0 1 0 20 64 Z" />
    <template #cut>
      <!-- Зазор между лентой и медалью, кант и звезда -->
      <path d="M17.5 64 A32.5 32.5 0 1 0 82.5 64 A32.5 32.5 0 1 0 17.5 64 Z" />
      <path d="M29 64 A21 21 0 1 0 71 64 A21 21 0 1 0 29 64 Z" />
      <path d="M50 49 L54 60 L65.6 60.3 L56.4 67.4 L59.7 78.6 L50 72 L40.3 78.6 L43.6 67.4 L34.4 60.3 L46 60 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
