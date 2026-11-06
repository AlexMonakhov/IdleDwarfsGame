<script lang="ts" setup>
import SplitIcon from '../SplitIcon.vue';
import type { SplitIconProps } from '../splitIcon';
import { PASSIVE_COLOR } from './passiveColor';

// SharpTongue: раздвоенный язык, поджигающий удар
const props = defineProps<SplitIconProps>();
</script>

<template>
  <SplitIcon v-bind="props" :secondary-color="props.secondaryColor ?? PASSIVE_COLOR">
    <!-- Язык и его раздвоенный кончик -->
    <path d="M41 97 C37 79 57 71 53 55 C50 45 43 41 45 31 L55 31 C53 41 60 45 63 55 C67 73 47 81 51 97 Z" />
    <path d="M46 33 L27 8 L36 5 L51 27 Z" />
    <path d="M54 33 L73 8 L64 5 L49 27 Z" />
    <!-- Пламя -->
    <path transform="translate(62 56) scale(0.38)" d="M52 4 C56 18 66 24 70 36 C73 28 72 21 69 15 C83 24 91 41 89 60 C87 81 71 95 50 95 C29 95 13 81 11 60 C10 46 16 34 25 27 C24 35 26 40 31 45 C30 29 39 15 52 4 Z" />
  </SplitIcon>
</template>
