<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Профессия секироносца: бородовидная секира
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Топорище, лезвие, обух -->
    <path d="M45 4 L54 4 L54 97 L45 97 Z" />
    <path d="M54 12 L67 9 C82 14 92 27 93 44 C86 53 76 60 67 72 C67 61 62 52 54 50 Z" />
    <path d="M45 20 L30 27 L45 35 Z" />
    <template #cut>
      <path d="M71 17 C81 23 86 32 87 42" />
      <path d="M43 62 L56 62 M43 70 L56 70" />
    </template>
  </SplitIcon>
</template>
