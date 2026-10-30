import { ref } from 'vue'

// Вкладки окна PvP: обычная арена и рейтинговая (ranked*)
export type PvpTab = 'fight' | 'seasons' | 'history' | 'ranked' | 'rankedSeasons' | 'rankedHistory'

export const isRankedTab = (tab: PvpTab) => tab.startsWith('ranked')

// Открыто ли окно PvP. Общее на приложение: арена после боя PvP возвращает игрока прямо в него
const pvpOpen = ref(false)
// Открытая вкладка — тоже общая: после боя на рейтинговой арене игрок возвращается на её вкладку
const pvpTab = ref<PvpTab>('fight')

export function usePvp() {
  return {
    pvpOpen,
    pvpTab
  }
}
