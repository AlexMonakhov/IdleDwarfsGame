<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import CollectionSplit from './CollectionSplit.vue';
import { STATUS_ICONS } from '@/components/icons/status';
import { DAMAGE_COLORS, STATUS_VISUALS } from '@/battle/FxRegistry';
import { statusLabel } from '@/i18n';
import { EFFECT_SAMPLES, HEAL_STATUSES, SKIP_TURN_STATUSES, STATUS_DAMAGE } from './catalog';
import { CollectionPreview, statusScene } from './previewScenes';

// Статусы: на сцене один юнит, на нём статус накладывается, срабатывает несколько ходов и спадает
const { t, te } = useI18n();

const selected = ref<string | null>(null);

const select = (name: string) => {
    selected.value = selected.value === name ? null : name;
};

// Цвет статуса: окраска силуэта, иначе вспышка тика
const colorOf = (name: string) => {
    const visual = STATUS_VISUALS[name];
    return visual?.tint?.color ?? visual?.tickFlash ?? (HEAL_STATUSES.has(name) ? DAMAGE_COLORS.Heal : '#fde68a');
};

// Описание с правилом, как складываются повторные наложения; для незнакомого статуса — общее
const noteOf = (name: string) => {
    if (te(`collection.statusRules.${name}`)) return t(`collection.statusRules.${name}`);
    if (SKIP_TURN_STATUSES.has(name)) return t('collection.statusSkip');
    if (HEAL_STATUSES.has(name)) return t('collection.statusHeal');
    return t('collection.statusTick');
};

const preview = computed<CollectionPreview | null>(() => {
    const name = selected.value;
    if (!name) return null;
    return {
        key: name,
        scene: statusScene(name),
        element: 'Basic',
        title: statusLabel(name),
        color: colorOf(name),
        description: noteOf(name),
    };
});
</script>

<template>
    <CollectionSplit :hint="selected ? t('collection.switchHint') : t('collection.effectsHint')" :preview="preview">
        <template v-if="selected && STATUS_DAMAGE[selected]" #tags>
            <span class="collection-chip" :style="{ '--chip-color': DAMAGE_COLORS[STATUS_DAMAGE[selected]] }">
                {{ t(`collection.damageTypes.${STATUS_DAMAGE[selected]}`) }}
            </span>
        </template>

        <button
            v-for="name in EFFECT_SAMPLES"
            :key="name"
            class="collection-tile"
            :class="{ 'collection-tile-selected': selected === name }"
            :style="{ '--tile-accent': colorOf(name) }"
            @click="select(name)"
        >
            <span class="effect-icon" :style="{ color: colorOf(name) }">
                <component v-if="STATUS_ICONS[name]" :is="STATUS_ICONS[name]" size="26px" />
                <template v-else>{{ name[0] }}</template>
            </span>
            <span class="collection-tile-text">
                <span class="collection-tile-name">{{ statusLabel(name) }}</span>
                <span class="collection-tile-note">{{ noteOf(name) }}</span>
            </span>
        </button>
    </CollectionSplit>
</template>

<style scoped>
.effect-icon {
    flex-shrink: 0;
    width: 34px;
    height: 34px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    background: radial-gradient(circle, color-mix(in srgb, currentColor 30%, transparent) 0%, rgba(0, 0, 0, 0.6) 70%);
    font-weight: bold;
    filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.8));
}
</style>
