<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Звук включён: динамик с волнами
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M8 36 L27 36 L50 15 L50 85 L27 64 L8 64 Z" />
    <path d="M59 35 C65 41 65 59 59 65 L64 70 C73 61 73 39 64 30 Z" />
    <path d="M70 22 C82 34 82 66 70 78 L75 83 C90 68 90 32 75 17 Z" />
  </SplitIcon>
</template>
