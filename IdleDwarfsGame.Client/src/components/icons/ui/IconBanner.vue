<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Отряд: знамя на перекладине с шевронами
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Перекладина с навершиями -->
    <path d="M14 10 L86 10 L86 17 L14 17 Z" />
    <path d="M4.5 13.5 A5.5 5.5 0 1 0 15.5 13.5 A5.5 5.5 0 1 0 4.5 13.5 Z" />
    <path d="M84.5 13.5 A5.5 5.5 0 1 0 95.5 13.5 A5.5 5.5 0 1 0 84.5 13.5 Z" />
    <!-- Полотнище с вырезом-«ласточкиным хвостом» -->
    <path d="M24 20 L76 20 L76 93 L50 79 L24 93 Z" />
    <template #cut>
      <!-- Кайма и два шеврона -->
      <path d="M31 27 L69 27 L69 82 L50 71.5 L31 82 Z" />
      <path d="M38 52 L50 41 L62 52" />
      <path d="M38 63 L50 52 L62 63" />
    </template>
  </SplitIcon>
</template>
