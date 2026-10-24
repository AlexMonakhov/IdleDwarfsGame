<template>
  <!-- Обертка вокруг элемента, на который мы наводим мышь -->
  <div
    class="dwarf-tooltip-wrapper"
    @mouseenter="show"
    @mouseleave="hide"
    @mousemove="move"
  >
    <!-- Дефолтный слот для самого предмета/кнопки -->
    <slot />

    <!-- Сам тултип телепортируется в корень документа -->
    <Teleport to="body">
      <Transition name="fade">
        <div
          ref="tooltipRef"
          v-if="isVisible"
          class="dwarf-tooltip"
          :style="{ top: `${y}px`, left: `${x}px` }"
        >
          <!-- Деревянная подложка (как .inner в кнопке) -->
          <div class="tooltip-inner-bg"></div>

          <!-- Контент тултипа, лежащий поверх подложки -->
          <div class="tooltip-content">
            <div class="tooltip-title">
                <slot name="title">Default Title</slot>
            </div>
            <div class="tooltip-desc">
                <slot name="description">Default Description</slot>
            </div>
            <div class="tooltip-stats" v-if="$slots.stats">
                <slot name="stats">Default Stats</slot>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { nextTick, ref } from 'vue';

// interface Props {
  
// }


// const props = withDefaults(defineProps<Props>(), {
  
// })

const isVisible = ref(false);
const x = ref(0);
const y = ref(0);
const tooltipRef = ref<HTMLElement | null>(null);

// Смещение в пикселях, чтобы курсор не перекрывал сам тултип
const OFFSET_X = 15;
const OFFSET_Y = 15;

const show = (event: MouseEvent) => {
  isVisible.value = true;
  updatePosition(event);
};

const hide = () => {
  isVisible.value = false;
};

const move = (event: MouseEvent) => {
  if (isVisible.value) {
    updatePosition(event);
  }
};

const updatePosition = async (event: MouseEvent) => {
  // 1. Сначала задаем позицию "по умолчанию" (справа-снизу от курсора)
  let posX = event.clientX + OFFSET_X;
  let posY = event.clientY + OFFSET_Y;

  x.value = posX;
  y.value = posY;

  // 2. Ждем, пока Vue отрендерит элемент, чтобы мы могли узнать его реальную ширину и высоту
  await nextTick();

  if (tooltipRef.value) {
    const rect = tooltipRef.value.getBoundingClientRect();
    
    // 3. Проверяем правый край экрана
    if (posX + rect.width > window.innerWidth) {
      // Если не влезает справа — перекидываем влево от мыши
      posX = event.clientX - rect.width - OFFSET_X;
    }

    // 4. Проверяем нижний край экрана (бонус: чтобы тултип не уходил под пол)
    if (posY + rect.height > window.innerHeight) {
      // Если не влезает снизу — перекидываем вверх от мыши
      posY = event.clientY - rect.height - OFFSET_Y;
    }

    // 5. Применяем откорректированные координаты
    x.value = posX;
    y.value = posY;
  }
};
</script>

<style scoped>
.dwarf-tooltip-wrapper {
  display: inline-block;
  cursor: help;
}

/* =========================================
   ОСНОВНОЙ КОНТЕЙНЕР ТУЛТИПА (Металлическая рамка)
   ========================================= */
.dwarf-tooltip {
  position: fixed;
  z-index: 9999;
  pointer-events: none; /* Чтобы мышь проходила насквозь */
  
  width: max-content;
  max-width: 250px;
  
  /* Отступы задают "толщину" рамки до текста */
  padding: 14px; 

  /* Стили перенесены из .icon-button-square */
  border-radius: 15px;
  background: var(--metalic-background, linear-gradient(145deg, #444, #222));
  border: 0.7px solid rgba(0, 0, 0, 0.1);
  box-shadow: 
      0 8px 15px rgba(0, 0, 0, 0.4),          /* Внешняя тень */
      inset 0 1px 2px rgba(255, 255, 255, 0.8), /* Блик на рамке */
      0 0 2px 2px rgba(0,0,0, 0.3);           /* Темная окантовка */
}

/* =========================================
   ВНУТРЕННИЙ ФОН (Дерево)
   ========================================= */
.tooltip-inner-bg {
  position: absolute;
  top: 2px; 
  bottom: 2px;
  left: 2px; 
  right: 2px;
  
  /* Стили перенесены из .inner */
  background: var(--wood-background, #3e2723);
  box-shadow: inset 0px 0px 6px black, inset 0px 0px 20px #333;
  border-radius: 12px; /* Радиус чуть меньше внешнего (15px) */
  z-index: 10;
}

/* =========================================
   ТЕКСТ И СЛОТЫ
   ========================================= */
.tooltip-content {
  position: relative;
  z-index: 100; /* Поднимаем текст поверх деревянного фона */
  color: #e0e0e0;
  font-family: Arial, sans-serif; /* Можно поменять на ваш шрифт */
}

.tooltip-title {
  margin: 0 0 8px 0;
  color: #ffd700; /* Золото */
  font-size: 14px;
  font-weight: bold;
  text-transform: uppercase;
  border-bottom: 1px dashed rgba(255, 255, 255, 0.2);
  padding-bottom: 6px;
  text-shadow: 1px 1px 3px rgba(0, 0, 0, 0.9); /* Тень для читаемости на дереве */
}

.tooltip-desc {
  margin: 0;
  font-size: 13px;
  line-height: 1.4;
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
}

.tooltip-stats {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  font-size: 12px;
  color: #a3be8c; /* Бледно-зеленый */
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
}

/* =========================================
   АНИМАЦИИ
   ========================================= */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>