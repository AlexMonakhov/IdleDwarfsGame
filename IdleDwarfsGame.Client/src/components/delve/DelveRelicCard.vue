<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import GlassButton from '@/components/common/GlassButton.vue';
import { RELIC_ICONS, relicAccent, relicColors } from './delveRelics';

// Реликвия на выбор — плитка как у способности алтаря: оправа, стекло цвета реликвии, значок, название, эффект.
// Редкая — золотая кайма; если такая уже есть, внизу сколько копий будет после выбора
const props = defineProps<{
    relicKey: string;
    rare?: boolean;
    // Сколько таких уже у отряда
    owned?: number;
    // Предел копий (null — без предела)
    max?: number | null;
    disabled?: boolean;
}>();

const emit = defineEmits<{ (e: 'pick'): void }>();
const { t } = useI18n();

const accent = computed(() => relicAccent(props.relicKey));
const icon = computed(() => RELIC_ICONS[props.relicKey]);
const colors = computed(() => relicColors(props.relicKey));
</script>

<template>
    <button
        type="button"
        class="relic-card"
        data-sound-hover="ui_hover_card"
        :class="{ 'relic-card-rare': rare }"
        :style="{ '--relic-accent': accent }"
        :disabled="disabled"
        @click="emit('pick')"
    >
        <GlassButton tag="div" class="relic-card-glass">
            <span class="relic-card-kind">{{ rare ? t('social.delve.relicRare') : t('social.delve.relicKind') }}</span>
            <span class="relic-card-icon">
                <component :is="icon" v-if="icon" size="40px" :primary-color="colors.light" :secondary-color="colors.dark" />
                <template v-else>💠</template>
            </span>
            <span class="relic-card-name">{{ t(`social.delve.relicNames.${relicKey}`) }}</span>
            <span class="relic-card-text">{{ t(`social.delve.relicHints.${relicKey}`) }}</span>
            <span v-if="owned" class="relic-card-owned">
                {{ t('social.delve.relicOwned', { count: owned }) }}<template v-if="max"> / {{ max }}</template>
            </span>
        </GlassButton>
    </button>
</template>

<style scoped>
/* Оправа и стекло — как у плитки способности (AbilityTile) */
.relic-card {
    --glass-top: color-mix(in srgb, var(--relic-accent) 30%, transparent);
    --glass-mid: color-mix(in srgb, var(--relic-accent) 18%, transparent);
    --glass-bottom: color-mix(in srgb, var(--relic-accent) 10%, rgba(10, 14, 24, 0.6));
    box-sizing: border-box;
    width: 156px;
    display: flex;
    padding: 8px;
    border: none;
    border-radius: 14px;
    background: var(--metalic-background);
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 4px 3px 2px #181717,
        0 3px 12px 2px rgba(0, 0, 0, 0.6);
    color: #f8fafc;
    font-family: 'Arial', sans-serif;
    text-align: center;
    text-shadow: var(--text-outline);
    cursor: pointer;
    transition: transform 0.2s cubic-bezier(0.34, 1.4, 0.64, 1);
}

.relic-card:hover:not(:disabled) {
    transform: translateY(-4px);
}

.relic-card:disabled {
    cursor: default;
    opacity: 0.6;
}

.relic-card-rare {
    box-shadow:
        inset 0 1px 2px rgba(255, 255, 255, 0.8),
        0 0 0 2px #facc15,
        0 0 16px 3px rgba(250, 204, 21, 0.45),
        0 4px 3px 2px #181717;
}

.relic-card .relic-card-glass {
    --glass-glow: color-mix(in srgb, var(--relic-accent) 45%, transparent);
    flex: 1;
    width: 100%;
    min-width: 0;
    justify-content: flex-start;
    row-gap: 4px;
    padding: 8px 6px 10px;
    border-radius: 10px;
}

.relic-card-glass > * {
    position: relative;
    z-index: 1;
}

.relic-card-kind {
    color: #e9d5ff;
    font-size: 10px;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.relic-card-rare .relic-card-kind {
    color: #fde68a;
}

.relic-card-icon {
    height: 46px;
    display: grid;
    place-items: center;
    font-size: 32px;
}

.relic-card-icon > :deep(svg) {
    filter: drop-shadow(2px 2px 1px #000);
}

.relic-card-name {
    font-size: 13px;
    font-weight: bold;
}

.relic-card-text {
    color: #e2e8f0;
    font-size: 11px;
    line-height: 1.3;
}

.relic-card-owned {
    margin-top: auto;
    padding-top: 4px;
    color: #fde68a;
    font-size: 11px;
    font-weight: bold;
}
</style>
