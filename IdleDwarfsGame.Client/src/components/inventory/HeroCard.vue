<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import UnitPortrait from '@/components/interface/UnitPortrait.vue';
import { HeroInfo } from '@/models/types/Heroes';
import { elementColor } from '@/battle/sprites';

const { t } = useI18n();

const props = defineProps<{
    hero: HeroInfo;
    // Карточку сейчас тащат — показываем её полупрозрачной на старом месте
    dragging?: boolean;
    // Карточку нельзя взять (герой в экспедиции): её видно, но не перетащить и не выбрать
    disabled?: boolean;
    // Карточку выбирают кликом, а не перетаскиванием
    selectable?: boolean;
}>();

const color = computed(() => elementColor(props.hero.element));
</script>

<template>
    <div
        class="hero-card"
        :class="[`hero-card-${color}`, {
            'hero-card-dragging': dragging,
            'hero-card-disabled': disabled,
            'hero-card-selectable': selectable && !disabled,
        }]"
        :draggable="!disabled && !selectable"
    >
        <UnitPortrait :unit="hero" :size="64" />
        <div class="hero-card-name" :title="hero.name">{{ hero.name }}</div>
        <div class="hero-card-element">{{ t(`elements.${hero.element}`) }}</div>
        <div class="hero-card-power" :title="t('power.value', { power: hero.power.total })">⚔ {{ hero.power.total }}</div>
        <div v-if="hero.onExpedition" class="hero-card-busy">{{ t('inventory.onExpedition') }}</div>
    </div>
</template>

<style scoped>
.hero-card {
    box-sizing: border-box;
    width: 112px;
    padding: 8px 6px;
    display: flex;
    flex-direction: column;
    align-items: center;
    row-gap: 4px;
    border-radius: 10px;
    background: rgba(15, 23, 32, 0.75);
    border: 2px solid var(--card-accent, rgb(157, 173, 175));
    box-shadow: 0 3px 6px rgba(0, 0, 0, 0.5);
    cursor: grab;
    user-select: none;
    transition: transform 0.1s ease, opacity 0.15s ease;
}

.hero-card:hover {
    transform: translateY(-2px);
}

.hero-card:active {
    cursor: grabbing;
}

.hero-card-dragging {
    opacity: 0.35;
}

.hero-card-selectable {
    cursor: pointer;
}

.hero-card-disabled {
    cursor: not-allowed;
    filter: grayscale(0.85) brightness(0.7);
}

.hero-card-disabled:hover {
    transform: none;
}

.hero-card-busy {
    padding: 1px 6px;
    border-radius: 6px;
    background: rgba(0, 0, 0, 0.6);
    color: #fde68a;
    font-family: 'Arial', sans-serif;
    font-size: 10px;
    font-weight: bold;
    text-transform: uppercase;
}

.hero-card-red { --card-accent: var(--red-gem-color); }
.hero-card-blue { --card-accent: var(--blue-gem-color); }
.hero-card-yellow { --card-accent: var(--yellow-gem-color); }
.hero-card-green { --card-accent: var(--green-gem-color); }

.hero-card-name {
    max-width: 100%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    color: #f8fafc;
    font-family: 'Arial', sans-serif;
    font-size: 13px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.hero-card-element {
    color: var(--card-accent, #cbd5e1);
    font-family: 'Arial', sans-serif;
    font-size: 11px;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.hero-card-power {
    color: #ffd166;
    font-family: 'Arial', sans-serif;
    font-size: 12px;
    font-weight: bold;
    text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}
</style>
