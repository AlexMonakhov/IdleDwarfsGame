<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Выжигание энергии: молния в тёмном пламени
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#c8b4ff" mid="#5b26c4" dark="#170838" rim="#e2d8ff" outline="rgba(20, 5, 50, 0.7)">
    <path d="M50 14 C57 29 74 38 74 59 C74 75 63 86 50 86 C37 86 26 75 26 59 C26 47 32 41 38 32 C40 42 44 46 47 48 C46 37 45 25 50 14 Z" fill="#efe8ff" />
    <path d="M54 40 L39 62 H49 L45 79 L62 56 H52 L57 40 Z" fill="#4a1aa8" stroke="none" />
  </StatusIconBase>
</template>
