<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Герои и профиль: голова дворфа в рогатом шлеме с бородой
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Купол шлема с ободом и наносником -->
    <path d="M20 46 C20 24 33 11 50 11 C67 11 80 24 80 46 Z" />
    <path d="M17 44 L83 44 L83 52 L17 52 Z" />
    <path d="M45.5 50 L54.5 50 L54.5 63 L50 66 L45.5 63 Z" />
    <!-- Рога -->
    <path d="M24 32 C13 29 6 19 7 5 C13 15 20 20 28 22 Z" />
    <path d="M76 32 C87 29 94 19 93 5 C87 15 80 20 72 22 Z" />
    <!-- Лицо и борода -->
    <path d="M27 52 L73 52 L73 62 L27 62 Z" />
    <path d="M22 58 C24 76 34 90 50 97 C66 90 76 76 78 58 C70 66 60 68 50 68 C40 68 30 66 22 58 Z" />
    <template #cut>
      <!-- Глаза, усы и пряди бороды -->
      <path d="M36 57 L37 57" stroke-width="5" />
      <path d="M63 57 L64 57" stroke-width="5" />
      <path d="M28 63 C38 70 45 67 50 64 C55 67 62 70 72 63" />
      <path d="M38 76 L42 86" />
      <path d="M62 76 L58 86" />
      <!-- Обод шлема -->
      <path d="M20 43 L80 43" />
    </template>
  </SplitIcon>
</template>
