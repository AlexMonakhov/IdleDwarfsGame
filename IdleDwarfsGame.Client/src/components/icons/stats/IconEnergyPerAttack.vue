<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Энергия за удар: меч и выбитый им кристалл
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Меч как в мощи, остриём в правый верхний угол, сдвинут вниз-вправо, чтобы освободить место кристаллу -->
    <g transform="translate(8 8) rotate(45 50 50)">
      <path d="M50 1 L58 14 L58 63 L42 63 L42 14 Z" />
      <path d="M27 63 L73 63 L73 71 L27 71 Z" />
      <path d="M45 71 L55 71 L55 86 L45 86 Z" />
      <path d="M50 84 L57 91 L50 98 L43 91 Z" />
    </g>
    <!-- Кристалл в левом верхнем углу и осколки от удара -->
    <path d="M24 3 L37 16 L33 40 L24 46 L15 40 L11 16 Z" />
    <path d="M47 24 L51 29 L47 34 L43 29 Z" />
    <path d="M41 42 L44 45.5 L41 49 L38 45.5 Z" />
    <template #cut>
      <path transform="translate(8 8) rotate(45 50 50)" d="M50 15 L50 52" />
      <path d="M11 16 L19 20 L29 20 L37 16" />
    </template>
  </SplitIcon>
</template>
