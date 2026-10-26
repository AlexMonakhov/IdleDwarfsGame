<script lang="ts" setup>
// Выпуклое стекло-кристалл в металлической оправе: блик сверху, свет изнутри снизу,
// отблеск пробегает при наведении и при выборе
interface Props {
  // Выбранная: стекло разгорается, подпись золотая
  active?: boolean
  icon?: string
  label?: string
  // Внутри другой кнопки (например, вкладки) рисуем span — кнопку в кнопку вкладывать нельзя
  tag?: 'button' | 'span' | 'div'
}

const props = withDefaults(defineProps<Props>(), {
  active: false,
  tag: 'button'
})
</script>

<template>
    <component
        :is="props.tag"
        :type="props.tag === 'button' ? 'button' : undefined"
        class="glass-button"
        :class="{ 'glass-button-active': props.active }"
    >
        <slot>
            <span v-if="props.icon" class="glass-button-icon">{{ props.icon }}</span>
            <span v-if="props.label" class="glass-button-label">{{ props.label }}</span>
        </slot>
    </component>
</template>

<style scoped>
.glass-button {
  /* Свет внутри кристалла: у обычной приглушён, у выбранной разгорается */
  --glass-glow: rgba(101, 167, 184, 0.35);
  position: relative;
  box-sizing: border-box;
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  row-gap: 4px;
  min-width: 64px;
  min-height: 56px;
  padding: 6px 10px;
  overflow: hidden;
  border: none;
  border-radius: 6px 12px 12px 6px;
  color: #cbd5e1;
  font: inherit;
  cursor: inherit;
  background:
    /* Свечение снизу — свет проходит сквозь толщу стекла */
    radial-gradient(ellipse 90% 60% at 50% 105%, var(--glass-glow) 0%, transparent 70%),
    /* Грани кристалла — как лучи у GemButton, только мягче */
    linear-gradient(45deg,
      rgba(0, 0, 0, 0.18) 0%, rgba(0, 0, 0, 0.05) 25%,
      rgba(255, 255, 255, 0.12) 30%, rgba(255, 255, 255, 0.06) 40%,
      rgba(0, 0, 0, 0.05) 45%, rgba(0, 0, 0, 0.05) 65%,
      rgba(255, 255, 255, 0.12) 70%, rgba(255, 255, 255, 0.06) 75%,
      rgba(0, 0, 0, 0.18) 100%),
    /* Толща стекла; родитель может перекрасить кристалл через --glass-top/mid/bottom */
    linear-gradient(165deg, var(--glass-top, #7a9cc2) 0%, var(--glass-mid, #46638a) 40%, var(--glass-bottom, #22334d) 100%);
  box-shadow:
    /* Оправа */
    0 0 0 4px rgb(157, 173, 175),
    0 0 0 6px #333,
    /* Выпуклость: светлая кромка сверху, тень по низу */
    inset 0 2px 1px rgba(255, 255, 255, 0.6),
    inset 0 -4px 8px rgba(0, 0, 0, 0.45),
    inset 0 0 14px var(--glass-glow);
  transition: box-shadow 0.3s ease, color 0.3s ease;
}

button.glass-button {
  cursor: pointer;
}

/* Глянцевый блик на верхней половине стекла */
.glass-button::before {
  content: '';
  position: absolute;
  top: 2px;
  left: 6%;
  right: 6%;
  height: 46%;
  border-radius: 6px 10px 50% 50% / 6px 10px 30% 30%;
  background: linear-gradient(to bottom, rgba(255, 255, 255, 0.5) 0%, rgba(255, 255, 255, 0.08) 100%);
  pointer-events: none;
}

/* Отблеск, пробегающий по стеклу */
.glass-button::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(115deg, transparent 30%, rgba(255, 255, 255, 0.45) 45%, transparent 60%);
  transform: translateX(-120%);
  transition: transform 0.6s ease;
  pointer-events: none;
}

.glass-button:hover,
.glass-button-active {
  color: #fff;
}

.glass-button:hover::after,
.glass-button-active::after {
  transform: translateX(120%);
}

.glass-button-active {
  --glass-glow: rgba(120, 220, 240, 0.75);
}

.glass-button-icon {
  font-size: 28px;
  line-height: 1;
  filter: drop-shadow(1px 2px 1px rgba(0, 0, 0, 0.8));
}

.glass-button-label {
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font: bold 11px 'Arial', sans-serif;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.8);
}

.glass-button-active .glass-button-label {
  color: #ffd166;
}
</style>
