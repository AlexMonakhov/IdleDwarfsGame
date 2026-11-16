import type { TutorialScenario } from '../types'

// Проверка движка на /testpage без входа в игру: облачко по центру, вырез, клик по подсвеченному,
// блокирующий шаг и якорь, которого нет (через 3 с облачко встаёт по центру)
export const sandbox: TutorialScenario = {
  id: 'sandbox',
  trigger: 'manual',
  // Прогон песочницы не пишется в прогресс игрока
  transient: true,
  steps: [
    { id: 'center', text: 'sandbox.center', advance: 'next' },
    { id: 'panel', target: 'sandbox.panel', text: 'sandbox.panel', placement: 'bottom', advance: 'next' },
    { id: 'click', target: 'sandbox.button', text: 'sandbox.click', placement: 'right', advance: 'click-target', block: true },
    { id: 'missing', target: 'sandbox.missing', text: 'sandbox.missing', advance: 'next' },
  ],
}
