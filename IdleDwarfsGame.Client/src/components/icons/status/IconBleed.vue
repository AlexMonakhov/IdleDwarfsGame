<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Кровотечение: крупная капля и капля поменьше
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#ff8fa3" mid="#c8133f" dark="#3d0414" rim="#ffd0d9" outline="rgba(50, 0, 12, 0.7)">
    <path d="M43 16 C53 33 65 44 65 59 A22 22 0 0 1 21 59 C21 44 33 33 43 16 Z" fill="#ffe0e6" />
    <path d="M71 45 C75 53 80 58 80 65 A9 9 0 0 1 62 65 C62 58 67 53 71 45 Z" fill="#ffe0e6" />
    <path d="M32 58 C32 66 36 71 42 73" fill="none" stroke="#ff8fa3" stroke-width="3.5" paint-order="normal" />
  </StatusIconBase>
</template>
