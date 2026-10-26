import { computed, ref, watch } from 'vue'
import { useAuth } from '@/composables/UseAuth'
import { FRAME_EFFECTS, FrameEffect, FrameEffectInfo } from '@/models/types/Account'

// Выбранная рамка хранится по игроку: на одном браузере могут играть разные аккаунты
const STORAGE_KEY = 'idle-dwarfs-frame'

const { user } = useAuth()
const userId = computed(() => user.value?.id ?? null)

// Условия открытия рамок появятся позже — пока открыты все.
// Когда появятся, вернуть проверку: effect === 'none' || <условие для effect>
// eslint-disable-next-line @typescript-eslint/no-unused-vars
const isFrameUnlocked = (effect: FrameEffect): boolean => true

const readFrame = (id: string | null): FrameEffect => {
  if (!id) return 'none'
  try {
    const saved = localStorage.getItem(`${STORAGE_KEY}:${id}`) as FrameEffect | null
    // Рамка могла закрыться (или ключ устарел) — тогда возвращаемся к обычной
    return saved && FRAME_EFFECTS.includes(saved) && isFrameUnlocked(saved) ? saved : 'none'
  } catch {
    return 'none'
  }
}

// Состояние общее для всего приложения, как и в useAuth
const selectedFrame = ref<FrameEffect>(readFrame(userId.value))
watch(userId, id => { selectedFrame.value = readFrame(id) })

export function useAccount() {
  // Как считать уровень аккаунта, решим позже — пока у всех первый
  const level = computed(() => (user.value ? 1 : 0))

  const frames = computed<FrameEffectInfo[]>(() =>
    FRAME_EFFECTS.map(key => ({ key, unlocked: isFrameUnlocked(key) })))

  const selectFrame = (effect: FrameEffect) => {
    if (!userId.value || !isFrameUnlocked(effect)) return
    selectedFrame.value = effect
    try {
      localStorage.setItem(`${STORAGE_KEY}:${userId.value}`, effect)
    } catch {
      // localStorage недоступен — выбор проживёт до перезагрузки
    }
  }

  return {
    user,
    level,
    frames,
    selectedFrame,
    selectFrame
  }
}
