<script lang="ts" setup>
import { computed, useId } from 'vue';
import type { SplitIconProps } from './splitIcon';

// Основа иконок в стиле шлема (IconHelm): плоский силуэт, левая половина одного цвета, правая — другого.
// Фигура рисуется один раз в слоте по умолчанию (viewBox 0 0 100 100, только path, без своего fill):
// компонент сам режет её пополам и красит. Прорези-детали — в слоте cut: всё, что там нарисовано
// (по умолчанию штрихом толщиной 3.5), вырезается из фигуры насквозь.
// Слот badge — значок поверх фигуры (звезда рейтинга, колба извлечения): прорези на него не действуют,
// поэтому под значок можно вырезать в cut сплошной круг, и значок встанет в него с зазором.
// Цвета: пропсы, иначе CSS-переменные --split-icon-primary / --split-icon-secondary от родителя
// (так кнопка перекрашивает все иконки внутри себя), иначе цвета шлема в игре
// Размер по умолчанию — под текущий шрифт: <IconBattle /> в кнопке или строке встаёт как буква
const props = withDefaults(defineProps<SplitIconProps>(), {
  size: '1.2em',
});

const primary = computed(() => props.primaryColor ?? 'var(--split-icon-primary, rgb(217 215 178))');
const secondary = computed(() => props.secondaryColor ?? 'var(--split-icon-secondary, rgb(64 255 251))');

// id внутри svg должны быть уникальны, иначе несколько иконок на странице делят одну маску
const uid = useId();
const leftId = `split-left-${uid}`;
const rightId = `split-right-${uid}`;
const cutId = `split-cut-${uid}`;
</script>

<template>
  <svg class="split-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" :width="size" :height="size">
    <defs>
      <!-- Левая половина чуть заходит за середину: правая ляжет сверху, и на стыке не будет просвета -->
      <clipPath :id="leftId"><rect x="0" y="0" width="50.5" height="100" /></clipPath>
      <clipPath :id="rightId"><rect x="50" y="0" width="50" height="100" /></clipPath>
      <mask :id="cutId" maskUnits="userSpaceOnUse" x="0" y="0" width="100" height="100">
        <rect x="0" y="0" width="100" height="100" fill="#fff" />
        <g fill="none" stroke="#000" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round">
          <slot name="cut" />
        </g>
      </mask>
    </defs>

    <g :mask="`url(#${cutId})`">
      <g :clip-path="`url(#${leftId})`" :style="`fill:${primary};fill-opacity: var(--opacityLeft,1)`">
        <slot />
      </g>
      <g :clip-path="`url(#${rightId})`" :style="`fill:${secondary};fill-opacity: var(--opacityRight,1)`">
        <slot />
      </g>
    </g>
    <template v-if="$slots.badge">
      <g :clip-path="`url(#${leftId})`" :style="`fill:${primary};fill-opacity: var(--opacityLeft,1)`">
        <slot name="badge" />
      </g>
      <g :clip-path="`url(#${rightId})`" :style="`fill:${secondary};fill-opacity: var(--opacityRight,1)`">
        <slot name="badge" />
      </g>
    </template>
  </svg>
</template>

<style scoped>
/* В строке текста иконка садится на базовую линию, как буква; во flex-контейнере это свойство ни на что не влияет */
.split-icon {
  flex-shrink: 0;
  vertical-align: -0.2em;
}
</style>
