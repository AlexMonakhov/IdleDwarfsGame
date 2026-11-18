<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Регенерация: крест лечения и искорки
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#8ff5c8" mid="#0a9a62" dark="#022a1c" rim="#c4fbe2" outline="rgba(0, 40, 25, 0.65)">
    <path d="M41 20 H59 V41 H80 V59 H59 V80 H41 V59 H20 V41 H41 Z" fill="#f0fff7" />
    <polygon points="77.0,15.0 78.8,20.2 84.0,22.0 78.8,23.8 77.0,29.0 75.2,23.8 70.0,22.0 75.2,20.2" fill="#f0fff7" stroke="none" />
    <polygon points="23.0,73.0 24.4,76.6 28.0,78.0 24.4,79.4 23.0,83.0 21.6,79.4 18.0,78.0 21.6,76.6" fill="#f0fff7" stroke="none" />
  </StatusIconBase>
</template>
