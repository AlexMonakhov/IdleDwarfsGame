<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import GlassButton from '@/components/common/GlassButton.vue';
import ItemTile from '@/components/equipment/ItemTile.vue';
import AbilityTile from '@/components/abilities/AbilityTile.vue';
import { ELEMENT_COLORS, ELEMENT_ICONS } from '@/components/inventory/lootStyles';
import type { DelveRewardInfo } from '@/models/types/Social';

// Награда узла спуска. Предмет — плиткой склада, спелл / пассивка / аура — плиткой способности алтаря
// (те же компоненты), стихия, ранг и улучшения — своей карточкой в том же стиле
const props = defineProps<{
    reward: DelveRewardInfo;
    selected?: boolean;
    disabled?: boolean;
}>();

const emit = defineEmits<{ (e: 'pick'): void }>();

const { t } = useI18n();

const UPGRADE_ICONS: Partial<Record<DelveRewardInfo['kind'], string>> = {
    LevelUp: '⭐',
    SpellUp: '✨',
    PassiveUp: '📜',
    AuraUp: '🌀',
};

const elementName = computed(() => (props.reward.element ? t(`elementNames.${props.reward.element}`) : ''));
const accent = computed(() => (props.reward.element ? ELEMENT_COLORS[props.reward.element] : '#ffd166'));

const plainTitle = computed(() =>
    props.reward.kind === 'Element' ? elementName.value : t(`social.delve.rewardKinds.${props.reward.kind}`));

const plainText = computed(() => {
    switch (props.reward.kind) {
        case 'Element': return t('social.delve.rewardElement');
        case 'LevelUp': return t('social.delve.rewardLevelUp', { element: elementName.value });
        case 'SpellUp': return t('social.delve.rewardSpellUp');
        case 'PassiveUp': return t('social.delve.rewardPassiveUp');
        case 'AuraUp': return t('social.delve.rewardAuraUp');
        default: return '';
    }
});

// Куда пойдёт награда: спелл этажей 6–10 — сам первому дворфу стихии, остальное — на выбранного
const hint = computed(() => {
    const r = props.reward;
    if (r.kind === 'Spell' && !r.needsTarget) return t('social.delve.rewardSpell', { element: elementName.value });
    if (r.kind === 'Spell' || r.kind === 'Aura') return t('social.delve.onlyElement', { element: elementName.value });
    return '';
});
</script>

<template>
    <div
        class="delve-reward"
        :class="{ 'delve-reward-selected': selected, 'delve-reward-disabled': disabled }"
        @click="!disabled && emit('pick')"
    >
        <!-- Предмет — плитка склада (описание в её подсказке), спелл / пассивка / аура — плитка алтаря -->
        <div v-if="reward.item" class="delve-reward-tile">
            <ItemTile :item="reward.item" />
        </div>
        <div v-else-if="reward.ability" class="delve-reward-tile">
            <AbilityTile :ability="reward.ability" :selected="selected" />
        </div>

        <!-- Стихия, ранг, улучшения -->
        <div v-else class="delve-reward-plain" :style="{ '--reward-accent': accent }">
            <GlassButton tag="div" class="delve-reward-plain-glass" :active="selected">
                <span class="delve-reward-kind">{{ t(`social.delve.rewardKinds.${reward.kind}`) }}</span>
                <img v-if="reward.kind === 'Element' && reward.element && ELEMENT_ICONS[reward.element]" class="delve-reward-icon-img" :src="ELEMENT_ICONS[reward.element]" alt="" />
                <span v-else class="delve-reward-icon">{{ UPGRADE_ICONS[reward.kind] ?? '💠' }}</span>
                <span class="delve-reward-title">{{ plainTitle }}</span>
                <span class="delve-reward-text">{{ plainText }}</span>
            </GlassButton>
        </div>

        <!-- Место под подсказку есть всегда: плитки в ряду остаются одной высоты -->
        <span class="delve-reward-hint">{{ hint }}</span>
    </div>
</template>

<style scoped>
/* Обёртка без своей рамки: карточка выглядит как на складе, выбор — свечением вокруг плитки */
.delve-reward {
    width: 148px;
    display: flex;
    flex-direction: column;
    align-items: stretch;
    gap: 6px;
    cursor: pointer;
    font-family: 'Arial', sans-serif;
}

/* Одинаковый размер: плитка занимает всю ширину и высоту карточки */
.delve-reward-tile,
.delve-reward-plain {
    flex: 1;
    display: flex;
    border-radius: 14px;
    transition: box-shadow 0.15s ease, transform 0.15s ease;
}

.delve-reward:hover .delve-reward-tile,
.delve-reward:hover .delve-reward-plain {
    transform: translateY(-2px);
}

.delve-reward-selected .delve-reward-tile,
.delve-reward-selected .delve-reward-plain {
    box-shadow: 0 0 0 2px #ffd166, 0 0 18px 4px rgba(255, 209, 102, 0.6);
}

.delve-reward-disabled {
    cursor: default;
    opacity: 0.6;
}

.delve-reward-disabled:hover .delve-reward-tile,
.delve-reward-disabled:hover .delve-reward-plain {
    transform: none;
}

.delve-reward-tile > :deep(*),
.delve-reward-tile :deep(.item-tile),
.delve-reward-tile :deep(.ability-tile) {
    flex: 1;
    display: flex;
    width: 100%;
}

.delve-reward-hint {
    min-height: 28px;
    color: #cbd5e1;
    font-size: 11px;
    text-align: center;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.7);
}

/* Своя карточка — оправа и стекло как у плитки способности */
.delve-reward-plain {
    --glass-top: color-mix(in srgb, var(--reward-accent) 30%, transparent);
    --glass-mid: color-mix(in srgb, var(--reward-accent) 18%, transparent);
    --glass-bottom: color-mix(in srgb, var(--reward-accent) 10%, rgba(10, 14, 24, 0.6));
    box-sizing: border-box;
    width: 100%;
    display: flex;
    padding: 8px;
    border-radius: 14px;
    background: var(--metalic-background);
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 4px 3px 2px #181717,
        0 3px 12px 2px rgba(0, 0, 0, 0.6);
    text-align: center;
    text-shadow: var(--text-outline);
}

.delve-reward-plain .delve-reward-plain-glass {
    --glass-glow: color-mix(in srgb, var(--reward-accent) 45%, transparent);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    flex: 1;
    width: 100%;
    min-width: 0;
    padding: 8px 6px;
    border-radius: 10px;
}

.delve-reward-plain-glass > * {
    position: relative;
    z-index: 1;
}

.delve-reward-kind {
    color: #fde68a;
    font-size: 10px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.delve-reward-icon {
    font-size: 28px;
    line-height: 1.1;
}

.delve-reward-icon-img {
    width: 34px;
    height: 34px;
}

.delve-reward-title {
    color: #f8fafc;
    font-size: 13px;
    font-weight: bold;
}

.delve-reward-text {
    color: #e2e8f0;
    font-size: 11px;
    line-height: 1.3;
}
</style>
