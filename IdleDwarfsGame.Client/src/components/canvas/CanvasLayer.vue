<script lang="ts" setup>
import { ref, onMounted, onUnmounted, provide } from 'vue';
import { GlobalParticleService } from '@/services/particleService/ParticleSystem';
import type { PixiFxRenderer } from '@/services/pixiFx/PixiFxRenderer';

const wrapperRef = ref<HTMLElement | null>(null); // Ссылка на саму обертку
const bgCanvasRef = ref<HTMLCanvasElement | null>(null);
const fgCanvasRef = ref<HTMLCanvasElement | null>(null);
// Pixi-канвасы (prop pixi): лежат над своими canvas-слоями, перенесённые эффекты рисуются в них
const pixiBgRef = ref<HTMLCanvasElement | null>(null);
const pixiFgRef = ref<HTMLCanvasElement | null>(null);
let pixiRenderer: PixiFxRenderer | null = null;
let unmounted = false;

const particleService = new GlobalParticleService();
provide('particleService', particleService);
defineExpose({ particleService });

// Счётчик изменений раскладки: CanvasComponent пересчитывает по нему координаты своих эмиттеров
const layoutVersion = ref(0);
provide('particleLayoutVersion', layoutVersion);

let resizeObserver: ResizeObserver | null = null;
let resizeFrame = 0;

interface Props { // Теперь всё передается только сюда
  page?: boolean;
  // Перенесённые эффекты рисует Pixi (WebGL). Пока только у арены и тестов: у браузера ~16 живых WebGL-контекстов,
  // а модалок с CanvasLayer много (docs/pixi-migration.md, этап 4)
  pixi?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  page: true,
  pixi: false,
});

const handleResize = () => {
  // Теперь мы берем размеры не всего окна, а конкретно этой обертки
  if (wrapperRef.value && bgCanvasRef.value && fgCanvasRef.value) {
    // Размер в вёрстке, а не getBoundingClientRect: тот уже умножен на масштаб интерфейса (.ui-stage, UseUiScale.ts),
    // а канвас внутри сцены масштабируется вместе с ней — получилось бы масштабирование дважды
    const width = wrapperRef.value.offsetWidth * 2;
    const height = wrapperRef.value.offsetHeight * 2;
    // Присваивание width/height очищает канвас, поэтому трогаем только при реальном изменении
    for (const canvas of [bgCanvasRef.value, fgCanvasRef.value]) {
      if (canvas.width !== width) canvas.width = width;
      if (canvas.height !== height) canvas.height = height;
    }
    pixiRenderer?.resize(width, height);
  }
  layoutVersion.value++;
};

// Не чаще раза за кадр: при перетаскивании края окна resize приходит пачками
const scheduleResize = () => {
  cancelAnimationFrame(resizeFrame);
  resizeFrame = requestAnimationFrame(handleResize);
};

onMounted(() => {
  if (wrapperRef.value && bgCanvasRef.value && fgCanvasRef.value) {
    particleService.init(bgCanvasRef.value, fgCanvasRef.value);

    // ResizeObserver ловит изменение размера самой обёртки (в т.ч. в модалке),
    // window resize — сдвиги содержимого, когда размер обёртки не поменялся
    resizeObserver = new ResizeObserver(scheduleResize);
    resizeObserver.observe(wrapperRef.value);
    window.addEventListener('resize', scheduleResize);

    handleResize(); // Задаем начальный размер
    if (props.pixi) initPixi();
  }
});

// Pixi грузится лениво: в основной бандл он не попадает. Пока грузится — всё рисует canvas
const initPixi = async () => {
  if (!pixiBgRef.value || !pixiFgRef.value || !bgCanvasRef.value) return;
  try {
    const { PixiFxRenderer } = await import(/* webpackChunkName: "pixi-fx" */ '@/services/pixiFx/PixiFxRenderer');
    const renderer = await PixiFxRenderer.create(pixiBgRef.value, pixiFgRef.value, bgCanvasRef.value.width, bgCanvasRef.value.height);
    if (unmounted) {
      renderer.destroy();
      return;
    }
    pixiRenderer = renderer;
    particleService.attachPixi(renderer);
  } catch (error) {
    // Нет WebGL — остаёмся на canvas
    console.warn('Pixi effects are unavailable, falling back to canvas', error);
  }
};

onUnmounted(() => {
  unmounted = true;
  particleService.destroy();
  pixiRenderer?.destroy();
  pixiRenderer = null;
  cancelAnimationFrame(resizeFrame);
  resizeObserver?.disconnect();
  window.removeEventListener('resize', scheduleResize);
});
</script>

<template>
  <div ref="wrapperRef" class="particle-layer-wrapper">
    <canvas ref="bgCanvasRef" class="particle-canvas canvas-bg"></canvas>
    
    <!-- <div v-if="props.page" class="layer-content">
      <slot></slot>
    </div> -->
    <div class="layer-content-single">
      <slot></slot>
    </div>


    <canvas ref="fgCanvasRef" class="particle-canvas canvas-fg"></canvas>
    <template v-if="props.pixi">
      <canvas ref="pixiBgRef" class="particle-canvas pixi-bg"></canvas>
      <canvas ref="pixiFgRef" class="particle-canvas pixi-fg"></canvas>
    </template>
  </div>
</template>

<style scoped>
.particle-layer-wrapper { /* Обязательно! Чтобы absolute канвасы не улетали */
  position: relative;
  width: 100%;
  height: 100%;
}

.layer-content {
  position: relative;
  z-index: 10;
  width: 100%;
  height: 100%;
}

.layer-content-single {
  position: relative;
  z-index: 10;
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
}

.particle-canvas {
  position: absolute; /* Теперь канвас лежит строго по размеру обертки */
  top: -50%;
  left: -50%;
  pointer-events: none;
}

.canvas-bg {
  z-index: 0; 
}

.canvas-fg {
  z-index: 9999; 
}

/* Pixi — сразу над своим canvas-слоем: под юнитами и над всем */
.pixi-bg {
  z-index: 1;
}

.pixi-fg {
  z-index: 10000;
}
</style>