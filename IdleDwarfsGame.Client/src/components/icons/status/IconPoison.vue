<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Яд: ядовитая капля с пузырьками
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#d4ff7a" mid="#4f9a0c" dark="#132a02" rim="#e4ffa8" outline="rgba(15, 40, 0, 0.7)">
    <path d="M50 15 C61 34 75 46 75 63 A25 25 0 0 1 25 63 C25 46 39 34 50 15 Z" fill="#e8ffc0" />
    <circle cx="43" cy="64" r="7" fill="#5aa616" stroke="none" />
    <circle cx="58" cy="54" r="4.5" fill="#5aa616" stroke="none" />
    <circle cx="57" cy="70" r="3" fill="#5aa616" stroke="none" />
  </StatusIconBase>
</template>
