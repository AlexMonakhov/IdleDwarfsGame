<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Hemorrhage: капля крови, рассечённая когтями
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Blood">
    <path d="M50 3 C63.248 30.599999999999998 83.12 49 83.12 61.88 C83.12 85.06400000000001 68.21600000000001 95 50 95 C31.784 95 16.880000000000003 85.06400000000001 16.880000000000003 61.88 C16.880000000000003 49 36.752 30.599999999999998 50 3 Z" />
    <template #cut>
      <!-- Три следа когтей -->
      <path d="M27 54 L58 85 M37 44 L68 75 M48 36 L75 63" stroke-width="5" />
    </template>
  </SplitIcon>
</template>
