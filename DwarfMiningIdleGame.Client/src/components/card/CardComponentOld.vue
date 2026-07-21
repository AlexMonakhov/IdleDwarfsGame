<template>
  <div
    ref="cardRef"
    class="tilt-card"
    @mousemove="handleMouseMove"
    @mouseleave="handleMouseLeave"
    :style="cardStyle"
  >
    <div class="tilt-card-content">
      
      <slot>
        <h2>Dwarf Mining</h2>
        <p>Наведи курсор</p>
      </slot>
    </div>
    <div @click="clicked++" v-if="clicked < 0" class="overlay-block" style="left: 0; top: 0; width: 100%; height: 100%;"></div>
    <div 
      v-else
      @click="onClickBlock" 
      v-for="block in getBlocks" 
      :key="`${block.id}`"
      class="overlay-block"
      :style="{
        left: `${block.left}%`,
        top: `${block.top}%`,
        width: `${block.width}%`,
        height: `${block.height}%`,
        opacity: `${block.opacity}`
      }"
    >
    </div>
  </div>
    <!-- Внутренний контейнер для эффекта глубины -->
    
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue';
import { generateOverlaySets, OverlayBlock } from './cardHelper';

const blocks = ref<[OverlayBlock[], OverlayBlock[], OverlayBlock[]]>([[], [], []]);
const clicked = ref<number>(-1);

const cardRef = ref<HTMLElement | null>(null);

const rotateX = ref(0);
const rotateY = ref(0);

// Максимальный угол наклона в градусах
const maxTilt = 15; 

onMounted(() => {
  // Генерируем 10 случайных блоков
  blocks.value = generateOverlaySets(7);
});

const getBlocks = computed(() => {
  return blocks.value[clicked.value];
});

const onClickBlock = async () => {
  clicked.value++;
  if(clicked.value === 2) {
    await nextTick();
    
    // 2. Даем небольшую задержку, чтобы браузер зафиксировал начальное состояние
    setTimeout(() => {
      for (const block of blocks.value[2]) {
        // Теперь меняем координаты — и CSS transition подхватит это изменение
        block.left = Math.random() * 300 - 150;
        block.top = Math.random() * 300 - 150;
        block.opacity = 0; // Добавляем исчезновение через opacity
        // Можно заодно добавить исчезновение через opacity:
        // (Для этого добавьте свойство opacity в объект block и привяжите его в :style)
      }
    }, 50);
  }
};

const handleMouseMove = (event: MouseEvent) => {
  if (!cardRef.value) return;

  const rect = cardRef.value.getBoundingClientRect();

  // Координаты мыши относительно левого верхнего угла карточки
  const x = event.clientX - rect.left;
  const y = event.clientY - rect.top;

  // Находим центр карточки
  const centerX = rect.width / 2;
  const centerY = rect.height / 2;

  // Вычисляем процент отклонения курсора от центра (от -1 до 1)
  const percentX = (x - centerX) / centerX;
  const percentY = (y - centerY) / centerY;

  // По оси Y крутим в зависимости от X (движение курсора влево-вправо)
  rotateY.value = percentX * maxTilt;
  // По оси X крутим в зависимости от Y (движение курсора вверх-вниз)
  // Знак минус нужен, чтобы верхняя часть карточки наклонялась к зрителю, когда курсор сверху
  rotateX.value = -percentY * maxTilt;
};

const handleMouseLeave = () => {
  // Плавно возвращаем карточку в исходное положение при уходе курсора
  rotateX.value = 0;
  rotateY.value = 0;
};

const cardStyle = computed(() => ({
  transform: `perspective(1000px) rotateX(${rotateX.value}deg) rotateY(${rotateY.value}deg)`,
  // Меняем transition динамически: 
  // Быстрый отклик при движении, плавная анимация возврата (когда координаты 0)
  transition: (rotateX.value === 0 && rotateY.value === 0)
    ? 'transform 0.5s cubic-bezier(0.23, 1, 0.32, 1)'
    : 'transform 0.1s linear'
}));
</script>

<style scoped>


.overlay-block {
  position: absolute;
  
  /* --- 1. ЭФФЕКТ КАМНЯ --- */
  background-color: #3b3c3e;
  /* Градиент имитирует освещенную поверхность камня и затемнение внизу */
  background-image: linear-gradient(90deg, #57585a 0%, #595d66 100%);
  
  /* Немного скруглим углы, чтобы порода не была бритвенно-острой */
  border-radius: 2px;

  /* --- 2. ТЕНИ (ОБЪЕМ + СВЕЧЕНИЕ) --- */
  box-shadow: 
    /* Внутренний белый блик сверху (делает камень выпуклым) */
    inset 2px 2px 3px rgba(255, 255, 255, 0.15), 
    /* Внутренняя черная тень снизу (усиливает 3D-эффект) */
    inset -2px -2px 6px rgba(0, 0, 0, 0.233),
    
    /* Внешнее плотное свечение в самих щелях (уран/магия гномов) */
    0 0 8px 2px rgb(241, 43, 17),
    /* Широкий ореол света для атмосферы */
    0 0 20px 4px rgba(255, 217, 0, 0.836);

  /* --- 3. ТРЕЩИНЫ --- */
  /* Уменьшаем блок на 3% — это создаст зазоры, в которых будет виден свет */
  
  /* Плавная анимация при клике (когда камни дробятся) */
  transition: all 2s cubic-bezier(0.25, 0.8, 0.25, 1);
  transform: translateZ(30px); /* Чтобы браузер включил GPU-ускорение для плавной анимации */
  /* Чтобы клики проходили сквозь тени, но попадали по камням */
  cursor: pointer;
}
.tilt-card {
  width: 200px;
  height: 300px;
  background: linear-gradient(135deg, #2b2b2b, #1a1a1a);
  border-radius: 16px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4), 0 0 10px 2px rgb(72, 255, 0);
  border: 1px solid #3a3a3a;
  
  /* Критично для сохранения 3D-эффекта дочерних элементов */
  transform-style: preserve-3d;
  will-change: transform;
  
  /* Центрирование контента */
  display: flex;
  align-items: center;
  justify-content: center;
}

.tilt-card-content {
  color: #e0e0e0;
  text-align: center;
  pointer-events: none; /* Чтобы контент не перехватывал события мыши */
  
  /* Выдвигаем контент ближе к зрителю по оси Z */
  /* За счет transform-style: preserve-3d у родителя это создаст крутой параллакс */
  transform: translateZ(20px);
}

</style>