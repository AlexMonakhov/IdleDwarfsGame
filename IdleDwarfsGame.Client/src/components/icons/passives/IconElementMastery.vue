<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// ElementMastery: пламя, капля и молния — эффекты стихий сильнее
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Пламя -->
    <path transform="translate(28 -2) scale(0.44)" d="M52 4 C56 18 66 24 70 36 C73 28 72 21 69 15 C83 24 91 41 89 60 C87 81 71 95 50 95 C29 95 13 81 11 60 C10 46 16 34 25 27 C24 35 26 40 31 45 C30 29 39 15 52 4 Z" />
    <!-- Капля -->
    <path d="M26 50 C32.336 63.2 41.84 72 41.84 78.16 C41.84 89.248 34.712 94 26 94 C17.288 94 10.16 89.248 10.16 78.16 C10.16 72 19.664 63.2 26 50 Z" />
    <!-- Молния -->
    <path transform="translate(50 50) scale(0.46)" d="M60 3 L20 55 L45 55 L33 97 L82 39 L55 39 L70 3 Z" />
  </SplitIcon>
</template>
