<script lang="ts" setup>    
    import { ref, onMounted, onUnmounted, watch, defineProps, withDefaults } from "vue";
    import { HeroModel } from "@/models/hero";

const props = withDefaults(defineProps<{
  model: HeroModel;
  animationType?: 'idle' | 'attack' | 'victory';
  frameWidth: number;
  frameHeight: number;
  frameCount: number;
  columns: number;
  fps?: number; 
  scale?: number;
  padding?: number;
}>(), {
  animationType: 'idle',
  padding: 1,
  fps: 10,
  scale: 1,
  frameCount: 25,
});

/* ===== STATE ===== */
// Внутреннее состояние анимации (то, что мы реально видим)
const localAnimType = ref<'idle' | 'attack' | 'victory'>('idle');

const canvasRef = ref<HTMLCanvasElement | null>(null);

let ctx: CanvasRenderingContext2D | null = null;
let currentImage = new Image(); // Текущий спрайт-лист
let spriteIdle = new Image();
let spriteAttack = new Image();
let spriteVictory = new Image();

let currentFrame = 0;
let lastTime = 0;
let animationId = 0;

/* ===== Вспомогательная функция смены спрайта ===== */
const updateActiveSprite = (type: 'idle' | 'attack' | 'victory') => {
  localAnimType.value = type;
  switch (type) {
    case 'idle': currentImage = spriteIdle; break;
    case 'attack': currentImage = spriteAttack; break;
    case 'victory': currentImage = spriteVictory; break;
  }
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
}

/* ===== LOOP ===== */
function animate(time: number) {
  // Рассчитываем FPS внутри цикла. 
  // Если нужно "2 удара в секунду", а в спрайте 10 кадров -> ставь fps 20
  const frameDuration = 1000 / (props.fps || 10);

  if (time - lastTime >= frameDuration) {
    let nextFrame = currentFrame + 1;

    // ПРОВЕРКА ЗАВЕРШЕНИЯ
    if (nextFrame >= props.frameCount) {
      if (localAnimType.value !== 'idle') {
        // Если проиграли атаку/победу до конца — возвращаемся в idle
        updateActiveSprite('idle');
        nextFrame = 0;
      } else {
        // Если это был idle — просто зацикливаем
        nextFrame = 0;
      }
    }

    currentFrame = nextFrame;
    lastTime = time;
  }

  drawFrame(currentFrame);
  animationId = requestAnimationFrame(animate);
}

/* ===== LOAD IMAGES ===== */
const requireContext = require.context('../assets/sprite-lists', true, /\.png$/);
const getSpriteUrl = (animType: string, color: string, level: number, frame = 1) => {
  const path = `./dwarf-${animType}/dwarf_${animType}_lvl${level}_${color}_${frame}.png`;
  try { return requireContext(path); } catch (e) { return ''; }
};

onMounted(() => {
  ctx = canvasRef.value!.getContext("2d");
  
  // Предзагрузка всех спрайтов
  spriteIdle.src = getSpriteUrl('idle', props.model.color, props.model.lvl);
  spriteAttack.src = getSpriteUrl('attack', props.model.color, props.model.lvl, 2);
  spriteVictory.src = getSpriteUrl('victory', props.model.color, props.model.lvl);

  updateActiveSprite('idle'); // Стартуем с idle
  
  spriteIdle.onload = () => {
    animationId = requestAnimationFrame(animate);
  };
});

/* ===== WATCHER & METHODS ===== */
const playAnimation = (type: 'idle' | 'attack' | 'victory') => {
  currentFrame = 0;
  lastTime = performance.now(); // Сброс таймера для мгновенного начала
  updateActiveSprite(type);
};

defineExpose({
  playAnimation
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