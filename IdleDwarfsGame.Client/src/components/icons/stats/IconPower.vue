<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Боевая мощь: скрещённые мечи (раньше — эмодзи ⚔)
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Меч остриём вверх, повёрнут на ±45° вокруг центра -->
    <g transform="rotate(-45 50 50)">
      <path d="M50 1 L58 14 L58 63 L42 63 L42 14 Z" />
      <path d="M27 63 L73 63 L73 71 L27 71 Z" />
      <path d="M45 71 L55 71 L55 86 L45 86 Z" />
      <path d="M50 84 L57 91 L50 98 L43 91 Z" />
    </g>
    <g transform="rotate(45 50 50)">
      <path d="M50 1 L58 14 L58 63 L42 63 L42 14 Z" />
      <path d="M27 63 L73 63 L73 71 L27 71 Z" />
      <path d="M45 71 L55 71 L55 86 L45 86 Z" />
      <path d="M50 84 L57 91 L50 98 L43 91 Z" />
    </g>
    <template #cut>
      <!-- Долы клинков, не доходят до перекрестья -->
      <path transform="rotate(-45 50 50)" d="M50 15 L50 36" />
      <path transform="rotate(45 50 50)" d="M50 15 L50 36" />
      <!-- Кромки верхнего меча там, где он лежит на нижнем: отделяют один клинок от другого -->
      <path transform="rotate(45 50 50)" d="M40.3 38 L40.3 60 M59.7 38 L59.7 60" />
    </template>
  </SplitIcon>
</template>
