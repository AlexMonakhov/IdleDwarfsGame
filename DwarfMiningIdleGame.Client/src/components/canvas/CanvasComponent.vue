<script lang="ts" setup>
import { ref, onMounted, onUnmounted, inject, watch } from 'vue';
import { EffectType } from '@/services/particleService/types/interfaces/canvas-interfaces';

export interface EffectConfig {
  func: EffectType;
  padding?: number;
  isUnder?: boolean;
}

interface Props {
  effects: EffectConfig[]; // Теперь всё передается только сюда
  active?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  active: true,
});

const particleService = inject<any>('particleService');
const containerRef = ref<HTMLElement | null>(null);
const emitterIds = ref<string[]>([]);

const registerEmitters = () => {
  emitterIds.value.forEach(id => particleService?.removeEmitter(id));
  emitterIds.value = [];

  if (containerRef.value && props.active && props.effects?.length) {
    setTimeout(() => {
      if (!containerRef.value) return;
      
      const rect = containerRef.value.getBoundingClientRect();
      
      // Используем const, чтобы не менять оригинальные экранные координаты
      const btnScreenCenterX = rect.left + rect.width / 2;
      const btnScreenCenterY = rect.top + rect.height / 2;

      props.effects.forEach(effect => {
        const id = Math.random().toString(36).substring(7);
        emitterIds.value.push(id);
        
        const layer = effect.isUnder === false ? 'foreground' : 'background';
        const canvasNode = layer === 'foreground' ? particleService?.fgCanvas : particleService?.bgCanvas;
        
        // Создаем локальные переменные для текущего эффекта
        let localX = btnScreenCenterX;
        let localY = btnScreenCenterY;
        
        if (canvasNode) {
          const canvasRect = canvasNode.getBoundingClientRect();
          localX -= canvasRect.left;
          localY -= canvasRect.top;
        }

        particleService?.addEmitter({
          id,
          x: localX,
          y: localY,
          type: effect.func,
          padding: effect.padding || 40,
          layer
        });
      });
    }, 300);
  }
};


onMounted(registerEmitters);

// Следим за изменениями массива эффектов и активности
watch(
  () => [props.active, props.effects], 
  registerEmitters, 
  { deep: true }
);

onUnmounted(() => {
  emitterIds.value.forEach(id => particleService?.removeEmitter(id));
});
</script>

<template>
  <div ref="containerRef" class="canvas-wrapper">
    <slot></slot>
  </div>
</template>

<style scoped>
.canvas-wrapper {
  position: relative;
  display: inline-block;
}
</style>