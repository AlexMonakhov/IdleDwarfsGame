<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Шок: молния с искрами
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#fff27a" mid="#d99a06" dark="#3d2400" rim="#fff7b8" outline="rgba(55, 30, 0, 0.7)">
    <path d="M58 13 L27 55 H46 L39 87 L73 42 H54 L63 13 Z" fill="#fffbe0" />
    <circle cx="26" cy="30" r="4" fill="#fffbe0" />
    <circle cx="76" cy="70" r="4" fill="#fffbe0" />
  </StatusIconBase>
</template>
