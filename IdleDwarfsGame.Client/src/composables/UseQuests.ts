import { computed, ref } from 'vue'
import QuestsApiService from '@/services/apiServices/QuestsApiService'
import type { QuestBoard } from '@/models/types/Quests'

// Доска ежедневных заданий — общая на приложение: окно заданий её показывает, кнопка в шапке берёт из неё значок.
// Прогресс копится на сервере от любых действий, поэтому доску перечитываем раз в POLL_MS, пока игрок в игре
const POLL_MS = 60_000

const board = ref<QuestBoard | null>(null)
let timer = 0

const refresh = async () => {
  try {
    board.value = await QuestsApiService.get()
  } catch {
    // Значок просто не обновится, пока сервер недоступен
  }
}

export function useQuests() {
  const set = (value: QuestBoard) => {
    board.value = value
  }

  const start = () => {
    stop()
    refresh()
    timer = window.setInterval(refresh, POLL_MS)
  }

  const stop = () => {
    window.clearInterval(timer)
    timer = 0
  }

  const clear = () => {
    stop()
    board.value = null
  }

  return {
    // Только для чтения: доску меняют set и refresh
    board: computed(() => board.value),
    claimable: computed(() => board.value?.claimable ?? 0),
    refresh,
    set,
    start,
    stop,
    clear
  }
}
