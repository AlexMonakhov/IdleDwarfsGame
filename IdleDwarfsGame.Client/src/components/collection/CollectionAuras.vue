<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import CollectionSplit from './CollectionSplit.vue';
import { resolveAuraVisual } from '@/battle/FxRegistry';
import { AURA_SAMPLES, AuraSample } from './catalog';
import { CollectionPreview, auraScene } from './previewScenes';

// Ауры героев: по одной на каждый визуал. На сцене один носитель, сфера ауры и её волны
const { t } = useI18n();

const selected = ref<AuraSample | null>(null);

const visualOf = (aura: AuraSample) => resolveAuraVisual(aura.effect, aura.target);

const select = (aura: AuraSample) => {
    selected.value = selected.value === aura ? null : aura;
};

const preview = computed<CollectionPreview | null>(() => {
    const aura = selected.value;
    if (!aura) return null;
    return {
        key: aura.key,
        scene: auraScene(aura),
        element: aura.element,
        auraPulse: true,
        title: t(`collection.auras.${aura.key}`),
        color: visualOf(aura).color,
        description: t(`collection.auraTargets.${aura.target}`),
    };
});

// Сфера в цветах ауры: обод, дымка и блик, как на сцене
const orbStyle = (aura: AuraSample) => {
    const visual = visualOf(aura);
    return {
        background: `radial-gradient(circle at 35% 30%, ${visual.accent} 0%, ${visual.glow} 35%, ${visual.color} 70%, transparent 72%)`,
        boxShadow: `0 0 12px 2px ${visual.glow}`,
    };
};
</script>

<template>
    <CollectionSplit :hint="selected ? t('collection.switchHint') : t('collection.aurasHint')" :preview="preview">
        <button
            v-for="aura in AURA_SAMPLES"
            :key="aura.key"
            class="collection-tile"
            :class="{ 'collection-tile-selected': selected === aura }"
            :style="{ '--tile-accent': visualOf(aura).color }"
            @click="select(aura)"
        >
            <span class="aura-orb" :style="orbStyle(aura)"></span>
            <span class="collection-tile-text">
                <span class="collection-tile-name">{{ t(`collection.auras.${aura.key}`) }}</span>
                <span class="collection-tile-note">{{ t(`collection.auraTargets.${aura.target}`) }}</span>
            </span>
        </button>
    </CollectionSplit>
</template>

<style scoped>
.aura-orb {
    flex-shrink: 0;
    width: 34px;
    height: 34px;
    border-radius: 50%;
    animation: aura-orb-pulse 2.4s ease-in-out infinite;
}

@keyframes aura-orb-pulse {
    0%, 100% { transform: scale(0.92); opacity: 0.85; }
    50% { transform: scale(1.05); opacity: 1; }
}

@media (prefers-reduced-motion: reduce) {
    .aura-orb {
        animation: none;
    }
}
</style>
