<script lang="ts" setup>
import { computed } from 'vue';
import UnitAvatar from '@/components/interface/UnitAvatar.vue';
import CanvasAuraEffect from '@/components/effects/CanvasAuraEffect.vue';
import CssAuraEffect from '@/components/effects/CssAuraEffect.vue';
import type { FrameEffect } from '@/models/types/Account';

// Аватар игрока в металлической рамке. Эффект рамки — аура вокруг (components/effects)
// плюс свечение самого кольца. Ауру рисует канвас или CSS — их можно сравнить через renderer
interface Props {
    effect?: FrameEffect;
    size?: number;
    renderer?: 'canvas' | 'css';
}

const props = withDefaults(defineProps<Props>(), {
    effect: 'none',
    size: 56,
    renderer: 'css',
});

// Своих аватаров пока нет — показываем лицо ледяного дворфа
const AVATAR_UNIT = { name: 'Dwarf', unitType: 'dwarf', element: 'Frost', level: 4, isBoss: false };
// Толщина металлического кольца
const RING = 4;

const aura = computed(() => (props.renderer === 'css' ? CssAuraEffect : CanvasAuraEffect));
</script>

<template>
    <div class="avatar-frame" :class="`avatar-frame-${effect}`" :style="{ '--ring': `${RING}px` }">
        <component :is="aura" :effect="effect" :size="size">
            <div class="frame-ring">
                <UnitAvatar :unit="AVATAR_UNIT" :size="size - RING * 2" :show-level="false" />
            </div>
        </component>
        <slot />
    </div>
</template>

<style scoped>
.avatar-frame {
    position: relative;
    flex-shrink: 0;
}

.frame-ring {
    box-sizing: border-box;
    width: 100%;
    height: 100%;
    padding: var(--ring);
    border-radius: 50%;
    background: var(--metalic-background);
    box-shadow: 0 0 0 2px #333, 0 4px 8px rgba(0, 0, 0, 0.6), inset 0 1px 2px rgba(255, 255, 255, 0.8);
    transition: box-shadow 0.3s ease;
}

/* Кольцо UnitAvatar дублировало бы металл рамки — оставляем только тонкую тёмную кромку */
.frame-ring :deep(.portrait) {
    border-width: 1px;
    border-color: #333;
    box-shadow: inset 0 0 8px black;
}

/* ===== Свечение кольца под каждый эффект ===== */
.avatar-frame-snow .frame-ring {
    box-shadow: 0 0 0 2px #335, 0 0 10px 3px rgba(125, 211, 252, 0.8), 0 4px 8px rgba(0, 0, 0, 0.6), inset 0 0 6px 1px #e0f6ff;
}

.avatar-frame-embers .frame-ring {
    animation: frame-ember-glow 2.4s ease-in-out infinite;
}

.avatar-frame-runes .frame-ring {
    box-shadow: 0 0 0 2px #3a2a00, 0 0 10px 2px rgba(255, 209, 102, 0.7), 0 4px 8px rgba(0, 0, 0, 0.6), inset 0 0 5px #ffd166;
}

@keyframes frame-ember-glow {
    0%, 100% { box-shadow: 0 0 0 2px #3a1a00, 0 0 8px 2px rgba(255, 123, 0, 0.6), 0 4px 8px rgba(0, 0, 0, 0.6), inset 0 0 4px #ffb347; }
    50% { box-shadow: 0 0 0 2px #3a1a00, 0 0 14px 4px rgba(255, 90, 0, 0.9), 0 4px 8px rgba(0, 0, 0, 0.6), inset 0 0 6px 1px #ffd166; }
}

@media (prefers-reduced-motion: reduce) {
    .frame-ring {
        animation: none !important;
    }
}
</style>
