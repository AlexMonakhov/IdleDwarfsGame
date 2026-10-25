<script lang="ts" setup>
import { ref, onMounted, onUnmounted, provide } from 'vue';
import { GlobalParticleService } from '@/services/particleService/ParticleSystem';

const wrapperRef = ref<HTMLElement | null>(null); // Ссылка на саму обертку
const bgCanvasRef = ref<HTMLCanvasElement | null>(null);
const fgCanvasRef = ref<HTMLCanvasElement | null>(null);

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
}

const props = withDefaults(defineProps<Props>(), {
  page: true,
});

const handleResize = () => {
  // Теперь мы берем размеры не всего окна, а конкретно этой обертки
  if (wrapperRef.value && bgCanvasRef.value && fgCanvasRef.value) {
    const rect = wrapperRef.value.getBoundingClientRect();
    const width = Math.round(rect.width * 2);
    const height = Math.round(rect.height * 2);
    // Присваивание width/height очищает канвас, поэтому трогаем только при реальном изменении
    for (const canvas of [bgCanvasRef.value, fgCanvasRef.value]) {
      if (canvas.width !== width) canvas.width = width;
      if (canvas.height !== height) canvas.height = height;
    }
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
  }
});

onUnmounted(() => {
  particleService.destroy();
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
</style>