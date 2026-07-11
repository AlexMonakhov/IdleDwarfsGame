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



.modal-content {

  box-sizing: border-box;

  width: 100%;

  height: calc(100% - 80px);

  overflow-y: auto;

  color: white;

  border-radius: 15px;

  border: 2px solid #333;

  padding: 5px;

  position: absolute;

  background:url(https://png.pngtree.com/png-vector/20240808/ourmid/pngtree-background-with-ground-texture-png-image_13065911.png) center / cover no-repeat, radial-gradient( #e1e8eb, #4c6a8d);

  box-shadow: 0 0 0 4px rgb(157, 173, 175),

              0 0 0 6px #333,

              inset 0px 0px 6px black, inset 0px 0px 20px #333;

  display: flex;

  padding: 10px 20px;

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



.modal-fade-enter-active,

.modal-fade-leave-active {

  transition: all 0.25s ease;

}



.modal-fade-enter-from,

.modal-fade-leave-to {

  opacity: 0;

  transform: scale(0.95);

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

          <div class="modal-content">
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

