import { computed, ref, watch } from 'vue'
import { useAuth } from '@/composables/UseAuth'

// Прогресс обучения игрока. Самого обучения ещё нет: здесь только состояние, которое оно будет читать —
// пройдено ли, на каком шаге, и идёт ли прямо сейчас
const STORAGE_KEY = 'idle-dwarfs-tutorial'

interface TutorialState {
  completed: boolean
  step: number
}

const { user } = useAuth()
const userId = computed(() => user.value?.id ?? 'guest')

const initialState = (): TutorialState => ({ completed: false, step: 0 })

const readState = (id: string): TutorialState => {
  try {
    const saved = JSON.parse(localStorage.getItem(`${STORAGE_KEY}:${id}`) ?? 'null')
    return { ...initialState(), ...saved }
  } catch {
    return initialState()
  }
}

const state = ref<TutorialState>(readState(userId.value))
// Обучение показывается сейчас. Не сохраняется: после перезагрузки его запустит само обучение по state
const active = ref(false)

watch(userId, id => {
  state.value = readState(id)
  active.value = false
})

const save = () => {
  try {
    localStorage.setItem(`${STORAGE_KEY}:${userId.value}`, JSON.stringify(state.value))
  } catch {
    // localStorage недоступен — прогресс проживёт до перезагрузки
  }
}

export function useTutorial() {
  // Сбросить: обучение начнётся с начала при следующем входе в игру
  const reset = () => {
    state.value = initialState()
    active.value = false
    save()
  }

  // Начать заново: сброс и сразу запуск
  const restart = () => {
    reset()
    active.value = true
  }

  return {
    completed: computed(() => state.value.completed),
    step: computed(() => state.value.step),
    active,
    reset,
    restart
  }
}
