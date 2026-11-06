<script lang="ts" setup>
import { STAT_ICONS } from './index';

// Значок характеристики под размер текущего шрифта: ставится в строку перед числом.
// stat — ключ из STAT_ICONS (power, hp, attack, speed, energy...)
withDefaults(defineProps<{ stat: string; size?: string }>(), { size: '1.2em' });
</script>

<template>
    <component :is="STAT_ICONS[stat]" v-if="STAT_ICONS[stat]" :size="size" />
</template>
