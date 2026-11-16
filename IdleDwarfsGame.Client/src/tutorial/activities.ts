import type { TutorialModalId } from './types'

// Активности игры для обучения: у каждой — окно, кнопка в городе или шапке, этап кампании, с которого
// к ней стоит звать, и ключ прогресса на сервере (PlayerTutorial).
// - Значок «!» стоит на кнопке, пока игрок не познакомился с активностью (её сценарий пройден или пропущен;
//   без сценария — просто открыл окно).
// - «Попробовал» — сервер отмечает по главному действию (TutorialActivities.cs). У активностей без своего действия
//   (doneOnSeen) попробовал = познакомился.
// - Подсказка при входе зовёт в первую по порядку непопробованную активность, уже доступную по этапу кампании.
// Порядок в массиве — порядок, в котором зовём. Таблица этапов — docs/tutorial-plan.md
export interface TutorialActivity {
  // Ключ на сервере; он же — id сценария знакомства, если сценарий есть
  id: string
  modal: TutorialModalId
  // Здание в городе (TownView, ключ здания) или кнопка в шапке (InterfaceView) — на ней «!» и якорь подсказки
  building?: string
  nav?: string
  // Сколько уровней кампании пройти, прежде чем звать сюда подсказкой
  unlockStage: number
  doneOnSeen?: boolean
}

export const ACTIVITIES: TutorialActivity[] = [
  { id: 'campaign', modal: 'campaign', building: 'campaign', unlockStage: 0 },
  { id: 'warehouse', modal: 'warehouse', building: 'warehouse', unlockStage: 1 },
  { id: 'altar', modal: 'altar', building: 'altar', unlockStage: 1 },
  { id: 'mail', modal: 'mail', nav: 'mail', unlockStage: 1, doneOnSeen: true },
  { id: 'forge', modal: 'forge', building: 'blacksmith', unlockStage: 3 },
  { id: 'shop', modal: 'shop', building: 'market', unlockStage: 3 },
  { id: 'mine', modal: 'mine', building: 'mine', unlockStage: 5 },
  { id: 'expeditions', modal: 'expeditions', building: 'expedition', unlockStage: 5 },
  { id: 'quests', modal: 'quests', nav: 'quests', unlockStage: 5, doneOnSeen: true },
  { id: 'tower', modal: 'tower', building: 'tower', unlockStage: 8 },
  { id: 'pvp', modal: 'pvp', building: 'arena', unlockStage: 10 },
  { id: 'friends', modal: 'friends', nav: 'friends', unlockStage: 10, doneOnSeen: true },
  { id: 'guild', modal: 'guild', building: 'guild', unlockStage: 15 },
  { id: 'event', modal: 'event', nav: 'event', unlockStage: 15, doneOnSeen: true },
  { id: 'collection', modal: 'collection', nav: 'collection', unlockStage: 20, doneOnSeen: true },
  { id: 'delve', modal: 'delve', nav: 'delve', unlockStage: 20, doneOnSeen: true },
]

export const activityByModal = (modal: TutorialModalId) => ACTIVITIES.find(activity => activity.modal === modal)
export const activityByBuilding = (building: string) => ACTIVITIES.find(activity => activity.building === building)
export const activityByNav = (nav: string) => ACTIVITIES.find(activity => activity.nav === nav)

// Якорь кнопки активности: здание в городе или кнопка шапки
export const activityAnchor = (activity: TutorialActivity) =>
  activity.building ? `town.${activity.building}` : `nav.${activity.nav}`
