<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Мороз: снежинка — три перекладины с галочками на концах
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#b9f6ff" mid="#1d8fd6" dark="#06284a" rim="#d4fbff" outline="rgba(4, 30, 60, 0.65)">
    <path d="M46.5 18 H53.5 V82 H46.5 Z" />
    <path d="M46.5 18 H53.5 V82 H46.5 Z" transform="rotate(60 50 50)" />
    <path d="M46.5 18 H53.5 V82 H46.5 Z" transform="rotate(120 50 50)" />
    <path d="M50 34 L39 23 L43.5 18.5 L50 25 L56.5 18.5 L61 23 Z" />
    <path d="M50 34 L39 23 L43.5 18.5 L50 25 L56.5 18.5 L61 23 Z" transform="rotate(60 50 50)" />
    <path d="M50 34 L39 23 L43.5 18.5 L50 25 L56.5 18.5 L61 23 Z" transform="rotate(120 50 50)" />
    <path d="M50 34 L39 23 L43.5 18.5 L50 25 L56.5 18.5 L61 23 Z" transform="rotate(180 50 50)" />
    <path d="M50 34 L39 23 L43.5 18.5 L50 25 L56.5 18.5 L61 23 Z" transform="rotate(240 50 50)" />
    <path d="M50 34 L39 23 L43.5 18.5 L50 25 L56.5 18.5 L61 23 Z" transform="rotate(300 50 50)" />
    <circle cx="50" cy="50" r="7" />
  </StatusIconBase>
</template>
