<script lang="ts" setup>
import StatusIconBase from './StatusIconBase.vue';

// Прилив энергии: молния и плюс — энергия вернулась
withDefaults(defineProps<{ size?: string }>(), { size: '16px' });
</script>

<template>
  <StatusIconBase :size="size" light="#9fe0ff" mid="#0a6fad" dark="#06233d" rim="#e0f4ff" outline="rgba(3, 25, 45, 0.7)">
    <path d="M50 14 L24 56 H42 L36 86 L67 42 H49 Z" fill="#ffe89a" />
    <path d="M70 20 H78 V30 H88 V38 H78 V48 H70 V38 H60 V30 H70 Z" fill="#f0fbff" />
  </StatusIconBase>
</template>
