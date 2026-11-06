<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// SurvivalOfTheWeakest: щит, о который слабеет удар сильного
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <path d="M50 6 C64 12 76 14 88 14 C88 50 76 76 50 94 C24 76 12 50 12 14 C24 14 36 12 50 6 Z" />
    <template #cut>
      <path d="M50 15 C62 20 72 22 80 22 C79 50 70 70 50 85 C30 70 21 50 20 22 C28 22 38 20 50 15 Z" />
      <!-- Шеврон вниз — урон меньше -->
      <path d="M33 36 L50 52 L67 36 L67 47 L50 63 L33 47 Z" fill="#000" stroke="none" />
    </template>
  </SplitIcon>
</template>
