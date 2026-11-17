// Сценарии обучения — данные: компоненты о них не знают, только расставляют якоря (v-tutorial) и шлют события
// (tutorial/events.ts). План — docs/tutorial-plan.md

// Окна, которые обучение умеет узнавать и открывать само (tutorial/modals.ts). Окно отмечается для обучения
// пропсом tutorial-id у ModalComponent
export type TutorialModalId =
  | 'campaign'
  | 'tower'
  | 'mail'
  | 'collection'
  | 'warehouse'
  | 'altar'
  | 'forge'
  | 'shop'
  | 'mine'
  | 'expeditions'
  | 'quests'
  | 'event'
  | 'pvp'
  | 'guild'
  | 'friends'
  | 'delve'

// Когда сценарий запускается сам. 'manual' — только из кода (стартовое обучение, «?» в окне, песочница)
export type TutorialTrigger =
  | { modal: TutorialModalId, tab?: string }
  | 'manual'

// События, по которым шаг идёт дальше. Шлют места, где действие уже случилось (emitTutorialEvent)
export type TutorialEvent =
  | 'chest:opened'
  | 'chest:closed'
  | 'hero:upgraded'
  | 'expedition:claimed'
  | 'mine:collected'
  | 'fusion:ready'
  | 'fusion:done'
  | 'squad:saved'
  | 'squad:placed'

// Что сделать перед показом шага
export type TutorialAction =
  | { openModal: TutorialModalId }
  | { closeModal: TutorialModalId }
  | { selectTab: { modal: TutorialModalId, tab: string } }

export type TutorialPlacement = 'top' | 'bottom' | 'left' | 'right' | 'auto'

export interface TutorialStep {
  id: string
  // Якорь (v-tutorial); без него облачко стоит по центру экрана
  target?: string
  // Ключ i18n без префикса tutorial.: 'warehouse.tabs'
  text: string
  placement?: TutorialPlacement
  // 'next' — кнопка «Дальше», 'click-target' — клик по подсвеченному, { event } — событие из игры
  advance: 'next' | 'click-target' | { event: TutorialEvent }
  before?: TutorialAction[]
  // Клики мимо подсвеченного запрещены (по умолчанию — да: иначе игрок уйдёт в другое окно посреди знакомства).
  // На шаге «Дальше» подсвеченное только показывается — нажать в нём тоже нельзя
  block?: boolean
  // Подсвеченное только показывают, нажать в нём нельзя, и на любом шаге (отряд в пути — ждём, пока вернётся)
  passive?: boolean
  // Якорь может появиться не скоро (игрок на другой странице, идёт бой): ждать его молча, без облачка по центру
  wait?: boolean
  // Шаг без облачка: просто ждёт событие (идёт анимация сундука, таймер экспедиции)
  hidden?: boolean
  // Стрелки-указатели над каждым элементом якоря (под одним именем их может быть несколько: что взять и куда положить)
  pointer?: boolean
  // Показать не сразу, а через столько мс (дворф копает несколько секунд)
  delay?: number
  // Шаг уже не нужен (игрок уже это сделал или уже там, куда шаг звал) — сразу следующий
  skipIf?: (context: TutorialContext) => boolean
  // Пока шаг ждёт, появился этот якорь (любой из списка) — значит, шаг уже не нужен (окно шахты открылось вместо окна постройки)
  skipWhen?: string | string[]
  // Контрольная точка: обязательный сценарий после перезагрузки продолжается с последней пройденной
  checkpoint?: boolean
}

// Что знает раннер о прогрессе игрока — для skipIf
export interface TutorialContext {
  campaignCleared: number
  // Активности, которые игрок уже пробовал (сервер, TutorialActivities)
  done: string[]
}

export interface TutorialScenario {
  id: string
  trigger: TutorialTrigger
  // Кто первым, если на одном открытии сработало несколько сценариев
  priority?: number
  // Только стартовое обучение: не закрывается Esc, пропуск — с подтверждением
  mandatory?: boolean
  // Разовый сценарий (подсказка, пауза боя, песочница): его конец никуда не записывается
  transient?: boolean
  // Подпись кнопки пропуска (ключ без префикса tutorial.), по умолчанию «Пропустить»
  skipLabel?: string
  steps: TutorialStep[]
}

export type TutorialStatus = 'completed' | 'skipped'
