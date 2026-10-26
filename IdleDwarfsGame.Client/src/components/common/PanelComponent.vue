<script setup lang="ts">
import { computed, type CSSProperties } from 'vue'

// Рамка в стиле ModalComponent, но без оверлея — для обычных страниц (логин, 404 и т.д.)
interface Props {
  width?: string
  height?: string
  headerMessage?: string
}

const props = withDefaults(defineProps<Props>(), {
  width: '420px',
  height: 'auto'
})

const panelStyle = computed<CSSProperties>(() => ({
  width: props.width,
  height: props.height
}))
</script>

<template>
  <div class="panel-window" :style="panelStyle">
    <div class="panel-header"><div class="panel-header-text">{{ props.headerMessage }}</div></div>
    <div class="panel-content">
      <slot />
      <!-- Кнопки живут внутри контента, в правом нижнем углу -->
      <div v-if="$slots.panelFooter" class="panel-footer">
        <slot name="panelFooter"></slot>
      </div>
    </div>
  </div>
</template>

<style scoped>
.panel-window {
  --border-radius: 100px;
  position: relative;
  z-index: 0;
  box-sizing: border-box;
  max-width: calc(100vw - 60px);
  padding-top: 80px;
  border-radius: var(--border-radius);
}

.panel-window::before {
  position: absolute;
  top: -15px; bottom: -15px;
  left: -15px; right: -15px;
  background: var(--metalic-background);
  content: '';
  z-index: -1;
  border-radius: 80px 80px 10px 10px;
  box-shadow: 0 4px 3px 2px #181717, 0 3px 20px 4px #65a7b8;
}

.panel-header {
  width: 100%;
  height: 70px;
  background: var(--wood-background);
  position: absolute;
  top: 0;
  left: 0;
  border-radius: var(--border-radius) var(--border-radius) 0 0;
  box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333, 0 0 0 4px rgb(157, 173, 175), 0 0 0 6px #333;
  text-align: center;
}

.panel-header-text {
  line-height: 70px;
  font-weight: 900;
  font-size: 2em;
  letter-spacing: 0.2em;
  text-transform: uppercase;
  background: var(--rune-text-background);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  text-shadow: 1px 1px 4px #b2bfe0;
  filter: drop-shadow(-4px 1px 1px #333);
  font-family: RunesFont, sans-serif;
}

.panel-content {
  box-sizing: border-box;
  width: 100%;
  color: white;
  border-radius: 15px;
  border: 2px solid #333;
  padding: 20px 24px;
  /* Тот же тёмный центр, что у ModalComponent: белый текст читается, панели и модалки выглядят одинаково */
  background: radial-gradient(ellipse at 50% 35%, #5b7798 0%, #3f5a7a 45%, #1f2d42 100%);
  box-shadow: 0 0 0 4px rgb(157, 173, 175),
              0 0 0 6px #333,
              inset 0px 0px 6px black, inset 0px 0px 20px #333;
  display: flex;
  flex-direction: column;
}

.panel-footer {
  margin-top: auto;
  padding-top: 16px;
  display: flex;
  justify-content: flex-end;
  align-items: center;
  column-gap: 10px;
}
</style>
