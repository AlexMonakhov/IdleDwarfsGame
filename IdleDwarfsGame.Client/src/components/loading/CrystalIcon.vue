<script lang="ts" setup>
import { computed } from 'vue';
import {
  createRandom, gemFacets, gemOutline, generateCracks, gridShards, toPoints,
  type CrystalVariant, type Facet, type GemShape,
} from './crystalGeometry';

// Граненый кристалл одним SVG. По умолчанию — целый (progress = 1): так он работает как обычная иконка камня.
// При progress < 1 расколот и прозрачен — его собирает CrystalLoader. Рамки нет: только сам камень

interface Props {
  progress?: number
  // assemble — грани слетаются на место; heal — трещины затягиваются, цвет поднимается снизу;
  // shards — осколки как у карточки, кристаллизация от центра к краям
  variant?: CrystalVariant
  shape?: GemShape
  // Любой CSS-цвет (в т.ч. var(...))
  color?: string
  // Второй цвет — градиент по диагонали
  colorTo?: string | null
  size?: string | null
  // Фиксированный seed по умолчанию — у одинаковых иконок одинаковая огранка
  seed?: number
  // Запас по краям, чтобы разлетевшиеся грани было видно (для загрузчика)
  roomy?: boolean
  // Блик и искры на целом камне
  effects?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  progress: 1,
  variant: 'assemble',
  shape: 'brilliant',
  color: '#2196f3',
  colorTo: null,
  size: null,
  seed: 7,
  roomy: false,
  effects: false,
});

const uid = `ci-${Math.random().toString(36).slice(2, 9)}`;
const p = computed(() => Math.max(0, Math.min(1, props.progress)));
const complete = computed(() => p.value >= 1);

const geometry = computed(() => {
  const rand = createRandom(props.seed);
  const facets = props.variant === 'shards' ? gridShards(7, 8, rand) : gemFacets(props.shape, rand);
  const cracks = props.variant === 'heal' ? generateCracks(rand, 7) : [];
  return { facets, cracks };
});

const outline = computed(() => toPoints(gemOutline(props.shape)));

const rootStyle = computed(() => ({
  '--c-a': props.color,
  '--c-b': props.colorTo ?? props.color,
  width: props.size ?? undefined,
  height: props.size ?? undefined,
}));

// Цвет грани: база (цвет или точка градиента) + светлота грани
const facetColor = (f: Facet) => {
  const t = Math.round(100 - ((f.cx + f.cy) / 2));
  const base = props.colorTo ? `color-mix(in srgb, var(--c-a) ${t}%, var(--c-b))` : 'var(--c-a)';
  const amount = Math.round(Math.abs(f.shade) * 100);
  return `color-mix(in srgb, ${base} ${100 - amount}%, ${f.shade >= 0 ? '#fff' : '#000'})`;
};

// Порог, на котором грань встает на место: к 97% собраны все
const isPlaced = (f: Facet) => p.value >= 0.03 + f.order * 0.92;

const facetStyle = (f: Facet) => {
  if (props.variant === 'heal') {
    // Куски на месте, но раздвинуты трещинами — щели схлопываются по мере загрузки
    const k = 1 - p.value;
    if (!k) return {};
    return {
      transform: `translate(${(f.scatter.x * 0.08 * k).toFixed(2)}px, ${(f.scatter.y * 0.08 * k).toFixed(2)}px) scale(${1 - 0.05 * k})`,
    };
  }
  const placed = isPlaced(f);
  if (placed) return { fill: facetColor(f) };
  if (props.variant === 'shards') {
    return { fill: 'var(--glass)', transform: `translate(${f.scatter.x * 0.25}px, ${f.scatter.y * 0.25}px) scale(0.8)` };
  }
  return {
    fill: 'var(--glass)',
    transform: `translate(${f.scatter.x}px, ${f.scatter.y}px) rotate(${f.scatter.rotate}deg) scale(0.72)`,
    transitionDelay: `${(f.order * 0.15).toFixed(2)}s`,
  };
};

// heal: уровень «жидкого» цвета снизу вверх; волна шириной 200 — анимация сдвигает ее на период, бесшовно
const wavePath = computed(() => {
  const y = 104 - p.value * 110;
  const amp = complete.value ? 0.01 : 2.2;
  let d = `M-100 ${y.toFixed(2)}`;
  for (let i = 0; i < 10; i++) d += ` q12.5 ${i % 2 ? amp : -amp} 25 0`;
  // Количество команд постоянное — так CSS может плавно интерполировать d
  return `${d} L150 130 L-100 130 Z`;
});
const waveStyle = computed(() => ({ d: `path('${wavePath.value}')` }) as Record<string, string>);
</script>

<template>
  <svg
    class="crystal-icon" :class="[`variant-${variant}`, { complete, effects }]" :style="rootStyle"
    :viewBox="roomy ? '-9 -9 118 118' : '-1 -1 102 102'" aria-hidden="true"
  >
    <defs>
      <clipPath :id="`${uid}-outline`">
        <polygon :points="outline" />
      </clipPath>
      <clipPath v-if="variant === 'heal'" :id="`${uid}-fill`">
        <path class="wave" :style="waveStyle" />
      </clipPath>
      <linearGradient v-if="effects" :id="`${uid}-shine`" x1="0" y1="0" x2="1" y2="0">
        <stop offset="0" stop-color="#fff" stop-opacity="0" />
        <stop offset="0.5" stop-color="#fff" stop-opacity="0.75" />
        <stop offset="1" stop-color="#fff" stop-opacity="0" />
      </linearGradient>
      <radialGradient v-if="!complete" :id="`${uid}-glass`" cx="0.35" cy="0.3" r="0.8">
        <stop offset="0" stop-color="#fff" stop-opacity="0.22" />
        <stop offset="1" stop-color="#fff" stop-opacity="0.04" />
      </radialGradient>
    </defs>

    <!-- Силуэт целого камня — «призрак», куда слетаются грани -->
    <polygon v-if="!complete" class="ghost" :points="outline" :fill="`url(#${uid}-glass)`" />

    <g v-if="variant === 'assemble'">
      <polygon
        v-for="f in geometry.facets" :key="f.id"
        class="facet" :class="{ placed: isPlaced(f) }"
        :points="toPoints(f.points)" :style="facetStyle(f)"
      />
    </g>

    <g v-else-if="variant === 'shards'" :clip-path="`url(#${uid}-outline)`">
      <polygon
        v-for="f in geometry.facets" :key="f.id"
        class="facet shard" :class="{ placed: isPlaced(f) }"
        :points="toPoints(f.points)" :style="facetStyle(f)"
      />
    </g>

    <g v-else>
      <polygon
        v-for="f in geometry.facets" :key="`g${f.id}`"
        class="facet heal-glass" :points="toPoints(f.points)" :style="facetStyle(f)"
      />
      <g :clip-path="`url(#${uid}-fill)`">
        <polygon
          v-for="f in geometry.facets" :key="`c${f.id}`"
          class="facet heal-color" :points="toPoints(f.points)"
          :style="{ ...facetStyle(f), fill: facetColor(f) }"
        />
      </g>
      <g v-if="!complete" :clip-path="`url(#${uid}-outline)`">
        <path
          v-for="(d, i) in geometry.cracks" :key="i"
          class="crack" :d="d" pathLength="1"
          :style="{ strokeDashoffset: p, opacity: 1 - p * 0.9 }"
        />
      </g>
    </g>

    <polygon class="outline" :points="outline" />

    <template v-if="effects">
      <g :clip-path="`url(#${uid}-outline)`">
        <rect class="shine" x="-60" y="-20" width="40" height="140" :fill="`url(#${uid}-shine)`" />
      </g>
      <g class="sparkles">
        <path d="M28 18 l2 6 l6 2 l-6 2 l-2 6 l-2 -6 l-6 -2 l6 -2 z" />
        <path d="M76 68 l1.5 4.5 l4.5 1.5 l-4.5 1.5 l-1.5 4.5 l-1.5 -4.5 l-4.5 -1.5 l4.5 -1.5 z" />
        <path d="M70 12 l1 3 l3 1 l-3 1 l-1 3 l-1 -3 l-3 -1 l3 -1 z" />
      </g>
    </template>
  </svg>
</template>

<style scoped>
.crystal-icon {
  --glass: rgba(255, 255, 255, 0.07);
  display: block;
  overflow: visible;
}

.ghost {
  stroke: rgba(255, 255, 255, 0.18);
  stroke-width: 0.5;
  stroke-dasharray: 2 2;
}

.facet {
  stroke: rgba(255, 255, 255, 0.45);
  stroke-width: 0.35;
  stroke-linejoin: round;
  transform-box: fill-box;
  transform-origin: center;
  transition:
    transform 0.75s cubic-bezier(0.2, 1.35, 0.4, 1),
    fill 0.6s ease,
    stroke 0.6s ease;
}

.facet.placed { stroke: rgba(255, 255, 255, 0.28); }

/* Нерасставленный осколок чуть подрагивает в воздухе */
.variant-assemble .facet:not(.placed) { animation: drift 2.4s ease-in-out infinite alternate; }
.facet:not(.placed):nth-child(3n) { animation-duration: 3.1s; }
.facet:not(.placed):nth-child(3n + 1) { animation-duration: 1.9s; animation-delay: -0.8s; }

@keyframes drift {
  from { translate: 0 -0.8px; }
  to { translate: 0 0.8px; }
}

.shard { stroke-width: 0.25; }

.heal-glass { fill: var(--glass); transition: transform 0.4s ease; }
.heal-color { stroke: rgba(255, 255, 255, 0.25); transition: transform 0.4s ease; }

.wave {
  transition: d 0.4s ease;
  animation: wave-move 2.2s linear infinite;
}

.complete .wave { animation: none; }

@keyframes wave-move {
  from { transform: translateX(0); }
  to { transform: translateX(50px); }
}

.crack {
  fill: none;
  stroke: rgba(235, 248, 255, 0.85);
  stroke-width: 0.6;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-dasharray: 1 1;
  filter: drop-shadow(0 0 0.6px #fff);
  transition: stroke-dashoffset 0.4s ease, opacity 0.4s ease;
}

.outline {
  fill: none;
  stroke: rgba(255, 255, 255, 0.55);
  stroke-width: 0.8;
  stroke-linejoin: round;
}

.shine { opacity: 0; }

.complete .shine {
  opacity: 1;
  animation: shine-run 2.8s ease-in-out 0.3s infinite;
}

@keyframes shine-run {
  0% { transform: translateX(0) skewX(-20deg); }
  45%, 100% { transform: translateX(200px) skewX(-20deg); }
}

.sparkles path {
  fill: #fff;
  opacity: 0;
  transform-box: fill-box;
  transform-origin: center;
}

.complete .sparkles path { animation: sparkle 2.8s ease-in-out infinite; }
.complete .sparkles path:nth-child(2) { animation-delay: 0.9s; }
.complete .sparkles path:nth-child(3) { animation-delay: 1.7s; }

@keyframes sparkle {
  0%, 100% { opacity: 0; transform: scale(0.2) rotate(0deg); }
  15% { opacity: 1; transform: scale(1) rotate(45deg); }
  35% { opacity: 0; transform: scale(0.3) rotate(90deg); }
}
</style>
