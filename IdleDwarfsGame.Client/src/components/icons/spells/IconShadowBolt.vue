<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Shadow Bolt: тёмная сфера со шлейфом
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Dark">
    <!-- Шлейф -->
    <path d="M32 54 C22 62 14 74 4 79 C18 82 31 76 40 64 Z" />
    <path d="M40 68 C33 79 28 89 17 96 C32 96 45 87 49 74 Z" />
    <!-- Сфера -->
    <path d="M26 44 A32 32 0 1 0 90 44 A32 32 0 1 0 26 44 Z" />
    <template #cut>
      <!-- Завиток внутри -->
      <path d="M58 44 C58 38 66 38 66 44 C66 53 53 55 49 46 C45 36 57 27 69 31 C80 35 84 50 76 60" />
    </template>
  </SplitIcon>
</template>
