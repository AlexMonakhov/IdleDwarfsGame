<script setup lang="ts">

import { computed, onBeforeUnmount, onMounted, ref, watch, type CSSProperties} from 'vue'

import { useModalStack } from '@/composables/UseModalStack';

import GemButton from './GemButton.vue';

import BadgeComponent from './BadgeComponent.vue';

import GlassButton from './GlassButton.vue';

import type { ModalTab } from '@/models/types/Modal';

import { vDragScroll } from '@/directives/dragScroll';



interface Props {

  modelValue: boolean

  width?: string

  height?: string

  backgroundImage?: string

  closeOnOverlay?: boolean

  showClose?: boolean

  headerMessage?: string

  // Вкладки-кнопки справа от рамки; без них модалка выглядит как раньше
  tabs?: ModalTab[]

  activeTab?: string

}



const props = withDefaults(defineProps<Props>(), {

  width: '40%',

  height: '60%',

  backgroundImage: '/images/modal-frame.png',

  closeOnOverlay: true,

  showClose: true

})



const emit = defineEmits<{

  (e: 'update:modelValue', value: boolean): void

  (e: 'close'): void

  (e: 'update:activeTab', value: string): void

}>()



const close = (): void => {

  emit('update:modelValue', false)

  emit('close')

}



const selectTab = (key: string): void => {

  if (key !== props.activeTab) emit('update:activeTab', key)

}



const { zIndex, push, pop } = useModalStack()



watch(

  () => props.modelValue,

  (isOpen, wasOpen) => {

    if (isOpen && !wasOpen) {

      push()

    }



    if (!isOpen && wasOpen) {

      pop()

    }

  },

  { immediate: true }

)



// Размер окна нужен вкладкам: они растягивают металл рамки на её размер, чтобы стык был без шва.
// offsetWidth/Height не зависят от transform, так что анимация появления замер не портит
const windowEl = ref<HTMLElement | null>(null)

const windowSize = ref<{ width: number, height: number } | null>(null)

let resizeObserver: ResizeObserver | null = null

// Вкладки могут появиться уже после открытия (окно гильдии получает их после загрузки)
watch([windowEl, () => !!props.tabs?.length], ([el, hasTabs]) => {

  resizeObserver?.disconnect()

  resizeObserver = null

  if (!el || !hasTabs) return

  resizeObserver = new ResizeObserver(() => {

    windowSize.value = { width: el.offsetWidth, height: el.offsetHeight }

  })

  resizeObserver.observe(el)

})

onBeforeUnmount(() => resizeObserver?.disconnect())



const modalStyle = computed<CSSProperties>(() => ({

  width: props.width,

  height: props.height,

  ...(windowSize.value && {

    '--window-w': `${windowSize.value.width}px`,

    '--window-h': `${windowSize.value.height}px`,

  }),

//   backgroundImage: `url(${props.backgroundImage})`,

//   backgroundSize: '100% 100%',

//   backgroundRepeat: 'no-repeat',

//   backgroundPosition: 'center'

  // borderImage: `url(${image}) 60 stretch`,

  // borderStyle: 'solid',

  // borderWidth: '15px'

 

}))

</script>



<style scoped>

.modal-overlay {

  position: fixed;

  inset: 0;

  background: rgba(0, 0, 0, 0.65);

  backdrop-filter: blur(4px);

  display: flex;

  justify-content: center;

  align-items: center;

  --frame-radius: 28px;

}



.modal-window {

  position: relative;

  box-sizing: border-box;

  /* Шапка теперь висит над рамкой, поэтому контенту нужен только небольшой отступ под её низ */
  --content-top: 44px;

  padding-top: var(--content-top);

  min-width: 400px;

  min-height: 300px;

  border-radius: var(--frame-radius);

}



/* .modal-window::after {

    position: absolute;

    top: -20px; bottom: -20px;

    left: -20px; right: -20px;

    background: linear-gradient(to bottom, rgb(117, 144, 145), rgb(8, 8, 8));    

    content: '';

    z-index: -2;

    border-radius: var(--border-radius) var(--border-radius) 10px 10px;

    box-shadow: 0 1px 5px 2px rgb(8, 9, 14);

} */



.modal-window::before {

    position: absolute;

    top: -15px; bottom: -15px;

    left: -15px; right: -15px;

    background: var(--metalic-background);

    content: '';

    z-index: -1;

    border-radius: var(--frame-radius);

    box-shadow: 0 4px 3px 2px #181717, 0 3px 20px 4px #65a7b8;

}



/* Табличка с заголовком: выступает над верхним краем рамки и прибита заклёпками по углам */
.modal-header {
  --rivet: radial-gradient(circle at 35% 35%, #ffffff 0%, #c9d2d6 25%, #6e777b 55%, #2b2b2b 72%, transparent 78%);
  position: absolute;
  top: -40px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 2;
  box-sizing: border-box;
  min-width: 55%;
  max-width: calc(100% - 80px);
  height: 64px;
  padding: 0 44px;
  background: var(--wood-background);
  border-radius: 12px;
  box-shadow:
    inset 0px 0px 6px black,
    inset 0px 0px 20px #333,
    0 0 0 4px rgb(157, 173, 175),
    0 0 0 6px #333,
    0 8px 14px 2px rgba(0, 0, 0, 0.6),
    0 3px 16px 4px #65a7b8;
  text-align: center;
}

.modal-header::before {
  content: '';
  position: absolute;
  inset: 7px 10px;
  pointer-events: none;
  background:
    var(--rivet) left top / 12px 12px no-repeat,
    var(--rivet) right top / 12px 12px no-repeat,
    var(--rivet) left bottom / 12px 12px no-repeat,
    var(--rivet) right bottom / 12px 12px no-repeat;
}



.modal-header-text{

  line-height: 64px;

  white-space: nowrap;

  overflow: hidden;

  text-overflow: ellipsis;

  font-weight: 900;

  font-size: 2em;

  letter-spacing: 0.2em;

  text-transform: uppercase;

  background: var(--rune-text-background);

  background-clip:text;

  -webkit-background-clip: text;

  -webkit-text-fill-color: transparent;

  text-shadow: 1px 1px 4px #b2bfe0;

  filter: drop-shadow(-4px 1px 1px #333);

  font-family: RunesFont, sans-serif;

}



.modal-content {

  box-sizing: border-box;

  width: 100%;

  height: calc(100% - var(--content-top));

  /* Прокручивается .modal-body; здесь только обрезаем его по скруглению рамки */
  overflow: hidden;

  color: white;

  /* Радиус рамки минус её толщина — чтобы углы шли параллельно */
  border-radius: calc(var(--frame-radius) - 14px);

  border: 2px solid #333;

  position: absolute;

  /* Тёмный центр, чтобы белый текст читался; свою текстуру можно положить первым слоем из assets */
  background: radial-gradient(ellipse at 50% 35%, #5b7798 0%, #3f5a7a 45%, #1f2d42 100%);

  box-shadow: 0 0 0 4px rgb(157, 173, 175),

              0 0 0 6px #333,

              inset 0px 0px 6px black, inset 0px 0px 20px #333;

  display: flex;

  flex-direction: column;

}



/* Отступы здесь, а не у .modal-content: тогда свечение карточек у края обрезается там же, где и раньше.
   Горизонтальной прокрутки нет — лишь бы не вылезла полоса от свечения или лёгкого увеличения при наведении */
.modal-body {
  flex: 1;
  min-height: 0;
  box-sizing: border-box;
  display: flex;
  padding: 10px 20px;
  overflow-x: hidden;
  overflow-y: auto;
}

.modal-footer {
  padding: 12px 20px 10px;
  display: flex;
  justify-content: flex-end;
  align-items: center;
  column-gap: 10px;
}



.close-btn {

  position: absolute;

  top: -18px;

  right: -18px;

  z-index: 10000;

}



/* Вкладки — выступы рамки справа. Слои внутри окна (оно изолировано, см. .modal-window-tabs):
   неактивные (-2) → рамка ::before (-1) → активная (-1, но позже в DOM) → контент (auto).
   Поэтому активная лежит поверх рамки и продолжает её, а неактивные уходят под неё */
.modal-window-tabs {
  isolation: isolate;
  --tabs-top: 60px;
  --tab-size: 96px;
  --tab-gap: 12px;
  /* Насколько вкладка заходит на рамку. Если ставить её встык, на дробном пикселе край сглаживается
     и сквозь него просвечивает тёмная обводка рамки. Не больше 9px — дальше начинается кольцо контента */
  --tab-overlap: 8px;
}

/* Контейнер сам клики не ловит: он лежит в слое выше вкладок с отрицательным z-index
   и иначе перехватывал бы нажатия на них */
.modal-tabs {
  position: absolute;
  top: var(--tabs-top);
  left: calc(100% + 15px - var(--tab-overlap));
  display: flex;
  flex-direction: column;
  row-gap: var(--tab-gap);
  pointer-events: none;
}

.modal-tab {
  /* Верх вкладки в координатах рамки (рамка начинается на 15px выше окна) */
  --tab-y: calc(var(--tabs-top) + 15px + var(--tab-index) * (var(--tab-size) + var(--tab-gap)));
  position: relative;
  z-index: -2;
  box-sizing: border-box;
  width: calc(100px + var(--tab-overlap));
  height: var(--tab-size);
  padding: 0;
  border: none;
  background: none;
  color: #cbd5e1;
  cursor: pointer;
  pointer-events: auto;
  /* Неактивная — на слой ниже: задвинута под рамку, чуть меньше и в тени */
  transform-origin: left center;
  transform: translateX(-4px) scale(0.92);
  filter: brightness(0.55) saturate(0.7);
  transition: transform 0.3s cubic-bezier(0.34, 1.4, 0.64, 1), filter 0.3s ease, color 0.3s ease;
}

.modal-tab:hover {
  color: #fff;
  filter: brightness(0.75) saturate(0.85);
  transform: translateX(2px) scale(0.95);
}

/* Тень — отдельным слоем под телом вкладки. Он начинается ровно с края рамки, а левая тень срезана:
   на заходящую на рамку часть вкладки тень не ложится */
.modal-tab::before {
  content: '';
  position: absolute;
  inset: 0 0 0 var(--tab-overlap);
  border-radius: 0 18px 18px 0;
  box-shadow: 0 4px 3px 2px #181717, 0 3px 12px 2px rgba(0, 0, 0, 0.6);
  clip-path: inset(-40px -40px -40px 0);
  transition: box-shadow 0.3s ease;
}

/* Тот же металл, что у рамки, растянутый на её размер и сдвинутый к месту вкладки — шва нет.
   Вкладка начинается на --tab-overlap левее конца плитки фона рамки, отсюда сдвиг по горизонтали */
.modal-tab-body {
  position: absolute;
  inset: 0;
  border-radius: 0 18px 18px 0;
  background: var(--metalic-background);
  background-size: calc(var(--window-w, 600px) + 30px) calc(var(--window-h, 400px) + 30px);
  background-position: var(--tab-overlap) calc(-1 * var(--tab-y));
}

/* Активная выдвигается из-под рамки и становится её частью: свет и свечение как у рамки */
.modal-tab-active,
.modal-tab-active:hover {
  z-index: -1;
  color: #fff;
  cursor: default;
  transform: none;
  filter: none;
}

.modal-tab-active::before {
  box-shadow: 0 4px 3px 2px #181717, 0 3px 20px 4px #65a7b8;
}

/* Стекло вкладки (GlassButton) растянуто по ней. Слева от края рамки 6px: ровно под кольцо оправы,
   чтобы между окном модалки и стеклом осталась полоса рамки той же ширины.
   Селектор из двух классов — чтобы перебить position: relative самого GlassButton */
.modal-tab .modal-tab-plate {
  position: absolute;
  inset: 15px 15px 15px calc(var(--tab-overlap) + 6px);
  min-width: 0;
  min-height: 0;
  padding: 0 4px;
}

/* Отблеск и белый текст — при наведении на всю вкладку, а не только на стекло */
.modal-tab:hover .modal-tab-plate {
  color: #fff;
}

.modal-tab:hover .modal-tab-plate::after {
  transform: translateX(120%);
}



/* Фон только проявляется, а окно «выпрыгивает» с лёгким перелётом.
   Vue снимает классы по transition корневого элемента (оверлея),
   поэтому его длительность не короче, чем у окна */
.modal-fade-enter-active {
  transition: opacity 0.35s ease;
}

.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}

.modal-fade-enter-active .modal-window {
  transition: transform 0.35s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.25s ease;
}

.modal-fade-leave-active .modal-window {
  transition: transform 0.2s ease-in, opacity 0.2s ease-in;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

/* Только сдвиг, без scale: CanvasLayer/CanvasComponent меряют getBoundingClientRect при монтировании,
   и масштаб в этот момент сжимает канвас и смещает эффекты. Сдвиг же одинаково двигает и канвас, и кнопки */
.modal-fade-enter-from .modal-window,
.modal-fade-leave-to .modal-window {
  opacity: 0;
  transform: translateY(32px);
}

</style>



<template>

  <Teleport to="body">

    <Transition name="modal-fade">

      <div

        v-if="modelValue"

        class="modal-overlay"

        :style="{zIndex}"

        @click="closeOnOverlay && close()"

      >

        <div
          ref="windowEl"
          class="modal-window"
          :class="{ 'modal-window-tabs': tabs?.length }"
          @click.stop
          :style="modalStyle"
        >

          <div
            v-if="showClose"
            class="close-btn"
          >
            <GemButton v-if="showClose" round @click="close" :color="'red'">X</GemButton>
          </div>

          <div class="modal-header"><div class="modal-header-text">{{ props.headerMessage }}</div></div>

          <div v-if="tabs?.length" class="modal-tabs" role="tablist">
            <button
              v-for="(tab, index) in tabs"
              :key="tab.key"
              type="button"
              role="tab"
              class="modal-tab"
              :class="{ 'modal-tab-active': tab.key === activeTab }"
              :style="{ '--tab-index': index }"
              :aria-selected="tab.key === activeTab"
              :title="tab.label"
              @click="selectTab(tab.key)"
            >
              <span class="modal-tab-body">
                <GlassButton
                  tag="span"
                  class="modal-tab-plate"
                  :active="tab.key === activeTab"
                  :icon="tab.icon"
                  :label="tab.label"
                />
              </span>
              <BadgeComponent v-if="tab.badgeCount" :count="tab.badgeCount" />
            </button>
          </div>

          <!-- Тело прокручивается само, без полосы; футер с кнопками стоит под ним внутри рамки -->
          <div class="modal-content">
            <div v-drag-scroll class="modal-body">
              <slot />
            </div>

            <div v-if="$slots.modalFooter" class="modal-footer">
              <slot name="modalFooter"></slot>
            </div>
          </div>

        </div>
      </div>
    </Transition>
  </Teleport>
</template> 

