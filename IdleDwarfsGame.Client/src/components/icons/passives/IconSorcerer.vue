<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// Sorcerer: колпак чародея
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Колпак и поля -->
    <path d="M54 4 C49 12 46 18 44 26 C40 40 36 52 31 66 L69 66 C66 50 62 34 64 20 C66 14 72 10 80 10 C70 6 62 4 54 4 Z" />
    <path d="M6 72 C28 63 72 63 94 72 C72 82 28 82 6 72 Z" />
    <template #cut>
      <!-- Звезда и лента -->
      <path d="M51 35 L53.1 41.1 L59.6 41.2 L54.4 45.1 L56.3 51.3 L51 47.6 L45.7 51.3 L47.6 45.1 L42.4 41.2 L48.9 41.1 Z" fill="#000" stroke="none" />
      <path d="M33 59 L67 59" />
    </template>
  </SplitIcon>
</template>
