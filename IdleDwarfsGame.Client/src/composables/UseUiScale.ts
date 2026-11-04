import { ref } from 'vue'

// Игра свёрстана под окно около 1920×1000 (1080p за вычетом панелей браузера): размеры юнитов, кнопок и модалок
// заданы в пикселях. Чтобы на другом экране всё выглядело так же, только мельче или крупнее, приложение рисуется
// в «логическом» экране (--stage-w × --stage-h, примерно 1920×1000) и целиком масштабируется на --ui-scale
// (классы .ui-stage в App.vue и оверлей ModalComponent). Внутри сцены вместо vw/vh — var(--vw)/var(--vh).
export const DESIGN_WIDTH = 1920
export const DESIGN_HEIGHT = 1000
const MIN_SCALE = 0.5
const MAX_SCALE = 1.5

export const uiScale = ref(1)

const compute = () => {
  const scale = Math.min(window.innerWidth / DESIGN_WIDTH, window.innerHeight / DESIGN_HEIGHT)
  return Math.min(Math.max(scale, MIN_SCALE), MAX_SCALE)
}

const apply = () => {
  // Округление убирает дрожание субпикселей при плавном ресайзе окна
  uiScale.value = Math.round(compute() * 1000) / 1000
  document.documentElement.style.setProperty('--ui-scale', String(uiScale.value))
}

export const installUiScale = () => {
  apply()
  window.addEventListener('resize', apply)
}
