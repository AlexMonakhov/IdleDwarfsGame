<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Меч: слот оружия, профессия воина
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Клинок, гарда, рукоять, навершие -->
    <path d="M50 2 L59 15 L59 64 L41 64 L41 15 Z" />
    <path d="M24 64 L76 64 L76 72 L24 72 Z" />
    <path d="M45 72 L55 72 L55 88 L45 88 Z" />
    <path d="M50 86 L57 92 L50 99 L43 92 Z" />
    <template #cut>
      <path d="M50 17 L50 56" />
    </template>
  </SplitIcon>
</template>
