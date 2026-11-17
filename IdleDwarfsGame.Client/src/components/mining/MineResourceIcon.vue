<script lang="ts" setup>
import CrystalIcon from '@/components/loading/CrystalIcon.vue';
import { IconCoin, IconExperience } from '@/components/icons/ui';
import { GEM_COLORS, GEM_SHAPES } from '@/components/inventory/lootStyles';
import { MineType } from '@/models/types/Mining';

// Значок того, что добывает шахта: золото, опыт или кристалл стихии (пока нет картинок руды).
// Кристалл — вариант «наливается» (heal): progress — доля следующего камня, цвет поднимается снизу волной.
// Без progress камень целый
withDefaults(defineProps<{ type: MineType; size?: string; progress?: number }>(), { size: '1.2em', progress: 1 });
</script>

<template>
    <IconCoin v-if="type === 'Gold'" :size="size" />
    <IconExperience v-else-if="type === 'Experience'" :size="size" />
    <CrystalIcon
        v-else
        class="mine-crystal"
        variant="heal"
        :shape="GEM_SHAPES[type] ?? 'crystal'"
        :color="GEM_COLORS[type] ?? '#cbd5e1'"
        :progress="progress"
        :size="size"
    />
</template>

<style scoped>
.mine-crystal {
    display: inline-block;
    vertical-align: middle;
}
</style>
