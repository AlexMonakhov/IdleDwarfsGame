import { ref } from 'vue'
import type { CombatResult } from '@/battle/types'

// Бой режима, у которого нет своего пути на арене: наёмник друга в кампании, повтор «Боя дня», бой глубинного спуска.
// Окно режима кладёт сюда, как получить бой и куда вернуться, и открывает /arena?external=1&attack=<метка>.
// Перезагрузка страницы теряет это состояние — арена тогда просто вернёт в город
export interface ExternalFight {
  // Подпись над ареной: «Глубинный спуск · этаж 3»
  title: string
  load: () => Promise<{ combat: CombatResult; background: string }>
  // Кнопка после боя и куда она ведёт (в город, затем — открыть окно режима)
  backLabel: string
  back: () => void
}

const pendingFight = ref<ExternalFight | null>(null)

export function useExternalFight() {
  return { pendingFight }
}
