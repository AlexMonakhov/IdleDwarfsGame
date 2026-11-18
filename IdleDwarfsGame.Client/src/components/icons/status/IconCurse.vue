<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Проклятие: череп
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#e2c2ff" mid="#7b25c9" dark="#22073f" rim="#eedcff" outline="rgba(30, 5, 55, 0.7)">
    <path d="M50 17 C33 17 23 29 23 44 C23 54 28 60 35 63 V73 C35 77 37 80 41 80 H59 C63 80 65 77 65 73 V63 C72 60 77 54 77 44 C77 29 67 17 50 17 Z" fill="#f6edff" />
    <circle cx="39.5" cy="45" r="7.5" fill="#3b0b6b" stroke="none" />
    <circle cx="60.5" cy="45" r="7.5" fill="#3b0b6b" stroke="none" />
    <path d="M50 54 L45.5 62 H54.5 Z" fill="#3b0b6b" stroke="none" />
    <path d="M44 70 V79 M50 70 V79 M56 70 V79" stroke="#3b0b6b" stroke-width="2.5" fill="none" paint-order="normal" />
  </StatusIconBase>
</template>
