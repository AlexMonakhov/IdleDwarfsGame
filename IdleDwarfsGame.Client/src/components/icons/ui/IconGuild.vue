<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Гильдия: крепость с воротами
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M8 92 L8 18 L15 18 L15 24 L21 24 L21 18 L28 18 L28 40 L36 40 L36 12 L43 12 L43 19 L46.5 19 L46.5 12 L53.5 12 L53.5 19 L57 19 L57 12 L64 12 L64 40 L72 40 L72 18 L79 18 L79 24 L85 24 L85 18 L92 18 L92 92 Z" />
    <template #cut>
      <!-- Ворота аркой -->
      <path d="M39 92 L39 72 C39 64 44 59 50 59 C56 59 61 64 61 72 L61 92 Z" fill="#000" stroke="none" />
      <!-- Бойницы -->
      <path d="M18 46 L18 58 M82 46 L82 58 M50 28 L50 40" />
      <path d="M28 44 L28 92 M72 44 L72 92" />
    </template>
  </SplitIcon>
</template>
