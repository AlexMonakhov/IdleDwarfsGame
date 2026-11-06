<script lang="ts" setup>
import { computed } from 'vue';
import IconGem from './ui/IconGem.vue';
import { GEM_COLORS } from '@/components/inventory/lootStyles';

// Камень стихии (рубин, сапфир...) — самоцвет его цвета: правая половина — сам камень, левая — он же, но светлее.
// gem — GemType с сервера («Ruby»); неизвестный камень серый
const props = withDefaults(defineProps<{ gem: string; size?: string }>(), { size: '1.2em' });

const color = computed(() => GEM_COLORS[props.gem] ?? '#cbd5e1');
const light = computed(() => `color-mix(in srgb, ${color.value} 45%, white)`);
</script>

<template>
    <IconGem :size="size" :primary-color="light" :secondary-color="color" />
</template>
