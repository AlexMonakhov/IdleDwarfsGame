<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Кнопка «Вход»: арочная дверь со стрелкой внутрь
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M22 94 L22 36 Q22 8 50 8 Q78 8 78 36 L78 94 Z" />
    <template #cut>
      <!-- Стрелка внутрь и порог -->
      <path d="M34 54 L62 54 M52 42 L64 54 L52 66" />
      <path d="M14 92 L86 92" />
    </template>
  </SplitIcon>
</template>
