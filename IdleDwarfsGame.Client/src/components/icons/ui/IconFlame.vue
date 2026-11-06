<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Пламя: вкладка эффектов, обычная арена, ярость
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M52 4 C56 18 66 24 70 36 C73 28 72 21 69 15 C83 24 91 41 89 60 C87 81 71 95 50 95 C29 95 13 81 11 60 C10 46 16 34 25 27 C24 35 26 40 31 45 C30 29 39 15 52 4 Z" />
    <template #cut>
      <!-- Внутренний язык пламени -->
      <path d="M51 40 C53 50 62 55 65 66 C68 78 60 87 50 87 C40 87 32 79 35 67 C37 60 42 57 44 52 C46 57 47 60 50 61 C49 54 49 47 51 40 Z" />
    </template>
  </SplitIcon>
</template>
