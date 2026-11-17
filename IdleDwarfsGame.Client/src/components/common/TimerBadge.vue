<script lang="ts" setup>
// Таймер обратного отсчёта в окнах: тёмная плашка, жёлтые цифры, песочные часы (как сброс боссов гильдии).
// Текст — пропсом text или в слоте; danger — красный, когда отсчёт до чего-то неприятного (закрытие лавки)
withDefaults(defineProps<{
    text?: string;
    danger?: boolean;
}>(), {
    text: '',
    danger: false,
});
</script>

<template>
    <span class="timer-badge" :class="{ 'timer-badge-danger': danger }">⏳ <slot>{{ text }}</slot></span>
</template>

<style scoped>
.timer-badge {
    flex-shrink: 0;
    padding: 2px 8px;
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.65);
    color: #fde68a;
    font-size: 12px;
    font-weight: bold;
    font-variant-numeric: tabular-nums;
}

.timer-badge-danger {
    color: #fca5a5;
}
</style>
