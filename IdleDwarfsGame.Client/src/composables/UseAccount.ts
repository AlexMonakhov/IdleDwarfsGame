import { computed, ref, watch } from 'vue'
import { useAuth } from '@/composables/UseAuth'
import { useRewards } from '@/composables/UseRewards'
import { FRAME_EFFECTS, FrameEffect, FrameEffectInfo } from '@/models/types/Account'

// Выбранная рамка хранится по игроку: на одном браузере могут играть разные аккаунты
const STORAGE_KEY = 'idle-dwarfs-frame'

const { user } = useAuth()
const userId = computed(() => user.value?.id ?? null)

// Рамки открываются достижениями (сервер: Services/Achievements.cs, рамка — у ступени достижения)
const { unlockedFrames } = useRewards()
const isFrameUnlocked = (effect: FrameEffect): boolean => effect === 'none' || unlockedFrames.value.includes(effect)

const readFrame = (id: string | null): FrameEffect => {
  if (!id) return 'none'
  try {
    const saved = localStorage.getItem(`${STORAGE_KEY}:${id}`) as FrameEffect | null
    return saved && FRAME_EFFECTS.includes(saved) ? saved : 'none'
  } catch {
    return 'none'
  }
}

// Состояние общее для всего приложения, как и в useAuth. Сохранённый выбор не сбрасываем, пока достижения
// не загрузились: показываем его, только когда рамка открыта, иначе — обычную
const savedFrame = ref<FrameEffect>(readFrame(userId.value))
watch(userId, id => { savedFrame.value = readFrame(id) })
const selectedFrame = computed<FrameEffect>(() => (isFrameUnlocked(savedFrame.value) ? savedFrame.value : 'none'))

export function useAccount() {
  // Как считать уровень аккаунта, решим позже — пока у всех первый
  const level = computed(() => (user.value ? 1 : 0))

  const frames = computed<FrameEffectInfo[]>(() =>
    FRAME_EFFECTS.map(key => ({ key, unlocked: isFrameUnlocked(key) })))

  const selectFrame = (effect: FrameEffect) => {
    if (!userId.value || !isFrameUnlocked(effect)) return
    savedFrame.value = effect
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
