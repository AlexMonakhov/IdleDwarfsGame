<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// StoneSkin: каменная глыба с трещинами
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <path d="M18 38 L30 16 L58 8 L84 22 L94 52 L82 82 L50 93 L20 82 L6 58 Z" />
    <template #cut>
      <path d="M30 16 L40 40 L30 58 M58 8 L56 34 L74 50 M40 40 L56 34 M30 58 L50 66 L74 50 M50 66 L50 93 M74 50 L94 52" />
    </template>
  </SplitIcon>
</template>
