<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import GemIcon from '@/components/icons/GemIcon.vue';
import GemTile from '@/components/inventory/GemTile.vue';
import GemFilters from '@/components/inventory/GemFilters.vue';
import ForgeApiService from '@/services/apiServices/ForgeApiService';
import { RARITY_COLORS } from '@/components/inventory/lootStyles';
import { vDragScroll } from '@/directives/dragScroll';
import type { ItemInfo, Rarity } from '@/models/types/Inventory';
import { canForgeFrom, ForgeOverview, nextRarity, RARITY_ORDER } from '@/models/types/Forge';
import { forgeErrorText } from './forgeText';

// Сплав камней: gemFusionSize камней одной ступени — в один камень того же вида ступенью выше.
// Можно сразу несколько раз; выше maxRarity не сплавляется
const props = defineProps<{ forge: ForgeOverview }>();
const emit = defineEmits<{
    (e: 'changed', forge: ForgeOverview): void;
    (e: 'stale'): void;
}>();

const { t, locale } = useI18n();

const size = computed(() => props.forge.gemFusionSize);

const format = (value: number) => value.toLocaleString(locale.value);

const quantity = (gem: string, rarity: Rarity) =>
    props.forge.gems.find(g => g.gem === gem && g.rarity === rarity)?.quantity ?? 0;

// Список как на складе (InventoryGems): те же карточки, сначала редкие ступени
// Фильтры по камню и ступени
const gemFilter = ref<string | null>(null);
const rarityFilter = ref<Rarity | null>(null);

const stacks = computed<ItemInfo[]>(() => props.forge.gems
    .filter(g => (!gemFilter.value || g.gem === gemFilter.value) && (!rarityFilter.value || g.rarity === rarityFilter.value))
    .map(g => ({ itemType: 'Gem' as const, subType: g.gem, rarity: g.rarity, level: 1, quantity: g.quantity }))
    .sort((a, b) => RARITY_ORDER.indexOf(b.rarity) - RARITY_ORDER.indexOf(a.rarity) || a.subType.localeCompare(b.subType)));

const selected = ref<{ gem: string; rarity: Rarity } | null>(null);
const times = ref(1);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const maxTimes = computed(() => (selected.value ? Math.floor(quantity(selected.value.gem, selected.value.rarity) / size.value) : 0));
const target = computed(() => (selected.value ? nextRarity(selected.value.rarity) : null));

const canSelect = (gem: string, rarity: Rarity) =>
    canForgeFrom(rarity, props.forge.maxRarity) && quantity(gem, rarity) >= size.value;

const select = (gem: string, rarity: Rarity) => {
    if (!canSelect(gem, rarity)) return;
    message.value = null;
    selected.value = { gem, rarity };
    times.value = 1;
};

// Стопку потратили или камней стало меньше — держим число слияний в пределах доступного
watch(maxTimes, max => {
    if (max <= 0) selected.value = null;
    else times.value = Math.min(Math.max(1, times.value), max);
});

// Почему стопку нельзя сплавить — подсказка на затемнённой карточке
const reason = (gem: string, rarity: Rarity) => {
    if (!canForgeFrom(rarity, props.forge.maxRarity)) return t('forge.gems.top');
    if (quantity(gem, rarity) < size.value) return t('forge.gems.needMore', { count: size.value });
    return undefined;
};

const fuse = async () => {
    if (!selected.value || busy.value || times.value < 1) return;
    busy.value = true;
    try {
        const { gem, rarity } = selected.value;
        const result = await ForgeApiService.fuseGems(gem, rarity, times.value);
        message.value = {
            text: t('forge.gems.done', { count: result.created, rarity: t(`rarity.${result.rarity}`), gem: t(`gems.${result.gem}`) }),
            error: false,
        };
        emit('changed', result.forge);
    } catch (error) {
        message.value = { text: forgeErrorText(error), error: true };
        emit('stale');
    } finally {
        busy.value = false;
    }
};
</script>

<template>
    <div class="forge">
        <aside class="forge-panel">
            <div class="forge-note">{{ t('forge.gems.hint', { count: size }) }}</div>

            <template v-if="selected && target">
                <h4 class="forge-subtitle">{{ t('forge.gems.recipe') }}</h4>
                <div class="gem-recipe">
                    <span class="gem-recipe-part" :style="{ '--rarity-color': RARITY_COLORS[selected.rarity] }">
                        <GemIcon :gem="selected.gem" size="34px" />
                        <span class="gem-recipe-count">×{{ format(size * times) }}</span>
                        <span class="gem-recipe-name">{{ t(`rarity.${selected.rarity}`) }}</span>
                    </span>
                    <span class="gem-recipe-arrow">→</span>
                    <span class="gem-recipe-part" :style="{ '--rarity-color': RARITY_COLORS[target] }">
                        <GemIcon :gem="selected.gem" size="34px" />
                        <span class="gem-recipe-count">×{{ format(times) }}</span>
                        <span class="gem-recipe-name">{{ t(`rarity.${target}`) }}</span>
                    </span>
                </div>
                <div class="forge-note">{{ t(`gems.${selected.gem}`) }} · {{ t('forge.gems.owned', { count: format(quantity(selected.gem, selected.rarity)) }) }}</div>

                <!-- Сколько раз сплавить -->
                <div class="gem-times">
                    <button class="forge-filter" :disabled="times <= 1 || busy" @click="times--">−</button>
                    <span class="gem-times-value">{{ times }}</span>
                    <button class="forge-filter" :disabled="times >= maxTimes || busy" @click="times++">+</button>
                    <button class="forge-filter" :disabled="times >= maxTimes || busy" @click="times = maxTimes">{{ t('forge.gems.max', { count: maxTimes }) }}</button>
                </div>

                <div class="forge-actions">
                    <GemButton class="forge-action" :color="'blue'" :disabled="busy" @click="fuse">
                        {{ busy ? t('forge.gems.fusing') : t('forge.gems.fuse') }}
                    </GemButton>
                </div>
            </template>
            <div v-else class="forge-note forge-note-warn">{{ t('forge.gems.pick') }}</div>

            <div class="forge-note">{{ t('forge.gems.top') }}</div>
            <div v-if="message" class="forge-message" :class="{ 'forge-message-error': message.error }">{{ message.text }}</div>
        </aside>

        <section v-drag-scroll class="forge-list">
            <GemFilters v-model:gem="gemFilter" v-model:rarity="rarityFilter" :gems="forge.gems" />
            <!-- Карточки как на складе; не хватает на сплав (или ступень уже высшая) — затемнены и не выбираются -->
            <div v-if="stacks.length" class="forge-grid">
                <button
                    v-for="stack in stacks"
                    :key="`${stack.subType}-${stack.rarity}`"
                    type="button"
                    class="forge-gem"
                    :class="{
                        'forge-gem-picked': selected?.gem === stack.subType && selected?.rarity === stack.rarity,
                        'forge-gem-off': !canSelect(stack.subType, stack.rarity),
                    }"
                    v-tooltip="reason(stack.subType, stack.rarity)"
                    @click="select(stack.subType, stack.rarity)"
                >
                    <GemTile :item="stack" />
                </button>
            </div>
            <div v-else class="forge-note">{{ forge.gems.length ? t('lootFilters.nothing') : t('forge.gems.empty') }}</div>
        </section>
    </div>
</template>

<style scoped src="@/components/abilities/forge.css"></style>
<style scoped src="./forgeShared.css"></style>

<style scoped>
.gem-recipe {
    display: flex;
    justify-content: center;
    align-items: center;
    column-gap: 10px;
}

.gem-recipe-part {
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 2px;
    min-width: 86px;
    padding: 8px 6px;
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.4);
    box-shadow: 0 0 0 1px color-mix(in srgb, var(--rarity-color) 60%, transparent);
}

.gem-recipe-count {
    color: #f8fafc;
    font-size: 16px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.gem-recipe-name {
    color: var(--rarity-color);
    font-size: 12px;
    font-weight: bold;
}

.gem-recipe-arrow {
    color: #ffd166;
    font-size: 22px;
    font-weight: bold;
}

.gem-times {
    display: flex;
    justify-content: center;
    align-items: center;
    column-gap: 6px;
}

.gem-times-value {
    min-width: 40px;
    color: #f8fafc;
    font-size: 18px;
    font-weight: bold;
    text-align: center;
    font-variant-numeric: tabular-nums;
}

/* Карточка в списке — кнопка вокруг GemTile: выбрана — золотая кайма, не хватает на сплав — затемнена */
.forge-gem {
    display: flex;
    padding: 0;
    border: none;
    border-radius: 16px;
    background: none;
    cursor: pointer;
    transition: box-shadow 0.15s ease, opacity 0.15s ease;
}

.forge-gem-picked {
    box-shadow: 0 0 0 3px #ffd166, 0 0 14px 3px rgba(255, 209, 102, 0.5);
}

.forge-gem-off {
    opacity: 0.4;
    filter: grayscale(0.6) brightness(0.8);
    cursor: not-allowed;
}
</style>
