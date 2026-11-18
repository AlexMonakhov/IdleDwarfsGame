<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Метка: прицел — кольцо, перекрестие и сердцевина
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#f7b8ff" mid="#a21caf" dark="#33063a" rim="#fbdcff" outline="rgba(45, 5, 50, 0.7)">
    <path d="M50 24 A26 26 0 1 0 50.01 24 Z M50 32 A18 18 0 1 1 49.99 32 Z" fill="#fff4ff" fill-rule="evenodd" />
    <path d="M46.5 12 H53.5 V30 H46.5 Z M46.5 70 H53.5 V88 H46.5 Z M12 46.5 H30 V53.5 H12 Z M70 46.5 H88 V53.5 H70 Z" fill="#fff4ff" />
    <circle cx="50" cy="50" r="7" fill="#f7b8ff" />
  </StatusIconBase>
</template>
