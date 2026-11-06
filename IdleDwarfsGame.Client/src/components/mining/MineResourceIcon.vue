<script lang="ts" setup>
import GemIcon from '@/components/icons/GemIcon.vue';
import { IconCoin, IconExperience } from '@/components/icons/ui';
import { MineType } from '@/models/types/Mining';

// Значок того, что добывает шахта: золото, опыт или камень стихии (пока нет картинок руды)
withDefaults(defineProps<{ type: MineType; size?: string }>(), { size: '1.2em' });
</script>

<template>
    <IconCoin v-if="type === 'Gold'" :size="size" />
    <IconExperience v-else-if="type === 'Experience'" :size="size" />
    <GemIcon v-else :gem="type" :size="size" />
</template>
