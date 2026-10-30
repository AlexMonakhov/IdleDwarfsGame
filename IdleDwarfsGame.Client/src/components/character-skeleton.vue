<script lang="ts" setup>
    import { ref, computed, onMounted, onUnmounted, watch, defineProps, withDefaults } from "vue";
    import { HeroModel } from "@/models/hero";
    import { SpriteTint } from "@/battle/FxRegistry";
    import { getSpriteAtlas, getSpriteVariants, getSpriteZoom, SpriteAnimation, SpriteAtlas } from "@/battle/sprites";
    import { acquireSheet, releaseSheet, SheetImage, sheetHeight, sheetWidth } from "@/battle/spriteCache";

const props = withDefaults(defineProps<{
  model: HeroModel;
  animationType?: SpriteAnimation;
  frameWidth: number;
  frameHeight: number;
  frameCount: number;
  columns: number;
  fps?: number;
  scale?: number;
  padding?: number;
  // Постоянная окраска силуэта (статусы)
  tint?: SpriteTint | null;
  // Множитель скорости idle-анимации (0 — заморозка)
  speed?: number;
  // Полная остановка анимации на текущем кадре (смерть юнита)
  paused?: boolean;
  // Какие анимации загрузить: остальные не грузятся и не запускаются (в бою не ходят — walk не нужен)
  animations?: SpriteAnimation[];
}>(), {
  animationType: 'idle',
  padding: 1,
  fps: 10,
  scale: 1,
  frameCount: 25,
  tint: null,
  speed: 1,
  paused: false,
  animations: () => ['idle', 'walk', 'attack', 'victory', 'death'],
});

// Короткая вспышка силуэта (получение урона, удар, лечение)
let flashState: { color: string; alpha: number; start: number; duration: number } | null = null;

const flash = (color: string, alpha = 0.6, duration = 350) => {
  flashState = { color, alpha, start: performance.now(), duration };
};

// Где на канвасе нарисован текущий кадр: x, y, ширина, высота
type Rect = [number, number, number, number];

// Закрасить только непрозрачные пиксели спрайта. Заливаем лишь прямоугольник кадра, а не весь канвас:
// канвас крупнее кадра в zoom раз по каждой стороне, а пульсирующая окраска (статусы, цель) перерисовывается каждый кадр
function overlay(color: string, alpha: number, [x, y, w, h]: Rect) {
  if (!ctx || alpha <= 0) return;
  ctx.save();
  ctx.globalCompositeOperation = 'source-atop';
  ctx.globalAlpha = Math.min(alpha, 1);
  ctx.fillStyle = color;
  ctx.fillRect(Math.floor(x), Math.floor(y), Math.ceil(w) + 1, Math.ceil(h) + 1);
  ctx.restore();
}

/* ===== STATE ===== */
// Внутреннее состояние анимации (то, что мы реально видим)
const localAnimType = ref<SpriteAnimation>('idle');

const canvasRef = ref<HTMLCanvasElement | null>(null);

let ctx: CanvasRenderingContext2D | null = null;
interface Sheet {
  url: string;
  // Атлас листа (кадры без пустого поля, см. battle/sprites.ts): грузится и рисуется он; null — сам лист
  atlas: SpriteAtlas | null;
  // Лист, уменьшенный под размер на экране (см. battle/spriteCache.ts); null — ещё готовится
  image: SheetImage | null;
  // Ширина копии, запрошенной последней: ответ на устаревший запрос (сменился масштаб) не применяется
  requested: number;
  // Ширина копии в image — её держим в кэше, пока рисуем; 0 — копии нет
  held: number;
  // Готовность копии последнего запрошенного размера
  loading: Promise<void>;
  // Во сколько раз кадр листа шире обычного (атаки с прыжками и шлейфами), см. battle/sprites.ts
  zoom: number;
}
const emptySheet: Sheet = { url: '', atlas: null, image: null, requested: 0, held: 0, loading: Promise.resolve(), zoom: 1 };
let current: Sheet = emptySheet; // Текущий спрайт-лист
// Все варианты листов каждой анимации (_1, _2, ...): при каждом запуске анимации берётся случайный
const sheets: Record<SpriteAnimation, Sheet[]> = { idle: [], walk: [], attack: [], victory: [], death: [] };
// Компонент снят: пришедшие после этого копии сразу отдаются обратно в кэш
let disposed = false;

let currentFrame = 0;
let lastTime = 0;
let animationId = 0;
// Кадр перерисовывается, только когда на нём что-то поменялось (или идёт вспышка/пульсация окраски)
let dirty = true;

// Плотность пикселей экрана: канвас рисуется в пикселях экрана, а не CSS, иначе браузер растягивает его и мылит.
// Меняется при переносе окна на другой монитор и при Ctrl + колесо
const dpr = ref(window.devicePixelRatio || 1);
const onResize = () => {
  dpr.value = window.devicePixelRatio || 1;
};

const sheetUrls = (type: SpriteAnimation) => getSpriteVariants(type, props.model.color, props.model.lvl, props.model.unitType);

// Ширина кадра листа в пикселях экрана
const cellPx = (zoom: number) => props.frameWidth * props.scale * zoom * dpr.value;

const sheetSource = (sheet: Sheet) => sheet.atlas?.url ?? sheet.url;

// Ширина копии, у которой клетка кадра шириной px пикселей экрана
const copyWidth = (sheet: Sheet, px: number) =>
  (sheet.atlas ? Math.round((sheet.atlas.width * px) / sheet.atlas.cell) : props.columns * px);

// Запросить у кэша копию листа под текущий размер; пока она готовится, рисуется прежняя.
// Прежняя отдаётся кэшу, только когда новая уже на её месте: закрытую копию рисовать нельзя
const prepare = (sheet: Sheet) => {
  const width = copyWidth(sheet, Math.ceil(cellPx(sheet.zoom)));
  if (width === sheet.requested) return;
  sheet.requested = width;
  const source = sheetSource(sheet);
  sheet.loading = acquireSheet(source, width).then(image => {
    if (disposed || sheet.requested !== width) {
      releaseSheet(source, width);
      return;
    }
    if (sheet.held) releaseSheet(source, sheet.held);
    sheet.image = image;
    sheet.held = width;
    if (sheet === current) dirty = true;
  }).catch(() => undefined);
};

const allSheets = () => (Object.keys(sheets) as SpriteAnimation[]).flatMap(type => sheets[type]);

const loadSheets = () => {
  props.animations.forEach(type => {
    sheets[type] = sheetUrls(type).map(url => ({
      url,
      atlas: getSpriteAtlas(url) ?? null,
      image: null,
      requested: 0,
      held: 0,
      loading: Promise.resolve(),
      zoom: getSpriteZoom(url),
    }));
    sheets[type].forEach(prepare);
  });
};

const releaseSheets = () => {
  disposed = true;
  allSheets().forEach(sheet => {
    if (sheet.held) releaseSheet(sheetSource(sheet), sheet.held);
    sheet.held = 0;
    sheet.image = null;
  });
};

// Все листы юнита готовы к отрисовке (арена ждёт этого перед началом боя).
// Лист попадает в видеокарту при первой отрисовке — рисуем каждый по пикселю сейчас, а не посреди первого удара
const ready = () => Promise.all(allSheets().map(sheet => sheet.loading)).then(() => {
  allSheets().forEach(sheet => {
    if (sheet.image) ctx?.drawImage(sheet.image, 0, 0, 1, 1, 0, 0, 1, 1);
  });
  dirty = true;
});

// Рамка юнита в вёрстке — обычный кадр; канвас больше на самый крупный лист и выходит за рамку,
// чтобы атака с zoom рисовалась целиком, не сдвигая соседей.
// inline-block: у блочного канваса отрицательные margin схлопываются с родителями, и рамка юнита
// (а с ней тень и точки привязки эффектов) вырастает до размера канваса
const boxWidth = computed(() => props.frameWidth * props.scale * props.padding);
const boxHeight = computed(() => props.frameHeight * props.scale * props.padding);
const maxZoom = computed(() => Math.max(1, ...props.animations.flatMap(type => sheetUrls(type).map(getSpriteZoom))));
const canvasStyle = computed(() => ({
  width: `${boxWidth.value * maxZoom.value}px`,
  height: `${boxHeight.value * maxZoom.value}px`,
  ...(maxZoom.value > 1 && {
    display: 'inline-block',
    verticalAlign: 'top',
    margin: `${(-boxHeight.value * (maxZoom.value - 1)) / 2}px ${(-boxWidth.value * (maxZoom.value - 1)) / 2}px`,
    pointerEvents: 'none' as const,
  }),
}));

/* ===== Вспомогательная функция смены спрайта ===== */
// false — листа для такой анимации нет (например, смерть ещё не нарисована)
const updateActiveSprite = (type: SpriteAnimation): boolean => {
  const variants = sheets[type];
  if (!variants.length) return false;
  localAnimType.value = type;
  current = variants[Math.floor(Math.random() * variants.length)];
  dirty = true;
  return true;
};

/* ===== DRAW ===== */
function drawFrame(frame: number) {
  if (!ctx) return;
  const { width: canvasWidth, height: canvasHeight } = ctx.canvas;
  ctx.clearRect(0, 0, canvasWidth, canvasHeight);

  const image = current.image;
  const atlas = current.atlas;
  let drawn: Rect | null = null;
  if (image) {
    // Клетка кадра на экране — по центру канваса
    const width = cellPx(current.zoom);
    const height = (width * props.frameHeight) / props.frameWidth;
    const left = (canvasWidth - width) / 2;
    const top = (canvasHeight - height) / 2;
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = 'high';

    if (atlas) {
      // Кадр атласа — только сам персонаж; на экране он встаёт туда же, где лежал в своей клетке листа.
      // Координаты атласа переводятся в пиксели копии: её размер зависит от масштаба на экране
      const f = atlas.frames[frame];
      if (f) {
        const sx = sheetWidth(image) / atlas.width;
        const sy = sheetHeight(image) / atlas.height;
        const k = width / atlas.cell;
        drawn = [left + f[4] * k, top + f[5] * k, f[2] * k, f[3] * k];
        ctx.drawImage(image, f[0] * sx, f[1] * sy, f[2] * sx, f[3] * sy, ...drawn);
      }
    } else {
      const col = frame % props.columns;
      const row = Math.floor(frame / props.columns);
      // Кадр в пикселях листа — из самого листа: размер копии зависит от масштаба на экране
      const cellWidth = sheetWidth(image) / props.columns;
      const cellHeight = (cellWidth * props.frameHeight) / props.frameWidth;
      drawn = [left, top, width, height];
      ctx.drawImage(image, col * cellWidth, row * cellHeight, cellWidth, cellHeight, ...drawn);
    }
  }

  const now = performance.now();
  if (drawn && props.tint) {
    const pulse = props.tint.pulse ? 0.75 + 0.25 * Math.sin((now / 1000) * Math.PI * 2 * props.tint.pulse) : 1;
    overlay(props.tint.color, props.tint.alpha * pulse, drawn);
  }
  if (flashState) {
    const progress = (now - flashState.start) / flashState.duration;
    if (progress >= 1) flashState = null;
    else if (drawn) overlay(flashState.color, flashState.alpha * (1 - progress), drawn);
  }
}

/* ===== LOOP ===== */
function animate(time: number) {
  // Рассчитываем FPS внутри цикла.
  // Если нужно "2 удара в секунду", а в спрайте 10 кадров -> ставь fps 20
  // Статусы замедляют только idle: атака/победа доигрываются с обычной скоростью
  // Смерть доигрывается даже на паузе — пауза как раз и означает, что юнит мёртв
  const dying = localAnimType.value === 'death';
  const speed = dying ? 1 : props.paused ? 0 : localAnimType.value === 'idle' ? props.speed : 1;
  const frameDuration = 1000 / (props.fps || 10) / Math.max(speed, 0.0001);

  if (speed > 0 && time - lastTime >= frameDuration) {
    let nextFrame = currentFrame + 1;

    // ПРОВЕРКА ЗАВЕРШЕНИЯ
    if (nextFrame >= props.frameCount) {
      if (localAnimType.value === 'attack') {
        // Если проиграли атаку до конца — возвращаемся в idle
        updateActiveSprite('idle');
      }
      // Смерть замирает на последнем кадре, idle, ходьба и победа зацикливаются
      nextFrame = dying ? props.frameCount - 1 : 0;
    }

    currentFrame = nextFrame;
    // Ровный шаг: следующий кадр отсчитывается от момента, когда должен был смениться этот,
    // а не от того, когда экран успел обновиться — иначе кадры держатся то 5, то 6 обновлений экрана.
    // После долгой паузы (вкладка была скрыта) догонять не пытаемся
    lastTime = time - lastTime > frameDuration * 3 ? time : lastTime + frameDuration;
    dirty = true;
  }

  if (dirty || flashState || props.tint?.pulse) {
    drawFrame(currentFrame);
    dirty = false;
  }
  animationId = requestAnimationFrame(animate);
}

/* ===== LOAD IMAGES ===== */
onMounted(() => {
  ctx = canvasRef.value!.getContext("2d");
  window.addEventListener('resize', onResize);

  // Предзагрузка всех спрайтов
  loadSheets();

  updateActiveSprite('idle'); // Стартуем с idle
  // Уже мёртвый юнит сразу лежит на последнем кадре смерти
  if (props.paused && updateActiveSprite('death')) currentFrame = props.frameCount - 1;

  lastTime = performance.now();
  animationId = requestAnimationFrame(animate);
});

// Сменился размер на экране — канвас пересоздаёт буфер (и очищается), листы нужны другого размера
watch([dpr, () => props.scale], () => {
  allSheets().forEach(prepare);
  dirty = true;
}, { flush: 'post' });

watch(() => props.tint, () => {
  dirty = true;
}, { deep: true });

/* ===== WATCHER & METHODS ===== */
const playAnimation = (type: SpriteAnimation) => {
  // Нет листа (например, смерть ещё не нарисована) — остаёмся в текущей анимации
  if (!updateActiveSprite(type)) return;
  currentFrame = 0;
  lastTime = performance.now(); // Сброс таймера для мгновенного начала
};

// Юнит умер — проигрываем смерть, если она нарисована, иначе просто замираем (см. animate)
watch(() => props.paused, (paused) => {
  if (paused) playAnimation('death');
  else if (localAnimType.value === 'death') playAnimation('idle');
});

defineExpose({
  playAnimation,
  flash,
  ready,
});

// Следим за пропсом. Как только он меняется — сбрасываем кадр и ставим новую анимацию
watch(() => props.animationType, (newType) => {
  if (newType) {
    playAnimation(newType);
  }
});

onUnmounted(() => {
  cancelAnimationFrame(animationId);
  window.removeEventListener('resize', onResize);
  releaseSheets();
});
</script>

<template>
    <canvas
        ref="canvasRef"
        :width="Math.round(boxWidth * maxZoom * dpr)"
        :height="Math.round(boxHeight * maxZoom * dpr)"
        :style="canvasStyle"
    ></canvas>
</template>
