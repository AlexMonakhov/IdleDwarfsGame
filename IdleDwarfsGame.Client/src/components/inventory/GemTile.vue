<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { ItemInfo } from '@/models/types/Inventory';
import { GEM_COLORS, RARITY_COLORS } from './lootStyles';
import CrystalIcon from '@/components/loading/CrystalIcon.vue';
import type { GemShape } from '@/components/loading/crystalGeometry';
import GlassButton from '@/components/common/GlassButton.vue';

// Стопка камней — карточка как у предмета (lootTile.css): стекло цвета ступени, камень, название, ступень и сколько штук
const props = defineProps<{ item: ItemInfo }>();

const { t, te } = useI18n();

// Камень — граненый кристалл загрузчика (целый). Огранка зависит от камня, чтобы стопки различались не только цветом
const CRYSTAL_SHAPES: Record<string, GemShape> = {
    Ruby: 'brilliant', Diamond: 'brilliant', Garnet: 'brilliant',
    Sapphire: 'crystal', Emerald: 'crystal', Topaz: 'crystal', Amethyst: 'crystal',
};
const gemColor = computed(() => GEM_COLORS[props.item.subType] ?? '#cbd5e1');
const gemShape = computed(() => CRYSTAL_SHAPES[props.item.subType] ?? 'brilliant');

const name = computed(() => te(`gems.${props.item.subType}`) ? t(`gems.${props.item.subType}`) : props.item.subType);
</script>

<template>
    <div class="loot-tile gem-tile" :class="`loot-tile-${item.rarity}`" :style="{ '--rarity-color': RARITY_COLORS[item.rarity] }">
        <GlassButton tag="div" class="loot-tile-glass">
            <span class="loot-tile-picture"><CrystalIcon :color="gemColor" :shape="gemShape" size="52px" /></span>
            <span class="loot-tile-name">{{ name }}</span>
            <span class="loot-tile-rarity">{{ t(`rarity.${item.rarity}`) }}</span>
        </GlassButton>
        <span class="loot-tile-count">×{{ item.quantity.toLocaleString() }}</span>
    </div>
</template>

<style scoped src="./lootTile.css"></style>
