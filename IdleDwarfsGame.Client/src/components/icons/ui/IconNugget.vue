<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Самородок — валюта ивента «Общие шахты»: угловатый слиток с гранями
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :primary-color="props.primaryColor ?? '#fef3c7'" :secondary-color="props.secondaryColor ?? '#d97706'">
    <path d="M14 58 L26 30 L48 18 L74 24 L90 46 L84 72 L58 86 L28 82 Z" />
    <template #cut>
      <!-- Грани -->
      <path d="M26 30 L44 46 L74 24 M44 46 L58 86 M44 46 L14 58 M44 46 L90 46" />
    </template>
  </SplitIcon>
</template>
