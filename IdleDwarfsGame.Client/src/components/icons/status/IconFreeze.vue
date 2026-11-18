<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Заморозка: юнит в ледяной глыбе — кубик льда с гранями разной яркости
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#d6f6ff" mid="#3d9fd6" dark="#082c47" rim="#e6fbff" outline="rgba(4, 30, 55, 0.65)">
    <path d="M50 17 L79 32 L79 66 L50 83 L21 66 L21 32 Z" fill="#a8e6ff" />
    <path d="M50 17 L79 32 L50 47 L21 32 Z" fill="#f0fcff" stroke="none" />
    <path d="M79 32 L79 66 L50 83 L50 47 Z" fill="#5fc4f2" stroke="none" />
    <path d="M29 41 L29 55 M35 44 L35 50" stroke="#ffffff" stroke-width="3" fill="none" paint-order="normal" />
  </StatusIconBase>
</template>
