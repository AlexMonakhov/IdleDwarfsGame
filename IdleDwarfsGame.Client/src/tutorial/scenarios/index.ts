import type { TutorialScenario } from '../types'
import { tour } from './tour'
import { sandbox } from './sandbox'

// Все сценарии обучения. Новый сценарий — файл рядом и строчка здесь (и в таблице «Что покрываем»
// в docs/tutorial-plan.md)
export const SCENARIOS: TutorialScenario[] = [
  tour,
  // Только для песочницы на /testpage
  sandbox,
]

export const findScenario = (id: string) => SCENARIOS.find(scenario => scenario.id === id)
