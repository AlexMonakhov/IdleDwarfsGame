<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import CrystalIcon from './CrystalIcon.vue';
import { createRandom, type CrystalVariant, type GemShape } from './crystalGeometry';
import { gemColorVar, resolveGemColors, type GemColor } from './gemPalette';

// Круглый загрузчик: рамка как у GemButton, внутри CrystalIcon, который собирается по мере progress
interface Props {
  // Прогресс загрузки 0..1
  progress: number
  variant?: CrystalVariant
  shape?: GemShape
  // Цвет камня; 'random' — любой из палитры кнопок
  color?: GemColor | 'random'
  // Второй цвет — градиент по диагонали камня. 'random' — любой, кроме первого
  gradientTo?: GemColor | 'random' | null
  // Рамка как у GemButton: круглая или со скошенными углами
  frame?: 'round' | 'bevel'
  size?: string
  seed?: number
  showPercent?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'assemble',
  shape: 'brilliant',
  color: 'random',
  gradientTo: null,
  frame: 'round',
  size: '180px',
  seed: () => Math.floor(Math.random() * 1e9),
  showPercent: true,
});

const p = computed(() => Math.max(0, Math.min(1, props.progress)));
const complete = computed(() => p.value >= 1);

// Случайный цвет — от seed: при смене seed новый раскол и новый цвет
const colors = computed(() => resolveGemColors(createRandom(props.seed ^ 0x5bd1e995), props.color, props.gradientTo));
const colorA = computed(() => gemColorVar(colors.value.a));
const colorB = computed(() => (colors.value.b ? gemColorVar(colors.value.b) : null));

const rootStyle = computed(() => ({
  '--loader-size': props.size,
  '--c-a': colorA.value,
  '--p': p.value,
}));

// Короткая вспышка при завершении
const flash = ref(false);
watch(complete, (done) => {
  if (!done) return;
  flash.value = true;
  setTimeout(() => (flash.value = false), 900);
});
</script>

<template>
  <div class="crystal-loader" :class="[`frame-${frame}`, { complete, flash }]" :style="rootStyle">
    <div class="loader-frame">
      <div class="loader-inner">
        <CrystalIcon
          class="loader-crystal"
          :progress="p" :variant="variant" :shape="shape"
          :color="colorA" :color-to="colorB" :seed="seed"
          roomy effects
        />
      </div>
    </div>
    <div v-if="showPercent" class="loader-percent">{{ Math.floor(p * 100) }}%</div>
  </div>
</template>

<style scoped>
.crystal-loader {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

/* Металлическая рамка, как у GemButton */
.loader-frame {
  width: var(--loader-size);
  height: var(--loader-size);
  padding: 4px;
  box-sizing: border-box;
  background: var(--metalic-background, linear-gradient(135deg, #ccc, #777));
  border: 0.7px solid rgba(0, 0, 0, 0.1);
  box-shadow: 0 10px 22px rgba(0, 0, 0, 0.45), inset 0 1px 2px rgba(255, 255, 255, 0.8);
}

/* Углубление под камень: темное стекло, тонирующееся цветом камня по мере сборки */
.loader-inner {
  position: relative;
  width: 100%;
  height: 100%;
  overflow: hidden;
  background:
    radial-gradient(circle at 50% 45%,
      color-mix(in srgb, var(--c-a) calc(var(--p) * 45%), #1b2230) 0%,
      #0d1118 75%);
  box-shadow: inset 0 0 2px rgba(255, 255, 255, 0.5), inset 0 2px 10px rgba(0, 0, 0, 0.8), 0 0.5px 1px 0.5px #555;
  transition: box-shadow 0.4s ease;
}

/* Граненые блики стекла поверх (как .gem-inner::after) */
.loader-inner::after {
  content: '';
  position: absolute;
  inset: 0;
  pointer-events: none;
  background: linear-gradient(45deg,
    rgba(0,0,0,0.3) 0%, rgba(0,0,0,0.1) 25%, rgba(255,255,255,0.12) 30%, rgba(255,255,255,0.06) 40%,
    rgba(0,0,0,0.1) 45%, rgba(0,0,0,0.1) 65%, rgba(255,255,255,0.12) 70%, rgba(255,255,255,0.06) 75%,
    rgba(0,0,0,0.1) 80%, rgba(0,0,0,0.3) 100%);
}

.frame-round .loader-frame,
.frame-round .loader-inner { border-radius: 50%; }

.frame-bevel .loader-frame { border-radius: 18px; corner-shape: bevel; }
.frame-bevel .loader-inner { border-radius: 15px; corner-shape: bevel; }

.loader-crystal {
  position: relative;
  z-index: 1;
  width: 100%;
  height: 100%;
}

/* Собранный камень светится цветом изнутри */
.complete .loader-inner {
  box-shadow:
    inset 0 0 2px rgba(255, 255, 255, 0.6),
    inset 0 0 26px color-mix(in srgb, var(--c-a) 70%, transparent),
    0 0.5px 1px 0.5px #555;
}

.complete .loader-frame { animation: glow-pulse 2.4s ease-in-out infinite; }

@keyframes glow-pulse {
  0%, 100% { box-shadow: 0 10px 22px rgba(0, 0, 0, 0.45), inset 0 1px 2px rgba(255, 255, 255, 0.8), 0 0 10px color-mix(in srgb, var(--c-a) 40%, transparent); }
  50% { box-shadow: 0 10px 22px rgba(0, 0, 0, 0.45), inset 0 1px 2px rgba(255, 255, 255, 0.8), 0 0 26px color-mix(in srgb, var(--c-a) 80%, transparent); }
}

/* Вспышка в момент, когда камень стал целым */
.loader-inner::before {
  content: '';
  position: absolute;
  inset: 0;
  z-index: 2;
  pointer-events: none;
  background: radial-gradient(circle, #fff 0%, color-mix(in srgb, var(--c-a) 60%, transparent) 40%, transparent 70%);
  opacity: 0;
}

.flash .loader-inner::before { animation: flash 0.9s ease-out; }

@keyframes flash {
  0% { opacity: 0; transform: scale(0.4); }
  20% { opacity: 1; }
  100% { opacity: 0; transform: scale(1.4); }
}

.loader-percent {
  color: #fff;
  font-family: 'Arial', sans-serif;
  font-weight: bold;
  font-size: 18px;
  letter-spacing: 1px;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.6);
  font-variant-numeric: tabular-nums;
}
</style>
