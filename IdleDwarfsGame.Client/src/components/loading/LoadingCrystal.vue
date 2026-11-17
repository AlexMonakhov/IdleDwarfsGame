<script lang="ts" setup>
import { computed, onBeforeUnmount, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import CrystalLoader from './CrystalLoader.vue';
import type { GemColor } from './gemPalette';

// Загрузка с сервера в окнах: кристалл A2 с тестовой страницы (грани слетаются, сине-фиолетовый, скошенная рамка).
// Сколько осталось, запрос не сообщает — поэтому без progress кристалл сам собирается, замедляясь к концу
// и не доходя до целого: целым он не станет, пока ответ не пришёл. Окно просто убирает компонент, когда данные есть
interface Props {
  // Реальный прогресс 0..1, если он известен (несколько запросов подряд)
  progress?: number | null
  size?: string
  // Подпись «Загрузка...» под кристаллом
  label?: boolean
  color?: GemColor
  gradientTo?: GemColor | null
}

const props = withDefaults(defineProps<Props>(), {
  progress: null,
  size: '120px',
  label: true,
  color: 'blue',
  gradientTo: 'purple',
});

const { t } = useI18n();

// Самосборка: быстрый старт, потом все медленнее; предел ниже 1 — камень не «готов» раньше данных
const CREEP_LIMIT = 0.94;
const creep = ref(0.04);
const timer = window.setInterval(() => {
  creep.value += Math.max((CREEP_LIMIT - creep.value) * 0.05, 0.0015);
  if (creep.value >= CREEP_LIMIT) creep.value = CREEP_LIMIT;
}, 120);
onBeforeUnmount(() => clearInterval(timer));

const shown = computed(() => props.progress ?? creep.value);
const seed = Math.floor(Math.random() * 1e9);
</script>

<template>
  <div class="loading-crystal" role="status" :aria-label="t('common.loading')">
    <CrystalLoader
      :progress="shown" variant="assemble" shape="crystal"
      :color="color" :gradient-to="gradientTo" frame="bevel"
      :size="size" :seed="seed" :show-percent="false"
    />
    <span v-if="label" class="loading-crystal-label">{{ t('common.loading') }}</span>
  </div>
</template>

<style scoped>
/* Появляется с небольшой задержкой: быстрый ответ не успевает мигнуть загрузчиком */
.loading-crystal {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  /* В сетке карточек — на всю строку, во flex-окне — по центру */
  grid-column: 1 / -1;
  align-self: center;
  margin: auto;
  padding: 24px;
  opacity: 0;
  animation: loading-crystal-in 0.3s ease-out 0.15s forwards;
}

@keyframes loading-crystal-in {
  from { opacity: 0; transform: scale(0.92); }
  to { opacity: 1; transform: scale(1); }
}

.loading-crystal-label {
  color: #fff;
  font-family: 'Arial', sans-serif;
  font-size: 15px;
  font-weight: bold;
  letter-spacing: 3px;
  text-transform: uppercase;
  text-shadow: var(--text-outline, 1px 1px 0 #000);
  animation: loading-crystal-pulse 1.4s ease-in-out infinite alternate;
}

@keyframes loading-crystal-pulse {
  from { opacity: 0.55; }
  to { opacity: 1; }
}
</style>
