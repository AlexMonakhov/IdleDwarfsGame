<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import { RewardInfo } from '@/models/types/Inventory';
import { CHEST_ICONS, ELEMENT_COLORS, GEM_COLORS, RARITY_COLORS } from '@/components/inventory/lootStyles';
import { chestLabel } from '@/i18n';
import GemIcon from '@/components/icons/GemIcon.vue';
import { IconCoin, IconExperience } from '@/components/icons/ui';

// Награда одной строкой: золото, опыт, камни (цвет — камень, рамка — редкость) и сундуки (цвет — стихия)
const props = defineProps<{ reward: RewardInfo }>();

const { t, te, locale } = useI18n();

const format = (value: number) => value.toLocaleString(locale.value);
const gemName = (subType: string) => te(`gems.${subType}`) ? t(`gems.${subType}`) : subType;
</script>

<template>
    <div class="reward-line">
        <span><IconCoin /> {{ format(props.reward.gold) }}</span>
        <span><IconExperience /> {{ format(props.reward.experience) }}</span>
        <span
            v-for="item in props.reward.items"
            :key="`${item.subType}-${item.rarity}`"
            class="reward-gem"
            :style="{ '--gem-color': GEM_COLORS[item.subType] ?? '#cbd5e1', '--rarity-color': RARITY_COLORS[item.rarity] }"
            v-tooltip="`${gemName(item.subType)} · ${t(`rarity.${item.rarity}`)}`"
        >
            <GemIcon class="reward-gem-stone" :gem="item.subType" />×{{ item.quantity }}
        </span>
        <span
            v-for="(chest, index) in props.reward.chests"
            :key="`chest-${index}`"
            class="reward-chest"
            :style="{ '--chest-color': ELEMENT_COLORS[chest.element] ?? ELEMENT_COLORS.Basic }"
            v-tooltip="chestLabel(chest)"
        >
            <component :is="CHEST_ICONS[chest.type]" /> {{ chest.level }}
        </span>
    </div>
</template>

<style scoped>
.reward-line {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: center;
    gap: 4px 10px;
    font-size: 13px;
    font-weight: 700;
    text-shadow: 0 1px 3px #000, 0 0 6px #000;
}

.reward-gem,
.reward-chest {
    padding: 0 6px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.45);
}

.reward-gem {
    border: 1px solid var(--rarity-color);
}

.reward-gem-stone {
    color: var(--gem-color);
    filter: drop-shadow(0 0 4px var(--gem-color));
}

.reward-chest {
    border: 1px solid var(--chest-color);
    color: var(--chest-color);
}
</style>
