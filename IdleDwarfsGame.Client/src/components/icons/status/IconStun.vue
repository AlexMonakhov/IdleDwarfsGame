<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Оглушение: звёзды над головой на орбите
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#ffe48a" mid="#d27a06" dark="#3d1a02" rim="#fff1c2" outline="rgba(55, 25, 0, 0.7)">
    <ellipse cx="50" cy="52" rx="33" ry="13" fill="none" stroke="#fff6dc" stroke-width="3.5" stroke-dasharray="7 5" transform="rotate(-15 50 52)" paint-order="normal" />
    <polygon points="50.0,22.0 54.4,33.9 67.1,34.4 57.1,42.3 60.6,54.6 50.0,47.5 39.4,54.6 42.9,42.3 32.9,34.4 45.6,33.9" fill="#fffbeb" />
    <polygon points="76.0,49.0 78.2,54.9 84.6,55.2 79.6,59.2 81.3,65.3 76.0,61.8 70.7,65.3 72.4,59.2 67.4,55.2 73.8,54.9" fill="#fff1c2" />
    <polygon points="25.0,56.0 27.0,61.2 32.6,61.5 28.2,65.1 29.7,70.5 25.0,67.4 20.3,70.5 21.8,65.1 17.4,61.5 23.0,61.2" fill="#fff1c2" />
  </StatusIconBase>
</template>
