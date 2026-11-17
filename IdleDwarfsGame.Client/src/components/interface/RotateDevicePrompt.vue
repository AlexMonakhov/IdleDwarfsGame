<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import { rotateRequired } from '@/composables/UseUiScale';

// Телефон держат вертикально: игра (арена, город) рассчитана только на горизонтальный экран.
// Закрываем всё и просим повернуть; после поворота заглушка пропадает сама (UseUiScale следит за размером окна)
const { t } = useI18n();
</script>

<template>
    <Teleport to="body">
        <div v-if="rotateRequired" class="rotate-prompt" role="alertdialog" :aria-label="t('nav.rotateTitle')">
            <div class="rotate-phone" aria-hidden="true">
                <span class="rotate-screen"></span>
            </div>
            <h2 class="rotate-title">{{ t('nav.rotateTitle') }}</h2>
            <p class="rotate-hint">{{ t('nav.rotateHint') }}</p>
        </div>
    </Teleport>
</template>

<style scoped>
/* Выше всего, включая облачко обучения и подсказки */
.rotate-prompt {
    position: fixed;
    inset: 0;
    z-index: 1000000;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    row-gap: 14px;
    padding: 24px;
    box-sizing: border-box;
    background: radial-gradient(ellipse at 50% 40%, #145f6d 0%, #0c3f48 55%, #06262c 100%);
    color: #ffffff;
    font-family: 'Arial', sans-serif;
    text-align: center;
}

/* Телефон в металлической рамке поворачивается из вертикали в горизонталь и обратно */
.rotate-phone {
    width: 64px;
    height: 112px;
    padding: 6px;
    box-sizing: border-box;
    border-radius: 14px;
    background: var(--metalic-background);
    box-shadow: 0 8px 18px rgba(0, 0, 0, 0.5);
    animation: rotate-phone 2.4s ease-in-out infinite;
}

.rotate-screen {
    display: block;
    width: 100%;
    height: 100%;
    border-radius: 8px;
    background: linear-gradient(135deg, var(--blue-gem-color) 0%, #0b4f8a 100%);
    box-shadow: inset 0 0 4px rgba(255, 255, 255, 0.5);
}

@keyframes rotate-phone {
    0%, 20% { transform: rotate(0deg); }
    50%, 80% { transform: rotate(-90deg); }
    100% { transform: rotate(0deg); }
}

@media (prefers-reduced-motion: reduce) {
    .rotate-phone {
        animation: none;
        transform: rotate(-90deg);
    }
}

.rotate-title {
    margin: 18px 0 0;
    font-size: 22px;
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: var(--text-outline);
}

.rotate-hint {
    margin: 0;
    max-width: 280px;
    font-size: 15px;
    line-height: 1.4;
    color: #cfe9ec;
}
</style>
