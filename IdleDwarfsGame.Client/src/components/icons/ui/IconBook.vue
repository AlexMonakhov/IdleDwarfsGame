<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Рейтинговая история: раскрытая книга
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M49 24 C39 16 24 13 6 17 L6 83 C24 79 39 81 49 89 Z" />
    <path d="M51 24 C61 16 76 13 94 17 L94 83 C76 79 61 81 51 89 Z" />
    <template #cut>
      <!-- Строки на страницах -->
      <path d="M14 32 C24 29 34 30 42 35 M14 44 C24 41 34 42 42 47 M14 56 C24 53 34 54 42 59" />
      <path d="M86 32 C76 29 66 30 58 35 M86 44 C76 41 66 42 58 47 M86 56 C76 53 66 54 58 59" />
    </template>
  </SplitIcon>
</template>
