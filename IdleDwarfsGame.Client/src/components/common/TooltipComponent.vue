<template>
  <!-- Обертка вокруг элемента, на который мы наводим мышь -->
  <div
    class="dwarf-tooltip-wrapper"
    @mouseenter="show"
    @mouseleave="hide"
  >
    <!-- Дефолтный слот для самого предмета/кнопки -->
    <slot />

    <TooltipPanel v-if="anchor" :anchor="anchor" :accent="accent">
      <template #title><slot name="title">{{ t('tooltip.defaultTitle') }}</slot></template>
      <template v-if="$slots.subtitle" #subtitle><slot name="subtitle" /></template>
      <template #description><slot name="description">{{ t('tooltip.defaultDescription') }}</slot></template>
      <template v-if="$slots.stats" #stats><slot name="stats" /></template>
    </TooltipPanel>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import TooltipPanel from './TooltipPanel.vue';

// Подсказка с разметкой: заголовок, описание, характеристики. Для одной строки текста — директива v-tooltip
defineProps<{
  accent?: string;
}>();

const { t } = useI18n();

const anchor = ref<DOMRect | null>(null);

const hide = () => {
  anchor.value = null;
  window.removeEventListener('dragstart', hide, true);
};

const show = (event: MouseEvent) => {
  anchor.value = (event.currentTarget as HTMLElement).getBoundingClientRect();
  // Пока что-то тащат, mouseleave не приходит
  window.addEventListener('dragstart', hide, true);
};

onBeforeUnmount(hide);
</script>

<style scoped>
.dwarf-tooltip-wrapper {
  display: inline-block;
  cursor: help;
}
</style>
