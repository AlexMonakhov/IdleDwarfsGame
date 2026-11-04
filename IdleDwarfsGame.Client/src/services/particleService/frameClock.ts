// Время кадра для эффектов. Скорости, затухания и вероятности спавна в стратегиях подобраны «на кадр при 60 Гц»,
// а requestAnimationFrame идёт с частотой экрана: на 120 Гц (ProMotion) частицы летели бы и гасли вдвое быстрее,
// а при скачках FPS их скорость прыгала бы, хотя полёт снарядов и прогресс заклинаний считаются по времени.
// Поэтому всё «покадровое» умножается на frameScale — сколько кадров по 60 Гц прошло за этот кадр.

const BASE_FRAME_MS = 1000 / 60;
// После паузы (вкладка в фоне, долгий кадр) не прыгаем на много шагов вперёд
const MAX_SCALE = 3;

let lastFrameAt = 0;
export let frameScale = 1;

// Вызывается в начале кадра каждого сервиса частиц с меткой времени requestAnimationFrame. Она общая для всех
// колбэков одного кадра, так что второй слой частиц (модалка поверх арены) не обнулит шаг первому
export const tickFrameClock = (now: number) => {
  if (now === lastFrameAt) return;
  frameScale = lastFrameAt ? Math.min(Math.max((now - lastFrameAt) / BASE_FRAME_MS, 0), MAX_SCALE) : 1;
  lastFrameAt = now;
};

// Множитель «за кадр» (сопротивление, усадка), приведённый к длине текущего кадра
export const perFrameFactor = (factor: number) => (frameScale === 1 ? factor : Math.pow(factor, frameScale));

// Вероятность события «за кадр при 60 Гц», приведённая к длине текущего кадра
export const perFrameChance = (chance: number) => Math.random() < 1 - Math.pow(1 - Math.min(Math.max(chance, 0), 1), frameScale);

// Сколько штук заспавнить за этот кадр при rate штук за кадр 60 Гц (дробная часть — с вероятностью)
export const perFrameCount = (rate: number) => {
  const scaled = rate * frameScale;
  return Math.floor(scaled) + (Math.random() < scaled % 1 ? 1 : 0);
};
