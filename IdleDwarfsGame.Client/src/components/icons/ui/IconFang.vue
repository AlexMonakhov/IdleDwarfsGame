<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Клык — валюта ивента «Мировые боссы»: изогнутый клык с корнем
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :primary-color="props.primaryColor ?? '#f5f5f4'" :secondary-color="props.secondaryColor ?? '#a8a29e'">
    <path d="M22 8 L78 8 L74 30 C70 52 60 74 40 94 C42 72 36 52 26 30 Z" />
    <template #cut>
      <!-- Корень и трещина эмали -->
      <path d="M24 24 L76 24" />
      <path d="M52 36 L46 52 L54 62" />
    </template>
  </SplitIcon>
</template>
