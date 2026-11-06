<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';

// Слияние дворфов: дворф и шевроны — ранг выше
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props">
    <!-- Основа уменьшена и сдвинута влево-вниз, справа сверху — значок -->
    <path transform="translate(0 16) scale(0.84)" d="M20 46 C20 24 33 11 50 11 C67 11 80 24 80 46 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M17 44 L83 44 L83 52 L17 52 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M45.5 50 L54.5 50 L54.5 63 L50 66 L45.5 63 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M24 32 C13 29 6 19 7 5 C13 15 20 20 28 22 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M76 32 C87 29 94 19 93 5 C87 15 80 20 72 22 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M27 52 L73 52 L73 62 L27 62 Z" />
    <path transform="translate(0 16) scale(0.84)" d="M22 58 C24 76 34 90 50 97 C66 90 76 76 78 58 C70 66 60 68 50 68 C40 68 30 66 22 58 Z" />
    <template #cut>
      <path transform="translate(0 16) scale(0.84)" d="M36 57 L37 57" stroke-width="5" />
      <path transform="translate(0 16) scale(0.84)" d="M63 57 L64 57" stroke-width="5" />
      <path transform="translate(0 16) scale(0.84)" d="M28 63 C38 70 45 67 50 64 C55 67 62 70 72 63" />
      <path transform="translate(0 16) scale(0.84)" d="M38 76 L42 86" />
      <path transform="translate(0 16) scale(0.84)" d="M62 76 L58 86" />
      <path transform="translate(0 16) scale(0.84)" d="M20 43 L80 43" />
      <!-- Место под значок -->
      <path d="M56 20 A24 24 0 1 0 104 20 A24 24 0 1 0 56 20 Z" fill="#000" stroke="none" />
    </template>
    <template #badge>
      <!-- Два шеврона: ранг выше -->
      <path d="M64 22 L80 6 L96 22 L96 31 L80 15 L64 31 Z" />
      <path d="M64 36 L80 20 L96 36 L96 45 L80 29 L64 45 Z" />
    </template>
  </SplitIcon>
</template>
