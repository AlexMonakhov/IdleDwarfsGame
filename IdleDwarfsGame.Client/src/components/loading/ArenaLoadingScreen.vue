<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { arenaPreview } from '@/battle/arenaBackgrounds';
import CrystalBar from './CrystalBar.vue';
import { gemColorVar, type GemColor } from './gemPalette';

// Экран загрузки арены: пещера этой арены в дымке, свет и кристальная пыль ее цвета,
// по центру — полоса-кристалл из осколков (CrystalBar shards) и подсказки
interface Props {
  // Идет загрузка (запрос боя, фон, спрайт-листы)
  active: boolean
  // Реальный прогресс по этапам 0..1; между этапами полоса сама медленно ползет вперед
  progress: number
  // Ключ арены с сервера (lava, ice...) — от него фон и цвет кристалла
  background?: string | null
  // Ошибка запуска боя — экран убираем сразу, без досборки до 100%
  failed?: boolean
}

const props = withDefaults(defineProps<Props>(), { background: null, failed: false });
// open — экран появился; closed — полностью исчез (досборка, вспышка и затухание позади): можно начинать бой
const emit = defineEmits<{ open: []; closed: [] }>();

const { t, tm, rt } = useI18n();

// Цвета кристалла под стихию арены: [основной, к чему переливается]
const ARENA_COLORS: Record<string, [GemColor, GemColor]> = {
  lava: ['red', 'yellow'],
  ice: ['blue', 'white'],
  storm: ['yellow', 'white'],
  marsh: ['green', 'yellow'],
  crypt: ['purple', 'blue'],
  sanctum: ['white', 'yellow'],
  altar: ['purple', 'pink'],
  colosseum: ['purple', 'pink'],
};
const colors = computed(() => ARENA_COLORS[props.background ?? ''] ?? ARENA_COLORS.lava);
const cave = computed(() => arenaPreview(props.background));

// ---- Видимость и плавный прогресс ----
const visible = ref(props.active);
const shown = ref(0);
let timer = 0;
let hideTimer = 0;

const step = () => {
  const target = props.active ? props.progress : 1;
  if (shown.value < target) {
    shown.value = Math.min(target, shown.value + Math.max((target - shown.value) * 0.14, 0.004));
  } else if (props.active) {
    // Этап затянулся — полоса все равно понемногу ползет, но не уходит далеко за этап
    const cap = Math.min(target + 0.08, 0.97);
    if (shown.value < cap) shown.value += (cap - shown.value) * 0.012;
  }
  if (!props.active && shown.value >= 1 && !hideTimer) {
    // Даем увидеть собранный кристалл со вспышкой, потом гасим экран
    hideTimer = window.setTimeout(() => { visible.value = false; stop(); }, 550);
  }
};

const start = () => {
  clearTimeout(hideTimer);
  hideTimer = 0;
  if (!timer) timer = window.setInterval(step, 50);
};
const stop = () => {
  clearInterval(timer);
  clearTimeout(hideTimer);
  timer = 0;
  hideTimer = 0;
};

watch(() => props.active, (active) => {
  if (active) {
    if (!visible.value) shown.value = 0;
    visible.value = true;
    start();
  } else if (props.failed) {
    visible.value = false;
    stop();
  }
}, { immediate: true });

onBeforeUnmount(stop);

watch(visible, (v) => { if (v) emit('open'); }, { immediate: true });

// ---- Подсказки: меняются по кругу, первая — случайная ----
const tips = computed(() => (tm('arena.loadingTips') as unknown[]).map(tip => rt(tip as never)));
const tipIndex = ref(0);
let tipTimer = 0;
watch(visible, (v) => {
  clearInterval(tipTimer);
  if (!v || !tips.value.length) return;
  tipIndex.value = Math.floor(Math.random() * tips.value.length);
  tipTimer = window.setInterval(() => { tipIndex.value = (tipIndex.value + 1) % tips.value.length; }, 4200);
}, { immediate: true });
onBeforeUnmount(() => clearInterval(tipTimer));

// ---- Кристальная пыль: генерируется один раз ----
const motes = Array.from({ length: 26 }, (_, i) => ({
  id: i,
  left: `${Math.random() * 100}%`,
  size: `${4 + Math.random() * 9}px`,
  duration: `${7 + Math.random() * 9}s`,
  delay: `${-Math.random() * 16}s`,
  sway: `${(Math.random() - 0.5) * 120}px`,
  white: Math.random() < 0.35,
}));

const rootStyle = computed(() => ({
  '--arena-a': gemColorVar(colors.value[0]),
  '--arena-b': gemColorVar(colors.value[1]),
}));
</script>

<template>
  <Transition name="arena-loading" @after-leave="emit('closed')">
    <div v-if="visible" class="arena-loading-screen" :style="rootStyle">
      <!-- Пещера арены в дымке; при смене арены — плавная смена картинки -->
      <TransitionGroup name="cave-fade" tag="div" class="loading-cave-wrap">
        <div :key="cave" class="loading-cave" :style="{ backgroundImage: `url(${cave})` }" />
      </TransitionGroup>
      <div class="loading-glow" />
      <div class="loading-rays" />
      <div class="loading-motes">
        <span
          v-for="m in motes" :key="m.id" class="mote" :class="{ white: m.white }"
          :style="{ left: m.left, width: m.size, height: m.size, animationDuration: m.duration, animationDelay: m.delay, '--sway': m.sway }"
        />
      </div>
      <div class="loading-vignette" />

      <div class="loading-center">
        <div class="loading-title">{{ t('arena.loadingSprites') }}</div>
        <CrystalBar
          class="loading-bar"
          :progress="shown" variant="shards"
          :color="colors[0]" :gradient-to="colors[1]"
          counter="end" width="min(560px, 70vw)"
        />
        <Transition name="tip-fade" mode="out-in">
          <div v-if="tips.length" :key="tipIndex" class="loading-tip">{{ tips[tipIndex] }}</div>
        </Transition>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
.arena-loading-screen {
  position: absolute;
  inset: 0;
  z-index: 10000;
  overflow: hidden;
  background: #070a10;
}

.arena-loading-leave-active { transition: opacity 0.45s ease; }
.arena-loading-leave-to { opacity: 0; }

/* Пещера: размытая, притемненная, медленно наезжает камерой */
.loading-cave-wrap {
  position: absolute;
  inset: 0;
}

.loading-cave {
  position: absolute;
  inset: -6%;
  background-size: cover;
  background-position: center;
  filter: blur(6px) brightness(0.42) saturate(1.2);
  animation: cave-drift 24s ease-in-out infinite alternate;
}

@keyframes cave-drift {
  from { transform: scale(1.05) translate(-1.5%, 0.5%); }
  to { transform: scale(1.16) translate(1.5%, -1%); }
}

.cave-fade-enter-active,
.cave-fade-leave-active { transition: opacity 0.8s ease; }
.cave-fade-enter-from,
.cave-fade-leave-to { opacity: 0; }

/* Свечение стихии снизу, «дышит» */
.loading-glow {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse 70% 55% at 50% 100%, color-mix(in srgb, var(--arena-a) 45%, transparent) 0%, transparent 70%),
    radial-gradient(ellipse 40% 30% at 50% 52%, color-mix(in srgb, var(--arena-b) 18%, transparent) 0%, transparent 80%);
  mix-blend-mode: screen;
  animation: glow-breathe 4s ease-in-out infinite alternate;
}

@keyframes glow-breathe {
  from { opacity: 0.65; }
  to { opacity: 1; }
}

/* Лучи сверху: тонкий веер из-за верхнего края, медленно поворачивается */
.loading-rays {
  position: absolute;
  left: 50%;
  top: -40%;
  width: 120%;
  aspect-ratio: 1;
  translate: -50% -50%;
  background: repeating-conic-gradient(
    from 0deg at 50% 50%,
    color-mix(in srgb, var(--arena-b) 9%, transparent) 0deg 3deg,
    transparent 3deg 14deg
  );
  -webkit-mask: radial-gradient(circle at 50% 50%, transparent 20%, #000 38%, transparent 70%);
  mask: radial-gradient(circle at 50% 50%, transparent 20%, #000 38%, transparent 70%);
  mix-blend-mode: screen;
  animation: rays-turn 90s linear infinite;
}

@keyframes rays-turn {
  to { rotate: 360deg; }
}

/* Кристальная пыль: ромбики всплывают снизу вверх, покачиваясь */
.loading-motes {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.mote {
  position: absolute;
  bottom: -20px;
  rotate: 45deg;
  background: linear-gradient(135deg, #fff 0%, var(--arena-a) 60%);
  box-shadow: 0 0 8px var(--arena-a);
  border-radius: 1px;
  opacity: 0;
  animation-name: mote-rise;
  animation-timing-function: linear;
  animation-iteration-count: infinite;
}

.mote.white {
  background: linear-gradient(135deg, #fff 0%, var(--arena-b) 70%);
  box-shadow: 0 0 6px #fff;
}

@keyframes mote-rise {
  0% { transform: translate(0, 0) scale(0.6); opacity: 0; }
  10% { opacity: 0.85; }
  50% { transform: translate(var(--sway), -55vh) scale(1); }
  85% { opacity: 0.6; }
  100% { transform: translate(0, -110vh) scale(0.4); opacity: 0; }
}

.loading-vignette {
  position: absolute;
  inset: 0;
  background: radial-gradient(ellipse at center, transparent 35%, rgba(0, 0, 0, 0.75) 100%);
  pointer-events: none;
}

/* Центр: заголовок, полоса, подсказка */
.loading-center {
  position: absolute;
  left: 50%;
  top: 50%;
  translate: -50% -50%;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 18px;
}

.loading-title {
  color: #fff;
  font-family: 'Arial', sans-serif;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: 6px;
  text-transform: uppercase;
  text-shadow: 0 0 12px color-mix(in srgb, var(--arena-a) 80%, transparent), 1px 1px 0 rgba(0, 0, 0, 0.7);
  animation: title-pulse 1.6s ease-in-out infinite alternate;
}

@keyframes title-pulse {
  from { opacity: 0.6; }
  to { opacity: 1; }
}

.loading-bar {
  filter: drop-shadow(0 0 18px color-mix(in srgb, var(--arena-a) 35%, transparent));
}

.loading-tip {
  max-width: min(560px, 80vw);
  min-height: 2.6em;
  color: rgba(255, 255, 255, 0.85);
  font-family: 'Arial', sans-serif;
  font-size: 14px;
  line-height: 1.3;
  text-align: center;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.tip-fade-enter-active,
.tip-fade-leave-active { transition: opacity 0.35s ease, transform 0.35s ease; }
.tip-fade-enter-from { opacity: 0; transform: translateY(6px); }
.tip-fade-leave-to { opacity: 0; transform: translateY(-6px); }
</style>
