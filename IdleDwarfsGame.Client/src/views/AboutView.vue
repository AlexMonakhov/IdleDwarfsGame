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
      <img :src="isNight ? backgroundskynight : backgroundsky" class="wide-image-bg" @load="onSkyLoad" />
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
        <template v-for="b in buildings" :key="b.key">
          <!-- Обычная и hover-картинка лежат друг на друге, при наведении меняется только прозрачность.
               Если менять src, браузер заново декодирует большую картинку и здание мигает -->
          <div
            :class="['building', `building-${b.key}`, { 'is-hovered': hoveredBuilding === b.key, 'is-active': !!b.onClick }]"
            :style="{ left: `${b.left}%`, top: `${b.top}%`, width: `${b.width}%` }"
            @mouseenter="hoveredBuilding = b.key"
            @mouseleave="hoveredBuilding = null"
            @click.capture="b.onClick?.()"
          >
            <img :src="b.image" class="building-img" @dragstart.prevent />
            <img :src="b.hover" class="building-img building-img-hover" @dragstart.prevent />
          </div>
          <!-- Плашка с названием: стоит у нижнего края здания, наведение и клик — как у самого здания -->
          <div
            :class="['building-plaque', { 'is-hovered': hoveredBuilding === b.key, 'is-active': !!b.onClick }]"
            :style="{ left: `${b.left}%`, top: `${b.top}%` }"
            @mouseenter="hoveredBuilding = b.key"
            @mouseleave="hoveredBuilding = null"
            @click.capture="b.onClick?.()"
          >
            {{ t(`town.buildings.${b.key}`) }}
          </div>
        </template>
      </div>
    </div>
  </div>

  <!-- Ширина — под строку уровня: два отряда по 5 портретов, арена и кнопка боя без переноса -->
  <ModalComponent
    v-model="campaignDialogOpen"
    :header-message="t('campaign.header')"
    width="min(980px, calc(94 * var(--vw)))"
    height="min(760px, calc(86 * var(--vh)))"
  >
    <FightPreviewListContainer></FightPreviewListContainer>
  </ModalComponent>

  <ModalComponent
    v-model="infinityTowerDialogOpen"
    :header-message="t('tower.header')"
    width="min(980px, calc(94 * var(--vw)))"
    height="min(760px, calc(86 * var(--vh)))"
  >
    <TowerContainer></TowerContainer>
  </ModalComponent>
</template>

<script lang="ts" setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue';
import background from "@/assets/town/background.png";
import backgroundsky from "@/assets/backgroundsky.png";
import altar from "@/assets/town/altar.png";
import altarHover from "@/assets/town/altar-hover.png";
import arena from "@/assets/town/arena.png";
import arenaHover from "@/assets/town/arena-hover.png";
import blacksmith from "@/assets/town/blacksmith.png";
import blacksmithHover from "@/assets/town/blacksmith-hover.png";
import campaign from "@/assets/town/campaign.png";
import campaignHover from "@/assets/town/campaign-hover.png";
import expedition from "@/assets/town/expedition.png";
import expeditionHover from "@/assets/town/expedition-hover.png";
import guild from "@/assets/town/guild.png";
import guildHover from "@/assets/town/guild-hover.png";
import market from "@/assets/town/market.png";
import marketHover from "@/assets/town/market-hover.png";
import tower from "@/assets/town/tower.png";
import towerHover from "@/assets/town/tower-hover.png";
import warehouse from "@/assets/town/warehouse.png";
import warehouseHover from "@/assets/town/warehouse-hover.png";
import mine from "@/assets/town/mine.png";
import mineHover from "@/assets/town/mine-hover.png";
import ModalComponent from '@/components/common/ModalComponent.vue';
import FightPreviewListContainer from '@/components/interface/FightPreviewListContainer.vue';
import TowerContainer from '@/components/tower/TowerContainer.vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useAuth } from '@/composables/UseAuth';
import { useGuild } from '@/composables/UseGuild';
import { usePvp } from '@/composables/UsePvp';
import { useInterface } from '@/composables/UseInterface';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const { isAuthenticated } = useAuth();
const { guildOpen } = useGuild();
const { pvpOpen } = usePvp();
const { expeditionsOpen, inventoryOpen, upgradeOpen, miningOpen } = useInterface();


// --- НАСТРОЙКИ ---
const PARALLAX_SPEED = 0.3; // Чем меньше число, тем медленнее движется фон (глубже эффект)
const EDGE_THRESHOLD = 0.01; 
const MAX_SPEED = 10;
// Дневное небо по GMT: с DAY_START_HOUR (включительно) до DAY_END_HOUR (не включительно)
const DAY_START_HOUR = 6;
const DAY_END_HOUR = 20;
const SKY_CHECK_INTERVAL_MS = 60_000;
// Пока смена дня и ночи отключена — всегда день. Поставить false, чтобы включить.
const ALWAYS_DAY = true;

// --- СОСТОЯНИЕ ---
const slider = ref<HTMLElement | null>(null);
const imageRef = ref<HTMLImageElement | null>(null);
const isDown = ref(false);
const startX = ref(0);
const scrollLeftState = ref(0);
const opacity = ref(0);
const campaignDialogOpen = ref(false);
// /about?tower=1 — возврат с арены после боя в башне: окно башни открывается сразу
const infinityTowerDialogOpen = ref(!!route.query.tower);

// --- ЗДАНИЯ ГОРОДА ---
// left/top/width — в процентах от фоновой картинки. left — центр здания,
// top — нижний край картинки (точка, где она стоит на платформе).
// Порядок в массиве = порядок отрисовки: что ниже на экране, то идёт позже и перекрывает верхние.
interface TownBuilding {
  key: string;
  image: string;
  hover: string;
  left: number;
  top: number;
  width: number;
  onClick?: () => void;
}

// Разделы игрока: гостя отправляем на логин, а после входа он вернётся в город
const requireAuth = (open: () => void) => () => {
  if (!isAuthenticated.value) {
    router.push({ name: 'login', query: { redirect: route.fullPath } });
    return;
  }
  open();
};

// Кузница и рынок пока без раздела: подсвечиваются, но по клику ничего не открывают
const buildings: TownBuilding[] = [
  { key: 'expedition', image: expedition, hover: expeditionHover, left: 34,   top: 31,   width: 8,    onClick: requireAuth(() => (expeditionsOpen.value = true)) },
  { key: 'guild',      image: guild,      hover: guildHover,      left: 14.5, top: 43,   width: 12,   onClick: requireAuth(() => (guildOpen.value = true)) },
  { key: 'tower',      image: tower,      hover: towerHover,      left: 83,   top: 51,   width: 13.5, onClick: () => (infinityTowerDialogOpen.value = true) },
  { key: 'altar',      image: altar,      hover: altarHover,      left: 38,   top: 53,   width: 9,    onClick: requireAuth(() => (upgradeOpen.value = true)) },
  { key: 'blacksmith', image: blacksmith, hover: blacksmithHover, left: 26.5, top: 56,   width: 9 },
  { key: 'campaign',   image: campaign,   hover: campaignHover,   left: 51,   top: 56,   width: 14.5, onClick: () => (campaignDialogOpen.value = true) },
  { key: 'market',     image: market,     hover: marketHover,     left: 61.5, top: 61.5, width: 7.5 },
  { key: 'arena',      image: arena,      hover: arenaHover,      left: 59,   top: 83,   width: 9,    onClick: requireAuth(() => (pvpOpen.value = true)) },
  { key: 'warehouse',  image: warehouse,  hover: warehouseHover,  left: 35,   top: 87,   width: 9,    onClick: requireAuth(() => (inventoryOpen.value = true)) },
  { key: 'mine',       image: mine,       hover: mineHover,       left: 47,   top: 84,   width: 9,    onClick: requireAuth(() => (miningOpen.value = true)) },
];
const hoveredBuilding = ref<string | null>(null);

// Параллакс смещение
const bgOffset = ref(0);

// --- ДЕНЬ / НОЧЬ ---
const isNightNow = () => {
  if (ALWAYS_DAY) return false;
  const hour = new Date().getUTCHours();
  return hour < DAY_START_HOUR || hour >= DAY_END_HOUR;
};
const isNight = ref(isNightNow());
let skyIntervalId = 0;
let skyLoadedOnce = false;

// Центрируем только при первой загрузке, чтобы смена неба не сбрасывала скролл игрока
const onSkyLoad = () => {
  if (skyLoadedOnce) return;
  skyLoadedOnce = true;
  scrollToCenter();
};

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
  skyIntervalId = window.setInterval(() => {
    isNight.value = isNightNow();
  }, SKY_CHECK_INTERVAL_MS);
  if (imageRef.value && imageRef.value.complete) {
    scrollToCenter();
  }
});

onUnmounted(() => {
  cancelAnimationFrame(animationFrameId);
  clearInterval(skyIntervalId);
});

const onStageClick = (n: number) => {
  console.log(`Уровень ${n} нажат`);
};
</script>

<style lang="scss" scoped>
.about-container {
  position: relative;
  width: 100%;
  height: var(--stage-h);
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
  /* Приглушённый фон, чтобы здания на нём выделялись */
  filter: saturate(0.5);
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

/* Позиция и ширина задаются инлайном из массива buildings */
.building {
  position: absolute;
  z-index: 1;
  height: auto;
  transform: translate(-50%, -100%);
  pointer-events: auto; /* Разрешаем клики по картинке */
  /* Без наведения здание приглушено. Пустые тени — чтобы список фильтров совпадал с hover и переход шёл плавно */
  filter:
    saturate(0.7)
    drop-shadow(0 0 0 transparent)
    drop-shadow(0 0 0 transparent)
    drop-shadow(0 0 0 transparent);
  transition: filter 0.2s ease-out;

  &.is-active { cursor: pointer; }

  /* Полный цвет и светящийся контур по силуэту здания: drop-shadow повторяет прозрачность картинки */
  &.is-hovered {
    filter:
      saturate(1)
      drop-shadow(0 0 2px rgba(255, 236, 170, 0.95))
      drop-shadow(0 0 10px rgba(255, 196, 90, 0.75))
      drop-shadow(0 0 24px rgba(255, 150, 40, 0.45));
  }
}

.building-img {
  display: block;
  width: 100%;
  height: auto;
}

/* Hover-картинка того же размера ровно поверх обычной */
.building-img-hover {
  position: absolute;
  inset: 0;
  opacity: 0;
  transition: opacity 0.15s ease-out;

  .building.is-hovered & { opacity: 1; }
}

/* Плашки поверх всех зданий, чтобы нижние здания не закрывали подписи верхних */
.building-plaque {
  position: absolute;
  z-index: 2;
  transform: translate(-50%, -60%);
  padding: 0.25em 0.9em;
  border-radius: 8px;
  background: var(--wood-background);
  box-shadow:
    inset 0 0 6px black,
    0 0 0 2px rgb(157, 173, 175),
    0 0 0 4px #333,
    0 4px 8px rgba(0, 0, 0, 0.6);
  color: #ffe2a0;
  font-family: 'Arial', sans-serif;
  font-size: clamp(11px, calc(1.7 * var(--vh)), 18px);
  font-weight: bold;
  letter-spacing: 0.03em;
  white-space: nowrap;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.85);
  pointer-events: auto;
  user-select: none;
  transition: color 0.2s ease-out, box-shadow 0.2s ease-out, transform 0.2s ease-out;

  &.is-active { cursor: pointer; }

  &.is-hovered {
    z-index: 3;
    color: #fff4d0;
    transform: translate(-50%, -60%) scale(1.06);
    box-shadow:
      inset 0 0 6px black,
      0 0 0 2px #ffd166,
      0 0 0 4px #333,
      0 0 14px 4px rgba(255, 190, 80, 0.55);
  }
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