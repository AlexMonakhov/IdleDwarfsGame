import { computed, ref } from 'vue'
import AchievementsApiService from '@/services/apiServices/AchievementsApiService'
import type { AchievementBoard, LoginCalendarInfo } from '@/models/types/Achievements'

// Достижения и календарь входов — общие на приложение: окно наград их показывает, кнопка в шапке берёт значок,
// окно аккаунта — открытые рамки. Прогресс достижений копится на сервере от любых действий, поэтому, пока игрок
// в игре, перечитываем раз в POLL_MS (как доску заданий).
// TODO(SignalR): когда появится хаб — пуш AchievementsChanged из ActivityTracker вместо опроса, опрос оставить
// запасным, пока hubStatus !== 'connected' (см. UseQuests в стеше с хабом)
const POLL_MS = 60_000

const board = ref<AchievementBoard | null>(null)
const calendar = ref<LoginCalendarInfo | null>(null)
let timer = 0

const refreshAchievements = async () => {
  try {
    board.value = await AchievementsApiService.get()
  } catch {
    // Значок просто не обновится, пока сервер недоступен
  }
}

const refreshCalendar = async () => {
  try {
    calendar.value = await AchievementsApiService.getCalendar()
  } catch {
    // см. выше
  }
}

const refresh = () => Promise.all([refreshAchievements(), refreshCalendar()])

export function useRewards() {
  const setBoard = (value: AchievementBoard) => {
    board.value = value
  }

  const setCalendar = (value: LoginCalendarInfo) => {
    calendar.value = value
  }

  const start = async () => {
    stop()
    timer = window.setInterval(refresh, POLL_MS)
    await refresh()
  }

  const stop = () => {
    window.clearInterval(timer)
    timer = 0
  }

  const clear = () => {
    stop()
    board.value = null
    calendar.value = null
  }

  const calendarClaimable = computed(() => !!calendar.value && !calendar.value.claimedToday)
  const achievementsClaimable = computed(() => board.value?.claimable ?? 0)

  return {
    // Только для чтения: меняют set* и refresh
    board: computed(() => board.value),
    calendar: computed(() => calendar.value),
    calendarClaimable,
    achievementsClaimable,
    // Значок на кнопке: готовые ступени достижений и сегодняшняя награда календаря
    claimable: computed(() => achievementsClaimable.value + (calendarClaimable.value ? 1 : 0)),
    unlockedFrames: computed(() => board.value?.unlockedFrames ?? []),
    refresh,
    refreshAchievements,
    refreshCalendar,
    setBoard,
    setCalendar,
    start,
    stop,
    clear
  }
}
