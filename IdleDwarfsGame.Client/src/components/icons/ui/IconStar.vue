<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Рейтинг арены: звезда с внутренней гранью (раньше — ⭐)
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :primary-color="props.primaryColor ?? '#fff1b8'" :secondary-color="props.secondaryColor ?? '#ffc53d'">
    <path d="M50 5 L61.8 36.8 L95.7 38.2 L69 59.2 L78.2 91.8 L50 73 L21.8 91.8 L31 59.2 L4.3 38.2 L38.2 36.8 Z" />
    <template #cut>
      <path d="M50 23 L57.3 42.9 L78.5 43.7 L61.9 56.9 L67.6 77.3 L50 65.5 L32.4 77.3 L38.1 56.9 L21.5 43.7 L42.7 42.9 Z" />
    </template>
  </SplitIcon>
</template>
