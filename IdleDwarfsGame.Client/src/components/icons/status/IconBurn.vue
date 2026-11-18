<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Горение: язык пламени с горячей сердцевиной
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#ffc46b" mid="#e8430c" dark="#4a0b00" rim="#ffd98a" outline="rgba(60, 10, 0, 0.65)">
    <path d="M50 15 C57 30 73 38 73 59 C73 75 62 85 50 85 C38 85 27 75 27 59 C27 47 33 41 39 32 C41 42 44 46 47 48 C46 37 45 26 50 15 Z" fill="#ffeaa6" />
    <path d="M50 50 C55 57 61 62 61 70 C61 77 56 81 50 81 C44 81 39 77 39 70 C39 62 45 57 50 50 Z" fill="#ff7417" stroke="none" />
  </StatusIconBase>
</template>
