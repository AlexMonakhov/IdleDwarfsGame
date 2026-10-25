<script lang="ts" setup>    
    import { ref, onMounted, onUnmounted, watch, defineProps, withDefaults } from "vue";
    import { HeroModel } from "@/models/hero";
    import { SpriteTint } from "@/battle/FxRegistry";
    import { getSpriteVariants, SpriteAnimation } from "@/battle/sprites";

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
}>(), {
  animationType: 'idle',
  padding: 1,
  fps: 10,
  scale: 1,
  frameCount: 25,
  tint: null,
  speed: 1,
  paused: false,
});

// Короткая вспышка силуэта (получение урона, удар, лечение)
let flashState: { color: string; alpha: number; start: number; duration: number } | null = null;

const flash = (color: string, alpha = 0.6, duration = 350) => {
  flashState = { color, alpha, start: performance.now(), duration };
};

// Закрасить только непрозрачные пиксели спрайта
function overlay(color: string, alpha: number) {
  if (!ctx || alpha <= 0) return;
  ctx.save();
  ctx.globalCompositeOperation = 'source-atop';
  ctx.globalAlpha = Math.min(alpha, 1);
  ctx.fillStyle = color;
  ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);
  ctx.restore();
}

/* ===== STATE ===== */
// Внутреннее состояние анимации (то, что мы реально видим)
const localAnimType = ref<SpriteAnimation>('idle');

const canvasRef = ref<HTMLCanvasElement | null>(null);

let ctx: CanvasRenderingContext2D | null = null;
let currentImage = new Image(); // Текущий спрайт-лист
// Все варианты листов каждой анимации (_1, _2, ...): при каждом запуске анимации берётся случайный
const sheets: Record<SpriteAnimation, HTMLImageElement[]> = { idle: [], attack: [], victory: [], death: [] };

let currentFrame = 0;
let lastTime = 0;
let animationId = 0;

const loadSheets = () => {
  (Object.keys(sheets) as SpriteAnimation[]).forEach(type => {
    sheets[type] = getSpriteVariants(type, props.model.color, props.model.lvl, props.model.unitType).map(src => {
      const image = new Image();
      image.src = src;
      return image;
    });
  });
};

/* ===== Вспомогательная функция смены спрайта ===== */
// false — листа для такой анимации нет (например, смерть ещё не нарисована)
const updateActiveSprite = (type: SpriteAnimation): boolean => {
  const variants = sheets[type];
  if (!variants.length) return false;
  localAnimType.value = type;
  currentImage = variants[Math.floor(Math.random() * variants.length)];
  return true;
};

/* ===== DRAW ===== */
function drawFrame(frame: number) {
  if (!ctx) return;
  const col = frame % props.columns;
  const row = Math.floor(frame / props.columns);

  ctx.clearRect(0, 0, props.frameWidth * props.scale * props.padding, props.frameHeight * props.scale * props.padding);
  ctx.drawImage(
    currentImage, // Используем внутреннюю переменную
    col * props.frameWidth,
    row * props.frameHeight,
    props.frameWidth,
    props.frameHeight,
    (props.frameWidth * props.scale * props.padding - props.frameWidth * props.scale) / 2,
    (props.frameHeight * props.scale * props.padding - props.frameHeight * props.scale) / 2,
    props.frameWidth * props.scale,
    props.frameHeight * props.scale
  );

  const now = performance.now();
  if (props.tint) {
    const pulse = props.tint.pulse ? 0.75 + 0.25 * Math.sin((now / 1000) * Math.PI * 2 * props.tint.pulse) : 1;
    overlay(props.tint.color, props.tint.alpha * pulse);
  }
  if (flashState) {
    const progress = (now - flashState.start) / flashState.duration;
    if (progress >= 1) flashState = null;
    else overlay(flashState.color, flashState.alpha * (1 - progress));
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
      // Смерть замирает на последнем кадре, idle и победа зацикливаются
      nextFrame = dying ? props.frameCount - 1 : 0;
    }

    currentFrame = nextFrame;
    lastTime = time;
  }

  drawFrame(currentFrame);
  animationId = requestAnimationFrame(animate);
}

/* ===== LOAD IMAGES ===== */
onMounted(() => {
  ctx = canvasRef.value!.getContext("2d");
  
  // Предзагрузка всех спрайтов
  loadSheets();

  updateActiveSprite('idle'); // Стартуем с idle
  // Уже мёртвый юнит сразу лежит на последнем кадре смерти
  if (props.paused && updateActiveSprite('death')) currentFrame = props.frameCount - 1;

  const firstSheet = sheets[localAnimType.value][0];
  if (firstSheet) {
    firstSheet.onload = () => {
      animationId = requestAnimationFrame(animate);
    };
  }
});

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
});

// Следим за пропсом. Как только он меняется — сбрасываем кадр и ставим новую анимацию
watch(() => props.animationType, (newType) => {
  if (newType) {
    playAnimation(newType);
  }
});

onUnmounted(() => cancelAnimationFrame(animationId));
</script>

<template> 
    <canvas   
        ref="canvasRef"  
        :width="props.frameWidth * props.scale * props.padding"
        :height="props.frameHeight * props.scale * props.padding"
    ></canvas>
</template>