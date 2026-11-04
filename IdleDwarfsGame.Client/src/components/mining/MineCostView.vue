<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import { GEM_COLORS } from '@/components/inventory/lootStyles';
import { MineCost, MiningWallet } from '@/models/types/Mining';

// Цена в шахтах: золото, опыт и кристаллы; чего не хватает — красным
const props = defineProps<{ cost: MineCost; wallet: MiningWallet | null }>();

const { t, locale } = useI18n();

const format = (value: number) => value.toLocaleString(locale.value);

const lacks = (have: number | undefined, need: number) => (have ?? 0) < need;
</script>

<template>
    <span class="mine-cost">
        <span
            v-if="cost.gold"
            class="mine-cost-gold"
            :class="{ 'mine-cost-lacking': lacks(props.wallet?.gold, cost.gold) }"
            v-tooltip="t('resources.gold')"
        >🪙 {{ format(cost.gold) }}</span>
        <span
            v-if="cost.experience"
            class="mine-cost-experience"
            :class="{ 'mine-cost-lacking': lacks(props.wallet?.experience, cost.experience) }"
            v-tooltip="t('resources.experience')"
        >✦ {{ format(cost.experience) }}</span>
        <span
            v-if="cost.gem && cost.crystals"
            class="mine-cost-crystals"
            :class="{ 'mine-cost-lacking': lacks(props.wallet?.crystals[cost.gem], cost.crystals) }"
            :style="{ '--gem-color': GEM_COLORS[cost.gem] }"
            v-tooltip="t(`gems.${cost.gem}`)"
        ><span class="mine-cost-gem">◆</span> {{ format(cost.crystals) }}</span>
    </span>
</template>

<style scoped>
.mine-cost {
    display: inline-flex;
    align-items: baseline;
    column-gap: 10px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.mine-cost-gold {
    color: #ffd166;
}

.mine-cost-experience {
    color: #a5f3fc;
}

.mine-cost-crystals {
    color: #f8fafc;
}

.mine-cost-gem {
    color: var(--gem-color);
}

.mine-cost-lacking,
.mine-cost-lacking .mine-cost-gem {
    color: #ff8a8a;
}
</style>
