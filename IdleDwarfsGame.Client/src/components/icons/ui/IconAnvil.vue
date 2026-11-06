<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Слияние героев: наковальня и молот
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Наковальня с рогом слева -->
    <path d="M8 42 L88 42 L88 54 L74 54 C66 56 62 61 62 66 L62 72 L76 76 L76 88 L24 88 L24 76 L38 72 L38 66 C38 61 33 56 25 54 C16 53 10 49 8 42 Z" />
    <!-- Молот над ней -->
    <path transform="rotate(-25 65 15)" d="M52 6 L78 6 L78 24 L52 24 Z" />
    <path transform="rotate(-25 65 15)" d="M14 12 L54 12 L54 18 L14 18 Z" />
    <template #cut>
      <path d="M14 48 L86 48" />
    </template>
  </SplitIcon>
</template>
