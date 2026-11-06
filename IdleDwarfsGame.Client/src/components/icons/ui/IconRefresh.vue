<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Пересчитать (повторная разведка): кольцо со стрелкой по часовой (раньше — ↻)
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Кольцо от правого верха по часовой почти до макушки: внешний радиус 38, внутренний 24 -->
    <path d="M82.9 31 A38 38 0 1 1 46.7 12.2 L47.9 26.1 A24 24 0 1 0 70.8 38 Z" />
    <!-- Наконечник на макушке смотрит по ходу часовой — в зазор кольца -->
    <path d="M46 4.2 L69 17.4 L48.6 34.1 Z" />
    <template #cut>
      <!-- Риска по середине кольца -->
      <path d="M20.9 60.6 A31 31 0 0 0 39.4 79.1" />
    </template>
  </SplitIcon>
</template>
