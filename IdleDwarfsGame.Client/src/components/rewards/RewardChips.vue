<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { CHEST_ICONS } from '@/components/inventory/lootStyles';
import { chestLabel } from '@/i18n';
import { IconCoin, IconElderSeal, IconExperience, IconRainbowCoin, IconStar } from '@/components/icons/ui';
import type { RewardInfo } from '@/models/types/Inventory';
import type { FrameEffect } from '@/models/types/Account';

// Награда значками: радужные монеты, печати, сундуки, золото и опыт, рамка аватара.
// compact — без опыта (в клетке календаря места мало)
const props = withDefaults(defineProps<{
    reward: RewardInfo;
    rainbowCoins?: number;
    elderSeals?: number;
    frame?: FrameEffect | null;
    compact?: boolean;
}>(), { rainbowCoins: 0, elderSeals: 0, frame: null, compact: false });

const { t, locale } = useI18n();
const format = (value: number) => value.toLocaleString(locale.value);

// Одинаковые сундуки — одним значком с числом
const chests = computed(() => {
    const groups = new Map<string, { chest: RewardInfo['chests'][number]; count: number }>();
    for (const chest of props.reward.chests) {
        const key = `${chest.type}:${chest.level}:${chest.element}`;
        const group = groups.get(key);
        if (group) group.count++;
        else groups.set(key, { chest, count: 1 });
    }
    return [...groups.values()];
});
</script>

<template>
    <span class="reward-chips" :class="{ 'reward-chips-compact': compact }">
        <span v-if="frame" class="reward-chip reward-chip-frame" v-tooltip="t('achievements.frameReward', { frame: t(`account.frames.${frame}`) })">
            <IconStar /> {{ t(`account.frames.${frame}`) }}
        </span>
        <span v-if="rainbowCoins" class="reward-chip reward-chip-rainbow" v-tooltip="t('shop.currency.rainbowCoins')">
            <IconRainbowCoin /> {{ format(rainbowCoins) }}
        </span>
        <span v-for="group in chests" :key="`${group.chest.type}${group.chest.level}`" class="reward-chip" v-tooltip="chestLabel(group.chest)">
            <component :is="CHEST_ICONS[group.chest.type]" /><template v-if="group.count > 1">×{{ group.count }}</template>
        </span>
        <span v-if="elderSeals" class="reward-chip reward-chip-seal" v-tooltip="t('resources.elderSeals')">
            <IconElderSeal /> {{ format(elderSeals) }}
        </span>
        <span v-if="reward.gold" class="reward-chip reward-chip-gold" v-tooltip="t('resources.gold')">
            <IconCoin /> {{ format(reward.gold) }}
        </span>
        <span v-if="reward.experience && !compact" class="reward-chip reward-chip-exp" v-tooltip="t('resources.experience')">
            <IconExperience /> {{ format(reward.experience) }}
        </span>
    </span>
</template>

<style scoped>
.reward-chips {
    display: inline-flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 4px 10px;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.reward-chips-compact {
    justify-content: center;
    gap: 2px 6px;
    font-size: 12px;
}

.reward-chip {
    display: inline-flex;
    align-items: center;
    column-gap: 3px;
    white-space: nowrap;
}

.reward-chip-rainbow { color: #f5d0fe; }
.reward-chip-seal { color: #fed7aa; }
.reward-chip-gold { color: #ffd166; }
.reward-chip-exp { color: #a5f3fc; }
.reward-chip-frame {
    color: #fde68a;
    --split-icon-primary: #fde68a;
}
</style>
