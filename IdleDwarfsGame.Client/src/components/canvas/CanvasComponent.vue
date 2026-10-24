<script lang="ts" setup>
import { ref, onMounted, onUnmounted, inject, watch } from 'vue';
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
      const centerX = rect.left + rect.width / 2;
      const centerY = rect.top + rect.height / 2;

      props.effects.forEach(effect => {
        const key = effectKey(effect);
        const layer = effect.isUnder === false ? 'foreground' : 'background';
        const canvasNode = layer === 'foreground' ? particleService?.fgCanvas : particleService?.bgCanvas;

        const canvasRect = canvasNode?.getBoundingClientRect();

        let localX = centerX;
        let localY = centerY;

        if (canvasRect) {
          localX -= canvasRect.left;
          localY -= canvasRect.top;
        }

        const fromPoint = resolveCenter(effect.from ?? effect.args?.from);
        const toPoint = resolveCenter(effect.to ?? effect.args?.to);

        const startX = fromPoint ? fromPoint.x - (canvasRect?.left ?? 0) : localX;
        const startY = fromPoint ? fromPoint.y - (canvasRect?.top ?? 0) : localY;
        const endX = toPoint ? toPoint.x - (canvasRect?.left ?? 0) : localX;
        const endY = toPoint ? toPoint.y - (canvasRect?.top ?? 0) : localY;

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
