<script lang="ts" setup>
// Облачко реплики над персонажем. Хвостик смотрит вниз, на говорящего.
// Позиционирует облачко родитель; здесь только вид и анимация появления.
interface Props {
  text: string;
}

defineProps<Props>();
</script>

<template>
  <div class="dialogue-bubble">
    <div class="dialogue-bubble-inner">{{ text }}</div>
  </div>
</template>

<style scoped>
/* Металлическая рамка как у панели статусов, внутри — пергамент */
.dialogue-bubble {
  position: relative;
  width: max-content;
  max-width: 190px;
  padding: 2px;
  border-radius: 10px;
  background: var(--metalic-background);
  border: 0.7px solid rgba(0, 0, 0, 0.2);
  box-shadow:
    0 4px 10px rgba(0, 0, 0, 0.55),
    inset 0 1px 2px rgba(255, 255, 255, 0.8);
  transform-origin: 50% calc(100% + 9px);
  animation: bubble-pop 0.28s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.dialogue-bubble-inner {
  padding: 6px 10px;
  border-radius: 8px;
  background: radial-gradient(ellipse at 50% 30%, #fbf4de 0%, #efe2bd 70%, #dfcd9c 100%);
  box-shadow: inset 0 0 4px rgba(90, 60, 20, 0.45);
  color: #2b1d0e;
  font-size: 13px;
  font-weight: 700;
  line-height: 1.3;
  text-align: center;
  white-space: normal;
  overflow-wrap: break-word;
}

/* Хвостик: внешний треугольник — рамка, внутренний — пергамент */
.dialogue-bubble::before,
.dialogue-bubble::after {
  content: '';
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
  border-style: solid;
  border-color: transparent;
}

.dialogue-bubble::before {
  top: 100%;
  border-width: 10px 9px 0 9px;
  border-top-color: #a9a9a9;
  filter: drop-shadow(0 2px 2px rgba(0, 0, 0, 0.45));
}

.dialogue-bubble::after {
  top: calc(100% - 2px);
  border-width: 9px 7px 0 7px;
  border-top-color: #e4d3a5;
}

@keyframes bubble-pop {
  from { opacity: 0; transform: scale(0.5) translateY(6px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}
</style>
