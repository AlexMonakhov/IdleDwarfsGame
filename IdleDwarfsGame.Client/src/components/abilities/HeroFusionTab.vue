<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import GemButton from '@/components/common/GemButton.vue';
import HeroCard from '@/components/inventory/HeroCard.vue';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import { ELEMENT_ICONS } from '@/components/inventory/lootStyles';
import AbilitiesApiService from '@/services/apiServices/AbilitiesApiService';
import { AbilityActionResult, FUSION_REFUND, FUSION_SIZE, MAX_FUSION_RANK } from '@/models/types/Abilities';
import { useResources } from '@/composables/UseResources';
import { HeroInfo, isBusy } from '@/models/types/Heroes';
import { HeroElement } from '@/battle/types';
import { vDragScroll } from '@/directives/dragScroll';
import { abilityErrorText } from './abilityText';
import { IconCoin, IconExperience } from '@/components/icons/ui';

// Слияние: FUSION_SIZE дворфов одного ранга распыляются в одного рангом выше.
// Стихия — как у случайного из них, у дворфов 1-го ранга стихии нет — тогда любая поровну
const props = defineProps<{ heroes: HeroInfo[] }>();
const emit = defineEmits<{
    (e: 'fused', result: AbilityActionResult): void;
    // Сервер отказал — данные устарели, окну стоит перечитать их
    (e: 'stale'): void;
}>();

const { t } = useI18n();
const { set: setResources } = useResources();

const pickedIds = ref<string[]>([]);
const confirming = ref(false);
const busy = ref(false);
const message = ref<{ text: string; error: boolean } | null>(null);

const picked = computed(() => pickedIds.value
    .map(id => props.heroes.find(h => h.id === id))
    .filter((h): h is HeroInfo => !!h));
const rank = computed(() => picked.value[0]?.level ?? null);
const full = computed(() => picked.value.length === FUSION_SIZE);

// Дворфы, ушедшие из списка (распылили в другом окне), из выбора выпадают
watch(() => props.heroes, () => {
    pickedIds.value = pickedIds.value.filter(id => props.heroes.some(h => h.id === id));
});

watch(pickedIds, () => { confirming.value = false; }, { deep: true });

// Ранги по возрастанию: сливают обычно младших
const sorted = computed(() => [...props.heroes]
    .sort((a, b) => a.level - b.level || a.element.localeCompare(b.element) || a.name.localeCompare(b.name)));

const unavailable = (hero: HeroInfo) =>
    isBusy(hero)
    || hero.level >= MAX_FUSION_RANK
    || (rank.value !== null && hero.level !== rank.value && !pickedIds.value.includes(hero.id));

const reason = (hero: HeroInfo) => {
    if (isBusy(hero)) return t('abilities.busy');
    if (hero.level >= MAX_FUSION_RANK) return t('abilities.fusion.maxRank', { rank: hero.level });
    if (rank.value !== null && hero.level !== rank.value) return t('abilities.errors.DifferentRanks');
    return null;
};

const toggle = (hero: HeroInfo) => {
    message.value = null;
    if (pickedIds.value.includes(hero.id)) {
        pickedIds.value = pickedIds.value.filter(id => id !== hero.id);
        return;
    }
    if (unavailable(hero) || full.value) return;
    pickedIds.value = [...pickedIds.value, hero.id];
};

interface Chance {
    element: HeroElement;
    percent: number;
}

// Сколько вернётся за прокачку выбранных: та же доля, что считает сервер
const refund = computed(() => ({
    gold: Math.round(picked.value.reduce((sum, h) => sum + (h.upgrade.invested?.gold ?? 0), 0) * FUSION_REFUND),
    experience: Math.round(picked.value.reduce((sum, h) => sum + (h.upgrade.invested?.experience ?? 0), 0) * FUSION_REFUND),
}));

// Доля каждой стихии среди выбранных — её шанс
const chances = computed<Chance[]>(() => {
    const counts = new Map<HeroElement, number>();
    picked.value.filter(h => h.element !== 'Basic').forEach(h => counts.set(h.element, (counts.get(h.element) ?? 0) + 1));
    const total = [...counts.values()].reduce((sum, n) => sum + n, 0);
    return [...counts.entries()]
        .map(([element, count]) => ({ element, percent: Math.round((count / total) * 100) }))
        .sort((a, b) => b.percent - a.percent);
});

const fuse = async () => {
    if (!full.value || busy.value) return;
    if (!confirming.value) {
        confirming.value = true;
        return;
    }

    busy.value = true;
    try {
        const result = await AbilitiesApiService.fuse(pickedIds.value);
        const hero = result.hero;
        const returned = result.refund && (result.refund.gold > 0 || result.refund.experience > 0)
            ? ' ' + t('abilities.fusion.refunded', { gold: result.refund.gold, experience: result.refund.experience })
            : '';
        message.value = hero ? { text: t('abilities.fusion.done', { name: hero.name, rank: hero.level }) + returned, error: false } : null;
        if (result.resources) setResources(result.resources);
        pickedIds.value = [];
        emit('fused', result);
    } catch (error) {
        message.value = { text: abilityErrorText(error), error: true };
        emit('stale');
    } finally {
        busy.value = false;
        confirming.value = false;
    }
};

const buttonText = computed(() => {
    if (busy.value) return t('abilities.fusion.fusing');
    if (confirming.value) return t('abilities.fusion.confirm', { count: FUSION_SIZE });
    return t('abilities.fusion.fuse');
});
</script>

<template>
    <div class="forge">
        <aside class="forge-panel">
            <div class="forge-note">{{ t('abilities.fusion.hint', { count: FUSION_SIZE }) }}</div>

            <h4 class="forge-subtitle">{{ t('abilities.fusion.slots', { count: picked.length, total: FUSION_SIZE }) }}</h4>
            <div class="fusion-slots">
                <button
                    v-for="index in FUSION_SIZE"
                    :key="index"
                    class="fusion-slot"
                    :class="{ 'fusion-slot-filled': picked[index - 1] }"
                    :disabled="!picked[index - 1]"
                    v-tooltip="picked[index - 1]?.name"
                    @click="picked[index - 1] && toggle(picked[index - 1])"
                >
                    <UnitPortrait v-if="picked[index - 1]" :unit="picked[index - 1]" :size="48" :show-level="false" />
                    <span v-else class="fusion-slot-empty">+</span>
                </button>
            </div>

            <template v-if="rank !== null">
                <div class="forge-note">{{ t('abilities.fusion.result', { rank: rank + 1 }) }}</div>
                <h4 class="forge-subtitle">{{ t('abilities.fusion.chances') }}</h4>
                <div v-if="!chances.length" class="forge-note">{{ t('abilities.fusion.anyElement') }}</div>
                <div v-else class="fusion-chances">
                    <div v-for="chance in chances" :key="chance.element" class="fusion-chance">
                        <img v-if="ELEMENT_ICONS[chance.element]" :src="ELEMENT_ICONS[chance.element]" alt="" draggable="false" />
                        <span class="fusion-chance-name">{{ t(`elements.${chance.element}`) }}</span>
                        <span class="fusion-chance-value">{{ chance.percent }}%</span>
                    </div>
                </div>
            </template>

            <div v-if="refund.gold > 0 || refund.experience > 0" class="forge-note">
                <i18n-t keypath="abilities.fusion.refund" tag="span">
                    <template #percent>{{ FUSION_REFUND * 100 }}</template>
                    <template #gold><IconCoin /> {{ refund.gold }}</template>
                    <template #experience><IconExperience /> {{ refund.experience }}</template>
                </i18n-t>
            </div>
            <div class="forge-note forge-note-warn">{{ t('abilities.fusion.lost') }}</div>

            <div class="fusion-actions">
                <button class="forge-filter" :disabled="!picked.length || busy" @click="pickedIds = []">{{ t('abilities.fusion.clear') }}</button>
                <GemButton
                    class="forge-action"
                    :color="confirming ? 'red' : 'blue'"
                    :disabled="!full || busy"
                    @click="fuse"
                >{{ buttonText }}</GemButton>
            </div>

            <div v-if="message" class="forge-message" :class="{ 'forge-message-error': message.error }">{{ message.text }}</div>
        </aside>

        <section v-drag-scroll class="forge-list">
            <div class="forge-grid">
                <HeroCard
                    v-for="item in sorted"
                    :key="item.id"
                    v-tooltip="reason(item)"
                    :hero="item"
                    selectable
                    :disabled="unavailable(item)"
                    :selected="pickedIds.includes(item.id)"
                    @click="toggle(item)"
                />
            </div>
            <div v-if="!heroes.length" class="forge-note">{{ t('inventory.noHeroes') }}</div>
        </section>
    </div>
</template>

<style scoped src="./forge.css"></style>

<style scoped>
.fusion-slots {
    display: flex;
    justify-content: center;
    gap: 8px;
}

.fusion-slot {
    width: 54px;
    height: 54px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    border: 2px dashed rgba(157, 173, 175, 0.6);
    border-radius: 10px;
    background: rgba(0, 0, 0, 0.45);
    cursor: default;
}

.fusion-slot-filled {
    border: 2px solid #ffd166;
    box-shadow: 0 0 10px rgba(255, 209, 102, 0.45);
    cursor: pointer;
}

.fusion-slot-empty {
    color: rgba(203, 213, 225, 0.5);
    font-size: 22px;
    font-weight: bold;
}

.fusion-chances {
    display: flex;
    flex-direction: column;
    row-gap: 4px;
}

.fusion-chance {
    display: flex;
    align-items: center;
    column-gap: 8px;
    padding: 3px 0;
    border-bottom: 1px solid rgba(157, 173, 175, 0.2);
}

.fusion-chance img {
    width: 18px;
    height: 18px;
}

.fusion-chance-name {
    flex: 1;
    color: #f8fafc;
    font-size: 13px;
}

.fusion-chance-value {
    color: #ffd166;
    font-size: 14px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.fusion-actions {
    display: flex;
    align-items: center;
    justify-content: center;
    column-gap: 10px;
}

.fusion-actions .forge-action {
    min-width: 160px;
}
</style>
