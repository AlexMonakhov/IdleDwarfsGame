<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Подтвердить: галочка
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M6 52 L21 37 L40 56 L79 15 L94 30 L40 86 Z" />
  </SplitIcon>
</template>
