<script lang="ts" setup>
import BadgeComponent from '@/components/common/BadgeComponent.vue';

// Кнопка верхнего меню и мобильной панели: круглый самоцвет в металлической оправе с иконкой и подписью под ним.
// Подпись короткая и не переносится, ширина кнопки — по самоцвету, поэтому весь ряд меню влезает в одну строку.
// Размеры — через --nav-gem (диаметр самоцвета) и --nav-caption (кегль подписи) от родителя
interface Props {
  label: string
  color?: 'pink' | 'green' | 'blue' | 'yellow' | 'red'
  badgeCount?: number
  // «!» у раздела, с которым игрок ещё не знаком (обучение)
  badgeText?: string
  // Подсветка: раздел открыт (раскрытое «Ещё»)
  active?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  color: 'blue',
  badgeCount: 0,
  active: false,
});
</script>

<template>
  <button type="button" class="nav-button" :class="{ 'nav-button-active': props.active }" :aria-label="props.label">
    <span class="nav-stone">
      <span :class="['nav-core', `nav-core-${props.color}`]">
        <span class="nav-icon"><slot /></span>
      </span>
      <BadgeComponent v-if="props.badgeCount > 0 || props.badgeText" :count="props.badgeCount" :text="props.badgeText" />
    </span>
    <span class="nav-caption">{{ props.label }}</span>
  </button>
</template>

<style scoped>
.nav-button {
  --split-icon-primary: #ffffff;
  --split-icon-secondary: #ffe08a;
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  row-gap: 3px;
  min-width: calc(var(--nav-gem, 52px) + 8px);
  padding: 0;
  border: none;
  background: none;
  cursor: pointer;
  -webkit-tap-highlight-color: transparent;
  touch-action: manipulation;
}

.nav-button:focus-visible {
  outline: 2px solid #40fffb;
  outline-offset: 2px;
  border-radius: 8px;
}

/* Оправа: тот же металл, что у GemButton */
.nav-stone {
  position: relative;
  width: var(--nav-gem, 52px);
  height: var(--nav-gem, 52px);
  box-sizing: border-box;
  padding: 3px;
  border-radius: 50%;
  background: var(--metalic-background);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.45), inset 0 1px 2px rgba(255, 255, 255, 0.8);
  transition: transform 0.1s ease, filter 0.2s ease;
}

.nav-button:hover .nav-stone {
  filter: brightness(1.1);
}

.nav-button:active .nav-stone {
  transform: scale(0.94);
}

.nav-button-active .nav-stone {
  box-shadow: 0 0 0 2px #40fffb, 0 0 10px rgba(64, 255, 251, 0.7), 0 6px 12px rgba(0, 0, 0, 0.45);
}

/* Сам камень с бликами-гранями */
.nav-core {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  overflow: hidden;
  border-radius: 50%;
  box-shadow: inset 0 0 3px rgba(255, 255, 255, 0.6), 0 0.5px 1px 0.5px #555;
}

.nav-core::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(45deg,
    rgba(0, 0, 0, 0.3) 0%,
    rgba(0, 0, 0, 0.1) 25%,
    rgba(255, 255, 255, 0.3) 30%,
    rgba(255, 255, 255, 0.2) 40%,
    rgba(0, 0, 0, 0.1) 45%,
    rgba(0, 0, 0, 0.1) 65%,
    rgba(255, 255, 255, 0.3) 70%,
    rgba(255, 255, 255, 0.2) 75%,
    rgba(0, 0, 0, 0.3) 100%);
  pointer-events: none;
}

.nav-core-blue { background-color: var(--blue-gem-color); }
.nav-core-green { background-color: var(--green-gem-color); }
.nav-core-pink { background-color: var(--pink-gem-color); }
.nav-core-red { background-color: var(--red-gem-color); }
.nav-core-yellow { background-color: var(--yellow-gem-color); }

/* Иконка — 55% камня, тень отделяет её от граней */
.nav-icon {
  position: relative;
  z-index: 1;
  display: flex;
  font-size: calc(var(--nav-gem, 52px) * 0.46);
  filter: drop-shadow(1px 1px 0 rgba(0, 0, 0, 0.6));
}

/* Значок — на верхний правый край оправы, а не поверх иконки */
.nav-stone :deep(.badge-outer) {
  top: -5px;
  right: -7px;
}

.nav-caption {
  color: #ffffff;
  font-family: 'Arial', sans-serif;
  font-size: var(--nav-caption, 12px);
  font-weight: bold;
  line-height: 1.1;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  white-space: nowrap;
  text-shadow: var(--text-outline);
  user-select: none;
}
</style>
