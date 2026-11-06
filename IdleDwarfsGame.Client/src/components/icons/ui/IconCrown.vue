<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Корона: рейтинговые сезоны, босс, лидер гильдии
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Зубцы с шариками и обруч -->
    <path d="M10 30 L31 54 L50 20 L69 54 L90 30 L84 76 L16 76 Z" />
    <path d="M4 25 A6 6 0 1 0 16 25 A6 6 0 1 0 4 25 Z" />
    <path d="M44 14 A6 6 0 1 0 56 14 A6 6 0 1 0 44 14 Z" />
    <path d="M84 25 A6 6 0 1 0 96 25 A6 6 0 1 0 84 25 Z" />
    <path d="M16 80 L84 80 L84 92 L16 92 Z" />
    <template #cut>
      <!-- Камни на обруче -->
      <path d="M30 86 L30.5 86 M50 86 L50.5 86 M70 86 L70.5 86" stroke-width="5" />
      <path d="M20 66 L80 66" />
    </template>
  </SplitIcon>
</template>
