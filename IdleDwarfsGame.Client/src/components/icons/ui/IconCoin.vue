<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Золото: монета перед стопкой монет
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :primary-color="props.primaryColor ?? '#ffe7a3'" :secondary-color="props.secondaryColor ?? '#f2b33d'">
    <!-- Стопка -->
    <path d="M6 62 L46 62 L46 71 L6 71 Z" />
    <path d="M6 74 L46 74 L46 83 L6 83 Z" />
    <path d="M6 86 L46 86 L46 95 L6 95 Z" />
    <!-- Монета -->
    <path d="M26 44 A34 34 0 1 0 94 44 A34 34 0 1 0 26 44 Z" />
    <template #cut>
      <!-- Зазор вокруг монеты, гурт и чекан -->
      <path d="M23.5 44 A36.5 36.5 0 1 0 96.5 44 A36.5 36.5 0 1 0 23.5 44 Z" />
      <path d="M35 44 A25 25 0 1 0 85 44 A25 25 0 1 0 35 44 Z" />
      <path d="M60 33 L62.9 41 L71.4 41.3 L64.8 46.5 L67.1 54.7 L60 50 L52.9 54.7 L55.2 46.5 L48.6 41.3 L57.1 41 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
