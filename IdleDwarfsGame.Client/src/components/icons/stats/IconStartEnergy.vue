<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Энергия в начале боя: тот же кристалл, что у энергии, заряженный наполовину
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <path d="M50 3 L77 30 L69 86 L50 97 L31 86 L23 30 Z" />
    <template #cut>
      <!-- Пустая верхняя часть: внутренний кристалл до волнистой границы заряда -->
      <path
        d="M50 15 L67.5 32 L65 54 C59 46 55 60 50 54 C45 48 41 62 35 54 L32.5 32 Z"
        fill="#000"
        stroke="none"
      />
      <!-- Нижние грани заряженной части -->
      <path d="M40 64 L46 88" />
      <path d="M60 64 L54 88" />
    </template>
  </SplitIcon>
</template>
