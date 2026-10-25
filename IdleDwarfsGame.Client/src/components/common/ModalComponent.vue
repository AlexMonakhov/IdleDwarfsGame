<script setup lang="ts">

import { computed, onBeforeUnmount, onMounted, watch, type CSSProperties} from 'vue'

import { useModalStack } from '@/composables/UseModalStack';

import GemButton from './GemButton.vue';



interface Props {

  modelValue: boolean

  width?: string

  height?: string

  backgroundImage?: string

  closeOnOverlay?: boolean

  showClose?: boolean

  headerMessage?: string

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

}>()



const close = (): void => {

  emit('update:modelValue', false)

  emit('close')

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



const modalStyle = computed<CSSProperties>(() => ({

  width: props.width,

  height: props.height,

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

  overflow-y: auto;

  color: white;

  /* Радиус рамки минус её толщина — чтобы углы шли параллельно */
  border-radius: calc(var(--frame-radius) - 14px);

  border: 2px solid #333;

  padding: 5px;

  position: absolute;

  /* Тёмный центр, чтобы белый текст читался; свою текстуру можно положить первым слоем из assets */
  background: radial-gradient(ellipse at 50% 35%, #5b7798 0%, #3f5a7a 45%, #1f2d42 100%);

  box-shadow: 0 0 0 4px rgb(157, 173, 175),

              0 0 0 6px #333,

              inset 0px 0px 6px black, inset 0px 0px 20px #333;

  display: flex;

  padding: 10px 20px;

}



.modal-content-with-footer {
  flex-direction: column;
}

.modal-body {
  flex: 1;
  display: flex;
}

.modal-footer {
  padding-top: 12px;
  display: flex;
  justify-content: flex-end;
  align-items: center;
  column-gap: 10px;
}

.modal-content{

  scrollbar-width: thin; /* Firefox */

  scrollbar-color: #707070 transparent;

}



/* Chrome / Edge / Safari */

.modal-content::-webkit-scrollbar {

  width: 4px;

}



.modal-content::-webkit-scrollbar-track {

  background: transparent;

}



.close-btn {

  position: absolute;

  top: -18px;

  right: -18px;

  z-index: 10000;

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
          class="modal-window"
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

          <!-- С футером контент становится колонкой: тело сверху, кнопки внутри рамки в правом нижнем углу -->
          <div class="modal-content" :class="{ 'modal-content-with-footer': $slots.modalFooter }">
            <div v-if="$slots.modalFooter" class="modal-body">
              <slot />
            </div>
            <slot v-else />

            <div v-if="$slots.modalFooter" class="modal-footer">
              <slot name="modalFooter"></slot>
            </div>
          </div>

        </div>
      </div>
    </Transition>
  </Teleport>
</template> 

