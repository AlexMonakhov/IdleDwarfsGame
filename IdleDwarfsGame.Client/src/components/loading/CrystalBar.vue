<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import {
  BAR_W, barOutline, barShards, barSpikes, createRandom, toPoints,
  type Facet,
} from './crystalGeometry';
import { gemColorVar, resolveGemColors, type GemColor } from './gemPalette';

// Полоса загрузки — вытянутый кристалл в металлической рамке GemButton. Собирается слева направо
interface Props {
  // Прогресс загрузки 0..1
  progress: number
  // shards — осколки кристаллизуются рваным фронтом; cluster — из основания растет друза кристаллов
  variant?: 'shards' | 'cluster'
  color?: GemColor | 'random'
  // Второй цвет — градиент вдоль полосы
  gradientTo?: GemColor | 'random' | null
  // Где счетчик процентов: по центру полосы, в камне на конце, над фронтом заполнения или нигде
  counter?: 'center' | 'end' | 'follow' | 'none'
  width?: string
  seed?: number
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'shards',
  color: 'random',
  gradientTo: null,
  counter: 'center',
  width: '420px',
  seed: () => Math.floor(Math.random() * 1e9),
});

const uid = `cb-${Math.random().toString(36).slice(2, 9)}`;
const p = computed(() => Math.max(0, Math.min(1, props.progress)));
const complete = computed(() => p.value >= 1);

const geometry = computed(() => {
  const rand = createRandom(props.seed);
  const colors = resolveGemColors(rand, props.color, props.gradientTo);
  return {
    colors,
    facets: props.variant === 'shards' ? barShards(rand) : [],
    spikes: props.variant === 'cluster' ? barSpikes(rand) : [],
  };
});

const colorA = computed(() => gemColorVar(geometry.value.colors.a));
const colorB = computed(() => (geometry.value.colors.b ? gemColorVar(geometry.value.colors.b) : null));
const outline = toPoints(barOutline());

const rootStyle = computed(() => ({
  '--bar-width': props.width,
  '--c-a': colorA.value,
  '--c-b': colorB.value ?? colorA.value,
  '--p': p.value,
}));

// Цвет по x (градиент вдоль полосы) + светлота грани
const shadeColor = (x: number, shade: number) => {
  const t = Math.round(100 - (x / BAR_W) * 100);
  const base = colorB.value ? `color-mix(in srgb, var(--c-a) ${t}%, var(--c-b))` : 'var(--c-a)';
  const amount = Math.round(Math.abs(shade) * 100);
  return `color-mix(in srgb, ${base} ${100 - amount}%, ${shade >= 0 ? '#fff' : '#000'})`;
};

// Грань встает на место, когда до нее дошел фронт (к 98% — все)
const isPlaced = (order: number) => p.value >= 0.01 + order * 0.97;

const facetStyle = (f: Facet) => {
  if (isPlaced(f.order)) return { fill: shadeColor(f.cx, f.shade) };
  return { fill: 'var(--glass)', transform: `translate(${f.scatter.x}px, ${f.scatter.y}px) scale(0.75)` };
};


// Счетчик плавно догоняет реальный прогресс, а не скачет рывками загрузки
const shown = ref(p.value * 100);
let raf = 0;
const tick = () => {
  const target = p.value * 100;
  const diff = target - shown.value;
  shown.value = Math.abs(diff) < 0.2 ? target : shown.value + diff * 0.12;
  raf = shown.value === target ? 0 : requestAnimationFrame(tick);
};
watch(p, () => { if (!raf) raf = requestAnimationFrame(tick); });
onBeforeUnmount(() => cancelAnimationFrame(raf));
const percentText = computed(() => `${Math.floor(shown.value)}%`);

// Короткая вспышка при завершении
const flash = ref(false);
watch(complete, (done) => {
  if (!done) return;
  flash.value = true;
  setTimeout(() => (flash.value = false), 900);
});
</script>

<template>
  <div class="crystal-bar" :class="[`variant-${variant}`, `counter-${counter}`, { complete, flash }]" :style="rootStyle">
    <div class="bar-frame">
      <div class="bar-inner">
        <svg class="bar-svg" viewBox="-8 -16 416 92" aria-hidden="true">
          <defs>
            <clipPath :id="`${uid}-outline`"><polygon :points="outline" /></clipPath>
            <linearGradient :id="`${uid}-shine`" x1="0" y1="0" x2="1" y2="0">
              <stop offset="0" stop-color="#fff" stop-opacity="0" />
              <stop offset="0.5" stop-color="#fff" stop-opacity="0.7" />
              <stop offset="1" stop-color="#fff" stop-opacity="0" />
            </linearGradient>
            <linearGradient :id="`${uid}-glass`" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0" stop-color="#fff" stop-opacity="0.2" />
              <stop offset="1" stop-color="#fff" stop-opacity="0.03" />
            </linearGradient>
          </defs>

          <!-- Друза: кристаллы растут из основания -->
          <template v-if="variant === 'cluster'">
            <polygon class="ledge" points="-4,58 404,58 398,66 2,66" />
            <g v-for="s in geometry.spikes" :key="s.id" :transform="`rotate(${s.angle.toFixed(1)} ${s.x.toFixed(1)} 58)`">
              <g class="spike" :class="{ placed: isPlaced(s.order), back: s.back }">
                <polygon
                  v-for="(face, i) in s.faces" :key="i"
                  class="spike-face" :points="toPoints(face.points)"
                  :style="{ fill: isPlaced(s.order) ? shadeColor(s.x, face.shade - (s.back ? 0.15 : 0)) : 'var(--glass)' }"
                />
              </g>
            </g>
          </template>

          <template v-else>
            <!-- Силуэт целой полосы — «призрак», куда встают грани -->
            <polygon class="ghost" :points="outline" :fill="`url(#${uid}-glass)`" />

            <g :clip-path="`url(#${uid}-outline)`">
              <polygon
                v-for="f in geometry.facets" :key="f.id"
                class="facet shard" :class="{ placed: isPlaced(f.order) }"
                :points="toPoints(f.points)" :style="facetStyle(f)"
              />
            </g>

            <polygon class="outline" :points="outline" />
          </template>

          <!-- Блик-пробег по собранной полосе -->
          <g :clip-path="variant === 'cluster' ? undefined : `url(#${uid}-outline)`">
            <rect class="shine" x="-90" y="-20" width="60" height="100" :fill="`url(#${uid}-shine)`" />
          </g>
        </svg>

        <span v-if="counter === 'center'" class="bar-counter bar-counter-center">{{ percentText }}</span>
        <span
          v-else-if="counter === 'follow'" class="bar-counter bar-counter-follow"
          :style="{ left: `calc(${(Math.min(Math.max(p, 0.06), 0.94) * 100).toFixed(2)}%)` }"
        >{{ percentText }}</span>
      </div>
    </div>

    <!-- Счетчик в маленьком камне на конце полосы -->
    <div v-if="counter === 'end'" class="bar-end-gem">
      <span class="bar-end-inner"><span class="bar-counter">{{ percentText }}</span></span>
    </div>
  </div>
</template>

<style scoped>
.crystal-bar {
  --glass: rgba(255, 255, 255, 0.07);
  position: relative;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

/* Металлическая рамка, как у GemButton со скошенными углами */
.bar-frame {
  width: var(--bar-width);
  padding: 4px;
  box-sizing: border-box;
  border-radius: 14px;
  corner-shape: bevel;
  background: var(--metalic-background, linear-gradient(135deg, #ccc, #777));
  border: 0.7px solid rgba(0, 0, 0, 0.1);
  box-shadow: 0 8px 18px rgba(0, 0, 0, 0.45), inset 0 1px 2px rgba(255, 255, 255, 0.8);
}

.bar-inner {
  position: relative;
  border-radius: 11px;
  corner-shape: bevel;
  overflow: hidden;
  background:
    radial-gradient(ellipse at calc(var(--p) * 100%) 50%,
      color-mix(in srgb, var(--c-b) calc(var(--p) * 40%), #1b2230) 0%,
      #0d1118 70%);
  box-shadow: inset 0 0 2px rgba(255, 255, 255, 0.5), inset 0 2px 10px rgba(0, 0, 0, 0.8), 0 0.5px 1px 0.5px #555;
  transition: box-shadow 0.4s ease;
}

.bar-inner::after {
  content: '';
  position: absolute;
  inset: 0;
  pointer-events: none;
  background: linear-gradient(80deg,
    rgba(0,0,0,0.25) 0%, rgba(255,255,255,0.08) 22%, rgba(0,0,0,0.08) 30%, rgba(0,0,0,0.08) 62%,
    rgba(255,255,255,0.08) 70%, rgba(0,0,0,0.25) 100%);
}

.bar-svg {
  position: relative;
  z-index: 1;
  display: block;
  width: 100%;
  height: auto;
  overflow: visible;
}

.ghost {
  stroke: rgba(255, 255, 255, 0.18);
  stroke-width: 0.6;
  stroke-dasharray: 3 3;
}

.facet {
  stroke: rgba(255, 255, 255, 0.4);
  stroke-width: 0.4;
  stroke-linejoin: round;
  transform-box: fill-box;
  transform-origin: center;
  transition:
    transform 0.7s cubic-bezier(0.2, 1.35, 0.4, 1),
    fill 0.5s ease,
    stroke 0.5s ease;
}

.facet.placed { stroke: rgba(255, 255, 255, 0.25); }


.shard { stroke-width: 0.3; }


.outline {
  fill: none;
  stroke: rgba(255, 255, 255, 0.55);
  stroke-width: 0.9;
  stroke-linejoin: round;
}

/* Друза */
.ledge {
  fill: #2a313d;
  stroke: rgba(255, 255, 255, 0.2);
  stroke-width: 0.6;
}

.spike {
  transform-box: fill-box;
  transform-origin: 50% 100%;
  transform: scaleY(0.14);
  transition: transform 0.8s cubic-bezier(0.2, 1.4, 0.4, 1);
}

.spike.placed { transform: scaleY(1); }

.spike-face {
  stroke: rgba(255, 255, 255, 0.45);
  stroke-width: 0.4;
  stroke-linejoin: round;
  transition: fill 0.5s ease;
}

.spike.placed .spike-face { stroke: rgba(255, 255, 255, 0.3); }

.shine { opacity: 0; }

.complete .shine {
  opacity: 1;
  animation: shine-run 2.6s ease-in-out 0.3s infinite;
}

@keyframes shine-run {
  0% { transform: translateX(0) skewX(-25deg); }
  50%, 100% { transform: translateX(560px) skewX(-25deg); }
}

/* Целая полоса светится цветом */
.complete .bar-inner {
  box-shadow:
    inset 0 0 2px rgba(255, 255, 255, 0.6),
    inset 0 0 22px color-mix(in srgb, var(--c-a) 60%, transparent),
    0 0.5px 1px 0.5px #555;
}

.complete .bar-frame { animation: glow-pulse 2.4s ease-in-out infinite; }

@keyframes glow-pulse {
  0%, 100% { box-shadow: 0 8px 18px rgba(0, 0, 0, 0.45), inset 0 1px 2px rgba(255, 255, 255, 0.8), 0 0 8px color-mix(in srgb, var(--c-a) 40%, transparent); }
  50% { box-shadow: 0 8px 18px rgba(0, 0, 0, 0.45), inset 0 1px 2px rgba(255, 255, 255, 0.8), 0 0 22px color-mix(in srgb, var(--c-a) 80%, transparent); }
}

.bar-inner::before {
  content: '';
  position: absolute;
  inset: 0;
  z-index: 2;
  pointer-events: none;
  background: radial-gradient(ellipse at 50% 50%, #fff 0%, color-mix(in srgb, var(--c-a) 60%, transparent) 35%, transparent 70%);
  opacity: 0;
}

.flash .bar-inner::before { animation: flash 0.9s ease-out; }

@keyframes flash {
  0% { opacity: 0; transform: scaleX(0.3); }
  20% { opacity: 1; }
  100% { opacity: 0; transform: scaleX(1.3); }
}

/* Счетчик */
.bar-counter {
  color: #fff;
  font-family: 'Arial', sans-serif;
  font-weight: bold;
  letter-spacing: 1px;
  font-variant-numeric: tabular-nums;
  text-shadow: var(--text-outline, 1px 1px 0 rgba(0, 0, 0, 0.7)), 0 0 6px rgba(0, 0, 0, 0.6);
  pointer-events: none;
  user-select: none;
  white-space: nowrap;
}

.bar-counter-center {
  position: absolute;
  inset: 0;
  z-index: 3;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
}

.complete .bar-counter-center { animation: counter-pop 0.6s cubic-bezier(0.2, 1.6, 0.4, 1); }

@keyframes counter-pop {
  0% { transform: scale(1); }
  40% { transform: scale(1.3); }
  100% { transform: scale(1); }
}

/* Над фронтом: плашка едет вместе с заполнением */
.bar-counter-follow {
  position: absolute;
  top: 50%;
  z-index: 3;
  transform: translate(-50%, -50%);
  padding: 2px 8px;
  border-radius: 6px;
  font-size: 14px;
  background: rgba(10, 14, 24, 0.55);
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.35);
  transition: left 0.35s ease;
}

/* Камень на конце: как маленький круглый GemButton цвета полосы.
   Висит сбоку и не входит в размер компонента — при центрировании по центру встает сама полоса */
.bar-end-gem {
  position: absolute;
  left: calc(100% + 8px);
  top: 50%;
  translate: 0 -50%;
  flex: none;
  width: 58px;
  height: 58px;
  padding: 3px;
  box-sizing: border-box;
  border-radius: 50%;
  background: var(--metalic-background, linear-gradient(135deg, #ccc, #777));
  box-shadow: 0 6px 14px rgba(0, 0, 0, 0.45), inset 0 1px 2px rgba(255, 255, 255, 0.8);
}

.bar-end-inner {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  border-radius: 50%;
  overflow: hidden;
  background: color-mix(in srgb, var(--c-b) calc(30% + var(--p) * 70%), #1b2230);
  box-shadow: inset 0 0 2px rgba(255, 255, 255, 0.6), 0 0.5px 1px 0.5px #555;
  transition: background 0.4s ease;
}

.bar-end-inner::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(45deg,
    rgba(0,0,0,0.3) 0%, rgba(0,0,0,0.1) 25%, rgba(255,255,255,0.3) 30%, rgba(255,255,255,0.2) 40%,
    rgba(0,0,0,0.1) 45%, rgba(0,0,0,0.1) 65%, rgba(255,255,255,0.3) 70%, rgba(255,255,255,0.2) 75%,
    rgba(0,0,0,0.1) 80%, rgba(0,0,0,0.3) 100%);
}

.bar-end-inner .bar-counter {
  position: relative;
  z-index: 1;
  font-size: 14px;
}
</style>
