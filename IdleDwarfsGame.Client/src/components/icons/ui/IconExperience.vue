<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Опыт: большая и малая искры (раньше — ✦)
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :primary-color="props.primaryColor ?? '#e0f7ff'" :secondary-color="props.secondaryColor ?? '#67e8f9'">
    <path d="M42 14 L49.1 48.9 L84 56 L49.1 63.1 L42 98 L34.9 63.1 L0 56 L34.9 48.9 Z" />
    <path d="M82 2 L84.8 15.2 L98 18 L84.8 20.8 L82 34 L79.2 20.8 L66 18 L79.2 15.2 Z" />
  </SplitIcon>
</template>
