<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import GlassButton from '@/components/common/GlassButton.vue';
import { CHEST_ICONS, ELEMENT_COLORS, RARITY_COLORS } from '@/components/inventory/lootStyles';
import { RARITY_ORDER } from '@/models/types/Forge';
import type { ChestInfo } from '@/models/types/Inventory';

// Сундук во вложении письма и в каталоге админки — карточка как на складе (InventoryChests): стекло цвета стихии
// (у сундука бесконечности) или уровня, значок, название, уровень; amount — сколько сундуков
const props = defineProps<{ chest: Pick<ChestInfo, 'type' | 'level' | 'element'>; amount?: number | null }>();

const { t, locale } = useI18n();

// Уровень сундука — как редкость: 1 — обычный … 5 — легендарный
const rarity = computed(() => RARITY_ORDER[Math.min(Math.max(props.chest.level, 1), 5) - 1]);
const color = computed(() => (props.chest.element !== 'Basic' ? ELEMENT_COLORS[props.chest.element] : RARITY_COLORS[rarity.value]));
</script>

<template>
    <div class="loot-tile chest-tile" :class="`loot-tile-${rarity}`" :style="{ '--rarity-color': color }">
        <GlassButton tag="div" class="loot-tile-glass">
            <span class="loot-tile-picture"><component :is="CHEST_ICONS[chest.type]" size="52px" /></span>
            <span class="loot-tile-name">{{ t(`chests.types.${chest.type}`) }}</span>
            <span v-if="chest.element !== 'Basic'" class="chest-tile-element" :style="{ color }">{{ t(`elementNames.${chest.element}`) }}</span>
            <span class="loot-tile-rarity">{{ t('chests.level', { level: chest.level }) }}</span>
        </GlassButton>
        <span v-if="amount && amount > 1" class="loot-tile-count">×{{ amount.toLocaleString(locale) }}</span>
    </div>
</template>

<style scoped src="@/components/inventory/lootTile.css"></style>

<style scoped>
.chest-tile-element {
    font-size: 12px;
    font-weight: 700;
}
</style>
