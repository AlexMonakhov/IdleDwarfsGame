<template>
  <div
    ref="cardRef"
    class="tilt-card"
    @mousemove="handleMouseMove"
    @mouseleave="handleMouseLeave"
    :style="cardStyle"
  >
    <!-- Свечение цветом редкости вокруг карты при наведении -->
    <div class="hover-glow"></div>

    <span class="tilt-card-inner" >
        <info-component></info-component>
        <div class="tilt-card-content">

        <slot>
          <icon-helm></icon-helm>
        </slot>
      </div>
    </span>

    <!-- Волна цвета редкости из точки последнего удара -->
    <div v-if="clickPhase >= 3" class="reveal-ring"></div>

    <div  class="rock-layer" @click="handleRockClick" :style="{ opacity: layerOpacity, pointerEvents: clickPhase >= 3 ? 'none' : 'auto' }">

      <!-- Осколки (каждый рисует всю рубашку, clip-path оставляет только его кусок) -->
      <div
        v-for="shard in shards"
        :key="shard.id"
        class="rock-shard"
        :style="{
          clipPath: shard.clipPath,
          transform: shard.transform,
          '--base-transform': shard.transform, /* Передаем базовый сдвиг в CSS */
          opacity: shard.opacity,
          transformOrigin: `${shard.centerX}% ${shard.centerY}%`,
          transitionDuration: shard.transitionDuration,
          transitionTimingFunction: shard.easing || 'cubic-bezier(0.175, 0.885, 0.32, 1.275)',
          animation: shard.animation || 'none'
        }"
      ></div>

      <!-- Идеально гладкая "крышка" для 0 фазы (скрывает микро-швы clip-path) -->
      <div
        class="rock-cover"
        :style="{ opacity: clickPhase === 0 ? 1 : 0 }"
      >
        <div class="back-glow"></div>
        <div class="back-sheen"></div>
      </div>


  </div>
  </div>
    <!-- Внутренний контейнер для эффекта глубины -->

</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick, watch } from 'vue';
import { generateOverlaySets, OverlayBlock } from './cardHelper';
import { ART_LAYOUT, CardBackOptions, getCardBack, getCardGlowMask, getCardSilhouetteMask } from './cardBackArt';
import IconHelm from '../icons/IconHelm.vue';
import InfoComponent from '../common/InfoComponent.vue';

type CardColor = 'pink' | 'green' | 'blue' | 'yellow' | 'red' | 'grey' | 'dark' | 'light';

interface Props {
  color?: CardColor
  // Все настройки рубашки одним объектом: { variant, seed, frame: {...} } (см. card/README.md)
  back?: CardBackOptions
}


const props = withDefaults(defineProps<Props>(), {
  color: 'grey',
  back: () => ({}),
})

// Фолбэки на случай, если в App.vue нет переменной --{color}-gem-color (например, для grey)
const COLOR_FALLBACK: Record<CardColor, string> = {
  grey: '#9aa3ad',
  dark: '#4b5563',
  light: '#e5e7eb',
  green: '#00c853',
  blue: '#2196f3',
  pink: '#e91e63',
  red: '#d40808',
  yellow: '#ffb300',
};

const blocks = ref<[OverlayBlock[], OverlayBlock[], OverlayBlock[]]>([[], [], []]);
const clicked = ref<number>(-1);

const cardRef = ref<HTMLElement | null>(null);

const rotateX = ref(0);
const rotateY = ref(0);
const layerOpacity = ref(1);
// Точка последнего удара (в % от карточки) — там ярче всего светится ядро
const hitX = ref(50);
const hitY = ref(50);
// Готовая картинка рубашки (CSS url), генерируется под цвет редкости
const cardBack = ref('none');
// Максимальный угол наклона в градусах
const maxTilt = 15;

const updateCardBack = async () => {
  await nextTick();
  if (!cardRef.value) return;
  // Берем уже вычисленный браузером цвет (с учетом переменных из App.vue)
  const color = getComputedStyle(cardRef.value).getPropertyValue('--bg-color').trim() || COLOR_FALLBACK[props.color];
  cardBack.value = await getCardBack(color, props.back);
};

// Маски зависят только от стиля и кромки — не пересчитываем их на каждое движение мыши
const masks = computed(() => ({
  '--back-emblem-glow': getCardGlowMask(props.back),
  '--back-silhouette': getCardSilhouetteMask(props.back),
}));

onMounted(() => {
  // Генерируем 10 случайных блоков
  shards.value = generateShards(COLS, ROWS);
  blocks.value = generateOverlaySets(7);
  updateCardBack();
});

watch(() => [props.color, JSON.stringify(props.back)], updateCardBack);

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

const cardStyle = computed(() => (
 {
    transform: `perspective(1000px) rotateX(${rotateX.value}deg) rotateY(${rotateY.value}deg)`,
    // Меняем transition динамически:
    // Быстрый отклик при движении, плавная анимация возврата (когда координаты 0)
    transition: (rotateX.value === 0 && rotateY.value === 0)
      ? 'transform 0.5s cubic-bezier(0.23, 1, 0.32, 1)'
      : 'transform 0.1s linear',
    '--bg-color': `var(--${props.color}-gem-color, ${COLOR_FALLBACK[props.color] ?? COLOR_FALLBACK.grey})`,
    '--card-back': cardBack.value,
    ...masks.value,
    // Картинка рубашки шире карты (торчащие кристаллы) — размеры слоя породы берем из арта
    '--art-left': ART_LAYOUT.left,
    '--art-top': ART_LAYOUT.top,
    '--art-width': ART_LAYOUT.width,
    '--art-height': ART_LAYOUT.height,
    '--art-inset-x': ART_LAYOUT.insetX,
    '--art-inset-y': ART_LAYOUT.insetY,
    '--art-card-width': ART_LAYOUT.cardWidth,
    '--art-card-height': ART_LAYOUT.cardHeight,
    '--hit-x': `${hitX.value}%`,
    '--hit-y': `${hitY.value}%`,
      // Переменные для управления бликами
      '--glare-x': `${rotateY.value / maxTilt * 50 + 50}%`,
      '--glare-y': `${-rotateX.value / maxTilt * 50 + 50}%`,
      // Переменная для плавного возврата бликов в центр
      '--glare-transition': rotateX.value === 0 && rotateY.value === 0
        ? 'background-position 0.6s cubic-bezier(0.23, 1, 0.32, 1)'
      : 'background-position 0s',
}));



interface Shard {
  id: number;
  clipPath: string;
  centerX: number;
  centerY: number;
  transform: string;
  opacity: number;
  transitionDuration: string;
  // Рандомное смещение для 2-й фазы (чтобы куски чуть сдвигались)
  shiftX: number;
  shiftY: number;
  animation?: string; // Для дрожания в фазе 2
  easing?: string; // Для фазы 3 (разлет)
}

const shards = ref<Shard[]>([]);
const clickPhase = ref(0); // 0: целая, 1: трещины, 2: разломы, 3: разлет

const COLS = 6;
const ROWS = 7;

function generateShards(cols: number, rows: number): Shard[] {
  const points: { x: number; y: number }[][] = [];
  const cellWidth = 100 / cols;
  const cellHeight = 100 / rows;

  for (let i = 0; i <= rows; i++) {
    const rowPoints = [];
    for (let j = 0; j <= cols; j++) {
      let x = j * cellWidth;
      let y = i * cellHeight;

      if (i > 0 && i < rows && j > 0 && j < cols) {
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
        clipPath: `polygon(${p1.x}% ${p1.y}%, ${p2.x}% ${p2.y}%, ${p3.x}% ${p3.y}%, ${p4.x}% ${p4.y}%)`,
        centerX: (p1.x + p2.x + p3.x + p4.x) / 4,
        centerY: (p1.y + p2.y + p3.y + p4.y) / 4,

        // Начальное состояние (Фаза 0)
        transform: 'translate(0px, 0px) rotate(0deg) scale(1)',
        opacity: 1,
        transitionDuration: '0.1s', // Быстрый первый удар

        // Подготавливаем хаотичные сдвиги для 2-й фазы
        shiftX: (Math.random() - 0.5) * 4,
        shiftY: (Math.random() - 0.5) * 4,
      });
    }
  }
  return result;
}

const handleRockClick = (event: MouseEvent) => {
  if (clickPhase.value >= 3) return; // Если уже разбито - игнорируем

  const target = event.currentTarget as HTMLElement;
  const rect = target.getBoundingClientRect();

  // Координаты клика в процентах относительно карточки
  const clickX = ((event.clientX - rect.left) / rect.width) * 100;
  const clickY = ((event.clientY - rect.top) / rect.height) * 100;
  hitX.value = clickX;
  hitY.value = clickY;

  clickPhase.value++;

  shards.value.forEach(shard => {
    if (clickPhase.value === 1) {
      // ФАЗА 1: Легкие трещины (сквозь щели светится ядро)
      shard.transform = 'translate(0px, 0px) rotate(0deg) scale(0.97)';
      shard.transitionDuration = '0.15s'; // Резкий раскол

    } else if (clickPhase.value === 2) {
      // ФАЗА 2: Глубокие разломы и сдвиг кусков
      shard.transform = `translate(${shard.shiftX}px, ${shard.shiftY}px) rotate(${shard.shiftX}deg) scale(0.92)`;
      shard.transitionDuration = '0.5s';
      // Делаем рандомную скорость тряски (от 0.15s до 0.25s) для хаотичности
      const randomSpeed = (0.15 + Math.random() * 0.1).toFixed(2);
      shard.animation = `shake ${randomSpeed}s infinite alternate`; // Добавляем дрожание

    } else if (clickPhase.value === 3) {
      // ФАЗА 3: Полное разрушение и разлет
      shard.animation = 'none'; // Останавливаем дрожание
      const deltaX = shard.centerX - clickX;
      const deltaY = shard.centerY - clickY;

      const angle = Math.atan2(deltaY, deltaX);
      // Сила разлета: от 80% до 250% ширины карточки
      const distance = (Math.random() * 150 + 80);

      const flyX = Math.cos(angle) * distance;
      const flyY = Math.sin(angle) * distance;
      const rotate = Math.random() * 360 - 180;

      // Летим в % от размера карточки
      setTimeout(() => {
        shard.transform = `translate(${flyX}%, ${flyY}%) rotate(${rotate}deg) scale(0.4)`;
        shard.opacity = 0;
        shard.transitionDuration = '1.2s'; // Медленный красивый разлет
        shard.easing = 'cubic-bezier(0.25, 1, 0.5, 1)';
        layerOpacity.value = 0; // Скрываем слой, чтобы не было видно "крышки"
         // Отключаем overflow, чтобы куски могли вылетать за пределы карточки
      }, 20); // Даем браузеру зафиксировать начальное состояние
    }
  });
};
</script>

<style scoped>
/* Настройки самой карточки оставляй свои */
.tilt-card {
  --bg-color: rgba(255, 192, 203, 0.8); /* Цвет кристалла по умолчанию */
  position: relative;
  width: 200px;
  height: 300px;
  margin: 10px;

  /* Металлический фон рамки */
  background: var(--metalic-background, linear-gradient(135deg, #df0404, #240315));
  padding: 6px; /* Толщина рамки (сделали толще, т.к. карточка большая) */
  border: 1px solid rgba(0, 0, 0, 0.2);
  border-radius: 16px; /* Радиус внешней рамки */

  /* Внешняя тень и светлый блик на самом металле */
  box-shadow:
    0 15px 30px rgba(0, 0, 0, 0.4),
    inset 0 1px 3px rgba(255, 255, 255, 0.8);

  transform-style: preserve-3d;
  will-change: transform;

  display: flex;
  align-items: center;
  justify-content: center;
}

/* Имитация самого кристалла (.gem-inner) через ::before */
.tilt-card::before {
  content: '';
  position: absolute;
  top: 6px; left: 6px; right: 6px; bottom: 6px; /* Отступы на ширину рамки */
  border-radius: 12px; /* Внутренний радиус кристалла */

  /* Цвет кристалла (здесь розовый как в .pink-gem, можешь менять) */
  background-color: var(--bg-color, rgba(255, 192, 203, 0.8));
  opacity: 0.9;
  /* Внутреннее свечение кристалла */
  box-shadow: inset 0 0 6px rgba(255, 255, 255, 0.5), 0px 1px 3px rgba(0,0,0,0.5);
  z-index: 0; /* Под контентом */
  filter: drop-shadow(0 0 6px rgba(255, 255, 255, 0.5)) drop-shadow(0 0 4px rgba(0,0,0,0.5)) contrast(1.4);
}

/* Блики света на кристалле (.gem-inner::after) */
.tilt-card::after {
  content: '';
  position: absolute;
  top: 6px; left: 6px; right: 6px; bottom: 6px;
  border-radius: 12px;

  /* Те самые граненые переливы */
  background: linear-gradient( 45deg ,
    rgba(0,0,0,0.3) 0%,
    rgba(0,0,0,0.1) 25%,
    rgba(255,255,255,0.3) 30%,
    rgba(255,255,255,0.2) 40%,
    rgba(0,0,0,0.1) 45%,
    rgba(0,0,0,0.1) 65%,
    rgba(255,255,255,0.3) 70%,
    rgba(255,255,255,0.2) 75%,
    rgba(0,0,0,0.1) 80%,
    rgba(0,0,0,0.3) 100%);

  pointer-events: none;
  z-index: 1; /* Выше фона, но ниже текста */
  background-size: 250% 250%;

  /* Привязываем позицию фона к нашим CSS-переменным из Vue */
  background-position: var(--glare-x, 50%) var(--glare-y, 50%);

  /* Плавность движения бликов (берем из Vue, чтобы совпадало с наклоном) */
  transition: var(--glare-transition, background-position 0.1s linear);
}

/* Текст внутри карточки */
.tilt-card-content {
  position: relative;
  z-index: 2; /* Текст поверх бликов */
  color: #ffffff;
  text-align: center;
  pointer-events: none;

  /* 3D эффект */
  transform: translateZ(20px);

  /* Перенесли стили шрифта из .gem-text */
  font-family: 'Arial', sans-serif;
  font-weight: bold;
  font-size: 24px; /* Сделали крупнее для карточки */
  text-transform: uppercase;
  letter-spacing: 2px;
  text-shadow: 2px 2px 0px rgba(0,0,0,0.6);
}

.tilt-card-inner{
  position: relative;
  width: 80%;
  height: 80%;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 3px solid rgba(255, 255, 255, 0.2);
  border-radius: 10px;
}



/* Контейнер породы (ядро) */
.rock-layer {
  position: absolute;
  /* Слой больше карты на запас под торчащие кристаллы */
  top: var(--art-top, -1%);
  left: var(--art-left, -1%);
  width: var(--art-width, 102%);
  height: var(--art-height, 102%);
  overflow: visible; /* Чтобы куски вылетали за пределы карточки */
  cursor: pointer;
  transform: translateZ(21px);
  z-index: 10;
  transition: opacity 1s ease-out; /* Плавное исчезновение слоя при фазе 3 */
}

/* Ядро цвета редкости, которое просвечивает сквозь щели (ярче всего в точке удара).
   Только в границах карты, чтобы не светиться в прозрачных местах вокруг силуэта */
.rock-layer::before {
  content: '';
  position: absolute;
  inset: var(--art-inset-y, 0) var(--art-inset-x, 0);
  border-radius: 16px;
  background: radial-gradient(
    circle at var(--hit-x, 50%) var(--hit-y, 50%),
    #fff 0%,
    var(--bg-color) 30%,
    color-mix(in srgb, var(--bg-color) 40%, #111) 65%,
    #111 100%
  );
}

/* Камень рубашки — одна готовая картинка, одинаковая у осколков и "крышки" */
.rock-shard,
.rock-cover {
  /* Запасной фон, пока картинка генерируется, — только в границах карты */
  background:
    var(--card-back, none) center / 100% 100% no-repeat,
    linear-gradient(160deg, #545a63 0%, #363a41 50%, #22252a 100%)
      center / var(--art-card-width, 100%) var(--art-card-height, 100%) no-repeat;
}

/* Осколки */
.rock-shard {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;

  /* Анимация: duration задается из JS динамически */
  transition-property: transform, opacity;
  transition-timing-function: cubic-bezier(0.175, 0.885, 0.32, 1.275); /* Эффект "отскока" при ударе */
  will-change: transform, opacity;
}

/* Верхняя "крышка" для нулевой фазы */
.rock-cover {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  pointer-events: none; /* Чтобы клики проходили сквозь нее к .rock-layer */
  transform: translateZ(30px); /* Чтобы она была выше осколков */
  transition: opacity 0.1s ease-out; /* Резко исчезает при первом клике */
}

/* Пульсирующее свечение рун (только на целой карте, поверх запеченного свечения) */
.back-glow,
.back-sheen {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.back-glow {
  -webkit-mask: var(--back-emblem-glow) center / 100% 100% no-repeat;
  mask: var(--back-emblem-glow) center / 100% 100% no-repeat;
  background: var(--bg-color);
  animation: rune-pulse 2.6s ease-in-out infinite;
}

.rock-layer:hover .back-glow {
  animation-duration: 1.4s;
}

/* Блик, бегающий по камню за наклоном карты */
.back-sheen {
  background: linear-gradient(
    115deg,
    transparent 35%,
    rgba(255, 255, 255, 0.12) 45%,
    rgba(255, 255, 255, 0.22) 50%,
    rgba(255, 255, 255, 0.12) 55%,
    transparent 65%
  );
  background-size: 250% 250%;
  background-position: var(--glare-x, 50%) var(--glare-y, 50%);
  transition: var(--glare-transition, background-position 0.1s linear);
  /* Блик только по самому кристаллу, не по прозрачным местам вокруг */
  -webkit-mask: var(--back-silhouette) center / 100% 100% no-repeat;
  mask: var(--back-silhouette) center / 100% 100% no-repeat;
}

@keyframes rune-pulse {
  0%, 100% { opacity: 0; }
  50% { opacity: 0.6; }
}

/* Свечение при наведении: тень снаружи карты, показываем через opacity (дешево, на GPU) */
.hover-glow {
  position: absolute;
  inset: -1px;
  border-radius: 16px;
  pointer-events: none;
  opacity: 0;
  transition: opacity 0.35s ease;
}

.hover-glow::before {
  content: '';
  position: absolute;
  inset: 0;
  border-radius: inherit;
  box-shadow:
    0 0 14px 2px var(--bg-color),
    0 0 40px 10px color-mix(in srgb, var(--bg-color) 60%, transparent);
}

.tilt-card:hover .hover-glow {
  opacity: 1;
}

/* Пока курсор на карте — свечение мягко "дышит" */
.tilt-card:hover .hover-glow::before {
  animation: hover-glow-pulse 1.8s ease-in-out infinite;
}

@keyframes hover-glow-pulse {
  0%, 100% { opacity: 0.75; }
  50% { opacity: 1; }
}

/* Ударная волна из точки последнего клика */
.reveal-ring {
  position: absolute;
  left: var(--hit-x);
  top: var(--hit-y);
  width: 60px;
  height: 60px;
  margin: -30px 0 0 -30px;
  border-radius: 50%;
  border: 3px solid var(--bg-color);
  box-shadow: 0 0 12px var(--bg-color), inset 0 0 12px var(--bg-color);
  pointer-events: none;
  z-index: 12;
  animation: shockwave 0.7s ease-out forwards;
}

@keyframes shockwave {
  0% { transform: translateZ(25px) scale(0.2); opacity: 1; }
  100% { transform: translateZ(25px) scale(6); opacity: 0; }
}
</style>

<style>
/* Глобально, т.к. анимация задается inline-стилем из JS */
@keyframes shake {
  0% { transform: var(--base-transform) translate(0px, 0px) rotate(0deg); }
  25% { transform: var(--base-transform) translate(-1px, 1.5px) rotate(-1deg); }
  50% { transform: var(--base-transform) translate(1px, -1px) rotate(1deg); }
  75% { transform: var(--base-transform) translate(-1.5px, -1px) rotate(-0.5deg); }
  100% { transform: var(--base-transform) translate(1px, 1px) rotate(0.5deg); }
}
</style>
