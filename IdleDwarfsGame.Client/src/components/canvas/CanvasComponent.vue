<script lang="ts" setup>
import { ref, onMounted, onUnmounted, inject, watch } from 'vue';
import type { Ref } from 'vue';
import { EffectType } from '@/services/particleService/types/interfaces/canvas-interfaces';

export interface EffectConfig {
  func: EffectType;
  padding?: number;
  isUnder?: boolean;
  args?: Record<string, any>;
  from?: HTMLElement | { value?: HTMLElement | null } | { $el?: HTMLElement | null } | string | number | null;
  to?: HTMLElement | { value?: HTMLElement | null } | { $el?: HTMLElement | null } | string | number | null;
}

interface Props {
  effects?: EffectConfig[];
  active?: boolean;
  animationKey?: string | number | boolean;
}

const props = withDefaults(defineProps<Props>(), {
  active: true,
  animationKey: undefined,
});

const particleService = inject<any>('particleService');
const containerRef = ref<HTMLElement | null>(null);
const emitterMap = ref(new Map<string, string>());
const refKeyMap = new WeakMap<object, string>();
let refSequence = 0;

const objectKey = (value: unknown) => {
  if (value && typeof value === 'object') {
    const existing = refKeyMap.get(value as object);
    if (existing) return existing;

    const next = `obj-${++refSequence}`;
    refKeyMap.set(value as object, next);
    return next;
  }

  return String(value ?? '');
};

const resolveElement = (value: unknown): HTMLElement | null => {
  if (!value) return null;

  if (value instanceof HTMLElement) return value;

  if (typeof value === 'object') {
    if ('$el' in value && (value as { $el?: unknown }).$el instanceof HTMLElement) {
      return (value as { $el: HTMLElement }).$el;
    }

    if ('value' in value) {
      const nested = (value as { value?: unknown }).value;
      if (nested instanceof HTMLElement) return nested;
      if (nested && typeof nested === 'object' && '$el' in nested && (nested as { $el?: unknown }).$el instanceof HTMLElement) {
        return (nested as { $el: HTMLElement }).$el;
      }
    }
  }

  return null;
};

const resolveCenter = (value: unknown) => {
  const element = resolveElement(value);
  if (!element) return null;

  const rect = element.getBoundingClientRect();
  return {
    x: rect.left + rect.width / 2,
    y: rect.top + rect.height / 2,
  };
};

const effectKey = (e: EffectConfig) =>
  `${e.func}|${e.padding ?? ''}|${e.isUnder ? 1 : 0}|${objectKey(e.from)}|${objectKey(e.to)}`;

const layerOf = (effect: EffectConfig) => (effect.isUnder === false ? 'foreground' : 'background');

// Координаты эффекта в системе канваса его слоя: центр компонента либо from/to-элементы
const resolvePoints = (effect: EffectConfig, rect: DOMRect) => {
  const canvasNode = layerOf(effect) === 'foreground' ? particleService?.fgCanvas : particleService?.bgCanvas;
  const canvasRect = canvasNode?.getBoundingClientRect();
  const offsetX = canvasRect?.left ?? 0;
  const offsetY = canvasRect?.top ?? 0;

  const localX = rect.left + rect.width / 2 - offsetX;
  const localY = rect.top + rect.height / 2 - offsetY;

  const fromPoint = resolveCenter(effect.from ?? effect.args?.from);
  const toPoint = resolveCenter(effect.to ?? effect.args?.to);

  return {
    startX: fromPoint ? fromPoint.x - offsetX : localX,
    startY: fromPoint ? fromPoint.y - offsetY : localY,
    endX: toPoint ? toPoint.x - offsetX : localX,
    endY: toPoint ? toPoint.y - offsetY : localY,
  };
};

// После ресайза окна/обёртки двигаем уже запущенные эмиттеры, не перезапуская их анимацию
const repositionEmitters = () => {
  if (!containerRef.value || !emitterMap.value.size) return;
  const rect = containerRef.value.getBoundingClientRect();

  (props.effects || []).forEach(effect => {
    const id = emitterMap.value.get(effectKey(effect));
    if (!id) return;
    const points = resolvePoints(effect, rect);
    particleService?.updateEmitter(id, { ...points, x: points.startX, y: points.startY });
  });
};

const layoutVersion = inject<Ref<number> | null>('particleLayoutVersion', null);
if (layoutVersion) watch(layoutVersion, repositionEmitters);

const registerEmitters = () => {
  const newKeys = new Set((props.effects || []).map(effectKey));

  for (const [key, id] of emitterMap.value.entries()) {
    if (!newKeys.has(key)) {
      particleService?.removeEmitter(id, 200);
      emitterMap.value.delete(key);
    }
  }

  if (containerRef.value && props.active && props.effects?.length) {
    setTimeout(() => {
      if (!containerRef.value) return;

      const rect = containerRef.value.getBoundingClientRect();

      props.effects.forEach(effect => {
        const key = effectKey(effect);
        const layer = layerOf(effect);
        const { startX, startY, endX, endY } = resolvePoints(effect, rect);

        if (emitterMap.value.has(key)) {
          const id = emitterMap.value.get(key)!;
          particleService?.addEmitter({
            id,
            x: startX,
            y: startY,
            startX,
            startY,
            endX,
            endY,
            type: effect.func,
            padding: effect.padding || 40,
            layer,
            args: effect.args,
            travelStartedAt: performance.now(),
            travelDuration: Number(effect.args?.duration ?? 600),
          });
        } else {
          const id = Math.random().toString(36).substring(7);
          emitterMap.value.set(key, id);
          particleService?.addEmitter({
            id,
            x: startX,
            y: startY,
            startX,
            startY,
            endX,
            endY,
            type: effect.func,
            padding: effect.padding || 40,
            layer,
            args: effect.args,
            travelStartedAt: performance.now(),
            travelDuration: Number(effect.args?.duration ?? 600),
          });
        }
      });
    }, 0);
  }
};

onMounted(registerEmitters);

watch(
  () => [props.active, props.effects, props.animationKey],
  registerEmitters,
  { deep: true }
);

onUnmounted(() => {
  for (const id of emitterMap.value.values()) {
    particleService?.removeEmitter(id, 200);
  }
  emitterMap.value.clear();
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
