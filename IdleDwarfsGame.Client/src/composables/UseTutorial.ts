import { computed, nextTick, ref, shallowReactive, shallowRef, watch } from 'vue'
import { useAuth } from '@/composables/UseAuth'
import TutorialApiService, { type TutorialProgress } from '@/services/apiServices/TutorialApiService'
import { SCENARIOS, findScenario } from '@/tutorial/scenarios'
import { ONBOARDING_ID, onboarding } from '@/tutorial/scenarios/onboarding'
import { TOUR_ID, tour } from '@/tutorial/scenarios/tour'
import { MODAL_FLAGS } from '@/tutorial/modals'
import { ACTIVITIES, activityByModal, type TutorialActivity } from '@/tutorial/activities'
import { findAnchor, isInAnchor } from '@/tutorial/anchors'
import { onTutorialEvent } from '@/tutorial/events'
import type { TutorialAction, TutorialModalId, TutorialScenario, TutorialStatus, TutorialStep } from '@/tutorial/types'

// Раннер обучения: какие сценарии пройдены, какой идёт сейчас и на каком шаге, какие активности игрок уже знает.
// Сценарии — данные (tutorial/scenarios), активности — tutorial/activities.ts, показывает шаг TutorialOverlay.
// Прогресс хранит сервер (PlayerTutorial); пока он не загружен, знакомства сами не запускаются — иначе опытному
// игроку они показались бы заново.
// Стартовое обучение (onboarding) идёт само, пока сервер считает его активным, и продолжается с последней
// контрольной точки после перезагрузки. Паузы обучающего боя показываются поверх него (interrupt), не прерывая
interface ActiveScenario {
  scenario: TutorialScenario
  stepIndex: number
}

const { user, isAuthenticated } = useAuth()

const progress = ref<TutorialProgress | null>(null)
const active = shallowRef<ActiveScenario | null>(null)
// Разовый шаг поверх сценария (пауза боя): пока он есть, показывается он, а сценарий под ним ждёт
const interrupt = shallowRef<ActiveScenario | null>(null)

let loadVersion = 0
const load = async () => {
  const version = ++loadVersion
  if (!isAuthenticated.value) {
    progress.value = null
    return
  }
  try {
    const loaded = await TutorialApiService.get()
    if (version === loadVersion) progress.value = loaded
  } catch {
    // Не загрузилось — обучение молчит до следующей попытки (вход, возврат в город)
  }
}

watch(() => user.value?.id, () => {
  active.value = null
  interrupt.value = null
  progress.value = null
  load()
}, { immediate: true })

// Статус пишем сразу у себя, а ответ сервера (весь прогресс) его подтверждает. Не дошло — проживёт до перезагрузки
const saveStatus = async (key: string, status: TutorialStatus) => {
  if (!progress.value) return
  progress.value = { ...progress.value, scenarios: { ...progress.value.scenarios, [key]: status } }
  try {
    progress.value = await TutorialApiService.setStatus(key, status)
  } catch {
    // см. выше
  }
}

const onboardingActive = () => !!progress.value?.onboarding.active

/* ===== Окна ===== */

// Открытые окна, отмеченные tutorial-id, и их вкладки — по ним срабатывают триггеры
const openModals = shallowReactive(new Map<TutorialModalId, string | undefined>())

interface ModalControl {
  selectTab: (tab: string) => void
}

const modalControls = new Map<TutorialModalId, ModalControl>()

const runAction = (action: TutorialAction) => {
  if ('openModal' in action) MODAL_FLAGS[action.openModal].value = true
  else if ('closeModal' in action) MODAL_FLAGS[action.closeModal].value = false
  else modalControls.get(action.selectTab.modal)?.selectTab(action.selectTab.tab)
}

/* ===== Активности ===== */

// Познакомился: сценарий активности пройден или пропущен (без сценария — открыл окно)
const isSeen = (activity: TutorialActivity) => !!progress.value?.scenarios[activity.id]

// «!» на кнопке активности: только когда прогресс известен, иначе значки мигнули бы у всех.
// Пока идёт стартовое обучение, значков нет: дорогу показывает наставник
const needsBadge = (activity: TutorialActivity | undefined) =>
  !!activity && !!progress.value && !onboardingActive() && !isSeen(activity)

/* ===== Сценарий ===== */

// Что показано сейчас: пауза поверх сценария или сам сценарий
const shown = computed(() => interrupt.value ?? active.value)

const currentStep = computed(() => {
  const current = shown.value
  return current ? current.scenario.steps[current.stepIndex] ?? null : null
})

const begin = (scenario: TutorialScenario, stepIndex = 0) => {
  active.value = { scenario, stepIndex: Math.min(Math.max(stepIndex, 0), scenario.steps.length - 1) }
}

// Пауза ждёт код (бой): чем она кончилась — «Дальше» или «Пропустить»
let pending: { scenario: TutorialScenario, resolve: (status: TutorialStatus) => void } | null = null

const settlePending = (status: TutorialStatus) => {
  const current = pending
  pending = null
  current?.resolve(status)
}

const finish = async (status: TutorialStatus) => {
  if (interrupt.value) {
    interrupt.value = null
    settlePending(status)
    return
  }
  const current = active.value
  if (!current) return
  active.value = null
  if (current.scenario.id === ONBOARDING_ID) {
    await finishOnboarding(status)
    // Прошёл — сразу обзор остальных зданий
    if (status === 'completed' && !active.value) begin(tour)
  } else if (!current.scenario.transient) saveStatus(current.scenario.id, status)
  // Пока шёл этот сценарий, могло открыться окно со своим знакомством — оно ждало в очереди
  nextTick(checkTriggers)
}

// Прервать без отметки: окно сценария закрыли — знакомство начнётся заново при следующем открытии
const abort = () => {
  active.value = null
}

const next = () => {
  if (interrupt.value) {
    finish('completed')
    return
  }
  const current = active.value
  if (!current) return
  if (current.stepIndex + 1 >= current.scenario.steps.length) finish('completed')
  else active.value = { ...current, stepIndex: current.stepIndex + 1 }
}

const skip = () => finish('skipped')

function checkTriggers() {
  if (active.value || !progress.value) return
  // Стартовое обучение — первым и вместо знакомств
  if (onboardingActive()) {
    begin(onboarding, resumeIndex(progress.value.onboarding.step))
    return
  }
  const statuses = progress.value.scenarios
  const candidates = SCENARIOS.filter(scenario => {
    if (scenario.trigger === 'manual' || statuses[scenario.id]) return false
    const { modal, tab } = scenario.trigger
    return openModals.has(modal) && (tab === undefined || openModals.get(modal) === tab)
  })
  if (!candidates.length) return
  candidates.sort((a, b) => (b.priority ?? 0) - (a.priority ?? 0))
  begin(candidates[0])
}

// Обзор города ждёт: стартовое обучение позади (у старых аккаунтов его нет — им и обзор не нужен), а обзор не пройден
// и не пропущен — бросил на середине или сбросил обучение в аккаунте
const tourPending = () => {
  const statuses = progress.value?.scenarios
  return !!statuses && !onboardingActive() && !!statuses[ONBOARDING_ID] && !statuses[TOUR_ID]
}

// Прогресс загрузился (вход, перезагрузка) — стартовое обучение, обзор или знакомство открытого окна начинается сейчас
watch(progress, (loaded, previous) => {
  if (!loaded || previous) return
  if (!active.value && tourPending()) begin(tour)
  else checkTriggers()
})

/* ===== Стартовое обучение ===== */

// Сервер хранит номер шага последней пройденной контрольной точки; с неё и продолжаем
const resumeIndex = (saved: number) => {
  const steps = onboarding.steps
  for (let i = Math.min(saved, steps.length - 1); i > 0; i--) {
    if (steps[i].checkpoint) return i
  }
  return 0
}

// Дошли до контрольной точки — запомнить на сервере
watch(active, current => {
  if (current?.scenario.id !== ONBOARDING_ID || !current.scenario.steps[current.stepIndex].checkpoint) return
  if (progress.value) progress.value = { ...progress.value, onboarding: { ...progress.value.onboarding, step: current.stepIndex } }
  TutorialApiService.onboardingStep(current.stepIndex).catch(() => undefined)
})

// Пропуск — сервер выдаёт всё, что обучение выдало бы по дороге; конец — отметка «пройдено»
const finishOnboarding = async (status: TutorialStatus) => {
  if (progress.value) progress.value = { ...progress.value, onboarding: { active: false, step: 0 } }
  try {
    // Пропустил — обучение ему не нужно: все окна знакомы, ни знакомств, ни «!»
    progress.value = status === 'skipped'
      ? await TutorialApiService.skipOnboarding([...ACTIVITIES.map(activity => activity.id), TOUR_ID])
      : await TutorialApiService.completeOnboarding()
  } catch {
    // Не дошло — при следующем входе обучение продолжится, пропуск можно повторить
  }
  onOnboardingFinished.forEach(listener => listener(status))
}

// Кто перечитывает ресурсы и героев после пропуска (InterfaceView): сервер выдал всё сразу
const onOnboardingFinished = new Set<(status: TutorialStatus) => void>()

/* ===== Шаги ===== */

// Перед шагом — его действия (открыть окно, вкладку). Порядок важен: окно должно успеть
// зарегистрировать управление вкладками, поэтому вкладку переключаем после nextTick.
// Шаг, который уже не нужен (skipIf), сразу уступает следующему
const context = () => ({ campaignCleared: progress.value?.campaignCleared ?? 0, done: progress.value?.done ?? [] })

watch(currentStep, async step => {
  if (!step) return
  if (step.skipIf?.(context())) {
    next()
    return
  }
  for (const action of step.before ?? []) {
    runAction(action)
    await nextTick()
  }
})

// Шаг уже не нужен, как только появился его skipWhen
watch(currentStep, (step, _, onCleanup) => {
  const anchor = step?.skipWhen
  if (!anchor) return
  const timer = window.setInterval(() => {
    if (currentStep.value === step && findAnchor(anchor)) next()
  }, 200)
  onCleanup(() => window.clearInterval(timer))
})

// Шаг ждёт событие из игры
watch(currentStep, (step, _, onCleanup) => {
  const advance = step?.advance
  if (!advance || typeof advance === 'string') return
  const off = onTutorialEvent(event => {
    if (event === advance.event && currentStep.value === step) next()
  })
  onCleanup(off)
})

// Шаг ждёт клик по подсвеченному. Слушаем на захвате, а дальше идём после самого клика —
// чтобы сначала сработал обработчик элемента (переключение вкладки)
watch(currentStep, (step, _, onCleanup) => {
  if (step?.advance !== 'click-target' || !step.target) return
  const target = step.target
  const onClick = (event: MouseEvent) => {
    if (!isInAnchor(target, event.target as Node)) return
    setTimeout(() => { if (currentStep.value === step) next() })
  }
  window.addEventListener('click', onClick, true)
  onCleanup(() => window.removeEventListener('click', onClick, true))
})

export function useTutorial() {
  // Сбросить: все сценарии снова не пройдены, знакомства и «!» вернутся. Отметки «попробовал» сервер сохраняет,
  // стартовое обучение сбросом не начинается заново
  const reset = async () => {
    if (active.value?.scenario.id !== ONBOARDING_ID) active.value = null
    try {
      progress.value = await TutorialApiService.reset()
    } catch {
      if (progress.value) progress.value = { ...progress.value, scenarios: {} }
    }
  }

  // Начать заново: сброс и сразу обзор города (после сброса он и так начнётся при следующем входе)
  const restart = async () => {
    await reset()
    begin(tour)
  }

  // Запустить сценарий вручную (кнопка «?», песочница), можно с нужного шага. Идущий сценарий прерывается
  const start = (id: string, stepIndex = 0) => {
    const scenario = id === ONBOARDING_ID ? onboarding : findScenario(id)
    if (!scenario) return
    begin(scenario, stepIndex)
  }

  // Показать один шаг поверх идущего сценария и дождаться его конца (паузы обучающего боя)
  const pause = (step: TutorialStep) => new Promise<TutorialStatus>(resolve => {
    settlePending('skipped')
    const scenario: TutorialScenario = { id: `pause.${step.id}`, trigger: 'manual', transient: true, steps: [step] }
    interrupt.value = { scenario, stepIndex: 0 }
    pending = { scenario, resolve }
  })

  // Убрать паузу (ушли с арены посреди неё)
  const cancelPause = () => {
    interrupt.value = null
    settlePending('skipped')
  }

  // Окно с tutorial-id сообщает, что открылось, закрылось или сменило вкладку
  const notifyModal = (id: TutorialModalId, open: boolean, tab?: string) => {
    if (open) {
      openModals.set(id, tab)
      // Активность без сценария знакомства считается знакомой с первого открытия — «!» снимается
      const activity = activityByModal(id)
      if (activity && progress.value && !isSeen(activity) && !findScenario(activity.id)) saveStatus(activity.id, 'completed')
      checkTriggers()
      return
    }
    openModals.delete(id)
    const trigger = active.value?.scenario.trigger
    if (trigger && trigger !== 'manual' && trigger.modal === id) abort()
  }

  const registerModalControl = (id: TutorialModalId, control: ModalControl) => {
    modalControls.set(id, control)
    return () => {
      if (modalControls.get(id) === control) modalControls.delete(id)
    }
  }

  const onFinished = (listener: (status: TutorialStatus) => void) => {
    onOnboardingFinished.add(listener)
    return () => { onOnboardingFinished.delete(listener) }
  }

  return {
    scenarios: [onboarding, ...SCENARIOS],
    statuses: computed(() => progress.value?.scenarios ?? {}),
    progress: computed(() => progress.value),
    // Что показано: пауза поверх сценария или сам сценарий
    active: shown,
    currentStep,
    onboardingActive: computed(onboardingActive),
    next,
    skip,
    start,
    pause,
    cancelPause,
    reset,
    restart,
    refresh: load,
    needsBadge,
    notifyModal,
    registerModalControl,
    onOnboardingFinished: onFinished
  }
}
