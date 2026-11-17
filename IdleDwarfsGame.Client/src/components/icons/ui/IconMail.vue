<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Кнопка «Почта»: конверт
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M6 22 L94 22 L94 82 L6 82 Z" />
    <template #cut>
      <!-- Клапан конверта и боковые сгибы -->
      <path d="M8 24 L50 56 L92 24" />
      <path d="M8 80 L36 56 M92 80 L64 56" />
    </template>
  </SplitIcon>
</template>
