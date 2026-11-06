<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { SPELL_COLORS } from './spellColors';

// Lightning: молния
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? SPELL_COLORS.Lightning">
    <path d="M60 3 L20 55 L45 55 L33 97 L82 39 L55 39 L70 3 Z" />
  </SplitIcon>
</template>
