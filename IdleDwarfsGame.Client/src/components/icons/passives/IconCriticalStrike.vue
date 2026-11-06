<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// CriticalStrike: прицел — удар в уязвимое место
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Кольцо прицела, засечки и точка -->
    <path d="M84 50 L83.7 54.4 L82.8 58.8 L81.4 63 L79.4 67 L77 70.7 L74 74 L70.7 77 L67 79.4 L63 81.4 L58.8 82.8 L54.4 83.7 L50 84 L45.6 83.7 L41.2 82.8 L37 81.4 L33 79.4 L29.3 77 L26 74 L23 70.7 L20.6 67 L18.6 63 L17.2 58.8 L16.3 54.4 L16 50 L16.3 45.6 L17.2 41.2 L18.6 37 L20.6 33 L23 29.3 L26 26 L29.3 23 L33 20.6 L37 18.6 L41.2 17.2 L45.6 16.3 L50 16 L54.4 16.3 L58.8 17.2 L63 18.6 L67 20.6 L70.7 23 L74 26 L77 29.3 L79.4 33 L81.4 37 L82.8 41.2 L83.7 45.6 L84 50 L76 50 L75.8 46.6 L75.1 43.3 L74 40.1 L72.5 37 L70.6 34.2 L68.4 31.6 L65.8 29.4 L63 27.5 L59.9 26 L56.7 24.9 L53.4 24.2 L50 24 L46.6 24.2 L43.3 24.9 L40.1 26 L37 27.5 L34.2 29.4 L31.6 31.6 L29.4 34.2 L27.5 37 L26 40.1 L24.9 43.3 L24.2 46.6 L24 50 L24.2 53.4 L24.9 56.7 L26 59.9 L27.5 63 L29.4 65.8 L31.6 68.4 L34.2 70.6 L37 72.5 L40.1 74 L43.3 75.1 L46.6 75.8 L50 76 L53.4 75.8 L56.7 75.1 L59.9 74 L63 72.5 L65.8 70.6 L68.4 68.4 L70.6 65.8 L72.5 63 L74 59.9 L75.1 56.7 L75.8 53.4 L76 50 Z" />
    <path d="M46.5 2 L53.5 2 L53.5 30 L46.5 30 Z" />
    <path d="M46.5 70 L53.5 70 L53.5 98 L46.5 98 Z" />
    <path d="M2 46.5 L30 46.5 L30 53.5 L2 53.5 Z" />
    <path d="M70 46.5 L98 46.5 L98 53.5 L70 53.5 Z" />
    <path d="M42 50 A8 8 0 1 0 58 50 A8 8 0 1 0 42 50 Z" />
  </SplitIcon>
</template>
