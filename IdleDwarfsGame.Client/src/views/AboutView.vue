<template>
  <div 
    class="about-container" 
    ref="slider"
    @mousedown="startDrag"
    @mouseleave="onMouseLeave"
    @mouseup="stopDrag"
    @mousemove="onMouseMove"   
    @scroll="handleScroll"
    :class="{ 'is-dragging': isDown }"
    :style="{'opacity': opacity}"
  >
    <div 
      class="parallax-bg" 
      :style="{ transform: `translateX(${-bgOffset}px)` }"
    >
      <img :src="backgroundsky" class="wide-image-bg" @load="scrollToCenter" />
    </div>

    <div class="about-content">
      <img 
        ref="imageRef"
        :src="background" 
        alt="Sizing Layer" 
        class="wide-image-placeholder" 
        @dragstart.prevent 
      />

      <div class="interactive-layer">
        <img 
          :src="campaignHovered ? campaignHover : campaign"
          @mouseenter="campaignHovered = true"
          @mouseleave="campaignHovered = false"
          @click.capture="campaignDialogOpen = true"
          @dragstart.prevent 
          class="campaign-picture" 
        />
      </div>
    </div>
  </div>

  <ModalComponent 
    v-model="campaignDialogOpen"
    :header-message="'campaign'"
  >
    <FightPreviewListContainer :count="6"></FightPreviewListContainer>
  </ModalComponent>
</template>

<script lang="ts" setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue';
import background from "@/assets/background4.png";
import backgroundsky from "@/assets/backgroundsky.png";
import campaign from "@/assets/campaign-dark.png";
import campaignHover from "@/assets/campaign-hover.png";
import ModalComponent from '@/components/common/ModalComponent.vue';
import FightPreviewListContainer from '@/components/interface/FightPreviewListContainer.vue';


// --- НАСТРОЙКИ ---
const PARALLAX_SPEED = 0.3; // Чем меньше число, тем медленнее движется фон (глубже эффект)
const EDGE_THRESHOLD = 0.01; 
const MAX_SPEED = 10;

// --- СОСТОЯНИЕ ---
const slider = ref<HTMLElement | null>(null);
const imageRef = ref<HTMLImageElement | null>(null);
const isDown = ref(false);
const startX = ref(0);
const scrollLeftState = ref(0);
const opacity = ref(0);
const campaignDialogOpen = ref(false);
const campaignHovered = ref(false);

// Параллакс смещение
const bgOffset = ref(0);

// Данные для Edge Scroll
const mouseX = ref(-1);
const containerWidth = ref(0);
let animationFrameId = 0;

// --- ЛОГИКА ПАРАЛЛАКСА ---
const handleScroll = () => {
  if (!slider.value) return;
  // Мы вычисляем смещение. Если PARALLAX_SPEED = 0.3, фон проедет только 30% пути скролла
  bgOffset.value = slider.value.scrollLeft * PARALLAX_SPEED;
};

// --- DRAG-TO-SCROLL ---
const startDrag = (e: MouseEvent) => {
  console.log('Drag started');
  if (!slider.value) return;
  isDown.value = true;
  startX.value = e.pageX - slider.value.offsetLeft;
  scrollLeftState.value = slider.value.scrollLeft;
};

const stopDrag = () => {
  isDown.value = false;
};

const onMouseLeave = () => {
  isDown.value = false;
  mouseX.value = -1;
};

const onMouseMove = (e: MouseEvent) => {
  if (!slider.value) return;

  const rect = slider.value.getBoundingClientRect();
  mouseX.value = e.clientX - rect.left;
  containerWidth.value = rect.width;

  if (isDown.value) {
    e.preventDefault();
    const x = e.pageX - slider.value.offsetLeft;
    const walk = (x - startX.value) * 1.5;
    slider.value.scrollLeft = scrollLeftState.value - walk;
  }
};

// --- EDGE SCROLL LOOP ---
const edgeScrollLoop = () => {
  if (slider.value && mouseX.value !== -1 && !isDown.value) {
    const width = containerWidth.value;
    const x = mouseX.value;
    let speed = 0;

    const leftBoundary = width * EDGE_THRESHOLD;
    const rightBoundary = width * (1 - EDGE_THRESHOLD);

    if (x < leftBoundary) {
      const intensity = 1 - (x / leftBoundary);
      speed = -MAX_SPEED * intensity;
    } else if (x > rightBoundary) {
      const intensity = (x - rightBoundary) / (width * EDGE_THRESHOLD);
      speed = MAX_SPEED * intensity;
    }

    if (speed !== 0) {
      slider.value.scrollLeft += speed;
    }
  }
  animationFrameId = requestAnimationFrame(edgeScrollLoop);
};

// --- ИНИЦИАЛИЗАЦИЯ ---
const scrollToCenter = async () => {
  await nextTick(); 
  setTimeout(() => {
    if (!slider.value) return;
    const maxScroll = slider.value.scrollWidth - slider.value.clientWidth;
    slider.value.scrollLeft = maxScroll / 2;
    handleScroll(); // Обновляем параллакс сразу после центрирования
    opacity.value = 1;
  }, 50);
};

onMounted(() => {
  edgeScrollLoop();
  if (imageRef.value && imageRef.value.complete) {
    scrollToCenter();
  }
});

onUnmounted(() => {
  cancelAnimationFrame(animationFrameId);
});

const onStageClick = (n: number) => {
  console.log(`Уровень ${n} нажат`);
};
</script>

<style lang="scss" scoped>
.about-container {
  position: relative;
  width: 100%;
  height: 100vh;
  overflow-x: auto;
  overflow-y: hidden;
  white-space: nowrap;
  cursor: grab;
  background: #000;
  
  &::-webkit-scrollbar { display: none; }
  scrollbar-width: none;
  transition: opacity 0.3s ease-out;
}

.parallax-bg {
  position: fixed;
  top: 0;
  left: 0;
  height: 100%;
  pointer-events: none;
  will-change: transform;
}

.wide-image-bg {
  height: 100%;
  width: auto;
  display: block;
}

.about-content {
  position: relative;
  display: inline-block;
  height: 100%;
}

.wide-image-placeholder {
  height: 100%;
  width: auto;
  display: block;
}

.interactive-layer {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 3;
  pointer-events: none;
}

.campaign-picture {
  position: absolute;
  top: 59%;
  left: 49.5%;
  width: 10%;
  height: auto;
  transform: translate(-50%, -100%);
  cursor: pointer;
  pointer-events: auto; /* Разрешаем клики по картинке */
}

.campaign-picture:hover {
  filter: drop-shadow(0 0 2px white) drop-shadow(2px 2px 28px #334863) brightness(1.2);
}

.company-list {
  list-style: none;
  padding: 20px;
  .company-list-row {
    margin-bottom: 10px;
    display: flex;
    align-items: center;
    gap: 15px;
  }
}
</style>