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


<!-- ЭТО ДЛЯ РАЗРУШЕНИЙ ОСКОЛКОВ КОТОРЫЕ РЯДОМ И ВЗРЫВА КОГДА ДОХОДИТ УРОН -->
<!--

<script setup lang="ts">
import { ref, onMounted } from 'vue';

interface Shard {
  id: number;
  // Индексы в сетке для поиска соседей
  row: number;
  col: number;

  clipPath: string;
  centerX: number;
  centerY: number;
  transform: string;
  opacity: number;
  filter: string;
  transitionDuration: string;
  zIndex: number;

  // Состояние осколка: 0 - цел, 1 - легкие трещины, 2 - глубокие трещины
  damageLevel: number; 

  // Подготовка к взрыву
  shiftX: number;
  shiftY: number;
}

const shards = ref<Shard[]>([]);
const isFirstClickHappened = ref(false); // Чтобы скрыть cover
const isFullyShattered = ref(false); // Флаг финального взрыва
const cardRef = ref<HTMLElement | null>(null);

// Разрешение сетки осколков (чем больше, тем мельче осколки)
const COLS = 6; 
const ROWS = 8;

onMounted(() => {
  shards.value = generateFineShards(COLS, ROWS);
});

function generateFineShards(cols: number, rows: number): Shard[] {
  const points: { x: number; y: number }[][] = [];
  const cellWidth = 100 / cols;
  const cellHeight = 100 / rows;

  for (let i = 0; i <= rows; i++) {
    const rowPoints = [];
    for (let j = 0; j <= cols; j++) {
      let x = j * cellWidth;
      let y = i * cellHeight;
      if (i > 0 && i < rows && j > 0 && j < cols) {
        // Увеличиваем jitter для более рваных форм
        const jitterX = (Math.random() - 0.5) * cellWidth * 0.9;
        const jitterY = (Math.random() - 0.5) * cellHeight * 0.9;
        x += jitterX;
        y += jitterY;
      }
      rowPoints.push({ x, y });
    }
    points.push(rowPoints);
  }

  const result: Shard[] = [];
  let id = 0;

  for (let i = 0; i < rows; i++) {
    for (let j = 0; j < cols; j++) {
      const p1 = points[i][j], p2 = points[i][j + 1];
      const p3 = points[i + 1][j + 1], p4 = points[i + 1][j];

      result.push({
        id: id++,
        row: i, // Сохраняем позицию в сетке
        col: j,
        clipPath: `polygon(${p1.x}% ${p1.y}%, ${p2.x}% ${p2.y}%, ${p3.x}% ${p3.y}%, ${p4.x}% ${p4.y}%)`,
        centerX: (p1.x + p2.x + p3.x + p4.x) / 4,
        centerY: (p1.y + p2.y + p3.y + p4.y) / 4,
        
        // Начальное состояние: Идеально ровный и целый
        transform: 'translate(0px, 0px) rotate(0deg) scale(1)',
        filter: 'none',
        opacity: 1,
        transitionDuration: '0.1s',
        zIndex: 1,
        damageLevel: 0, 
        
        shiftX: (Math.random() - 0.5) * 6,
        shiftY: (Math.random() - 0.5) * 6,
      });
    }
  }
  return result;
}

// Поиск индексов соседей в 1D массиве
function getNeighborIndices(clickedIndex: number): number[] {
  const neighbors: number[] = [];
  const shard = shards.value[clickedIndex];

  // Проверяем 8 соседей вокруг (включая диагонали)
  for (let dr = -1; dr <= 1; dr++) {
    for (let dc = -1; dc <= 1; dc++) {
      if (dr === 0 && dc === 0) continue; // Это сам кликнутый осколок

      const nr = shard.row + dr;
      const nc = shard.col + dc;

      // Проверяем границы сетки
      if (nr >= 0 && nr < ROWS && nc >= 0 && nc < COLS) {
        // Переводим 2D координаты сетки в 1D индекс массива
        neighbors.push(nr * COLS + nc);
      }
    }
  }
  return neighbors;
}

// Функция нанесения урона осколку
function damageShard(shardIndex: number, forceDeepCracks = false) {
  const shard = shards.value[shardIndex];
  
  if (shard.damageLevel >= 2) return; // Ужe макс. поврежден

  if (forceDeepCracks) {
    shard.damageLevel = 2; // Принудительно глубокие (для центра удара)
  } else {
    shard.damageLevel++; // Постепенно
  }

  shard.transitionDuration = '0.15s'; // Быстрый раскол

  if (shard.damageLevel === 1) {
    // ЛЕГКИЕ ТРЕЩИНЫ
    shard.transform = 'translate(0px, 0px) rotate(0deg) scale(0.97)';
    // Тонкое красное свечение
    shard.filter = 'drop-shadow(0 0 2px rgb(241, 43, 17))';
  } else if (shard.damageLevel === 2) {
    // ГЛУБОКИЕ РАЗЛОМЫ
    // Куски слегка сдвигаются
    shard.transform = `translate(${shard.shiftX}px, ${shard.shiftY}px) rotate(${shard.shiftX}deg) scale(0.90)`;
    // Яркое оранжевое ядро и тень
    shard.filter = 'drop-shadow(0 0 7px rgb(255, 100, 0)) drop-shadow(0 0 2px rgba(0,0,0,0.8))';
  }
}

// Обработчик клика по КОНКРЕТНОМУ осколку
const handleShardClick = (event: MouseEvent, index: number) => {
  if (isFullyShattered.value) return; // Если все разбито - игнорируем

  if (!isFirstClickHappened.value) {
    isFirstClickHappened.value = true;
    // Сразу дамажим все осколки на scale 0.99, чтобы между ними появились микро-щели
    // (Иначе clip-path может давать артефакты сглаживания)
    shards.value.forEach(s => {
       s.transform = 'translate(0px, 0px) rotate(0deg) scale(0.995)';
    });
  }

  // 1. Повреждаем сам осколок, по которому кликнули (сразу глубоко)
  damageShard(index, true);

  // 2. Находим и повреждаем соседей (постепенно)
  const neighbors = getNeighborIndices(index);
  neighbors.forEach(nIndex => {
    damageShard(nIndex);
  });

  // 3. Проверяем, нужно ли взорвать всю плиту
  checkFinalShatter(event);
};

// Функция проверки на финальный взрыв
function checkFinalShatter(event: MouseEvent) {
  // Например, взрываем, когда 80% осколков имеют хотя бы легкие трещины
  const damagedCount = shards.value.filter(s => s.damageLevel > 0).length;
  const totalCount = shards.value.length;

  if (damagedCount / totalCount > 0.8) {
    isFullyShattered.value = true;
    
    // Получаем координаты финального клика
    const target = event.currentTarget as HTMLElement;
    const rect = target.getBoundingClientRect();
    const clickX = ((event.clientX - rect.left) / rect.width) * 100;
    const clickY = ((event.clientY - rect.top) / rect.height) * 100;

    // Запускаем финальный разлет
    shards.value.forEach(shard => {
      shard.damageLevel = 2; // Все становятся глубоко разбитыми

      const deltaX = shard.centerX - clickX;
      const deltaY = shard.centerY - clickY;
      const angle = Math.atan2(deltaY, deltaX);
      const distance = (Math.random() * 150 + 100); 
      
      const flyX = Math.cos(angle) * distance;
      const flyY = Math.sin(angle) * distance;
      const rotate = Math.random() * 360 - 180;

      shard.transform = `translate(${flyX}%, ${flyY}%) rotate(${rotate}deg) scale(0.3)`;
      shard.opacity = 0;
      shard.zIndex = 100; // Полет поверх всего
      shard.transitionDuration = '1s'; // Медленный красивый разлет
    });
  }
}
</script> -->

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