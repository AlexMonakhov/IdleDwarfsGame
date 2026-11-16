<script lang="ts" setup>
import { ref, shallowRef, onMounted, onUnmounted, provide } from 'vue';
import { GlobalParticleService } from '@/services/particleService/ParticleSystem';
import type { PixiScene } from '@/services/pixiFx/PixiScene';

// Слой с Pixi-сценой (services/pixiFx/PixiScene): один WebGL-канвас по размеру слоя рисует эффекты под юнитами,
// самих юнитов (CharacterComponent внутри слоя) и эффекты над ними. Содержимое слота (DOM) лежит поверх канваса:
// имена, полоски, цифры урона, интерфейс. Эффекты запускают CanvasComponent и BattleFx через particleService
const wrapperRef = ref<HTMLElement | null>(null);
const canvasRef = ref<HTMLCanvasElement | null>(null);

const particleService = new GlobalParticleService();
provide('particleService', particleService);
// Сцена появляется после ленивой загрузки Pixi — юниты (CharacterComponent) ждут её
const scene = shallowRef<PixiScene | null>(null);
provide('pixiScene', scene);
// Сцену не удалось создать (нет WebGL) — юниты рисуют себя сами, как вне слоя
const sceneFailed = ref(false);
provide('pixiSceneFailed', sceneFailed);
defineExpose({ particleService, scene });

// Счётчик изменений раскладки: CanvasComponent и юниты пересчитывают по нему свои координаты
const layoutVersion = ref(0);
provide('particleLayoutVersion', layoutVersion);

let resizeObserver: ResizeObserver | null = null;
let resizeFrame = 0;
let unmounted = false;

const handleResize = () => {
  // Размер в вёрстке, а не getBoundingClientRect: тот уже умножен на масштаб интерфейса (.ui-stage, UseUiScale.ts),
  // а канвас внутри сцены масштабируется вместе с ней — получилось бы масштабирование дважды
  if (wrapperRef.value) scene.value?.resize(wrapperRef.value.offsetWidth, wrapperRef.value.offsetHeight);
  layoutVersion.value++;
};

// Не чаще раза за кадр: при перетаскивании края окна resize приходит пачками
const scheduleResize = () => {
  cancelAnimationFrame(resizeFrame);
  resizeFrame = requestAnimationFrame(handleResize);
};

// Pixi грузится лениво: в основной бандл он не попадает
const initScene = async () => {
  if (!canvasRef.value || !wrapperRef.value) return;
  try {
    const { PixiScene } = await import(/* webpackChunkName: "pixi-fx" */ '@/services/pixiFx/PixiScene');
    const created = await PixiScene.create(canvasRef.value, wrapperRef.value.offsetWidth, wrapperRef.value.offsetHeight);
    if (unmounted) {
      created.destroy();
      return;
    }
    scene.value = created;
    particleService.attachScene(created);
    handleResize();
  } catch (error) {
    // Без WebGL эффектов и Pixi-юнитов не будет; CharacterComponent тогда рисует юнитов сам
    console.warn('Pixi scene is unavailable', error);
    sceneFailed.value = true;
  }
};

onMounted(() => {
  if (!wrapperRef.value) return;
  particleService.init(wrapperRef.value);
  // ResizeObserver ловит изменение размера самой обёртки (в т.ч. в модалке),
  // window resize — сдвиги содержимого, когда размер обёртки не поменялся
  resizeObserver = new ResizeObserver(scheduleResize);
  resizeObserver.observe(wrapperRef.value);
  window.addEventListener('resize', scheduleResize);
  initScene();
});

onUnmounted(() => {
  unmounted = true;
  particleService.destroy();
  scene.value?.destroy();
  scene.value = null;
  cancelAnimationFrame(resizeFrame);
  resizeObserver?.disconnect();
  window.removeEventListener('resize', scheduleResize);
});
</script>

<template>
  <div ref="wrapperRef" class="particle-layer-wrapper">
    <canvas ref="canvasRef" class="pixi-canvas"></canvas>
    <div class="layer-content-single">
      <slot></slot>
    </div>
  </div>
</template>

<style scoped>
.particle-layer-wrapper {
  position: relative;
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

/* Сцена — ровно по слою, под его содержимым (DOM-интерфейс юнитов и арены — поверх) */
.pixi-canvas {
  position: absolute;
  top: 0;
  left: 0;
  z-index: 1;
  pointer-events: none;
}
</style>
