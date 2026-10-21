<script lang="ts" setup>
import { ref, onMounted, onUnmounted, provide } from 'vue';
import { GlobalParticleService } from '@/services/particleService/ParticleSystem';

const wrapperRef = ref<HTMLElement | null>(null); // Ссылка на саму обертку
const bgCanvasRef = ref<HTMLCanvasElement | null>(null);
const fgCanvasRef = ref<HTMLCanvasElement | null>(null);

const particleService = new GlobalParticleService();
provide('particleService', particleService);

let resizeObserver: ResizeObserver | null = null;

interface Props { // Теперь всё передается только сюда
  page?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  page: true,
});

const handleResize = () => {
  // Теперь мы берем размеры не всего окна, а конкретно этой обертки
  if (wrapperRef.value && bgCanvasRef.value && fgCanvasRef.value) {
    console.log(wrapperRef.value);
    const rect = wrapperRef.value.getBoundingClientRect();
    bgCanvasRef.value.width = rect.width * 2;
    bgCanvasRef.value.height = rect.height * 2;
    fgCanvasRef.value.width = rect.width * 2;
    fgCanvasRef.value.height = rect.height * 2;
  }
};

onMounted(() => {
  if (wrapperRef.value && bgCanvasRef.value && fgCanvasRef.value) {
    particleService.init(bgCanvasRef.value, fgCanvasRef.value);
    
    // Используем ResizeObserver вместо window.addEventListener('resize')
    // Он будет реагировать даже если модалка плавно меняет размер при открытии
    // resizeObserver = new ResizeObserver(handleResize);
    // resizeObserver.observe(wrapperRef.value);
    
    handleResize(); // Задаем начальный размер
  }
});

onUnmounted(() => {
  particleService.destroy();
  // if (resizeObserver) resizeObserver.disconnect();
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