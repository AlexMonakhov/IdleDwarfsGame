<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Звук выключен: динамик с крестом
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M8 36 L27 36 L50 15 L50 85 L27 64 L8 64 Z" />
    <path transform="rotate(45 78 50)" d="M74 30 L82 30 L82 70 L74 70 Z" />
    <path transform="rotate(45 78 50)" d="M58 46 L98 46 L98 54 L58 54 Z" />
  </SplitIcon>
</template>
