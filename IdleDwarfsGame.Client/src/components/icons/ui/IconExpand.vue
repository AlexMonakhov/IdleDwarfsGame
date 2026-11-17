<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Развернуть: шеврон вниз (развёрнутый блок поворачивает его на 180°)
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M6 30 L20 16 L50 46 L80 16 L94 30 L50 74 Z" />
  </SplitIcon>
</template>
