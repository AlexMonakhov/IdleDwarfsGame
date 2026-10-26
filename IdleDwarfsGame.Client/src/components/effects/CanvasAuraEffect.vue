<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch, type CSSProperties } from 'vue';
import type { FrameEffect } from '@/models/types/Account';
import { FRAME_CANVAS_SCALE, mountFrameEffect } from '@/services/frameEffects/FrameEffects';

// Аура вокруг круглого элемента на канвасе: свечение под содержимым слота и частицы поверх.
// API тот же, что у CssAuraEffect — их можно менять местами:
//   <CanvasAuraEffect effect="snow" :size="64"><UnitAvatar ... /></CanvasAuraEffect>
// Сама отрисовка — services/frameEffects
interface Props {
    effect?: FrameEffect;
    size?: number;
}

const props = withDefaults(defineProps<Props>(), {
    effect: 'none',
    size: 56,
});

const backCanvas = ref<HTMLCanvasElement | null>(null);
const frontCanvas = ref<HTMLCanvasElement | null>(null);
const hasEffect = computed(() => props.effect !== 'none');

// Канвас больше элемента и выступает поровну со всех сторон
const OFFSET = `${(-(FRAME_CANVAS_SCALE - 1) / 2) * 100}%`;
const canvasStyle: CSSProperties = {
    left: OFFSET,
    top: OFFSET,
    width: `${FRAME_CANVAS_SCALE * 100}%`,
    height: `${FRAME_CANVAS_SCALE * 100}%`,
};

let stopEffect: (() => void) | null = null;

watch([backCanvas, frontCanvas, () => props.effect, () => props.size], ([back, front, effect, size]) => {
    stopEffect?.();
    stopEffect = back && front ? mountFrameEffect(back, front, effect, size) : null;
}, { immediate: true });

onBeforeUnmount(() => stopEffect?.());
</script>

<template>
    <div class="aura" :style="{ width: `${size}px`, height: `${size}px` }">
        <canvas v-if="hasEffect" ref="backCanvas" class="aura-canvas aura-canvas-back" :style="canvasStyle" aria-hidden="true"></canvas>
        <div class="aura-content">
            <slot />
        </div>
        <canvas v-if="hasEffect" ref="frontCanvas" class="aura-canvas aura-canvas-front" :style="canvasStyle" aria-hidden="true"></canvas>
    </div>
</template>

<style scoped>
.aura {
    position: relative;
}

.aura-content {
    position: relative;
    z-index: 1;
    width: 100%;
    height: 100%;
}

.aura-canvas {
    position: absolute;
    pointer-events: none;
}

.aura-canvas-back {
    z-index: 0;
}

.aura-canvas-front {
    z-index: 2;
}
</style>
