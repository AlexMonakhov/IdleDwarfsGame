<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// Cleave: размашистый удар, задевающий следующего
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Два серпа замаха -->
    <path d="M8 16 C56 4 94 36 88 92 C78 50 52 26 8 16 Z" />
    <path d="M6 40 C36 36 60 56 60 92 C50 66 32 50 6 40 Z" />
  </SplitIcon>
</template>
