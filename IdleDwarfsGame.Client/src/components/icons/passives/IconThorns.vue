<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// Thorns: щит с шипами — ударивший ранится сам
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Щит -->
    <path transform="translate(13 12) scale(0.74)" d="M50 6 C64 12 76 14 88 14 C88 50 76 76 50 94 C24 76 12 50 12 14 C24 14 36 12 50 6 Z" />
    <!-- Шипы -->
    <path d="M45 18 L50 2 L55 18 Z" />
    <path d="M24.8 26.2 L17.4 12.4 L31.2 19.8 Z" />
    <path d="M68.8 19.8 L82.6 12.4 L75.2 26.2 Z" />
    <path d="M22 48.5 L7 44 L22 39.5 Z" />
    <path d="M78 39.5 L93 44 L78 48.5 Z" />
    <path d="M31.6 68.7 L16.7 73.6 L26.4 61.3 Z" />
    <path d="M73.6 61.3 L83.3 73.6 L68.4 68.7 Z" />
    <path d="M54.5 82 L50 96 L45.5 82 Z" />
    <template #cut>
      <path d="M50 26 L50 72 M33 40 L67 40" stroke-width="4" />
    </template>
  </SplitIcon>
</template>
