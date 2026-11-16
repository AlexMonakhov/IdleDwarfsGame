import type { TutorialEvent } from './types'

// Шина событий обучения: место, где действие уже случилось (сундук открыт, бой выигран), сообщает о нём,
// а шаг сценария, который этого ждёт, идёт дальше. Нет активного шага — событие просто никто не слушает
type Listener = (event: TutorialEvent) => void

const listeners = new Set<Listener>()

export const emitTutorialEvent = (event: TutorialEvent) => {
  for (const listener of [...listeners]) listener(event)
}

export const onTutorialEvent = (listener: Listener) => {
  listeners.add(listener)
  return () => { listeners.delete(listener) }
}
