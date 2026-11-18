import { ref } from 'vue'

// Игра свёрстана под окно около 1920×1000 (1080p за вычетом панелей браузера): размеры юнитов, кнопок и модалок
// заданы в пикселях. Чтобы на другом экране всё выглядело так же, только мельче или крупнее, приложение рисуется
// в «логическом» экране (--stage-w × --stage-h, примерно 1920×1000) и целиком масштабируется на --ui-scale
// (классы .ui-stage в App.vue и оверлей ModalComponent). Внутри сцены вместо vw/vh — var(--vw)/var(--vh).
// Масштаб только уменьшает: на экране больше дизайнерского интерфейс остаётся в натуральную величину,
// а лишнее место достаётся сцене — иначе на 1440p/4K модалки и кнопки раздувались бы
export const DESIGN_WIDTH = 1920
export const DESIGN_HEIGHT = 1000
const MIN_SCALE = 0.5
const MAX_SCALE = 1

// Меню (InterfaceView) живёт вне сцены и масштабируется само (--hud-scale). На компьютере — как сцена,
// но не мельче HUD_MIN_SCALE: на ноутбуке подписи кнопок иначе уходили в 10px.
// На телефоне — в настоящих пикселях: сцена там упирается в MIN_SCALE, и кнопки становились нечитаемыми
const HUD_MIN_SCALE = 0.75

// Раскладка меню: ряд сверху (компьютер) или столбец слева (телефон, узкое или низкое окно).
// Вертикальный экран телефона не поддерживаем — арену под него не уложить, просим повернуть (RotateDevicePrompt)
export type HudLayout = 'desktop' | 'landscape'

// Узкое или низкое окно — телефон или планшет: меню перестраивается под палец
const COMPACT_MAX_WIDTH = 1000
const COMPACT_MAX_HEIGHT = 560

export const uiScale = ref(1)
export const hudScale = ref(1)
export const hudLayout = ref<HudLayout>('desktop')
// Логический размер сцены. Меньше дизайнерского (1920×1000) он бывает, только когда масштаб упёрся в MIN_SCALE
// (телефон): тогда сцена ниже, чем рассчитана вёрстка, и арена уменьшает бойцов (ArenaView)
export const stageSize = ref({ width: DESIGN_WIDTH, height: DESIGN_HEIGHT })
// Сенсорное устройство держат вертикально — показываем просьбу повернуть. Мышь не в счёт: узкое окно
// на компьютере просто получает раскладку телефона
export const rotateRequired = ref(false)

const coarsePointer = window.matchMedia?.('(pointer: coarse)')

const apply = () => {
  const width = window.innerWidth
  const height = window.innerHeight
  const layout: HudLayout = width >= COMPACT_MAX_WIDTH && height >= COMPACT_MAX_HEIGHT ? 'desktop' : 'landscape'
  // Меню висит поверх сцены и места у неё не забирает: сцене достаётся всё окно
  const stageW = width
  const stageH = height
  const scale = Math.min(Math.max(Math.min(stageW / DESIGN_WIDTH, stageH / DESIGN_HEIGHT), MIN_SCALE), MAX_SCALE)
  // Округление убирает дрожание субпикселей при плавном ресайзе окна
  uiScale.value = Math.round(scale * 1000) / 1000
  hudScale.value = layout === 'desktop' ? Math.max(uiScale.value, HUD_MIN_SCALE) : 1
  hudLayout.value = layout
  rotateRequired.value = !!coarsePointer?.matches && height > width

  const root = document.documentElement.style
  root.setProperty('--ui-scale', String(uiScale.value))
  root.setProperty('--hud-scale', String(hudScale.value))
  // Размер логического экрана для сцены и оверлея окон. innerWidth/innerHeight, а не 100vw/100vh:
  // на телефоне 100vh включает место под адресной строкой браузера
  stageSize.value = { width: stageW / uiScale.value, height: stageH / uiScale.value }
  root.setProperty('--stage-w', `${stageW / uiScale.value}px`)
  root.setProperty('--stage-h', `${stageH / uiScale.value}px`)
}

export const installUiScale = () => {
  apply()
  window.addEventListener('resize', apply)
  // Поворот телефона: resize приходит не во всех браузерах, а сенсорность может смениться (планшет с клавиатурой)
  window.addEventListener('orientationchange', apply)
  coarsePointer?.addEventListener?.('change', apply)
}
