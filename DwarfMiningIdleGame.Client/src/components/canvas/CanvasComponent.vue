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
    const rect = containerRef.value.getBoundingClientRect();
    
    // Получаем абсолютные координаты центра кнопки на экране
    let centerX = rect.left + rect.width / 2;
    let centerY = rect.top + rect.height / 2;

    props.effects.forEach(effect => {
      const id = Math.random().toString(36).substring(7);
      emitterIds.value.push(id);
      
      const layer = effect.isUnder === false ? 'foreground' : 'background';

      // --- ВОТ ЭТО ИСПРАВЛЯЕТ СМЕЩЕНИЕ ---
      // Получаем инстанс нужного канваса из сервиса
      const canvasNode = layer === 'foreground' ? particleService?.fgCanvas : particleService?.bgCanvas;
      
      if (canvasNode) {
        const canvasRect = canvasNode.getBoundingClientRect();
        // Вычитаем позицию канваса, чтобы получить координаты относительно ВНУТРЕННОСТЕЙ холста
        centerX -= canvasRect.left;
        centerY -= canvasRect.top;
      }
      // -----------------------------------

      particleService?.addEmitter({
        id,
        x: centerX,
        y: centerY,
        type: effect.func,
        padding: effect.padding || 40,
        layer
      });
    });
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