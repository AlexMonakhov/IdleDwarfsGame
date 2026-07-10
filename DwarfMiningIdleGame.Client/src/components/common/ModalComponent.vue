<script setup lang="ts">
import { computed, onBeforeUnmount, nextTick, watch, ref, provide, type CSSProperties } from 'vue'
import image from "@/assets/Gemini_Generated_Image_kfl11vkfl11vkfl1.png";
import CanvasLayer from '../canvas/CanvasLayer.vue';
import { useModalStack } from '@/composables/UseModalStack';
import GemButton from './GemButton.vue';
import { GlobalParticleService } from '@/services/particleService/ParticleSystem';

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

// --- ИНТЕГРАЦИЯ КАНВАСОВ ---
const modalParticleService = new GlobalParticleService();
provide('particleService', modalParticleService); // Провайдим сервис для кнопок внутри

const modalContentRef = ref<HTMLElement | null>(null);
const bgCanvasRef = ref<HTMLCanvasElement | null>(null);
const fgCanvasRef = ref<HTMLCanvasElement | null>(null);
let resizeObserver: ResizeObserver | null = null;

const setupCanvases = () => {
  if (!modalContentRef.value || !bgCanvasRef.value || !fgCanvasRef.value) return;

  // Функция для точной подгонки физического размера холста под размер блока
  const updateSize = () => {
    const { width, height } = modalContentRef.value!.getBoundingClientRect();
    bgCanvasRef.value!.width = width;
    bgCanvasRef.value!.height = height;
    fgCanvasRef.value!.width = width;
    fgCanvasRef.value!.height = height;
  };

  updateSize();
  modalParticleService.init(bgCanvasRef.value, fgCanvasRef.value);

  // Следим за изменением размеров модалки
  resizeObserver = new ResizeObserver(updateSize);
  resizeObserver.observe(modalContentRef.value);
};

watch(
  () => props.modelValue,
  async (isOpen, wasOpen) => {
    if (isOpen && !wasOpen) {
      push()
      await nextTick() // Ждем рендера DOM
      setupCanvases()
    }

    if (!isOpen && wasOpen) {
      pop()
      modalParticleService.destroy()
      if (resizeObserver) resizeObserver.disconnect()
    }
  },
  { immediate: true }
)

onBeforeUnmount(() => {
  modalParticleService.destroy()
  if (resizeObserver) resizeObserver.disconnect()
})
// ---------------------------

const modalStyle = computed<CSSProperties>(() => ({
  width: props.width,
  height: props.height,
}))
</script>

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
          <div v-if="showClose" class="close-btn">
            <GemButton v-if="showClose" round @click="close" :color="'red'">X</GemButton>
          </div>
          
          <div class="modal-header">
            <div class="modal-header-text">{{ props.headerMessage }}</div>
          </div>

          <div class="modal-content" >
            
            

              
                <slot />
              

            
            
          </div>

          <div v-if="$slots.modalFooter">
                <slot name="modalFooter"></slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.65);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: center;
  align-items: center;
  --border-radius: 100px;
}

.modal-window {
  position: relative;
  box-sizing: border-box;
  padding-top: 80px;
  min-width: 400px;
  min-height: 300px;
  border-radius: var(--border-radius);
}

.modal-window::before {
    position: absolute;
    top: -15px; bottom: -15px;
    left: -15px; right: -15px;
    background: var(--metalic-background);
    content: '';
    z-index: -1;
    border-radius: 80px 80px 10px 10px;
    box-shadow: 0 4px 3px 2px #181717, 0 3px 20px 4px #65a7b8;
}

.modal-header{
  width: 100%;
  height: 70px;
  background: var(--wood-background);
  position: absolute;
  top: 0;
  border-radius: var(--border-radius) var(--border-radius) 0 0;
  box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;
  text-align: center;
}

.modal-header-text{
  line-height: 70px;
  font-weight: 900;
  font-size: 2em;
  letter-spacing: 0.2em;
  font-family: cursive;
  text-transform: uppercase;
  background: var(--rune-text-background);
  background-clip:text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  text-shadow: 1px 1px 4px #b2bfe0;
  filter: drop-shadow(-4px 1px 1px #333);
  font-family: RunesFont, sans-serif;
}

/* Изменения тут: убрали скролл и паддинги, оставили только фон */
.modal-content {
  box-sizing: border-box;
  width: 100%;
  height: 100%;
  color: white; 
  border-radius: 15px;
  border: 2px solid #333;
  position: relative; /* relative, чтобы канвасы абсолютно позиционировались внутри */
  overflow: hidden; /* Чтобы канвасы не вылезали за скругления */
  background:url(https://png.pngtree.com/png-vector/20240808/ourmid/pngtree-background-with-ground-texture-png-image_13065911.png) center / cover no-repeat, radial-gradient( #e1e8eb, #4c6a8d);
  box-shadow: 0 0 0 4px rgb(157, 173, 175),
              0 0 0 6px #333, 
              inset 0px 0px 6px black, inset 0px 0px 20px #333;
}

/* Канвасы внутри модалки */
.inner-canvas {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
}
.inner-canvas.bg { z-index: 0; }
.inner-canvas.fg { z-index: 20; }

/* Скролл и отступы переехали сюда */
.scroll-wrapper {
  position: relative;
  z-index: 10; /* Контент строго между двумя канвасами */
  width: 100%;
  height: 100%;
  overflow-y: auto;
  padding: 10px 20px;
  box-sizing: border-box;
  display: flex;
}

.scroll-wrapper {
  scrollbar-width: thin; /* Firefox */
  scrollbar-color: #707070 transparent;
}
.scroll-wrapper::-webkit-scrollbar { width: 4px; }
.scroll-wrapper::-webkit-scrollbar-track { background: transparent; }

.close-btn {
  position: absolute;
  top: -18px;
  right: -18px;
  z-index: 10000;
}

.modal-fade-enter-active,
.modal-fade-leave-active { transition: all 0.25s ease; }
.modal-fade-enter-from,
.modal-fade-leave-to { opacity: 0; transform: scale(0.95); }
</style>